using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BopItAccess.Installer;

internal sealed record InstallerProgress(string Step, long Completed, long? Total);
internal sealed record InstallerState(string? GamePath, bool ValidGamePath, bool Installed,
    bool ReleaseAvailable, bool UpdateAvailable, bool Busy, string? Message,
    long PathRevision = 0, bool ReadyForOperations = false);

internal sealed class InstallerService
{
    internal event Action<string>? StatusChanged;
    internal event Action<InstallerProgress>? ProgressChanged;
    internal event Action<InstallerState>? StateChanged;

    private readonly string _stateDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
    private readonly object _gamePathLock = new();
    private string? _gamePath;
    private long _gamePathRevision;
    private bool _userSelectedGamePath;
    private GitHubRelease? _release;
    private bool _busy;
    private bool _initializing = true;
    private bool _initialized;
    private int _operationActive;
    private readonly bool _launchedForUninstall;
    private readonly object _progressLock = new();
    private string? _lastProgressStep;
    private long _lastProgressAt;
    private CancellationTokenSource? _abort;
    internal int LastUninstallWarningCount { get; private set; }

    internal InstallerService(bool launchedForUninstall = false) =>
        _launchedForUninstall = launchedForUninstall;

    internal async Task InitializeAsync(CancellationToken cancellation = default)
    {
        try
        {
            await InstallTransaction.RecoverInterruptedAsync(_stateDirectory, Log);
            cancellation.ThrowIfCancellationRequested();
            Log("Searching Steam libraries across available drives.");
            var found = await Task.Run(() => GameLocator.FindInstallations(cancellation), cancellation);
            var manifest = ReadManifest();
            string? detectedPath = manifest is not null
                ? Path.GetFullPath(manifest.GameDirectory)
                : found.Count > 0 ? found[0] : null;
            string? selectedPath;
            bool manuallySelected;
            lock (_gamePathLock)
            {
                manuallySelected = _userSelectedGamePath;
                if (!manuallySelected)
                {
                    _gamePath = detectedPath;
                    _gamePathRevision++;
                }
                selectedPath = _gamePath;
            }
            Log(manuallySelected ? "Keeping the game folder selected by the user."
                : selectedPath is null ? "Bop It! was not found. Use Browse to select its game folder."
                : $"Found Bop It! at {selectedPath}.");
            PublishState();
            if (!_launchedForUninstall)
            {
                try
                {
                    Log("Checking GitHub for the latest Bop It Access release.");
                    _release = await InstallerNetwork.LatestReleaseAsync(cancellation);
                    Log(_release is null
                        ? "No downloadable release is published yet. Install alpha is available from the latest source commit."
                        : $"Latest release: {_release.Tag} ({_release.AssetName}).");
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    Log("Could not check GitHub releases: " + ex.Message);
                }
            }
            _initialized = true;
        }
        finally
        {
            _initializing = false;
            PublishState();
        }
    }

    internal void SetGamePath(string path)
    {
        string? selectedPath = string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
        lock (_gamePathLock)
        {
            _gamePath = selectedPath;
            _gamePathRevision++;
            _userSelectedGamePath = true;
        }
        Log(GameLocator.IsGameDirectory(selectedPath)
            ? $"Verified Bop It! game folder: {selectedPath}."
            : "That folder does not contain BopIt!.exe and BopIt!_Data.");
        PublishState();
    }

    internal Task InstallReleaseAsync(CancellationToken cancellation) =>
        RunInstallAsync(alpha: false, cancellation);

    internal Task InstallAlphaAsync(CancellationToken cancellation) =>
        RunInstallAsync(alpha: true, cancellation);

    internal Task UpdateAsync(CancellationToken cancellation) =>
        RunInstallAsync(alpha: false, cancellation);

    internal void RequestAbort()
    {
        try { _abort?.Cancel(); }
        catch (ObjectDisposedException) { /* The operation finished before the click arrived. */ }
    }

    internal async Task UninstallAsync(CancellationToken cancellation)
    {
        var manifestPath = Path.Combine(_stateDirectory, InstallManifest.FileName);
        var manifest = File.Exists(manifestPath) ? InstallManifest.Load(manifestPath) : null;
        string? selected = GetGamePath();
        if (manifest != null && selected != null && !PathsEqual(selected, manifest.GameDirectory))
            throw new InvalidOperationException("The installed-file ledger belongs to another game folder. Select " + manifest.GameDirectory + " before uninstalling.");
        var game = manifest == null ? RequireGame() : Path.GetFullPath(manifest.GameDirectory);
        LastUninstallWarningCount = 0;
        await RunBusyAsync(async ct =>
        {
            if (!File.Exists(manifestPath))
            {
                Log("This mod installation predates the installer. Removing only known mod-owned files.");
                var legacyResult = await UninstallManager.UninstallLegacyAsync(game, Log, ct);
                LastUninstallWarningCount = legacyResult.PreferenceWarnings.Count;
                if (LastUninstallWarningCount > 0)
                    Log($"Preference cleanup reported {LastUninstallWarningCount} warning(s). Review the status log.");
                PublishState();
                return;
            }
            Log("Reading the installed-file ledger.");
            var result = await UninstallManager.UninstallAsync(manifestPath, Log, ct);
            if (!result.Success)
                throw new InvalidOperationException("Uninstall stopped because installed files or backups were changed. Review the conflicts in the status log; no files were removed.");
            LastUninstallWarningCount = result.PreferenceWarnings.Count;
            if (LastUninstallWarningCount > 0)
                Log($"Preference cleanup reported {LastUninstallWarningCount} warning(s). Review the status log.");
            Log(LastUninstallWarningCount == 0
                ? "Uninstall completed. The .NET SDK remains installed, as requested."
                : "Mod file removal finished, but preference cleanup is incomplete. Use Uninstall to retry. The .NET SDK remains installed.");
            if (LastUninstallWarningCount == 0) ScheduleStandaloneCleanup();
            PublishState();
        }, cancellation);
    }

    private async Task RunInstallAsync(bool alpha, CancellationToken cancellation)
    {
        var game = RequireGame();
        await RunBusyAsync(async ct =>
        {
            string existingManifest = Path.Combine(_stateDirectory, InstallManifest.FileName);
            var existing = File.Exists(existingManifest) ? InstallManifest.Load(existingManifest) : null;
            if (existing != null && !PathsEqual(existing.GameDirectory, game))
                throw new InvalidOperationException("The installer already manages another game folder: " + existing.GameDirectory);
            if (existing?.UninstallFilesRemoved == true)
                throw new InvalidOperationException("Finish the pending uninstall preference cleanup before installing again.");
            if (!alpha && _release is null)
                throw new InvalidOperationException("No GitHub release package exists yet. Use Install alpha until the first release is published.");
            if (IsGameRunning())
                throw new InvalidOperationException("Close Bop It! before installing or updating the mod.");

            var temp = Path.Combine(Path.GetTempPath(), "BopItAccessInstaller-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            bool addedSdk = false;
            bool committed = false;
            InstallTransaction? transaction = null;
            try
            {
                var (dotnet, sdkAdded) = await SdkAndBuild.EnsureSdkAsync(game, temp, Log, Progress, ct);
                addedSdk = sdkAdded;
                Log("Verifying MelonLoader 0.7.3 Open-Beta.");
                bool melonWasPresent = Directory.Exists(Path.Combine(game, "MelonLoader")) ||
                    File.Exists(Path.Combine(game, "version.dll"));
                string? melonRoot = null;
                if (!HasPinnedMelonLoader(game))
                {
                    var melonZip = Path.Combine(temp, "melonloader.zip");
                    await DownloadPinnedAsync(InstallerNetwork.MelonUrl, melonZip, InstallerNetwork.MelonSha256,
                        "MelonLoader", ct);
                    melonRoot = Path.Combine(temp, "melonloader");
                    SafeZip.Extract(melonZip, melonRoot,
                        (done, total) => Progress("Extracting MelonLoader", done, total), ct);
                    if (!File.Exists(Path.Combine(melonRoot, "version.dll")) ||
                        !File.Exists(Path.Combine(melonRoot, "MelonLoader", "net6", "MelonLoader.dll")))
                        throw new InvalidDataException("MelonLoader archive is missing required files.");
                }
                else Log("MelonLoader 0.7.3 Open-Beta is already installed.");

                Log("Verifying official Prism 0.18.3 for Windows x64.");
                string? prismDll = null;
                var installedPrism = Path.Combine(game, "prism.dll");
                var prismZip = Path.Combine(temp, "prism.zip");
                await DownloadPinnedAsync(InstallerNetwork.PrismUrl, prismZip, InstallerNetwork.PrismSha256,
                    "Prism", ct);
                var prismRoot = Path.Combine(temp, "prism");
                SafeZip.Extract(prismZip, prismRoot,
                    (done, total) => Progress("Extracting Prism", done, total), ct);
                var verifiedPrism = Path.Combine(prismRoot, "dynamic", "release", "bin", "prism.dll");
                if (!File.Exists(verifiedPrism)) throw new InvalidDataException("Prism archive has no release prism.dll.");
                InstallerNetwork.VerifyHash(verifiedPrism, InstallerNetwork.PrismDllSha256);
                if (!File.Exists(installedPrism) ||
                    !InstallerNetwork.Sha256(installedPrism).Equals(InstallerNetwork.PrismDllSha256, StringComparison.OrdinalIgnoreCase))
                    prismDll = verifiedPrism;
                else Log("Official Prism 0.18.3 is already installed; its license notices will still be installed.");

                string sourceRoot;
                string reference;
                if (alpha)
                {
                    Log("Checking the latest commit on the main branch.");
                    reference = await InstallerNetwork.LatestCommitAsync(ct);
                    Log("Latest source commit: " + reference);
                    var sourceZip = Path.Combine(temp, "source.zip");
                    await InstallerNetwork.DownloadAsync(
                        new Uri($"https://github.com/Chris-E-Shaw/BopItAccess/archive/{reference}.zip"), sourceZip,
                        (done, total) => Progress("Downloading latest source", done, total), ct);
                    var extracted = Path.Combine(temp, "source");
                    SafeZip.Extract(sourceZip, extracted,
                        (done, total) => Progress("Extracting latest source", done, total), ct);
                    sourceRoot = FindSourceRoot(extracted);
                }
                else
                {
                    reference = _release!.Tag;
                    var releaseZip = Path.Combine(temp, "release.zip");
                    Log("Downloading release package " + _release.AssetName + ".");
                    await InstallerNetwork.DownloadAsync(_release.AssetUrl, releaseZip,
                        (done, total) => Progress("Downloading release", done, total), ct);
                    var extracted = Path.Combine(temp, "release");
                    SafeZip.Extract(releaseZip, extracted,
                        (done, total) => Progress("Extracting release", done, total), ct);
                    sourceRoot = FindPackageRoot(extracted);
                }

                transaction = new InstallTransaction(game, _stateDirectory, Log);
                if (melonRoot is not null)
                {
                    Log("Installing MelonLoader files into the game folder.");
                    await InstallTreeAsync(transaction, melonRoot, game, ct);
                    transaction.MelonLoaderInstalledByInstaller |= !melonWasPresent;
                }
                if (prismDll is not null)
                {
                    Log("Installing verified Prism speech library.");
                    await transaction.InstallFileAsync(prismDll, installedPrism, ct);
                }

                // Set loader UI preferences before the first proxy-generation
                // launch, and use the same path for release and alpha installs.
                await LoaderUiDefaults.ApplyAsync(transaction, temp, Log, ct);

                string dll;
                if (alpha)
                {
                    await EnsureProxiesAsync(game, transaction, ct);
                    dll = await SdkAndBuild.BuildAsync(dotnet, sourceRoot, game, Log, Progress, ct);
                }
                else dll = FindReleaseDll(sourceRoot);

                var displayVersion = alpha
                    ? ReadModVersion(sourceRoot) + "-alpha." + reference[..7]
                    : reference.TrimStart('v', 'V');

                var mods = Path.Combine(game, "Mods");
                transaction.CreateDirectory(mods);
                Log("Installing BopItAccess.dll.");
                await transaction.InstallFileAsync(dll, Path.Combine(mods, "BopItAccess.dll"), ct);
                await InstallDocumentationAsync(transaction, sourceRoot, game, ct);
                var prismNotices = Path.Combine(game, "documentation", "THIRD-PARTY-LICENSES", "Prism");
                var noticeFile = Path.Combine(prismRoot, "NOTICE");
                var licenses = Path.Combine(prismRoot, "LICENSES");
                if (!File.Exists(noticeFile) || !Directory.Exists(licenses))
                    throw new InvalidDataException("The official Prism package is missing its license notices.");
                await transaction.InstallFileAsync(noticeFile, Path.Combine(prismNotices, "NOTICE"), ct);
                await InstallTreeAsync(transaction, licenses, Path.Combine(prismNotices, "LICENSES"), ct);
                Log("Installed Prism and bundled third-party license notices.");
                await InstallUninstallerAsync(transaction, temp, displayVersion, ct);
                ct.ThrowIfCancellationRequested();
                Log("Committing the installation ledger.");
                await transaction.CommitAsync(displayVersion, alpha ? "alpha" : "release", reference, ct);
                committed = true;
                Log("Bop It Access installation completed successfully.");
            }
            catch
            {
                if (transaction is not null && !committed)
                {
                    Log("Reversing changes made during this attempt.");
                    await CloseGameBeforeRollbackAsync();
                    await transaction.RollbackAsync();
                }
                if (addedSdk)
                {
                    var sdk = Path.Combine(game, "dotnet");
                    if (Directory.Exists(sdk))
                    {
                        Log("Removing the SDK added during the aborted installation.");
                        DeleteOwnedDirectory(sdk, game, "dotnet");
                    }
                }
                throw;
            }
            finally
            {
                try { DeleteOwnedDirectory(temp, Path.GetTempPath(), "BopItAccessInstaller-"); }
                catch (Exception ex) { Log("Temporary download cleanup needs attention: " + ex.Message); }
            }
            PublishState();
        }, cancellation);
    }

    private async Task RunBusyAsync(Func<CancellationToken, Task> body, CancellationToken cancellation)
    {
        if (!_initialized) throw new InvalidOperationException("Wait for the initial installation checks to finish before starting an operation.");
        if (Interlocked.CompareExchange(ref _operationActive, 1, 0) != 0)
            throw new InvalidOperationException("Another installer operation is already running.");
        _busy = true;
        _abort = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        PublishState();
        try { await body(_abort.Token); }
        finally
        {
            _abort.Dispose();
            _abort = null;
            _busy = false;
            Volatile.Write(ref _operationActive, 0);
            PublishState();
        }
    }

    private string RequireGame()
    {
        string? gamePath = GetGamePath();
        return GameLocator.IsGameDirectory(gamePath)
            ? Path.GetFullPath(gamePath!)
            : throw new InvalidOperationException("Select a valid Bop It! game folder first.");
    }

    private string? GetGamePath()
    {
        lock (_gamePathLock)
            return _gamePath;
    }

    private void PublishState()
    {
        string? gamePath;
        long pathRevision;
        lock (_gamePathLock)
        {
            gamePath = _gamePath;
            pathRevision = _gamePathRevision;
        }
        var valid = GameLocator.IsGameDirectory(gamePath);
        var manifest = ReadManifest();
        var installed = gamePath != null && ((manifest is not null &&
            PathsEqual(manifest.GameDirectory, gamePath)) ||
            (valid && File.Exists(Path.Combine(gamePath, "Mods", "BopItAccess.dll"))));
        var update = installed && manifest?.UninstallFilesRemoved != true && _release is not null &&
            !string.Equals(manifest?.SourceReference, _release.Tag, StringComparison.OrdinalIgnoreCase);
        string? message = _initializing ? "Checking the installation and Steam libraries."
            : !_initialized ? "Initial checks could not finish. Review the status log, then reopen the installer."
            : installed && manifest?.UninstallFilesRemoved == true ? "Mod files were removed. Uninstall can retry remaining preference cleanup."
            : !valid && installed ? "The base game is missing; Uninstall can still remove the mod and its settings."
            : !valid ? "Choose a valid Bop It! game folder."
            : _busy ? "Installer is working. Review the status log for each step."
            : _release is null ? "No GitHub release is published yet. Install alpha builds the latest source."
            : installed ? (update ? $"An update ({_release.Tag}) is available." : "Bop It Access is installed.")
            : $"Ready to install release {_release.Tag}.";
        StateChanged?.Invoke(new InstallerState(gamePath, valid, installed, _release is not null,
            update, _busy || _initializing, message, pathRevision, _initialized));
    }

    private InstallManifest? ReadManifest()
    {
        var path = Path.Combine(_stateDirectory, "install-manifest.json");
        if (!File.Exists(path)) return null;
        try { return InstallManifest.Load(path); }
        catch (Exception ex) { Log("Could not read installation ledger: " + ex.Message); return null; }
    }

    private void ScheduleStandaloneCleanup()
    {
        var current = Environment.ProcessPath;
        var stable = Path.Combine(_stateDirectory, "BopItAccess.Uninstaller.exe");
        if (_launchedForUninstall && string.Equals(current, stable, StringComparison.OrdinalIgnoreCase))
            return; // Apps & Features launcher waits for this process, then cleans the state folder.
        var script = Path.Combine(_stateDirectory, "uninstall.ps1");
        if (!File.Exists(script)) return;
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"WindowsPowerShell\v1.0\powershell.exe");
        var info = new ProcessStartInfo(powershell)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        foreach (var argument in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden",
                     "-File", script, "-CleanupOnly", "-ProcessId", Environment.ProcessId.ToString() })
            info.ArgumentList.Add(argument);
        using var cleanup = Process.Start(info) ?? throw new IOException("Could not start the final uninstall cleanup process.");
        Log("Scheduled cleanup of the installer launcher after this window closes.");
    }

    private void Log(string message) => StatusChanged?.Invoke(message);
    private void Progress(string step, long done, long? total)
    {
        long now = Environment.TickCount64;
        lock (_progressLock)
        {
            bool completed = total is >= 0 && done >= total.Value;
            if (_lastProgressStep == step && !completed && now - _lastProgressAt < 100) return;
            _lastProgressStep = step;
            _lastProgressAt = now;
        }
        ProgressChanged?.Invoke(new InstallerProgress(step, done, total));
    }

    private static bool IsGameRunning()
    {
        Process[] processes = Process.GetProcessesByName("BopIt!");
        try { return processes.Length > 0; }
        finally { foreach (Process process in processes) process.Dispose(); }
    }

    private static bool PathsEqual(string first, string second) => string.Equals(
        Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
        Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

    private async Task CloseGameBeforeRollbackAsync()
    {
        var running = Process.GetProcessesByName("BopIt!");
        if (running.Length == 0) return;
        Log("Asking the game to close before reversing files it may be using.");
        foreach (var process in running)
        {
            try { process.CloseMainWindow(); }
            catch { /* The player can close the game normally. */ }
            finally { process.Dispose(); }
        }
        var until = DateTimeOffset.UtcNow.AddSeconds(30);
        while (IsGameRunning() && DateTimeOffset.UtcNow < until)
            await Task.Delay(500);
        if (IsGameRunning())
            throw new IOException("Bop It! is still open. Close it and reopen the installer to finish reversing this attempt. The recovery journal is preserved.");
        Log("Game closed; rollback can continue.");
    }

    private static void DeleteOwnedDirectory(string target, string parent, string expectedNamePrefix)
    {
        var full = Path.GetFullPath(target).TrimEnd(Path.DirectorySeparatorChar);
        var root = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar);
        if (!string.Equals(Path.GetDirectoryName(full), root, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(full).StartsWith(expectedNamePrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Refusing to remove a directory outside its expected parent.");
        if (!Directory.Exists(full)) return;
        if ((File.GetAttributes(full) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidOperationException("Refusing to remove a linked directory.");
        Directory.Delete(full, recursive: true);
    }

    private static bool HasPinnedMelonLoader(string game)
    {
        var version = Path.Combine(game, "version.dll");
        var managed = Path.Combine(game, "MelonLoader", "net6", "MelonLoader.dll");
        return File.Exists(version) && File.Exists(managed) &&
            (FileVersionInfo.GetVersionInfo(managed).FileVersion ?? "").StartsWith("0.7.3", StringComparison.Ordinal) &&
            InstallerNetwork.Sha256(version).Equals("0ce7a4e530c7f83f172a2c44aed45eedb3bdcf06b83760f143a0fba6885fa929", StringComparison.OrdinalIgnoreCase);
    }

    private async Task DownloadPinnedAsync(string url, string path, string hash, string name, CancellationToken cancellation)
    {
        Log($"Downloading {name} from its official release.");
        await InstallerNetwork.DownloadAsync(new Uri(url), path,
            (done, total) => Progress("Downloading " + name, done, total), cancellation);
        Log($"Verifying {name} checksum.");
        InstallerNetwork.VerifyHash(path, hash);
    }

    private static string FindSourceRoot(string extracted)
    {
        var root = Directory.EnumerateDirectories(extracted).SingleOrDefault();
        if (root is null || !File.Exists(Path.Combine(root, "src", "BopItAccess.csproj")))
            throw new InvalidDataException("The source archive did not contain the expected project.");
        return root;
    }

    private static string FindPackageRoot(string extracted)
    {
        if (File.Exists(Path.Combine(extracted, "Mods", "BopItAccess.dll"))) return extracted;
        var root = Directory.EnumerateDirectories(extracted).SingleOrDefault();
        if (root is null || !File.Exists(Path.Combine(root, "Mods", "BopItAccess.dll")))
            throw new InvalidDataException("Release ZIP must contain Mods/BopItAccess.dll.");
        return root;
    }

    private static string FindReleaseDll(string root) => Path.Combine(root, "Mods", "BopItAccess.dll");

    private static string ReadModVersion(string root)
    {
        var source = Path.Combine(root, "src", "BopItAccessMod.cs");
        if (!File.Exists(source)) return "source";
        var match = Regex.Match(File.ReadAllText(source),
            "MelonInfo\\([^\\r\\n]*?,\\s*\"Bop It Access\"\\s*,\\s*\"(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)\"");
        return match.Success ? match.Groups["version"].Value : "source";
    }

    private async Task InstallTreeAsync(InstallTransaction transaction, string source, string destination,
        CancellationToken cancellation)
    {
        var files = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories).ToArray();
        for (int i = 0; i < files.Length; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(source, files[i]);
            await transaction.InstallFileAsync(files[i], Path.Combine(destination, relative), cancellation);
            Progress("Installing dependency files", i + 1, files.Length);
        }
    }

    private async Task InstallDocumentationAsync(InstallTransaction transaction, string root, string game,
        CancellationToken cancellation)
    {
        var target = Path.Combine(game, "documentation");
        transaction.CreateDirectory(target);
        var sourceDocs = Path.Combine(root, "documentation");
        if (Directory.Exists(sourceDocs))
            await InstallTreeAsync(transaction, sourceDocs, target, cancellation);
        foreach (var name in new[] { "BopItAccess-user-guide.html", "BopItAccess-build-history.html",
                     "README.md", "README.txt", "GIT-WORKFLOW.md", "THIRD-PARTY-NOTICES.txt" })
        {
            var file = Path.Combine(root, name);
            if (File.Exists(file)) await transaction.InstallFileAsync(file, Path.Combine(target, name), cancellation);
        }
        if (!File.Exists(Path.Combine(target, "BopItAccess-user-guide.html")))
            throw new InvalidDataException("The package has no English user guide.");
        Log("Installed game documentation and translated guides.");
    }

    private async Task InstallUninstallerAsync(InstallTransaction transaction, string temp,
        string reference, CancellationToken cancellation)
    {
        var executable = Environment.ProcessPath;
        if (string.IsNullOrEmpty(executable) || !File.Exists(executable))
            throw new InvalidOperationException("Could not locate the installer executable for Apps & Features registration.");
        var stableExe = Path.Combine(_stateDirectory, "BopItAccess.Uninstaller.exe");
        await transaction.InstallFileAsync(executable, stableExe, cancellation);
        var script = Path.Combine(temp, "uninstall.ps1");
        await using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BopItAccess.Installer.uninstall.ps1")
            ?? throw new InvalidOperationException("The uninstaller script is missing from the executable."))
        await using (var output = File.Create(script))
            await stream.CopyToAsync(output, cancellation);
        var stableScript = Path.Combine(_stateDirectory, "uninstall.ps1");
        await transaction.InstallFileAsync(script, stableScript, cancellation);
        transaction.RegisterUninstall(stableScript, reference);
        Log("Registered an Apps & Features uninstall entry using the supplied uninstaller script.");
    }

    private async Task EnsureProxiesAsync(string game, InstallTransaction transaction, CancellationToken cancellation)
    {
        string[] required = { "Assembly-CSharp.dll", "Il2Cppmscorlib.dll", "UnityEngine.CoreModule.dll",
            "Unity.Localization.dll", "UnityEngine.AudioModule.dll", "Il2CppFMODUnity.dll",
            "UnityEngine.UI.dll", "UnityEngine.UIModule.dll", "Unity.TextMeshPro.dll",
            "Unity.InputSystem.dll", "Il2CppDOTween.dll" };
        var directory = Path.Combine(game, "MelonLoader", "Il2CppAssemblies");
        bool ready() => required.All(name => File.Exists(Path.Combine(directory, name)) &&
            new FileInfo(Path.Combine(directory, name)).Length > 0);
        if (ready()) { Log("Verified all game-generated build proxy assemblies."); return; }
        transaction.TrackGeneratedTree(directory, cancellation);
        transaction.TrackGeneratedTree(Path.Combine(game, "MelonLoader", "Logs"), cancellation);

        Log("Launching Bop It! once so MelonLoader can generate build references from this copy of the game. If the game does not open, launch it through Steam.");
        Progress("Generating game build references", 0, null);
        var launchedAt = DateTime.UtcNow.AddSeconds(-2);
        var info = new ProcessStartInfo(Path.Combine(game, "BopIt!.exe"))
        {
            WorkingDirectory = game,
            UseShellExecute = true
        };
        Process? opened = null;
        try { opened = Process.Start(info); }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Log("Direct game launch did not work: " + ex.Message + " Launch Bop It! through Steam; the installer will keep waiting.");
        }
        using var launched = opened;
        var until = DateTimeOffset.UtcNow.AddMinutes(8);
        var previousSizes = string.Empty;
        int stableSeconds = 0;
        bool generationFinished()
        {
            var latest = Path.Combine(game, "MelonLoader", "Latest.log");
            if (!File.Exists(latest) || File.GetLastWriteTimeUtc(latest) < launchedAt) return false;
            try { return File.ReadAllText(latest).Contains("Loading Mods...", StringComparison.OrdinalIgnoreCase); }
            catch (IOException) { return false; }
        }
        while (true)
        {
            cancellation.ThrowIfCancellationRequested();
            if (ready())
            {
                var sizes = string.Join(",", required.Select(name =>
                    new FileInfo(Path.Combine(directory, name)).Length.ToString()));
                stableSeconds = sizes == previousSizes ? stableSeconds + 1 : 0;
                previousSizes = sizes;
                if (stableSeconds >= 3 && generationFinished()) break;
            }
            if (DateTimeOffset.UtcNow > until)
                throw new TimeoutException("MelonLoader did not finish generating the game's build references within eight minutes. Check MelonLoader/Latest.log and retry.");
            await Task.Delay(1000, cancellation);
        }
        Log("MelonLoader generated the required game build references.");
        try { launched?.CloseMainWindow(); } catch { }
        Log("Waiting for Bop It! to close before installing the mod. If the game remains open, please close it normally.");
        until = DateTimeOffset.UtcNow.AddMinutes(5);
        while (IsGameRunning())
        {
            cancellation.ThrowIfCancellationRequested();
            if (DateTimeOffset.UtcNow > until)
                throw new TimeoutException("The game is still open. Close it normally, then retry the installation.");
            await Task.Delay(1000, cancellation);
        }
        Log("Game closed; source build can continue.");
    }
}
