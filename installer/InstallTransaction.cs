using Microsoft.Win32;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;

namespace BopItAccess.Installer;

/// <summary>
/// Installs files through a durable, reversible journal. Call CommitAsync only
/// after every prerequisite and mod file has been installed successfully.
/// </summary>
public sealed class InstallTransaction
{
    public const string UninstallSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\BopItAccess";
    private static readonly EnumerationOptions SafeRecursiveEnumeration = new()
    {
        RecurseSubdirectories = true,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

    private readonly string _gameDirectory;
    private readonly string _stateDirectory;
    private readonly string _journalPath;
    private readonly string _transactionDirectory;
    private readonly Action<string> _log;
    private readonly InstallManifest? _previous;
    private readonly Dictionary<string, InstalledFile> _files;
    private readonly HashSet<string> _createdDirectories;
    private readonly TransactionJournal _journal;
    private readonly bool _modLogExistedBeforeInstall;
    private readonly bool _legacyModWasPresent;
    private bool _finished;

    public string GameDirectory => _gameDirectory;
    public string StateDirectory => _stateDirectory;
    public string ManifestPath => Path.Combine(_stateDirectory, InstallManifest.FileName);
    public bool MelonLoaderInstalledByInstaller { get; set; }

    public InstallTransaction(string gameDirectory, string stateDirectory, Action<string> log)
    {
        _gameDirectory = Path.GetFullPath(gameDirectory);
        _stateDirectory = Path.GetFullPath(stateDirectory);
        _log = log ?? throw new ArgumentNullException(nameof(log));
        if (!Directory.Exists(_gameDirectory))
            throw new DirectoryNotFoundException($"Game directory not found: {_gameDirectory}");
        string expectedState = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        if (!PathsEqual(_stateDirectory, expectedState))
            throw new ArgumentException("Installer state must be in the dedicated ProgramData folder.");
        if (IsWithin(_stateDirectory, _gameDirectory) || IsWithin(_gameDirectory, _stateDirectory))
            throw new ArgumentException("Installer state must be outside the game directory.");
        bool stateExisted = Directory.Exists(_stateDirectory);
        if (stateExisted) VerifyTrustedStateDirectory(_stateDirectory);

        _journalPath = Path.Combine(_stateDirectory, "transaction.json");
        if (File.Exists(_journalPath))
            throw new InvalidOperationException("An interrupted installation exists. Recover it before starting another.");
        _previous = File.Exists(ManifestPath) ? InstallManifest.Load(ManifestPath) : null;
        if (_previous is not null && !PathsEqual(_previous.GameDirectory, _gameDirectory))
            throw new InvalidOperationException("The existing installation manifest belongs to another game directory.");
        _files = new Dictionary<string, InstalledFile>(StringComparer.OrdinalIgnoreCase);
        if (_previous is not null)
            foreach (InstalledFile file in _previous.Files)
                _files.Add(Path.GetFullPath(file.Path), file);
        _createdDirectories = new HashSet<string>(
            (IEnumerable<string>?)_previous?.CreatedDirectories ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        MelonLoaderInstalledByInstaller = _previous?.MelonLoaderInstalledByInstaller ?? false;
        _legacyModWasPresent = _previous is null &&
            File.Exists(Path.Combine(_gameDirectory, "Mods", "BopItAccess.dll"));
        _modLogExistedBeforeInstall = _previous?.ModLogExistedBeforeInstall ??
            (!_legacyModWasPresent && File.Exists(Path.Combine(_gameDirectory, "Mods", "BopItAccess.log")));
        if (_legacyModWasPresent) AdoptLegacyFiles();

        Directory.CreateDirectory(_stateDirectory);
        if ((File.GetAttributes(_stateDirectory) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The installer state directory is a reparse point.");
        if (!stateExisted) VerifyNewStateOwner(_stateDirectory);
        HardenStateDirectoryAcl(_stateDirectory);
        _journal = new TransactionJournal
        {
            Id = Guid.NewGuid().ToString("N"),
            GameDirectory = _gameDirectory,
            StateDirectory = _stateDirectory
        };
        _transactionDirectory = Path.Combine(_stateDirectory, "transactions", _journal.Id);
        Directory.CreateDirectory(_transactionDirectory);
        SaveJournal();
    }

    /// <summary>
    /// Record every newly created directory. Existing directories remain untouched
    /// on rollback and uninstall.
    /// </summary>
    public void CreateDirectory(string path)
    {
        ThrowIfFinished();
        if (PathsEqual(path, _gameDirectory) || PathsEqual(path, _stateDirectory)) return;
        string full = ValidateDestination(path);
        if (Directory.Exists(full)) return;

        var missing = new Stack<string>();
        for (string? current = full; current is not null && !Directory.Exists(current);
             current = Path.GetDirectoryName(current))
        {
            ValidateDestination(current);
            missing.Push(current);
        }
        while (missing.Count > 0)
        {
            string directory = missing.Pop();
            AddAction(new JournalAction { Kind = "directory", Path = directory });
            Directory.CreateDirectory(directory);
            _createdDirectories.Add(directory);
            _log($"Created folder: {directory}");
        }
    }

    /// <summary>
    /// Snapshot a game subdirectory immediately before MelonLoader starts the
    /// game and generates build proxies or logs. Newly generated files can then
    /// be removed during abort or crash recovery without deleting prior files.
    /// </summary>
    public void TrackGeneratedTree(string path)
    {
        ThrowIfFinished();
        string full = ValidateDestination(path);
        string proxies = Path.Combine(_gameDirectory, "MelonLoader", "Il2CppAssemblies");
        string logs = Path.Combine(_gameDirectory, "MelonLoader", "Logs");
        if (!PathsEqual(full, proxies) && !PathsEqual(full, logs))
            throw new ArgumentException("Only MelonLoader's generated proxy and log directories can be tracked.");
        bool existed = Directory.Exists(full);
        var files = existed
            ? Directory.EnumerateFiles(full, "*", SafeRecursiveEnumeration)
                .Select(Path.GetFullPath).ToList()
            : new List<string>();
        AddAction(new JournalAction
        {
            Kind = "generated-tree",
            Path = full,
            Existed = existed,
            ExistingFiles = files
        });
        _log($"Recorded existing files before game-generated build references: {full}");
    }

    /// <summary>
    /// Copy an installer-owned source file into the game or installer state.
    /// Existing files are backed up before replacement. A prior unmanaged file is
    /// preserved as the baseline to restore on uninstall.
    /// </summary>
    public async Task InstallFileAsync(string sourceAbsolute, string destinationAbsolute,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFinished();
        string source = Path.GetFullPath(sourceAbsolute);
        string destination = ValidateDestination(destinationAbsolute);
        if (!File.Exists(source)) throw new FileNotFoundException("Install source not found.", source);
        if (PathsEqual(source, destination))
        {
            // The installed Apps & Features EXE can also be opened directly.
            // Windows cannot replace its running image with itself; it is
            // already the desired source file in this case.
            string hash = await Sha256Async(source, cancellationToken);
            _files.TryGetValue(destination, out InstalledFile? current);
            _files[destination] = new InstalledFile
            {
                Path = destination,
                Sha256 = hash,
                OriginalBackupPath = current?.OriginalBackupPath,
                OriginalSha256 = current?.OriginalSha256
            };
            _log($"Installer file is already in place: {destination}");
            return;
        }
        CreateDirectory(Path.GetDirectoryName(destination)!);
        cancellationToken.ThrowIfCancellationRequested();

        bool existed = File.Exists(destination);
        bool adoptLegacy = existed && _legacyModWasPresent &&
            await ShouldAdoptLegacyFileAsync(destination, cancellationToken);
        string sourceHash = await Sha256Async(source, cancellationToken);
        string? rollbackBackup = null;
        string? originalBackup = null;
        string? originalHash = null;
        try
        {
            if (existed)
            {
                rollbackBackup = Path.Combine(_transactionDirectory, Guid.NewGuid().ToString("N") + ".bak");
                File.Copy(destination, rollbackBackup);
                if (!_files.ContainsKey(destination) && !adoptLegacy)
                {
                    string originals = Path.Combine(_stateDirectory, "originals");
                    Directory.CreateDirectory(originals);
                    originalBackup = Path.Combine(originals, Guid.NewGuid().ToString("N") + ".bak");
                    File.Copy(destination, originalBackup);
                    originalHash = await Sha256Async(originalBackup, CancellationToken.None);
                }
            }
        }
        catch
        {
            if (rollbackBackup is not null && File.Exists(rollbackBackup)) File.Delete(rollbackBackup);
            if (originalBackup is not null && File.Exists(originalBackup)) File.Delete(originalBackup);
            throw;
        }

        var action = new JournalAction
        {
            Kind = "file",
            Path = destination,
            Existed = existed,
            RollbackBackupPath = rollbackBackup,
            NewOriginalBackupPath = originalBackup
        };
        try
        {
            AddAction(action);
        }
        catch
        {
            if (rollbackBackup is not null && File.Exists(rollbackBackup)) File.Delete(rollbackBackup);
            if (originalBackup is not null && File.Exists(originalBackup)) File.Delete(originalBackup);
            throw;
        }
        string stage = destination + ".bopitaccess-" + _journal.Id + ".tmp";
        try
        {
            await using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read))
            await using (var output = new FileStream(stage, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await input.CopyToAsync(output, cancellationToken);
                await output.FlushAsync(cancellationToken);
                output.Flush(flushToDisk: true);
            }
            if (!string.Equals(sourceHash, await Sha256Async(stage, cancellationToken), StringComparison.OrdinalIgnoreCase))
                throw new IOException($"The staged copy failed integrity validation: {destination}");
            File.Move(stage, destination, overwrite: true);
            _files.TryGetValue(destination, out InstalledFile? prior);
            _files[destination] = new InstalledFile
            {
                Path = destination,
                Sha256 = sourceHash,
                OriginalBackupPath = prior?.OriginalBackupPath ?? originalBackup,
                OriginalSha256 = prior?.OriginalSha256 ?? originalHash
            };
            _log($"Installed file: {destination}");
        }
        finally
        {
            if (File.Exists(stage)) File.Delete(stage);
        }
    }

    /// <summary>
    /// Remove a file owned by an earlier installer version during an update.
    /// The original pre-installer file, if any, is restored in its place.
    /// </summary>
    public async Task RemoveInstalledFileAsync(string destinationAbsolute,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFinished();
        string destination = ValidateDestination(destinationAbsolute);
        if (!_files.TryGetValue(destination, out InstalledFile? old))
            throw new InvalidOperationException($"The installer does not own {destination}.");
        if (File.Exists(destination) &&
            !string.Equals(await Sha256Async(destination, cancellationToken), old.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new IOException($"An installed file was modified outside the installer: {destination}");

        bool existed = File.Exists(destination);
        string? rollbackBackup = null;
        if (existed)
        {
            rollbackBackup = Path.Combine(_transactionDirectory, Guid.NewGuid().ToString("N") + ".bak");
            File.Copy(destination, rollbackBackup);
        }
        AddAction(new JournalAction
        {
            Kind = "file",
            Path = destination,
            Existed = existed,
            RollbackBackupPath = rollbackBackup
        });
        if (old.OriginalBackupPath is not null)
        {
            string original = ValidateBackup(old.OriginalBackupPath, _stateDirectory);
            File.Copy(original, destination, overwrite: true);
        }
        else if (existed)
            File.Delete(destination);
        _files.Remove(destination);
        _log($"Removed obsolete installer file: {destination}");
    }

    /// <summary>
    /// Register the stable uninstaller script in Windows Installed Apps.
    /// The script launches the installer UI in uninstall mode, then removes
    /// its own files after that process exits successfully.
    /// </summary>
    public void RegisterUninstall(string uninstallerScriptPath, string displayVersion)
    {
        ThrowIfFinished();
        string script = ValidateDestination(uninstallerScriptPath);
        if (!File.Exists(script)) throw new FileNotFoundException("Uninstall script not installed.", script);
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using RegistryKey? prior = baseKey.OpenSubKey(UninstallSubKey, writable: false);
        var action = new JournalAction
        {
            Kind = "registry",
            Path = UninstallSubKey,
            Existed = prior is not null,
            RegistryValues = prior is null ? new List<RegistryValueBackup>() :
                prior.GetValueNames().Select(name => RegistryValueBackup.Capture(prior, name)).ToList()
        };
        AddAction(action);
        using RegistryKey key = baseKey.CreateSubKey(UninstallSubKey, writable: true)
            ?? throw new IOException("Could not create the Windows uninstall entry.");
        string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            @"WindowsPowerShell\v1.0\powershell.exe");
        string command = $"\"{powershell}\" -NoProfile -ExecutionPolicy Bypass -File \"{script}\"";
        key.SetValue("DisplayName", "Bop It Access", RegistryValueKind.String);
        key.SetValue("DisplayVersion", displayVersion, RegistryValueKind.String);
        key.SetValue("Publisher", "Christopher Shaw", RegistryValueKind.String);
        key.SetValue("InstallLocation", _gameDirectory, RegistryValueKind.String);
        key.SetValue("UninstallString", command, RegistryValueKind.String);
        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        _log("Registered Bop It Access in Windows Installed Apps.");
    }

    public Task<InstallManifest> CommitAsync(string version, string channel, string sourceRef,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFinished();
        cancellationToken.ThrowIfCancellationRequested();
        var generatedFiles = new HashSet<string>(
            (_previous?.GeneratedMelonLoaderFiles as IEnumerable<string>) ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);
        foreach (JournalAction action in _journal.Actions.Where(action => action.Kind == "generated-tree"))
        {
            if (!Directory.Exists(action.Path)) continue;
            var previousFiles = new HashSet<string>(action.ExistingFiles, StringComparer.OrdinalIgnoreCase);
            foreach (string file in Directory.EnumerateFiles(action.Path, "*", SafeRecursiveEnumeration))
                if (!previousFiles.Contains(Path.GetFullPath(file))) generatedFiles.Add(Path.GetFullPath(file));
        }
        var manifest = new InstallManifest
        {
            GameDirectory = _gameDirectory,
            StateDirectory = _stateDirectory,
            Version = version,
            Channel = channel,
            SourceReference = sourceRef,
            TransactionId = _journal.Id,
            InstalledAtUtc = DateTimeOffset.UtcNow,
            Files = _files.Values.OrderBy(file => file.Path, StringComparer.OrdinalIgnoreCase).ToList(),
            GeneratedMelonLoaderFiles = generatedFiles.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList(),
            CreatedDirectories = _createdDirectories.OrderByDescending(path => path.Length).ToList(),
            MelonLoaderInstalledByInstaller = MelonLoaderInstalledByInstaller,
            ModLogExistedBeforeInstall = _modLogExistedBeforeInstall,
            UninstallRegistryKey = UninstallSubKey
        };
        InstallManifest.SaveAtomic(manifest, ManifestPath);
        _finished = true;
        try { CleanupTransactionFiles(_journalPath, _transactionDirectory); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The manifest's transaction ID lets startup recovery identify
            // this as committed and finish journal cleanup next launch.
            _log("Installation committed; temporary journal cleanup will retry next launch: " + ex.Message);
        }
        _log($"Committed Bop It Access {version} ({channel}).");
        return Task.FromResult(manifest);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // Rollback must finish even if the operation's CancellationToken was cancelled.
        if (_finished) return Task.CompletedTask;
        RollbackJournal(_journal, _log);
        CleanupTransactionFiles(_journalPath, _transactionDirectory);
        _finished = true;
        _log("Reversed installer changes made in this attempt.");
        return Task.CompletedTask;
    }

    /// <summary>Reverse an interrupted install found on the next launch.</summary>
    public static Task RecoverInterruptedAsync(string stateDirectory, Action<string> log)
    {
        string state = Path.GetFullPath(stateDirectory);
        string expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        if (!PathsEqual(state, expected))
            throw new ArgumentException("Unexpected installer state directory.");
        if (Directory.Exists(state))
        {
            VerifyTrustedStateDirectory(state);
            HardenStateDirectoryAcl(state);
        }
        string journalPath = Path.Combine(state, "transaction.json");
        if (!File.Exists(journalPath)) return Task.CompletedTask;
        TransactionJournal journal = ReadJournal(journalPath);
        if (!PathsEqual(journal.StateDirectory, state))
            throw new InvalidDataException("The installer journal refers to another state directory.");
        string manifestPath = Path.Combine(state, InstallManifest.FileName);
        if (File.Exists(manifestPath) && InstallManifest.Load(manifestPath).TransactionId == journal.Id)
            log("Found a committed installation; removing its stale transaction journal.");
        else
        {
            RollbackJournal(journal, log);
            log("Recovered and reversed an interrupted installation.");
        }
        CleanupTransactionFiles(journalPath, Path.Combine(state, "transactions", journal.Id));
        return Task.CompletedTask;
    }

    private void AddAction(JournalAction action)
    {
        _journal.Actions.Add(action);
        SaveJournal();
    }

    private void SaveJournal()
    {
        string temp = _journalPath + ".new";
        using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            JsonSerializer.Serialize(stream, _journal, InstallManifest.JsonOptions);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, _journalPath, overwrite: true);
    }

    private static TransactionJournal ReadJournal(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<TransactionJournal>(stream, InstallManifest.JsonOptions)
            ?? throw new InvalidDataException("The installer transaction journal is empty.");
    }

    private static void RollbackJournal(TransactionJournal journal, Action<string> log)
    {
        foreach (JournalAction action in journal.Actions.AsEnumerable().Reverse())
        {
            if (action.Kind == "file")
            {
                string destination = ValidateAgainstRoots(action.Path, journal.GameDirectory, journal.StateDirectory);
                if (action.Existed)
                {
                    string backup = ValidateBackup(action.RollbackBackupPath, journal.StateDirectory);
                    if (!File.Exists(backup)) throw new IOException($"Rollback backup missing: {backup}");
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    File.Copy(backup, destination, overwrite: true);
                }
                else if (File.Exists(destination))
                    File.Delete(destination);
                if (action.NewOriginalBackupPath is not null)
                {
                    string original = ValidateBackup(action.NewOriginalBackupPath, journal.StateDirectory);
                    if (File.Exists(original)) File.Delete(original);
                }
                log($"Reversed file change: {destination}");
            }
            else if (action.Kind == "directory")
            {
                string directory = ValidateAgainstRoots(action.Path, journal.GameDirectory, journal.StateDirectory);
                if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            else if (action.Kind == "generated-tree")
            {
                string directory = ValidateAgainstRoots(action.Path, journal.GameDirectory, journal.StateDirectory);
                string proxies = Path.Combine(journal.GameDirectory, "MelonLoader", "Il2CppAssemblies");
                string logs = Path.Combine(journal.GameDirectory, "MelonLoader", "Logs");
                if (!PathsEqual(directory, proxies) && !PathsEqual(directory, logs))
                    throw new InvalidDataException("Unexpected generated-file journal path.");
                if (!Directory.Exists(directory)) continue;
                var previous = new HashSet<string>(action.ExistingFiles.Select(path =>
                    ValidateAgainstRoots(path, journal.GameDirectory, journal.StateDirectory)),
                    StringComparer.OrdinalIgnoreCase);
                foreach (string file in Directory.EnumerateFiles(directory, "*", SafeRecursiveEnumeration))
                {
                    if (previous.Contains(Path.GetFullPath(file))) continue;
                    File.Delete(file);
                    log($"Removed file generated during aborted installation: {file}");
                }
                foreach (string child in Directory.EnumerateDirectories(directory, "*", SafeRecursiveEnumeration)
                             .OrderByDescending(path => path.Length))
                    if (!Directory.EnumerateFileSystemEntries(child).Any()) Directory.Delete(child);
                if (!action.Existed && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            else if (action.Kind == "registry")
                RestoreRegistry(action);
            else
                throw new InvalidDataException($"Unknown transaction action: {action.Kind}");
        }
    }

    private static void RestoreRegistry(JournalAction action)
    {
        if (action.Path != UninstallSubKey)
            throw new InvalidDataException("Unexpected registry path in installer journal.");
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        if (!action.Existed)
        {
            baseKey.DeleteSubKeyTree(UninstallSubKey, throwOnMissingSubKey: false);
            return;
        }
        using RegistryKey key = baseKey.CreateSubKey(UninstallSubKey, writable: true)
            ?? throw new IOException("Could not restore the previous Windows uninstall entry.");
        foreach (string name in key.GetValueNames()) key.DeleteValue(name, throwOnMissingValue: false);
        foreach (RegistryValueBackup value in action.RegistryValues)
            key.SetValue(value.Name, value.RestoreValue(), value.Kind);
    }

    private static void CleanupTransactionFiles(string journalPath, string transactionDirectory)
    {
        if (Directory.Exists(transactionDirectory)) Directory.Delete(transactionDirectory, recursive: true);
        string? transactions = Path.GetDirectoryName(transactionDirectory);
        if (transactions is not null && Directory.Exists(transactions) &&
            !Directory.EnumerateFileSystemEntries(transactions).Any())
            Directory.Delete(transactions);
        if (File.Exists(journalPath)) File.Delete(journalPath);
        CleanupUnusedOriginals(Path.GetDirectoryName(journalPath)!);
    }

    private static void CleanupUnusedOriginals(string stateDirectory)
    {
        string originals = Path.Combine(stateDirectory, "originals");
        if (!Directory.Exists(originals)) return;
        string manifestPath = Path.Combine(stateDirectory, InstallManifest.FileName);
        var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(manifestPath))
        {
            // Never discard a baseline backup if the ownership ledger cannot
            // be read; that would make a future uninstall irreversible.
            InstallManifest manifest;
            try { manifest = InstallManifest.Load(manifestPath); }
            catch { return; }
            foreach (InstalledFile file in manifest.Files)
                if (file.OriginalBackupPath is not null)
                    referenced.Add(Path.GetFullPath(file.OriginalBackupPath));
        }
        foreach (string backup in Directory.EnumerateFiles(originals, "*.bak", SearchOption.TopDirectoryOnly))
            if (!referenced.Contains(Path.GetFullPath(backup))) File.Delete(backup);
        if (!Directory.EnumerateFileSystemEntries(originals).Any()) Directory.Delete(originals);
    }

    private static void HardenStateDirectoryAcl(string directory)
    {
        // ProgramData permits ordinary users to create child directories. The
        // elevated Apps & Features launcher must not reside in a directory a
        // different user can edit.
        var acl = new DirectorySecurity();
        acl.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
        acl.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
        const InheritanceFlags inheritance = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
        acl.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
            FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
        acl.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
            FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow));
        acl.AddAccessRule(new FileSystemAccessRule(
            new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
            FileSystemRights.ReadAndExecute, inheritance, PropagationFlags.None, AccessControlType.Allow));
        new DirectoryInfo(directory).SetAccessControl(acl);
    }

    private static void VerifyTrustedStateDirectory(string directory)
    {
        if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("The installer state directory is a reparse point.");
        DirectorySecurity acl = new DirectoryInfo(directory).GetAccessControl(
            AccessControlSections.Access | AccessControlSections.Owner);
        var owner = acl.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier
            ?? throw new InvalidDataException("The installer state directory has no readable owner.");
        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        var system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        if (!owner.Equals(administrators) && !owner.Equals(system))
            throw new InvalidDataException(
                "The existing ProgramData installer folder is not owned by Administrators or SYSTEM. " +
                "An administrator must inspect and remove it before installation can continue.");

        const FileSystemRights writing = FileSystemRights.WriteData | FileSystemRights.AppendData |
            FileSystemRights.WriteAttributes | FileSystemRights.WriteExtendedAttributes |
            FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles |
            FileSystemRights.ChangePermissions | FileSystemRights.TakeOwnership;
        foreach (FileSystemAccessRule rule in acl.GetAccessRules(true, true, typeof(SecurityIdentifier)))
        {
            if (rule.AccessControlType != AccessControlType.Allow || (rule.FileSystemRights & writing) == 0)
                continue;
            var principal = (SecurityIdentifier)rule.IdentityReference;
            if (!principal.Equals(administrators) && !principal.Equals(system))
                throw new InvalidDataException(
                    "The existing ProgramData installer folder is writable by an untrusted account. " +
                    "An administrator must inspect and remove it before installation can continue.");
        }
    }

    private static void VerifyNewStateOwner(string directory)
    {
        DirectorySecurity acl = new DirectoryInfo(directory).GetAccessControl(AccessControlSections.Owner);
        var owner = acl.GetOwner(typeof(SecurityIdentifier)) as SecurityIdentifier
            ?? throw new InvalidDataException("The new installer state directory has no readable owner.");
        var current = WindowsIdentity.GetCurrent().User
            ?? throw new InvalidDataException("The current Windows identity has no readable user SID.");
        var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        var system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        if (!owner.Equals(current) && !owner.Equals(administrators) && !owner.Equals(system))
            throw new InvalidDataException("Another account created the installer state folder before it could be secured.");
    }

    private string ValidateDestination(string path) =>
        ValidateAgainstRoots(path, _gameDirectory, _stateDirectory);

    private async Task<bool> ShouldAdoptLegacyFileAsync(string destination,
        CancellationToken cancellationToken)
    {
        if (PathsEqual(destination, Path.Combine(_gameDirectory, "Mods", "BopItAccess.dll")))
            return true;
        string docs = Path.Combine(_gameDirectory, "documentation");
        if (IsWithin(destination, docs) &&
            UninstallManager.IsLegacyDocumentationName(Path.GetFileName(destination)))
            return true;
        if (PathsEqual(destination, Path.Combine(_gameDirectory, "prism.dll")) &&
            !UninstallManager.HasOtherMods(_gameDirectory))
            return string.Equals(await Sha256Async(destination, cancellationToken),
                UninstallManager.LegacyPrismSha256, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    private void AdoptLegacyFiles()
    {
        // The existing mod has no installer ledger. Its own DLL and guides
        // should be removed on a later uninstall, while abort still restores
        // each file from the transaction's rollback backup.
        void adopt(string path)
        {
            if (!File.Exists(path)) return;
            string full = ValidateDestination(path);
            using var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
            _files[full] = new InstalledFile
            {
                Path = full,
                Sha256 = Convert.ToHexString(SHA256.HashData(stream))
            };
        }

        adopt(Path.Combine(_gameDirectory, "Mods", "BopItAccess.dll"));
        string docs = Path.Combine(_gameDirectory, "documentation");
        if (Directory.Exists(docs) && (File.GetAttributes(docs) & FileAttributes.ReparsePoint) == 0)
            foreach (string file in Directory.EnumerateFiles(docs, "*", SafeRecursiveEnumeration))
                if (UninstallManager.IsLegacyDocumentationPath(docs, file)) adopt(file);

        string prism = Path.Combine(_gameDirectory, "prism.dll");
        if (File.Exists(prism) && !UninstallManager.HasOtherMods(_gameDirectory))
        {
            using var stream = new FileStream(prism, FileMode.Open, FileAccess.Read, FileShare.Read);
            string hash = Convert.ToHexString(SHA256.HashData(stream));
            if (hash.Equals(UninstallManager.LegacyPrismSha256, StringComparison.OrdinalIgnoreCase))
                _files[ValidateDestination(prism)] = new InstalledFile { Path = prism, Sha256 = hash };
        }
        _log("Adopted identifiable files from the existing Bop It Access installation.");
    }

    internal static string ValidateAgainstRoots(string path, string gameDirectory, string stateDirectory)
    {
        string full = Path.GetFullPath(path);
        if (!IsWithin(full, gameDirectory) && !IsWithin(full, stateDirectory))
            throw new InvalidDataException($"Installer path escapes its game and state directories: {full}");
        // A junction beneath either root could redirect file writes or deletions elsewhere.
        string root = IsWithin(full, gameDirectory) ? gameDirectory : stateDirectory;
        for (string? ancestor = Path.GetDirectoryName(full);
             ancestor is not null && !PathsEqual(ancestor, root);
             ancestor = Path.GetDirectoryName(ancestor))
        {
            if (Directory.Exists(ancestor) &&
                (File.GetAttributes(ancestor) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException($"Installer path passes through a junction: {ancestor}");
        }
        return full;
    }

    internal static string ValidateBackup(string? path, string stateDirectory)
    {
        if (string.IsNullOrWhiteSpace(path) || !IsWithin(path, stateDirectory))
            throw new InvalidDataException("A backup path is outside the installer state directory.");
        return Path.GetFullPath(path);
    }

    internal static bool IsWithin(string path, string directory)
    {
        string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
        string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar);
        return full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathsEqual(string first, string second) =>
        string.Equals(Path.GetFullPath(first).TrimEnd(Path.DirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

    internal static async Task<string> Sha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }

    private void ThrowIfFinished()
    {
        if (_finished) throw new InvalidOperationException("The install transaction is already finished.");
    }

    private sealed class TransactionJournal
    {
        public string Id { get; set; } = string.Empty;
        public string GameDirectory { get; set; } = string.Empty;
        public string StateDirectory { get; set; } = string.Empty;
        public List<JournalAction> Actions { get; set; } = new();
    }

    private sealed class JournalAction
    {
        public string Kind { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool Existed { get; set; }
        public string? RollbackBackupPath { get; set; }
        public string? NewOriginalBackupPath { get; set; }
        public List<RegistryValueBackup> RegistryValues { get; set; } = new();
        public List<string> ExistingFiles { get; set; } = new();
    }

    private sealed class RegistryValueBackup
    {
        public string Name { get; set; } = string.Empty;
        public RegistryValueKind Kind { get; set; }
        public string? Text { get; set; }
        public long Number { get; set; }
        public byte[]? Binary { get; set; }
        public string[]? TextArray { get; set; }

        public static RegistryValueBackup Capture(RegistryKey key, string name)
        {
            RegistryValueKind kind = key.GetValueKind(name);
            object value = key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames)
                ?? throw new InvalidDataException($"Registry value disappeared: {name}");
            return new RegistryValueBackup
            {
                Name = name,
                Kind = kind,
                Text = value is string text ? text : null,
                Number = value is int integer ? integer : value is long big ? big : 0,
                Binary = value as byte[],
                TextArray = value as string[]
            };
        }

        public object RestoreValue() => Kind switch
        {
            RegistryValueKind.DWord => checked((int)Number),
            RegistryValueKind.QWord => Number,
            RegistryValueKind.Binary or RegistryValueKind.None => Binary ?? Array.Empty<byte>(),
            RegistryValueKind.MultiString => TextArray ?? Array.Empty<string>(),
            _ => Text ?? string.Empty
        };
    }
}
