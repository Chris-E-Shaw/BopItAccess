using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private long _lastButtonHintActivityAt;
    private string? _buttonHintContextKey;
    private bool _buttonHintSpokenSinceActivity;
    private int _lastButtonHintIntervalSeconds = -1;
    private (string Key, string Hint)? _cachedButtonHintContext;
    private long _nextButtonHintContextProbeAt;
    private long _nextButtonHintErrorAt;

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
            _lastButtonHintActivityAt = now;
            return;
        }

        if (inputDetected)
        {
            _lastButtonHintActivityAt = now;
            _buttonHintSpokenSinceActivity = false;
        }

        int interval = _repeatButtonHintsSeconds;
        if (interval != _lastButtonHintIntervalSeconds)
        {
            _lastButtonHintIntervalSeconds = interval;
            _lastButtonHintActivityAt = now;
            _buttonHintSpokenSinceActivity = false;
            _nextButtonHintContextProbeAt = 0;
        }

        if (interval == 0 || !_speechEnabled || _shutdownRequested.IsSet)
        {
            _buttonHintContextKey = null;
            _cachedButtonHintContext = null;
            _lastButtonHintActivityAt = now;
            _buttonHintSpokenSinceActivity = false;
            return;
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
            _lastButtonHintActivityAt = now;
            _buttonHintSpokenSinceActivity = false;
            return;
        }

        if (!string.Equals(_buttonHintContextKey, context.Value.Key,
            StringComparison.Ordinal))
        {
            _buttonHintContextKey = context.Value.Key;
            _lastButtonHintActivityAt = now;
            _buttonHintSpokenSinceActivity = false;
            return;
        }

        if (_buttonHintSpokenSinceActivity ||
            now - _lastButtonHintActivityAt < interval * 1000L)
            return;

        // Give one reminder per idle stretch. A long score, description, or
        // credits announcement can then finish without a reminder backlog.
        // New input or a different screen arms the timer again.
        QueueSequentialSpeech(context.Value.Hint);
        _buttonHintSpokenSinceActivity = true;
        WriteStatus($"Repeated button hints for {context.Value.Key}.");
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
