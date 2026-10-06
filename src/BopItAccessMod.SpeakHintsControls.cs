using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string SpeakHintsKeyboardKey = "BopItAccess.SpeakHints.Keyboard";
    private const string SpeakHintsGamepadKey = "BopItAccess.SpeakHints.Gamepad";
    private InputActionAsset? _speakHintsActionAsset;
    private InputAction? _speakHintsAction;
    private AddedSpeakHintsControlRow? _speakHintsControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _speakHintsRebindOperation;
    private InputRebindingManager? _speakHintsRebindManager;
    private InputActionMap? _speakHintsRebindUiMap;
    private bool _speakHintsRebindUiMapWasEnabled;
    private bool _speakHintsRebindActionWasEnabled;
    private int _speakHintsRebindIndex;
    private string? _speakHintsRebindOriginalPath;
    private long _nextSpeakHintsControlErrorAt;
    private int _speakHintsSuppressPressThroughFrame = -1;
    private bool _speakHintsAnyRebindWasActive;

    private InputAction EnsureSpeakHintsAction()
    {
        if (_speakHintsAction != null)
            return _speakHintsAction;

        // This asset belongs to the mod. It can be bound to a native prompt,
        // while its override keys remain independent of the game's save file.
        _speakHintsActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = _speakHintsActionAsset.AddActionMap("BopItAccess");
        InputAction action = map.AddAction("SpeakHints", InputActionType.Button);
        action.AddBinding("<Keyboard>/h");
        action.AddBinding("<Gamepad>/rightStickPress");
        string keyboard = PlayerPrefs.GetString(SpeakHintsKeyboardKey, string.Empty);
        string gamepad = PlayerPrefs.GetString(SpeakHintsGamepadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(keyboard))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 0, keyboard);
        if (!string.IsNullOrWhiteSpace(gamepad))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 1, gamepad);
        _speakHintsAction = action;
        WriteStatus("Speak Hints input initialized with keyboard and controller bindings.");
        return action;
    }

    private bool WasSpeakHintsPressed(bool hintsAvailable)
    {
        try
        {
            bool rebinding = _speakHintsRebindOperation != null ||
                _descriptionRebindOperation != null ||
                _scoreRebindOperation != null ||
                _toggleSpeechRebindOperation != null ||
                _leaderboardRebindOperation != null ||
                _resetGyroRebindOperation != null ||
                _changeSpeechOutputRebindOperation != null ||
                _controlsRebindingManager?.IsRebinding == true;
            if (!hintsAvailable || rebinding || !_speechEnabled ||
                _speechSuppressedForBackground)
            {
                if (_speakHintsAction?.enabled == true)
                    _speakHintsAction.Disable();
                if (rebinding)
                    _speakHintsAnyRebindWasActive = true;
                return false;
            }

            if (_speakHintsAnyRebindWasActive)
            {
                // The final captured button must not also read the hints.
                _speakHintsAnyRebindWasActive = false;
                _speakHintsSuppressPressThroughFrame = Time.frameCount + 1;
                return false;
            }

            InputAction action = EnsureSpeakHintsAction();
            if (!action.enabled)
                action.Enable();
            if (Time.frameCount <= _speakHintsSuppressPressThroughFrame)
                return false;

            // The native DebugMenu is Right Shoulder + Right Stick Press.
            // A plain stick press is free, but preserve the game combo.
            foreach (Gamepad gamepad in Gamepad.all)
                if (gamepad.rightShoulder.isPressed &&
                    gamepad.rightStickButton.wasPressedThisFrame)
                    return false;

            return action.WasPressedThisFrame();
        }
        catch (Exception ex)
        {
            try
            {
                if (_speakHintsAction?.enabled == true)
                    _speakHintsAction.Disable();
            }
            catch (Exception cleanup)
            {
                WriteStatus("Speak Hints input cleanup failed: " + cleanup.Message);
            }
            if (Environment.TickCount64 >= _nextSpeakHintsControlErrorAt)
            {
                WriteStatus("Speak Hints input failed: " + ex);
                _nextSpeakHintsControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private bool IsSpeakHintsHeld()
    {
        try
        {
            return _speakHintsAction?.enabled == true &&
                _speakHintsAction.IsPressed();
        }
        catch (Exception ex)
        {
            if (Environment.TickCount64 >= _nextSpeakHintsControlErrorAt)
            {
                WriteStatus("Speak Hints held input check failed: " + ex.Message);
                _nextSpeakHintsControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private string SpeakHintsBindingInstruction()
    {
        string purpose = _trackSelectUi?.gameManager?.GameState ==
            GameState.WaitingToStart &&
            IsHintPanelVisible(_trackSelectUi.startScreen)
            ? "speak hints and tutorial text" : "speak hints";
        try
        {
            InputAction action = EnsureSpeakHintsAction();
            string keyboard = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ?? "H";
            string gamepad = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ?? "right stick press";
            return FormatHintPress(keyboard, gamepad, purpose);
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Speak Hints bindings: " + ex.Message);
            string keyboard = HintSavedBinding(SpeakHintsKeyboardKey, "H");
            string gamepad = HintSavedBinding(SpeakHintsGamepadKey,
                "right stick press");
            return FormatHintPress(keyboard, gamepad, purpose);
        }
    }

    private void AddSpeakHintsControlRow(ControlRow template, Transform parent, int insertAt)
    {
        InputAction action = EnsureSpeakHintsAction();
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "SPEAK_HINTS";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
            throw new InvalidOperationException("The Speak Hints row lost a native control.");

        row.ActionMapName = "BopItAccess";
        row.ActionName = "SpeakHints";
        SetClonedLabel(defaultLabel, "SPEAK HINTS");
        SetClonedLabel(activeLabel, "SPEAK HINTS");
        InputActionReference reference = InputActionReference.Create(action);
        _leaderboardPromptReferences.Add(reference);
        SetDisplayPrompt(row.ActiveDisplayPrompt, reference,
            (ControlPromptSpriteSwapper.CompositePart)0);
        SetDisplayPrompt(row.DefaultDisplayPrompt, reference,
            (ControlPromptSpriteSwapper.CompositePart)0);

        Navigation navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.Vertical;
        selectable.navigation = navigation;
        GameObject? active = row.ActiveContainer;
        GameObject? inactive = row.DefaultContainer;
        if (active == null || inactive == null)
            throw new InvalidOperationException("The Speak Hints row lost its focus containers.");
        _speakHintsControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Match the leaderboard rows: the native listener would try to find
        // this action in the game's asset, so only the visual row is kept.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedSpeakHintsControlRow? FindAddedSpeakHintsControlRow(GameObject? selected)
    {
        AddedSpeakHintsControlRow? row = _speakHintsControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private string? SpeakHintsControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction action = EnsureSpeakHintsAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = IsGamepadDevice(device) ? 1 : 0;
        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? SpeakHintsControlSnapshot()
    {
        InputAction action = EnsureSpeakHintsAction();
        string? keyboard = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        string? gamepad = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 1));
        return keyboard == null || gamepad == null ? null : keyboard + "\u001e" + gamepad;
    }

    private void SpeakHintsControlRow(AddedSpeakHintsControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = SpeakHintsControlBinding(manager);
        bool rebinding = _speakHintsRebindOperation != null;

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType(L("Speak Hints"), "button");
            string message = binding == null ? label : LF("{0}, {1}", label,
                LocalizeBindingDisplay(binding));
            if (rebinding)
                message += L(".") + " " + L("Listening for input");
            QueueFocusSpeech(WithControlsIntroduction(message));
            return;
        }

        if (binding != null && !string.Equals(binding, _lastControlsBinding,
                StringComparison.Ordinal))
        {
            _lastControlsBinding = binding;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = true;
            QueueSpeech(LocalizeBindingDisplay(binding));
            return;
        }

        if (rebinding && !_lastControlsRebinding)
        {
            _controlsBindingChangedDuringRebind = false;
            QueueSpeech(L("Listening for input"));
        }
        else if (!rebinding && _lastControlsRebinding &&
            !_controlsBindingChangedDuringRebind)
            QueueSpeech(L("Binding unchanged"));

        _lastControlsRebinding = rebinding;
    }

    private void UpdateSpeakHintsControlRebinding()
    {
        try
        {
            UpdateSpeakHintsControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _speakHintsRebindOperation != null;
            if (Environment.TickCount64 >= _nextSpeakHintsControlErrorAt)
            {
                WriteStatus("Speak Hints rebinding failed: " + ex);
                _nextSpeakHintsControlErrorAt = Environment.TickCount64 + 5000;
            }
            try
            {
                if (wasRebinding)
                    RestoreSpeakHintsOriginalOverride();
                CancelSpeakHintsControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Speak Hints rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _speakHintsControlRow = null;
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
        }
    }

    private void UpdateSpeakHintsControlRebindingCore()
    {
        AddedSpeakHintsControlRow? row = _speakHintsControlRow;
        // Destroyed Unity objects still have a managed wrapper; null
        // propagation does not detect them. Check before calling Unity.
        if (row == null || row.Root == null)
        {
            _speakHintsControlRow = null;
            if (_speakHintsRebindOperation != null)
            {
                RestoreSpeakHintsOriginalOverride();
                CancelSpeakHintsControlRebinding(false);
            }
            return;
        }

        MainMenuUIManager? main = _mainMenu;
        Panel? panel = main == null ? null : main.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_speakHintsRebindOperation != null)
            {
                RestoreSpeakHintsOriginalOverride();
                CancelSpeakHintsControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedSpeakHintsControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_speakHintsRebindOperation != null)
        {
            if (_speakHintsRebindOperation.completed)
                CompleteSpeakHintsControlRebinding();
            else if (_speakHintsRebindOperation.canceled)
                CancelSpeakHintsControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding || !WasControlsSubmitPressed(manager))
            return;

        if (focused)
        {
            StartSpeakHintsControlRebinding(manager);
            return;
        }

        // The native Reset row handles the game's own bindings. Include the
        // two mod-owned overrides in the same user action.
        ResetToDefaultRow? reset = _controlsResetRow ??
            row.Root.GetComponentInParent<Panel>()?.GetComponentInChildren<ResetToDefaultRow>(true);
        if (reset != null && selected != null &&
            (selected.GetInstanceID() == reset.gameObject.GetInstanceID() ||
             selected.transform.IsChildOf(reset.transform)))
            ResetSpeakHintsControlBindings();
    }

    private void StartSpeakHintsControlRebinding(InputRebindingManager manager)
    {
        InputAction action = EnsureSpeakHintsAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        bool gamepad = IsGamepadDevice(device);
        int index = gamepad ? 1 : 0;
        string layout = gamepad ? "<Gamepad>" : "<Keyboard>";
        string cancelPath = gamepad ? "<Gamepad>/buttonEast" : "<Keyboard>/escape";
        string? original = action.bindings[index].overridePath;
        InputActionMap? uiMap = manager.inputActions?.FindActionMap("UI", false);
        bool uiWasEnabled = uiMap?.enabled ?? false;
        bool actionWasEnabled = action.enabled;
        if (actionWasEnabled)
            action.Disable();
        if (uiWasEnabled)
            uiMap!.Disable();

        InputActionRebindingExtensions.RebindingOperation? operation = null;
        try
        {
            operation = InputActionRebindingExtensions.PerformInteractiveRebinding(action, index);
            operation.WithTargetBinding(index)
                .WithControlsHavingToMatchPath(layout)
                .WithControlsExcluding("<Keyboard>/anyKey")
                .WithControlsExcluding("<Pointer>/*")
                .WithControlsExcluding("<Touchscreen>/*")
                .WithCancelingThrough(cancelPath)
                .OnMatchWaitForAnother(0.1f)
                .Start();
            _speakHintsRebindOperation = operation;
            _speakHintsRebindManager = manager;
            _speakHintsRebindUiMap = uiMap;
            _speakHintsRebindUiMapWasEnabled = uiWasEnabled;
            _speakHintsRebindActionWasEnabled = actionWasEnabled;
            _speakHintsRebindIndex = index;
            _speakHintsRebindOriginalPath = original;
            WriteStatus($"Rebinding Speak Hints on {device}, binding {index}.");
        }
        catch
        {
            operation?.Dispose();
            if (uiWasEnabled)
                uiMap!.Enable();
            if (actionWasEnabled)
                action.Enable();
            throw;
        }
    }

    private void CompleteSpeakHintsControlRebinding()
    {
        InputAction? action = _speakHintsAction;
        InputRebindingManager? manager = _speakHintsRebindManager;
        int index = _speakHintsRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved))
        {
            RestoreSpeakHintsOriginalOverride();
            ReleaseSpeakHintsControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        if (IsBindingAssignedElsewhere(manager, action, index, resolved,
                out string owner))
        {
            RestoreSpeakHintsOriginalOverride();
            ReleaseSpeakHintsControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        PlayerPrefs.SetString(index == 1 ? SpeakHintsGamepadKey : SpeakHintsKeyboardKey, path!);
        SaveModPreferencesAndConfig();
        ReleaseSpeakHintsControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshSpeakHintsControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Speak Hints to {path}.");
        QueueSpeech(_lastControlsBinding == null ? L("Binding changed") :
            LocalizeBindingDisplay(_lastControlsBinding));
    }

    private bool IsSpeakHintsBindingInUse(InputRebindingManager manager,
        string path)
    {
        InputAction[] modActions =
            { EnsureDescriptionAction(), EnsureScoreAction(), EnsureToggleSpeechAction() };
        foreach (InputAction action in modActions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (SpeakHintsPathsMatch(action.bindings[i].effectivePath, path))
                    return true;
            }
        }

        string[] nativeNames = { "Bop", "Twist", "Pull", "Spin", "Flick",
            "AltBop", "Submit", "Back", "Cancel", "Navigate",
            "ChangeGroup", "ChangeDateRange", "ChangeDevice",
            "FlipAchievementPages", "DebugMenu", "ResetGyro", "AutoPlay",
            "Menu" };
        InputActionAsset?[] assets =
            { manager.playerInput?.actions, manager.inputActions };
        foreach (InputActionAsset? asset in assets)
        {
            if (asset == null)
                continue;
            foreach (string name in nativeNames)
            {
                InputAction? action = asset.FindAction(name, false);
                if (action == null)
                    continue;
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (!SpeakHintsPathsMatch(binding.effectivePath, path))
                        continue;
                    // The default right stick press is only the button part
                    // of the native Right Shoulder + stick DebugMenu chord.
                    if (name == "DebugMenu" && binding.isPartOfComposite &&
                        SpeakHintsPathsMatch(path, "<Gamepad>/rightStickPress"))
                        continue;
                    return true;
                }
            }
        }
        return false;
    }

    private void ResetSpeakHintsControlBindings()
    {
        InputAction action = EnsureSpeakHintsAction();
        bool wasEnabled = action.enabled;
        if (wasEnabled)
            action.Disable();
        try
        {
            InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
            InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
            PlayerPrefs.DeleteKey(SpeakHintsKeyboardKey);
            PlayerPrefs.DeleteKey(SpeakHintsGamepadKey);
            SaveModPreferencesAndConfig();
        }
        finally
        {
            if (wasEnabled)
                action.Enable();
            _speakHintsSuppressPressThroughFrame = Time.frameCount + 1;
        }
        RefreshSpeakHintsControlPrompts();
        WriteStatus("Reset Speak Hints bindings to H and right stick press.");
    }

    private void CancelSpeakHintsControlRebinding(bool announce = true)
    {
        bool wasActive = _speakHintsRebindOperation != null;
        ReleaseSpeakHintsControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseSpeakHintsControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _speakHintsRebindOperation;
        InputActionMap? uiMap = _speakHintsRebindUiMap;
        bool uiWasEnabled = _speakHintsRebindUiMapWasEnabled;
        bool actionWasEnabled = _speakHintsRebindActionWasEnabled;
        _speakHintsRebindOperation = null;
        _speakHintsRebindManager = null;
        _speakHintsRebindUiMap = null;
        _speakHintsRebindUiMapWasEnabled = false;
        _speakHintsRebindActionWasEnabled = false;
        _speakHintsRebindIndex = 0;
        _speakHintsRebindOriginalPath = null;
        if (operation != null)
        {
            if (operation.started && !operation.completed && !operation.canceled)
                operation.Cancel();
            operation.Dispose();
        }
        if (uiMap != null && uiWasEnabled)
            uiMap.Enable();
        if (_speakHintsAction != null && actionWasEnabled)
            _speakHintsAction.Enable();
        if (operation != null)
            _speakHintsSuppressPressThroughFrame = Time.frameCount + 1;
    }

    private void RestoreSpeakHintsOriginalOverride()
    {
        InputAction? action = _speakHintsAction;
        int index = _speakHintsRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_speakHintsRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _speakHintsRebindOriginalPath);
    }

    private void RefreshSpeakHintsControlPrompts()
    {
        try
        {
            _speakHintsControlRow?.ActiveDisplayPrompt?.Refresh();
            _speakHintsControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Speak Hints prompt: " + ex.Message);
        }
    }

    private static bool SpeakHintsPathsMatch(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;
        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            return true;
        string? controlA = InputSystem.FindControl(a)?.path;
        string? controlB = InputSystem.FindControl(b)?.path;
        return controlA != null && controlB != null &&
            string.Equals(controlA, controlB, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record AddedSpeakHintsControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
