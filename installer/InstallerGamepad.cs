using System.Globalization;
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
        WmNextDialogControl = 0x0028, WmCopy = 0x0301, BmClick = 0x00F5,
        EmGetSelection = 0x00B0, EmSetSelection = 0x00B1, EmGetCaretIndex = 0x1512,
        EmPositionFromChar = 0x00D6, EmFindWordBreak = 0x044C;
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
    private int? _verticalGoalX;
    private string? _navigationText;
    private int[] _textElements = [];
    private string? _lastReviewMessage;

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
            ResetSelection();
            return;
        }
        bool changedWindow = target.Window != _activeWindow;
        _activeWindow = target.Window;
        if (changedWindow || !ReferenceEquals(FocusedControl(target.Form), _selectionBox)) ResetSelection();
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
                Navigate(target, direction, state.Gamepad.LeftTrigger > 30, state.Gamepad.RightTrigger > 30);
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
        if ((pressed & Y) != 0)
        {
            if (SelectAllText(target)) return;
            if (target.Form == _owner) { _advanced?.Invoke(); return; }
        }
        if ((pressed & X) != 0)
        {
            CopySelection(target);
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

    private void Navigate(Target target, Direction direction, bool byUnit, bool extendSelection)
    {
        if (!IsStillActive(target)) return;
        Control? focused = FocusedControl(target.Form);
        if (focused is TextBoxBase box)
        {
            MoveCaret(target, box, direction, byUnit, extendSelection);
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
        ResetSelection();
        if (target.Form is null)
        {
            PostMessage(target.Window, WmNextDialogControl, forward ? 0 : 1, 0);
            return;
        }
        if (_navigateFocus?.Invoke(target.Form, forward) == true) return;
        target.Form.SelectNextControl(FocusedControl(target.Form), forward, true, true, true);
    }

    private void ResetSelection()
    {
        _selectionBox = null;
        _verticalGoalX = null;
        _lastReviewMessage = null;
    }

    private bool SelectAllText(Target target)
    {
        if (FocusedControl(target.Form) is TextBoxBase box)
        {
            if (!IsFocusedTarget(target, box.Handle)) return true;
            box.SelectAll();
            // Select All anchors at the beginning, with the active end at the
            // end of the text, so the next RT arrow can shrink or extend it.
            _selectionBox = box;
            _selectionAnchor = box.SelectionStart;
            _selectionCaret = _selectionAnchor + box.SelectionLength;
            _verticalGoalX = null;
            _lastReviewMessage = null;
            InstallerFeedback.Announce(box, box.SelectionLength > 0 ? "All text selected." : "No text to select.", important: true);
            return true;
        }
        if (target.Form is not null) return false;
        nint focus = NativeFocus(target.Window);
        if (!IsFocusedTarget(target, focus) || !IsTextWindow(focus)) return false;
        ResetSelection();
        SendMessage(focus, EmSetSelection, 0, -1);
        if (!IsFocusedTarget(target, focus)) return true;
        SendMessageGetSelection(focus, EmGetSelection, out int start, out int end);
        InstallerFeedback.Announce(_owner, start != end ? "All text selected." : "No text to select.", important: true);
        return true;
    }

    private void CopySelection(Target target)
    {
        if (FocusedControl(target.Form) is TextBoxBase box)
        {
            if (!IsFocusedTarget(target, box.Handle)) return;
            string selection = box.SelectedText;
            if (selection.Length == 0) { InstallerFeedback.Announce(box, "No text selected.", important: true); return; }
            try
            {
                if (!IsFocusedTarget(target, box.Handle)) return;
                Clipboard.SetText(selection);
                InstallerFeedback.Announce(box, "Text copied to clipboard.", important: true);
            }
            catch (Exception error) when (error is ExternalException or ThreadStateException)
            {
                InstallerDiagnostics.Current?.Error("Could not copy selected installer text", error);
                InstallerFeedback.Announce(box, "Could not copy text.", important: true);
            }
            return;
        }
        if (target.Form is not null) return;
        nint focus = NativeFocus(target.Window);
        if (!IsFocusedTarget(target, focus) || !IsTextWindow(focus)) return;
        SendMessageGetSelection(focus, EmGetSelection, out int start, out int end);
        if (start == end) { InstallerFeedback.Announce(_owner, "No text selected.", important: true); return; }
        uint sequence = GetClipboardSequenceNumber();
        if (!IsFocusedTarget(target, focus)) return;
        SendMessage(focus, WmCopy, 0, 0);
        // WM_COPY has no success return value. A new clipboard sequence owned
        // by this edit control confirms that it published the selection.
        bool copied = GetClipboardSequenceNumber() != sequence && GetClipboardOwner() == focus;
        InstallerFeedback.Announce(_owner, copied ? "Text copied to clipboard." : "Could not copy text.", important: true);
    }

    private void MoveCaret(Target target, TextBoxBase box, Direction direction, bool byUnit, bool extend)
    {
        if (!IsFocusedTarget(target, box.Handle)) return;
        string text = box.Text;
        bool appendedText = !string.Equals(text, _navigationText, StringComparison.Ordinal) &&
            _navigationText is string previousText && text.StartsWith(previousText, StringComparison.Ordinal);
        if (!string.Equals(text, _navigationText, StringComparison.Ordinal))
        {
            _navigationText = text;
            _textElements = StringInfo.ParseCombiningCharacters(text);
            Array.Resize(ref _textElements, _textElements.Length + 1);
            _textElements[^1] = text.Length;
        }
        int activeCaret = ActiveCaret(box);
        // Appending status text can restore the same range with its endpoints
        // normalized by the Form. Keep the review anchor across that update.
        bool sameSelection = ReferenceEquals(box, _selectionBox) && (activeCaret == _selectionCaret || appendedText) &&
            box.SelectionStart == Math.Min(_selectionAnchor, _selectionCaret) &&
            box.SelectionLength == Math.Abs(_selectionCaret - _selectionAnchor);
        int caret = sameSelection ? _selectionCaret : activeCaret;
        bool hadSelection = box.SelectionLength > 0;
        if (!sameSelection)
        {
            _selectionBox = box;
            _selectionAnchor = caret == box.SelectionStart ? box.SelectionStart + box.SelectionLength : box.SelectionStart;
            _verticalGoalX = null;
        }
        bool vertical = direction is Direction.Up or Direction.Down;
        if (vertical && !byUnit) _verticalGoalX ??= CaretPosition(box, caret).X;
        else _verticalGoalX = null;

        // A local key message lets EDIT retain its native visual-line movement,
        // caret affinity and selection collapse. Modifier flags cannot be
        // encoded in WM_KEYDOWN, so use explicit ranges for trigger modifiers.
        // Reading keyboard state also prevents physical modifiers from changing
        // an unmodified controller arrow. No keyboard state is ever changed.
        if (!byUnit && !extend && !KeyboardModifiersHeld())
        {
            SendLocalKey(target, box.Handle, DirectionKey(direction));
            if (!IsFocusedTarget(target, box.Handle)) { ResetSelection(); return; }
            int nativeCaret = ActiveCaret(box);
            int safeCaret = TextBoundary(nativeCaret, direction is Direction.Right or Direction.Down);
            if (nativeCaret != safeCaret) box.Select(safeCaret, 0);
            _selectionAnchor = _selectionCaret = safeCaret;
            if (!hadSelection) AnnounceCaretUnit(target, box, text, direction, byUnit, caret);
            return;
        }

        int next = caret;
        bool collapseSelection = !byUnit && !extend && box.SelectionLength > 0 && direction is Direction.Left or Direction.Right;
        if (collapseSelection)
        {
            next = direction == Direction.Left ? box.SelectionStart : box.SelectionStart + box.SelectionLength;
        }
        else if (!vertical)
        {
            next = byUnit ? MoveWord(box, text, caret, direction == Direction.Right) : MoveCharacter(caret, direction == Direction.Right);
        }
        else if (byUnit)
        {
            next = MoveParagraph(text, caret, direction == Direction.Down);
        }
        else if (box.Multiline)
        {
            next = MoveVisualLine(box, text, caret, direction == Direction.Down, _verticalGoalX!.Value);
        }
        _selectionCaret = TextBoundary(Math.Clamp(next, 0, text.Length), direction is Direction.Right or Direction.Down);
        if (!extend) _selectionAnchor = _selectionCaret;
        if (!IsFocusedTarget(target, box.Handle)) { ResetSelection(); return; }
        // A signed length passes anchor/active-end order through EM_SETSEL and
        // lets WinForms raise its native UIA selection events. Those events let
        // screen readers report selected/unselected deltas without a duplicate
        // application notification of the growing selection.
        box.Select(_selectionAnchor, _selectionCaret - _selectionAnchor);
        box.ScrollToCaret();
        if (!extend && !hadSelection) AnnounceCaretUnit(target, box, text, direction, byUnit, caret);
        else _lastReviewMessage = null;
    }

    private void AnnounceCaretUnit(Target target, TextBoxBase box, string text, Direction direction, bool byUnit, int previousCaret)
    {
        if (!IsFocusedTarget(target, box.Handle)) return;
        int caret = _selectionCaret;
        string message;
        if (caret == previousCaret)
        {
            if (caret == 0 && direction is Direction.Left or Direction.Up) message = "Start of text.";
            else if (caret == text.Length && direction is Direction.Right or Direction.Down) message = "End of text.";
            else return;
            if (message == _lastReviewMessage) return;
        }
        else if (direction is Direction.Up or Direction.Down)
        {
            int start = caret, end = caret;
            if (byUnit)
            {
                while (start > 0 && text[start - 1] is not ('\r' or '\n')) start--;
                while (end < text.Length && text[end] is not ('\r' or '\n')) end++;
            }
            else
            {
                int line = box.GetLineFromCharIndex(-1);
                start = Math.Max(0, box.GetFirstCharIndexFromLine(line));
                end = box.GetFirstCharIndexFromLine(line + 1);
                if (end < 0) end = text.Length;
                while (end > start && text[end - 1] is '\r' or '\n') end--;
            }
            message = end > start ? text[start..end] : string.Empty;
            if (string.IsNullOrWhiteSpace(message)) message = byUnit ? "Blank paragraph." : "Blank line.";
        }
        else if (caret == text.Length) message = "End of text.";
        else if (byUnit)
        {
            int start = caret, end = caret;
            while (start > 0 && !char.IsWhiteSpace(text, MoveCharacter(start, false))) start = MoveCharacter(start, false);
            while (end < text.Length && !char.IsWhiteSpace(text, end)) end = MoveCharacter(end, true);
            message = end > start ? text[start..end] : CharacterName(text[caret..MoveCharacter(caret, true)]);
        }
        else message = CharacterName(text[caret..MoveCharacter(caret, true)]);
        _lastReviewMessage = message;
        // NVDA's arrow scripts speak the navigation unit; a local window
        // message does not trigger those scripts. Announce collapsed movement
        // through native UIA, without writing reviewed text into diagnostics.
        InstallerFeedback.Announce(box, message, logContent: false, replacePending: true);
    }

    private static string CharacterName(string character) => character switch
    {
        " " => "Space.", "\t" => "Tab.", "\r" or "\n" or "\r\n" => "New line.",
        "." => "Period.", "," => "Comma.", ":" => "Colon.", ";" => "Semicolon.",
        "!" => "Exclamation mark.", "?" => "Question mark.", "-" => "Hyphen.", "_" => "Underscore.",
        "/" => "Slash.", "\\" => "Backslash.", "(" => "Left parenthesis.", ")" => "Right parenthesis.",
        "[" => "Left bracket.", "]" => "Right bracket.", "{" => "Left brace.", "}" => "Right brace.",
        "\"" => "Quote.", "'" => "Apostrophe.", "=" => "Equals.", "+" => "Plus.", "*" => "Asterisk.",
        "@" => "At sign.", "#" => "Number sign.", "$" => "Dollar sign.", "%" => "Percent.",
        "&" => "Ampersand.", "|" => "Vertical bar.", "<" => "Less than.", ">" => "Greater than.",
        "`" => "Backtick.", "~" => "Tilde.", "^" => "Caret.", _ => character
    };

    internal static int ActiveCaret(TextBoxBase box)
    {
        int start = box.SelectionStart, end = start + box.SelectionLength;
        if (start == end) return start;
        if (box is TextBox && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            int caret = (int)SendMessage(box.Handle, EmGetCaretIndex, 0, 0);
            if (caret == start || caret == end) return caret;
        }
        if (GetCaretPos(out Point position) && box.GetPositionFromCharIndex(start) == position) return start;
        return end;
    }

    private static Point CaretPosition(TextBoxBase box, int caret) => ActiveCaret(box) == caret && GetCaretPos(out Point position)
        ? position : box.GetPositionFromCharIndex(caret);

    private int TextBoundary(int position, bool forward)
    {
        int index = Array.BinarySearch(_textElements, position);
        return index >= 0 ? position : _textElements[Math.Clamp(forward ? ~index : ~index - 1, 0, _textElements.Length - 1)];
    }

    private int MoveCharacter(int caret, bool forward)
    {
        int index = Array.BinarySearch(_textElements, caret);
        index = index >= 0 ? index + (forward ? 1 : -1) : forward ? ~index : ~index - 1;
        return _textElements[Math.Clamp(index, 0, _textElements.Length - 1)];
    }

    private int MoveWord(TextBoxBase box, string text, int caret, bool forward)
    {
        if (box is RichTextBox)
            return Math.Clamp((int)SendMessage(box.Handle, EmFindWordBreak, forward ? 5 : 4, caret), 0, text.Length);
        // Plain EDIT treats runs separated by whitespace as words. Keep its
        // treatment of punctuation in file paths, and move by text elements.
        int next = caret;
        if (forward)
        {
            while (next < text.Length && !char.IsWhiteSpace(text, next)) next = MoveCharacter(next, true);
            while (next < text.Length && char.IsWhiteSpace(text, next)) next = MoveCharacter(next, true);
        }
        else
        {
            while (next > 0 && char.IsWhiteSpace(text, MoveCharacter(next, false))) next = MoveCharacter(next, false);
            while (next > 0 && !char.IsWhiteSpace(text, MoveCharacter(next, false))) next = MoveCharacter(next, false);
        }
        return next;
    }

    private static int MoveParagraph(string text, int caret, bool forward)
    {
        if (forward)
        {
            int next = caret;
            while (next < text.Length && text[next] is not ('\r' or '\n')) next++;
            if (next < text.Length && text[next++] == '\r' && next < text.Length && text[next] == '\n') next++;
            return next;
        }
        int start = caret;
        while (start > 0 && text[start - 1] is not ('\r' or '\n')) start--;
        if (start < caret || start == 0) return start;
        start--;
        if (start > 0 && text[start] == '\n' && text[start - 1] == '\r') start--;
        while (start > 0 && text[start - 1] is not ('\r' or '\n')) start--;
        return start;
    }

    private int MoveVisualLine(TextBoxBase box, string text, int caret, bool forward, int goalX)
    {
        int line = box.GetLineFromCharIndex(caret) + (forward ? 1 : -1);
        int start = line >= 0 ? box.GetFirstCharIndexFromLine(line) : -1;
        if (start < 0) return forward ? text.Length : 0;
        int end = box.GetFirstCharIndexFromLine(line + 1);
        if (end < 0) end = text.Length;
        while (end > start && text[end - 1] is '\r' or '\n') end--;
        int best = start;
        long distance = long.MaxValue;
        int first = Array.BinarySearch(_textElements, start);
        if (first < 0) first = ~first;
        // Query 32-bit character positions rather than EM_CHARFROMPOS's
        // truncated 16-bit offsets, so long status logs remain navigable.
        for (int index = first; index < _textElements.Length && _textElements[index] <= end; index++)
        {
            int candidate = _textElements[index];
            bool lineEnd = candidate == end && candidate > start;
            if (!lineEnd && box.GetLineFromCharIndex(candidate) != line) continue;
            Point position = lineEnd && (candidate == text.Length || box.GetLineFromCharIndex(candidate) != line)
                ? LineEndPosition(box, text, candidate) : CharacterPosition(box, candidate);
            long candidateDistance = Math.Abs((long)position.X - goalX);
            if (candidateDistance < distance) { best = candidate; distance = candidateDistance; }
        }
        return best;
    }

    private Point LineEndPosition(TextBoxBase box, string text, int end)
    {
        int previous = MoveCharacter(end, false);
        Point position = CharacterPosition(box, previous);
        int width = TextRenderer.MeasureText(text[previous..end], box.Font, Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine).Width;
        position.X += box.RightToLeft == RightToLeft.Yes ? -width : width;
        return position;
    }

    private static Point CharacterPosition(TextBoxBase box, int index)
    {
        if (box is RichTextBox) return box.GetPositionFromCharIndex(index);
        int position = (int)SendMessage(box.Handle, EmPositionFromChar, index, 0);
        return new((short)position, (short)(position >> 16));
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

    private bool IsFocusedTarget(Target target, nint focus) => focus != 0 && IsStillActive(target) && NativeFocus(target.Window) == focus;

    private static bool KeyboardModifiersHeld() => (GetKeyState((int)Keys.ControlKey) & 0x8000) != 0 ||
        (GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0 || (GetKeyState((int)Keys.Menu) & 0x8000) != 0;

    private void SendLocalKey(Target target, nint focus, Keys key)
    {
        if (!IsFocusedTarget(target, focus)) return;
        uint scan = MapVirtualKey((uint)key, 0);
        int keyData = 1 | ((int)scan << 16) | (1 << 24);
        SendMessage(focus, WmKeyDown, (nint)key, keyData);
        if (IsFocusedTarget(target, focus)) SendMessage(focus, WmKeyUp, (nint)key, unchecked(keyData | (int)0xC0000000));
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
    [DllImport("user32.dll")] private static extern short GetKeyState(int key);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetCaretPos(out Point position);
    [DllImport("user32.dll")] private static extern uint GetClipboardSequenceNumber();
    [DllImport("user32.dll")] private static extern nint GetClipboardOwner();
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetGUIThreadInfo(uint thread, ref GuiThreadInformation information);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsChild(nint parent, nint child);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsIconic(nint window);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsWindowEnabled(nint window);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(nint window, StringBuilder name, int maximum);
    [DllImport("user32.dll", EntryPoint = "SendMessageW")] private static extern nint SendMessage(nint window, int message, nint wParam, nint lParam);
    [DllImport("user32.dll", EntryPoint = "SendMessageW")] private static extern nint SendMessageGetSelection(nint window, int message, out int start, out int end);
    [DllImport("user32.dll", EntryPoint = "PostMessageW")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool PostMessage(nint window, int message, nint wParam, nint lParam);
    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyW")] private static extern uint MapVirtualKey(uint code, uint type);
}
