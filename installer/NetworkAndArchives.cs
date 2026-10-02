using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace BopItAccess.Installer;

internal sealed record GitHubRelease(string Tag, string AssetName, Uri AssetUrl);

internal static class InstallerNetwork
{
    internal const string MelonUrl = "https://github.com/LavaGang/MelonLoader/releases/download/v0.7.3/MelonLoader.x64.zip";
    internal const string MelonSha256 = "5b2b2f3d1cd42b59ec886c5bdc2663edae87a0097a4f4a8f58c0965a99dda416";
    internal const string PrismUrl = "https://github.com/ethindp/prism/releases/download/v0.18.3/prism-windows-x64.zip";
    internal const string PrismSha256 = "13d3c9d1d524b0737cde261c6468845753e36b6c5c2637e7d26bfb8bf4d8783f";
    internal const string PrismDllSha256 = "7c7d09c8c7306e8e1a0c46d603ccb2a391c194c77843400c11b746e7dd56a467";
    internal const string SdkUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/6.0.428/dotnet-sdk-6.0.428-win-x64.zip";
    internal const string SdkSha512 = "c027cb47b264a13e529f8c7f3ba33ac91152b56749c8681fede1d6cd48723ae1e5f04a43bac1302ee81e35a5383f3e169654e5bb7c1d331dc11cce5a95052e32";
    private static readonly HttpClient Client = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BopItAccessInstaller/0.1 (+https://github.com/Chris-E-Shaw/BopItAccess)");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return client;
    }

    internal static async Task<GitHubRelease?> LatestReleaseAsync(CancellationToken cancellation)
    {
        using var response = await Client.GetAsync("https://api.github.com/repos/Chris-E-Shaw/BopItAccess/releases/latest", cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellation);
        var root = json.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? throw new InvalidDataException("Release tag is missing.");
        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString() ?? "";
            if (!name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                !name.Contains("BopItAccess", StringComparison.OrdinalIgnoreCase)) continue;
            var url = asset.GetProperty("browser_download_url").GetString();
            if (url is not null && Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                return new GitHubRelease(tag, name, uri);
        }
        return null;
    }

    internal static async Task<string> LatestCommitAsync(CancellationToken cancellation)
    {
        using var response = await Client.GetAsync("https://api.github.com/repos/Chris-E-Shaw/BopItAccess/commits/main", cancellation);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellation);
        var sha = json.RootElement.GetProperty("sha").GetString() ?? "";
        if (sha.Length != 40 || !sha.All(Uri.IsHexDigit)) throw new InvalidDataException("GitHub returned an invalid commit ID.");
        return sha;
    }

    internal static async Task DownloadAsync(Uri url, string destination, Action<long, long?> progress, CancellationToken cancellation)
    {
        if (url.Scheme != Uri.UriSchemeHttps) throw new InvalidDataException("Downloads must use HTTPS.");
        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
        response.EnsureSuccessStatusCode();
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        await using var input = await response.Content.ReadAsStreamAsync(cancellation);
        await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true);
        var buffer = new byte[1024 * 128];
        long completed = 0;
        var total = response.Content.Headers.ContentLength;
        int count;
        while ((count = await input.ReadAsync(buffer, cancellation)) > 0)
        {
            await output.WriteAsync(buffer.AsMemory(0, count), cancellation);
            completed += count;
            progress(completed, total);
        }
        await output.FlushAsync(cancellation);
        if (total is not null && completed != total.Value) throw new InvalidDataException("Download size did not match the server response.");
    }

    internal static string Sha256(string path) => Hash(path, SHA256.Create());
    internal static string Sha512(string path) => Hash(path, SHA512.Create());

    private static string Hash(string path, HashAlgorithm algorithm)
    {
        using (algorithm)
        using (var file = File.OpenRead(path))
            return Convert.ToHexString(algorithm.ComputeHash(file)).ToLowerInvariant();
    }

    internal static void VerifyHash(string path, string expected, bool useSha512 = false)
    {
        var actual = useSha512 ? Sha512(path) : Sha256(path);
        if (!actual.Equals(expected, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Checksum mismatch for {Path.GetFileName(path)}. The download was not installed.");
    }
}

internal static class SafeZip
{
    internal static void Extract(string archivePath, string target, Action<long, long?> progress, CancellationToken cancellation)
    {
        Directory.CreateDirectory(target);
        var root = Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        using var archive = ZipFile.OpenRead(archivePath);
        if (archive.Entries.Count > 50000) throw new InvalidDataException("Archive has too many entries.");
        long total = archive.Entries.Sum(e => e.Length);
        if (total > 4L * 1024 * 1024 * 1024) throw new InvalidDataException("Archive is too large when extracted.");
        long done = 0;
        foreach (var entry in archive.Entries)
        {
            cancellation.ThrowIfCancellationRequested();
            var relative = entry.FullName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(relative) || relative.StartsWith(Path.DirectorySeparatorChar) || relative.Contains(':'))
                throw new InvalidDataException("Archive contains an unsafe path.");
            if (((entry.ExternalAttributes >> 16) & 0xF000) == 0xA000)
                throw new InvalidDataException("Archive contains a symbolic link.");
            var dest = Path.GetFullPath(Path.Combine(root, relative));
            if (!dest.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Archive attempted to escape its extraction directory.");
            if (entry.FullName.EndsWith('/')) { Directory.CreateDirectory(dest); continue; }
            if (entry.Length > 1024L * 1024 * 1024) throw new InvalidDataException("Archive entry is too large.");
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            using var input = entry.Open();
            using var output = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[1024 * 128];
            int count;
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellation.ThrowIfCancellationRequested();
                output.Write(buffer, 0, count);
                done += count;
                progress(done, total);
            }
        }
    }
}
