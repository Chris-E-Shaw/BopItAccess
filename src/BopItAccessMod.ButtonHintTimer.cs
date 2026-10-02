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
    private readonly List<InputControl> _assignedButtonHintControls = new();
    private long _nextAssignedButtonHintControlsRefreshAt;
    private long _nextAssignedButtonHintControlsErrorAt;

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
                        focused[^1] != '.' && focused[^1] != '!' &&
                        focused[^1] != '?')
                        focused += ".";
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
        foreach (InputControl control in _assignedButtonHintControls)
        {
            if (control is ButtonControl button && Pressed(button))
                return true;
            if (control is Vector2Control vector)
            {
                // An absolute cursor position is nonzero while the mouse is
                // stationary. Only actual movement counts as activity.
                if (control.device is Mouse mouse &&
                    string.Equals(control.name, "position",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (mouse.delta.ReadValue().sqrMagnitude > 0.01f)
                        return true;
                }
                else if (vector.ReadValue().sqrMagnitude > 0.04f)
                    return true;
            }
            else if (control is AxisControl axis &&
                Math.Abs(axis.ReadValue()) > 0.2f)
                return true;
        }
        return false;
    }

    private void RefreshAssignedButtonHintControls()
    {
        long now = Environment.TickCount64;
        if (now < _nextAssignedButtonHintControlsRefreshAt)
            return;
        _nextAssignedButtonHintControlsRefreshAt = now + 250;
        try
        {
            _assignedButtonHintControls.Clear();
            var seen = new HashSet<InputControl>();
            InputRebindingManager? manager = _controlsRebindingManager;
            if (manager == null)
            {
                MainMenuUIManager? main = _mainMenu;
                manager = main?.controlsPanel?
                    .GetComponentInChildren<InputRebindingManager>(true);
            }

            AddAssignedControls(manager?.playerInput?.actions, seen);
            AddAssignedControls(manager?.inputActions, seen);

            InputSystemUIInputModule? module =
                EventSystem.current?.GetComponent<InputSystemUIInputModule>();
            AddAssignedControls(module?.move?.action, seen);
            AddAssignedControls(module?.submit?.action, seen);
            AddAssignedControls(module?.cancel?.action, seen);
            AddAssignedControls(module?.point?.action, seen);
            AddAssignedControls(module?.leftClick?.action, seen);
            AddAssignedControls(module?.rightClick?.action, seen);
            AddAssignedControls(module?.middleClick?.action, seen);
            AddAssignedControls(module?.scrollWheel?.action, seen);

            AddAssignedControls(_descriptionAction, seen);
            AddAssignedControls(_scoreAction, seen);
            AddAssignedControls(_toggleSpeechAction, seen);
            AddAssignedControls(_speakHintsAction, seen);
            AddAssignedControls(_changeSpeechOutputAction, seen);
        }
        catch (Exception ex)
        {
            _assignedButtonHintControls.Clear();
            if (now >= _nextAssignedButtonHintControlsErrorAt)
            {
                WriteStatus("Assigned button hint input refresh failed: " + ex.Message);
                _nextAssignedButtonHintControlsErrorAt = now + 5000;
            }
        }
    }

    private void AddAssignedControls(InputActionAsset? asset,
        HashSet<InputControl> seen)
    {
        if (asset == null)
            return;
        foreach (InputActionMap map in asset.actionMaps)
            foreach (InputAction action in map.actions)
                AddAssignedControls(action, seen);
    }

    private void AddAssignedControls(InputAction? action,
        HashSet<InputControl> seen)
    {
        if (action == null)
            return;
        foreach (InputControl control in action.controls)
        {
            if (!seen.Add(control))
                continue;
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
            if (explicitlyAssigned)
                _assignedButtonHintControls.Add(control);
            else
                seen.Remove(control);
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
