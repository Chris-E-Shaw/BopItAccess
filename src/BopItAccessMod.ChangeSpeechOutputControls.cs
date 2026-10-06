using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string ChangeSpeechOutputKeyboardKey = "BopItAccess.ChangeSpeechOutput.Keyboard";
    private const string ChangeSpeechOutputGamepadKey = "BopItAccess.ChangeSpeechOutput.Gamepad";
    private InputActionAsset? _changeSpeechOutputActionAsset;
    private InputAction? _changeSpeechOutputAction;
    private AddedChangeSpeechOutputControlRow? _changeSpeechOutputControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _changeSpeechOutputRebindOperation;
    private InputRebindingManager? _changeSpeechOutputRebindManager;
    private InputActionMap? _changeSpeechOutputRebindUiMap;
    private bool _changeSpeechOutputRebindUiMapWasEnabled;
    private bool _changeSpeechOutputRebindActionWasEnabled;
    private int _changeSpeechOutputRebindIndex;
    private string? _changeSpeechOutputRebindOriginalPath;
    private long _nextChangeSpeechOutputControlErrorAt;
    private int _changeSpeechOutputSuppressPressThroughFrame = -1;
    private bool _changeSpeechOutputAnyRebindWasActive;

    private InputAction EnsureChangeSpeechOutputAction()
    {
        if (_changeSpeechOutputAction != null)
            return _changeSpeechOutputAction;

        // This asset belongs to the mod. It can be bound to a native prompt,
        // while its override keys remain independent of the game's save file.
        _changeSpeechOutputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = _changeSpeechOutputActionAsset.AddActionMap("BopItAccess");
        InputAction action = map.AddAction("ChangeSpeechOutput", InputActionType.Button);
        action.AddBinding("<Keyboard>/f9");
        action.AddBinding("<Gamepad>/buttonWest");
        string keyboard = PlayerPrefs.GetString(ChangeSpeechOutputKeyboardKey, string.Empty);
        string gamepad = PlayerPrefs.GetString(ChangeSpeechOutputGamepadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(keyboard))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 0, keyboard);
        if (!string.IsNullOrWhiteSpace(gamepad))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 1, gamepad);
        _changeSpeechOutputAction = action;
        WriteStatus("Change Speech Output input initialized with keyboard and controller bindings.");
        return action;
    }

    private bool WasChangeSpeechOutputPressed()
    {
        try
        {
            bool rebinding = _changeSpeechOutputRebindOperation != null ||
                _descriptionRebindOperation != null ||
                _scoreRebindOperation != null ||
                _toggleSpeechRebindOperation != null ||
                _speakHintsRebindOperation != null ||
                _leaderboardRebindOperation != null ||
                _resetGyroRebindOperation != null ||
                _controlsRebindingManager?.IsRebinding == true;
            if (rebinding || !_speechEnabled ||
                _speechSuppressedForBackground)
            {
                if (_changeSpeechOutputAction?.enabled == true)
                    _changeSpeechOutputAction.Disable();
                if (rebinding)
                    _changeSpeechOutputAnyRebindWasActive = true;
                return false;
            }

            if (_changeSpeechOutputAnyRebindWasActive)
            {
                // The final captured input must not also change the output.
                _changeSpeechOutputAnyRebindWasActive = false;
                _changeSpeechOutputSuppressPressThroughFrame = Time.frameCount + 1;
                return false;
            }

            InputAction action = EnsureChangeSpeechOutputAction();
            if (!action.enabled)
                action.Enable();
            if (Time.frameCount <= _changeSpeechOutputSuppressPressThroughFrame)
                return false;

            // Native Invincible is Right Shoulder + West. The unmodified West
            // button is available; leave the game's modifier chord intact.
            foreach (Gamepad gamepad in Gamepad.all)
                if (gamepad.rightShoulder.isPressed &&
                    gamepad.buttonWest.wasPressedThisFrame)
                    return false;

            return action.WasPressedThisFrame();
        }
        catch (Exception ex)
        {
            try
            {
                if (_changeSpeechOutputAction?.enabled == true)
                    _changeSpeechOutputAction.Disable();
            }
            catch (Exception cleanup)
            {
                WriteStatus("Change Speech Output input cleanup failed: " + cleanup.Message);
            }
            if (Environment.TickCount64 >= _nextChangeSpeechOutputControlErrorAt)
            {
                WriteStatus("Change Speech Output input failed: " + ex);
                _nextChangeSpeechOutputControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private void UpdateChangeSpeechOutputFromInput()
    {
        if (!WasChangeSpeechOutputPressed())
            return;

        int current = Array.FindIndex(OutputModes,
            mode => string.Equals(mode, _outputMode, StringComparison.OrdinalIgnoreCase));
        string next = OutputModes[(Math.Max(0, current) + 1) % OutputModes.Length];
        SetOutputModeFromMenu(next);
        _speechModeSlider?.SetValue(L(_outputMode));
        QueueSpeech(LF("Output mode, {0}", L(_outputMode)));
    }

    private string ChangeSpeechOutputBindingInstruction()
    {
        try
        {
            InputAction action = EnsureChangeSpeechOutputAction();
            string keyboard = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ?? "F9";
            string gamepad = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ?? "West button";
            return FormatHintPress(keyboard, gamepad,
                "change speech output");
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Change Speech Output bindings: " + ex.Message);
            string keyboard = HintSavedBinding(ChangeSpeechOutputKeyboardKey, "F9");
            string gamepad = HintSavedBinding(ChangeSpeechOutputGamepadKey,
                "West button");
            return FormatHintPress(keyboard, gamepad,
                "change speech output");
        }
    }

    private void AddChangeSpeechOutputControlRow(ControlRow template, Transform parent, int insertAt)
    {
        InputAction action = EnsureChangeSpeechOutputAction();
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "CHANGE_SPEECH_OUTPUT";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
            throw new InvalidOperationException("The Change Speech Output row lost a native control.");

        row.ActionMapName = "BopItAccess";
        row.ActionName = "ChangeSpeechOutput";
        SetClonedLabel(defaultLabel, "CHANGE SPEECH OUTPUT");
        SetClonedLabel(activeLabel, "CHANGE SPEECH OUTPUT");
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
            throw new InvalidOperationException("The Change Speech Output row lost its focus containers.");
        _changeSpeechOutputControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Match the leaderboard rows: the native listener would try to find
        // this action in the game's asset, so only the visual row is kept.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedChangeSpeechOutputControlRow? FindAddedChangeSpeechOutputControlRow(GameObject? selected)
    {
        AddedChangeSpeechOutputControlRow? row = _changeSpeechOutputControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private string? ChangeSpeechOutputControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction action = EnsureChangeSpeechOutputAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = IsGamepadDevice(device) ? 1 : 0;
        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? ChangeSpeechOutputControlSnapshot()
    {
        InputAction action = EnsureChangeSpeechOutputAction();
        string? keyboard = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        string? gamepad = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 1));
        return keyboard == null || gamepad == null ? null : keyboard + "\u001e" + gamepad;
    }

    private void ChangeSpeechOutputControlRow(AddedChangeSpeechOutputControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ChangeSpeechOutputControlBinding(manager);
        bool rebinding = _changeSpeechOutputRebindOperation != null;

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType(L("Change Speech Output"), "button");
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
            if (rebinding)
            {
                // Completion callbacks announce only accepted assignments.
                _lastControlsRebinding = true;
                return;
            }
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

    private void UpdateChangeSpeechOutputControlRebinding()
    {
        try
        {
            UpdateChangeSpeechOutputControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _changeSpeechOutputRebindOperation != null;
            if (Environment.TickCount64 >= _nextChangeSpeechOutputControlErrorAt)
            {
                WriteStatus("Change Speech Output rebinding failed: " + ex);
                _nextChangeSpeechOutputControlErrorAt = Environment.TickCount64 + 5000;
            }
            try
            {
                if (wasRebinding)
                    RestoreChangeSpeechOutputOriginalOverride();
                CancelChangeSpeechOutputControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Change Speech Output rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _changeSpeechOutputControlRow = null;
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
        }
    }

    private void UpdateChangeSpeechOutputControlRebindingCore()
    {
        AddedChangeSpeechOutputControlRow? row = _changeSpeechOutputControlRow;
        // Destroyed Unity objects still have a managed wrapper; null
        // propagation does not detect them. Check before calling Unity.
        if (row == null || row.Root == null)
        {
            _changeSpeechOutputControlRow = null;
            if (_changeSpeechOutputRebindOperation != null)
            {
                RestoreChangeSpeechOutputOriginalOverride();
                CancelChangeSpeechOutputControlRebinding(false);
            }
            return;
        }

        MainMenuUIManager? main = _mainMenu;
        Panel? panel = main == null ? null : main.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_changeSpeechOutputRebindOperation != null)
            {
                RestoreChangeSpeechOutputOriginalOverride();
                CancelChangeSpeechOutputControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedChangeSpeechOutputControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_changeSpeechOutputRebindOperation != null)
        {
            if (_changeSpeechOutputRebindOperation.completed)
                CompleteChangeSpeechOutputControlRebinding();
            else if (_changeSpeechOutputRebindOperation.canceled)
                CancelChangeSpeechOutputControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding || !WasControlsSubmitPressed(manager))
            return;

        if (focused)
        {
            StartChangeSpeechOutputControlRebinding(manager);
            return;
        }

        // The native Reset row handles the game's own bindings. Include the
        // two mod-owned overrides in the same user action.
        ResetToDefaultRow? reset = _controlsResetRow ??
            row.Root.GetComponentInParent<Panel>()?.GetComponentInChildren<ResetToDefaultRow>(true);
        if (reset != null && selected != null &&
            (selected.GetInstanceID() == reset.gameObject.GetInstanceID() ||
             selected.transform.IsChildOf(reset.transform)))
            ResetChangeSpeechOutputControlBindings();
    }

    private void StartChangeSpeechOutputControlRebinding(InputRebindingManager manager)
    {
        InputAction action = EnsureChangeSpeechOutputAction();
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
            _changeSpeechOutputRebindOperation = operation;
            _changeSpeechOutputRebindManager = manager;
            _changeSpeechOutputRebindUiMap = uiMap;
            _changeSpeechOutputRebindUiMapWasEnabled = uiWasEnabled;
            _changeSpeechOutputRebindActionWasEnabled = actionWasEnabled;
            _changeSpeechOutputRebindIndex = index;
            _changeSpeechOutputRebindOriginalPath = original;
            WriteStatus($"Rebinding Change Speech Output on {device}, binding {index}.");
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

    private void CompleteChangeSpeechOutputControlRebinding()
    {
        InputAction? action = _changeSpeechOutputAction;
        InputRebindingManager? manager = _changeSpeechOutputRebindManager;
        int index = _changeSpeechOutputRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved))
        {
            RestoreChangeSpeechOutputOriginalOverride();
            ReleaseChangeSpeechOutputControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        if (IsBindingAssignedElsewhere(manager, action, index, resolved,
                out string owner))
        {
            RestoreChangeSpeechOutputOriginalOverride();
            ReleaseChangeSpeechOutputControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        PlayerPrefs.SetString(index == 1 ? ChangeSpeechOutputGamepadKey : ChangeSpeechOutputKeyboardKey, path!);
        SaveModPreferencesAndConfig();
        ReleaseChangeSpeechOutputControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshChangeSpeechOutputControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Change Speech Output to {path}.");
        QueueControlAssignmentSpeech(_lastControlsBinding, "Change Speech Output");
    }

    private void ResetChangeSpeechOutputControlBindings()
    {
        InputAction action = EnsureChangeSpeechOutputAction();
        bool wasEnabled = action.enabled;
        if (wasEnabled)
            action.Disable();
        try
        {
            InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
            InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
            PlayerPrefs.DeleteKey(ChangeSpeechOutputKeyboardKey);
            PlayerPrefs.DeleteKey(ChangeSpeechOutputGamepadKey);
            SaveModPreferencesAndConfig();
        }
        finally
        {
            if (wasEnabled)
                action.Enable();
            _changeSpeechOutputSuppressPressThroughFrame = Time.frameCount + 1;
        }
        RefreshChangeSpeechOutputControlPrompts();
        WriteStatus("Reset Change Speech Output bindings to F9 and West button.");
    }

    private void CancelChangeSpeechOutputControlRebinding(bool announce = true)
    {
        bool wasActive = _changeSpeechOutputRebindOperation != null;
        ReleaseChangeSpeechOutputControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseChangeSpeechOutputControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _changeSpeechOutputRebindOperation;
        InputActionMap? uiMap = _changeSpeechOutputRebindUiMap;
        bool uiWasEnabled = _changeSpeechOutputRebindUiMapWasEnabled;
        bool actionWasEnabled = _changeSpeechOutputRebindActionWasEnabled;
        _changeSpeechOutputRebindOperation = null;
        _changeSpeechOutputRebindManager = null;
        _changeSpeechOutputRebindUiMap = null;
        _changeSpeechOutputRebindUiMapWasEnabled = false;
        _changeSpeechOutputRebindActionWasEnabled = false;
        _changeSpeechOutputRebindIndex = 0;
        _changeSpeechOutputRebindOriginalPath = null;
        if (operation != null)
        {
            if (operation.started && !operation.completed && !operation.canceled)
                operation.Cancel();
            operation.Dispose();
        }
        if (uiMap != null && uiWasEnabled)
            uiMap.Enable();
        if (_changeSpeechOutputAction != null && actionWasEnabled)
            _changeSpeechOutputAction.Enable();
        if (operation != null)
            _changeSpeechOutputSuppressPressThroughFrame = Time.frameCount + 1;
    }

    private void RestoreChangeSpeechOutputOriginalOverride()
    {
        InputAction? action = _changeSpeechOutputAction;
        int index = _changeSpeechOutputRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_changeSpeechOutputRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _changeSpeechOutputRebindOriginalPath);
    }

    private void RefreshChangeSpeechOutputControlPrompts()
    {
        try
        {
            _changeSpeechOutputControlRow?.ActiveDisplayPrompt?.Refresh();
            _changeSpeechOutputControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Change Speech Output prompt: " + ex.Message);
        }
    }

    private sealed record AddedChangeSpeechOutputControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
