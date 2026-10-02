using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private int _lastFocusedPlayModeButtonId;
    private int _lastObservedPlayModeSelectionId;
    private bool _playModesWasVisible;
    private long _nextPlayModesSearchAt;

    private bool ReadPlayModesFocus()
    {
        if (_mainMenu == null)
        {
            long now = Environment.TickCount64;
            if (now < _nextPlayModesSearchAt)
                return false;

            _nextPlayModesSearchAt = now + 500;
            _mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_mainMenu == null)
                return false;
        }

        Panel? panel = _mainMenu.gameModePanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetPlayModesFocus();
            return false;
        }

        if (!_playModesWasVisible)
        {
            WriteStatus("Play mode panel is visible; monitoring its four mode buttons.");
            _playModesWasVisible = true;
        }

        EventSystem? eventSystem = EventSystem.current;
        GameObject? selected = eventSystem == null ? null : eventSystem.currentSelectedGameObject;
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        if (selectedId != _lastObservedPlayModeSelectionId)
        {
            WriteStatus($"Play mode selected object: {(selected == null ? "none" : selected.name)}.");
            _lastObservedPlayModeSelectionId = selectedId;
        }

        Button? button = selected == null ? null : selected.GetComponentInParent<Button>();
        if (button == null || !button.transform.IsChildOf(panel.transform))
        {
            _lastFocusedPlayModeButtonId = 0;
            return true;
        }

        int buttonId = button.GetInstanceID();
        string? label = GetPlayModeLabel(buttonId);
        if (label == null && button.name.Contains("Back", StringComparison.OrdinalIgnoreCase))
            label = ReadNativeButtonLabel(button, "BACK");

        if (label == null)
        {
            _lastFocusedPlayModeButtonId = 0;
            return true;
        }

        if (buttonId != _lastFocusedPlayModeButtonId)
        {
            _lastFocusedPlayModeButtonId = buttonId;
            QueueFocusSpeech(IndexPlayModeLabel(panel, button,
                WithControlType(label, "button")));
        }

        return true;
    }

    private string? GetPlayModeLabel(int buttonId)
    {
        if (Matches(_mainMenu!.soloButton, buttonId))
            return ReadNativeButtonLabel(_mainMenu.soloButton, "SOLO");
        if (Matches(_mainMenu.partyButton, buttonId))
            return ReadNativeButtonLabel(_mainMenu.partyButton, "PARTY");
        if (Matches(_mainMenu.passItButton, buttonId))
            return ReadNativeButtonLabel(_mainMenu.passItButton, "PASS IT");
        if (Matches(_mainMenu.oneOnOneButton, buttonId))
            return ReadNativeButtonLabel(_mainMenu.oneOnOneButton, "ONE ON ONE");
        return null;
    }

    private string IndexPlayModeLabel(Panel panel, Button focused, string label)
    {
        var choices = new List<Button>(5);
        AddAvailablePlayModeButton(choices, panel, _mainMenu!.soloButton);
        AddAvailablePlayModeButton(choices, panel, _mainMenu.partyButton);
        AddAvailablePlayModeButton(choices, panel, _mainMenu.passItButton);
        AddAvailablePlayModeButton(choices, panel, _mainMenu.oneOnOneButton);

        // The Back control is a selectable on some builds. Include it only
        // when present and available, rather than counting a Back prompt.
        foreach (Button candidate in panel.GetComponentsInChildren<Button>(true))
        {
            if (candidate.name.Contains("Back", StringComparison.OrdinalIgnoreCase) &&
                candidate.gameObject.activeInHierarchy && candidate.interactable)
            {
                AddAvailablePlayModeButton(choices, panel, candidate);
                break;
            }
        }

        int focusedId = focused.GetInstanceID();
        for (int index = 0; index < choices.Count; index++)
        {
            if (choices[index].GetInstanceID() == focusedId)
                return WithMenuIndex(label, index, choices.Count);
        }

        return label;
    }

    private static void AddAvailablePlayModeButton(List<Button> choices, Panel panel, Button? button)
    {
        if (button != null && button.gameObject.activeInHierarchy && button.interactable &&
            button.transform.IsChildOf(panel.transform))
            choices.Add(button);
    }

    private void ResetPlayModesFocus()
    {
        _lastFocusedPlayModeButtonId = 0;
        _lastObservedPlayModeSelectionId = 0;
        _playModesWasVisible = false;
    }
}
