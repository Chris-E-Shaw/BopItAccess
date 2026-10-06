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
    private readonly Dictionary<InputControl, bool> _hintAnalogWasActive = new();

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

        // Device choice and inactivity timing use the same assigned inputs.
        // An unused key such as Control affects neither one.
        RefreshAssignedButtonHintControls();
        bool keyboardInput = false;
        bool controllerInput = false;
        foreach (InputControl control in _assignedButtonHintControls)
        {
            bool controlTouched = control is ButtonControl button
                ? NewPress(button)
                : WasAssignedAnalogControlNewlyUsed(control);
            if (!controlTouched)
                continue;
            if (control.device is Keyboard or Mouse)
                keyboardInput = true;
            else if (control.device is Gamepad or Joystick)
                controllerInput = true;
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

    private bool WasAssignedAnalogControlNewlyUsed(InputControl control)
    {
        if (control.device is Mouse mouse &&
            string.Equals(control.name, "position",
                StringComparison.OrdinalIgnoreCase))
            return mouse.delta.ReadValue().sqrMagnitude > 1f;

        bool active = control switch
        {
            Vector2Control vector => vector.ReadValue().sqrMagnitude > 0.16f,
            AxisControl axis => Math.Abs(axis.ReadValue()) > 0.4f,
            _ => false
        };
        bool previous = _hintAnalogWasActive.TryGetValue(control, out bool wasActive) &&
            wasActive;
        _hintAnalogWasActive[control] = active;
        return active && !previous;
    }

    private string FormatHintPress(string keyboard, string controller,
        string purpose)
    {
        // The binding comes first so a listener hears the control before its
        // effect. The hint's purpose then explains what that input does.
        string action = L(purpose.Trim().TrimEnd('.'));
        string keyboardName = LocalizeBindingDisplay(keyboard);
        string controllerName = LocalizeBindingDisplay(controller);
        return EffectiveHintDevice switch
        {
            HintDevice.Keyboard => LF("{0}, {1}.",
                keyboardName, action),
            HintDevice.Controller => LF("{0}, {1}.",
                controllerName, action),
            _ => LF("{0} on keyboard; {1} on controller, {2}.",
                keyboardName, controllerName, action)
        };
    }

    private string FormatHintUse(string keyboard, string controller, string purpose) =>
        FormatHintPress(keyboard, controller, purpose);
}
