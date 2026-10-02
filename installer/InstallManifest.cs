using System.Text.Json;

namespace BopItAccess.Installer;

/// <summary>
/// Durable ownership record for one installed Bop It Access copy. Paths are absolute
/// and are checked against GameDirectory/StateDirectory before removal.
/// </summary>
public sealed class InstallManifest
{
    public const int CurrentSchemaVersion = 1;
    public const string FileName = "install-manifest.json";

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public string GameDirectory { get; set; } = string.Empty;
    public string StateDirectory { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTimeOffset InstalledAtUtc { get; set; }
    public List<InstalledFile> Files { get; set; } = new();
    public List<string> GeneratedMelonLoaderFiles { get; set; } = new();
    public List<string> CreatedDirectories { get; set; } = new();
    public bool MelonLoaderInstalledByInstaller { get; set; }
    public bool ModLogExistedBeforeInstall { get; set; }
    public string? UninstallRegistryKey { get; set; }

    public static InstallManifest Load(string path)
    {
        using var stream = File.OpenRead(path);
        var manifest = JsonSerializer.Deserialize<InstallManifest>(stream, JsonOptions)
            ?? throw new InvalidDataException("The installation manifest is empty.");
        if (manifest.SchemaVersion != CurrentSchemaVersion)
            throw new InvalidDataException($"Unsupported installation manifest version {manifest.SchemaVersion}.");
        if (string.IsNullOrWhiteSpace(manifest.GameDirectory) ||
            string.IsNullOrWhiteSpace(manifest.StateDirectory))
            throw new InvalidDataException("The installation manifest has no game or state directory.");
        return manifest;
    }

    public static void SaveAtomic(InstallManifest manifest, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".new";
        using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, manifest, JsonOptions);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, path, overwrite: true);
    }

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
}

public sealed class InstalledFile
{
    public string Path { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Copy of a file that existed before the first installer-managed install.
    /// It is restored on uninstall. Null means that the installer created Path.
    /// </summary>
    public string? OriginalBackupPath { get; set; }
    public string? OriginalSha256 { get; set; }
}
