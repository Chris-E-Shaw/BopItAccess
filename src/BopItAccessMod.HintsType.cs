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
    private readonly Dictionary<IntPtr, bool> _hintAnalogWasActive = new();

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
        foreach (AssignedHintControl control in _assignedButtonHintControls)
        {
            if (!control.IsAvailable)
                continue;
            bool controlTouched = control.Button != null
                ? NewPress(control.Button)
                : WasAssignedAnalogControlNewlyUsed(control);
            if (!controlTouched)
                continue;
            if (control.DeviceType == HintDevice.Keyboard)
                keyboardInput = true;
            else if (control.DeviceType == HintDevice.Controller)
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

    private bool WasAssignedAnalogControlNewlyUsed(AssignedHintControl control)
    {
        if (control.MousePosition)
            return control.Mouse!.delta.ReadValue().sqrMagnitude > 1f;

        float magnitude = control.ReadMagnitudeSquared();
        // Preserve the edge latch by native identity, not wrapper identity.
        // First observation uses previous-frame state so a held control on a
        // newly enabled action/device does not look like a fresh touch.
        bool previous = _hintAnalogWasActive.TryGetValue(control.Pointer, out bool wasActive)
            ? wasActive : control.ReadPreviousMagnitudeSquared() > 0.16f;
        // A neutral range prevents stick noise near the press threshold from
        // stealing Automatic mode back from a newly touched keyboard.
        bool active = magnitude > (previous ? 0.0625f : 0.16f);
        _hintAnalogWasActive[control.Pointer] = active;
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
        string hint = EffectiveHintDevice switch
        {
            HintDevice.Keyboard => LF("{0}, {1}.",
                keyboardName, action),
            HintDevice.Controller => LF("{0}, {1}.",
                controllerName, action),
            _ => LF("{0} on keyboard; {1} on controller, {2}.",
                keyboardName, controllerName, action)
        };
        return CapitalizeHintSentenceStart(hint);
    }

    private static string CapitalizeHintSentenceStart(string hint)
    {
        // Runtime binding display names and translated fallbacks may begin
        // in lower case. Each complete hint is a sentence, regardless of the
        // speech-formatting preference. Keep the rest of the label intact.
        for (int index = 0; index < hint.Length; index++)
        {
            if (!char.IsLetter(hint[index]))
                continue;
            char capital = char.ToUpperInvariant(hint[index]);
            return capital == hint[index] ? hint :
                hint[..index] + capital + hint[(index + 1)..];
        }
        return hint;
    }

    private string FormatHintUse(string keyboard, string controller, string purpose) =>
        FormatHintPress(keyboard, controller, purpose);
}
