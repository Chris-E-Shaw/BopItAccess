using System.Runtime.InteropServices;

namespace BopItAccess.Installer;

internal static class InstallerWindowFocus
{
    private const int SwRestore = 9;
    private const uint GwEnabledPopup = 6;
    private const uint SwpNoSize = 0x0001, SwpNoMove = 0x0002, SwpNoActivate = 0x0010;
    private static readonly nint HwndTopMost = -1, HwndNoTopMost = -2;
    internal static readonly uint ActivationMessage =
        RegisterWindowMessage("BopItAccess.Installer.Activate.1");

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindowAsync(nint window, int command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(string? className, string title);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint RegisterWindowMessage(string message);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nuint wParam, nint lParam);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllowSetForegroundWindow(uint processId);
    [DllImport("user32.dll")]
    private static extern nint GetLastActivePopup(nint window);
    [DllImport("user32.dll")]
    private static extern nint GetWindow(nint window, uint command);
    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern nint GetFocus();
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowEnabled(nint window);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint window, nint insertAfter,
        int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo input);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo { internal uint Size, Tick; }
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { internal int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)]
    private struct GuiThreadInfo
    {
        internal uint Size, Flags;
        internal nint Active, Focus, Capture, MenuOwner, MoveSize, Caret;
        internal NativeRect CaretRect;
    }

    // Start only from Shown. Posting the first attempt lets WinForms finish
    // showing/activating the window and lets the desktop settle after UAC.
    internal static IDisposable BeginStartup(Form owner, Func<Control?> initialControl,
        Func<bool> canRetry) => new StartupActivation(owner, initialControl, canRetry);

    internal static bool Activate(Form owner)
    {
        if (owner.IsDisposed || owner.Disposing || !owner.IsHandleCreated) return false;
        if (owner.WindowState == FormWindowState.Minimized)
            owner.WindowState = FormWindowState.Normal;
        nint target = ActivationTarget(owner.Handle);
        // Do not activate a disabled owner behind its modal confirmation or
        // native folder/save dialog. Its existing focused control is preserved.
        if (target == 0 || !IsWindowVisible(target) || !IsWindowEnabled(target)) return false;
        if (Control.FromHandle(target) is Form targetForm) targetForm.Activate();
        bool accepted = SetForegroundWindow(target);
        Record("Explicit activation; SetForegroundWindow=" + accepted +
            "; target=" + FormatHandle(target) + "; " + Snapshot());
        return accepted;
    }

    internal static bool ActivateExisting()
    {
        nint owner = FindWindow(null, "Bop It Access Installer");
        if (owner == 0) return false;
        // A message/confirmation may use the main window's title too. Send
        // the request to the main form, which owns the activation handler.
        owner = GetAncestor(owner, 3); // GA_ROOTOWNER
        if (owner == 0) return false;
        GetWindowThreadProcessId(owner, out uint processId);
        bool granted = AllowSetForegroundWindow(processId);
        int grantError = granted ? 0 : Marshal.GetLastPInvokeError();
        // Let the existing UI thread restore its window and pick its currently
        // open popup. The receiver records the permission handoff outcome.
        if (ActivationMessage != 0 && PostMessage(owner, ActivationMessage,
                granted ? 1u : 0u, grantError)) return true;
        // The registered-message path can fail during window teardown. This
        // asynchronous restore avoids waiting on another process's UI thread.
        ShowWindowAsync(owner, SwRestore);
        return SetForegroundWindow(ActivationTarget(owner));
    }

    internal static bool TryHandleActivationMessage(Form owner, ref Message message)
    {
        if (ActivationMessage == 0 || (uint)message.Msg != ActivationMessage) return false;
        Record("Second launch foreground permission=" +
            (message.WParam != 0 ? "granted" : "denied") +
            "; AllowSetForegroundWindow error=" + message.LParam + "; " + Snapshot());
        Activate(owner);
        message.Result = 0;
        return true;
    }

    private static nint ActivationTarget(nint owner)
    {
        nint target = owner;
        // A nested modal can disable another owned popup. Keep the walk
        // bounded and never select a hidden or disabled window as the target.
        for (int depth = 0; depth < 8; depth++)
        {
            nint popup = GetLastActivePopup(target);
            if (popup == 0 || popup == target || !IsWindowVisible(popup))
                popup = GetWindow(target, GwEnabledPopup);
            if (popup == 0 || popup == target || !IsWindowVisible(popup)) break;
            target = popup;
        }
        return target;
    }

    private static bool TryLastInput(out uint tick)
    {
        var input = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        bool success = GetLastInputInfo(ref input);
        tick = input.Tick;
        return success;
    }

    private static string Snapshot()
    {
        nint foreground = GetForegroundWindow();
        uint thread = GetWindowThreadProcessId(foreground, out uint process);
        var info = new GuiThreadInfo { Size = (uint)Marshal.SizeOf<GuiThreadInfo>() };
        bool hasInfo = GetGUIThreadInfo(0, ref info);
        return "foreground=" + FormatHandle(foreground) + "; foregroundPID=" + process +
            "; foregroundThread=" + thread + "; foregroundFocus=" +
            (hasInfo ? FormatHandle(info.Focus) : "unavailable") +
            "; UI-thread focus=" + FormatHandle(GetFocus());
    }

    private static string FormatHandle(nint window) => "0x" + window.ToString("X");
    private static void Record(string message) => InstallerDiagnostics.Current?.Write("FOCUS", message);

    private sealed class StartupActivation : IDisposable
    {
        private const int MaximumAttempts = 8, MaximumMilliseconds = 2000, SettledMilliseconds = 250;
        private readonly Form _owner;
        private readonly Func<Control?> _initialControl;
        private readonly Func<bool> _canRetry;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 125 };
        private readonly long _started = Environment.TickCount64;
        private readonly uint _inputAtStart;
        private readonly bool _hasInputAtStart;
        private int _attempts;
        private bool _disposed, _pulsed;
        private long? _foregroundSince;
        private Control? _focusedControl;

        internal StartupActivation(Form owner, Func<Control?> initialControl, Func<bool> canRetry)
        {
            _owner = owner;
            _initialControl = initialControl;
            _canRetry = canRetry;
            _hasInputAtStart = TryLastInput(out _inputAtStart);
            _timer.Tick += Tick;
            _owner.Disposed += OwnerDisposed;
            Record("Startup activation scheduled; lastInputAvailable=" + _hasInputAtStart +
                "; " + Snapshot());
            _owner.BeginInvoke((Action)(() =>
            {
                if (_disposed) return;
                Attempt();
                if (!_disposed) _timer.Start();
            }));
        }

        private void OwnerDisposed(object? sender, EventArgs e) => Dispose();
        private void Tick(object? sender, EventArgs e) => Attempt();

        private void Attempt()
        {
            if (_disposed) return;
            if (_owner.IsDisposed || _owner.Disposing || !_owner.IsHandleCreated || !_owner.Visible)
            { Stop("window closed or hidden"); return; }
            if (!_canRetry() || !IsWindowEnabled(_owner.Handle) || ActivationTarget(_owner.Handle) != _owner.Handle)
            { Stop("operation or owned dialog took over"); return; }
            if (_hasInputAtStart && TryLastInput(out uint inputNow) && inputNow != _inputAtStart)
            { Stop("new user input; preserving the user's window and control choice"); return; }
            if (Environment.TickCount64 - _started >= MaximumMilliseconds)
            { Stop("startup deadline reached"); return; }

            bool foreground = GetForegroundWindow() == _owner.Handle;
            if (foreground && _focusedControl is not null && !_focusedControl.Focused)
            {
                if (_focusedControl.CanFocus)
                { Stop("control focus changed after initial selection"); return; }
                // Initialization may temporarily disable the game-folder
                // field. Let the caller select its enabled fallback instead
                // of treating that automatic change as a user's choice.
                _focusedControl = null;
            }
            if (foreground && _focusedControl?.Focused == true)
            {
                _foregroundSince ??= Environment.TickCount64;
                if (Environment.TickCount64 - _foregroundSince >= SettledMilliseconds)
                    Stop("foreground and initial keyboard focus confirmed");
                return;
            }
            _foregroundSince = null;
            if (_attempts >= MaximumAttempts) { Stop("startup attempt limit reached"); return; }

            _attempts++;
            // A single brief pulse makes a denied startup window visible. It
            // changes z-order only; foreground permission still belongs to
            // Windows. Never leave the installer topmost after this call.
            if (_attempts == 2 && !_pulsed && !_owner.TopMost) PulseVisibility();
            bool accepted = Activate(_owner);
            foreground = GetForegroundWindow() == _owner.Handle;
            bool focusResult = false;
            Control? control = _initialControl();
            // Focus() alone does not bring a process to the foreground. Assign
            // keyboard focus only after that separate operation is confirmed.
            if (foreground && control is not null && control.FindForm() == _owner && control.CanFocus)
            {
                focusResult = control.Focus();
                if (control.Focused) _focusedControl = control;
            }
            Record("Startup attempt " + _attempts + "; activationAccepted=" + accepted +
                "; actualForeground=" + foreground + "; initialControl=" +
                (control?.AccessibleName ?? control?.GetType().Name ?? "none") +
                "; Focus result=" + focusResult + "; controlFocused=" + (control?.Focused == true) +
                "; " + Snapshot() + (accepted ? "." :
                    ". Windows returned false; SetForegroundWindow does not document a last-error reason."));
            if (foreground && _focusedControl?.Focused == true) _foregroundSince = Environment.TickCount64;
        }

        private void PulseVisibility()
        {
            _pulsed = true;
            uint flags = SwpNoSize | SwpNoMove | SwpNoActivate;
            bool raised = false, restored = false;
            try { raised = SetWindowPos(_owner.Handle, HwndTopMost, 0, 0, 0, 0, flags); }
            finally
            {
                restored = SetWindowPos(_owner.Handle, HwndNoTopMost, 0, 0, 0, 0, flags);
                if (!restored)
                    restored = SetWindowPos(_owner.Handle, HwndNoTopMost, 0, 0, 0, 0, flags);
                Record("Temporary startup visibility pulse; raised=" + raised +
                    "; topmost removed=" + restored + "; " + Snapshot());
            }
        }

        private void Stop(string reason)
        {
            Record("Startup activation stopped after " + _attempts + " attempt(s); " + reason +
                "; " + Snapshot());
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Stop();
            _timer.Tick -= Tick;
            _timer.Dispose();
            _owner.Disposed -= OwnerDisposed;
        }
    }
}
