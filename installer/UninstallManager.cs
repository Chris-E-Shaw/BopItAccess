using Microsoft.Win32;
using System.Diagnostics;

namespace BopItAccess.Installer;

public enum UninstallPreferenceScope
{
    CurrentUser,
    AllUsers
}

public sealed class UninstallResult
{
    public bool Success { get; set; }
    public bool RequiresSelfCleanup { get; set; }
    public List<string> Conflicts { get; } = new();
    public List<string> PreservedSharedFiles { get; } = new();
    public List<string> PreferenceWarnings { get; } = new();
}

/// <summary>
/// Removes installer-owned files and recognisable earlier copies of this mod,
/// restoring unrelated files that were present before installation. The launcher script performs the
/// final removal of the running uninstaller and state directory after exit.
/// </summary>
public static class UninstallManager
{
    private static readonly string[] LegacyDocumentationNames =
    {
        "BopItAccess-user-guide.html", "BopItAccess-build-history.html", "BopItAccess-release-review.html",
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
        UninstallPreferenceScope scope, Action<string> log,
        CancellationToken cancellationToken = default, string? targetUserSid = null)
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
        EnsureGameClosed();
        cancellationToken.ThrowIfCancellationRequested();
        SelectPreferenceScope(manifest, scope, targetUserSid, log);

        var result = new UninstallResult { RequiresSelfCleanup = true };
        if (!manifest.UninstallFilesRemoved && !Directory.Exists(game))
        {
            // Directory.Exists also returns false when a drive is disconnected
            // or access is denied. Do not discard ownership records for a mod
            // which may still exist when that drive becomes available again.
            string? gameRoot = Path.GetPathRoot(game);
            if (string.IsNullOrEmpty(gameRoot) || !Directory.Exists(gameRoot))
                throw new IOException("The game drive is unavailable. Reconnect it before uninstalling Bop It Access.");
            bool missing = false;
            try { File.GetAttributes(game); }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            { missing = true; }
            if (!missing)
                throw new IOException("The game folder could not be inspected safely. Restore access before uninstalling Bop It Access.");
            log("The game folder was already removed. Cleaning the remaining mod preferences and installer records.");
            manifest.UninstallFilesRemoved = true;
            InstallManifest.SaveAtomic(manifest, manifestPath);
        }
        if (manifest.UninstallFilesRemoved)
        {
            log("Retrying remaining mod preference cleanup; installed files were already removed.");
            FinishPreferenceCleanup(manifest, manifestPath, result, log);
            return result;
        }
        var loaderRestorations = new List<LoaderUiDefaults.RestorePlan>();
        var baselineHashes = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        ValidateSettingsFiles(game);
        ValidateModLogs(game, removeMain: true);
        // A runtime added below a pre-existing loader may also be used by
        // other mods. Sharing is independent of who first installed the loader.
        bool sharedLoader = HasOtherMods(game);
        if (sharedLoader)
            log("Other mods are present, or their folders could not be safely inspected. MelonLoader and shared dependencies will be preserved.");
        if (manifest.MelonLoaderInstalledByInstaller && !sharedLoader)
        {
            string melon = Path.Combine(game, "MelonLoader");
            try { ValidateOwnedMelonLoaderTree(melon, game); }
            catch (InvalidDataException ex) { result.Conflicts.Add(ex.Message); }
            try { ValidateLoaderRemnants(game, manifest); }
            catch (InvalidDataException ex) { result.Conflicts.Add(ex.Message); }
        }

        // Check everything before changing the installation. A modified file is
        // not silently discarded; the caller can explain the conflict to the user.
        foreach (InstalledFile file in manifest.Files)
        {
            string path = InstallTransaction.ValidateAgainstRoots(file.Path, game, state);
            if (file.IsLoaderConfiguration) LoaderUiDefaults.ValidatePath(path, game);
            if (ShouldPreserveShared(path, game, sharedLoader, file)) continue;
            if (InstallTransaction.IsWithin(path, state)) continue; // Defer running uninstaller files.
            string? originalHash = null;
            if (file.OriginalBackupPath is not null)
            {
                string backup = InstallTransaction.ValidateBackup(file.OriginalBackupPath, state);
                if (!File.Exists(backup)) result.Conflicts.Add($"Original backup is missing: {backup}");
                else
                {
                    originalHash = await InstallTransaction.Sha256Async(backup, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(file.OriginalSha256) &&
                        !string.Equals(originalHash, file.OriginalSha256, StringComparison.OrdinalIgnoreCase))
                        result.Conflicts.Add($"Original backup changed: {backup}");
                }
            }
            baselineHashes[path] = originalHash;
            if (!file.IsLoaderConfiguration && File.Exists(path))
            {
                string currentHash = await InstallTransaction.Sha256Async(path, cancellationToken);
                // A prior uninstall may have restored this original before a
                // later file failed. Permit that exact baseline on retry while
                // continuing to reject every other unexpected file change.
                if (!string.Equals(currentHash, file.Sha256, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(currentHash, originalHash, StringComparison.OrdinalIgnoreCase))
                    result.Conflicts.Add($"Installed file was modified: {path}");
            }
            if (file.IsLoaderConfiguration)
            {
                try
                {
                    loaderRestorations.Add(LoaderUiDefaults.PrepareRestore(file, game, state,
                        manifest.MelonLoaderInstalledByInstaller && !sharedLoader));
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                    InvalidDataException or System.Text.DecoderFallbackException)
                {
                    result.Conflicts.Add("Loader configuration cannot be safely restored: " + ex.Message);
                }
            }
        }
        if (result.Conflicts.Count > 0)
        {
            foreach (string conflict in result.Conflicts) log(conflict);
            return result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        // Remember the scope before the first destructive action. A failed
        // profile cleanup can then retry the same user's request later.
        InstallManifest.SaveAtomic(manifest, manifestPath);
        // Apply shared-config plans before deleting any mod files. A new read
        // or write failure leaves the installed mod and manifest intact.
        foreach (LoaderUiDefaults.RestorePlan plan in loaderRestorations)
            LoaderUiDefaults.Restore(plan, log);

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
            if (file.IsLoaderConfiguration) continue; // Already restored before file removal.
            bool removeEarlierModCopy = IsBopItAccessFile(path, game) ||
                (!sharedLoader && PathsEqual(path, Path.Combine(game, "prism.dll")) &&
                 baselineHashes.TryGetValue(path, out string? baselineHash) &&
                 string.Equals(baselineHash, LegacyPrismSha256, StringComparison.OrdinalIgnoreCase));
            if (file.OriginalBackupPath is not null && !removeEarlierModCopy)
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

        RemoveModLogs(game, removeMain: true, log);
        RemoveSettingsFiles(game, log);
        if (manifest.MelonLoaderInstalledByInstaller && !sharedLoader)
        {
            RemoveOwnedMelonLoaderTree(game, log);
            RemoveLoaderRemnants(game, manifest, log);
        }

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
        // Persist the file-removal checkpoint before profile cleanup can report
        // warnings. Future retries must not hash/delete restored original files.
        manifest.UninstallFilesRemoved = true;
        InstallManifest.SaveAtomic(manifest, manifestPath);
        FinishPreferenceCleanup(manifest, manifestPath, result, log);
        return result;
    }

    /// <summary>
    /// Targeted cleanup for hand-installed builds with no ownership manifest.
    /// It intentionally leaves MelonLoader, the .NET SDK, and any dependency
    /// whose origin cannot be established. The caller must report that limit.
    /// </summary>
    public static async Task<UninstallResult> UninstallLegacyAsync(string gameDirectory,
        UninstallPreferenceScope scope, Action<string> log,
        CancellationToken cancellationToken = default, string? targetUserSid = null)
    {
        string game = Path.GetFullPath(gameDirectory);
        EnsureGameClosed();
        cancellationToken.ThrowIfCancellationRequested();
        targetUserSid = ResolvePreferenceUserSid(scope, targetUserSid);
        ValidateSettingsFiles(game);
        ValidateModLogs(game, removeMain: true);
        string state = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        string mod = InstallTransaction.ValidateAgainstRoots(Path.Combine(game, "Mods", "BopItAccess.dll"), game, state);
        string launcher = InstallTransaction.ValidateAgainstRoots(Path.Combine(game, "BopItAccess-uninstall.ps1"), game, state);
        if (File.Exists(mod)) { File.Delete(mod); log($"Removed {mod}"); }
        if (File.Exists(launcher)) { File.Delete(launcher); log("Removed the Bop It Access uninstall shortcut."); }
        RemoveModLogs(game, removeMain: true, log);

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
        var result = new UninstallResult { Success = true, RequiresSelfCleanup = false };
        result.PreferenceWarnings.AddRange(ModPreferenceCleaner.RemovePreferences(scope, targetUserSid, log));
        RemoveSettingsFiles(game, log);
        RemoveUninstallRegistration(log);
        log("Removed identifiable legacy mod files. Pre-existing MelonLoader and unverified shared files were preserved.");
        return result;
    }

    internal static bool IsLegacyDocumentationName(string name) =>
        LegacyDocumentationNames.Contains(name, StringComparer.OrdinalIgnoreCase);

    private static bool IsBopItAccessFile(string path, string game)
    {
        if (PathsEqual(path, Path.Combine(game, "Mods", "BopItAccess.dll")) ||
            PathsEqual(path, Path.Combine(game, "BopItAccess-uninstall.ps1")) ||
            PathsEqual(path, Path.Combine(game, "Mods", "BopItAccess.log")) ||
            PathsEqual(path, Path.Combine(game, "Mods", "BopItAccess.log.previous")))
            return true;
        // Only dedicated mod filenames in its guide folders qualify. Generic
        // README/licence names may have belonged to someone else beforehand.
        string name = Path.GetFileName(path);
        return name.StartsWith("BopItAccess-", StringComparison.OrdinalIgnoreCase) &&
            IsLegacyDocumentationPath(Path.Combine(game, "documentation"), path);
    }

    private static void FinishPreferenceCleanup(InstallManifest manifest, string manifestPath,
        UninstallResult result, Action<string> log)
    {
        if (manifest.UninstallPreferenceScope is null)
            throw new InvalidDataException("Choose whose mod preferences should be removed before retrying uninstall.");
        result.PreferenceWarnings.AddRange(ModPreferenceCleaner.RemovePreferences(
            manifest.UninstallPreferenceScope.Value, manifest.UninstallUserSid, log));
        if (result.PreferenceWarnings.Count == 0)
        {
            // Do not grant the launcher permission to delete its retry state
            // until the Windows entry has also been removed successfully.
            RemoveUninstallRegistration(log);
            manifest.UninstallCompleted = true;
            InstallManifest.SaveAtomic(manifest, manifestPath);
            log("Bop It Access was removed. The launcher will remove its own files on exit.");
        }
        else
        {
            manifest.UninstallCompleted = false;
            InstallManifest.SaveAtomic(manifest, manifestPath);
            log("Mod files were removed. The uninstall launcher and Windows entry remain so preference cleanup can be retried.");
        }
        result.Success = true;
    }

    private static void SelectPreferenceScope(InstallManifest manifest,
        UninstallPreferenceScope scope, string? targetUserSid, Action<string> log)
    {
        string? selectedSid = ResolvePreferenceUserSid(scope, targetUserSid);
        if (manifest.UninstallPreferenceScope is not null)
        {
            if (manifest.UninstallPreferenceScope == UninstallPreferenceScope.CurrentUser &&
                string.IsNullOrWhiteSpace(manifest.UninstallUserSid))
                throw new InvalidDataException("The saved uninstall choice has no Windows user identifier.");
            ResolvePreferenceUserSid(manifest.UninstallPreferenceScope.Value, manifest.UninstallUserSid);
            if (manifest.UninstallPreferenceScope != scope ||
                !string.Equals(manifest.UninstallUserSid, selectedSid, StringComparison.OrdinalIgnoreCase))
                log("Continuing the previously chosen preference cleanup so a retry does not affect another Windows user.");
            return;
        }
        manifest.UninstallPreferenceScope = scope;
        manifest.UninstallUserSid = selectedSid;
        log(scope == UninstallPreferenceScope.AllUsers
            ? "Removing mod preferences for everyone on this PC."
            : "Removing mod preferences for the selected Windows user only.");
    }

    private static string? ResolvePreferenceUserSid(UninstallPreferenceScope scope, string? targetUserSid)
    {
        if (scope == UninstallPreferenceScope.AllUsers) return null;
        if (scope != UninstallPreferenceScope.CurrentUser)
            throw new ArgumentOutOfRangeException(nameof(scope));
        return UninstallRequestUser.Resolve(targetUserSid);
    }

    /// <summary>
    /// Start a fresh ownership record when reinstalling from the same open UI.
    /// Call only while holding the installer's global mutex. Keep its running
    /// launcher and diagnostics; delayed cleanup will reject the new manifest.
    /// </summary>
    internal static void ForgetCompletedUninstall(string manifestPath, Action<string> log)
    {
        InstallManifest manifest = InstallManifest.Load(manifestPath);
        string expectedState = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        if (!manifest.UninstallCompleted || !manifest.UninstallFilesRemoved ||
            !PathsEqual(manifest.StateDirectory, expectedState) ||
            !PathsEqual(manifestPath, Path.Combine(expectedState, InstallManifest.FileName)) ||
            !Directory.Exists(expectedState) ||
            (File.GetAttributes(expectedState) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The previous uninstall has not completed safely.");
        InstallTransaction.ValidateAgainstRoots(manifestPath, expectedState, expectedState);
        string completedPath = InstallTransaction.ValidateAgainstRoots(
            Path.Combine(expectedState, InstallManifest.CompletedUninstallFileName), expectedState, expectedState);
        if (File.Exists(Path.Combine(expectedState, "transaction.json")))
            throw new InvalidDataException("Recover the interrupted installation before installing again.");
        foreach (string name in new[] { "originals", "transactions" })
        {
            string folder = Path.Combine(expectedState, name);
            InstallTransaction.ValidateAgainstRoots(folder, expectedState, expectedState);
            ValidateUnlinkedTree(folder);
        }
        foreach (string name in new[] { "originals", "transactions" })
        {
            string folder = Path.Combine(expectedState, name);
            if (Directory.Exists(folder)) DeleteTreeWithoutReparsePoints(folder);
        }
        // Retain a completed-removal checkpoint if a reinstall is then
        // aborted before commit. The delayed helper prefers a new active
        // manifest and never cleans state while a transaction is pending.
        File.Move(manifestPath, completedPath, overwrite: true);
        log("The previous uninstall is complete. Preparing a fresh installation.");
    }

    private static IReadOnlyList<string> ValidateLoaderRemnants(string game, InstallManifest manifest)
    {
        var files = new List<string>();
        string userData = Path.Combine(game, "UserData");
        foreach (string name in new[] { "MelonPreferences.cfg", "Loader.cfg" })
        {
            string file = InstallTransaction.ValidateAgainstRoots(Path.Combine(userData, name),
                game, manifest.StateDirectory);
            // The user requested complete loader cleanup once it is removed.
            // These two known loader configs are no longer useful even when
            // an older copy was restored from a baseline backup above.
            if (File.Exists(file) && (File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Loader preference cleanup cannot follow a linked file.");
            files.Add(file);
        }
        foreach (string name in new[] { "Plugins", "UserLibs", "UserData" })
        {
            string folder = InstallTransaction.ValidateAgainstRoots(Path.Combine(game, name),
                game, manifest.StateDirectory);
            ValidateUnlinkedTree(folder);
        }
        return files;
    }

    private static void RemoveLoaderRemnants(string game, InstallManifest manifest, Action<string> log)
    {
        foreach (string file in ValidateLoaderRemnants(game, manifest))
        {
            if (!File.Exists(file)) continue;
            File.Delete(file);
            log("Removed MelonLoader preferences: " + file);
        }
        foreach (string name in new[] { "Plugins", "UserLibs", "UserData" })
        {
            string folder = Path.Combine(game, name);
            if (!Directory.Exists(folder)) continue;
            RemoveEmptyFolders(folder);
            if (Directory.Exists(folder))
                log("Kept unrecognised files in " + name + " to protect files that were not installed by Bop It Access.");
            else log("Removed the unused " + name + " folder.");
        }
    }

    private static void RemoveEmptyFolders(string directory)
    {
        if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("Empty-folder cleanup cannot follow a linked folder.");
        foreach (string child in Directory.EnumerateDirectories(directory)) RemoveEmptyFolders(child);
        if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
    }

    private static void ValidateUnlinkedTree(string root)
    {
        if (!Directory.Exists(root)) return;
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            string directory = pending.Pop();
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Uninstall cleanup cannot follow a linked folder: " + directory);
            foreach (string entry in Directory.EnumerateFileSystemEntries(directory))
            {
                FileAttributes attributes = File.GetAttributes(entry);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Uninstall cleanup cannot follow a linked file or folder: " + entry);
                if ((attributes & FileAttributes.Directory) != 0) pending.Push(entry);
            }
        }
    }

    private static void RemoveModLogs(string game, bool removeMain, Action<string> log)
    {
        foreach (string path in ValidateModLogs(game, removeMain))
        {
            if (!File.Exists(path)) continue;
            File.Delete(path);
            log("Removed Bop It Access log: " + path);
        }
    }

    private static IReadOnlyList<string> ValidateModLogs(string game, bool removeMain)
    {
        string[] names = removeMain
            ? new[] { "BopItAccess.log", "BopItAccess.log.previous" }
            : new[] { "BopItAccess.log.previous" };
        var paths = new List<string>();
        foreach (string name in names)
        {
            string path = InstallTransaction.ValidateAgainstRoots(Path.Combine(game, "Mods", name),
                game, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess"));
            if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Mod log cleanup cannot follow a linked file: " + path);
            paths.Add(path);
        }
        return paths;
    }

    // These mutable preferences are created by the mod after installation,
    // so they cannot use the manifest's fixed content hashes. Remove only the
    // mod's own filenames; UserData may also contain settings for other mods.
    private static string[] ValidateSettingsFiles(string game)
    {
        string userData = Path.GetFullPath(Path.Combine(game, "UserData"));
        if (!InstallTransaction.IsWithin(userData, game) ||
            (Directory.Exists(userData) &&
             (File.GetAttributes(userData) & FileAttributes.ReparsePoint) != 0))
            throw new InvalidDataException("Settings cleanup cannot follow a UserData junction.");
        var paths = new List<string> { Path.Combine(userData, "BopItAccess.ini"),
            Path.Combine(userData, "BopItAccess.ini.tmp"),
            Path.Combine(userData, "Loader.cfg.bopitaccess-uninstall.tmp") };
        if (Directory.Exists(userData))
        {
            const string prefix = "BopItAccess.ini.";
            const string suffix = ".tmp";
            foreach (string candidate in Directory.EnumerateFiles(userData,
                         "BopItAccess.ini.*.tmp", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(candidate);
                if (name.Length == prefix.Length + 32 + suffix.Length &&
                    name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                    Guid.TryParseExact(name.Substring(prefix.Length, 32), "N", out _))
                    paths.Add(candidate);
            }
        }
        foreach (string path in paths)
        {
            if (!InstallTransaction.IsWithin(path, game) ||
                (File.Exists(path) &&
                 (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0))
                throw new InvalidDataException("Settings cleanup encountered a linked config file.");
        }
        return paths.ToArray();
    }

    private static void RemoveSettingsFiles(string game, Action<string> log)
    {
        foreach (string path in ValidateSettingsFiles(game))
        {
            if (!File.Exists(path)) continue;
            File.Delete(path);
            log($"Removed Bop It Access settings: {path}");
        }
    }

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
        try
        {
            foreach (string root in new[] { Path.Combine(game, "Mods"), Path.Combine(game, "Plugins"),
                         Path.Combine(game, "UserLibs") })
            {
                if (!Directory.Exists(root)) continue;
                var pending = new Stack<string>();
                pending.Push(root);
                while (pending.Count > 0)
                {
                    string folder = pending.Pop();
                    if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0) return true;
                    foreach (string entry in Directory.EnumerateFileSystemEntries(folder))
                    {
                        FileAttributes attributes = File.GetAttributes(entry);
                        if ((attributes & FileAttributes.ReparsePoint) != 0) return true;
                        if ((attributes & FileAttributes.Directory) != 0) pending.Push(entry);
                        else if (Path.GetExtension(entry).Equals(".dll", StringComparison.OrdinalIgnoreCase) &&
                            !PathsEqual(entry, Path.Combine(game, "Mods", "BopItAccess.dll"))) return true;
                    }
                }
            }
            return false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // An unreadable folder cannot establish that the loader is unused.
            return true;
        }
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

    private static void RemoveUninstallRegistration(Action<string> log)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        baseKey.DeleteSubKeyTree(InstallTransaction.UninstallSubKey, throwOnMissingSubKey: false);
        log("Removed the Windows Installed Apps entry.");
    }

    private static void EnsureGameClosed()
    {
        Process[] processes = Process.GetProcessesByName("BopIt!");
        bool running = processes.Length > 0;
        foreach (Process process in processes) process.Dispose();
        if (running)
            throw new InvalidOperationException("Close Bop It! before removing the mod.");
    }

    private static bool PathsEqual(string first, string second) =>
        string.Equals(Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
}
