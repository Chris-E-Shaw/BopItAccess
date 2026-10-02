using Microsoft.Win32;
using System.Diagnostics;

namespace BopItAccess.Installer;

public sealed class UninstallResult
{
    public bool Success { get; set; }
    public bool RequiresSelfCleanup { get; set; }
    public List<string> Conflicts { get; } = new();
    public List<string> PreservedSharedFiles { get; } = new();
}

/// <summary>
/// Removes exactly the files recorded as installer-owned and restores files
/// that were present before installation. The launcher script performs the
/// final removal of the running uninstaller and state directory after exit.
/// </summary>
public static class UninstallManager
{
    private static readonly string[] LegacyDocumentationNames =
    {
        "BopItAccess-user-guide.html", "BopItAccess-build-history.html",
        "README.md", "README.txt", "GIT-WORKFLOW.md", "THIRD-PARTY-NOTICES.txt"
    };
    private static readonly string[] LegacyLocaleFolders =
    {
        "fr", "it", "de", "es", "es-MX", "ja", "ko", "zh", "pt-BR"
    };

    // SHA-256 of the official Windows x64 Prism v0.18.3 DLL used by v0.9.0.
    internal const string LegacyPrismSha256 =
        "7C7D09C8C7306E8E1A0C46D603CCB2A391C194C77843400C11B746E7DD56A467";

    public static async Task<UninstallResult> UninstallAsync(string manifestPath,
        Action<string> log, CancellationToken cancellationToken = default)
    {
        InstallManifest manifest = InstallManifest.Load(manifestPath);
        string game = Path.GetFullPath(manifest.GameDirectory);
        string state = Path.GetFullPath(manifest.StateDirectory);
        string expectedState = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        if (!PathsEqual(state, expectedState) ||
            !Directory.Exists(state) ||
            (File.GetAttributes(state) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The installation state is outside the expected ProgramData folder.");
        string expectedManifest = Path.Combine(state, InstallManifest.FileName);
        if (!PathsEqual(manifestPath, expectedManifest))
            throw new InvalidDataException("The manifest path does not match its state directory.");
        if (!Directory.Exists(game)) throw new DirectoryNotFoundException(game);
        EnsureGameClosed();
        cancellationToken.ThrowIfCancellationRequested();

        var result = new UninstallResult { RequiresSelfCleanup = true };
        bool sharedLoader = manifest.MelonLoaderInstalledByInstaller && HasOtherMods(game);
        if (sharedLoader)
            log("Other mods are present. MelonLoader and shared dependencies will be preserved.");
        if (manifest.MelonLoaderInstalledByInstaller && !sharedLoader)
        {
            string melon = Path.Combine(game, "MelonLoader");
            try { ValidateOwnedMelonLoaderTree(melon, game); }
            catch (InvalidDataException ex) { result.Conflicts.Add(ex.Message); }
        }

        // Check everything before changing the installation. A modified file is
        // not silently discarded; the caller can explain the conflict to the user.
        foreach (InstalledFile file in manifest.Files)
        {
            string path = InstallTransaction.ValidateAgainstRoots(file.Path, game, state);
            if (ShouldPreserveShared(path, game, sharedLoader, file)) continue;
            if (InstallTransaction.IsWithin(path, state)) continue; // Defer running uninstaller files.
            if (file.OriginalBackupPath is not null)
            {
                string backup = InstallTransaction.ValidateBackup(file.OriginalBackupPath, state);
                if (!File.Exists(backup)) result.Conflicts.Add($"Original backup is missing: {backup}");
                else if (!string.IsNullOrWhiteSpace(file.OriginalSha256) &&
                    !string.Equals(await InstallTransaction.Sha256Async(backup, cancellationToken),
                        file.OriginalSha256, StringComparison.OrdinalIgnoreCase))
                    result.Conflicts.Add($"Original backup changed: {backup}");
            }
            if (File.Exists(path) &&
                !string.Equals(await InstallTransaction.Sha256Async(path, cancellationToken),
                    file.Sha256, StringComparison.OrdinalIgnoreCase))
                result.Conflicts.Add($"Installed file was modified: {path}");
        }
        if (result.Conflicts.Count > 0)
        {
            foreach (string conflict in result.Conflicts) log(conflict);
            return result;
        }

        // Once removal starts, finish it even if a UI cancellation token fires.
        foreach (InstalledFile file in manifest.Files.AsEnumerable().Reverse())
        {
            string path = InstallTransaction.ValidateAgainstRoots(file.Path, game, state);
            if (InstallTransaction.IsWithin(path, state)) continue;
            if (ShouldPreserveShared(path, game, sharedLoader, file))
            {
                result.PreservedSharedFiles.Add(path);
                continue;
            }
            if (file.OriginalBackupPath is not null)
            {
                string backup = InstallTransaction.ValidateBackup(file.OriginalBackupPath, state);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.Copy(backup, path, overwrite: true);
                log($"Restored the file that was present before installation: {path}");
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
                log($"Removed installed file: {path}");
            }
        }

        string modLog = Path.Combine(game, "Mods", "BopItAccess.log");
        if (!manifest.ModLogExistedBeforeInstall && File.Exists(modLog))
        {
            File.Delete(modLog);
            log("Removed the Bop It Access log.");
        }
        if (manifest.MelonLoaderInstalledByInstaller && !sharedLoader)
            RemoveOwnedMelonLoaderTree(game, log);
        RemoveModPreferences(log);

        foreach (string directory in manifest.CreatedDirectories.OrderByDescending(path => path.Length))
        {
            string full = InstallTransaction.ValidateAgainstRoots(directory, game, state);
            if (InstallTransaction.IsWithin(full, state)) continue;
            if (Directory.Exists(full) && !Directory.EnumerateFileSystemEntries(full).Any())
            {
                Directory.Delete(full);
                log($"Removed empty installer-created folder: {full}");
            }
        }
        RemoveUninstallRegistration(log);
        log("Bop It Access files were removed. The launcher will remove its own files on exit.");
        result.Success = true;
        return result;
    }

    /// <summary>
    /// Targeted cleanup for hand-installed builds with no ownership manifest.
    /// It intentionally leaves MelonLoader, the .NET SDK, and any dependency
    /// whose origin cannot be established. The caller must report that limit.
    /// </summary>
    public static async Task<UninstallResult> UninstallLegacyAsync(string gameDirectory,
        Action<string> log, CancellationToken cancellationToken = default)
    {
        string game = Path.GetFullPath(gameDirectory);
        EnsureGameClosed();
        cancellationToken.ThrowIfCancellationRequested();
        string mod = Path.Combine(game, "Mods", "BopItAccess.dll");
        string modLog = Path.Combine(game, "Mods", "BopItAccess.log");
        if (File.Exists(mod)) { File.Delete(mod); log($"Removed {mod}"); }
        if (File.Exists(modLog)) { File.Delete(modLog); log($"Removed {modLog}"); }

        string prism = Path.Combine(game, "prism.dll");
        if (File.Exists(prism) &&
            string.Equals(await InstallTransaction.Sha256Async(prism, cancellationToken),
                LegacyPrismSha256, StringComparison.OrdinalIgnoreCase) && !HasOtherMods(game))
        {
            File.Delete(prism);
            log("Removed the pinned Prism v0.18.3 DLL from the legacy install.");
        }
        string docs = Path.Combine(game, "documentation");
        if (Directory.Exists(docs) && (File.GetAttributes(docs) & FileAttributes.ReparsePoint) == 0)
        {
            var knownFolders = new[] { docs }.Concat(LegacyLocaleFolders.Select(code => Path.Combine(docs, code)));
            foreach (string folder in knownFolders.Where(Directory.Exists)
                         .Where(folder => (File.GetAttributes(folder) & FileAttributes.ReparsePoint) == 0))
            {
                foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!IsLegacyDocumentationName(Path.GetFileName(file))) continue;
                    File.Delete(file);
                    log($"Removed Bop It Access documentation: {file}");
                }
            }
            foreach (string code in LegacyLocaleFolders)
            {
                string locale = Path.Combine(docs, code);
                if (Directory.Exists(locale) &&
                    (File.GetAttributes(locale) & FileAttributes.ReparsePoint) == 0 &&
                    !Directory.EnumerateFileSystemEntries(locale).Any())
                    Directory.Delete(locale);
            }
            if (!Directory.EnumerateFileSystemEntries(docs).Any()) Directory.Delete(docs);
        }
        RemoveModPreferences(log);
        RemoveUninstallRegistration(log);
        log("Removed identifiable legacy mod files. Pre-existing MelonLoader and unverified shared files were preserved.");
        return new UninstallResult { Success = true, RequiresSelfCleanup = false };
    }

    internal static bool IsLegacyDocumentationName(string name) =>
        LegacyDocumentationNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    internal static bool IsLegacyDocumentationPath(string documentationRoot, string file)
    {
        if (!IsLegacyDocumentationName(Path.GetFileName(file))) return false;
        string? parent = Path.GetDirectoryName(Path.GetFullPath(file));
        if (parent is null) return false;
        if (PathsEqual(parent, documentationRoot)) return true;
        return LegacyLocaleFolders.Contains(Path.GetFileName(parent), StringComparer.OrdinalIgnoreCase) &&
            PathsEqual(Path.GetDirectoryName(parent)!, documentationRoot);
    }

    internal static bool HasOtherMods(string game)
    {
        string mods = Path.Combine(game, "Mods");
        if (Directory.Exists(mods) && Directory.EnumerateFiles(mods, "*.dll", SearchOption.TopDirectoryOnly)
                .Any(path => !Path.GetFileName(path).Equals("BopItAccess.dll", StringComparison.OrdinalIgnoreCase)))
            return true;
        string plugins = Path.Combine(game, "Plugins");
        return Directory.Exists(plugins) &&
            Directory.EnumerateFiles(plugins, "*.dll", SearchOption.AllDirectories).Any();
    }

    private static bool ShouldPreserveShared(string path, string game, bool sharedLoader,
        InstalledFile file)
    {
        if (!sharedLoader) return false;
        // A file the installer replaced existed before it arrived. Restore
        // that original even when a later mod uses the shared loader; only
        // files the installer itself introduced can be left shared.
        if (file.OriginalBackupPath is not null) return false;
        string melonLoader = Path.Combine(game, "MelonLoader");
        return InstallTransaction.IsWithin(path, melonLoader) ||
            PathsEqual(path, Path.Combine(game, "version.dll")) ||
            PathsEqual(path, Path.Combine(game, "prism.dll"));
    }

    private static void ValidateOwnedMelonLoaderTree(string melon, string game)
    {
        if (!PathsEqual(melon, Path.Combine(game, "MelonLoader")))
            throw new InvalidDataException("MelonLoader cleanup path is outside the game root.");
        if (!Directory.Exists(melon)) return;
        var pending = new Stack<string>();
        pending.Push(melon);
        while (pending.Count > 0)
        {
            string directory = pending.Pop();
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException($"MelonLoader cleanup encountered a junction: {directory}");
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
            {
                FileAttributes attributes = File.GetAttributes(entry);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException($"MelonLoader cleanup encountered a junction: {entry}");
                if ((attributes & FileAttributes.Directory) != 0) pending.Push(entry);
            }
        }
    }

    private static void RemoveOwnedMelonLoaderTree(string game, Action<string> log)
    {
        string melon = Path.Combine(game, "MelonLoader");
        if (!Directory.Exists(melon)) return;
        ValidateOwnedMelonLoaderTree(melon, game);
        // The loader directory was absent before this installer added it.
        // Only after checking for other mods and junctions do we remove its
        // generated Cpp2IL, proxy, dependency, and log files as well.
        DeleteTreeWithoutReparsePoints(melon);
        log("Removed installer-owned MelonLoader and its generated files.");
    }

    private static void DeleteTreeWithoutReparsePoints(string directory)
    {
        if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException($"Refusing to delete a junction: {directory}");
        foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
        {
            FileAttributes attributes = File.GetAttributes(entry);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException($"Refusing to delete a junction: {entry}");
            if ((attributes & FileAttributes.Directory) != 0) DeleteTreeWithoutReparsePoints(entry);
            else File.Delete(entry);
        }
        Directory.Delete(directory);
    }

    private static void RemoveModPreferences(Action<string> log)
    {
        // Unity's Windows PlayerPrefs values keep their original key prefix,
        // followed by a Unity hash suffix. Only BopItAccess.* values are ours.
        // Bop It!'s Windows PlayerPrefs key is Alliance/Bop It!; leave the
        // game's own preferences and audio/settings values untouched.
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Alliance\Bop It!", writable: true);
        if (key is null) return;
        foreach (string valueName in key.GetValueNames())
        {
            if (!valueName.StartsWith("BopItAccess.", StringComparison.OrdinalIgnoreCase)) continue;
            key.DeleteValue(valueName, throwOnMissingValue: false);
            log($"Removed mod preference: {valueName}");
        }
    }

    private static void RemoveUninstallRegistration(Action<string> log)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        baseKey.DeleteSubKeyTree(InstallTransaction.UninstallSubKey, throwOnMissingSubKey: false);
        log("Removed the Windows Installed Apps entry.");
    }

    private static void EnsureGameClosed()
    {
        if (Process.GetProcessesByName("BopIt!").Length > 0)
            throw new InvalidOperationException("Close Bop It! before removing the mod.");
    }

    private static bool PathsEqual(string first, string second) =>
        string.Equals(Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
}
