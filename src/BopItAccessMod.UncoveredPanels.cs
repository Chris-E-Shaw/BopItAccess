using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private Panel? _uncoveredPanel;
    private int _uncoveredPanelId;
    private int _uncoveredFocusedId;
    private string? _uncoveredControlValue;
    private GameUIManager? _uncoveredGameUi;
    private MainMenuUIManager? _uncoveredMainMenu;
    private SettingsPanel? _uncoveredSettings;
    private long _nextUncoveredUiSearchAt;
    private long _nextUncoveredPanelErrorLogAt;

    // A small safety net for player-facing panels that are not in the game's
    // known menu routes. Dedicated readers retain priority, especially the
    // final result reader, whose automatic score must be spoken first.
    private bool ReadUncoveredPanelFocus()
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        if (selected == null)
        {
            // Unity can briefly clear selection while rebuilding focus. Keep
            // the last panel identity so returning focus is not announced as
            // a fresh visit to the same screen.
            return false;
        }

        Panel? panel = selected.GetComponentInParent<Panel>();
        if (panel == null || !IsHintPanelVisible(panel) ||
            selected.GetComponentInParent<ScorePanel>() != null ||
            selected.GetComponentInParent<OneOnOneScorePanel>() != null ||
            selected.GetComponentInParent<GameOverPanel>() != null)
        {
            ResetUncoveredPanelFocus();
            return false;
        }

        long now = Environment.TickCount64;
        if (now >= _nextUncoveredUiSearchAt &&
            (_uncoveredGameUi == null || _uncoveredMainMenu == null ||
             _uncoveredSettings == null))
        {
            _nextUncoveredUiSearchAt = now + 500;
            if (_uncoveredGameUi == null)
                _uncoveredGameUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
            if (_uncoveredMainMenu == null)
                _uncoveredMainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_uncoveredSettings == null)
                _uncoveredSettings = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
        }
        if (IsUnderDedicatedPanel(selected.transform))
        {
            ResetUncoveredPanelFocus();
            return false;
        }
        GameState? state = _uncoveredGameUi?.gameManager?.GameState;
        if (state == GameState.Playing)
        {
            ResetUncoveredPanelFocus();
            return false;
        }

        Selectable? control = selected.GetComponentInParent<Selectable>();
        if (control == null || !control.transform.IsChildOf(panel.transform) ||
            !control.gameObject.activeInHierarchy)
        {
            ResetUncoveredPanelFocus();
            return false;
        }

        int panelId = panel.GetInstanceID();
        int focusedId = control.GetInstanceID();
        string? value = ReadUncoveredControlValue(control);
        if (panelId == _uncoveredPanelId && focusedId == _uncoveredFocusedId)
        {
            if (value != null && !string.Equals(value, _uncoveredControlValue,
                    StringComparison.Ordinal))
            {
                _uncoveredControlValue = value;
                RecordButtonHintUiActivity(now);
                QueueSpeech(value);
            }
            return true;
        }
        string? label = ReadUncoveredControlLabel(control);
        if (label == null)
            label = CleanSpeechValue(control.gameObject.name);
        if (label == null)
            return false;

        string type = control is Toggle ? "toggle" :
            control is Slider or Scrollbar ? "slider" :
            control is TMP_InputField ? "text field" : "button";
        (int index, int count) = IndexUncoveredControl(panel, focusedId);
        string focus = WithControlType(label, type);
        if (value != null)
            focus += ", " + value;
        focus = WithUnitySliderRange(focus, control);
        focus = WithMenuIndex(focus, index, count);
        if (panelId != _uncoveredPanelId)
        {
            _uncoveredPanel = panel;
            _uncoveredPanelId = panelId;
            _uncoveredFocusedId = focusedId;
            _uncoveredControlValue = value;
            string? heading = ReadUncoveredPanelText(panel, label);
            QueueFocusSpeech(heading == null ? focus : heading + ". " + focus);
            WriteStatus($"Additional panel visible: {panel.gameObject.name}; focus {label}.");
            return true;
        }

        if (focusedId != _uncoveredFocusedId)
        {
            _uncoveredFocusedId = focusedId;
            _uncoveredControlValue = value;
            QueueFocusSpeech(focus);
        }


        return true;
    }

    private bool IsUnderDedicatedPanel(Transform selected)
    {
        static bool Under(Transform current, Panel? panel) =>
            panel != null && current.IsChildOf(panel.transform);

        MainMenuUIManager? main = _mainMenu == null ? _uncoveredMainMenu : _mainMenu;
        GameUIManager? game = _uncoveredGameUi;
        return Under(selected, _speechMenuPanel) ||
            Under(selected, _calibrationPanel) ||
            Under(selected, main?.controlsPanel) ||
            Under(selected, main?.leaderboardPanel) ||
            Under(selected, main?.creditsPanel) ||
            Under(selected, main?.gameModePanel) ||
            Under(selected, main?.mainMenuPanel) ||
            Under(selected, _settingsPanel == null ? _uncoveredSettings : _settingsPanel) ||
            Under(selected, _pauseMenuPanel) ||
            Under(selected, game?.startScreen) ||
            Under(selected, game?.finalScorePanel) ||
            Under(selected, game?.combinedLeaderboardPanel) ||
            Under(selected, game?.partyLeaderboardPanel) ||
            Under(selected, game?.classicTutorialPanel) ||
            Under(selected, game?.extremeTutorialPanel) ||
            Under(selected, game?.oneOnOneTutorialPanel);
    }

    private static string? ReadUncoveredControlLabel(Selectable control)
    {
        foreach (TMP_Text text in control.GetComponentsInChildren<TMP_Text>(false))
        {
            if (text == null || !text.enabled || !text.gameObject.activeInHierarchy)
                continue;
            string? label = CleanSpeechValue(text.text);
            if (label != null)
                return label;
        }
        return null;
    }

    private static (int Index, int Count) IndexUncoveredControl(Panel panel, int id)
    {
        int index = -1;
        int count = 0;
        foreach (Selectable control in panel.GetComponentsInChildren<Selectable>(false))
        {
            if (control == null || !control.gameObject.activeInHierarchy ||
                !control.interactable)
                continue;
            if (control.GetInstanceID() == id)
                index = count;
            count++;
        }
        return (index, count);
    }

    private static string? ReadUncoveredControlValue(Selectable control)
    {
        return control switch
        {
            TMP_InputField field => CleanSpeechValue(field.text) ?? L("Empty"),
            Toggle toggle => toggle.isOn ? L("On") : L("Off"),
            Slider slider => slider.value.ToString("0.##",
                System.Globalization.CultureInfo.InvariantCulture),
            Scrollbar scrollbar => scrollbar.value.ToString("0.##",
                System.Globalization.CultureInfo.InvariantCulture),
            _ => null
        };
    }

    private static string? ReadUncoveredPanelText(Panel panel, string focusedLabel)
    {
        var lines = new List<string>(8);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int characters = 0;
        foreach (TMP_Text text in panel.GetComponentsInChildren<TMP_Text>(false))
        {
            if (text == null || !text.enabled || !text.gameObject.activeInHierarchy ||
                text.GetComponentInParent<Selectable>() != null)
                continue;
            string? line = CleanSpeechValue(text.text);
            if (line == null || line == focusedLabel || !seen.Add(line))
                continue;
            lines.Add(line);
            characters += line.Length;
            if (lines.Count >= 8 || characters >= 700)
                break;
        }
        return lines.Count == 0 ? null : string.Join(". ", lines);
    }

    private void ResetUncoveredPanelFocus()
    {
        _uncoveredPanel = null;
        _uncoveredPanelId = 0;
        _uncoveredFocusedId = 0;
        _uncoveredControlValue = null;
    }
}
