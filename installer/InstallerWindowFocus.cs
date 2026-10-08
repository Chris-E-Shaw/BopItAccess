using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace BopItAccess.Installer;

internal static class InstallerWindowFocus
{
    private const int SwRestore = 9;
    private const uint GwEnabledPopup = 6;
    private const string AttentionMessage =
        "Bop It Access Installer is open, but Windows kept another window active. " +
        "Press Alt+Tab to switch to Bop It Access Installer before using its keyboard or controller controls.";
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
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FlashInfo info);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetLastInputInfo(ref LastInputInfo input);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo { internal uint Size, Tick; }
    [StructLayout(LayoutKind.Sequential)]
    private struct FlashInfo
    {
        internal uint Size;
        internal nint Window;
        internal uint Flags, Count, Timeout;
    }
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
        Func<bool> canRetry, Action<string>? attention = null) =>
        new StartupActivation(owner, initialControl, canRetry, attention);

    // Capture before WinForms, mutex acquisition, local diagnostics and form
    // construction. The native single-file host and manifest elevation have
    // already run by managed Main; this cannot reconstruct pre-UAC focus.
    internal static string CaptureLaunchState()
    {
        // Retain a legitimate launch permission for this process through form
        // construction. This grants nobody else permission and cannot succeed
        // when Windows has already denied us the right to set foreground.
        bool permissionGranted = AllowSetForegroundWindow((uint)Environment.ProcessId);
        int permissionError = permissionGranted ? 0 : Marshal.GetLastPInvokeError();
        string state = Snapshot();
        string elevated, processAge;
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            elevated = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator).ToString();
        }
        catch (Exception ex) { elevated = "unavailable (" + ex.GetType().Name + ")"; }
        try
        {
            using var process = Process.GetCurrentProcess();
            processAge = Math.Max(0, (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMilliseconds)
                .ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception ex) { processAge = "unavailable (" + ex.GetType().Name + ")"; }
        return state + "; startupForegroundPermission=" + (permissionGranted ? "granted" : "denied") +
            "; AllowSetForegroundWindow error=" + permissionError +
            "; administratorToken=" + elevated + "; nativeProcessAgeMs=" + processAge;
    }

    internal static bool Activate(Form owner)
    {
        if (owner.IsDisposed || owner.Disposing || !owner.IsHandleCreated) return false;
        if (owner.WindowState == FormWindowState.Minimized)
            owner.WindowState = FormWindowState.Normal;
        nint target = ActivationTarget(owner.Handle);
        // Do not activate a disabled owner behind its modal confirmation or
        // native folder/save dialog. Its existing focused control is preserved.
        if (target == 0 || !IsWindowVisible(target) || !IsWindowEnabled(target)) return false;
        // SetForegroundWindow also activates the target. Form.Activate calls
        // the same native API for a top-level form, so do not request twice.
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
        if (!Activate(owner) && GetForegroundWindow() != ActivationTarget(owner.Handle))
        {
            RequestAttention(owner);
            InstallerFeedback.Announce(owner, AttentionMessage, important: true, allowBackground: true);
        }
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
            "; foregroundExecutable=" + ExecutableName(process) +
            "; foregroundThread=" + thread + "; foregroundFocus=" +
            (hasInfo ? FormatHandle(info.Focus) : "unavailable") +
            "; UI-thread focus=" + FormatHandle(GetFocus());
    }

    private static string FormatHandle(nint window) => "0x" + window.ToString("X");
    private static void Record(string message) => InstallerDiagnostics.Current?.Write("FOCUS", message);

    private static string ExecutableName(uint processId)
    {
        if (processId == 0) return "none";
        try
        {
            using var process = Process.GetProcessById(checked((int)processId));
            // ProcessName is the executable filename without its extension
            // or path. Do not read MainWindowTitle, arguments or module paths.
            return process.ProcessName;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
            Win32Exception or NotSupportedException or OverflowException)
        { return "unavailable (" + ex.GetType().Name + ")"; }
    }

    private static void RequestAttention(Form owner)
    {
        if (owner.IsDisposed || owner.Disposing || !owner.IsHandleCreated) return;
        var info = new FlashInfo
        {
            Size = (uint)Marshal.SizeOf<FlashInfo>(), Window = owner.Handle,
            Flags = 3, Count = 3 // FLASHW_ALL: three caption/taskbar flashes.
        };
        // The return value describes the previous caption state, not success.
        bool previouslyActive = FlashWindowEx(ref info);
        Record("Requested three taskbar/caption attention flashes; previouslyActive=" + previouslyActive +
            "; " + Snapshot());
    }

    private sealed class StartupActivation : IDisposable
    {
        private const int MaximumAttempts = 3, MaximumMilliseconds = 2000, SettledMilliseconds = 250;
        private readonly Form _owner;
        private readonly Func<Control?> _initialControl;
        private readonly Func<bool> _canRetry;
        private readonly Action<string>? _attention;
        private readonly HashSet<nint> _attemptedForegrounds = new();
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 125 };
        private readonly long _started = Environment.TickCount64;
        private readonly uint _inputAtStart;
        private readonly bool _hasInputAtStart;
        private int _attempts;
        private bool _disposed;
        private long? _foregroundSince;
        private Control? _focusedControl;

        internal StartupActivation(Form owner, Func<Control?> initialControl, Func<bool> canRetry,
            Action<string>? attention)
        {
            _owner = owner;
            _initialControl = initialControl;
            _canRetry = canRetry;
            _attention = attention;
            _hasInputAtStart = TryLastInput(out _inputAtStart);
            _timer.Tick += Tick;
            _owner.Disposed += OwnerDisposed;
            Record("Startup activation scheduled; original " + Snapshot() +
                "; lastInputAvailable=" + _hasInputAtStart +
                "; repeated requests against an unchanged foreground are suppressed.");
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
            { Stop("startup observation deadline reached", requestAttention: true); return; }

            nint foregroundWindow = GetForegroundWindow();
            bool foreground = foregroundWindow == _owner.Handle;
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
            bool? accepted = null;
            if (!foreground)
            {
                // The 0.2.1 log demonstrated eight denials against the same
                // foreground window. Wait for a desktop/foreground transition
                // before making another claim instead of repeating that call.
                if (_attemptedForegrounds.Contains(foregroundWindow)) return;
                if (_attempts >= MaximumAttempts)
                { Stop("startup foreground-transition limit reached", requestAttention: true); return; }
                _attemptedForegrounds.Add(foregroundWindow);
                _attempts++;
                accepted = Activate(_owner);
                foreground = GetForegroundWindow() == _owner.Handle;
            }
            bool focusResult = false;
            Control? control = _initialControl();
            // Focus() alone does not bring a process to the foreground. Assign
            // keyboard focus only after that separate operation is confirmed.
            if (foreground && control is not null && control.FindForm() == _owner && control.CanFocus)
            {
                focusResult = control.Focus();
                if (control.Focused) _focusedControl = control;
            }
            Record("Startup attempt " + _attempts + "; activationAccepted=" +
                (accepted?.ToString() ?? "not requested") +
                "; actualForeground=" + foreground + "; initialControl=" +
                (control?.AccessibleName ?? control?.GetType().Name ?? "none") +
                "; Focus result=" + focusResult + "; controlFocused=" + (control?.Focused == true) +
                "; keyboardReady=" + (foreground && control?.Focused == true) +
                "; " + Snapshot() + (accepted != false ? "." :
                    ". Windows returned false; SetForegroundWindow does not document a last-error reason."));
            if (foreground && _focusedControl?.Focused == true) _foregroundSince = Environment.TickCount64;
        }

        private void Stop(string reason, bool requestAttention = false)
        {
            Record("Startup activation stopped after " + _attempts + " attempt(s); " + reason +
                "; " + Snapshot());
            Dispose();
            if (requestAttention && GetForegroundWindow() != _owner.Handle)
            {
                RequestAttention(_owner);
                _attention?.Invoke(AttentionMessage);
            }
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
