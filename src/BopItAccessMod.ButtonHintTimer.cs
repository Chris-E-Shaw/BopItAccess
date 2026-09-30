using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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

    // Focus announcements use this instead of QueueSpeech so a None delay
    // places the complete hint in the same speech string as the focused item.
    private void QueueFocusSpeech(string text, bool interrupt = true)
    {
        if (_readButtonHintsEnabled && _buttonHintsDelaySeconds == 0 &&
            _speechEnabled && !_speechSuppressedForBackground)
        {
            try
            {
                (string Key, string Hint)? context = ResolveButtonHintContext();
                if (context != null)
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
            ResetButtonHintTimers(now);
            _nextButtonHintContextProbeAt = 0;
        }

        if (!_readButtonHintsEnabled || !_speechEnabled ||
            _speechSuppressedForBackground ||
            _shutdownRequested.IsSet)
        {
            _buttonHintContextKey = null;
            _cachedButtonHintContext = null;
            ResetButtonHintTimers(now);
            return;
        }

        if (inputDetected)
        {
            _lastButtonHintActivityAt = now;
            _buttonHintInitialSent = false;
            _buttonHintRepeatsSent = 0;
            _buttonHintNoneRepeatArmed = _buttonHintsDelaySeconds == 0 &&
                _repeatButtonHintsCount != 0;
            _nextButtonHintDueAt = now +
                (_buttonHintsDelaySeconds == 0
                    ? _repeatButtonHintsIntervalSeconds
                    : _buttonHintsDelaySeconds) * 1000L;
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
            _buttonHintContextKey = context.Value.Key;
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
        else if (_repeatButtonHintsCount == 0 ||
            (_repeatButtonHintsCount > 0 &&
             _buttonHintRepeatsSent >= _repeatButtonHintsCount))
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
        _nextButtonHintDueAt = _repeatButtonHintsCount == 0 ||
            (_repeatButtonHintsCount > 0 &&
             _buttonHintRepeatsSent >= _repeatButtonHintsCount)
            ? long.MaxValue
            : now + _repeatButtonHintsIntervalSeconds * 1000L;
        WriteStatus($"Button hints for {context.Value.Key}: " +
            (_buttonHintInitialSent && _buttonHintRepeatsSent == 0
                ? "initial" : $"repeat {_buttonHintRepeatsSent}") + ".");
    }

    private void ResetButtonHintTimers(long now)
    {
        _lastButtonHintActivityAt = 0;
        _buttonHintInitialSent = false;
        _buttonHintNoneRepeatArmed = false;
        _buttonHintRepeatsSent = 0;
        _nextButtonHintDueAt = _buttonHintsDelaySeconds > 0
            ? now + _buttonHintsDelaySeconds * 1000L
            : long.MaxValue;
    }

    private static bool IsUserInputActive()
    {
        Keyboard? keyboard = Keyboard.current;
        if (keyboard != null && Pressed(keyboard.anyKey))
            return true;

        Mouse? mouse = Mouse.current;
        if (mouse != null &&
            (Pressed(mouse.leftButton) || Pressed(mouse.rightButton) ||
             Pressed(mouse.middleButton) || Pressed(mouse.forwardButton) ||
             Pressed(mouse.backButton) ||
             mouse.delta.ReadValue().sqrMagnitude > 0.01f ||
             mouse.scroll.ReadValue().sqrMagnitude > 0.01f))
            return true;

        // Check every connected controller so a second player's activity
        // also postpones a reminder in Party and One-on-One.
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (Pressed(gamepad.buttonSouth) || Pressed(gamepad.buttonNorth) ||
                Pressed(gamepad.buttonEast) || Pressed(gamepad.buttonWest) ||
                Pressed(gamepad.startButton) || Pressed(gamepad.selectButton) ||
                Pressed(gamepad.leftShoulder) || Pressed(gamepad.rightShoulder) ||
                Pressed(gamepad.leftTrigger) || Pressed(gamepad.rightTrigger) ||
                Pressed(gamepad.leftStickButton) || Pressed(gamepad.rightStickButton) ||
                Pressed(gamepad.dpad.up) || Pressed(gamepad.dpad.down) ||
                Pressed(gamepad.dpad.left) || Pressed(gamepad.dpad.right) ||
                gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f ||
                gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f)
                return true;
        }

        Joystick? joystick = Joystick.current;
        if (joystick != null &&
            (Pressed(joystick.trigger) ||
             joystick.stick.ReadValue().sqrMagnitude > 0.04f))
            return true;

        return false;
    }

    private static bool Pressed(ButtonControl? button) =>
        button != null && (button.isPressed || button.wasPressedThisFrame ||
            button.wasReleasedThisFrame);
}
