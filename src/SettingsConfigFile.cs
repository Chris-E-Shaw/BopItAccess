using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BopItAccess;

// This file has no Unity dependencies. The runtime supplies validated settings;
// the document layer keeps the player's comments and unfamiliar future keys.
internal sealed class SettingsConfigFile
{
    internal const string FileName = "BopItAccess.ini";
    private const string ChoicesBegin = "# BEGIN BOP IT ACCESS AVAILABLE CHOICES";
    private const string ChoicesEnd = "# END BOP IT ACCESS AVAILABLE CHOICES";
    private static readonly JsonSerializerOptions QuotedValues = new()
        { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private readonly List<string> _lines;
    internal Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
    internal string? Fingerprint { get; private set; }

    private SettingsConfigFile(List<string> lines, string? fingerprint,
        Action<string> report)
    {
        _lines = lines;
        Fingerprint = fingerprint;
        string section = string.Empty;
        for (int index = 0; index < lines.Count; index++)
        {
            string line = lines[index].Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';'))
                continue;
            if (TrySection(line, out string? name))
            {
                section = name;
                continue;
            }
            if (!TryEntry(line, section, out string? key, out string? raw, out _))
            {
                report($"Config ignored malformed line {index + 1}; expected [Section] or Key = Value.");
                continue;
            }
            try
            {
                string value = ReadValue(raw);
                if (!Values.TryAdd(key, value))
                    report($"Config ignored duplicate {key} on line {index + 1}; the first entry wins.");
            }
            catch (JsonException)
            {
                report($"Config ignored invalid quoted value for {key} on line {index + 1}.");
            }
        }
    }

    internal static SettingsConfigFile Load(string path, Action<string> report)
    {
        if (!File.Exists(path))
            return new(new(), null, report);
        var info = new FileInfo(path);
        if (info.Length > 1024 * 1024)
            throw new InvalidDataException("The settings file is larger than one megabyte.");
        byte[] bytes = File.ReadAllBytes(path);
        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: true);
        string text = reader.ReadToEnd().Replace("\r\n", "\n").Replace('\r', '\n');
        return new(text.TrimEnd('\n').Split('\n').ToList(), Hash(bytes), report);
    }

    internal string Render(IReadOnlyDictionary<string, string> values,
        IReadOnlyDictionary<string, string> descriptions,
        IReadOnlyDictionary<string, string[]> choices)
    {
        List<string> output = new();
        if (_lines.Count == 0)
        {
            output.AddRange(new[]
            {
                "# Bop It Access settings",
                "# Close the game, edit this file in a text editor, save, then launch again.",
                "# The game and mod load these values at startup. Menu changes update this file.",
                "# Missing or invalid values keep the previous saved setting. See Mods/BopItAccess.log.",
                "# Keys stay in English in every game language. On/Off and True/False are accepted.",
                "# Recovery under [Game]: Language = en, MusicVolume = 30, SfxVolume = 30, VoiceOverVolume = 30.",
                "# Under [Mod]: SpeechOutput = On and OutputMode = Auto.",
                "# Under [OneCore] and [SAPI]: Voice = System default resets an unusable voice.",
                "# Lines starting # or ; are comments. Quotes support special characters in values.",
                "# Keyboard paths: <Keyboard>/space, <Keyboard>/g, <Keyboard>/f8, etc.",
                "# Controller paths: <Gamepad>/buttonSouth, <Gamepad>/leftTrigger, etc.",
                "# Default restores an input's original assignment. Conflicting inputs are rejected.",
                ""
            });
        }
        HashSet<string> written = new(StringComparer.OrdinalIgnoreCase);
        string section = string.Empty;
        for (int index = 0; index < _lines.Count; index++)
        {
            string original = _lines[index];
            if (original.Trim() == ChoicesBegin)
            {
                int end = _lines.FindIndex(index + 1, line => line.Trim() == ChoicesEnd);
                int nextBegin = _lines.FindIndex(index + 1, line => line.Trim() == ChoicesBegin);
                // A player may accidentally remove a marker while editing.
                // Strip only a complete, unnested generated block so later
                // comments and unfamiliar settings remain in the document.
                if (end >= 0 && (nextBegin < 0 || nextBegin > end))
                {
                    index = end;
                    continue;
                }
            }
            string line = original.Trim();
            if (TrySection(line, out string? name)) section = name;
            if (TryEntry(original, section, out string? key, out _, out int equals) &&
                values.TryGetValue(key, out string? value))
            {
                output.Add(original[..(equals + 1)] + " " + WriteValue(value));
                written.Add(key);
            }
            else output.Add(original);
        }

        foreach (var group in values.Where(pair => !written.Contains(pair.Key))
            .GroupBy(pair => pair.Key[..pair.Key.IndexOf('.')], StringComparer.OrdinalIgnoreCase))
        {
            output.Add("");
            output.Add("[" + group.Key + "]");
            foreach (var pair in group)
            {
                if (descriptions.TryGetValue(pair.Key, out string? description))
                    output.Add("# " + SingleLine(description));
                output.Add(pair.Key[(pair.Key.IndexOf('.') + 1)..] + " = " + WriteValue(pair.Value));
            }
        }
        while (output.Count > 0 && output[^1].Length == 0) output.RemoveAt(output.Count - 1);
        output.Add("");
        output.Add(ChoicesBegin);
        foreach (var pair in choices.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            output.Add("# " + pair.Key + ":");
            foreach (string choice in pair.Value.Distinct(StringComparer.OrdinalIgnoreCase))
                output.Add("#   " + SingleLine(choice));
        }
        output.Add(ChoicesEnd);
        return string.Join("\n", output) + "\n";
    }

    internal bool MatchesDisk(string path)
    {
        if (!File.Exists(path)) return Fingerprint == null;
        // Do not allocate unbounded memory if an external editor replaces
        // this small settings document with an unexpectedly large file.
        if (new FileInfo(path).Length > 1024 * 1024) return false;
        return string.Equals(Fingerprint, Hash(File.ReadAllBytes(path)),
            StringComparison.Ordinal);
    }

    internal bool HasSameText(string text) =>
        string.Join("\n", _lines).TrimEnd('\n') == text.TrimEnd('\n');

    internal void SaveAtomic(string path, string text)
    {
        if (!MatchesDisk(path)) throw new ConfigChangedOutsideGameException();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temporary = path + ".tmp";
        try
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(text);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            if (!MatchesDisk(path)) throw new ConfigChangedOutsideGameException();
            File.Move(temporary, path, overwrite: true);
            Fingerprint = Hash(bytes);
            _lines.Clear();
            _lines.AddRange(text.TrimEnd('\n').Split('\n'));
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static string ReadValue(string raw)
    {
        string trimmed = raw.Trim();
        if (trimmed.StartsWith('"'))
            return JsonSerializer.Deserialize<string>(trimmed) ?? string.Empty;
        return trimmed;
    }

    private static string WriteValue(string value) =>
        value.IndexOfAny(new[] { '\r', '\n', '"' }) >= 0 || value != value.Trim()
            ? JsonSerializer.Serialize(value, QuotedValues) : value;

    private static bool TrySection(string line, out string name)
    {
        name = string.Empty;
        if (line.Length < 3 || line[0] != '[' || line[^1] != ']') return false;
        name = line[1..^1].Trim();
        return name.Length != 0;
    }

    private static bool TryEntry(string line, string section, out string key,
        out string raw, out int equals)
    {
        key = raw = string.Empty;
        equals = line.IndexOf('=');
        string trimmed = line.TrimStart();
        if (section.Length == 0 || equals <= 0 || trimmed.StartsWith('#') ||
            trimmed.StartsWith(';') || trimmed.StartsWith('[')) return false;
        string name = line[..equals].Trim();
        if (name.Length == 0) return false;
        key = section + "." + name;
        raw = line[(equals + 1)..];
        return true;
    }

    private static string SingleLine(string text) => text.Replace('\r', ' ').Replace('\n', ' ');
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}

internal sealed class ConfigChangedOutsideGameException : IOException
{
    internal ConfigChangedOutsideGameException() : base("The configuration changed outside the game.") { }
}
