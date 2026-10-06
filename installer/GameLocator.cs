using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace BopItAccess.Installer;

internal static class GameLocator
{
    private const string AppId = "3214360";

    internal static bool IsGameDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        try
        {
            var full = Path.GetFullPath(path);
            return File.Exists(Path.Combine(full, "BopIt!.exe"))
                && Directory.Exists(Path.Combine(full, "BopIt!_Data"));
        }
        catch { return false; }
    }

    internal static IReadOnlyList<string> FindInstallations(CancellationToken cancellation = default)
    {
        var steamRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            cancellation.ThrowIfCancellationRequested();
            foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using var root = RegistryKey.OpenBaseKey(hive, view);
                    foreach (var name in new[] { @"Software\Valve\Steam", @"Software\WOW6432Node\Valve\Steam" })
                    {
                        using var key = root.OpenSubKey(name);
                        var value = key?.GetValue("SteamPath") as string ?? key?.GetValue("InstallPath") as string;
                        AddDirectory(steamRoots, value);
                    }
                }
                catch { /* Steam may not be installed in this registry view. */ }
            }
        }

        AddDirectory(steamRoots, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
        foreach (var drive in DriveInfo.GetDrives())
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                if (!drive.IsReady || drive.DriveType is DriveType.CDRom or DriveType.Ram) continue;
                foreach (var relative in new[] { "Steam", "SteamLibrary", @"Program Files (x86)\Steam", @"Program Files\Steam" })
                    AddDirectory(steamRoots, Path.Combine(drive.RootDirectory.FullName, relative));
            }
            catch { /* A removable drive may disappear during detection. */ }
        }

        // Steam's library file points to nonstandard libraries on any drive.
        foreach (var root in steamRoots.ToArray())
        {
            cancellation.ThrowIfCancellationRequested();
            var libraries = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraries)) continue;
            try
            {
                var vdf = File.ReadAllText(libraries);
                foreach (Match match in Regex.Matches(vdf, "\"path\"\\s*\"(?<value>(?:\\\\.|[^\"])*)\"", RegexOptions.IgnoreCase))
                    AddDirectory(steamRoots, match.Groups["value"].Value.Replace("\\\\", "\\"));
            }
            catch { /* An unreadable Steam library is skipped. */ }
        }

        foreach (var root in steamRoots)
        {
            cancellation.ThrowIfCancellationRequested();
            var steamApps = Path.Combine(root, "steamapps");
            var manifest = Path.Combine(steamApps, $"appmanifest_{AppId}.acf");
            if (File.Exists(manifest))
            {
                try
                {
                    var content = File.ReadAllText(manifest);
                    var match = Regex.Match(content, "\"installdir\"\\s*\"(?<value>[^\"]+)\"", RegexOptions.IgnoreCase);
                    if (match.Success)
                        candidates.Add(Path.Combine(steamApps, "common", match.Groups["value"].Value));
                }
                catch { /* Fall back to the normal folder name. */ }
            }
            candidates.Add(Path.Combine(steamApps, "common", "Bop It!"));
        }

        return candidates.Where(IsGameDirectory).Select(Path.GetFullPath).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddDirectory(HashSet<string> paths, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var full = Path.GetFullPath(path);
            if (Directory.Exists(full)) paths.Add(full);
        }
        catch { }
    }
}
