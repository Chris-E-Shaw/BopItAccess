using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private long _lastButtonHintActivityAt;
    private string? _buttonHintContextKey;
    private bool _buttonHintInitialSent;
    private bool _buttonHintNoneRepeatArmed;
    private int _buttonHintRepeatsSent;
    private long _nextButtonHintDueAt;
    private int _lastButtonHintSettingsSignature = int.MinValue;
    private (string Key, string Hint)? _cachedButtonHintContext;
    private long _nextButtonHintContextProbeAt;
    private long _nextButtonHintErrorAt;
    private bool _manualButtonHintCycleActive;
    private bool _manualButtonHintInputLatched;
    private int _lastButtonHintSelectedObjectId;
    private readonly List<AssignedHintControl> _assignedButtonHintControls = new();
    private long _nextAssignedButtonHintControlsRefreshAt;
    private long _nextAssignedButtonHintControlsErrorAt;
    private int _assignedButtonHintSourceSignature = int.MinValue;
    private int _lastAssignedButtonHintControlCounts = int.MinValue;
    private int _lastAssignedButtonHintControlsProbeFrame = -1;
    private int _lastAssignedButtonHintControlsProbeModActionSignature;
    private uint _assignedButtonHintControlsGeneration;
    private uint _lastAssignedButtonHintControlsProbeGeneration;
    private readonly InputAction?[] _immediateAssignedButtonHintActions = new InputAction?[13];

    // The slider describes total readings, including the first inline or
    // delayed hint. For example, 2X permits one additional timed reading.
    // -1 means indefinitely, and Off (0) permits no additional readings.
    private int MaximumButtonHintRepeats => _repeatButtonHintsCount > 0
        ? _repeatButtonHintsCount - 1 : _repeatButtonHintsCount;

    // Focus announcements use this instead of QueueSpeech so a None delay
    // places the complete hint in the same speech string as the focused item.
    private void QueueFocusSpeech(string text, bool interrupt = true)
    {
        // A newly spoken focus is evidence of real menu navigation even when
        // Unity's binding-control scan misses the key or controller input.
        // This also cancels a manual-hint repeat cycle after the player moves.
        RecordButtonHintUiActivity(Environment.TickCount64);
        if (_readButtonHintsEnabled && _buttonHintsDelaySeconds == 0 &&
            _speechEnabled && !_speechSuppressedForBackground)
        {
            try
            {
                (string Key, string Hint)? context = ResolveButtonHintContext();
                if (context != null && (!_manualButtonHintCycleActive ||
                    !string.Equals(_buttonHintContextKey, context.Value.Key,
                        StringComparison.Ordinal)))
                {
                    string focused = text.TrimEnd();
                    if (focused.Length > 0 &&
                        focused[^1] is not ('.' or '!' or '?' or '。' or
                            '！' or '？'))
                        focused += L(".");
                    text = focused + " " + context.Value.Hint;
                }
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextButtonHintErrorAt)
                {
                    WriteStatus("Inline button hint check failed: " + ex.Message);
                    _nextButtonHintErrorAt = now + 5000;
                }
            }
        }

        QueueSpeech(text, interrupt);
        _nextButtonHintContextProbeAt = 0;
    }

    private void UpdateRepeatButtonHints()
    {
        long now = Environment.TickCount64;
        bool inputDetected = false;
        try
        {
            inputDetected = IsUserInputActive();
        }
        catch (Exception ex)
        {
            if (now >= _nextButtonHintErrorAt)
            {
                WriteStatus("Repeat button hints input check failed: " + ex.Message);
                _nextButtonHintErrorAt = now + 5000;
            }
            ResetButtonHintTimers(now);
            return;
        }

        int signature = HashCode.Combine(_readButtonHintsEnabled,
            _buttonHintsDelaySeconds, _repeatButtonHintsCount,
            _repeatButtonHintsIntervalSeconds, _hintsType,
            EffectiveHintDevice);
        if (signature != _lastButtonHintSettingsSignature)
        {
            _lastButtonHintSettingsSignature = signature;
            // A changed hint setting or active input device starts a fresh
            // cycle. Keep a just-requested manual cycle when the Speak Hints
            // key itself changes Automatic's active input device.
            if (!_manualButtonHintCycleActive)
                RecordButtonHintUiActivity(now);
            _nextButtonHintContextProbeAt = 0;
        }

        if (!_speechEnabled || _speechSuppressedForBackground ||
            _shutdownRequested.IsSet)
        {
            _buttonHintContextKey = null;
            _cachedButtonHintContext = null;
            ResetButtonHintTimers(now);
            return;
        }

        if (_manualButtonHintInputLatched)
        {
            // A held Speak Hints binding, including its release frame, must
            // not restart the automatic first-hint delay. Repeats begin after
            // input becomes idle again.
            if (inputDetected || IsSpeakHintsHeld())
            {
                _lastButtonHintActivityAt = now;
                if (MaximumButtonHintRepeats != 0)
                    _nextButtonHintDueAt = now +
                        _repeatButtonHintsIntervalSeconds * 1000L;
            }
            else
                _manualButtonHintInputLatched = false;
        }
        else if (inputDetected)
        {
            RecordButtonHintUiActivity(now);
        }

        if (now >= _nextButtonHintContextProbeAt)
        {
            _nextButtonHintContextProbeAt = now + 250;
            try
            {
                _cachedButtonHintContext = ResolveButtonHintContext();
            }
            catch (Exception ex)
            {
                if (now >= _nextButtonHintErrorAt)
                {
                    WriteStatus("Repeat button hints screen check failed: " + ex.Message);
                    _nextButtonHintErrorAt = now + 5000;
                }
                _cachedButtonHintContext = null;
            }
        }

        (string Key, string Hint)? context = _cachedButtonHintContext;
        if (context == null)
        {
            _buttonHintContextKey = null;
            ResetButtonHintTimers(now);
            return;
        }

        if (!string.Equals(_buttonHintContextKey, context.Value.Key,
            StringComparison.Ordinal))
        {
            _manualButtonHintCycleActive = false;
            _manualButtonHintInputLatched = false;
            _buttonHintContextKey = context.Value.Key;
            _lastButtonHintSelectedObjectId =
                EventSystem.current?.currentSelectedGameObject?.GetInstanceID() ?? 0;
            _buttonHintInitialSent = false;
            _buttonHintRepeatsSent = 0;
            if (_buttonHintsDelaySeconds > 0)
            {
                _buttonHintNoneRepeatArmed = false;
                _nextButtonHintDueAt = now + _buttonHintsDelaySeconds * 1000L;
            }
            else
            {
                _buttonHintNoneRepeatArmed = _repeatButtonHintsCount != 0 &&
                    _lastButtonHintActivityAt != 0 &&
                    now - _lastButtonHintActivityAt <
                        _repeatButtonHintsIntervalSeconds * 1000L;
                _nextButtonHintDueAt = _buttonHintNoneRepeatArmed
                    ? _lastButtonHintActivityAt +
                        _repeatButtonHintsIntervalSeconds * 1000L
                    : long.MaxValue;
            }
            return;
        }

        // The Input System action-control list can omit a navigation path
        // even while the EventSystem moves focus. Catch that change before
        // dispatching a due hint in this update; QueueFocusSpeech covers a
        // change made later in the frame.
        int selectedId =
            EventSystem.current?.currentSelectedGameObject?.GetInstanceID() ?? 0;
        if (selectedId != 0 && selectedId != _lastButtonHintSelectedObjectId)
            RecordButtonHintUiActivity(now);

        // AUTO-SPEAK controls the initial hint. A manual request can still
        // run the selected number of repeats while this setting is Off.
        if (!_readButtonHintsEnabled && !_manualButtonHintCycleActive)
        {
            ResetButtonHintTimers(now);
            return;
        }

        if (now < _nextButtonHintDueAt)
            return;

        if (_buttonHintsDelaySeconds == 0 && !_buttonHintNoneRepeatArmed)
            return;

        bool firstTimedHint = _buttonHintsDelaySeconds > 0 &&
            !_buttonHintInitialSent;
        if (firstTimedHint)
        {
            _buttonHintInitialSent = true;
            _buttonHintRepeatsSent = 0;
        }
        else if (MaximumButtonHintRepeats == 0 ||
            (MaximumButtonHintRepeats > 0 &&
             _buttonHintRepeatsSent >= MaximumButtonHintRepeats))
        {
            _nextButtonHintDueAt = long.MaxValue;
            return;
        }
        else
        {
            _buttonHintRepeatsSent++;
        }

        // The deliberately short five-second delay may interrupt ordinary
        // menu speech. Never let it cut off a protected score or description.
        // All longer delays and subsequent repeats join the speech queue.
        bool protectedOutput;
        lock (_speechLock)
            protectedOutput = _pendingPrioritySpeech != null ||
                _pendingPriorityFollowUpSpeech != null ||
                _pendingSpeechIsDescription || _descriptionSpeechMayBeActive;
        bool mayInterrupt = firstTimedHint && _buttonHintsDelaySeconds == 5 &&
            !protectedOutput &&
            now >= Volatile.Read(ref _gameOverScoreDispatchPendingUntil) &&
            now >= Volatile.Read(ref _gameOverScoreSpeechProtectedUntil);
        if (mayInterrupt)
            QueueSpeech(context.Value.Hint);
        else
            QueueSequentialSpeech(context.Value.Hint);
        _nextButtonHintDueAt = MaximumButtonHintRepeats == 0 ||
            (MaximumButtonHintRepeats > 0 &&
             _buttonHintRepeatsSent >= MaximumButtonHintRepeats)
            ? long.MaxValue
            : now + _repeatButtonHintsIntervalSeconds * 1000L;
        WriteStatus($"Button hints for {context.Value.Key}: " +
            (_buttonHintInitialSent && _buttonHintRepeatsSent == 0
                ? "initial" : $"repeat {_buttonHintRepeatsSent}") + ".");
    }

    private void UpdateSpeakHintsOnDemand()
    {
        bool available = _speechEnabled && !_speechSuppressedForBackground &&
            !_shutdownRequested.IsSet && UnityEngine.Application.isFocused;
        if (available && _cachedButtonHintContext == null)
        {
            // Make the shortcut available as soon as a new menu appears,
            // including when automatic hints are disabled. The ordinary
            // timer still refreshes established contexts every 250 ms.
            try
            {
                _cachedButtonHintContext = ResolveButtonHintContext();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextButtonHintErrorAt)
                {
                    WriteStatus("Speak Hints screen check failed: " + ex.Message);
                    _nextButtonHintErrorAt = now + 5000;
                }
            }
        }
        available &= _cachedButtonHintContext != null;
        if (!WasSpeakHintsPressed(available))
            return;

        try
        {
            // Resolve again on the press: focus may have changed since the
            // timer's last low-frequency screen probe.
            (string Key, string Hint)? context = ResolveButtonHintContext();
            if (context == null)
                return;

            string requestedHint = context.Value.Hint;
            if (string.Equals(context.Value.Key, "SongSelect",
                    StringComparison.Ordinal))
            {
                string? tutorial = ReadPreRoundTutorialSummary();
                if (tutorial != null)
                    requestedHint += " " + tutorial;
            }

            long now = Environment.TickCount64;
            bool protectedOutput;
            lock (_speechLock)
                protectedOutput = _pendingPrioritySpeech != null ||
                    _pendingPriorityFollowUpSpeech != null ||
                    _pendingSpeechIsDescription || _descriptionSpeechMayBeActive;
            if (protectedOutput ||
                now < Volatile.Read(ref _gameOverScoreDispatchPendingUntil) ||
                now < Volatile.Read(ref _gameOverScoreSpeechProtectedUntil))
                QueueSequentialSpeech(requestedHint);
            else
                QueueSpeech(requestedHint);

            // The manual request supplies the first hint for this cycle.
            // It replaces the pending automatic hint, but still permits the
            // configured number of later repeats.
            _manualButtonHintCycleActive = true;
            _manualButtonHintInputLatched = true;
            _buttonHintContextKey = context.Value.Key;
            _cachedButtonHintContext = context;
            _lastButtonHintSelectedObjectId =
                EventSystem.current?.currentSelectedGameObject?.GetInstanceID() ?? 0;
            _nextButtonHintContextProbeAt = now + 250;
            _lastButtonHintActivityAt = now;
            _buttonHintInitialSent = true;
            _buttonHintRepeatsSent = 0;
            _buttonHintNoneRepeatArmed = _buttonHintsDelaySeconds == 0 &&
                MaximumButtonHintRepeats != 0;
            _nextButtonHintDueAt = MaximumButtonHintRepeats == 0
                ? long.MaxValue
                : now + _repeatButtonHintsIntervalSeconds * 1000L;
            WriteStatus("Button hints for " + context.Value.Key + ": manual.");
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextButtonHintErrorAt)
            {
                WriteStatus("Manual button hints failed: " + ex.Message);
                _nextButtonHintErrorAt = now + 5000;
            }
        }
    }

    private void ResetButtonHintTimers(long now)
    {
        _manualButtonHintCycleActive = false;
        _manualButtonHintInputLatched = false;
        _lastButtonHintActivityAt = 0;
        _buttonHintInitialSent = false;
        _buttonHintNoneRepeatArmed = false;
        _buttonHintRepeatsSent = 0;
        _nextButtonHintDueAt = _buttonHintsDelaySeconds > 0
            ? now + _buttonHintsDelaySeconds * 1000L
            : long.MaxValue;
    }

    // Call only for an observed game UI change, not for every speech string:
    // automatic result, score, and reminder speech must not count as input.
    private void RecordButtonHintUiActivity(long now)
    {
        _manualButtonHintCycleActive = false;
        _manualButtonHintInputLatched = false;
        _lastButtonHintActivityAt = now;
        _buttonHintInitialSent = false;
        _buttonHintRepeatsSent = 0;
        _buttonHintNoneRepeatArmed = _buttonHintsDelaySeconds == 0 &&
            _repeatButtonHintsCount != 0;
        _nextButtonHintDueAt = _buttonHintsDelaySeconds > 0
            ? now + _buttonHintsDelaySeconds * 1000L
            : _buttonHintNoneRepeatArmed
                ? now + _repeatButtonHintsIntervalSeconds * 1000L
                : long.MaxValue;
        int selectedId =
            EventSystem.current?.currentSelectedGameObject?.GetInstanceID() ?? 0;
        if (selectedId != 0)
            _lastButtonHintSelectedObjectId = selectedId;
    }

    private bool IsUserInputActive()
    {
        RefreshAssignedButtonHintControls();
        foreach (AssignedHintControl control in _assignedButtonHintControls)
        {
            if (!control.IsAvailable)
                continue;
            if (control.Button != null && Pressed(control.Button))
                return true;
            if (control.MousePosition)
            {
                // An absolute cursor position is nonzero while the mouse is
                // stationary. Only actual movement counts as activity.
                if (control.Mouse!.delta.ReadValue().sqrMagnitude > 0.01f)
                    return true;
            }
            else if (control.Button == null && control.ReadMagnitudeSquared() > 0.16f)
                return true;
        }
        return false;
    }

    private readonly Dictionary<IntPtr, AssignedHintControl> _assignedHintControlSet = new();

    private sealed class AssignedHintControl
    {
        internal readonly InputControl Control;
        internal readonly InputDevice Device;
        internal readonly HintDevice DeviceType;
        internal readonly ButtonControl? Button;
        internal readonly Vector2Control? Vector;
        internal readonly AxisControl? Axis;
        internal readonly Mouse? Mouse;
        internal readonly bool MousePosition;
        internal readonly List<InputAction> Actions = new();
        internal IntPtr Pointer => Control.Pointer;
        internal bool IsAvailable
        {
            get
            {
                if (!Device.added || !Device.enabled)
                    return false;
                for (int index = 0; index < Actions.Count; index++)
                    if (Actions[index].enabled)
                        return true;
                return false;
            }
        }

        internal AssignedHintControl(InputControl control, InputDevice device,
            HintDevice type, Mouse? mouse)
        {
            Control = control;
            Device = device;
            DeviceType = type;
            // action.controls and control.device return base IL2CPP wrappers.
            // Managed `is` checks cannot establish their native derived type.
            Button = control.TryCast<ButtonControl>();
            Vector = Button == null ? control.TryCast<Vector2Control>() : null;
            Axis = Button == null && Vector == null ? control.TryCast<AxisControl>() : null;
            Mouse = mouse;
            MousePosition = mouse != null && string.Equals(control.name, "position", StringComparison.OrdinalIgnoreCase);
        }

        internal float ReadMagnitudeSquared()
        {
            if (Vector != null) return Vector.ReadValue().sqrMagnitude;
            float value = Axis?.ReadValue() ?? 0f;
            return value * value;
        }

        internal float ReadPreviousMagnitudeSquared()
        {
            if (Vector != null) return Vector.ReadValueFromPreviousFrame().sqrMagnitude;
            float value = Axis?.ReadValueFromPreviousFrame() ?? 0f;
            return value * value;
        }

        internal void AddAction(InputAction action)
        {
            for (int index = 0; index < Actions.Count; index++)
                if (Actions[index].Pointer == action.Pointer)
                    return;
            Actions.Add(action);
        }
    }

    private void InvalidateAssignedButtonHintControls()
    {
        _nextAssignedButtonHintControlsRefreshAt = 0;
        _assignedButtonHintControlsGeneration = unchecked(_assignedButtonHintControlsGeneration + 1);
    }

    private static int HintActionStateSignature(InputAction? action) =>
        HashCode.Combine(action?.Pointer ?? IntPtr.Zero, action?.enabled ?? false);

    private void RefreshAssignedButtonHintControls()
    {
        int frame = Time.frameCount;
        uint generation = _assignedButtonHintControlsGeneration;
        long now = Environment.TickCount64;
        try
        {
            int modActionSignature = HashCode.Combine(
                HintActionStateSignature(_descriptionAction), HintActionStateSignature(_scoreAction),
                HintActionStateSignature(_toggleSpeechAction), HintActionStateSignature(_speakHintsAction),
                HintActionStateSignature(_changeSpeechOutputAction));
            // Update and LateUpdate share the successful probe. Binding
            // changes advance the generation; LateUpdate may also enable or
            // create a mod action before checking whether hints are due.
            if (frame == _lastAssignedButtonHintControlsProbeFrame &&
                generation == _lastAssignedButtonHintControlsProbeGeneration &&
                modActionSignature == _lastAssignedButtonHintControlsProbeModActionSignature)
                return;
            InputSystemUIInputModule? module =
                EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            int signature = HashCode.Combine(module?.Pointer ?? IntPtr.Zero,
                PlayerInput.all.Count, modActionSignature);
            // Device identity, rather than just device count, detects a
            // replacement controller and refreshes before its first input.
            foreach (InputDevice device in InputSystem.devices)
                signature = HashCode.Combine(signature, device.deviceId, device.enabled);
            foreach (PlayerInput player in PlayerInput.all)
            {
                InputActionMap? map = player.currentActionMap;
                signature = HashCode.Combine(signature, player.Pointer,
                    player.actions?.Pointer ?? IntPtr.Zero, map?.Pointer ?? IntPtr.Zero, map?.enabled ?? false);
            }
            InputAction?[] immediateActions = _immediateAssignedButtonHintActions;
            immediateActions[0] = module?.move?.action;
            immediateActions[1] = module?.submit?.action;
            immediateActions[2] = module?.cancel?.action;
            immediateActions[3] = module?.point?.action;
            immediateActions[4] = module?.leftClick?.action;
            immediateActions[5] = module?.rightClick?.action;
            immediateActions[6] = module?.middleClick?.action;
            immediateActions[7] = module?.scrollWheel?.action;
            immediateActions[8] = _descriptionAction;
            immediateActions[9] = _scoreAction;
            immediateActions[10] = _toggleSpeechAction;
            immediateActions[11] = _speakHintsAction;
            immediateActions[12] = _changeSpeechOutputAction;
            for (int index = 0; index < 8; index++)
                signature = HashCode.Combine(signature, HintActionStateSignature(immediateActions[index]));
            if (now < _nextAssignedButtonHintControlsRefreshAt && signature == _assignedButtonHintSourceSignature)
            {
                _lastAssignedButtonHintControlsProbeFrame = frame;
                _lastAssignedButtonHintControlsProbeGeneration = generation;
                _lastAssignedButtonHintControlsProbeModActionSignature = modActionSignature;
                return;
            }
            _assignedButtonHintSourceSignature = signature;
            _nextAssignedButtonHintControlsRefreshAt = now + 250;
            _assignedButtonHintControls.Clear();
            Dictionary<IntPtr, AssignedHintControl> seen = _assignedHintControlSet;
            seen.Clear();
            InputRebindingManager? manager = _controlsRebindingManager;
            if (manager == null)
            {
                MainMenuUIManager? main = _mainMenu;
                manager = main?.controlsPanel?
                    .GetComponentInChildren<InputRebindingManager>(true);
            }

            AddAssignedControls(manager?.playerInput?.actions, seen);
            AddAssignedControls(manager?.inputActions, seen);
            // PlayerInput owns live action copies with device pairing and
            // saved overrides; Controls may be absent in gameplay scenes.
            foreach (PlayerInput player in PlayerInput.all)
                AddAssignedControls(player.actions, seen);
            foreach (InputAction? action in immediateActions)
                AddAssignedControls(action, seen);
            foreach (IntPtr stale in _hintAnalogWasActive.Keys
                         .Where(pointer => !seen.ContainsKey(pointer)).ToArray())
                _hintAnalogWasActive.Remove(stale);
            int keyboard = _assignedButtonHintControls.Count(control => control.DeviceType == HintDevice.Keyboard);
            int controller = _assignedButtonHintControls.Count - keyboard;
            int counts = HashCode.Combine(keyboard, controller);
            if (counts != _lastAssignedButtonHintControlCounts)
            {
                _lastAssignedButtonHintControlCounts = counts;
                WriteStatus($"Assigned hint inputs: {keyboard} keyboard/mouse control(s), {controller} controller control(s).");
            }
            _lastAssignedButtonHintControlsProbeFrame = frame;
            _lastAssignedButtonHintControlsProbeGeneration = generation;
            _lastAssignedButtonHintControlsProbeModActionSignature = modActionSignature;
        }
        catch (Exception ex)
        {
            _assignedButtonHintControls.Clear();
            _nextAssignedButtonHintControlsRefreshAt = now + 250;
            if (now >= _nextAssignedButtonHintControlsErrorAt)
            {
                WriteStatus("Assigned button hint input refresh failed: " + ex.Message);
                _nextAssignedButtonHintControlsErrorAt = now + 5000;
            }
        }
    }

    private void AddAssignedControls(InputActionAsset? asset,
        Dictionary<IntPtr, AssignedHintControl> seen)
    {
        if (asset == null)
            return;
        foreach (InputActionMap map in asset.actionMaps)
            foreach (InputAction action in map.actions)
                AddAssignedControls(action, seen);
    }

    private void AddAssignedControls(InputAction? action,
        Dictionary<IntPtr, AssignedHintControl> seen)
    {
        if (action == null || !action.enabled)
            return;
        foreach (InputControl control in action.controls)
        {
            bool explicitlyAssigned = false;
            foreach (InputBinding binding in action.bindings)
            {
                if (binding.isComposite ||
                    !IsSpecificHintActivityPath(binding.effectivePath))
                    continue;
                if (InputControlPath.Matches(binding.effectivePath, control))
                {
                    explicitlyAssigned = true;
                    break;
                }
            }
            if (!explicitlyAssigned)
                continue;
            IntPtr pointer = control.Pointer;
            if (seen.TryGetValue(pointer, out AssignedHintControl? assigned))
            {
                assigned.AddAction(action);
                continue;
            }
            InputDevice device = control.device;
            Mouse? mouse = device.TryCast<Mouse>();
            HintDevice? type = mouse != null || device.TryCast<Keyboard>() != null ? HintDevice.Keyboard :
                device.TryCast<Gamepad>() != null || device.TryCast<Joystick>() != null ? HintDevice.Controller : null;
            if (type == null)
                continue;
            assigned = new AssignedHintControl(control, device, type.Value, mouse);
            if (assigned.Button == null && assigned.Vector == null && assigned.Axis == null)
                continue;
            assigned.AddAction(action);
            seen.Add(pointer, assigned);
            _assignedButtonHintControls.Add(assigned);
        }
    }

    private static bool IsSpecificHintActivityPath(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !path.Contains('*') &&
        !path.Contains("/anyKey", StringComparison.OrdinalIgnoreCase) &&
        !path.Contains("/anyButton", StringComparison.OrdinalIgnoreCase);

    private static bool Pressed(ButtonControl? button) =>
        button != null && (button.isPressed || button.wasPressedThisFrame ||
            button.wasReleasedThisFrame);
}
