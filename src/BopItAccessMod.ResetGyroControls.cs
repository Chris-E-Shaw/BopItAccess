using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string ResetGyroActionName = "ResetGyro";
    private AddedResetGyroControlRow? _resetGyroControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _resetGyroRebindOperation;
    private InputAction? _resetGyroRebindAction;
    private InputRebindingManager? _resetGyroRebindManager;
    private ControlRebindInputState? _resetGyroRebindInputState;
    private int _resetGyroRebindIndex;
    private string? _resetGyroRebindOriginalPath;
    private long _nextResetGyroControlErrorAt;

    private void AddResetGyroControlRow(ControlRow template, Transform parent,
        int insertAt, InputRebindingManager manager)
    {
        InputAction? action = FindResetGyroAction(manager.inputActions) ??
            FindResetGyroAction(manager.playerInput?.actions);
        if (action == null || FindResetGyroBindingIndex(action, false) < 0 ||
            FindResetGyroBindingIndex(action, true) < 0)
            throw new InvalidOperationException(
                "The native Reset Gyro keyboard and controller bindings are unavailable.");

        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject,
            parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "RESET_GYRO";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null ||
            selectable == null)
            throw new InvalidOperationException("The Reset Gyro row lost a native control.");

        row.ActionMapName = action.actionMap.name;
        row.ActionName = ResetGyroActionName;
        SetClonedLabel(defaultLabel, "RESET GYRO");
        SetClonedLabel(activeLabel, "RESET GYRO");
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
            throw new InvalidOperationException("The Reset Gyro row lost its focus containers.");
        _resetGyroControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Added native ControlRow listeners previously caused a crash. Keep
        // the visual row and perform rebinding through our own operation.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedResetGyroControlRow? FindAddedResetGyroControlRow(
        GameObject? selected)
    {
        AddedResetGyroControlRow? row = _resetGyroControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private static InputAction? FindResetGyroAction(InputActionAsset? asset) =>
        asset?.FindAction(ResetGyroActionName, false);

    private static int FindResetGyroBindingIndex(InputAction action, bool gamepad)
    {
        string layout = gamepad ? "<Gamepad>/" : "<Keyboard>/";
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (!binding.isComposite && !binding.isPartOfComposite &&
                (binding.path ?? string.Empty).StartsWith(layout,
                    StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private string? ReadResetGyroControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction? action = FindResetGyroAction(manager.inputActions) ??
            FindResetGyroAction(manager.playerInput?.actions);
        if (action == null)
            return null;
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = FindResetGyroBindingIndex(action, IsGamepadDevice(device));
        return index < 0 ? null : CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? ReadResetGyroControlSnapshot()
    {
        InputRebindingManager? manager = _controlsRebindingManager;
        InputAction? action = FindResetGyroAction(manager?.inputActions) ??
            FindResetGyroAction(manager?.playerInput?.actions);
        if (action == null)
            return null;
        int keyboard = FindResetGyroBindingIndex(action, false);
        int gamepad = FindResetGyroBindingIndex(action, true);
        if (keyboard < 0 || gamepad < 0)
            return null;
        return InputActionRebindingExtensions.GetBindingDisplayString(action, keyboard) +
            "|" + InputActionRebindingExtensions.GetBindingDisplayString(action, gamepad);
    }

    private void ReadResetGyroControlRow(AddedResetGyroControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ReadResetGyroControlBinding(manager);
        bool rebinding = _resetGyroRebindOperation != null;
        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType(L("Reset Gyro"), "button");
            string message = binding == null ? label : LF("{0}, {1}", label,
                LocalizeBindingDisplay(binding));
            if (rebinding)
                message += L(".") + " " + L("Listening for input");
            QueueFocusSpeech(WithControlsIntroduction(message));
            return;
        }

        if (binding != null &&
            !string.Equals(binding, _lastControlsBinding, StringComparison.Ordinal))
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
            QueueSpeech(L("Listening for input"));
        else if (!rebinding && _lastControlsRebinding &&
                 !_controlsBindingChangedDuringRebind)
            QueueSpeech(L("Binding unchanged"));
        _lastControlsRebinding = rebinding;
    }

    private void UpdateResetGyroControlRebinding()
    {
        try
        {
            UpdateResetGyroControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _resetGyroRebindOperation != null;
            if (Environment.TickCount64 >= _nextResetGyroControlErrorAt)
            {
                WriteStatus("Reset Gyro rebinding failed: " + ex);
                _nextResetGyroControlErrorAt = Environment.TickCount64 + 5000;
            }
            CancelResetGyroControlRebinding(false);
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
        }
    }

    private void UpdateResetGyroControlRebindingCore()
    {
        AddedResetGyroControlRow? row = _resetGyroControlRow;
        if (row == null || row.Root == null)
        {
            _resetGyroControlRow = null;
            if (_resetGyroRebindOperation != null)
            {
                CancelResetGyroControlRebinding(false);
            }
            return;
        }

        Panel? panel = _mainMenu?.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_resetGyroRebindOperation != null)
            {
                CancelResetGyroControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedResetGyroControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_resetGyroRebindOperation != null)
        {
            if (_resetGyroRebindOperation.completed)
                CompleteResetGyroControlRebinding();
            else if (_resetGyroRebindOperation.canceled)
                CancelResetGyroControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager != null && focused && !manager.IsRebinding &&
            WasControlsSubmitPressed(manager))
            StartResetGyroControlRebinding(manager);
    }

    private void StartResetGyroControlRebinding(InputRebindingManager manager)
    {
        InputAction? action = FindResetGyroAction(manager.inputActions) ??
            FindResetGyroAction(manager.playerInput?.actions);
        if (action == null)
        {
            QueueSpeech(L("Reset Gyro binding unavailable"));
            return;
        }
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        bool gamepad = IsGamepadDevice(device);
        int index = FindResetGyroBindingIndex(action, gamepad);
        if (index < 0)
        {
            QueueSpeech(L("Reset Gyro binding unavailable for this device"));
            return;
        }
        string? originalPath = action.bindings[index].overridePath;

        string layout = gamepad ? "<Gamepad>" : "<Keyboard>";
        string cancelPath = gamepad ? "<Gamepad>/buttonEast" : "<Keyboard>/escape";
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
            _resetGyroRebindOperation = operation;
            _resetGyroRebindAction = action;
            _resetGyroRebindManager = manager;
            _resetGyroRebindInputState = inputState;
            _resetGyroRebindIndex = index;
            _resetGyroRebindOriginalPath = originalPath;
            WriteStatus($"Rebinding Reset Gyro on {device}, binding {index}.");
        }
        catch
        {
            ReleaseControlRebindingCapture(operation, inputState);
            throw;
        }
    }

    private void CompleteResetGyroControlRebinding()
    {
        InputAction? action = _resetGyroRebindAction;
        InputRebindingManager? manager = _resetGyroRebindManager;
        int index = _resetGyroRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved))
        {
            RestoreResetGyroOriginalOverride();
            ReleaseResetGyroControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        if (IsBindingAssignedElsewhere(manager, action, index, resolved,
                out string owner))
        {
            RestoreResetGyroOriginalOverride();
            ReleaseResetGyroControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        string normalized = ControlNameFormatter.Format(resolved);
        if (!manager.ValidateBinding(normalized, resolved, action,
                out string invalidMessage))
        {
            RestoreResetGyroOriginalOverride();
            ReleaseResetGyroControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L(CleanSpeechValue(invalidMessage) ?? "Binding unavailable"));
            return;
        }

        // The manager saves native overrides. Keep the gameplay action copy
        // synchronized with the asset used by the Controls panel.
        InputAction? runtime = FindResetGyroAction(manager.playerInput?.actions);
        int runtimeIndex = runtime == null ? -1 : FindResetGyroBindingIndex(runtime,
            IsGamepadDevice(manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty));
        string? previousRuntimePath = runtimeIndex < 0 ? null :
            runtime!.bindings[runtimeIndex].overridePath;
        try
        {
            if (runtimeIndex >= 0)
                InputActionRebindingExtensions.ApplyBindingOverride(runtime!,
                    runtimeIndex, path);
            manager.SaveBindings();
        }
        catch
        {
            if (runtimeIndex >= 0)
            {
                try { RestoreNativeBindingOverride(runtime!, runtimeIndex, previousRuntimePath); }
                catch (Exception ex) { WriteStatus("Could not restore runtime Reset Gyro binding: " + ex.Message); }
            }
            throw;
        }
        ReleaseResetGyroControlRebinding();
        InputRebindingEvents.RebindCompleted?.Invoke(ResetGyroActionName,
            InputActionRebindingExtensions.GetBindingDisplayString(action, index));
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshResetGyroControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Reset Gyro to {path}.");
        QueueControlAssignmentSpeech(_lastControlsBinding, "Reset Gyro");
    }

    private void CancelResetGyroControlRebinding(bool announce = true)
    {
        bool wasActive = _resetGyroRebindOperation != null;
        if (wasActive)
        {
            try { RestoreResetGyroOriginalOverride(); }
            catch (Exception ex) { WriteStatus("Could not restore ResetGyro binding: " + ex.Message); }
        }
        ReleaseResetGyroControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseResetGyroControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _resetGyroRebindOperation;
        ControlRebindInputState? inputState = _resetGyroRebindInputState;
        _resetGyroRebindOperation = null;
        _resetGyroRebindManager = null;
        _resetGyroRebindInputState = null;
        _resetGyroRebindIndex = 0;
        _resetGyroRebindOriginalPath = null;
        _resetGyroRebindAction = null;
        ReleaseControlRebindingCapture(operation, inputState);
    }

    private void RestoreResetGyroOriginalOverride()
    {
        InputAction? action = _resetGyroRebindAction;
        int index = _resetGyroRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_resetGyroRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _resetGyroRebindOriginalPath);
    }

    private void RefreshResetGyroControlPrompts()
    {
        try
        {
            _resetGyroControlRow?.ActiveDisplayPrompt?.Refresh();
            _resetGyroControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Reset Gyro prompt: " + ex.Message);
        }
    }

    private sealed record AddedResetGyroControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);
}
