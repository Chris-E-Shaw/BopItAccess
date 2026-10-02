using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // New mod controls can provide their current, remappable binding here.
    // Every ordinary screen hint uses this registry, so adding a global
    // control does not require editing each screen's hint separately.
    private readonly List<(string Id, Func<string> Instruction)>
        _globalButtonHintControls = new();
    private bool _globalButtonHintControlsRegistered;

    private void RegisterGlobalButtonHintControl(string id,
        Func<string> instruction)
    {
        int existing = _globalButtonHintControls.FindIndex(entry =>
            string.Equals(entry.Id, id, StringComparison.Ordinal));
        if (existing >= 0)
            _globalButtonHintControls[existing] = (id, instruction);
        else
            _globalButtonHintControls.Add((id, instruction));
    }

    private void EnsureGlobalButtonHintControlsRegistered()
    {
        if (_globalButtonHintControlsRegistered)
            return;
        _globalButtonHintControlsRegistered = true;
        RegisterGlobalButtonHintControl("ToggleSpeech", ToggleSpeechHintInstruction);
        RegisterGlobalButtonHintControl("SpeakHints", SpeakHintsBindingInstruction);
        RegisterGlobalButtonHintControl("ChangeSpeechOutput", ChangeSpeechOutputBindingInstruction);
    }

    // Resolve the screen that currently owns input. This follows the same
    // precedence as OnLateUpdate, particularly for leaderboard wrappers that
    // remain alive behind song selection and the result screen.
    private (string Key, string Hint)? ResolveButtonHintContext()
    {
        (string Key, string Hint)? guideHint = GetGuideHintContext();
        if (guideHint != null)
            return guideHint;
        if (WelcomeScreenOpen)
            return ("Welcome", WelcomeScreenHint());

        GameUIManager? gameUi = _trackSelectUi;
        if (gameUi == null)
            gameUi = _gameOverUi;
        if (gameUi == null)
            gameUi = _leaderboardGameUi;
        MainMenuUIManager? main = _mainMenu == null ? null : _mainMenu;
        GameState? gameState = gameUi?.gameManager?.GameState;
        // Backing out of song selection can leave a cached GameUIManager
        // reporting Playing after the main-menu panels have returned. A
        // visible menu is stronger evidence of where input goes than that
        // stale game state. Keep gameplay silent when no menu owns the screen.
        bool mainMenuScreenVisible = IsHintPanelVisible(main?.mainMenuPanel) ||
            IsHintPanelVisible(main?.gameModePanel) ||
            IsHintPanelVisible(main?.controlsPanel) ||
            IsHintPanelVisible(main?.leaderboardPanel) ||
            IsHintPanelVisible(main?.creditsPanel) ||
            IsHintPanelVisible(_pauseMenuPanel) ||
            IsHintPanelVisible(_settingsPanel) ||
            IsHintPanelVisible(_speechMenuPanel) ||
            IsHintPanelVisible(_calibrationPanel) ||
            (_achievementBookWasOpen && _achievementsPanel != null &&
             _achievementsPanel.achievementViewState == AchievementViewState.Visible);
        if (gameState == GameState.Playing && !mainMenuScreenVisible)
            return null;

        if (_lastTitleScreenId != 0 && _titleScreen != null &&
            _titleScreen.gameObject.activeInHierarchy)
            return ("TitleScreen", WithGlobalControlHints(
                HintNativeAction("Bop", "open main menu", "Space", "confirm button")));

        if (_uncoveredPanel != null && IsHintPanelVisible(_uncoveredPanel))
            return ("AdditionalPanel:" + _uncoveredPanelId,
                WithGlobalControlHints(UiSubmitHint("activate item") + " " +
                    HintNavigation(true, "choose item") + " " +
                    UiBackHint("return")));

        if (_speechMenuOpen && _speechMenuRoot != null &&
            _speechMenuRoot.activeInHierarchy && IsHintPanelVisible(_speechMenuPanel))
        {
            string action = _speechUiOptions.FirstOrDefault(option =>
                option.Row.GetInstanceID() == _lastSpeechMenuRowId)?.ControlType ?? "button";
            return ("Speech:" + action,
                WithGlobalControlHints(MenuHintForControl(action, "Settings")));
        }

        if (IsHintPanelVisible(_calibrationPanel))
        {
            CalibrateState state = _calibrationPanel!.State;
            // Timed beat cues must remain clear, including the warmup period.
            if (state == CalibrateState.Warmup || state == CalibrateState.Calibrate ||
                state == CalibrateState.Finished)
                return null;
            return ("Calibration:" + state,
                WithGlobalControlHints(UiSubmitHint("activate option") +
                    " " + HintNavigation(true, "choose Calibrate or Back") + " " +
                    UiBackHint("return to Settings")));
        }

        if (IsHintPanelVisible(main?.controlsPanel))
        {
            bool rebinding = _controlsRebindingManager?.IsRebinding == true ||
                _leaderboardRebindOperation != null ||
                _descriptionRebindOperation != null ||
                _scoreRebindOperation != null ||
                _speakHintsRebindOperation != null ||
                _toggleSpeechRebindOperation != null ||
                _resetGyroRebindOperation != null ||
                _changeSpeechOutputRebindOperation != null;
            if (rebinding)
            {
                bool custom = _leaderboardRebindOperation != null ||
                    _descriptionRebindOperation != null ||
                    _scoreRebindOperation != null ||
                    _speakHintsRebindOperation != null ||
                    _toggleSpeechRebindOperation != null ||
                    _resetGyroRebindOperation != null ||
                    _changeSpeechOutputRebindOperation != null;
                string device = _controlsRebindingManager?.ActiveDevice ??
                    _controlsRebindingManager?.deviceTracker?.ActiveDevice ??
                    string.Empty;
                bool controller = IsGamepadDevice(device);
                string deviceSuffix = _hintsType == "Both"
                    ? controller ? " on controller" : " on keyboard"
                    : string.Empty;
                string assign = controller
                    ? "Press a new button" + deviceSuffix + " to assign it. "
                    : "Press a new key" + deviceSuffix + " to assign it. ";
                string cancel = custom
                    ? (controller ? "East face button" : "Escape") + deviceSuffix +
                        ", cancel rebinding."
                    : UiBackHint("cancel rebinding");
                return ("Controls:Rebinding", assign + cancel);
            }

            string focused = _controlsResetRow != null &&
                _controlsResetRow.GetInstanceID() == _lastFocusedControlsRowId
                ? "Reset" : "Binding";
            string use = focused == "Reset"
                ? UiSubmitHint("restore default bindings")
                : UiSubmitHint("reassign control");
            return ("Controls:" + focused,
                WithGlobalControlHints(use +
                    " " + HintNavigation(true, "choose control") + " " +
                    UiBackHint("return to Settings")));
        }

        if (IsHintPanelVisible(_pauseMenuPanel))
        {
            Button? resume = _pauseMenuPanel!.resumeButton;
            Button? mainMenu = _pauseMenuPanel.mainMenuButton;
            GameObject? selected = EventSystem.current?.currentSelectedGameObject;
            Button? focused = selected != null &&
                selected.transform.IsChildOf(_pauseMenuPanel.transform)
                ? selected.GetComponentInParent<Button>() : null;
            string purpose = focused != null &&
                Matches(mainMenu, focused.GetInstanceID())
                ? "return to main menu" : "resume game";
            return ("Pause:" + purpose,
                WithGlobalControlHints(UiSubmitHint(purpose) + " " +
                    HintNavigation(true, "choose pause option") + " " +
                    UiBackHint("resume game")));
        }

        if (gameState == GameState.GameOver &&
            !IsHintPanelVisible(gameUi?.startScreen))
        {
            PartyLeaderboardPanel? party = gameUi?.partyLeaderboardPanel;
            if (IsHintPanelVisible(party) &&
                IsHintPanelVisible(party?.leaderboardWrapper))
            {
                string nameHint = party!.State == PartyLeaderboardState.Input
                    ? "Choose name for score. " + UiSubmitHint("confirm name")
                    : "Choose name for score.";
                string continueHint = party.State == PartyLeaderboardState.Input
                    ? string.Empty : UiSubmitHint("continue");
                string backHint = UiBackHint("return to result screen");
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
                    WithGlobalControlHints(string.Join(" ", hints)));
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
                WithGlobalControlHints(UiBackHint("return to main menu")));
        }

        if (_achievementBookWasOpen && _achievementsPanel != null &&
            _achievementsPanel.achievementViewState == AchievementViewState.Visible)
        {
            bool canReadLines = _achievementBookController != null &&
                GetAchievementMoveAction(_achievementBookController) != null;
            string read = canReadLines && _achievementLines.Count > 1
                ? HintNavigation(true, "read current page",
                    GetAchievementMoveAction(_achievementBookController!)) + " "
                : string.Empty;
            return ("Achievements:" + (_lastAchievementSpread ?? "Opening"),
                WithGlobalControlHints(read +
                    HintAchievementPages() + " " +
                    UiBackHint("close book")));
        }

        if (IsHintPanelVisible(main?.creditsPanel))
        {
            string read = !_creditsAutoReading && _creditsLines.Count > 1
                ? HintNavigation(true, "read credits line by line",
                    HintUiMoveAction()) + " "
                : "Credits read automatically. ";
            return ("Credits:" + (_creditsAutoReading ? "Auto" : "Manual"),
                WithGlobalControlHints(read + UiBackHint("return to main menu")));
        }

        FinalScorePanel? final = gameUi?.finalScorePanel;
        if (gameState == GameState.GameOver && IsHintPanelVisible(final) &&
            (IsHintPanelVisible(final?.soloKillScreenPanel) ||
             IsHintPanelVisible(final?.withFriendsKillScreenPanel)))
        {
            bool solo = IsHintPanelVisible(final!.soloKillScreenPanel);
            string menu = solo
                ? UiSubmitHint("activate option") +
                    " " + HintNavigation(true,
                        "choose Replay or Leaderboard")
                : UiSubmitHint("activate Continue or Replay");
            return ("GameOver:" + (solo ? "Solo" : "WithFriends"),
                WithGlobalControlHints(menu + " " + UiBackHint("return") + " " +
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
            string startHint = HintNativeAction("Bop", "start game",
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
                WithGlobalControlHints(string.Join(" ", hints)));
        }

        if (IsHintPanelVisible(main?.gameModePanel))
            return ("PlayModes",
                WithGlobalControlHints(UiSubmitHint("select mode") +
                    " " + HintNavigation(true, "choose mode") + " " +
                    UiBackHint("return to main menu")));

        if (IsHintPanelVisible(_settingsPanel))
        {
            string action = GetFocusedSettingsOption(EventSystem.current?.currentSelectedGameObject)?
                .ControlType ?? "button";
            return ("Settings:" + action,
                WithGlobalControlHints(MenuHintForControl(action, "main menu")));
        }

        if (IsHintPanelVisible(main?.mainMenuPanel))
            return ("MainMenu",
                WithGlobalControlHints(UiSubmitHint("activate item") +
                    " " + HintNavigation(true, "choose menu item")));

        return null;
    }

    private (string Key, string Hint) CombinedLeaderboardHint(
        CombinedLeaderboardPanel panel, bool result)
    {
        string songHint = HintNativeAction("Twist", "change song",
            "Left Arrow", "top face button");
        string difficultyHint = HintNativeAction("Pull",
            "change difficulty", "Right Arrow", "right stick", true);
        string groupHint = LeaderboardAxisHint("ChangeGroup", "group");
        string dateHint = LeaderboardAxisHint("ChangeDateRange", "date");
        string continueHint = result ? UiSubmitHint("continue") : string.Empty;
        string backHint = result ? UiBackHint("return") :
            UiBackHint("return to main menu");

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
            WithGlobalControlHints(string.Join(" ", hints)));
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
            "choose previous " + noun, group ? "O" : "K",
            group ? "left shoulder button" : "D-Pad Left", "negative");
        string next = HintActionSentence(action, null,
            "choose next " + noun, group ? "P" : "L",
            group ? "right shoulder button" : "D-Pad Right", "positive");
        return previous + " " + next;
    }

    private string MenuHintForControl(string type, string returnTo)
    {
        string focused = type switch
        {
            "slider" => HintNavigation(false, "change slider"),
            "toggle" => UiSubmitHint("change toggle"),
            _ => UiSubmitHint("activate button")
        };
        return focused + " " + HintNavigation(true, "choose option") + " " +
            UiBackHint("return to " + returnTo);
    }

    private string WithGlobalControlHints(string hint)
    {
        if (_controlsRebindingManager?.IsRebinding == true ||
            _leaderboardRebindOperation != null || _descriptionRebindOperation != null ||
            _scoreRebindOperation != null || _speakHintsRebindOperation != null ||
            _toggleSpeechRebindOperation != null ||
            _resetGyroRebindOperation != null ||
            _changeSpeechOutputRebindOperation != null)
            return hint;

        EnsureGlobalButtonHintControlsRegistered();
        var hints = new List<string>(_globalButtonHintControls.Count + 1)
        {
            hint.TrimEnd()
        };
        foreach (var entry in _globalButtonHintControls)
        {
            string controlHint = entry.Instruction();
            if (!string.IsNullOrWhiteSpace(controlHint))
                hints.Add(controlHint);
        }
        return string.Join(" ", hints);
    }

    private string ToggleSpeechHintInstruction()
    {
        string keyboard = ReadToggleSpeechBindingLabel(0, ToggleSpeechKeyboardKey,
            "<Keyboard>/f8", "F8");
        string controller = ReadToggleSpeechBindingLabel(1, ToggleSpeechGamepadKey,
            "<Gamepad>/select", "Select");
        return FormatHintPress(keyboard, controller, "toggle speech");
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
