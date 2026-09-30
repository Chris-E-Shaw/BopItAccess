using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string DescriptionKeyboardKey = "BopItAccess.ReadDescriptions.Keyboard";
    private const string DescriptionGamepadKey = "BopItAccess.ReadDescriptions.Gamepad";
    private InputActionAsset? _descriptionActionAsset;
    private InputAction? _descriptionAction;
    private AddedDescriptionControlRow? _descriptionControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _descriptionRebindOperation;
    private InputRebindingManager? _descriptionRebindManager;
    private InputActionMap? _descriptionRebindUiMap;
    private bool _descriptionRebindUiMapWasEnabled;
    private bool _descriptionRebindActionWasEnabled;
    private int _descriptionRebindIndex;
    private string? _descriptionRebindOriginalPath;
    private long _nextDescriptionControlErrorAt;

    private InputAction EnsureDescriptionAction()
    {
        if (_descriptionAction != null)
            return _descriptionAction;

        // This asset belongs to the mod. It can be bound to a native prompt,
        // while its override keys remain independent of the game's save file.
        _descriptionActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = _descriptionActionAsset.AddActionMap("BopItAccess");
        InputAction action = map.AddAction("ReadDescriptions", InputActionType.Button);
        action.AddBinding("<Keyboard>/r");
        action.AddBinding("<Gamepad>/leftTrigger");
        string keyboard = PlayerPrefs.GetString(DescriptionKeyboardKey, string.Empty);
        string gamepad = PlayerPrefs.GetString(DescriptionGamepadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(keyboard))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 0, keyboard);
        if (!string.IsNullOrWhiteSpace(gamepad))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 1, gamepad);
        _descriptionAction = action;
        WriteStatus("Read Descriptions input initialized with keyboard and controller bindings.");
        return action;
    }

    private bool WasReadDescriptionsPressed(bool songSelectionActive)
    {
        try
        {
            if (!songSelectionActive)
            {
                // This runs frequently outside song selection, including
                // before the action has been created.
                if (_descriptionAction?.enabled == true)
                    _descriptionAction.Disable();
                return false;
            }

            InputAction action = EnsureDescriptionAction();
            if (!action.enabled)
                action.Enable();
            return action.WasPressedThisFrame();
        }
        catch (Exception ex)
        {
            try
            {
                if (_descriptionAction?.enabled == true)
                    _descriptionAction.Disable();
            }
            catch (Exception cleanup)
            {
                WriteStatus("Read Descriptions input cleanup failed: " + cleanup.Message);
            }
            if (Environment.TickCount64 >= _nextDescriptionControlErrorAt)
            {
                WriteStatus("Read Descriptions input failed: " + ex);
                _nextDescriptionControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private string ReadDescriptionsBindingInstruction()
    {
        try
        {
            InputAction action = EnsureDescriptionAction();
            string keyboard = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ?? "R";
            string gamepad = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ?? "left trigger";
            return $"Press {keyboard} on keyboard or {gamepad} on controller to read the selected stage description.";
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Read Descriptions bindings: " + ex.Message);
            string keyboard = HintSavedBinding(DescriptionKeyboardKey, "R");
            string gamepad = HintSavedBinding(DescriptionGamepadKey, "left trigger");
            return $"Press {keyboard} on keyboard or {gamepad} on controller to read the selected stage description.";
        }
    }

    private void AddDescriptionControlRow(ControlRow template, Transform parent, int insertAt)
    {
        InputAction action = EnsureDescriptionAction();
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "READ_DESCRIPTIONS";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
            throw new InvalidOperationException("The Read Descriptions row lost a native control.");

        row.ActionMapName = "BopItAccess";
        row.ActionName = "ReadDescriptions";
        SetClonedLabel(defaultLabel, "READ DESCRIPTIONS");
        SetClonedLabel(activeLabel, "READ DESCRIPTIONS");
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
            throw new InvalidOperationException("The Read Descriptions row lost its focus containers.");
        _descriptionControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Match the leaderboard rows: the native listener would try to find
        // this action in the game's asset, so only the visual row is kept.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedDescriptionControlRow? FindAddedDescriptionControlRow(GameObject? selected)
    {
        AddedDescriptionControlRow? row = _descriptionControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private string? ReadDescriptionControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction action = EnsureDescriptionAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = IsGamepadDevice(device) ? 1 : 0;
        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? ReadDescriptionControlSnapshot()
    {
        InputAction action = EnsureDescriptionAction();
        string? keyboard = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        string? gamepad = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 1));
        return keyboard == null || gamepad == null ? null : keyboard + "\u001e" + gamepad;
    }

    private void ReadDescriptionControlRow(AddedDescriptionControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ReadDescriptionControlBinding(manager);
        bool rebinding = _descriptionRebindOperation != null;

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType("Read Descriptions", "button");
            string message = binding == null ? label : $"{label}, {binding}";
            if (rebinding)
                message += ". Listening for input";
            QueueFocusSpeech(WithControlsIntroduction(message));
            return;
        }

        if (binding != null && !string.Equals(binding, _lastControlsBinding,
                StringComparison.Ordinal))
        {
            _lastControlsBinding = binding;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = true;
            QueueSpeech(binding);
            return;
        }

        if (rebinding && !_lastControlsRebinding)
        {
            _controlsBindingChangedDuringRebind = false;
            QueueSpeech("Listening for input");
        }
        else if (!rebinding && _lastControlsRebinding &&
            !_controlsBindingChangedDuringRebind)
            QueueSpeech("Binding unchanged");

        _lastControlsRebinding = rebinding;
    }

    private void UpdateDescriptionControlRebinding()
    {
        try
        {
            UpdateDescriptionControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _descriptionRebindOperation != null;
            if (Environment.TickCount64 >= _nextDescriptionControlErrorAt)
            {
                WriteStatus("Read Descriptions rebinding failed: " + ex);
                _nextDescriptionControlErrorAt = Environment.TickCount64 + 5000;
            }
            try
            {
                if (wasRebinding)
                    RestoreDescriptionOriginalOverride();
                CancelDescriptionControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Read Descriptions rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _descriptionControlRow = null;
            if (wasRebinding)
                QueueSpeech("Rebinding failed");
        }
    }

    private void UpdateDescriptionControlRebindingCore()
    {
        AddedDescriptionControlRow? row = _descriptionControlRow;
        // Destroyed Unity objects still have a managed wrapper; null
        // propagation does not detect them. Check before calling Unity.
        if (row == null || row.Root == null)
        {
            _descriptionControlRow = null;
            if (_descriptionRebindOperation != null)
            {
                RestoreDescriptionOriginalOverride();
                CancelDescriptionControlRebinding(false);
            }
            return;
        }

        MainMenuUIManager? main = _mainMenu;
        Panel? panel = main == null ? null : main.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_descriptionRebindOperation != null)
            {
                RestoreDescriptionOriginalOverride();
                CancelDescriptionControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedDescriptionControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_descriptionRebindOperation != null)
        {
            if (_descriptionRebindOperation.completed)
                CompleteDescriptionControlRebinding();
            else if (_descriptionRebindOperation.canceled)
                CancelDescriptionControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding || !WasControlsSubmitPressed(manager))
            return;

        if (focused)
        {
            StartDescriptionControlRebinding(manager);
            return;
        }

        // The native Reset row handles the game's own bindings. Include the
        // two mod-owned overrides in the same user action.
        ResetToDefaultRow? reset = _controlsResetRow ??
            row.Root.GetComponentInParent<Panel>()?.GetComponentInChildren<ResetToDefaultRow>(true);
        if (reset != null && selected != null &&
            (selected.GetInstanceID() == reset.gameObject.GetInstanceID() ||
             selected.transform.IsChildOf(reset.transform)))
            ResetDescriptionControlBindings();
    }

    private void StartDescriptionControlRebinding(InputRebindingManager manager)
    {
        InputAction action = EnsureDescriptionAction();
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
            _descriptionRebindOperation = operation;
            _descriptionRebindManager = manager;
            _descriptionRebindUiMap = uiMap;
            _descriptionRebindUiMapWasEnabled = uiWasEnabled;
            _descriptionRebindActionWasEnabled = actionWasEnabled;
            _descriptionRebindIndex = index;
            _descriptionRebindOriginalPath = original;
            WriteStatus($"Rebinding Read Descriptions on {device}, binding {index}.");
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

    private void CompleteDescriptionControlRebinding()
    {
        InputAction? action = _descriptionAction;
        InputRebindingManager? manager = _descriptionRebindManager;
        int index = _descriptionRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved) ||
            IsEssentialNativeDescriptionBinding(manager, resolved))
        {
            RestoreDescriptionOriginalOverride();
            ReleaseDescriptionControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(string.IsNullOrEmpty(resolved) ? "Binding unavailable" :
                "That input is used to start or leave the screen");
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        PlayerPrefs.SetString(index == 1 ? DescriptionGamepadKey : DescriptionKeyboardKey, path!);
        PlayerPrefs.Save();
        ReleaseDescriptionControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshDescriptionControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Read Descriptions to {path}.");
        QueueSpeech(_lastControlsBinding ?? "Binding changed");
    }

    private static bool IsEssentialNativeDescriptionBinding(InputRebindingManager manager,
        string path)
    {
        InputActionAsset? asset = manager.inputActions ?? manager.playerInput?.actions;
        if (asset == null)
            return false;
        string[] essential = { "Bop", "Twist", "Pull", "Spin", "Flick",
            "Submit", "Back", "Cancel" };
        foreach (string name in essential)
        {
            InputAction? action = asset.FindAction(name, false);
            if (action == null)
                continue;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (DescriptionPathsMatch(action.bindings[i].effectivePath, path))
                    return true;
            }
        }
        return false;
    }

    private void ResetDescriptionControlBindings()
    {
        InputAction action = EnsureDescriptionAction();
        InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
        InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
        PlayerPrefs.DeleteKey(DescriptionKeyboardKey);
        PlayerPrefs.DeleteKey(DescriptionGamepadKey);
        PlayerPrefs.Save();
        RefreshDescriptionControlPrompts();
        WriteStatus("Reset Read Descriptions bindings to R and left trigger.");
    }

    private void CancelDescriptionControlRebinding(bool announce = true)
    {
        bool wasActive = _descriptionRebindOperation != null;
        ReleaseDescriptionControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech("Binding unchanged");
    }

    private void ReleaseDescriptionControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _descriptionRebindOperation;
        InputActionMap? uiMap = _descriptionRebindUiMap;
        bool uiWasEnabled = _descriptionRebindUiMapWasEnabled;
        bool actionWasEnabled = _descriptionRebindActionWasEnabled;
        _descriptionRebindOperation = null;
        _descriptionRebindManager = null;
        _descriptionRebindUiMap = null;
        _descriptionRebindUiMapWasEnabled = false;
        _descriptionRebindActionWasEnabled = false;
        _descriptionRebindIndex = 0;
        _descriptionRebindOriginalPath = null;
        if (operation != null)
        {
            if (operation.started && !operation.completed && !operation.canceled)
                operation.Cancel();
            operation.Dispose();
        }
        if (uiMap != null && uiWasEnabled)
            uiMap.Enable();
        if (_descriptionAction != null && actionWasEnabled)
            _descriptionAction.Enable();
    }

    private void RestoreDescriptionOriginalOverride()
    {
        InputAction? action = _descriptionAction;
        int index = _descriptionRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_descriptionRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _descriptionRebindOriginalPath);
    }

    private void RefreshDescriptionControlPrompts()
    {
        try
        {
            _descriptionControlRow?.ActiveDisplayPrompt?.Refresh();
            _descriptionControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Read Descriptions prompt: " + ex.Message);
        }
    }

    private static bool DescriptionPathsMatch(string? a, string? b)
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

    private sealed record AddedDescriptionControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
