using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BopItAccess.Installer;

// Local recording is independent of the installation transaction: rolling
// files back must not erase the explanation of why that rollback happened.
internal sealed class InstallerDiagnostics : IDisposable
{
    private const int MaximumBytes = 8 * 1024 * 1024;
    private const int RetainedSessions = 10;
    private static readonly UTF8Encoding Encoding = new(false);
    private static readonly Regex SessionFileName = new(
        @"^Installer-\d{8}T\d{9}Z-[a-f0-9]{32}\.log$", RegexOptions.CultureInvariant);
    private static readonly Regex UrlPattern = new(@"https?://[^\s<>""']+",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static InstallerDiagnostics? _current;
    internal static InstallerDiagnostics? Current => Volatile.Read(ref _current);

    private readonly object _lock = new();
    private readonly Stopwatch _elapsed = Stopwatch.StartNew();
    private readonly StringBuilder _snapshot = new();
    private readonly HashSet<string> _gameDirectories = new(StringComparer.OrdinalIgnoreCase);
    private StreamWriter? _writer;
    private StreamWriter? _exportWriter;
    private string? _directory;
    private string? _filePath;
    private string? _persistenceFailure;
    private string? _exportFailure;
    private string? _exportPath;
    private int _recordedBytes;
    private bool _limitReached;
    private bool _disposed;
    private string? _progressStep;
    private int _progressBucket = -1;
    private bool _progressComplete;
    private long _progressAt;

    internal string SessionId { get; } =
        DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'", CultureInfo.InvariantCulture) +
        "-" + Guid.NewGuid().ToString("N");
    internal string? FilePath { get { lock (_lock) return _filePath; } }
    internal string? PersistenceFailure { get { lock (_lock) return _persistenceFailure; } }
    internal string? ExportFailure { get { lock (_lock) return _exportFailure; } }

    internal InstallerDiagnostics(bool uninstall)
    {
        Volatile.Write(ref _current, this);
        try
        {
            _directory = InstallTransaction.PrepareDiagnosticsDirectory();
            _filePath = Path.Combine(_directory, "Installer-" + SessionId + ".log");
            _writer = new StreamWriter(new FileStream(_filePath, FileMode.CreateNew,
                FileAccess.Write, FileShare.Read), Encoding) { AutoFlush = true };
        }
        catch (Exception ex)
        {
            StopDiskRecording(ex);
            _filePath = null;
        }
        Write("SESSION", "Session " + SessionId + " started; mode: " +
            (uninstall ? "Windows uninstall" : "installer") + ".");
        Write("ENVIRONMENT", "Installer " + Assembly.GetExecutingAssembly().GetName().Version +
            "; " + RuntimeInformation.FrameworkDescription + "; " +
            RuntimeInformation.OSDescription + "; process " + RuntimeInformation.ProcessArchitecture +
            "; OS " + RuntimeInformation.OSArchitecture + "; PID " + Environment.ProcessId + ".");
        Write("ENVIRONMENT", "Executable: " + Environment.ProcessPath +
            "; local time zone: " + TimeZoneInfo.Local.Id + ".");
        Write("PRIVACY", "Recorded locally only. Paths may include Windows user names. " +
            "URL queries, fragments and embedded credentials are omitted. " +
            "No environment-variable or registry dumps, preferences, or game-file contents are collected.");
        if (_persistenceFailure is not null)
            Write("WARNING", "Automatic file recording unavailable: " + _persistenceFailure +
                ". Use Save diagnostics to save the in-memory record.");
        if (_writer is not null)
        {
            try { PruneOlderSessions(); }
            catch (Exception ex) { Error("Could not remove older diagnostic sessions", ex); }
        }
    }

    internal void Write(string category, string message)
    {
        // Diagnostic failures must never replace an installation exception or
        // prevent a rollback. The memory copy remains available for export.
        try
        {
            lock (_lock)
            {
                if (_disposed || _limitReached) return;
                string entry = FormattableString.Invariant(
                    $"[{DateTimeOffset.UtcNow:O}] [+{_elapsed.Elapsed.TotalMilliseconds:F0} ms] [T{Environment.CurrentManagedThreadId}] [{category}] {Sanitize(message)}{Environment.NewLine}");
                int bytes = Encoding.GetByteCount(entry);
                if (_recordedBytes + bytes > MaximumBytes - 512)
                {
                    _limitReached = true;
                    entry = "[LIMIT] This session reached the 8 MiB diagnostic limit. " +
                        "Further entries are omitted. Start a new installer session for another record." +
                        Environment.NewLine;
                    bytes = Encoding.GetByteCount(entry);
                }
                _snapshot.Append(entry);
                _recordedBytes += bytes;
                try { _writer?.Write(entry); }
                catch (Exception ex) { StopDiskRecording(ex); }
                try { _exportWriter?.Write(entry); }
                catch (Exception ex)
                {
                    _exportFailure ??= Sanitize(ex.Message);
                    try { _exportWriter?.Dispose(); } catch { /* The automatic or memory record remains. */ }
                    _exportWriter = null;
                    _exportPath = null;
                }
            }
        }
        catch { /* Recording is optional; installation and UI remain usable. */ }
    }

    internal void Error(string context, Exception error)
    {
        try
        {
            Write("ERROR", context + ": " + error.GetType().FullName +
                $" (HRESULT 0x{error.HResult:X8})" + Environment.NewLine + error);
        }
        catch { /* Even exception formatting must not disrupt recovery. */ }
    }

    internal void Progress(string step, long completed, long? total)
    {
        lock (_lock)
        {
            long now = Environment.TickCount64;
            bool complete = total is >= 0 && completed >= total.Value;
            int bucket = total is > 0
                ? (int)Math.Clamp((double)completed / total.Value * 10, 0, 10) : -1;
            bool changed = !string.Equals(step, _progressStep, StringComparison.Ordinal);
            if (!changed && bucket == _progressBucket && complete == _progressComplete &&
                now - _progressAt < 5000) return;
            _progressStep = step;
            _progressBucket = bucket;
            _progressComplete = complete;
            _progressAt = now;
            Write("PROGRESS", FormattableString.Invariant(
                $"{step}: {completed} / {(total.HasValue ? total.Value.ToString(CultureInfo.InvariantCulture) : "unknown")} units."));
        }
    }

    internal string Snapshot()
    {
        lock (_lock) return _snapshot.ToString();
    }

    internal void ProtectGameDirectory(string directory)
    {
        lock (_lock) _gameDirectories.Add(Path.GetFullPath(directory));
    }

    internal void EnsureExportOutsideGameDirectory(string directory)
    {
        lock (_lock)
            if (_exportPath is not null && InstallTransaction.IsWithin(_exportPath, directory))
                throw new IOException("The active diagnostic copy is inside that game folder. " +
                    "Use Save diagnostics to choose a file outside the game folder before continuing.");
    }

    internal void SaveRecording(string destination)
    {
        string full = Path.GetFullPath(destination);
        string state = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BopItAccess");
        if (InstallTransaction.IsWithin(full, state))
            throw new IOException("Save the diagnostic copy outside the installer state folder, for example in Documents.");
        string extension = Path.GetExtension(full);
        if (!extension.Equals(".log", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            throw new IOException("Save diagnostics with a .log or .txt file extension.");
        for (string? path = full; path is not null; path = Path.GetDirectoryName(path))
            if ((File.Exists(path) || Directory.Exists(path)) &&
                (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Diagnostic export cannot follow a linked file or folder: " + path);
        lock (_lock)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(InstallerDiagnostics));
            if (_gameDirectories.Any(game => InstallTransaction.IsWithin(full, game)))
                throw new IOException("Save the diagnostic copy outside the game folder, for example in Documents.");
            StreamWriter? previous = _exportWriter;
            _exportWriter = null;
            _exportPath = null;
            try { previous?.Dispose(); } catch { /* A failed old export must not prevent choosing a new one. */ }
            var writer = new StreamWriter(new FileStream(full, FileMode.Create,
                FileAccess.Write, FileShare.Read), Encoding) { AutoFlush = true };
            try { writer.Write(_snapshot.ToString()); }
            catch { writer.Dispose(); throw; }
            _exportWriter = writer;
            _exportPath = full;
            _exportFailure = null;
        }
    }

    // Managed uninstall's existing final launcher removes the whole secured
    // state tree. A legacy/manual installation has no launcher or ledger, so
    // explicitly clear only our known diagnostic files after successful removal.
    internal void RemoveAfterLegacyUninstall()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
            if (_directory is null || !Directory.Exists(_directory)) return;
            InstallTransaction.PrepareDiagnosticsDirectory(); // Verify the trusted owner and ACL again.
            foreach (string file in Directory.EnumerateFiles(_directory))
            {
                if (!SessionFileName.IsMatch(Path.GetFileName(file))) continue;
                RejectLinkedFile(file);
                File.Delete(file);
            }
            _filePath = null;
            if (!Directory.EnumerateFileSystemEntries(_directory).Any()) Directory.Delete(_directory);
            string state = Path.GetDirectoryName(_directory)!;
            if (!Directory.EnumerateFileSystemEntries(state).Any()) Directory.Delete(state);
        }
    }

    private void PruneOlderSessions()
    {
        foreach (string file in Directory.EnumerateFiles(_directory!)
                     .Where(file => SessionFileName.IsMatch(Path.GetFileName(file)) &&
                         !string.Equals(file, _filePath, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(file => Path.GetFileName(file), StringComparer.Ordinal)
                     .Skip(RetainedSessions - 1))
        {
            RejectLinkedFile(file);
            File.Delete(file);
            Write("RETENTION", "Removed older diagnostic session: " + Path.GetFileName(file) + ".");
        }
    }

    private static void RejectLinkedFile(string file)
    {
        if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("Diagnostic cleanup cannot follow a linked file: " + file);
    }

    private static string Sanitize(string message) => UrlPattern.Replace(message, match =>
    {
        string value = match.Value.TrimEnd('.', ',', ';', ')', ']', '}');
        string trailing = match.Value[value.Length..];
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)) return "[invalid URL]" + trailing;
        var sanitized = new UriBuilder(uri) { UserName = "", Password = "", Query = "", Fragment = "" };
        return sanitized.Uri.GetLeftPart(UriPartial.Path) + trailing;
    });

    private void StopDiskRecording(Exception error)
    {
        _persistenceFailure ??= Sanitize(error.Message);
        try { _writer?.Dispose(); } catch { /* Keep the in-memory record. */ }
        _writer = null;
    }

    public void Dispose()
    {
        Write("SESSION", "Installer window closed; exit code " + Environment.ExitCode + ".");
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            Interlocked.CompareExchange(ref _current, null, this);
            try { _writer?.Dispose(); } catch { /* Process shutdown must remain possible. */ }
            _writer = null;
            try { _exportWriter?.Dispose(); } catch { /* User-selected export failure must not block shutdown. */ }
            _exportWriter = null;
            _exportPath = null;
        }
    }
}
