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
    private ControlRebindInputState? _descriptionRebindInputState;
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
        action.AddBinding("<Keyboard>/g");
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
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ?? "G";
            string gamepad = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ?? "left trigger";
            return FormatHintPress(keyboard, gamepad,
                L("read stage description"));
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Read Descriptions bindings: " + ex.Message);
            string keyboard = HintSavedBinding(DescriptionKeyboardKey, "G");
            string gamepad = HintSavedBinding(DescriptionGamepadKey, "left trigger");
            return FormatHintPress(keyboard, gamepad,
                L("read stage description"));
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
        SetClonedLabel(defaultLabel, L("READ DESCRIPTIONS"));
        SetClonedLabel(activeLabel, L("READ DESCRIPTIONS"));
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

    private void RefreshDescriptionControlLocale()
    {
        GameObject? root = _descriptionControlRow?.Root;
        if (root == null)
            return;
        TMP_Text? regular = FindControlLabel(root.transform, "Default/Label");
        TMP_Text? active = FindControlLabel(root.transform, "Active/Label");
        if (regular != null)
            SetClonedLabel(regular, L("READ DESCRIPTIONS"));
        if (active != null)
            SetClonedLabel(active, L("READ DESCRIPTIONS"));
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
            string label = WithControlType(L("Read Descriptions"), "button");
            string message = binding == null ? label :
                LF("{0}, {1}", label, LocalizeBindingDisplay(binding));
            if (rebinding)
                message += ". " + L("Listening for input");
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
                CancelDescriptionControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Read Descriptions rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _descriptionControlRow = null;
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
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
        ControlRebindInputState? inputState = null;

        InputActionRebindingExtensions.RebindingOperation? operation = null;
        try
        {
            inputState = PauseControlRebindInputs(manager, action);
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
            _descriptionRebindInputState = inputState;
            _descriptionRebindIndex = index;
            _descriptionRebindOriginalPath = original;
            WriteStatus($"Rebinding Read Descriptions on {device}, binding {index}.");
        }
        catch
        {
            ReleaseControlRebindingCapture(operation, inputState);
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
        bool unavailable = action == null || manager == null || string.IsNullOrEmpty(resolved);
        string owner = string.Empty;
        if (unavailable || IsBindingAssignedElsewhere(manager!, action!, index,
                resolved, out owner))
        {
            RestoreDescriptionOriginalOverride();
            ReleaseDescriptionControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(unavailable ? L("Binding unavailable") :
                LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        SaveReboundModControlPreference(index == 1 ? DescriptionGamepadKey : DescriptionKeyboardKey, path!);
        ReleaseDescriptionControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshDescriptionControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Read Descriptions to {path}.");
        QueueControlAssignmentSpeech(_lastControlsBinding, "Read Descriptions");
    }

    private void ResetDescriptionControlBindings()
    {
        InputAction action = EnsureDescriptionAction();
        InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
        InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
        PlayerPrefs.DeleteKey(DescriptionKeyboardKey);
        PlayerPrefs.DeleteKey(DescriptionGamepadKey);
        SaveModPreferencesAndConfig();
        RefreshDescriptionControlPrompts();
        WriteStatus("Reset Read Descriptions bindings to G and left trigger.");
    }

    private void CancelDescriptionControlRebinding(bool announce = true)
    {
        bool wasActive = _descriptionRebindOperation != null;
        if (wasActive)
        {
            try { RestoreDescriptionOriginalOverride(); }
            catch (Exception ex) { WriteStatus("Could not restore Description binding: " + ex.Message); }
        }
        ReleaseDescriptionControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseDescriptionControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _descriptionRebindOperation;
        ControlRebindInputState? inputState = _descriptionRebindInputState;
        _descriptionRebindOperation = null;
        _descriptionRebindManager = null;
        _descriptionRebindInputState = null;
        _descriptionRebindIndex = 0;
        _descriptionRebindOriginalPath = null;
        ReleaseControlRebindingCapture(operation, inputState);
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
        if (string.Equals(ConfigModConflictPath(a), ConfigModConflictPath(b),
                StringComparison.OrdinalIgnoreCase))
            return true;
        // A generic path can match every gamepad. Resolving each path to only
        // its first control misses a duplicate captured on a second controller.
        InputControl? controlA = InputSystem.FindControl(a);
        InputControl? controlB = InputSystem.FindControl(b);
        return (controlB != null && InputControlPath.Matches(a, controlB)) ||
            (controlA != null && InputControlPath.Matches(b, controlA));
    }

    private sealed record AddedDescriptionControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
