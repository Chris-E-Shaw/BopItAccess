using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private PauseMenuPanel? _pauseMenuPanel;
    private long _nextPauseMenuSearchAt;
    private bool _pauseMenuWasVisible;
    private int _lastPauseMenuPanelId;
    private int _lastPauseMenuFocusedId;

    // The pause panel lives outside GameUIManager. Its two native buttons
    // resume the same round or leave it for the main menu.
    private bool ReadPauseMenuFocus()
    {
        if (_pauseMenuPanel == null)
        {
            if (_pauseMenuWasVisible)
                StopSpeechForGameStart();
            ResetPauseMenuFocus();
            long now = Environment.TickCount64;
            if (now < _nextPauseMenuSearchAt)
                return false;

            _nextPauseMenuSearchAt = now + 500;
            _pauseMenuPanel = UnityEngine.Object.FindFirstObjectByType<PauseMenuPanel>();
            if (_pauseMenuPanel == null)
                return false;
        }

        PauseMenuPanel panel = _pauseMenuPanel;
        if (!IsHintPanelVisible(panel))
        {
            // Pause speech may still be queued when the player resumes, and
            // the game's state can change a frame after this panel closes.
            // Clearing here also drops stale pause hints on a menu exit.
            if (_pauseMenuWasVisible)
                StopSpeechForGameStart();
            ResetPauseMenuFocus();
            return false;
        }

        int panelId = panel.GetInstanceID();
        if (panelId != _lastPauseMenuPanelId)
        {
            ResetPauseMenuFocus();
            _lastPauseMenuPanelId = panelId;
        }

        (int focusedId, string? label, int index, int count) =
            ReadPauseMenuSelection(panel);
        if (!_pauseMenuWasVisible)
        {
            _pauseMenuWasVisible = true;
            _lastPauseMenuFocusedId = focusedId;
            string introduction = L("Paused.");
            if (label != null)
                introduction += " " + WithMenuIndex(
                    WithControlType(label, "button"), index, count) + ".";
            QueueFocusSpeech(introduction);
            WriteStatus("Pause menu visible; " + (label ?? "waiting for focus") + ".");
            return true;
        }

        if (focusedId != 0 && focusedId != _lastPauseMenuFocusedId)
        {
            _lastPauseMenuFocusedId = focusedId;
            if (label != null)
                QueueFocusSpeech(WithMenuIndex(
                    WithControlType(label, "button"), index, count));
        }

        return true;
    }

    private (int Id, string? Label, int Index, int Count)
        ReadPauseMenuSelection(PauseMenuPanel panel)
    {
        Button? resume = panel.resumeButton;
        Button? mainMenu = panel.mainMenuButton;
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        Button? button = selected != null &&
            selected.transform.IsChildOf(panel.transform)
            ? selected.GetComponentInParent<Button>() : null;
        int id = button?.GetInstanceID() ?? 0;
        string? label = Matches(resume, id)
            ? ReadPauseButtonLabel(resume, "Resume")
            : Matches(mainMenu, id)
                ? ReadPauseButtonLabel(mainMenu, "Main menu") : null;
        if (label == null)
            return (0, null, -1, 0);

        int index = -1;
        int count = 0;
        foreach (Button? candidate in new[] { resume, mainMenu })
        {
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
                continue;
            if (candidate.GetInstanceID() == id)
                index = count;
            count++;
        }
        return (id, label, index, count);
    }

    private static string ReadPauseButtonLabel(Button? button, string fallback) =>
        CleanSpeechValue(button?.GetComponentInChildren<TMP_Text>(true)?.text)
        ?? L(fallback);

    private void ResetPauseMenuFocus()
    {
        _pauseMenuWasVisible = false;
        _lastPauseMenuPanelId = 0;
        _lastPauseMenuFocusedId = 0;
    }
}
