using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private static readonly string[] LeaderboardDeviceNames = { "Keyboard", "Gamepad" };
    private InputActionRebindingExtensions.RebindingOperation? _leaderboardRebindOperation;
    private InputAction? _leaderboardRebindAction;
    private InputRebindingManager? _leaderboardRebindManager;
    private ControlRebindInputState? _leaderboardRebindInputState;
    private AddedLeaderboardControlRow? _leaderboardRebindRow;
    private int _leaderboardRebindIndex;
    private string? _leaderboardRebindOriginalOverridePath;
    private long _nextLeaderboardRebindErrorAt;
    private long _nextLeaderboardRuntimeSyncAt;

    private AddedLeaderboardControlRow? FindAddedLeaderboardControlRow(GameObject? selected)
    {
        if (selected == null)
            return null;

        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
        {
            if (row.Root != null &&
                (selected.GetInstanceID() == row.Root.GetInstanceID() ||
                 selected.transform.IsChildOf(row.Root.transform)))
                return row;
        }

        return null;
    }

    private void UpdateLeaderboardControlRebinding()
    {
        try
        {
            UpdateLeaderboardControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _leaderboardRebindOperation != null;
            if (Environment.TickCount64 >= _nextLeaderboardRebindErrorAt)
            {
                WriteStatus("Leaderboard control rebinding failed: " + ex);
                _nextLeaderboardRebindErrorAt = Environment.TickCount64 + 5000;
            }

            CancelLeaderboardControlRebinding(false);
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
        }
    }

    private void UpdateLeaderboardControlRebindingCore()
    {
        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        AddedLeaderboardControlRow? focused = FindAddedLeaderboardControlRow(selected);
        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
        {
            if (row.Root == null || !row.Root.activeInHierarchy)
                continue;

            bool isFocused = ReferenceEquals(row, focused);
            if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != isFocused)
                row.ActiveContainer.SetActive(isFocused);
            if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == isFocused)
                row.DefaultContainer.SetActive(!isFocused);
        }

        if (_leaderboardRebindOperation != null)
        {
            if (_leaderboardRebindRow?.Root == null ||
                !_leaderboardRebindRow.Root.activeInHierarchy)
            {
                CancelLeaderboardControlRebinding(false);
                return;
            }

            if (_leaderboardRebindOperation.completed)
            {
                CompleteLeaderboardControlRebinding();
                return;
            }

            if (_leaderboardRebindOperation.canceled)
            {
                CancelLeaderboardControlRebinding();
                return;
            }

            return;
        }

        if (Environment.TickCount64 >= _nextLeaderboardRuntimeSyncAt)
        {
            _nextLeaderboardRuntimeSyncAt = Environment.TickCount64 + 250;
            SynchronizeLeaderboardRuntimeBindings();
        }

        if (focused == null || !focused.Root.activeInHierarchy)
            return;

        InputRebindingManager? manager = focused.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding ||
            !WasControlsSubmitPressed(manager))
            return;

        StartLeaderboardControlRebinding(focused, manager);
    }

    private bool WasControlsSubmitPressed(InputRebindingManager manager)
    {
        if (manager.IsRebinding || AnyCustomControlRebinding)
            return false;
        // This is the action ControlRow.Start subscribes to in the game.
        InputAction? submit = manager.playerInput?.actions?.FindAction("Submit", false);
        if (submit == null || !submit.enabled || !submit.WasPerformedThisFrame())
            return false;
        if (!_controlsSubmitReady)
        {
            WriteStatus("Ignored a Controls binding submit from the input that opened the panel.");
            return false;
        }
        return true;
    }

    private void StartLeaderboardControlRebinding(AddedLeaderboardControlRow row,
        InputRebindingManager manager)
    {
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        string layout = IsGamepadDevice(device) ? "<Gamepad>" : "<Keyboard>";
        string cancelPath = layout == "<Gamepad>"
            ? "<Gamepad>/buttonEast" : "<Keyboard>/escape";
        InputAction? action = FindLeaderboardAction(manager.inputActions, row.Part) ??
            FindLeaderboardAction(manager.playerInput?.actions, row.Part);
        if (action == null)
        {
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        int index = FindCompositePartIndex(action, device, -1, row.Part.PartName);
        if (index < 0 ||
            !(action.bindings[index].path ?? string.Empty)
                .Contains(layout, StringComparison.OrdinalIgnoreCase))
        {
            QueueSpeech(L("Binding unavailable for this device"));
            return;
        }

        string? originalPath = action.bindings[index].overridePath;
        ControlRebindInputState? inputState = null;

        InputActionRebindingExtensions.RebindingOperation? operation = null;
        try
        {
            inputState = PauseControlRebindInputs(manager, action);
            operation = InputActionRebindingExtensions.PerformInteractiveRebinding(
                action, index);
            operation.WithTargetBinding(index)
                .WithControlsHavingToMatchPath(layout)
                .WithControlsExcluding("<Keyboard>/anyKey")
                .WithControlsExcluding("<Pointer>/*")
                .WithControlsExcluding("<Touchscreen>/*")
                .WithCancelingThrough(cancelPath)
                .OnMatchWaitForAnother(0.1f)
                .Start();
            _leaderboardRebindOperation = operation;
            _leaderboardRebindAction = action;
            _leaderboardRebindManager = manager;
            _leaderboardRebindInputState = inputState;
            _leaderboardRebindRow = row;
            _leaderboardRebindIndex = index;
            _leaderboardRebindOriginalOverridePath = originalPath;
            WriteStatus($"Rebinding {row.Part.Label} on {device}, binding {index}.");
        }
        catch
        {
            ReleaseControlRebindingCapture(operation, inputState);
            throw;
        }
    }

    private void CompleteLeaderboardControlRebinding()
    {
        InputAction? action = _leaderboardRebindAction;
        InputRebindingManager? manager = _leaderboardRebindManager;
        AddedLeaderboardControlRow? row = _leaderboardRebindRow;
        int index = _leaderboardRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;

        if (action == null || manager == null || row == null || string.IsNullOrEmpty(path))
        {
            CancelLeaderboardControlRebinding(false);
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unchanged"));
            return;
        }

        // Unity clears its candidate list immediately after completion, so
        // selectedControl is null by the time OnUpdate polls the operation.
        // The applied override remains available on the exact target binding.
        string? selectedPath = InputSystem.FindControl(path)?.path;
        WriteStatus($"Leaderboard rebind candidate for {row.Part.Label}: " +
            $"override {path}, resolved control {selectedPath ?? "none"}.");
        if (string.IsNullOrEmpty(selectedPath))
        {
            RestoreLeaderboardOriginalOverride();
            ReleaseLeaderboardControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        if (IsBindingAssignedElsewhere(manager, action, index, selectedPath,
                out string owner))
        {
            RestoreLeaderboardOriginalOverride();
            ReleaseLeaderboardControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        // Use the game's reserved-input and conflict checks. Its joystick
        // normalization is for single-binding actions and can overwrite an
        // entire composite, so keep this selected part's path unchanged.
        string normalized = ControlNameFormatter.Format(selectedPath);
        if (!manager.ValidateBinding(normalized, selectedPath, action,
                out string invalidMessage))
        {
            WriteStatus($"Leaderboard rebind rejected for {row.Part.Label}: " +
                (string.IsNullOrEmpty(invalidMessage) ? "no reason supplied" : invalidMessage));
            RestoreLeaderboardOriginalOverride();
            ReleaseLeaderboardControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L(CleanSpeechValue(invalidMessage) ?? "Binding unavailable"));
            return;
        }

        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        InputAction? runtime = FindLeaderboardAction(manager.playerInput?.actions, row.Part);
        int runtimeIndex = runtime == null ? -1 :
            FindCompositePartIndex(runtime, device, -1, row.Part.PartName);
        string? previousRuntimePath = runtimeIndex < 0 ? null :
            runtime!.bindings[runtimeIndex].overridePath;
        try
        {
            if (runtimeIndex >= 0)
                InputActionRebindingExtensions.ApplyBindingOverride(
                    runtime!, runtimeIndex, path);

            // Retain the operation's original source binding until persistence
            // succeeds, so the outer failure path can still cancel and restore.
            manager.SaveBindings();
        }
        catch
        {
            if (runtimeIndex >= 0)
            {
                try { RestoreNativeBindingOverride(runtime!, runtimeIndex, previousRuntimePath); }
                catch (Exception ex) { WriteStatus("Could not restore runtime leaderboard binding: " + ex.Message); }
            }
            throw;
        }
        ReleaseLeaderboardControlRebinding();

        // The native manager owns the game's persistent binding overrides.
        // Its source and runtime action copies have now both been saved.
        WriteStatus($"Rebound {row.Part.Label} to {path}.");
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        // Native completion sends these after saving; existing menu rows and
        // prompt listeners use them to refresh any displaced bindings.
        InputRebindingEvents.RebindCompleted?.Invoke(row.Part.Action, spoken);
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshLeaderboardRowPrompts(row);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        QueueControlAssignmentSpeech(_lastControlsBinding, row.Part.Label);
    }

    private void CancelLeaderboardControlRebinding(bool announce = true)
    {
        bool wasActive = _leaderboardRebindOperation != null;
        if (wasActive)
        {
            try { RestoreLeaderboardOriginalOverride(); }
            catch (Exception ex) { WriteStatus("Could not restore Leaderboard binding: " + ex.Message); }
        }
        ReleaseLeaderboardControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseLeaderboardControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _leaderboardRebindOperation;
        ControlRebindInputState? inputState = _leaderboardRebindInputState;
        _leaderboardRebindOperation = null;
        _leaderboardRebindManager = null;
        _leaderboardRebindInputState = null;
        _leaderboardRebindIndex = 0;
        _leaderboardRebindOriginalOverridePath = null;
        _leaderboardRebindAction = null;
        _leaderboardRebindRow = null;
        ReleaseControlRebindingCapture(operation, inputState);
    }

    private static InputAction? FindLeaderboardAction(InputActionAsset? asset,
        LeaderboardControlPart part) => asset?
            .FindActionMap(LeaderboardControlsMap, false)?
            .FindAction(part.Action, false);

    private void RestoreLeaderboardOriginalOverride()
    {
        InputAction? action = _leaderboardRebindAction;
        int index = _leaderboardRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;

        string? original = _leaderboardRebindOriginalOverridePath;
        if (original == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                original);
    }

    private void SynchronizeLeaderboardRuntimeBindings()
    {
        if (_leaderboardControlRows.Count == 0)
            return;

        AddedLeaderboardControlRow? first = null;
        foreach (AddedLeaderboardControlRow candidate in _leaderboardControlRows.Values)
        {
            first = candidate;
            break;
        }

        if (first?.Root == null || !first.Root.activeInHierarchy)
            return;

        InputRebindingManager? manager = first.Root.GetComponentInParent<InputRebindingManager>();
        if (manager?.inputActions == null || manager.playerInput?.actions == null)
            return;

        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
        {
            InputAction? source = FindLeaderboardAction(manager.inputActions, row.Part);
            InputAction? runtime = FindLeaderboardAction(manager.playerInput.actions, row.Part);
            if (source == null || runtime == null)
                continue;

            bool changed = false;
            foreach (string device in LeaderboardDeviceNames)
            {
                int sourceIndex = FindCompositePartIndex(source, device, -1,
                    row.Part.PartName);
                int runtimeIndex = FindCompositePartIndex(runtime, device, -1,
                    row.Part.PartName);
                if (sourceIndex < 0 || runtimeIndex < 0)
                    continue;

                string? desired = source.bindings[sourceIndex].overridePath;
                string? actual = runtime.bindings[runtimeIndex].overridePath;
                if (string.Equals(desired, actual, StringComparison.Ordinal))
                    continue;

                if (desired == null)
                    InputActionRebindingExtensions.RemoveBindingOverride(runtime,
                        runtimeIndex);
                else
                    InputActionRebindingExtensions.ApplyBindingOverride(runtime,
                        runtimeIndex, desired);
                changed = true;
            }

            if (changed)
            {
                RefreshLeaderboardRowPrompts(row);
                WriteStatus($"Synchronized {row.Part.Label} after a binding change or reset.");
            }
        }
    }

    private void RefreshLeaderboardRowPrompts(AddedLeaderboardControlRow row)
    {
        try
        {
            row.ActiveDisplayPrompt?.Refresh();
            row.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus($"Could not refresh {row.Part.Label} prompt: {ex.Message}");
        }
    }

    private static bool IsGamepadDevice(string device) =>
        device.Contains("Gamepad", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("Controller", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("Xbox", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("XInput", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("PlayStation", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("Dual", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("Switch", StringComparison.OrdinalIgnoreCase) ||
        device.Contains("NPad", StringComparison.OrdinalIgnoreCase);
}
