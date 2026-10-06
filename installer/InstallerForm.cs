using System.Drawing;

namespace BopItAccess.Installer;

internal sealed class InstallerForm : Form
{
    private readonly InstallerService _service;
    private readonly bool _startUninstall;
    private readonly TextBox _gamePath = new();
    private readonly Button _browse = new();
    private readonly Button _install = new();
    private readonly Button _installAlpha = new();
    private readonly Button _update = new();
    private readonly Button _uninstall = new();
    private readonly Button _abort = new();
    private readonly Button _saveDiagnostics = new();
    private readonly Button _copyDiagnostics = new();
    private readonly Label _stateLabel = new();
    private readonly ProgressBar _progress = new();
    private readonly TextBox _statusLog = new();
    private InstallerState _state = new(null, false, false, false, false, false, null);
    private CancellationTokenSource? _operationCancellation;
    private readonly CancellationTokenSource _initializationCancellation = new();
    private bool _initializationRunning;
    private bool _initializationFinished;
    private bool _closeAfterInitialization;
    private bool _updatingPath;
    private bool _gamePathHasUnappliedEdit;
    private bool _abortRequested;
    private bool _allowAbort;
    private bool _reportedDiagnosticFailure;
    private bool _reportedExportFailure;

    internal InstallerForm(InstallerService service, bool startUninstall)
    {
        _service = service;
        _startUninstall = startUninstall;
        Text = "Bop It Access Installer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(680, 420);
        Size = new Size(780, 540);
        Font = SystemFonts.MessageBoxFont;
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(layout);

        var folderRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            Margin = new Padding(0, 0, 0, 12)
        };
        folderRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folderRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        folderRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var folderLabel = new Label
        {
            Text = "&Game folder:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 0)
        };
        _gamePath.Dock = DockStyle.Fill;
        _gamePath.AccessibleName = "Bop It game folder";
        _gamePath.AccessibleDescription = "Type the Bop It game folder, or use Browse.";
        _gamePath.Margin = new Padding(0, 0, 8, 0);
        _gamePath.TextChanged += (_, _) =>
        {
            if (!_updatingPath)
                _gamePathHasUnappliedEdit = true;
        };
        _gamePath.Leave += (_, _) => ApplyGamePath();
        _gamePath.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            ApplyGamePath();
            e.SuppressKeyPress = true;
        };
        folderLabel.TabIndex = 0;
        _gamePath.TabIndex = 1;
        _browse.Text = "&Browse...";
        _browse.AutoSize = true;
        _browse.AccessibleName = "Browse for Bop It game folder";
        _browse.TabIndex = 2;
        _browse.Click += (_, _) => BrowseForGame();
        folderRow.Controls.Add(folderLabel, 0, 0);
        folderRow.Controls.Add(_gamePath, 1, 0);
        folderRow.Controls.Add(_browse, 2, 0);
        layout.Controls.Add(folderRow, 0, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        ConfigureAction(_install, "&Install", "Install the latest release", 3,
            async () => await RunOperationAsync(_service.InstallReleaseAsync,
                "Bop It Access was installed."));
        ConfigureAction(_installAlpha, "Install &alpha", "Install an alpha build", 4,
            async () =>
            {
                if (!Confirm("You are about to install bleeding-edge code from the latest GitHub commit. It may be unstable. Continue?",
                        "Install alpha"))
                    return;
                await RunOperationAsync(_service.InstallAlphaAsync,
                    "The Bop It Access alpha was installed.");
            });
        ConfigureAction(_update, "&Update", "Update Bop It Access", 5,
            async () => await RunOperationAsync(_service.UpdateAsync,
                "Bop It Access was updated."));
        ConfigureAction(_uninstall, "U&ninstall", "Uninstall Bop It Access", 6,
            UninstallRequestedAsync);
        _abort.Text = "Abo&rt";
        _abort.AutoSize = true;
        _abort.AccessibleName = "Abort the current operation";
        _abort.TabIndex = 7;
        _abort.Click += (_, _) => RequestAbortWithConfirmation();
        actions.Controls.AddRange(new Control[]
            { _install, _installAlpha, _update, _uninstall, _abort });
        layout.Controls.Add(actions, 0, 1);

        _stateLabel.AutoSize = true;
        _stateLabel.Text = "Checking installation status...";
        _stateLabel.AccessibleName = "Installer status";
        _stateLabel.Margin = new Padding(0, 0, 0, 8);
        layout.Controls.Add(_stateLabel, 0, 2);

        _progress.Dock = DockStyle.Top;
        _progress.Height = 22;
        _progress.AccessibleName = "Installation progress";
        _progress.Minimum = 0;
        _progress.Maximum = 100;
        _progress.Margin = new Padding(0, 0, 0, 12);
        layout.Controls.Add(_progress, 0, 4);

        var logPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty
        };
        logPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        logPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        logPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var logLabel = new Label
        {
            Text = "Status &log:",
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4)
        };
        _statusLog.Dock = DockStyle.Fill;
        _statusLog.Multiline = true;
        _statusLog.ReadOnly = true;
        _statusLog.MaxLength = int.MaxValue;
        _statusLog.ScrollBars = ScrollBars.Vertical;
        _statusLog.WordWrap = true;
        _statusLog.AccessibleName = "Installer status log";
        _statusLog.AccessibleDescription = "Read-only, selectable operation messages.";
        _statusLog.TabIndex = 8;
        logPanel.Controls.Add(logLabel, 0, 0);
        logPanel.Controls.Add(_statusLog, 0, 1);
        var diagnosticsActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 6, 0, 0)
        };
        _saveDiagnostics.Text = "Save &diagnostics...";
        _saveDiagnostics.AccessibleName = "Save diagnostics";
        _saveDiagnostics.AccessibleDescription = "Save this session's detailed log and keep recording to that file until this window closes.";
        _saveDiagnostics.AutoSize = true;
        _saveDiagnostics.TabIndex = 9;
        _saveDiagnostics.Click += (_, _) => SaveDiagnostics();
        _copyDiagnostics.Text = "&Copy diagnostics";
        _copyDiagnostics.AccessibleName = "Copy diagnostics";
        _copyDiagnostics.AccessibleDescription = "Copy this session's detailed diagnostic log to the clipboard.";
        _copyDiagnostics.AutoSize = true;
        _copyDiagnostics.TabIndex = 10;
        _copyDiagnostics.Click += (_, _) => CopyDiagnostics();
        diagnosticsActions.Controls.AddRange(new Control[] { _saveDiagnostics, _copyDiagnostics });
        logPanel.Controls.Add(diagnosticsActions, 0, 2);
        layout.Controls.Add(logPanel, 0, 3);

        _service.StatusChanged += message => OnUiThread(() => AppendStatus(message, record: false));
        _service.ProgressChanged += progress => OnUiThread(() => ShowProgress(progress));
        _service.StateChanged += state => OnUiThread(() => ShowState(state));
        Shown += async (_, _) => await InitializeAsync();
        FormClosing += OnFormClosing;
        AppendStatus(_service.Diagnostics.FilePath is string diagnosticPath
            ? "Diagnostic log for this session: " + diagnosticPath
            : "Automatic diagnostic recording is unavailable. Save diagnostics can save this session's in-memory record.");
        UpdateActions();
    }

    private void ConfigureAction(Button button, string text, string accessibleName,
        int tabIndex, Func<Task> action)
    {
        button.Text = text;
        button.AutoSize = true;
        button.AccessibleName = accessibleName;
        button.TabIndex = tabIndex;
        button.Click += async (_, _) => await action();
    }

    private async Task InitializeAsync()
    {
        _initializationRunning = true;
        try
        {
            await Task.Run(() => _service.InitializeAsync(_initializationCancellation.Token));
            if (_closeAfterInitialization || IsDisposed) return;
            _initializationFinished = true;
            UpdateActions();
            if (_startUninstall)
            {
                await UninstallRequestedAsync();
                if (_service.LastUninstallWarningCount == 0)
                    Close();
            }
        }
        catch (OperationCanceledException) when (_initializationCancellation.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!IsDisposed) ShowError("Could not inspect the installation", ex);
        }
        finally
        {
            _initializationRunning = false;
            _initializationFinished = true;
            if (!IsDisposed) UpdateActions();
            if (_closeAfterInitialization && !IsDisposed) Close();
        }
    }

    private void BrowseForGame()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select the Bop It game folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
            SelectedPath = Directory.Exists(_gamePath.Text) ? _gamePath.Text : string.Empty
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        _gamePath.Text = dialog.SelectedPath;
        ApplyGamePath();
    }

    private void ApplyGamePath()
    {
        if (_updatingPath || _operationCancellation != null || !_gamePathHasUnappliedEdit)
            return;
        string requestedPath = _gamePath.Text.Trim();
        _gamePathHasUnappliedEdit = false;
        try
        {
            _service.SetGamePath(requestedPath);
        }
        catch (Exception ex)
        {
            // Keep the user's text available for correction after an invalid path.
            _gamePathHasUnappliedEdit = true;
            _service.SetGamePath(string.Empty);
            ShowError("Could not use that game folder", ex);
        }
    }

    private async Task UninstallRequestedAsync()
    {
        if (!Confirm("Uninstall Bop It Access from the selected game folder?",
                "Confirm uninstall"))
            return;
        await RunOperationAsync(_service.UninstallAsync,
            "Bop It Access was uninstalled.", uninstall: true);
    }

    private async Task RunOperationAsync(Func<CancellationToken, Task> operation,
        string successMessage, bool uninstall = false)
    {
        if (_operationCancellation != null || _state.Busy)
            return;
        ApplyGamePath();
        if ((!uninstall && !_state.ValidGamePath) ||
            (uninstall && !_state.Installed && _service.LastUninstallWarningCount == 0))
        {
            _gamePath.Focus();
            AppendStatus("Select a valid Bop It game folder first.");
            return;
        }

        _abortRequested = false;
        _allowAbort = !uninstall;
        _operationCancellation = new CancellationTokenSource();
        bool succeeded = false;
        UpdateActions();
        try
        {
            await Task.Run(() => operation(_operationCancellation.Token), _operationCancellation.Token);
            if (!_abortRequested && !_operationCancellation.IsCancellationRequested)
            {
                succeeded = true;
                var message = uninstall && _service.LastUninstallWarningCount > 0
                    ? $"Bop It Access files were removed, but cleanup reported {_service.LastUninstallWarningCount} warning(s). Review the status log now. You can run Uninstall again to retry the remaining steps."
                    : successMessage;
                var icon = uninstall && _service.LastUninstallWarningCount > 0
                    ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                MessageBox.Show(this, message, "Bop It Access Installer",
                    MessageBoxButtons.OK, icon);
                if (uninstall && _startUninstall)
                    Environment.ExitCode = _service.LastUninstallWarningCount == 0 ? 0 : 1;
            }
        }
        catch (OperationCanceledException)
        {
            AppendStatus("Operation aborted.");
        }
        catch (Exception ex)
        {
            ShowError("Operation failed", ex);
        }
        finally
        {
            _operationCancellation.Dispose();
            _operationCancellation = null;
            _abortRequested = false;
            _allowAbort = false;
            UpdateActions();
            if (uninstall && succeeded && !_startUninstall && _service.LastUninstallWarningCount == 0)
                Close();
        }
    }

    private void RequestAbortWithConfirmation()
    {
        if (_operationCancellation == null || _abortRequested || !_allowAbort)
            return;
        CancellationTokenSource operation = _operationCancellation;
        if (!Confirm("Abort the current operation?", "Confirm abort"))
            return;
        // The modal confirmation pumps UI messages. The operation can finish
        // and dispose its token while the player is deciding what to do.
        if (!ReferenceEquals(_operationCancellation, operation)) return;
        _abortRequested = true;
        _abort.Enabled = false;
        _service.RequestAbort();
        operation.Cancel();
        AppendStatus("Abort requested.");
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_initializationRunning)
        {
            e.Cancel = true;
            _closeAfterInitialization = true;
            _initializationCancellation.Cancel();
            return;
        }
        if (_operationCancellation == null)
            return;
        e.Cancel = true;
        if (_allowAbort) RequestAbortWithConfirmation();
    }

    private void ShowState(InstallerState state)
    {
        if (state.PathRevision < _state.PathRevision)
            return;
        _state = state;
        if (!_gamePathHasUnappliedEdit && !string.Equals(_gamePath.Text, state.GamePath,
                StringComparison.OrdinalIgnoreCase))
        {
            _updatingPath = true;
            _gamePath.Text = state.GamePath ?? string.Empty;
            _updatingPath = false;
        }
        _stateLabel.Text = state.Message ??
            (state.Installed ? "Bop It Access is installed." :
                state.ValidGamePath ? "Ready to install." :
                    "Select the Bop It game folder.");
        UpdateActions();
    }

    private void UpdateActions()
    {
        bool busy = _state.Busy || _operationCancellation != null;
        bool ready = _state.ReadyForOperations && !busy && !_initializationRunning;
        _gamePath.Enabled = !busy;
        _browse.Enabled = !busy;
        _install.Enabled = ready && _state.ValidGamePath &&
            _state.ReleaseAvailable && !_state.Installed;
        _installAlpha.Enabled = ready && _state.ValidGamePath;
        _update.Enabled = ready && _state.ValidGamePath &&
            _state.Installed && _state.UpdateAvailable;
        _uninstall.Enabled = ready &&
            (_state.Installed || _service.LastUninstallWarningCount > 0);
        _abort.Enabled = busy && _allowAbort && _operationCancellation != null && !_abortRequested;
        _saveDiagnostics.Enabled = _initializationFinished;
        if (!busy && _progress.Style == ProgressBarStyle.Marquee)
            _progress.Style = ProgressBarStyle.Blocks;
    }

    private void ShowProgress(InstallerProgress progress)
    {
        if (progress.Total is > 0)
        {
            if (_progress.Style != ProgressBarStyle.Blocks)
                _progress.Style = ProgressBarStyle.Blocks;
            long percentage = Math.Clamp(progress.Completed * 100 / progress.Total.Value,
                0, 100);
            _progress.Value = (int)percentage;
        }
        else if (_state.Busy || _operationCancellation != null)
        {
            _progress.Style = ProgressBarStyle.Marquee;
        }
        _stateLabel.Text = progress.Step;
    }

    private void AppendStatus(string message, bool record = true)
    {
        if (record) _service.Diagnostics.Write("STATUS", message);
        // Keep the reader's caret and selection in place while new actions arrive.
        bool reviewing = _statusLog.Focused;
        int caret = _statusLog.SelectionStart;
        int selection = _statusLog.SelectionLength;
        _statusLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        if (reviewing)
        {
            _statusLog.SelectionStart = caret;
            _statusLog.SelectionLength = selection;
            _statusLog.ScrollToCaret();
        }
        if (!_reportedDiagnosticFailure && _service.Diagnostics.PersistenceFailure is string failure)
        {
            _reportedDiagnosticFailure = true;
            AppendStatus("Automatic diagnostic recording could not continue: " + failure +
                ". Save diagnostics can still save the in-memory record.", record: false);
        }
        if (!_reportedExportFailure && _service.Diagnostics.ExportFailure is string exportFailure)
        {
            _reportedExportFailure = true;
            AppendStatus("The diagnostic copy could not continue recording: " + exportFailure +
                ". Save diagnostics can select another file.", record: false);
        }
    }

    private void SaveDiagnostics()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Save installer diagnostics",
            Filter = "Diagnostic log (*.log)|*.log|Text file (*.txt)|*.txt",
            DefaultExt = "log",
            AddExtension = true,
            FileName = "BopItAccess-Installer-" + _service.Diagnostics.SessionId + ".log",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            _service.Diagnostics.SaveRecording(dialog.FileName);
            _reportedExportFailure = false;
            AppendStatus("Saving diagnostic copy: " + dialog.FileName +
                ". This file will keep recording until the installer closes.");
        }
        catch (Exception ex) { ShowError("Could not save diagnostics", ex); }
    }

    private void CopyDiagnostics()
    {
        try
        {
            Clipboard.SetText(_service.Diagnostics.Snapshot());
            AppendStatus("Copied this session's diagnostics to the clipboard.");
        }
        catch (Exception ex) { ShowError("Could not copy diagnostics", ex); }
    }

    private void ShowError(string caption, Exception ex)
    {
        _service.Diagnostics.Error(caption, ex);
        AppendStatus($"{caption}: {ex.Message}");
        MessageBox.Show(this, ex.Message, caption, MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private bool Confirm(string question, string caption) =>
        MessageBox.Show(this, question, caption, MessageBoxButtons.YesNo,
            MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) ==
        DialogResult.Yes;

    private void OnUiThread(Action action)
    {
        if (IsDisposed || Disposing)
            return;
        if (InvokeRequired)
        {
            if (IsHandleCreated)
            {
                try { BeginInvoke((Action)(() => { if (!IsDisposed && !Disposing) action(); })); }
                catch (InvalidOperationException) when (IsDisposed || Disposing || !IsHandleCreated)
                { /* The form closed after the worker checked its handle. */ }
            }
        }
        else
        {
            action();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _initializationCancellation.Dispose();
        base.Dispose(disposing);
    }
}
