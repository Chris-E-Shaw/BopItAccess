using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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
                WithGlobalToggleHint("Use up and down to choose Calibrate or Back. " +
                    UiSubmitHint("activate the chosen option") + " " +
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
                return ("Controls:Rebinding",
                    "Press the new key or controller button to assign it. " +
                    "Use Back to cancel the binding change.");

            string focused = _controlsResetRow != null &&
                _controlsResetRow.GetInstanceID() == _lastFocusedControlsRowId
                ? "Reset" : "Binding";
            string use = focused == "Reset"
                ? UiSubmitHint("restore the default bindings")
                : UiSubmitHint("reassign the focused control");
            return ("Controls:" + focused,
                WithGlobalToggleHint("Use up and down to choose a control. " +
                    use + " " + UiBackHint("return to Settings")));
        }

        if (gameState == GameState.GameOver &&
            !IsHintPanelVisible(gameUi?.startScreen))
        {
            PartyLeaderboardPanel? party = gameUi?.partyLeaderboardPanel;
            if (IsHintPanelVisible(party) &&
                IsHintPanelVisible(party?.leaderboardWrapper))
            {
                string nameHint = party!.State == PartyLeaderboardState.Input
                    ? "Type a name, then confirm it. "
                    : "Choose a name for your score, then continue. ";
                string rowsHint = _leaderboardCachedRows.Count > 0
                    ? "Use Page Up and Page Down to read score rows. " : string.Empty;
                return ("Leaderboard:Party:" + party.State,
                    WithGlobalToggleHint(nameHint + rowsHint +
                        UiBackHint("return to the result screen")));
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
                ? "Use up and down to read the current page. " : string.Empty;
            return ("Achievements:" + (_lastAchievementSpread ?? "Opening"),
                WithGlobalToggleHint(read +
                    "Use left and right arrows or controller shoulder buttons to turn pages. " +
                    UiBackHint("close the achievements book")));
        }

        if (IsHintPanelVisible(main?.creditsPanel))
        {
            string read = !_creditsAutoReading && _creditsLines.Count > 1
                ? "Use up and down to read the credits line by line. "
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
                ? "Use up and down to choose Replay or Leaderboard. " +
                    UiSubmitHint("activate the selected option")
                : UiSubmitHint("activate the Continue or Replay prompt shown on the result screen");
            return ("GameOver:" + (solo ? "Solo" : "WithFriends"),
                WithGlobalToggleHint(menu + " " + UiBackHint("return") + " " +
                    ReadScoreBindingInstruction()));
        }

        StartScreenPanel? start = gameUi?.startScreen;
        if (gameState == GameState.WaitingToStart && IsHintPanelVisible(start))
            return ("SongSelect",
                WithGlobalToggleHint("Twist to change song. Pull to change difficulty. " +
                    ReadDescriptionsBindingInstruction() +
                    " Bop to start. " + UiBackHint("return to mode selection")));

        if (IsHintPanelVisible(main?.gameModePanel))
            return ("PlayModes",
                WithGlobalToggleHint("Use up and down to choose Solo, Party, Pass It, or One on One. " +
                    UiSubmitHint("select the focused mode") + " " +
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
                WithGlobalToggleHint("Use up and down to choose a menu item. " +
                    UiSubmitHint("activate the focused item")));

        return null;
    }

    private (string Key, string Hint) CombinedLeaderboardHint(
        CombinedLeaderboardPanel panel, bool result)
    {
        string rows = _leaderboardCachedRows.Count > 0
            ? "Use Page Up and Page Down to read score rows. " : string.Empty;
        string filters = "Twist to change song. Pull to change Classic or Extreme. " +
            LeaderboardAxisHint("ChangeGroup", "group", "Local, Friends, and Global") + " " +
            LeaderboardAxisHint("ChangeDateRange", "date", "Today, This month, and All time");
        string returnHint = result
            ? UiSubmitHint("continue") + " " + UiBackHint("return")
            : UiBackHint("return to the main menu");
        return ("Leaderboard:Combined:" + (result ? "Result" : "Main") +
                ":" + panel.Mode,
            WithGlobalToggleHint(filters + " " + rows + returnHint));
    }

    private string LeaderboardAxisHint(string actionName, string noun, string choices)
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
        if (action == null)
            return $"Use {noun} previous and next controls to choose {choices}.";

        string previous = ReadLeaderboardAxisPart(action, "negative", noun + " previous");
        string next = ReadLeaderboardAxisPart(action, "positive", noun + " next");
        return $"{previous} to choose the previous {noun}; {next} to choose the next {noun}.";
    }

    private static string ReadLeaderboardAxisPart(InputAction action, string part,
        string fallback)
    {
        int keyboardIndex = FindCompositePartIndex(action, "Keyboard", -1, part);
        int gamepadIndex = FindCompositePartIndex(action, "Gamepad", -1, part);
        string? keyboard = keyboardIndex < 0 ? null : CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, keyboardIndex));
        string? gamepad = gamepadIndex < 0 ? null : CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, gamepadIndex));
        if (keyboard != null && gamepad != null)
            return $"Press {keyboard} on keyboard or {gamepad} on controller";
        if (keyboard != null)
            return $"Press {keyboard} on keyboard";
        if (gamepad != null)
            return $"Press {gamepad} on controller";
        return "Use " + fallback;
    }

    private string MenuHintForControl(string type, string returnTo)
    {
        string focused = type switch
        {
            "slider" => "Use left and right to change the focused slider.",
            "toggle" => UiSubmitHint("change the focused toggle"),
            _ => UiSubmitHint("activate the focused button")
        };
        return "Use up and down to choose an option. " + focused + " " +
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
        return hint + $" Press {keyboard} on keyboard or {controller} on controller to toggle speech.";
    }

    private static bool IsHintPanelVisible(Panel? panel) =>
        panel != null && panel.gameObject.activeInHierarchy &&
        panel.IsVisible && !panel.IsTransitioningHide;

    private string UiSubmitHint(string purpose)
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        return UiActionHint(module?.submit?.action, "Select", purpose);
    }

    private string UiBackHint(string purpose)
    {
        InputSystemUIInputModule? module =
            EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        return UiActionHint(module?.cancel?.action, "Back", purpose);
    }

    private static string UiActionHint(InputAction? action, string fallback,
        string purpose)
    {
        if (action == null)
            return $"Use {fallback} to {purpose}.";

        var keyboard = new List<string>(2);
        var controller = new List<string>(2);
        for (int index = 0; index < action.bindings.Count; index++)
        {
            InputBinding binding = action.bindings[index];
            if (binding.isComposite || binding.isPartOfComposite)
                continue;
            string path = binding.effectivePath ?? string.Empty;
            List<string>? target = path.Contains("<Keyboard>",
                StringComparison.OrdinalIgnoreCase) ? keyboard :
                path.Contains("<Gamepad>", StringComparison.OrdinalIgnoreCase)
                    ? controller : null;
            if (target == null || target.Count >= 2)
                continue;
            string? label = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, index));
            if (label != null && !target.Contains(label))
                target.Add(label);
        }

        string? keyboardText = keyboard.Count == 0 ? null :
            string.Join(" or ", keyboard) + " on keyboard";
        string? controllerText = controller.Count == 0 ? null :
            string.Join(" or ", controller) + " on controller";
        string controls = keyboardText != null && controllerText != null
            ? keyboardText + ", or " + controllerText
            : keyboardText ?? controllerText ?? fallback;
        return $"Press {controls} to {purpose}.";
    }
}
