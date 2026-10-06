using System.Diagnostics;

namespace BopItAccess.Installer;

internal static class SdkAndBuild
{
    private static string? FindInstalledSdk()
    {
        var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet")
        };
        var envRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot)) locations.Add(envRoot);
        var x64Root = Environment.GetEnvironmentVariable("DOTNET_ROOT_X64");
        if (!string.IsNullOrWhiteSpace(x64Root)) locations.Add(x64Root);
        foreach (var location in locations)
            if (HasSdk(location)) return Path.Combine(location, "dotnet.exe");
        return null;
    }

    private static bool HasSdk(string root)
    {
        try
        {
            string sdks = Path.Combine(root, "sdk");
            string packs = Path.Combine(root, "packs", "Microsoft.NETCore.App.Ref");
            string runtimes = Path.Combine(root, "shared", "Microsoft.NETCore.App");
            return File.Exists(Path.Combine(root, "dotnet.exe")) && Directory.Exists(sdks) &&
                Directory.EnumerateDirectories(sdks).Any(path =>
                    Version.TryParse(Path.GetFileName(path), out Version? version) && version.Major >= 6) &&
                Directory.Exists(packs) && Directory.EnumerateDirectories(packs, "6.0.*")
                    .Any(path => File.Exists(Path.Combine(path, "ref", "net6.0", "System.Runtime.dll"))) &&
                Directory.Exists(runtimes) && Directory.EnumerateDirectories(runtimes, "6.0.*")
                    .Any(path => File.Exists(Path.Combine(path, "System.Private.CoreLib.dll")));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        { return false; }
    }

    internal static async Task<(string Dotnet, bool AddedPortableSdk)> EnsureSdkAsync(
        string gamePath, string tempRoot, Action<string> log,
        Action<string, long, long?> progress, CancellationToken cancellation)
    {
        var portableRoot = Path.Combine(gamePath, "dotnet");
        if (HasSdk(portableRoot))
        {
            log("Verified a compatible portable SDK with the .NET 6 runtime and targeting pack in the game folder.");
            return (Path.Combine(portableRoot, "dotnet.exe"), false);
        }
        var installed = FindInstalledSdk();
        if (installed is not null)
        {
            log($"Verified an installed compatible SDK with the .NET 6 runtime and targeting pack: {installed}");
            return (installed, false);
        }
        if (Directory.Exists(portableRoot))
            throw new InvalidOperationException("The game already has a dotnet folder, but it does not contain the required .NET 6 SDK. Move or repair that folder before installation.");

        log("Downloading Microsoft's .NET 6.0.428 SDK with the .NET 6 targeting pack.");
        var zip = Path.Combine(tempRoot, "dotnet-sdk.zip");
        await InstallerNetwork.DownloadAsync(new Uri(InstallerNetwork.SdkUrl), zip,
            (done, total) => progress("Downloading .NET SDK", done, total), cancellation);
        log("Verifying Microsoft SDK checksum.");
        InstallerNetwork.VerifyHash(zip, InstallerNetwork.SdkSha512, useSha512: true);

        var staging = Path.Combine(gamePath, ".bopitaccess-sdk-staging-" + Guid.NewGuid().ToString("N"));
        try
        {
            log("Extracting the portable SDK beside the game. It will remain after uninstall.");
            SafeZip.Extract(zip, staging,
                (done, total) => progress("Extracting .NET SDK", done, total), cancellation);
            if (!HasSdk(staging)) throw new InvalidDataException("The SDK archive did not contain the expected SDK and targeting pack.");
            cancellation.ThrowIfCancellationRequested();
            Directory.Move(staging, portableRoot);
            log($"Installed portable .NET 6 SDK at {portableRoot}.");
            return (Path.Combine(portableRoot, "dotnet.exe"), true);
        }
        finally
        {
            if (Directory.Exists(staging))
            {
                var expectedParent = Path.GetFullPath(gamePath).TrimEnd(Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(staging).TrimEnd(Path.DirectorySeparatorChar);
                if (!string.Equals(Path.GetDirectoryName(full), expectedParent, StringComparison.OrdinalIgnoreCase) ||
                    !Path.GetFileName(full).StartsWith(".bopitaccess-sdk-staging-", StringComparison.OrdinalIgnoreCase) ||
                    (File.GetAttributes(full) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidOperationException("Refusing to remove an unexpected SDK staging directory.");
                Directory.Delete(full, recursive: true);
            }
        }
    }

    internal static async Task<string> BuildAsync(string dotnet, string sourceRoot, string gamePath,
        Action<string> log, Action<string, long, long?> progress, CancellationToken cancellation)
    {
        var project = Path.Combine(sourceRoot, "src", "BopItAccess.csproj");
        if (!File.Exists(project)) throw new InvalidDataException("The GitHub source archive did not contain the mod project.");
        log("Compiling the latest source against this game's generated proxy assemblies.");
        progress("Building Bop It Access", 0, null);
        var info = new ProcessStartInfo(dotnet)
        {
            WorkingDirectory = sourceRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        info.ArgumentList.Add("build");
        info.ArgumentList.Add(project);
        info.ArgumentList.Add("--configuration");
        info.ArgumentList.Add("Release");
        info.ArgumentList.Add("--nologo");
        info.ArgumentList.Add("-p:BopItGameDir=" + gamePath);
        info.Environment["DOTNET_ROOT"] = Path.GetDirectoryName(dotnet)!;
        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        var elapsed = Stopwatch.StartNew();
        InstallerDiagnostics.Current?.Write("process",
            $"Compiler starting: executable={dotnet}; project={project}; game={gamePath}; configuration=Release.");
        Process started;
        try
        {
            started = Process.Start(info) ?? throw new InvalidOperationException("Could not start the .NET compiler.");
        }
        catch (Exception ex)
        {
            InstallerDiagnostics.Current?.Error($"Compiler launch failed; elapsed_ms={elapsed.ElapsedMilliseconds}", ex);
            throw;
        }
        using var process = started;
        InstallerDiagnostics.Current?.Write("process", $"Compiler started: pid={process.Id}.");
        var output = new Queue<string>(4);
        var outputLock = new object();
        async Task DrainAsync(StreamReader reader, string streamName)
        {
            while (await reader.ReadLineAsync(cancellation) is { } line)
            {
                lock (outputLock)
                {
                    if (output.Count == 4) output.Dequeue();
                    output.Enqueue(line);
                }
                log("Build " + streamName + ": " + line);
            }
        }
        try
        {
            await Task.WhenAll(DrainAsync(process.StandardOutput, "stdout"), DrainAsync(process.StandardError, "stderr"),
                process.WaitForExitAsync(cancellation));
        }
        catch (OperationCanceledException)
        {
            InstallerDiagnostics.Current?.Write("process",
                $"Compiler cancellation requested: pid={process.Id}; elapsed_ms={elapsed.ElapsedMilliseconds}.");
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            // Kill signals termination; wait for it before rollback/temp
            // cleanup touches compiler-owned outputs and open file handles.
            await process.WaitForExitAsync(CancellationToken.None);
            InstallerDiagnostics.Current?.Write("process",
                $"Compiler stopped after cancellation: pid={process.Id}; exit_code={process.ExitCode}; elapsed_ms={elapsed.ElapsedMilliseconds}.");
            throw;
        }
        catch (Exception ex)
        {
            InstallerDiagnostics.Current?.Error($"Compiler output or wait failed: pid={process.Id}; elapsed_ms={elapsed.ElapsedMilliseconds}", ex);
            throw;
        }
        InstallerDiagnostics.Current?.Write("process",
            $"Compiler exited: pid={process.Id}; exit_code={process.ExitCode}; elapsed_ms={elapsed.ElapsedMilliseconds}.");
        if (process.ExitCode != 0)
            throw new InvalidOperationException("Source build failed. Review the build messages in the status log. " +
                string.Join(" ", output));
        var dll = Path.Combine(sourceRoot, "src", "bin", "Release", "net6.0", "BopItAccess.dll");
        if (!File.Exists(dll)) throw new FileNotFoundException("The compiler succeeded but did not produce BopItAccess.dll.", dll);
        log("Source build completed.");
        progress("Building Bop It Access", 1, 1);
        return dll;
    }
}
