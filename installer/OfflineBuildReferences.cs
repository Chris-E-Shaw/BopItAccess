using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace BopItAccess.Installer;

/// <summary>Build-only references from this user's game, without starting the game or modifying it.</summary>
internal static class OfflineBuildReferences
{
    private const string Cpp2IlUrl = "https://github.com/SamboyCoding/Cpp2IL/releases/download/2022.1.0-pre-release.21/Cpp2IL-2022.1.0-pre-release.21-Windows.exe";
    private const string Cpp2IlHash = "663fb432433b4371fd1ee0ebc321a8fff2a9aac5ac4230c843f9e03ddee4e04c";
    private const string PluginUrl = "https://github.com/SamboyCoding/Cpp2IL/releases/download/2022.1.0-pre-release.21/Cpp2IL.Plugin.StrippedCodeRegSupport.dll";
    private const string PluginHash = "2cc4f8c66541f18b4daed5410149ee7859c3190454d30bda5ccd0895d2989ee6";
    private const string UnityUrl = "https://github.com/LavaGang/MelonLoader.UnityDependencies/releases/download/2022.3.50/Managed.zip";
    private const string UnityHash = "7b770bce892b813ed26347c21ba6d860eefe773bfdb0d7a0c7b8b6ccd3723504";
    private static readonly string[] RequiredProxies =
    {
        "Assembly-CSharp.dll", "Il2Cppmscorlib.dll", "UnityEngine.CoreModule.dll", "Unity.Localization.dll",
        "UnityEngine.AudioModule.dll", "Il2CppFMODUnity.dll", "UnityEngine.UI.dll", "UnityEngine.UIModule.dll",
        "Unity.TextMeshPro.dll", "Unity.InputSystem.dll", "Il2CppDOTween.dll"
    };
    private static readonly string[] HelperReferences =
    {
        "AsmResolver", "AsmResolver.DotNet", "AsmResolver.PE", "AsmResolver.PE.File",
        "Il2CppInterop.Generator", "Il2CppInterop.Common", "Microsoft.Extensions.Logging.Abstractions", "System.Diagnostics.DiagnosticSource"
    };

    internal static async Task<string> PrepareAsync(string dotnet, string game, string loaderRoot,
        string temporaryRoot, Action<string> log, Action<string, long, long?> progress,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        if (InstallTransaction.IsWithin(temporaryRoot, game))
            throw new InvalidOperationException("Offline build outputs must be outside the game directory.");
        var gameAssembly = Path.Combine(game, "GameAssembly.dll");
        ValidateRegularFile(gameAssembly);
        string unityVersion = ReadUnityVersion(game);
        if (unityVersion != "2022.3.50f1")
            throw new InvalidOperationException("Offline alpha compilation currently supports Bop It!'s Unity 2022.3.50f1 build. " +
                "This installation reports " + unityVersion + ". No game files have been changed.");
        var existing = Path.Combine(game, "MelonLoader", "Il2CppAssemblies");
        bool reuseProxies = HasCompleteProxies(existing) && CacheMatchesGame(game, gameAssembly);
        bool sameLoaderRoot = string.Equals(Path.GetFullPath(game).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(loaderRoot).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        if (reuseProxies && sameLoaderRoot)
        {
            log("Reusing complete game-generated build references whose cache matches this game binary.");
            return game;
        }

        var references = Path.Combine(temporaryRoot, "build-references");
        if (Directory.Exists(references)) throw new IOException("Temporary build-reference directory already exists.");
        Directory.CreateDirectory(references);
        var stagedLoader = Path.Combine(references, "MelonLoader", "net6");
        var loaderLibraries = Path.Combine(loaderRoot, "MelonLoader", "net6");
        Directory.CreateDirectory(stagedLoader);
        foreach (var file in Directory.EnumerateFiles(loaderLibraries, "*", SearchOption.TopDirectoryOnly))
        {
            cancellation.ThrowIfCancellationRequested();
            ValidateRegularFile(file);
            File.Copy(file, Path.Combine(stagedLoader, Path.GetFileName(file)));
        }
        foreach (var name in HelperReferences) ValidateRegularFile(Path.Combine(stagedLoader, name + ".dll"));
        if (reuseProxies)
        {
            var reused = Path.Combine(references, "MelonLoader", "Il2CppAssemblies");
            Directory.CreateDirectory(reused);
            foreach (string file in Directory.EnumerateFiles(existing, "*.dll", SearchOption.TopDirectoryOnly))
            {
                cancellation.ThrowIfCancellationRequested();
                ValidateRegularFile(file);
                File.Copy(file, Path.Combine(reused, Path.GetFileName(file)));
            }
            if (!HasCompleteProxies(reused)) throw new InvalidDataException("Reused temporary references are incomplete.");
            log("Reusing matching game proxies with the staged, verified MelonLoader build libraries.");
            return references;
        }
        var cpp2IlRoot = Path.Combine(temporaryRoot, "cpp2il");
        Directory.CreateDirectory(cpp2IlRoot);
        var cpp2Il = Path.Combine(cpp2IlRoot, "Cpp2IL.exe");
        await DownloadVerifiedAsync(Cpp2IlUrl, Cpp2IlHash, cpp2Il, "Downloading offline assembly tool", progress, cancellation);
        await DownloadVerifiedAsync(PluginUrl, PluginHash,
            Path.Combine(cpp2IlRoot, "Plugins", "Cpp2IL.Plugin.StrippedCodeRegSupport.dll"),
            "Downloading offline assembly plugin", progress, cancellation);
        var unityArchive = Path.Combine(temporaryRoot, "unity-libraries.zip");
        await DownloadVerifiedAsync(UnityUrl, UnityHash, unityArchive, "Downloading Unity build libraries", progress, cancellation);
        var unityLibraries = Path.Combine(temporaryRoot, "unity-libraries");
        SafeZip.Extract(unityArchive, unityLibraries,
            (done, total) => progress("Extracting Unity build libraries", done, total), cancellation);
        ValidateRegularFile(Path.Combine(unityLibraries, "UnityEngine.CoreModule.dll"));

        log("Generating temporary alpha build references from local game files. The game will not be launched.");
        progress("Reading game assemblies offline", 0, null);
        var dumped = Path.Combine(cpp2IlRoot, "cpp2il_out");
        await RunProcessAsync(cpp2Il, cpp2IlRoot,
            new[] { "--game-path", game, "--exe-name", "BopIt!", "--output-as", "dummydll", "--use-processor",
                "attributeanalyzer", "attributeinjector", "--output-to", dumped }, "Cpp2IL", TimeSpan.FromMinutes(8), dotnet, log, cancellation);
        ValidateRegularFile(Path.Combine(dumped, "Assembly-CSharp.dll"));

        var helper = Path.Combine(temporaryRoot, "reference-helper");
        Directory.CreateDirectory(helper);
        await WriteResourceAsync("Program.cs.txt", Path.Combine(helper, "Program.cs"), cancellation);
        var projectText = await ReadResourceAsync("BuildReferenceGenerator.csproj.txt", cancellation);
        string Escape(string text) => SecurityElement.Escape(text) ?? "";
        string referenceXml = string.Join(Environment.NewLine, HelperReferences.Select(name =>
            "<Reference Include=\"" + Escape(name) + "\"><HintPath>" + Escape(Path.Combine(stagedLoader, name + ".dll")) +
            "</HintPath><Private>true</Private></Reference>"));
        projectText = projectText.Replace("{{REFERENCES}}", referenceXml, StringComparison.Ordinal)
            .Replace("{{LOADER_DIRECTORY}}", Escape(stagedLoader), StringComparison.Ordinal);
        var project = Path.Combine(helper, "BuildReferenceGenerator.csproj");
        await File.WriteAllTextAsync(project, projectText, new UTF8Encoding(false), cancellation);
        await File.WriteAllTextAsync(Path.Combine(helper, "NuGet.Config"),
            "<configuration><packageSources><clear /></packageSources></configuration>", cancellation);
        progress("Building offline reference helper", 0, null);
        await RunProcessAsync(dotnet, helper,
            new[] { "build", project, "-c", "Release", "--nologo", "-p:RestoreConfigFile=" + Path.Combine(helper, "NuGet.Config") },
            "Reference helper build", TimeSpan.FromMinutes(3), dotnet, log, cancellation);
        var helperDll = Path.Combine(helper, "bin", "Release", "net6.0", "BopItAccess.BuildReferenceGenerator.dll");
        ValidateRegularFile(helperDll);
        var proxies = Path.Combine(references, "MelonLoader", "Il2CppAssemblies");
        Directory.CreateDirectory(proxies);
        progress("Generating temporary interop references", 0, null);
        await RunProcessAsync(dotnet, helper, new[] { helperDll, gameAssembly, dumped, unityLibraries, proxies },
            "Interop reference generator", TimeSpan.FromMinutes(8), dotnet, log, cancellation);
        if (!HasCompleteProxies(proxies)) throw new InvalidDataException("Offline generation did not produce all required build references.");
        progress("Generating temporary interop references", 1, 1);
        log("Verified temporary build references. MelonLoader will generate its runtime assemblies at the player's first launch.");
        return references;
    }

    private static bool CacheMatchesGame(string game, string gameAssembly)
    {
        var configuration = Path.Combine(game, "MelonLoader", "Dependencies", "Il2CppAssemblyGenerator", "Config.cfg");
        try
        {
            ValidateRegularFile(configuration);
            if (new FileInfo(configuration).Length > 1024 * 1024) return false;
            string text = File.ReadAllText(configuration);
            var match = Regex.Match(text, "(?m)^\\s*GameAssemblyHash\\s*=\\s*\"([a-fA-F0-9]{128})\"\\s*$", RegexOptions.CultureInvariant);
            return match.Success && InstallerNetwork.Sha512(gameAssembly).Equals(match.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { return false; }
    }

    private static string ReadUnityVersion(string game)
    {
        var metadata = Path.Combine(game, "BopIt!_Data", "globalgamemanagers");
        ValidateRegularFile(metadata);
        using var input = File.OpenRead(metadata);
        var header = new byte[512];
        int count = input.Read(header, 0, header.Length);
        var match = Regex.Match(Encoding.ASCII.GetString(header, 0, count), @"\d{4}\.\d+\.\d+[abfp]\d+", RegexOptions.CultureInvariant);
        if (!match.Success) throw new InvalidDataException("Could not identify the Unity version from this game's metadata header.");
        return match.Value;
    }

    private static bool HasCompleteProxies(string directory)
    {
        try
        {
            foreach (var name in RequiredProxies)
            {
                string path = Path.Combine(directory, name);
                ValidateRegularFile(path);
                using var file = File.OpenRead(path);
                using var pe = new PEReader(file);
                if (!pe.HasMetadata || !pe.GetMetadataReader().IsAssembly) return false;
            }
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or BadImageFormatException) { return false; }
    }

    private static void ValidateRegularFile(string file)
    {
        if (!File.Exists(file) || new FileInfo(file).Length == 0)
            throw new FileNotFoundException("Required offline build input is missing or empty: " + file, file);
        for (var path = Path.GetFullPath(file); path is not null; path = Path.GetDirectoryName(path))
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Offline build input passes through a linked path: " + path);
    }

    private static async Task DownloadVerifiedAsync(string url, string hash, string destination, string step,
        Action<string, long, long?> progress, CancellationToken cancellation)
    {
        await InstallerNetwork.DownloadAsync(new Uri(url), destination, (done, total) => progress(step, done, total), cancellation);
        InstallerNetwork.VerifyHash(destination, hash);
    }

    private static async Task<string> ReadResourceAsync(string name, CancellationToken cancellation)
    {
        await using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(
            "BopItAccess.Installer.BuildReferenceGenerator." + name)
            ?? throw new InvalidDataException("Missing embedded offline build helper resource: " + name);
        using var reader = new StreamReader(resource, Encoding.UTF8);
        return await reader.ReadToEndAsync(cancellation);
    }

    private static async Task WriteResourceAsync(string name, string destination, CancellationToken cancellation) =>
        await File.WriteAllTextAsync(destination, await ReadResourceAsync(name, cancellation), new UTF8Encoding(false), cancellation);

    private static async Task RunProcessAsync(string executable, string workingDirectory, IEnumerable<string> arguments,
        string label, TimeSpan timeout, string dotnet, Action<string> log, CancellationToken cancellation)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(timeout);
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory, UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["DOTNET_ROOT"] = Path.GetDirectoryName(dotnet)!;
        start.Environment["DOTNET_ROOT_X64"] = Path.GetDirectoryName(dotnet)!;
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        start.Environment["NO_COLOR"] = "1";
        var elapsed = Stopwatch.StartNew();
        InstallerDiagnostics.Current?.Write("process", label + " starting: " + executable + "; working_directory=" + workingDirectory);
        using var process = Process.Start(start) ?? throw new IOException("Could not start " + label + ".");
        InstallerDiagnostics.Current?.Write("process", label + " started: pid=" + process.Id);
        async Task DrainAsync(StreamReader input, string stream)
        {
            while (await input.ReadLineAsync(deadline.Token) is { } line) log(label + " " + stream + ": " + line);
        }
        var tasks = new[]
        {
            DrainAsync(process.StandardOutput, "stdout"), DrainAsync(process.StandardError, "stderr"),
            process.WaitForExitAsync(deadline.Token)
        };
        try
        {
            var pending = new List<Task>(tasks);
            // Observe a failed stream immediately; waiting for all tasks first
            // could leave a child blocked on an undrained redirected pipe.
            while (pending.Count > 0)
            {
                Task completed = await Task.WhenAny(pending);
                await completed;
                pending.Remove(completed);
            }
        }
        catch (Exception error)
        {
            InstallerDiagnostics.Current?.Error(label + " failed; pid=" + process.Id + "; elapsed_ms=" + elapsed.ElapsedMilliseconds, error);
            deadline.Cancel();
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
            await process.WaitForExitAsync(CancellationToken.None);
            // Observe canceled/faulted drains before disposing their streams.
            try { await Task.WhenAll(tasks); } catch { /* Preserve the first failure above. */ }
            if (error is OperationCanceledException && !cancellation.IsCancellationRequested)
                throw new TimeoutException(label + " did not finish within " + timeout.TotalMinutes + " minutes. No game launch was performed.", error);
            throw;
        }
        InstallerDiagnostics.Current?.Write("process", label + " exited: pid=" + process.Id + "; exit_code=" + process.ExitCode +
            "; elapsed_ms=" + elapsed.ElapsedMilliseconds);
        if (process.ExitCode != 0) throw new InvalidOperationException(label + " failed. Review its messages in the installer diagnostic log.");
    }
}
