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
            label = "BACK";

        if (label == null)
        {
            _lastFocusedPlayModeButtonId = 0;
            return true;
        }

        if (buttonId != _lastFocusedPlayModeButtonId)
        {
            _lastFocusedPlayModeButtonId = buttonId;
            QueueSpeech(label);
        }

        return true;
    }

    private string? GetPlayModeLabel(int buttonId)
    {
        if (Matches(_mainMenu!.soloButton, buttonId)) return "SOLO";
        if (Matches(_mainMenu.partyButton, buttonId)) return "PARTY";
        if (Matches(_mainMenu.passItButton, buttonId)) return "PASS IT";
        if (Matches(_mainMenu.oneOnOneButton, buttonId)) return "ONE ON ONE";
        return null;
    }

    private void ResetPlayModesFocus()
    {
        _lastFocusedPlayModeButtonId = 0;
        _lastObservedPlayModeSelectionId = 0;
        _playModesWasVisible = false;
    }
}
