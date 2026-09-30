using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // Resolve the screen that currently owns input. This follows the same
    // precedence as OnLateUpdate, particularly for leaderboard wrappers that
    // remain alive behind song selection and the result screen.
    private (string Key, string Hint)? ResolveButtonHintContext()
    {
        GameUIManager? gameUi = _trackSelectUi;
        if (gameUi == null)
            gameUi = _gameOverUi;
        if (gameUi == null)
            gameUi = _leaderboardGameUi;
        MainMenuUIManager? main = _mainMenu == null ? null : _mainMenu;
        GameState? gameState = gameUi?.gameManager?.GameState;
        if (gameState == GameState.Playing)
            return null;

        if (_speechMenuOpen && _speechMenuRoot != null &&
            _speechMenuRoot.activeInHierarchy && IsHintPanelVisible(_speechMenuPanel))
        {
            string action = _speechUiOptions.FirstOrDefault(option =>
                option.Row.GetInstanceID() == _lastSpeechMenuRowId)?.ControlType ?? "button";
            return ("Speech:" + action,
                WithGlobalToggleHint(MenuHintForControl(action, "Settings")));
        }

        if (IsHintPanelVisible(_calibrationPanel))
        {
            CalibrateState state = _calibrationPanel!.State;
            // Timed beat cues must remain clear, including the warmup period.
            if (state == CalibrateState.Warmup || state == CalibrateState.Calibrate ||
                state == CalibrateState.Finished)
                return null;
            return ("Calibration:" + state,
                WithGlobalToggleHint(UiSubmitHint("activate the chosen option") +
                    " " + HintNavigation(true, "choose Calibrate or Back") + " " +
                    UiBackHint("return to Settings")));
        }

        if (IsHintPanelVisible(main?.controlsPanel))
        {
            bool rebinding = _controlsRebindingManager?.IsRebinding == true ||
                _leaderboardRebindOperation != null ||
                _descriptionRebindOperation != null ||
                _scoreRebindOperation != null ||
                _toggleSpeechRebindOperation != null;
            if (rebinding)
            {
                bool custom = _leaderboardRebindOperation != null ||
                    _descriptionRebindOperation != null ||
                    _scoreRebindOperation != null ||
                    _toggleSpeechRebindOperation != null;
                string device = _controlsRebindingManager?.ActiveDevice ??
                    _controlsRebindingManager?.deviceTracker?.ActiveDevice ??
                    string.Empty;
                bool controller = IsGamepadDevice(device);
                return ("Controls:Rebinding",
                    (controller ? "Press a new controller button to assign it. " :
                        "Press a new keyboard key to assign it. ") +
                    (custom
                        ? controller
                            ? "Press the east face button on controller to cancel the binding change."
                            : "Press Escape on keyboard to cancel the binding change."
                        : UiBackHint("cancel the binding change")));
            }

            string focused = _controlsResetRow != null &&
                _controlsResetRow.GetInstanceID() == _lastFocusedControlsRowId
                ? "Reset" : "Binding";
            string use = focused == "Reset"
                ? UiSubmitHint("restore the default bindings")
                : UiSubmitHint("reassign the focused control");
            return ("Controls:" + focused,
                WithGlobalToggleHint(use +
                    " " + HintNavigation(true, "choose a control") + " " +
                    UiBackHint("return to Settings")));
        }

        if (gameState == GameState.GameOver &&
            !IsHintPanelVisible(gameUi?.startScreen))
        {
            PartyLeaderboardPanel? party = gameUi?.partyLeaderboardPanel;
            if (IsHintPanelVisible(party) &&
                IsHintPanelVisible(party?.leaderboardWrapper))
            {
                string nameHint = party!.State == PartyLeaderboardState.Input
                    ? "Type or choose a name. " + UiSubmitHint("confirm it")
                    : "Choose a name for your score.";
                string continueHint = party.State == PartyLeaderboardState.Input
                    ? string.Empty : UiSubmitHint("continue");
                string backHint = UiBackHint("return to the result screen");
                string? focused = ReadLeaderboardFocusedItem(party.transform).Label;
                string rowsHint = _leaderboardCachedRows.Count > 0
                    ? HintLeaderboardRows(focused == null) : string.Empty;
                string? focusedHint = focused switch
                {
                    "Back" => backHint,
                    "Continue" => continueHint,
                    _ when focused != null && focused.StartsWith("Rank ",
                        StringComparison.OrdinalIgnoreCase) => rowsHint,
                    _ => null
                };
                var hints = new List<string>(5);
                if (!string.IsNullOrEmpty(focusedHint))
                    hints.Add(focusedHint);
                foreach (string hint in new[] { nameHint, rowsHint,
                    continueHint, backHint })
                    if (hint.Length > 0 && hint != focusedHint)
                        hints.Add(hint);
                return ("Leaderboard:Party:" + party.State,
                    WithGlobalToggleHint(string.Join(" ", hints)));
            }

            CombinedLeaderboardPanel? gameBoard = gameUi?.combinedLeaderboardPanel;
            if (IsHintPanelVisible(gameBoard) &&
                IsHintPanelVisible(gameBoard?.leaderboardWrapper))
                return CombinedLeaderboardHint(gameBoard!, true);
        }

        if (IsHintPanelVisible(main?.leaderboardPanel))
        {
            CombinedLeaderboardPanel? mainBoard = main!.leaderboardPanel!
                .GetComponentInChildren<CombinedLeaderboardPanel>(true);
            if (mainBoard != null &&
                (mainBoard.leaderboardWrapper == null ||
                 IsHintPanelVisible(mainBoard.leaderboardWrapper)))
                return CombinedLeaderboardHint(mainBoard, false);
            return ("Leaderboard:Loading",
                WithGlobalToggleHint(UiBackHint("return to the main menu")));
        }

        if (_achievementBookWasOpen && _achievementsPanel != null &&
            _achievementsPanel.achievementViewState == AchievementViewState.Visible)
        {
            bool canReadLines = _achievementBookController != null &&
                GetAchievementMoveAction(_achievementBookController) != null;
            string read = canReadLines && _achievementLines.Count > 1
                ? HintNavigation(true, "read the current page",
                    GetAchievementMoveAction(_achievementBookController!)) + " "
                : string.Empty;
            return ("Achievements:" + (_lastAchievementSpread ?? "Opening"),
                WithGlobalToggleHint(read +
                    HintAchievementPages() + " " +
                    UiBackHint("close the achievements book")));
        }

        if (IsHintPanelVisible(main?.creditsPanel))
        {
            string read = !_creditsAutoReading && _creditsLines.Count > 1
                ? HintNavigation(true, "read the credits line by line",
                    HintUiMoveAction()) + " "
                : "The credits are being read as they appear. ";
            return ("Credits:" + (_creditsAutoReading ? "Auto" : "Manual"),
                WithGlobalToggleHint(read + UiBackHint("return to the main menu")));
        }

        FinalScorePanel? final = gameUi?.finalScorePanel;
        if (gameState == GameState.GameOver && IsHintPanelVisible(final) &&
            (IsHintPanelVisible(final?.soloKillScreenPanel) ||
             IsHintPanelVisible(final?.withFriendsKillScreenPanel)))
        {
            bool solo = IsHintPanelVisible(final!.soloKillScreenPanel);
            string menu = solo
                ? UiSubmitHint("activate the selected option") +
                    " " + HintNavigation(true,
                        "choose Replay or Leaderboard")
                : UiSubmitHint("activate the Continue or Replay prompt shown on the result screen");
            return ("GameOver:" + (solo ? "Solo" : "WithFriends"),
                WithGlobalToggleHint(menu + " " + UiBackHint("return") + " " +
                    ReadScoreBindingInstruction()));
        }

        StartScreenPanel? start = gameUi?.startScreen;
        if (gameState == GameState.WaitingToStart && IsHintPanelVisible(start))
        {
            string songHint = HintNativeAction("Twist", "change song",
                "Left Arrow", "top face button");
            string difficultyHint = HintNativeAction("Pull", "change difficulty",
                "Right Arrow", "right stick", true);
            string descriptionHint = ReadDescriptionsBindingInstruction();
            string startHint = HintNativeAction("Bop", "start",
                "Space", "confirm button");
            string backHint = UiBackHint("return to mode selection");
            GameObject? selected = EventSystem.current?.currentSelectedGameObject;
            Selectable? focused = selected != null &&
                selected.transform.IsChildOf(start!.transform)
                ? selected.GetComponentInParent<Selectable>() : null;
            string? label = focused == null ? null :
                ReadTrackSelectControlLabel(focused);
            string? primary = label switch
            {
                "Start" => startHint,
                "Back" => backHint,
                _ when label != null && label.Contains("Extreme",
                    StringComparison.OrdinalIgnoreCase) => difficultyHint,
                _ => songHint
            };
            var hints = new List<string>(5) { primary };
            foreach (string hint in new[] { songHint, difficultyHint,
                descriptionHint, startHint, backHint })
                if (hint != primary)
                    hints.Add(hint);
            return ("SongSelect",
                WithGlobalToggleHint(string.Join(" ", hints)));
        }

        if (IsHintPanelVisible(main?.gameModePanel))
            return ("PlayModes",
                WithGlobalToggleHint(UiSubmitHint("select the focused mode") +
                    " " + HintNavigation(true,
                        "choose Solo, Party, Pass It, or One on One") + " " +
                    UiBackHint("return to the main menu")));

        if (IsHintPanelVisible(_settingsPanel))
        {
            string action = GetFocusedSettingsOption(EventSystem.current?.currentSelectedGameObject)?
                .ControlType ?? "button";
            return ("Settings:" + action,
                WithGlobalToggleHint(MenuHintForControl(action, "main menu")));
        }

        if (IsHintPanelVisible(main?.mainMenuPanel))
            return ("MainMenu",
                WithGlobalToggleHint(UiSubmitHint("activate the focused item") +
                    " " + HintNavigation(true, "choose a menu item")));

        return null;
    }

    private (string Key, string Hint) CombinedLeaderboardHint(
        CombinedLeaderboardPanel panel, bool result)
    {
        string songHint = HintNativeAction("Twist", "change song",
            "Left Arrow", "top face button");
        string difficultyHint = HintNativeAction("Pull",
            "change Classic or Extreme", "Right Arrow", "right stick", true);
        string groupHint = LeaderboardAxisHint("ChangeGroup", "group");
        string dateHint = LeaderboardAxisHint("ChangeDateRange", "date");
        string continueHint = result ? UiSubmitHint("continue") : string.Empty;
        string backHint = result ? UiBackHint("return") :
            UiBackHint("return to the main menu");

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool selectedInPanel = selected != null &&
            selected.transform.IsChildOf(panel.transform);
        string? focusedLabel = selectedInPanel
            ? ReadLeaderboardFocusedItem(panel.transform).Label : null;
        string rowsHint = _leaderboardCachedRows.Count > 0
            ? HintLeaderboardRows(focusedLabel == null) : string.Empty;
        string? primary = null;
        if (selectedInPanel)
        {
            if (selected!.GetComponentInParent<GroupFilterTab>() != null)
                primary = groupHint;
            else if (selected.GetComponentInParent<DateFilterTab>() != null)
                primary = dateHint;
            else if (selected.GetComponentInParent<LeaderboardLineItem>() != null ||
                selected.GetComponentInParent<PartyLeaderboardLineItem>() != null)
                primary = rowsHint;
            else if (string.Equals(focusedLabel, "Continue",
                StringComparison.OrdinalIgnoreCase))
                primary = continueHint;
            else if (string.Equals(focusedLabel, "Back",
                StringComparison.OrdinalIgnoreCase))
                primary = backHint;
            else if (string.Equals(focusedLabel, "Classic",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(focusedLabel, "Extreme",
                    StringComparison.OrdinalIgnoreCase))
                primary = difficultyHint;
            else if (focusedLabel is "Shapes" or "Space" or "City" or "Office")
                primary = songHint;
        }
        var hints = new List<string>(7);
        if (!string.IsNullOrEmpty(primary))
            hints.Add(primary);
        foreach (string hint in new[] { songHint, difficultyHint, groupHint,
            dateHint, rowsHint, continueHint, backHint })
            if (hint.Length > 0 && hint != primary)
                hints.Add(hint);
        return ("Leaderboard:Combined:" + (result ? "Result" : "Main") +
                ":" + panel.Mode,
            WithGlobalToggleHint(string.Join(" ", hints)));
    }

    private string LeaderboardAxisHint(string actionName, string noun)
    {
        InputRebindingManager? manager = _controlsRebindingManager;
        if (manager == null)
        {
            MainMenuUIManager? main = _mainMenu == null ? null : _mainMenu;
            manager = main?.controlsPanel?.GetComponentInChildren<InputRebindingManager>(true);
        }
        InputAction? action = manager?.playerInput?.actions?
            .FindActionMap(LeaderboardControlsMap, false)?
            .FindAction(actionName, false) ?? manager?.inputActions?
            .FindActionMap(LeaderboardControlsMap, false)?
            .FindAction(actionName, false);
        bool group = actionName == "ChangeGroup";
        string previous = HintActionSentence(action, null,
            "choose the previous " + noun, group ? "O" : "K",
            group ? "left shoulder button" : "D-Pad Left", "negative");
        string next = HintActionSentence(action, null,
            "choose the next " + noun, group ? "P" : "L",
            group ? "right shoulder button" : "D-Pad Right", "positive");
        return previous + " " + next;
    }

    private string MenuHintForControl(string type, string returnTo)
    {
        string focused = type switch
        {
            "slider" => HintNavigation(false, "change the focused slider"),
            "toggle" => UiSubmitHint("change the focused toggle"),
            _ => UiSubmitHint("activate the focused button")
        };
        return focused + " " + HintNavigation(true, "choose an option") + " " +
            UiBackHint("return to " + returnTo);
    }

    private string WithGlobalToggleHint(string hint)
    {
        if (_controlsRebindingManager?.IsRebinding == true ||
            _leaderboardRebindOperation != null || _descriptionRebindOperation != null ||
            _scoreRebindOperation != null || _toggleSpeechRebindOperation != null)
            return hint;
        string keyboard = ReadToggleSpeechBindingLabel(0, ToggleSpeechKeyboardKey,
            "<Keyboard>/f8", "F8");
        string controller = ReadToggleSpeechBindingLabel(1, ToggleSpeechGamepadKey,
            "<Gamepad>/select", "Select");
        return hint.TrimEnd() + " " + FormatHintPress(keyboard, controller,
            "toggle speech");
    }

    private static bool IsHintPanelVisible(Panel? panel) =>
        panel != null && panel.gameObject.activeInHierarchy &&
        panel.IsVisible && !panel.IsTransitioningHide;

    private string UiSubmitHint(string purpose)
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        return HintActionSentence(FindHintAction("UI", "Submit"),
            module?.submit?.action, purpose, "Enter or Space", "confirm button");
    }

    private string UiBackHint(string purpose)
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        return HintActionSentence(FindHintAction("UI", "Back"),
            module?.cancel?.action, purpose, "Backspace", "back button");
    }
}
