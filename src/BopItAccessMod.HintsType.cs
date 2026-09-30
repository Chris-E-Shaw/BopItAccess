using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string HintsTypePreferenceKey = "BopItAccess.HintsType";
    private static readonly string[] HintsTypes =
        { "Automatic", "Keyboard", "Controller", "Both" };
    private string _hintsType = "Automatic";
    private HintDevice _lastHintInputDevice = HintDevice.Keyboard;
    private readonly Dictionary<int, bool> _hintStickWasActive = new();

    private enum HintDevice { Keyboard, Controller, Both }

    private HintDevice EffectiveHintDevice => _hintsType switch
    {
        "Keyboard" => HintDevice.Keyboard,
        "Controller" => HintDevice.Controller,
        "Both" => HintDevice.Both,
        _ => _lastHintInputDevice
    };

    private void InitializeHintsTypePreferenceOnMainThread()
    {
        try
        {
            string saved = PlayerPrefs.GetString(HintsTypePreferenceKey, "Automatic");
            _hintsType = Array.Find(HintsTypes, option =>
                string.Equals(option, saved, StringComparison.OrdinalIgnoreCase)) ??
                "Automatic";
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read hints type preference: " + ex.Message);
        }
        WriteStatus("Hints type: " + _hintsType + ".");
    }

    private void SetHintsTypeFromMenu(string type)
    {
        if (!HintsTypes.Contains(type, StringComparer.OrdinalIgnoreCase) ||
            string.Equals(_hintsType, type, StringComparison.OrdinalIgnoreCase))
            return;
        _hintsType = type;
        SaveSpeechPreference(HintsTypePreferenceKey, type);
        _cachedButtonHintContext = null;
        _nextButtonHintContextProbeAt = 0;
        WriteStatus("Hints type changed to " + type + ".");
    }

    // This runs before focus and timed hint announcements. A held stick or key
    // must not repeatedly steal Automatic mode from a newly touched device.
    private void UpdateHintInputDevice()
    {
        if (!Application.isFocused)
            return;

        bool keyboardInput = Keyboard.current?.anyKey.wasPressedThisFrame == true;
        Mouse? mouse = Mouse.current;
        keyboardInput |= mouse != null &&
            (NewPress(mouse.leftButton) || NewPress(mouse.rightButton) ||
             NewPress(mouse.middleButton) || NewPress(mouse.forwardButton) ||
             NewPress(mouse.backButton) ||
             mouse.delta.ReadValue().sqrMagnitude > 1f ||
             mouse.scroll.ReadValue().sqrMagnitude > 0.01f);

        bool controllerInput = false;
        foreach (Gamepad gamepad in Gamepad.all)
        {
            bool button = NewPress(gamepad.buttonSouth) ||
                NewPress(gamepad.buttonNorth) || NewPress(gamepad.buttonEast) ||
                NewPress(gamepad.buttonWest) || NewPress(gamepad.startButton) ||
                NewPress(gamepad.selectButton) || NewPress(gamepad.leftShoulder) ||
                NewPress(gamepad.rightShoulder) || NewPress(gamepad.leftTrigger) ||
                NewPress(gamepad.rightTrigger) || NewPress(gamepad.leftStickButton) ||
                NewPress(gamepad.rightStickButton) || NewPress(gamepad.dpad.up) ||
                NewPress(gamepad.dpad.down) || NewPress(gamepad.dpad.left) ||
                NewPress(gamepad.dpad.right);
            bool stickActive = gamepad.leftStick.ReadValue().sqrMagnitude > 0.16f ||
                gamepad.rightStick.ReadValue().sqrMagnitude > 0.16f;
            bool wasActive = _hintStickWasActive.TryGetValue(gamepad.deviceId,
                out bool previous) && previous;
            _hintStickWasActive[gamepad.deviceId] = stickActive;
            controllerInput |= button || (stickActive && !wasActive);
        }

        Joystick? joystick = Joystick.current;
        if (joystick != null)
        {
            bool stickActive = joystick.stick.ReadValue().sqrMagnitude > 0.16f;
            bool wasActive = _hintStickWasActive.TryGetValue(joystick.deviceId,
                out bool previous) && previous;
            _hintStickWasActive[joystick.deviceId] = stickActive;
            controllerInput |= stickActive && !wasActive;
            foreach (InputControl control in joystick.allControls)
                if (control is ButtonControl button && NewPress(button))
                {
                    controllerInput = true;
                    break;
                }
        }

        HintDevice? touched = controllerInput == keyboardInput ? null :
            controllerInput ? HintDevice.Controller : HintDevice.Keyboard;
        if (touched == null || touched == _lastHintInputDevice)
            return;
        _lastHintInputDevice = touched.Value;
        if (_hintsType == "Automatic")
        {
            _cachedButtonHintContext = null;
            _nextButtonHintContextProbeAt = 0;
            WriteStatus("Automatic hints now use " +
                (_lastHintInputDevice == HintDevice.Controller ? "controller" : "keyboard") +
                " inputs.");
        }
    }

    private static bool NewPress(ButtonControl? button) =>
        button?.wasPressedThisFrame == true;

    private string FormatHintPress(string keyboard, string controller,
        string purpose, string controllerVerb = "press")
    {
        // The binding comes first so a listener hears the control before its
        // effect. The hint's purpose then explains what that input does.
        string action = purpose.Trim().TrimEnd('.');
        return EffectiveHintDevice switch
        {
            HintDevice.Keyboard => $"{keyboard}, {action}.",
            HintDevice.Controller => $"{controller}, {action}.",
            _ => $"{keyboard} on keyboard; {controller} on controller, {action}."
        };
    }

    private string FormatHintUse(string keyboard, string controller, string purpose)
    {
        string action = purpose.Trim().TrimEnd('.');
        return EffectiveHintDevice switch
        {
            HintDevice.Keyboard => $"{keyboard}, {action}.",
            HintDevice.Controller => $"{controller}, {action}.",
            _ => $"{keyboard} on keyboard; {controller} on controller, {action}."
        };
    }
}
