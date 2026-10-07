using System.Runtime.InteropServices;
using System.Text;

namespace BopItAccess.Installer;

/// <summary>Local XInput navigation for the installer and its owned dialogs.</summary>
internal sealed class InstallerGamepad : IDisposable
{
    private const ushort Up = 0x0001, Down = 0x0002, Left = 0x0004, Right = 0x0008,
        Start = 0x0010, LeftBumper = 0x0100, RightBumper = 0x0200,
        A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000;
    private const ushort ActionButtons = Start | LeftBumper | RightBumper | A | B | X | Y;
    private const int StickPress = 11000, StickRelease = 7500;
    private const int WmKeyDown = 0x0100, WmKeyUp = 0x0101, WmClose = 0x0010,
        WmNextDialogControl = 0x0028, BmClick = 0x00F5, EmSetSelection = 0x00B1;
    private readonly Form _owner;
    private readonly Action _quit;
    private readonly Action? _advanced;
    private readonly Action? _cancel;
    private readonly Func<Form, bool, bool>? _navigateFocus;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 16 };
    private readonly ControllerHistory[] _controllers = Enumerable.Range(0, 4).Select(_ => new ControllerHistory()).ToArray();
    private nint _activeWindow;
    private bool _disposed;
    private bool _xinputAvailable = true;
    private bool _useLegacyXinput;
    private TextBoxBase? _selectionBox;
    private int _selectionAnchor, _selectionCaret;
    private bool _extendingSelection;

    internal InstallerGamepad(Form owner, Action quit, Action? advanced = null, Action? cancel = null,
        Func<Form, bool, bool>? navigateFocus = null)
    {
        _owner = owner;
        _quit = quit;
        _advanced = advanced;
        _cancel = cancel;
        _navigateFocus = navigateFocus;
        _timer.Tick += Poll;
        _owner.FormClosed += OwnerClosed;
        _timer.Start();
    }

    private void OwnerClosed(object? sender, FormClosedEventArgs e) => Dispose();

    private void Poll(object? sender, EventArgs e)
    {
        if (_disposed || !_xinputAvailable || _owner.IsDisposed) return;
        var target = ActiveTarget();
        if (target is null)
        {
            _activeWindow = 0;
            return;
        }
        bool changedWindow = target.Window != _activeWindow;
        _activeWindow = target.Window;
        long now = Environment.TickCount64;
        for (uint index = 0; index < 4; index++)
        {
            var history = _controllers[index];
            if (!history.Connected && now < history.NextProbe) continue;
            if (!TryRead(index, out var state))
            {
                history.Connected = false;
                history.NextProbe = now + 1000;
                continue;
            }
            Direction direction = DirectionFrom(state.Gamepad, history.Direction);
            bool firstSample = changedWindow || !history.Connected;
            if (firstSample)
            {
                history.BlockedButtons = state.Gamepad.Buttons;
                history.BlockedDirection = direction;
                history.PreviousButtons = state.Gamepad.Buttons;
                history.Direction = direction;
                history.RepeatAt = now + 550;
                history.Connected = true;
                continue;
            }
            history.BlockedButtons &= state.Gamepad.Buttons;
            ushort pressed = (ushort)(state.Gamepad.Buttons & ~history.PreviousButtons & ~history.BlockedButtons & ActionButtons);
            history.PreviousButtons = state.Gamepad.Buttons;
            if (direction != history.BlockedDirection) history.BlockedDirection = Direction.None;
            bool directionReady = direction != Direction.None && history.BlockedDirection == Direction.None;
            bool navigation = directionReady && (direction != history.Direction || now >= history.RepeatAt);
            history.RepeatAt = direction != history.Direction ? now + 550 : navigation ? now + 110 : history.RepeatAt;
            history.Direction = direction;
            // Update histories before invoking UI actions: PerformClick can open
            // a modal dialog whose message loop polls this same timer again.
            if (pressed != 0)
            {
                DispatchButton(target, pressed);
                return;
            }
            if (navigation)
            {
                Navigate(target, direction, state.Gamepad.RightTrigger > 30);
                return;
            }
        }
    }

    private void DispatchButton(Target target, ushort pressed)
    {
        if (!IsStillActive(target)) return;
        if ((pressed & B) != 0) { Cancel(target); return; }
        if ((pressed & Start) != 0)
        {
            if (target.Form == _owner) _quit(); else Cancel(target);
            return;
        }
        if ((pressed & LeftBumper) != 0) { MoveFocus(target, false); return; }
        if ((pressed & RightBumper) != 0) { MoveFocus(target, true); return; }
        if ((pressed & Y) != 0 && target.Form == _owner) { _advanced?.Invoke(); return; }
        if ((pressed & X) != 0)
        {
            var control = FocusedControl(target.Form);
            if (control is TextBoxBase box) { box.SelectAll(); _extendingSelection = false; }
            else if (target.Form is null && NativeFocus(target.Window) is nint focus && focus != 0 && IsTextWindow(focus))
                SendMessage(focus, EmSetSelection, 0, -1);
            return;
        }
        if ((pressed & A) == 0) return;
        if (target.Form is null)
        {
            nint focus = NativeFocus(target.Window);
            if (focus == 0) return;
            if (WindowClass(focus).Equals("Button", StringComparison.OrdinalIgnoreCase) && IsWindowEnabled(focus))
                SendMessage(focus, BmClick, 0, 0);
            else PostLocalKey(target, focus, Keys.Enter);
            return;
        }
        Control? selected = FocusedControl(target.Form);
        if (selected is null || !selected.Enabled || !selected.Visible) return;
        switch (selected)
        {
            case IButtonControl button: button.PerformClick(); break;
            case CheckBox check: check.Checked = !check.Checked; break;
            case RadioButton radio: radio.Checked = true; break;
            case ComboBox combo: combo.DroppedDown = !combo.DroppedDown; break;
            case TextBoxBase: break; // A on a log never activates a separate default action.
            default:
                if (target.Form.AcceptButton is Button accept && accept.Enabled && accept.Visible) accept.PerformClick();
                break;
        }
    }

    private void Cancel(Target target)
    {
        if (target.Form == _owner) { (_cancel ?? _quit)(); return; }
        if (target.Form is null) { PostMessage(target.Window, WmClose, 0, 0); return; }
        if (target.Form.CancelButton is Button cancel && cancel.Enabled && cancel.Visible) cancel.PerformClick();
        else target.Form.Close();
    }

    private void Navigate(Target target, Direction direction, bool extendSelection)
    {
        if (!IsStillActive(target)) return;
        Control? focused = FocusedControl(target.Form);
        if (focused is TextBoxBase box && (box.Multiline || direction is Direction.Left or Direction.Right))
        {
            MoveCaret(box, direction, extendSelection);
            return;
        }
        if (focused is ListBox or ListView or TreeView or ComboBox or DataGridView or UpDownBase)
        {
            PostLocalKey(target, focused.Handle, DirectionKey(direction));
            return;
        }
        if (target.Form is null)
        {
            nint focus = NativeFocus(target.Window);
            if (focus != 0) PostLocalKey(target, focus, DirectionKey(direction));
            return;
        }
        MoveFocus(target, direction is Direction.Down or Direction.Right);
    }

    private void MoveFocus(Target target, bool forward)
    {
        _extendingSelection = false;
        if (target.Form is null)
        {
            PostMessage(target.Window, WmNextDialogControl, forward ? 0 : 1, 0);
            return;
        }
        if (_navigateFocus?.Invoke(target.Form, forward) == true) return;
        target.Form.SelectNextControl(FocusedControl(target.Form), forward, true, true, true);
    }

    private void MoveCaret(TextBoxBase box, Direction direction, bool extend)
    {
        bool sameSelection = ReferenceEquals(box, _selectionBox) &&
            box.SelectionStart == Math.Min(_selectionAnchor, _selectionCaret) &&
            box.SelectionLength == Math.Abs(_selectionCaret - _selectionAnchor);
        int caret = extend && _extendingSelection && sameSelection ? _selectionCaret :
            direction is Direction.Up or Direction.Left ? box.SelectionStart : box.SelectionStart + box.SelectionLength;
        if (!extend || !_extendingSelection || !sameSelection)
        {
            _selectionBox = box;
            _selectionAnchor = caret;
        }
        string text = box.Text;
        int next = caret;
        bool collapseSelection = !extend && box.SelectionLength > 0 && direction is Direction.Left or Direction.Right;
        if (direction == Direction.Left && caret > 0 && !collapseSelection)
        {
            next--;
            if (next > 0 && char.IsLowSurrogate(text[next]) && char.IsHighSurrogate(text[next - 1])) next--;
        }
        else if (direction == Direction.Right && caret < text.Length && !collapseSelection)
        {
            next++;
            if (next < text.Length && char.IsHighSurrogate(text[caret]) && char.IsLowSurrogate(text[next])) next++;
        }
        else if (direction is Direction.Up or Direction.Down)
        {
            int currentLine = box.GetLineFromCharIndex(caret);
            int lineStart = box.GetFirstCharIndexFromLine(currentLine);
            int nextLine = currentLine + (direction == Direction.Up ? -1 : 1);
            int nextStart = nextLine >= 0 ? box.GetFirstCharIndexFromLine(nextLine) : -1;
            if (lineStart >= 0 && nextStart >= 0)
            {
                int nextEnd = box.GetFirstCharIndexFromLine(nextLine + 1);
                if (nextEnd < 0) nextEnd = text.Length;
                while (nextEnd > nextStart && text[nextEnd - 1] is '\r' or '\n') nextEnd--;
                next = Math.Min(nextStart + caret - lineStart, nextEnd);
            }
        }
        _selectionCaret = Math.Clamp(next, 0, text.Length);
        _extendingSelection = extend;
        if (extend) box.Select(Math.Min(_selectionAnchor, _selectionCaret), Math.Abs(_selectionCaret - _selectionAnchor));
        else box.Select(_selectionCaret, 0);
        box.ScrollToCaret();
    }

    private Target? ActiveTarget()
    {
        nint foreground = GetForegroundWindow();
        if (foreground == 0 || !_owner.IsHandleCreated || !_owner.Visible || IsIconic(_owner.Handle)) return null;
        GetWindowThreadProcessId(foreground, out uint process);
        if (process != Environment.ProcessId) return null;
        if (Form.FromHandle(foreground) is Form form)
        {
            for (Form? candidate = form; candidate is not null; candidate = candidate.Owner)
                if (candidate == _owner) return new(foreground, form);
            return null;
        }
        // Standard Windows folder/save dialogs are native rather than Forms.
        // Accept them only when their Win32 owner chain reaches this installer.
        if (!WindowClass(foreground).Equals("#32770", StringComparison.Ordinal)) return null;
        for (nint window = foreground; window != 0; window = GetWindow(window, 4))
            if (window == _owner.Handle) return new(foreground, null);
        return null;
    }

    private bool IsStillActive(Target target) => !_disposed && GetForegroundWindow() == target.Window;

    private static Control? FocusedControl(Form? form)
    {
        Control? control = form?.ActiveControl;
        while (control is ContainerControl container && container.ActiveControl is Control child) control = child;
        return control;
    }

    private static nint NativeFocus(nint foreground)
    {
        uint thread = GetWindowThreadProcessId(foreground, out uint process);
        if (process != Environment.ProcessId) return 0;
        var information = new GuiThreadInformation { Size = (uint)Marshal.SizeOf<GuiThreadInformation>() };
        if (!GetGUIThreadInfo(thread, ref information)) return 0;
        return information.Focus == foreground || IsChild(foreground, information.Focus) ? information.Focus : 0;
    }

    private void PostLocalKey(Target target, nint focus, Keys key)
    {
        if (!IsStillActive(target) || focus == 0) return;
        GetWindowThreadProcessId(focus, out uint process);
        if (process != Environment.ProcessId || (focus != target.Window && !IsChild(target.Window, focus))) return;
        uint scan = MapVirtualKey((uint)key, 0);
        int keyData = 1 | ((int)scan << 16);
        if (key is Keys.Up or Keys.Down or Keys.Left or Keys.Right) keyData |= 1 << 24;
        PostMessage(focus, WmKeyDown, (nint)key, keyData);
        PostMessage(focus, WmKeyUp, (nint)key, unchecked(keyData | (int)0xC0000000));
    }

    private bool TryRead(uint index, out XInputState state)
    {
        try
        {
            return (_useLegacyXinput ? GetLegacyState(index, out state) : GetState(index, out state)) == 0;
        }
        catch (Exception error) when (error is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
        {
            if (!_useLegacyXinput)
            {
                _useLegacyXinput = true;
                return TryRead(index, out state);
            }
            _xinputAvailable = false;
            InstallerDiagnostics.Current?.Error("Windows XInput is unavailable; installer keyboard and mouse controls remain available", error);
            state = default;
            return false;
        }
    }

    private static Direction DirectionFrom(XInputGamepad state, Direction previous)
    {
        bool up = (state.Buttons & Up) != 0, down = (state.Buttons & Down) != 0,
            left = (state.Buttons & Left) != 0, right = (state.Buttons & Right) != 0;
        if (up != down) return up ? Direction.Up : Direction.Down;
        if (left != right) return left ? Direction.Left : Direction.Right;
        int threshold = previous == Direction.None ? StickPress : StickRelease;
        int x = state.LeftX, y = state.LeftY;
        if (Math.Max(Math.Abs(x), Math.Abs(y)) < threshold) return Direction.None;
        return Math.Abs(y) >= Math.Abs(x) ? y > 0 ? Direction.Up : Direction.Down : x > 0 ? Direction.Right : Direction.Left;
    }

    private static Keys DirectionKey(Direction direction) => direction switch
    {
        Direction.Up => Keys.Up, Direction.Down => Keys.Down, Direction.Left => Keys.Left, _ => Keys.Right
    };
    private static string WindowClass(nint window)
    {
        var name = new StringBuilder(128);
        GetClassName(window, name, name.Capacity);
        return name.ToString();
    }
    private static bool IsTextWindow(nint window) => WindowClass(window) is string name &&
        (name.Equals("Edit", StringComparison.OrdinalIgnoreCase) || name.StartsWith("RichEdit", StringComparison.OrdinalIgnoreCase));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= Poll;
        _timer.Dispose();
        _owner.FormClosed -= OwnerClosed;
    }

    private enum Direction { None, Up, Down, Left, Right }
    private sealed record Target(nint Window, Form? Form);
    private sealed class ControllerHistory
    {
        internal bool Connected;
        internal long NextProbe, RepeatAt;
        internal ushort PreviousButtons, BlockedButtons;
        internal Direction Direction, BlockedDirection;
    }
    [StructLayout(LayoutKind.Sequential)] private struct XInputState { internal uint Packet; internal XInputGamepad Gamepad; }
    [StructLayout(LayoutKind.Sequential)] private struct XInputGamepad
    {
        internal ushort Buttons;
        internal byte LeftTrigger, RightTrigger;
        internal short LeftX, LeftY, RightX, RightY;
    }
    [StructLayout(LayoutKind.Sequential)] private struct GuiThreadInformation
    {
        internal uint Size, Flags;
        internal nint Active, Focus, Capture, MenuOwner, MoveSize, Caret;
        internal int Left, Top, Right, Bottom;
    }
    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetState(uint index, out XInputState state);
    [DllImport("xinput9_1_0.dll", EntryPoint = "XInputGetState"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetLegacyState(uint index, out XInputState state);
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")] private static extern nint GetWindow(nint window, uint command);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetGUIThreadInfo(uint thread, ref GuiThreadInformation information);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsChild(nint parent, nint child);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsIconic(nint window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowEnabled(nint window);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(nint window, StringBuilder name, int maximum);
    [DllImport("user32.dll", EntryPoint = "SendMessageW")] private static extern nint SendMessage(nint window, int message, nint wParam, nint lParam);
    [DllImport("user32.dll", EntryPoint = "PostMessageW")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PostMessage(nint window, int message, nint wParam, nint lParam);
    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyW")] private static extern uint MapVirtualKey(uint code, uint type);
}
