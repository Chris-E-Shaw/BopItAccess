using System.Diagnostics;
using System.Reflection.PortableExecutable;
using Microsoft.Win32;

namespace BopItAccess.Installer;

internal sealed record InstallerDependencies(string? Dotnet, string? RuntimeSource);

internal static class SdkAndBuild
{
    private static IEnumerable<string> InstalledRoots()
    {
        var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet")
        };
        using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using var dotnet = machine.OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64");
        if (dotnet?.GetValue("InstallLocation") is string registered && !string.IsNullOrWhiteSpace(registered))
            locations.Add(registered);
        var envRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot)) locations.Add(envRoot);
        var x64Root = Environment.GetEnvironmentVariable("DOTNET_ROOT_X64");
        if (!string.IsNullOrWhiteSpace(x64Root)) locations.Add(x64Root);
        return locations;
    }

    private static bool HasRuntime(string root)
    {
        try
        {
            string runtimes = Path.Combine(root, "shared", "Microsoft.NETCore.App");
            string host = Path.Combine(root, "host", "fxr");
            return IsX64Executable(Path.Combine(root, "dotnet.exe")) && Directory.Exists(host) &&
                Directory.EnumerateDirectories(host).Any(path => File.Exists(Path.Combine(path, "hostfxr.dll"))) &&
                Directory.Exists(runtimes) && Directory.EnumerateDirectories(runtimes, "6.0.*")
                    .Any(path => File.Exists(Path.Combine(path, "System.Private.CoreLib.dll")) &&
                        File.Exists(Path.Combine(path, "coreclr.dll")));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        { return false; }
    }

    private static bool IsX64Executable(string path)
    {
        try
        {
            using var file = File.OpenRead(path);
            using var image = new PEReader(file);
            return image.PEHeaders.CoffHeader.Machine == Machine.Amd64;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or BadImageFormatException)
        { return false; }
    }

    private static bool HasSdk(string root)
    {
        try
        {
            string sdks = Path.Combine(root, "sdk");
            string packs = Path.Combine(root, "packs", "Microsoft.NETCore.App.Ref");
            return HasRuntime(root) && Directory.Exists(sdks) &&
                Directory.EnumerateDirectories(sdks).Any(path =>
                    Version.TryParse(Path.GetFileName(path), out Version? version) && version.Major >= 6) &&
                Directory.Exists(packs) && Directory.EnumerateDirectories(packs, "6.0.*")
                    .Any(path => File.Exists(Path.Combine(path, "ref", "net6.0", "System.Runtime.dll")));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        { return false; }
    }

    internal static async Task<InstallerDependencies> EnsureDependenciesAsync(bool buildFromSource,
        string gamePath, string tempRoot, Action<string> log,
        Action<string, long, long?> progress, CancellationToken cancellation)
    {
        var portableRoot = Path.Combine(gamePath, "dotnet");
        var roots = InstalledRoots().ToArray();
        if (buildFromSource)
        {
            var sdkRoot = roots.Prepend(portableRoot).FirstOrDefault(HasSdk);
            if (sdkRoot is not null)
            {
                log("Reusing an existing SDK with the .NET 6 runtime and targeting pack: " + sdkRoot + ".");
                return new(Path.Combine(sdkRoot, "dotnet.exe"), null);
            }
            await InstallMicrosoftSdkAsync(tempRoot, log, progress, cancellation);
            sdkRoot = InstalledRoots().FirstOrDefault(HasSdk);
            if (sdkRoot is null)
                throw new InvalidOperationException("Microsoft's SDK installer completed, but a .NET SDK with the .NET 6 runtime and targeting pack could not be found. Restart Windows if requested and retry.");
            return new(Path.Combine(sdkRoot, "dotnet.exe"), null);
        }
        var runtimeRoot = roots.Prepend(Path.Combine(gamePath, "MelonLoader", "Dependencies", "dotnet"))
            .Prepend(portableRoot).FirstOrDefault(HasRuntime);
        if (runtimeRoot is not null)
        {
            log("Verified the existing .NET 6 runtime: " + runtimeRoot + ". A release install does not need an SDK.");
            return new(null, null);
        }
        string archive = Path.Combine(tempRoot, "dotnet-runtime.zip");
        log("Downloading only Microsoft's .NET 6 runtime. A compiled release does not need the SDK or targeting pack.");
        await InstallerNetwork.DownloadAsync(new Uri(InstallerNetwork.RuntimeUrl), archive,
            (done, total) => progress("Downloading .NET runtime", done, total), cancellation);
        InstallerNetwork.VerifyHash(archive, InstallerNetwork.RuntimeSha512, useSha512: true);
        string runtime = Path.Combine(tempRoot, "dotnet-runtime");
        SafeZip.Extract(archive, runtime, (done, total) => progress("Extracting .NET runtime", done, total), cancellation);
        if (!HasRuntime(runtime)) throw new InvalidDataException("Microsoft's runtime archive is missing required .NET 6 files.");
        log("Prepared the .NET 6 runtime for MelonLoader's Dependencies folder. It will be recorded for rollback and uninstall.");
        return new(null, runtime);
    }

    private static async Task InstallMicrosoftSdkAsync(string tempRoot,
        Action<string> log, Action<string, long, long?> progress, CancellationToken cancellation)
    {
        const string name = ".NET 6 SDK and targeting pack";
        string package = Path.Combine(tempRoot, "dotnet-sdk.exe");
        log("Downloading Microsoft's " + name + ".");
        await InstallerNetwork.DownloadAsync(new Uri(InstallerNetwork.SdkUrl),
            package, (done, total) => progress("Downloading " + name, done, total), cancellation);
        InstallerNetwork.VerifyHash(package, InstallerNetwork.SdkSha512, useSha512: true);
        cancellation.ThrowIfCancellationRequested();
        log("Installing " + name + " into Windows' shared .NET location. No SDK folder will be created in the game. Shared .NET components remain after abort or uninstall.");
        progress("Installing " + name, 0, null);
        var info = new ProcessStartInfo(package) { UseShellExecute = false, CreateNoWindow = true };
        info.ArgumentList.Add("/install");
        info.ArgumentList.Add("/quiet");
        info.ArgumentList.Add("/norestart");
        info.ArgumentList.Add("/log");
        info.ArgumentList.Add(Path.Combine(tempRoot, "sdk-install.log"));
        var elapsed = Stopwatch.StartNew();
        using var process = Process.Start(info) ?? throw new IOException("Could not start Microsoft's " + name + " installer.");
        InstallerDiagnostics.Current?.Write("process", "Microsoft dependency installer started: " + name + "; pid=" + process.Id + ".");
        using var registration = cancellation.Register(() => log("Abort requested. Waiting for Microsoft's dependency installation to finish safely before reversing mod files."));
        // Interrupting MSI midway through a shared runtime change can damage
        // other applications. Finish this dependency step, then honour abort.
        await process.WaitForExitAsync(CancellationToken.None);
        InstallerDiagnostics.Current?.Write("process", "Microsoft dependency installer exited: " + name +
            "; exit_code=" + process.ExitCode + "; elapsed_ms=" + elapsed.ElapsedMilliseconds + ".");
        try
        {
            string dependencyLog = Path.Combine(tempRoot, "sdk-install.log");
            if (File.Exists(dependencyLog))
                foreach (string line in File.ReadLines(dependencyLog).TakeLast(30))
                    InstallerDiagnostics.Current?.Write("dependency", line);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            InstallerDiagnostics.Current?.Error("Could not read Microsoft's dependency log", ex);
        }
        if (process.ExitCode is not (0 or 3010))
            throw new InvalidOperationException("Microsoft's " + name + " installer returned code " + process.ExitCode + ". Review the saved diagnostics.");
        if (process.ExitCode == 3010) log("Microsoft's dependency installer requested a Windows restart. Restart before launching the game.");
        progress("Installing " + name, 1, 1);
        cancellation.ThrowIfCancellationRequested();
    }

    internal static async Task<string> BuildAsync(string dotnet, string sourceRoot, string gamePath,
        Action<string> log, Action<string, long, long?> progress, CancellationToken cancellation)
    {
        var project = Path.Combine(sourceRoot, "src", "BopItAccess.csproj");
        if (!File.Exists(project)) throw new InvalidDataException("The GitHub source archive did not contain the mod project.");
        log("Compiling the latest source against locally prepared game build references. The game stays closed.");
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
        info.Environment["DOTNET_ROOT_X64"] = Path.GetDirectoryName(dotnet)!;
        info.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(TimeSpan.FromMinutes(10));
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
            while (await reader.ReadLineAsync(deadline.Token) is { } line)
            {
                lock (outputLock)
                {
                    if (output.Count == 4) output.Dequeue();
                    output.Enqueue(line);
                }
                log("Build " + streamName + ": " + line);
            }
        }
        var tasks = new[]
        {
            DrainAsync(process.StandardOutput, "stdout"), DrainAsync(process.StandardError, "stderr"),
            process.WaitForExitAsync(deadline.Token)
        };
        try
        {
            var pending = new List<Task>(tasks);
            while (pending.Count > 0)
            {
                Task completed = await Task.WhenAny(pending);
                await completed;
                pending.Remove(completed);
            }
        }
        catch (OperationCanceledException)
        {
            InstallerDiagnostics.Current?.Write("process",
                $"Compiler cancellation requested: pid={process.Id}; elapsed_ms={elapsed.ElapsedMilliseconds}.");
            deadline.Cancel();
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            // Kill signals termination; wait for it before rollback/temp
            // cleanup touches compiler-owned outputs and open file handles.
            await process.WaitForExitAsync(CancellationToken.None);
            try { await Task.WhenAll(tasks); } catch { /* Preserve the original cancellation. */ }
            InstallerDiagnostics.Current?.Write("process",
                $"Compiler stopped after cancellation: pid={process.Id}; exit_code={process.ExitCode}; elapsed_ms={elapsed.ElapsedMilliseconds}.");
            if (!cancellation.IsCancellationRequested)
                throw new TimeoutException("The source build exceeded ten minutes and was stopped.");
            throw;
        }
        catch (Exception ex)
        {
            InstallerDiagnostics.Current?.Error($"Compiler output or wait failed: pid={process.Id}; elapsed_ms={elapsed.ElapsedMilliseconds}", ex);
            deadline.Cancel();
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            await process.WaitForExitAsync(CancellationToken.None);
            try { await Task.WhenAll(tasks); } catch { /* Preserve the original stream/wait failure. */ }
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
