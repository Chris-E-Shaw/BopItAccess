using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string ToggleSpeechKeyboardKey = "BopItAccess.ToggleSpeech.Keyboard";
    private const string ToggleSpeechGamepadKey = "BopItAccess.ToggleSpeech.Gamepad";
    private InputActionAsset? _toggleSpeechActionAsset;
    private InputAction? _toggleSpeechAction;
    private AddedToggleSpeechControlRow? _toggleSpeechControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _toggleSpeechRebindOperation;
    private InputRebindingManager? _toggleSpeechRebindManager;
    private ControlRebindInputState? _toggleSpeechRebindInputState;
    private int _toggleSpeechRebindIndex;
    private string? _toggleSpeechRebindOriginalPath;
    private long _nextToggleSpeechControlErrorAt;
    private int _toggleSpeechSuppressPressThroughFrame = -1;
    private bool _toggleSpeechAnyRebindWasActive;

    private InputAction EnsureToggleSpeechAction()
    {
        if (_toggleSpeechAction != null)
            return _toggleSpeechAction;

        // This asset belongs to the mod. It can be bound to a native prompt,
        // while its override keys remain independent of the game's save file.
        _toggleSpeechActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = _toggleSpeechActionAsset.AddActionMap("BopItAccess");
        InputAction action = map.AddAction("ToggleSpeech", InputActionType.Button);
        action.AddBinding("<Keyboard>/f8");
        action.AddBinding("<Gamepad>/select");
        string keyboard = PlayerPrefs.GetString(ToggleSpeechKeyboardKey, string.Empty);
        string gamepad = PlayerPrefs.GetString(ToggleSpeechGamepadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(keyboard))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 0, keyboard);
        if (!string.IsNullOrWhiteSpace(gamepad))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 1, gamepad);
        _toggleSpeechAction = action;
        WriteStatus("Toggle Speech input initialized with keyboard and controller bindings.");
        return action;
    }

    private bool WasToggleSpeechPressed()
    {
        try
        {
            // The recovery action must remain available on every screen, even
            // while the speech gate is closed. It is paused only for rebinding.
            bool rebinding = _toggleSpeechRebindOperation != null ||
                _descriptionRebindOperation != null ||
                _scoreRebindOperation != null ||
                _speakHintsRebindOperation != null ||
                _leaderboardRebindOperation != null ||
                _resetGyroRebindOperation != null ||
                _changeSpeechOutputRebindOperation != null ||
                _controlsRebindingManager?.IsRebinding == true;
            if (rebinding)
            {
                _toggleSpeechAnyRebindWasActive = true;
                return false;
            }

            if (_toggleSpeechAnyRebindWasActive)
            {
                // A capture can complete earlier in this same update. Do not
                // interpret its final key/button as a fresh toggle press.
                _toggleSpeechAnyRebindWasActive = false;
                _toggleSpeechSuppressPressThroughFrame = Time.frameCount + 1;
                return false;
            }

            InputAction action = EnsureToggleSpeechAction();
            if (!action.enabled)
                action.Enable();
            if (Time.frameCount <= _toggleSpeechSuppressPressThroughFrame)
                return false;
            return action.WasPressedThisFrame();
        }
        catch (Exception ex)
        {
            if (Environment.TickCount64 >= _nextToggleSpeechControlErrorAt)
            {
                WriteStatus("Toggle Speech input failed: " + ex);
                _nextToggleSpeechControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private string ReadToggleSpeechRecoveryInstruction()
    {
        string keyboard = ReadToggleSpeechBindingLabel(0, ToggleSpeechKeyboardKey,
            "<Keyboard>/f8", "F8");
        string gamepad = ReadToggleSpeechBindingLabel(1, ToggleSpeechGamepadKey,
            "<Gamepad>/select", "Select");
        return LF("{0} on keyboard or {1} on controller, turn speech back on.",
            LocalizeBindingDisplay(keyboard), LocalizeBindingDisplay(gamepad));
    }

    private string ReadToggleSpeechFallbackRecoveryInstruction()
    {
        // Prism can initialize before the main thread has created our action.
        // Read the persisted paths directly so the OFF announcement still
        // identifies both current recovery controls.
        string keyboard = ReadToggleSpeechSavedBindingLabel(
            ToggleSpeechKeyboardKey, "F8");
        string gamepad = ReadToggleSpeechSavedBindingLabel(
            ToggleSpeechGamepadKey, "Select");
        return LF("{0} on keyboard or {1} on controller, turn speech back on.",
            LocalizeBindingDisplay(keyboard), LocalizeBindingDisplay(gamepad));
    }

    private string ReadToggleSpeechBindingLabel(int index, string preference,
        string defaultPath, string defaultLabel)
    {
        try
        {
            InputAction action = EnsureToggleSpeechAction();
            string? displayed = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, index));
            if (displayed != null)
                return displayed;

            string effectivePath = action.bindings[index].effectivePath ?? defaultPath;
            displayed = CleanSpeechValue(
                InputControlPath.ToHumanReadableString(effectivePath));
            if (displayed != null)
                return displayed;
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Toggle Speech binding display: " + ex.Message);
        }

        return ReadToggleSpeechSavedBindingLabel(preference, defaultLabel);
    }

    private string ReadToggleSpeechSavedBindingLabel(string preference,
        string defaultLabel)
    {
        // Startup can ask for this before an InputSystem device has been
        // detected. Preserve a saved override in the recovery instruction.
        string path;
        try
        {
            path = PlayerPrefs.GetString(preference, string.Empty);
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the saved Toggle Speech binding: " + ex.Message);
            return defaultLabel;
        }

        if (string.IsNullOrWhiteSpace(path))
            return defaultLabel;

        try
        {
            string? saved = CleanSpeechValue(InputControlPath.ToHumanReadableString(path));
            if (saved != null)
                return saved;
        }
        catch (Exception ex)
        {
            WriteStatus("Could not format the saved Toggle Speech binding: " + ex.Message);
        }

        int slash = path.LastIndexOf('/');
        return slash >= 0 && slash < path.Length - 1
            ? path[(slash + 1)..] : path;
    }

    private void AddToggleSpeechControlRow(ControlRow template, Transform parent, int insertAt)
    {
        InputAction action = EnsureToggleSpeechAction();
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "TOGGLE_SPEECH";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
            throw new InvalidOperationException("The Toggle Speech row lost a native control.");

        row.ActionMapName = "BopItAccess";
        row.ActionName = "ToggleSpeech";
        SetClonedLabel(defaultLabel, "TOGGLE SPEECH");
        SetClonedLabel(activeLabel, "TOGGLE SPEECH");
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
            throw new InvalidOperationException("The Toggle Speech row lost its focus containers.");
        _toggleSpeechControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Match the leaderboard rows: the native listener would try to find
        // this action in the game's asset, so only the visual row is kept.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedToggleSpeechControlRow? FindAddedToggleSpeechControlRow(GameObject? selected)
    {
        AddedToggleSpeechControlRow? row = _toggleSpeechControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private string? ReadToggleSpeechControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction action = EnsureToggleSpeechAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = IsGamepadDevice(device) ? 1 : 0;
        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? ReadToggleSpeechControlSnapshot()
    {
        InputAction action = EnsureToggleSpeechAction();
        string? keyboard = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        string? gamepad = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 1));
        return keyboard == null || gamepad == null ? null : keyboard + "\u001e" + gamepad;
    }

    private void ReadToggleSpeechControlRow(AddedToggleSpeechControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ReadToggleSpeechControlBinding(manager);
        bool rebinding = _toggleSpeechRebindOperation != null;

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType(L("Toggle Speech"), "button");
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

    private void UpdateToggleSpeechControlRebinding()
    {
        try
        {
            UpdateToggleSpeechControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _toggleSpeechRebindOperation != null;
            if (Environment.TickCount64 >= _nextToggleSpeechControlErrorAt)
            {
                WriteStatus("Toggle Speech rebinding failed: " + ex);
                _nextToggleSpeechControlErrorAt = Environment.TickCount64 + 5000;
            }
            try
            {
                CancelToggleSpeechControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Toggle Speech rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _toggleSpeechControlRow = null;
            if (wasRebinding)
                QueueSpeech(L("Rebinding failed"));
        }
    }

    private void UpdateToggleSpeechControlRebindingCore()
    {
        AddedToggleSpeechControlRow? row = _toggleSpeechControlRow;
        // Destroyed Unity objects still have a managed wrapper; null
        // propagation does not detect them. Check before calling Unity.
        if (row == null || row.Root == null)
        {
            _toggleSpeechControlRow = null;
            if (_toggleSpeechRebindOperation != null)
            {
                CancelToggleSpeechControlRebinding(false);
            }
            return;
        }

        MainMenuUIManager? main = _mainMenu;
        Panel? panel = main == null ? null : main.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_toggleSpeechRebindOperation != null)
            {
                CancelToggleSpeechControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedToggleSpeechControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_toggleSpeechRebindOperation != null)
        {
            if (_toggleSpeechRebindOperation.completed)
                CompleteToggleSpeechControlRebinding();
            else if (_toggleSpeechRebindOperation.canceled)
                CancelToggleSpeechControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding || !WasControlsSubmitPressed(manager))
            return;

        if (focused)
        {
            StartToggleSpeechControlRebinding(manager);
            return;
        }

        // The native Reset row handles the game's own bindings. Include the
        // two mod-owned overrides in the same user action.
        ResetToDefaultRow? reset = _controlsResetRow ??
            row.Root.GetComponentInParent<Panel>()?.GetComponentInChildren<ResetToDefaultRow>(true);
        if (reset != null && selected != null &&
            (selected.GetInstanceID() == reset.gameObject.GetInstanceID() ||
             selected.transform.IsChildOf(reset.transform)))
            ResetToggleSpeechControlBindings();
    }

    private void StartToggleSpeechControlRebinding(InputRebindingManager manager)
    {
        InputAction action = EnsureToggleSpeechAction();
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
            _toggleSpeechRebindOperation = operation;
            _toggleSpeechRebindManager = manager;
            _toggleSpeechRebindInputState = inputState;
            _toggleSpeechRebindIndex = index;
            _toggleSpeechRebindOriginalPath = original;
            WriteStatus($"Rebinding Toggle Speech on {device}, binding {index}.");
        }
        catch
        {
            ReleaseControlRebindingCapture(operation, inputState);
            throw;
        }
    }

    private void CompleteToggleSpeechControlRebinding()
    {
        InputAction? action = _toggleSpeechAction;
        InputRebindingManager? manager = _toggleSpeechRebindManager;
        int index = _toggleSpeechRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved))
        {
            RestoreToggleSpeechOriginalOverride();
            ReleaseToggleSpeechControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(L("Binding unavailable"));
            return;
        }

        if (IsBindingAssignedElsewhere(manager, action, index, resolved,
                out string owner))
        {
            RestoreToggleSpeechOriginalOverride();
            ReleaseToggleSpeechControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(LF("That input is already assigned to {0}", L(owner)));
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        SaveReboundModControlPreference(index == 1 ? ToggleSpeechGamepadKey : ToggleSpeechKeyboardKey, path!);
        ReleaseToggleSpeechControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshToggleSpeechControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Toggle Speech to {path}.");
        QueueControlAssignmentSpeech(_lastControlsBinding, "Toggle Speech");
        if (!_speechEnabled)
            AnnounceSpeechToggleRecoveryNow();
    }

    private void ResetToggleSpeechControlBindings()
    {
        InputAction action = EnsureToggleSpeechAction();
        bool wasEnabled = action.enabled;
        if (wasEnabled)
            action.Disable();
        try
        {
            InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
            InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
            PlayerPrefs.DeleteKey(ToggleSpeechKeyboardKey);
            PlayerPrefs.DeleteKey(ToggleSpeechGamepadKey);
            SaveModPreferencesAndConfig();
        }
        finally
        {
            if (wasEnabled)
                action.Enable();
            // Reset is submitted with a control press. It must not double as
            // a Toggle Speech press when the action is re-enabled.
            _toggleSpeechSuppressPressThroughFrame = Time.frameCount + 1;
        }
        RefreshToggleSpeechControlPrompts();
        WriteStatus("Reset Toggle Speech bindings to F8 and Select.");
        if (!_speechEnabled)
            AnnounceSpeechToggleRecoveryNow();
    }

    private void CancelToggleSpeechControlRebinding(bool announce = true)
    {
        bool wasActive = _toggleSpeechRebindOperation != null;
        if (wasActive)
        {
            try { RestoreToggleSpeechOriginalOverride(); }
            catch (Exception ex) { WriteStatus("Could not restore ToggleSpeech binding: " + ex.Message); }
        }
        ReleaseToggleSpeechControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech(L("Binding unchanged"));
    }

    private void ReleaseToggleSpeechControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _toggleSpeechRebindOperation;
        ControlRebindInputState? inputState = _toggleSpeechRebindInputState;
        _toggleSpeechRebindOperation = null;
        _toggleSpeechRebindManager = null;
        _toggleSpeechRebindInputState = null;
        _toggleSpeechRebindIndex = 0;
        _toggleSpeechRebindOriginalPath = null;
        ReleaseControlRebindingCapture(operation, inputState);
    }

    private void RestoreToggleSpeechOriginalOverride()
    {
        InputAction? action = _toggleSpeechAction;
        int index = _toggleSpeechRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_toggleSpeechRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _toggleSpeechRebindOriginalPath);
    }

    private void RefreshToggleSpeechControlPrompts()
    {
        try
        {
            _toggleSpeechControlRow?.ActiveDisplayPrompt?.Refresh();
            _toggleSpeechControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Toggle Speech prompt: " + ex.Message);
        }
    }

    private sealed record AddedToggleSpeechControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
