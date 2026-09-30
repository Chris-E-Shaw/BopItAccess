using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string ScoreKeyboardKey = "BopItAccess.ReadScore.Keyboard";
    private const string ScoreGamepadKey = "BopItAccess.ReadScore.Gamepad";
    private InputActionAsset? _scoreActionAsset;
    private InputAction? _scoreAction;
    private AddedScoreControlRow? _scoreControlRow;
    private InputActionRebindingExtensions.RebindingOperation? _scoreRebindOperation;
    private InputRebindingManager? _scoreRebindManager;
    private InputActionMap? _scoreRebindUiMap;
    private bool _scoreRebindUiMapWasEnabled;
    private bool _scoreRebindActionWasEnabled;
    private int _scoreRebindIndex;
    private string? _scoreRebindOriginalPath;
    private long _nextScoreControlErrorAt;

    private InputAction EnsureScoreAction()
    {
        if (_scoreAction != null)
            return _scoreAction;

        // This asset belongs to the mod. It can be bound to a native prompt,
        // while its override keys remain independent of the game's save file.
        _scoreActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        InputActionMap map = _scoreActionAsset.AddActionMap("BopItAccess");
        InputAction action = map.AddAction("ReadScore", InputActionType.Button);
        action.AddBinding("<Keyboard>/t");
        action.AddBinding("<Gamepad>/leftStickPress");
        string keyboard = PlayerPrefs.GetString(ScoreKeyboardKey, string.Empty);
        string gamepad = PlayerPrefs.GetString(ScoreGamepadKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(keyboard))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 0, keyboard);
        if (!string.IsNullOrWhiteSpace(gamepad))
            InputActionRebindingExtensions.ApplyBindingOverride(action, 1, gamepad);
        _scoreAction = action;
        WriteStatus("Read Score input initialized with keyboard and controller bindings.");
        return action;
    }

    private bool WasReadScorePressed(bool gameOverScreenActive)
    {
        try
        {
            if (!gameOverScreenActive)
            {
                // This runs frequently outside the visible game-over screen, including
                // before the action has been created.
                if (_scoreAction?.enabled == true)
                    _scoreAction.Disable();
                return false;
            }

            InputAction action = EnsureScoreAction();
            if (!action.enabled)
                action.Enable();
            return action.WasPressedThisFrame();
        }
        catch (Exception ex)
        {
            try
            {
                if (_scoreAction?.enabled == true)
                    _scoreAction.Disable();
            }
            catch (Exception cleanup)
            {
                WriteStatus("Read Score input cleanup failed: " + cleanup.Message);
            }
            if (Environment.TickCount64 >= _nextScoreControlErrorAt)
            {
                WriteStatus("Read Score input failed: " + ex);
                _nextScoreControlErrorAt = Environment.TickCount64 + 5000;
            }
            return false;
        }
    }

    private string ReadScoreBindingInstruction()
    {
        try
        {
            InputAction action = EnsureScoreAction();
            string keyboard = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 0)) ?? "T";
            string gamepad = CleanSpeechValue(
                InputActionRebindingExtensions.GetBindingDisplayString(action, 1)) ?? "left stick press";
            return FormatHintPress(keyboard, gamepad,
                "read the final result again");
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the Read Score bindings: " + ex.Message);
            string keyboard = HintSavedBinding(ScoreKeyboardKey, "T");
            string gamepad = HintSavedBinding(ScoreGamepadKey,
                "left stick press");
            return FormatHintPress(keyboard, gamepad,
                "read the final result again");
        }
    }

    private void AddScoreControlRow(ControlRow template, Transform parent, int insertAt)
    {
        InputAction action = EnsureScoreAction();
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        _leaderboardAddedRows.Add(clone);
        clone.name = "READ_SCORE";
        clone.transform.SetSiblingIndex(insertAt);

        ControlRow? row = clone.GetComponent<ControlRow>();
        TMP_Text? defaultLabel = FindControlLabel(clone.transform, "Default/Label");
        TMP_Text? activeLabel = FindControlLabel(clone.transform, "Active/Label");
        Selectable? selectable = clone.GetComponentInChildren<Selectable>(true);
        if (row == null || defaultLabel == null || activeLabel == null || selectable == null)
            throw new InvalidOperationException("The Read Score row lost a native control.");

        row.ActionMapName = "BopItAccess";
        row.ActionName = "ReadScore";
        SetClonedLabel(defaultLabel, "READ SCORE");
        SetClonedLabel(activeLabel, "READ SCORE");
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
            throw new InvalidOperationException("The Read Score row lost its focus containers.");
        _scoreControlRow = new(clone, active, inactive,
            row.ActiveDisplayPrompt, row.DefaultDisplayPrompt);
        // Match the leaderboard rows: the native listener would try to find
        // this action in the game's asset, so only the visual row is kept.
        row.enabled = false;
        UnityEngine.Object.Destroy(row);
    }

    private AddedScoreControlRow? FindAddedScoreControlRow(GameObject? selected)
    {
        AddedScoreControlRow? row = _scoreControlRow;
        return selected != null && row?.Root != null &&
            (selected.GetInstanceID() == row.Root.GetInstanceID() ||
             selected.transform.IsChildOf(row.Root.transform)) ? row : null;
    }

    private string? ReadScoreControlBinding(InputRebindingManager? manager)
    {
        if (manager == null)
            return null;
        InputAction action = EnsureScoreAction();
        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        int index = IsGamepadDevice(device) ? 1 : 0;
        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(action, index));
    }

    private string? ReadScoreControlSnapshot()
    {
        InputAction action = EnsureScoreAction();
        string? keyboard = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 0));
        string? gamepad = CleanSpeechValue(
            InputActionRebindingExtensions.GetBindingDisplayString(action, 1));
        return keyboard == null || gamepad == null ? null : keyboard + "\u001e" + gamepad;
    }

    private void ReadScoreControlRow(AddedScoreControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ReadScoreControlBinding(manager);
        bool rebinding = _scoreRebindOperation != null;

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType("Read Score", "button");
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

    private void UpdateScoreControlRebinding()
    {
        try
        {
            UpdateScoreControlRebindingCore();
        }
        catch (Exception ex)
        {
            bool wasRebinding = _scoreRebindOperation != null;
            if (Environment.TickCount64 >= _nextScoreControlErrorAt)
            {
                WriteStatus("Read Score rebinding failed: " + ex);
                _nextScoreControlErrorAt = Environment.TickCount64 + 5000;
            }
            try
            {
                if (wasRebinding)
                    RestoreScoreOriginalOverride();
                CancelScoreControlRebinding(false);
            }
            catch (Exception cleanup)
            {
                WriteStatus("Read Score rebind cleanup failed: " + cleanup.Message);
            }
            if (!wasRebinding)
                _scoreControlRow = null;
            if (wasRebinding)
                QueueSpeech("Rebinding failed");
        }
    }

    private void UpdateScoreControlRebindingCore()
    {
        AddedScoreControlRow? row = _scoreControlRow;
        // Destroyed Unity objects still have a managed wrapper; null
        // propagation does not detect them. Check before calling Unity.
        if (row == null || row.Root == null)
        {
            _scoreControlRow = null;
            if (_scoreRebindOperation != null)
            {
                RestoreScoreOriginalOverride();
                CancelScoreControlRebinding(false);
            }
            return;
        }

        MainMenuUIManager? main = _mainMenu;
        Panel? panel = main == null ? null : main.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy ||
            !row.Root.activeInHierarchy)
        {
            if (_scoreRebindOperation != null)
            {
                RestoreScoreOriginalOverride();
                CancelScoreControlRebinding(false);
            }
            return;
        }

        GameObject? selected = EventSystem.current?.currentSelectedGameObject;
        bool focused = ReferenceEquals(FindAddedScoreControlRow(selected), row);
        if (row.ActiveContainer != null && row.ActiveContainer.activeSelf != focused)
            row.ActiveContainer.SetActive(focused);
        if (row.DefaultContainer != null && row.DefaultContainer.activeSelf == focused)
            row.DefaultContainer.SetActive(!focused);

        if (_scoreRebindOperation != null)
        {
            if (_scoreRebindOperation.completed)
                CompleteScoreControlRebinding();
            else if (_scoreRebindOperation.canceled)
                CancelScoreControlRebinding();
            return;
        }

        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>();
        if (manager == null || manager.IsRebinding || !WasControlsSubmitPressed(manager))
            return;

        if (focused)
        {
            StartScoreControlRebinding(manager);
            return;
        }

        // The native Reset row handles the game's own bindings. Include the
        // two mod-owned overrides in the same user action.
        ResetToDefaultRow? reset = _controlsResetRow ??
            row.Root.GetComponentInParent<Panel>()?.GetComponentInChildren<ResetToDefaultRow>(true);
        if (reset != null && selected != null &&
            (selected.GetInstanceID() == reset.gameObject.GetInstanceID() ||
             selected.transform.IsChildOf(reset.transform)))
            ResetScoreControlBindings();
    }

    private void StartScoreControlRebinding(InputRebindingManager manager)
    {
        InputAction action = EnsureScoreAction();
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
            _scoreRebindOperation = operation;
            _scoreRebindManager = manager;
            _scoreRebindUiMap = uiMap;
            _scoreRebindUiMapWasEnabled = uiWasEnabled;
            _scoreRebindActionWasEnabled = actionWasEnabled;
            _scoreRebindIndex = index;
            _scoreRebindOriginalPath = original;
            WriteStatus($"Rebinding Read Score on {device}, binding {index}.");
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

    private void CompleteScoreControlRebinding()
    {
        InputAction? action = _scoreAction;
        InputRebindingManager? manager = _scoreRebindManager;
        int index = _scoreRebindIndex;
        string? path = action == null || index < 0 || index >= action.bindings.Count
            ? null : action.bindings[index].overridePath;
        string? resolved = string.IsNullOrEmpty(path) ? null : InputSystem.FindControl(path)?.path;
        if (action == null || manager == null || string.IsNullOrEmpty(resolved) ||
            IsEssentialNativeScoreBinding(manager, resolved))
        {
            RestoreScoreOriginalOverride();
            ReleaseScoreControlRebinding();
            _lastControlsRebinding = false;
            QueueSpeech(string.IsNullOrEmpty(resolved) ? "Binding unavailable" :
                "That input is used to start or leave the screen");
            return;
        }

        // The operation has already applied its override to the targeted
        // keyboard or controller binding. Persist only that binding.
        PlayerPrefs.SetString(index == 1 ? ScoreGamepadKey : ScoreKeyboardKey, path!);
        PlayerPrefs.Save();
        ReleaseScoreControlRebinding();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        RefreshScoreControlPrompts();
        string spoken = InputActionRebindingExtensions.GetBindingDisplayString(action, index);
        _lastControlsBinding = CleanSpeechValue(spoken);
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        WriteStatus($"Rebound Read Score to {path}.");
        QueueSpeech(_lastControlsBinding ?? "Binding changed");
    }

    private static bool IsEssentialNativeScoreBinding(InputRebindingManager manager,
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
                if (ScorePathsMatch(action.bindings[i].effectivePath, path))
                    return true;
            }
        }
        return false;
    }

    private void ResetScoreControlBindings()
    {
        InputAction action = EnsureScoreAction();
        InputActionRebindingExtensions.RemoveBindingOverride(action, 0);
        InputActionRebindingExtensions.RemoveBindingOverride(action, 1);
        PlayerPrefs.DeleteKey(ScoreKeyboardKey);
        PlayerPrefs.DeleteKey(ScoreGamepadKey);
        PlayerPrefs.Save();
        RefreshScoreControlPrompts();
        WriteStatus("Reset Read Score bindings to T and left stick press.");
    }

    private void CancelScoreControlRebinding(bool announce = true)
    {
        bool wasActive = _scoreRebindOperation != null;
        ReleaseScoreControlRebinding();
        if (wasActive)
            _lastControlsRebinding = false;
        if (wasActive && announce)
            QueueSpeech("Binding unchanged");
    }

    private void ReleaseScoreControlRebinding()
    {
        InputActionRebindingExtensions.RebindingOperation? operation = _scoreRebindOperation;
        InputActionMap? uiMap = _scoreRebindUiMap;
        bool uiWasEnabled = _scoreRebindUiMapWasEnabled;
        bool actionWasEnabled = _scoreRebindActionWasEnabled;
        _scoreRebindOperation = null;
        _scoreRebindManager = null;
        _scoreRebindUiMap = null;
        _scoreRebindUiMapWasEnabled = false;
        _scoreRebindActionWasEnabled = false;
        _scoreRebindIndex = 0;
        _scoreRebindOriginalPath = null;
        if (operation != null)
        {
            if (operation.started && !operation.completed && !operation.canceled)
                operation.Cancel();
            operation.Dispose();
        }
        if (uiMap != null && uiWasEnabled)
            uiMap.Enable();
        if (_scoreAction != null && actionWasEnabled)
            _scoreAction.Enable();
    }

    private void RestoreScoreOriginalOverride()
    {
        InputAction? action = _scoreAction;
        int index = _scoreRebindIndex;
        if (action == null || index < 0 || index >= action.bindings.Count)
            return;
        if (_scoreRebindOriginalPath == null)
            InputActionRebindingExtensions.RemoveBindingOverride(action, index);
        else
            InputActionRebindingExtensions.ApplyBindingOverride(action, index,
                _scoreRebindOriginalPath);
    }

    private void RefreshScoreControlPrompts()
    {
        try
        {
            _scoreControlRow?.ActiveDisplayPrompt?.Refresh();
            _scoreControlRow?.DefaultDisplayPrompt?.Refresh();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not refresh Read Score prompt: " + ex.Message);
        }
    }

    private static bool ScorePathsMatch(string? a, string? b)
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

    private sealed record AddedScoreControlRow(
        GameObject Root, GameObject ActiveContainer, GameObject DefaultContainer,
        ControlPromptSpriteSwapperV2? ActiveDisplayPrompt,
        ControlPromptSpriteSwapperV2? DefaultDisplayPrompt);

}
