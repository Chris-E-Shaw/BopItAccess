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
    internal InstallerDiagnostics Diagnostics { get; }
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
    private InstallerState? _lastRecordedState;
    private CancellationTokenSource? _abort;
    internal int LastUninstallWarningCount { get; private set; }

    internal InstallerService(InstallerDiagnostics diagnostics, bool launchedForUninstall = false)
    {
        Diagnostics = diagnostics;
        _launchedForUninstall = launchedForUninstall;
    }

    internal async Task InitializeAsync(CancellationToken cancellation = default)
    {
        try
        {
            await InstallTransaction.RecoverInterruptedAsync(_stateDirectory, Log);
            cancellation.ThrowIfCancellationRequested();
            Log("Searching Steam libraries across available drives.");
            var found = await Task.Run(() => GameLocator.FindInstallations(cancellation), cancellation);
            foreach (string game in found) Diagnostics.ProtectGameDirectory(game);
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
                    Diagnostics.Error("GitHub release check failed", ex);
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
        if (GameLocator.IsGameDirectory(selectedPath))
        {
            Diagnostics.ProtectGameDirectory(selectedPath!);
            Diagnostics.EnsureExportOutsideGameDirectory(selectedPath!);
        }
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
        Diagnostics.EnsureExportOutsideGameDirectory(game);
        LastUninstallWarningCount = 0;
        await RunBusyAsync("Uninstall", async ct =>
        {
            if (!File.Exists(manifestPath))
            {
                Log("This mod installation predates the installer. Removing only known mod-owned files.");
                var legacyResult = await UninstallManager.UninstallLegacyAsync(game, Log, ct);
                LastUninstallWarningCount = legacyResult.PreferenceWarnings.Count;
                if (LastUninstallWarningCount == 0)
                {
                    try { Diagnostics.RemoveAfterLegacyUninstall(); }
                    catch (Exception ex)
                    {
                        LastUninstallWarningCount++;
                        Diagnostics.Error("Legacy diagnostic cleanup failed", ex);
                        Log("Automatic diagnostic files could not be removed. Use Uninstall to retry: " + ex.Message);
                    }
                }
                if (LastUninstallWarningCount > 0)
                    Log($"Cleanup reported {LastUninstallWarningCount} warning(s). Review the status log.");
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
                ? "Uninstall completed. Existing shared .NET installations and SDKs remain untouched."
                : "Mod file removal finished, but preference cleanup is incomplete. Use Uninstall to retry. Existing shared .NET installations and SDKs remain untouched.");
            if (LastUninstallWarningCount == 0) ScheduleStandaloneCleanup();
            PublishState();
        }, cancellation);
    }

    private async Task RunInstallAsync(bool alpha, CancellationToken cancellation)
    {
        var game = RequireGame();
        await RunBusyAsync(alpha ? "Install alpha" : "Install release or update", async ct =>
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
            bool committed = false;
            InstallTransaction? transaction = null;
            try
            {
                Log("Preparing a single-pass installation. Bop It! will not be launched by the installer.");
                var dependencies = await SdkAndBuild.EnsureDependenciesAsync(alpha, game, temp, Log, Progress, ct);
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

                string dll;
                if (alpha)
                {
                    var references = await OfflineBuildReferences.PrepareAsync(dependencies.Dotnet!, game,
                        melonRoot ?? game, temp, Log, Progress, ct);
                    dll = await SdkAndBuild.BuildAsync(dependencies.Dotnet!, sourceRoot, references, Log, Progress, ct);
                }
                else dll = FindReleaseDll(sourceRoot);

                var displayVersion = alpha
                    ? ReadModVersion(sourceRoot) + "-alpha." + reference[..7]
                    : reference.TrimStart('v', 'V');

                ct.ThrowIfCancellationRequested();
                if (IsGameRunning())
                    throw new InvalidOperationException("Bop It! was opened during preparation. Close it before installing. The installer has not changed the game files.");
                transaction = new InstallTransaction(game, _stateDirectory, Log);
                if (melonRoot is not null)
                {
                    Log("Installing MelonLoader files into the game folder.");
                    await InstallTreeAsync(transaction, melonRoot, game, ct);
                    transaction.MelonLoaderInstalledByInstaller |= !melonWasPresent;
                }
                var mods = Path.Combine(game, "Mods");
                transaction.CreateDirectory(mods);
                Log("Installing BopItAccess.dll immediately after MelonLoader.");
                await transaction.InstallFileAsync(dll, Path.Combine(mods, "BopItAccess.dll"), ct);

                if (dependencies.RuntimeSource is not null)
                {
                    Log("Installing the required runtime in MelonLoader's Dependencies folder.");
                    await InstallTreeAsync(transaction, dependencies.RuntimeSource,
                        Path.Combine(game, "MelonLoader", "Dependencies", "dotnet"), ct);
                }

                if (prismDll is not null)
                {
                    Log("Installing verified Prism speech library.");
                    await transaction.InstallFileAsync(prismDll, installedPrism, ct);
                }

                // Prepare loader UI preferences for the player's first launch.
                await LoaderUiDefaults.ApplyAsync(transaction, temp, Log, ct);
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
                Log("Bop It Access installation completed successfully. Launch Bop It! manually when ready. On its first launch, MelonLoader may download tools and generate assemblies for a minute or longer before mod speech starts.");
            }
            catch (Exception ex)
            {
                Diagnostics.Error("Installation attempt failed before rollback", ex);
                if (transaction is not null && !committed)
                {
                    Log("Reversing changes made during this attempt.");
                    await WaitForGameBeforeRollbackAsync();
                    await transaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                try { DeleteOwnedDirectory(temp, Path.GetTempPath(), "BopItAccessInstaller-"); }
                catch (Exception ex)
                {
                    Diagnostics.Error("Temporary download cleanup failed", ex);
                    Log("Temporary download cleanup needs attention: " + ex.Message);
                }
            }
            PublishState();
        }, cancellation);
    }

    private async Task RunBusyAsync(string operation, Func<CancellationToken, Task> body, CancellationToken cancellation)
    {
        if (!_initialized) throw new InvalidOperationException("Wait for the initial installation checks to finish before starting an operation.");
        if (Interlocked.CompareExchange(ref _operationActive, 1, 0) != 0)
            throw new InvalidOperationException("Another installer operation is already running.");
        _busy = true;
        long started = Environment.TickCount64;
        Diagnostics.Write("OPERATION", "Started " + operation + ".");
        _abort = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        PublishState();
        try
        {
            await body(_abort.Token);
            Diagnostics.Write("OPERATION", "Completed " + operation + ".");
        }
        catch (OperationCanceledException)
        {
            Diagnostics.Write("OPERATION", "Canceled " + operation + ".");
            throw;
        }
        catch (Exception ex)
        {
            Diagnostics.Error(operation + " failed", ex);
            throw;
        }
        finally
        {
            Diagnostics.Write("OPERATION", operation + " duration: " +
                (Environment.TickCount64 - started) + " ms.");
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
        if (!GameLocator.IsGameDirectory(gamePath))
            throw new InvalidOperationException("Select a valid Bop It! game folder first.");
        Diagnostics.ProtectGameDirectory(gamePath!);
        Diagnostics.EnsureExportOutsideGameDirectory(gamePath!);
        return Path.GetFullPath(gamePath!);
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
        var state = new InstallerState(gamePath, valid, installed, _release is not null,
            update, _busy || _initializing, message, pathRevision, _initialized);
        if (Interlocked.Exchange(ref _lastRecordedState, state) != state)
            Diagnostics.Write("STATE", $"Path revision {pathRevision}; game={gamePath ?? "not selected"}; " +
                $"valid={valid}; installed={installed}; release={state.ReleaseAvailable}; " +
                $"update={update}; busy={state.Busy}; ready={_initialized}; {message}");
        StateChanged?.Invoke(state);
    }

    private InstallManifest? ReadManifest()
    {
        var path = Path.Combine(_stateDirectory, "install-manifest.json");
        if (!File.Exists(path)) return null;
        try
        {
            var manifest = InstallManifest.Load(path);
            Diagnostics.ProtectGameDirectory(manifest.GameDirectory);
            return manifest;
        }
        catch (Exception ex)
        {
            Diagnostics.Error("Installation ledger could not be read", ex);
            Log("Could not read installation ledger: " + ex.Message);
            return null;
        }
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

    private void Log(string message)
    {
        Diagnostics.Write("STATUS", message);
        StatusChanged?.Invoke(message);
    }
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
        Diagnostics.Progress(step, done, total);
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

    private async Task WaitForGameBeforeRollbackAsync()
    {
        var running = Process.GetProcessesByName("BopIt!");
        if (running.Length == 0) return;
        Log("Bop It! was opened independently. Close it to allow installation changes to be reversed; the installer will not close your game.");
        foreach (var process in running)
        {
            process.Dispose();
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
        string[] libraries = { "0Harmony", "Il2CppInterop.Runtime", "Il2CppInterop.Generator",
            "Il2CppInterop.Common", "Il2CppInterop.HarmonySupport", "MelonLoader.NativeHost",
            "AsmResolver", "AsmResolver.DotNet", "AsmResolver.PE", "AsmResolver.PE.File",
            "Microsoft.Extensions.Logging.Abstractions", "System.Diagnostics.DiagnosticSource" };
        return File.Exists(version) && File.Exists(managed) &&
            File.Exists(Path.Combine(game, "MelonLoader", "net6", "MelonLoader.runtimeconfig.json")) &&
            libraries.All(name => File.Exists(Path.Combine(game, "MelonLoader", "net6", name + ".dll"))) &&
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

}
