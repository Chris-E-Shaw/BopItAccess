using System.Text;
using System.Text.RegularExpressions;

namespace BopItAccess.Installer;

// Loader.cfg is shared, mutable loader configuration. Change only these two
// flags, retain a rollback snapshot, and restore just their original values
// on uninstall so subsequent unrelated loader edits survive.
internal static class LoaderUiDefaults
{
    private static readonly (string Section, string Key)[] Flags =
        { ("loader", "disable_start_screen"), ("console", "hide_console") };

    internal static string ConfigPath(string game) => Path.Combine(game, "UserData", "Loader.cfg");

    internal static void ValidatePath(string path, string game)
    {
        string expected = Path.GetFullPath(ConfigPath(game));
        if (!string.Equals(Path.GetFullPath(path), expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Loader configuration must be UserData/Loader.cfg inside the game folder.");
        string folder = Path.GetDirectoryName(expected)!;
        foreach (string target in new[] { folder, expected })
            if ((File.Exists(target) || Directory.Exists(target)) &&
                (File.GetAttributes(target) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Loader configuration cannot follow a linked UserData folder or file.");
    }

    internal static async Task ApplyAsync(InstallTransaction transaction, string temporaryFolder,
        Action<string> log, CancellationToken cancellationToken)
    {
        string path = ConfigPath(transaction.GameDirectory);
        ValidatePath(path, transaction.GameDirectory);
        string text = File.Exists(path) ? Read(path) : string.Empty;
        string updated = HiddenDefaults(text);
        // Even when the flags already match, register the original state for
        // uninstall. It must not later restore somebody else's entire config.
        string source = Path.Combine(temporaryFolder, "loader-hidden-defaults.cfg");
        await File.WriteAllTextAsync(source, updated, new UTF8Encoding(false), cancellationToken);
        await transaction.InstallFileAsync(source, path, cancellationToken,
            isLoaderConfiguration: true);
        log("MelonLoader start screen and console are hidden by default.");
    }

    internal static string HiddenDefaults(string text)
    {
        foreach (var flag in Flags) text = SetValue(text, flag.Section, flag.Key, "true");
        return text;
    }

    // Prepare every read and managed-flag parse during uninstall preflight.
    // No owned mod file is removed until these plans can be safely applied.
    internal static RestorePlan PrepareRestore(InstalledFile file, string game,
        string state, bool removeOwnedLoader)
    {
        ValidatePath(file.Path, game);
        string original = file.OriginalBackupPath == null ? string.Empty :
            Read(InstallTransaction.ValidateBackup(file.OriginalBackupPath, state));
        string? current = File.Exists(file.Path) ? Read(file.Path) : null;
        string? updated = current == null ? null : RestoredText(current, original);
        return new RestorePlan(file.Path, game, original, current, updated,
            file.OriginalBackupPath == null, removeOwnedLoader);
    }

    internal static void Restore(RestorePlan plan, Action<string> log)
    {
        ValidatePath(plan.Path, plan.GameDirectory);
        string path = plan.Path;
        if (!File.Exists(path)) return; // Preserve an intentional deletion.
        // A config may change while other manifest files are being checked.
        // Rebase the same two flags onto its latest text before removing any
        // mod files, so newer edits are neither lost nor read midway through
        // the file-removal loop.
        string current = Read(path);
        string updated = current == plan.CurrentText ? plan.UpdatedText! :
            RestoredText(current, plan.OriginalText);
        if (plan.InstallerCreated && plan.RemoveOwnedLoader)
        {
            File.Delete(path);
            log("Removed configuration created for installer-owned MelonLoader.");
            return;
        }
        if (updated == current) return;
        if (plan.InstallerCreated && IsEmpty(updated))
        {
            File.Delete(path);
            log("Removed the empty loader configuration introduced by this installer.");
            return;
        }
        string temporary = path + ".bopitaccess-uninstall.tmp";
        try
        {
            File.WriteAllText(temporary, updated, new UTF8Encoding(false));
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        log("Restored original loader UI flags, preserving other loader configuration changes.");
    }

    private static string RestoredText(string current, string original)
    {
        string updated = current;
        foreach (var flag in Flags)
        {
            // A later user change to one of these flags takes precedence too.
            if (!string.Equals(GetValue(updated, flag.Section, flag.Key), "true", StringComparison.Ordinal))
                continue;
            updated = SetValue(updated, flag.Section, flag.Key,
                GetValue(original, flag.Section, flag.Key));
            if (!HasSection(original, flag.Section))
                updated = RemoveEmptySection(updated, flag.Section);
        }
        return updated;
    }

    internal sealed record RestorePlan(string Path, string GameDirectory,
        string OriginalText, string? CurrentText, string? UpdatedText,
        bool InstallerCreated, bool RemoveOwnedLoader);

    private static string Read(string path)
    {
        using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (input.Length > 1024 * 1024)
            throw new InvalidDataException("Loader.cfg exceeds one megabyte: " + path);
        // TOML uses UTF-8. Keep strict decoding so damaged bytes never become
        // replacement characters in a shared configuration rewrite.
        using var reader = new StreamReader(input, new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: false);
        string text = reader.ReadToEnd();
        if (text.Length > 0 && text[0] == '\uFEFF') text = text[1..];
        if (text.Contains('\0'))
            throw new InvalidDataException("Loader.cfg contains invalid text: " + path);
        ValidateManagedFlags(text, path);
        return text;
    }

    // Parse only the boolean flags this installer owns. Other loader options
    // remain text; their values are preserved rather than normalized.
    private static void ValidateManagedFlags(string text, string path)
    {
        string active = string.Empty;
        var sections = new HashSet<string>(StringComparer.Ordinal);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (string line in Lines(text))
        {
            string? heading = Section(line);
            if (heading != null)
            {
                active = heading;
                if (Flags.Any(flag => flag.Section == active) && !sections.Add(active))
                    throw new InvalidDataException("Loader.cfg contains a duplicate managed section [" + active + "]: " + path);
                continue;
            }
            foreach (var flag in Flags)
            {
                if (active != flag.Section) continue;
                Match entry = Entry(line, flag.Key);
                if (!entry.Success) continue;
                string key = flag.Section + "." + flag.Key;
                string value = entry.Groups[2].Value.Trim();
                if (!keys.Add(key) || value is not ("true" or "false"))
                    throw new InvalidDataException("Loader.cfg has an invalid boolean for " + key + ": " + path);
            }
        }
    }

    private static List<string> Lines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n').Split('\n').ToList();
    private static string Join(List<string> lines, string original) =>
        string.Join(original.Contains("\r\n") ? "\r\n" : "\n", lines) +
        (original.Contains("\r\n") ? "\r\n" : "\n");
    private static string? Section(string line)
    {
        Match match = Regex.Match(line, @"^\s*\[([^\]]+)\]\s*(?:#.*)?$");
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
    private static Match Entry(string line, string key) => Regex.Match(line,
        @"^(\s*" + Regex.Escape(key) + @"\s*=\s*)([^#]*)(#.*)?$");
    private static string? GetValue(string text, string section, string key)
    {
        string active = string.Empty;
        foreach (string line in Lines(text))
        {
            active = Section(line) ?? active;
            if (active != section) continue;
            Match match = Entry(line, key);
            if (match.Success) return match.Groups[2].Value.Trim();
        }
        return null;
    }
    private static bool HasSection(string text, string section) =>
        Lines(text).Any(line => Section(line) == section);
    private static string SetValue(string text, string section, string key, string? value)
    {
        List<string> lines = Lines(text);
        string active = string.Empty;
        int sectionAt = -1;
        int insertAt = lines.Count;
        bool found = false;
        for (int index = 0; index < lines.Count; index++)
        {
            string? heading = Section(lines[index]);
            if (heading != null)
            {
                if (active == section && insertAt == lines.Count) insertAt = index;
                active = heading;
                if (active == section) sectionAt = index;
                continue;
            }
            if (active != section) continue;
            Match match = Entry(lines[index], key);
            if (!match.Success) continue;
            found = true;
            if (value == null) lines.RemoveAt(index--);
            else lines[index] = match.Groups[1].Value + value +
                (match.Groups[3].Success ? " " + match.Groups[3].Value : string.Empty);
        }
        if (!found && value != null)
        {
            if (sectionAt < 0)
            {
                if (lines.Count > 0 && lines[^1].Length != 0) lines.Add(string.Empty);
                lines.Add("[" + section + "]");
                lines.Add(key + " = " + value);
            }
            else lines.Insert(Math.Min(insertAt, lines.Count), key + " = " + value);
        }
        return Join(lines, text);
    }
    private static string RemoveEmptySection(string text, string section)
    {
        List<string> lines = Lines(text);
        int start = lines.FindIndex(line => Section(line) == section);
        if (start < 0) return text;
        int end = lines.FindIndex(start + 1, line => Section(line) != null);
        if (end < 0) end = lines.Count;
        if (lines.Skip(start + 1).Take(end - start - 1).Any(line =>
                line.Trim().Length != 0 && !line.TrimStart().StartsWith('#')))
            return text;
        lines.RemoveAt(start);
        return Join(lines, text);
    }
    private static bool IsEmpty(string text) => Lines(text).All(line =>
        line.Trim().Length == 0 || line.TrimStart().StartsWith('#') || Section(line) != null);
}
