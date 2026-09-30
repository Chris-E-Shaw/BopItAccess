using System;
using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private InputRebindingManager? _controlsRebindingManager;
    private ControlRow[]? _controlsRows;
    private ResetToDefaultRow? _controlsResetRow;
    private int _lastFocusedControlsRowId;
    private int _lastObservedControlsSelectionId;
    private string? _lastControlsBinding;
    private string? _lastControlsFeedback;
    private string? _lastControlsResetSnapshot;
    private string? _lastControlsResetDevice;
    private bool _lastControlsRebinding;
    private bool _controlsBindingChangedDuringRebind;
    private bool _controlsWasVisible;
    private bool _controlsIntroductionPending;
    private long _controlsOpenedAt;
    private long _nextControlsPanelSearchAt;
    private bool _controlsSubmitPanelVisible;
    private bool _controlsSubmitReady;
    private int _controlsSubmitPanelId;
    private int _controlsSubmitOpenedFrame;

    private void UpdateControlsSubmitGate()
    {
        MainMenuUIManager? main = _mainMenu;
        if (main == null)
            main = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        Panel? panel = main?.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            _controlsSubmitPanelVisible = false;
            _controlsSubmitReady = false;
            return;
        }

        int panelId = panel.GetInstanceID();
        if (!_controlsSubmitPanelVisible || _controlsSubmitPanelId != panelId)
        {
            _controlsSubmitPanelVisible = true;
            _controlsSubmitPanelId = panelId;
            _controlsSubmitOpenedFrame = Time.frameCount;
            _controlsSubmitReady = false;
            return;
        }

        if (!_controlsSubmitReady && Time.frameCount > _controlsSubmitOpenedFrame &&
            !IsUiSubmitHeld())
            _controlsSubmitReady = true;
    }

    private bool ReadControlsFocus()
    {
        if (_mainMenu == null && Environment.TickCount64 >= _nextControlsPanelSearchAt)
        {
            _nextControlsPanelSearchAt = Environment.TickCount64 + 500;
            _mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
        }

        Panel? panel = _mainMenu == null ? null : _mainMenu.controlsPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetControlsFocus();
            return false;
        }

        if (!_controlsWasVisible)
        {
            _controlsRebindingManager = panel.GetComponentInChildren<InputRebindingManager>(true);
            var foundRows = panel.GetComponentsInChildren<ControlRow>(true);
            var nativeRows = new List<ControlRow>(foundRows.Length);
            foreach (ControlRow row in foundRows)
            {
                // Added leaderboard visuals may still have their disabled
                // ControlRow component until Unity destroys it this frame.
                if (row != null && row.enabled)
                    nativeRows.Add(row);
            }
            _controlsRows = nativeRows.ToArray();
            _controlsResetRow = panel.GetComponentInChildren<ResetToDefaultRow>(true);
            _controlsIntroductionPending = true;
            _controlsOpenedAt = Environment.TickCount64;
            _controlsWasVisible = true;
            WriteStatus($"Controls panel is visible; found {_controlsRows.Length} binding rows and " +
                (_controlsResetRow == null ? "no reset row." : "the reset row."));
        }

        EventSystem? eventSystem = EventSystem.current;
        GameObject? selected = eventSystem == null ? null : eventSystem.currentSelectedGameObject;
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        if (selectedId != _lastObservedControlsSelectionId)
        {
            WriteStatus($"Controls selected object: {(selected == null ? "none" : selected.name)}.");
            _lastObservedControlsSelectionId = selectedId;
        }

        AddedLeaderboardControlRow? addedRow = FindAddedLeaderboardControlRow(selected);
        if (addedRow != null && addedRow.Root.activeInHierarchy)
        {
            ReadAddedLeaderboardControlRow(addedRow);
            return true;
        }

        AddedDescriptionControlRow? descriptionRow = FindAddedDescriptionControlRow(selected);
        if (descriptionRow != null && descriptionRow.Root.activeInHierarchy)
        {
            ReadDescriptionControlRow(descriptionRow);
            return true;
        }

        AddedScoreControlRow? scoreRow = FindAddedScoreControlRow(selected);
        if (scoreRow != null && scoreRow.Root.activeInHierarchy)
        {
            ReadScoreControlRow(scoreRow);
            return true;
        }

        AddedToggleSpeechControlRow? toggleSpeechRow =
            FindAddedToggleSpeechControlRow(selected);
        if (toggleSpeechRow != null && toggleSpeechRow.Root.activeInHierarchy)
        {
            ReadToggleSpeechControlRow(toggleSpeechRow);
            return true;
        }

        ControlRow? focusedRow = FindFocusedControlRow(panel, selected);
        if (focusedRow != null)
        {
            ReadControlRow(focusedRow);
            return true;
        }

        ResetToDefaultRow? resetRow = FindFocusedResetRow(panel, selected);
        if (resetRow != null)
        {
            ReadResetRow(resetRow);
            return true;
        }

        _lastFocusedControlsRowId = 0;
        _lastControlsBinding = null;
        _lastControlsFeedback = null;
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        _lastControlsResetSnapshot = null;

        // The panel has no selectable Back row. Its Back control is an on-screen
        // prompt, so include that instruction once even if selection is delayed.
        if (_controlsIntroductionPending && Environment.TickCount64 - _controlsOpenedAt >= 500)
        {
            _controlsIntroductionPending = false;
            QueueSpeech("Controls. Use Back to return to Settings.");
        }

        return true;
    }

    private ControlRow? FindFocusedControlRow(Panel panel, GameObject? selected)
    {
        if (_controlsRows == null)
            return null;

        if (selected != null)
        {
            if (!selected.transform.IsChildOf(panel.transform))
                return null;

            ControlRow? selectedRow = selected.GetComponentInParent<ControlRow>();
            if (selectedRow != null && selectedRow.gameObject.activeInHierarchy)
                return selectedRow;

            if (selected.GetComponentInParent<ResetToDefaultRow>() != null)
                return null;
        }

        // The game also sets a focus flag on each row. This covers selection
        // moving to a child prompt while a binding is being edited.
        foreach (ControlRow row in _controlsRows)
        {
            if (row != null && row.gameObject.activeInHierarchy && row.IsFocused)
                return row;
        }

        return null;
    }

    private ResetToDefaultRow? FindFocusedResetRow(Panel panel, GameObject? selected)
    {
        ResetToDefaultRow? row = _controlsResetRow;
        if (row == null || !row.gameObject.activeInHierarchy)
            return null;

        if (selected != null)
        {
            if (!selected.transform.IsChildOf(panel.transform))
                return null;

            ResetToDefaultRow? selectedRow = selected.GetComponentInParent<ResetToDefaultRow>();
            if (selectedRow != null)
                return selectedRow;
        }

        return row.IsFocused ? row : null;
    }

    private void ReadControlRow(ControlRow row)
    {
        int id = row.GetInstanceID();
        InputRebindingManager? manager = row.InputRebindingManager ?? _controlsRebindingManager;
        string? binding = ReadControlBinding(row, manager);
        bool rebinding = manager != null && manager.IsRebinding;
        string? feedback = ReadVisibleControlFeedback(row);

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = feedback;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;

            string label = WithControlType(GetControlRowLabel(row), "button");
            string message = binding == null ? label : $"{label}, {binding}";
            if (rebinding)
                message += $". {feedback ?? "Listening for input"}";
            QueueSpeech(WithControlsIntroduction(message));
            return;
        }

        if (binding != null &&
            !string.Equals(binding, _lastControlsBinding, StringComparison.Ordinal))
        {
            if (rebinding || _lastControlsRebinding)
                _controlsBindingChangedDuringRebind = true;
            _lastControlsBinding = binding;
            _lastControlsFeedback = feedback;
            _lastControlsRebinding = rebinding;
            QueueSpeech(binding);
            return;
        }

        if (rebinding && !_lastControlsRebinding)
        {
            _controlsBindingChangedDuringRebind = false;
            QueueSpeech(feedback ?? "Listening for input");
        }
        else if (!rebinding && _lastControlsRebinding)
        {
            if (feedback != null &&
                !string.Equals(feedback, _lastControlsFeedback, StringComparison.Ordinal))
                QueueSpeech(feedback);
            else if (!_controlsBindingChangedDuringRebind)
                QueueSpeech("Binding unchanged");

            _controlsBindingChangedDuringRebind = false;
        }
        else if (feedback != null &&
                 !string.Equals(feedback, _lastControlsFeedback, StringComparison.Ordinal))
            QueueSpeech(feedback);

        _lastControlsFeedback = feedback;
        _lastControlsRebinding = rebinding;
    }

    private void ReadAddedLeaderboardControlRow(AddedLeaderboardControlRow row)
    {
        int id = row.Root.GetInstanceID();
        InputRebindingManager? manager = row.Root.GetComponentInParent<InputRebindingManager>()
            ?? _controlsRebindingManager;
        string? binding = ReadAddedLeaderboardBinding(row, manager);
        bool rebinding = _leaderboardRebindOperation != null &&
            ReferenceEquals(_leaderboardRebindRow, row);

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = binding;
            _lastControlsFeedback = null;
            _lastControlsRebinding = rebinding;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = null;
            string label = WithControlType(row.Part.Label, "button");
            string message = binding == null ? label : $"{label}, {binding}";
            if (rebinding)
                message += ". Listening for input";
            QueueSpeech(WithControlsIntroduction(message));
            return;
        }

        if (binding != null &&
            !string.Equals(binding, _lastControlsBinding, StringComparison.Ordinal))
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

    private string? ReadAddedLeaderboardBinding(AddedLeaderboardControlRow row,
        InputRebindingManager? manager)
    {
        if (manager == null)
            return null;

        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice
            ?? string.Empty;
        InputAction? action = FindLeaderboardAction(manager.playerInput?.actions,
            row.Part) ?? FindLeaderboardAction(manager.inputActions, row.Part);
        if (action == null)
            return null;

        int index = FindCompositePartIndex(action, device, -1, row.Part.PartName);
        if (index < 0)
            return null;

        return CleanSpeechValue(InputActionRebindingExtensions.GetBindingDisplayString(
            action, index));
    }

    private void ReadResetRow(ResetToDefaultRow row)
    {
        int id = row.GetInstanceID();
        InputRebindingManager? manager = row.InputRebindingManager ?? _controlsRebindingManager;
        string device = manager?.ActiveDevice ?? manager?.deviceTracker?.ActiveDevice ?? string.Empty;
        string? snapshot = GetControlsBindingSnapshot();

        if (id != _lastFocusedControlsRowId)
        {
            _lastFocusedControlsRowId = id;
            _lastControlsBinding = null;
            _lastControlsFeedback = null;
            _lastControlsRebinding = false;
            _controlsBindingChangedDuringRebind = false;
            _lastControlsResetSnapshot = snapshot;
            _lastControlsResetDevice = device;
            QueueSpeech(WithControlsIntroduction(
                WithControlType("Reset to Default", "button")));
            return;
        }

        if (!string.Equals(device, _lastControlsResetDevice, StringComparison.Ordinal))
        {
            // A keyboard/controller switch changes displayed bindings without
            // resetting them. Treat it as a new baseline.
            _lastControlsResetDevice = device;
            _lastControlsResetSnapshot = snapshot;
            return;
        }

        if (_lastControlsResetSnapshot == null)
        {
            // Wait until every binding can be read before comparing. Otherwise
            // late initialization could look like a reset.
            _lastControlsResetSnapshot = snapshot;
            return;
        }

        if (snapshot != null &&
            !string.Equals(snapshot, _lastControlsResetSnapshot, StringComparison.Ordinal))
        {
            _lastControlsResetSnapshot = snapshot;
            QueueSpeech("Bindings reset to default");
        }
    }

    private string? GetControlsBindingSnapshot()
    {
        if (_controlsRows == null || _controlsRows.Length == 0)
            return null;

        string[] values = new string[_controlsRows.Length + _leaderboardControlRows.Count +
            (_descriptionControlRow == null ? 0 : 1) +
            (_scoreControlRow == null ? 0 : 1) +
            (_toggleSpeechControlRow == null ? 0 : 1)];
        for (int i = 0; i < _controlsRows.Length; i++)
        {
            ControlRow row = _controlsRows[i];
            if (row == null)
                return null;

            string? value = ReadControlBinding(row, row.InputRebindingManager ?? _controlsRebindingManager);
            if (value == null)
                return null;

            values[i] = value;
        }

        int next = _controlsRows.Length;
        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
        {
            string? value = ReadAddedLeaderboardBinding(row, _controlsRebindingManager);
            if (value == null)
                return null;
            values[next++] = value;
        }

        if (_descriptionControlRow != null)
        {
            string? value = ReadDescriptionControlSnapshot();
            if (value == null)
                return null;
            values[next++] = value;
        }

        if (_scoreControlRow != null)
        {
            string? value = ReadScoreControlSnapshot();
            if (value == null)
                return null;
            values[next++] = value;
        }

        if (_toggleSpeechControlRow != null)
        {
            string? value = ReadToggleSpeechControlSnapshot();
            if (value == null)
                return null;
            values[next] = value;
        }

        return string.Join("\u001f", values);
    }

    private string? ReadControlBinding(ControlRow row, InputRebindingManager? manager)
    {
        if (manager == null)
            return null;

        string device = manager.ActiveDevice ?? manager.deviceTracker?.ActiveDevice ?? string.Empty;
        // PlayerInput can have a private runtime copy of the action asset with
        // the user's current overrides. Read that copy before the source asset.
        InputAction? action = manager.playerInput?.actions?
            .FindActionMap(row.ActionMapName, false)?
            .FindAction(row.ActionName, false);
        if (action == null)
            action = manager.inputActions?
                .FindActionMap(row.ActionMapName, false)?
                .FindAction(row.ActionName, false);
        if (action == null)
            action = row.ActiveDisplayPrompt?.ActionReference?.action;

        if (action != null)
        {
            int index = manager.FindAppropriateBindingIndex(action, device);
            if (index >= 0)
            {
                string? displayed = CleanSpeechValue(
                    InputActionRebindingExtensions.GetBindingDisplayString(action, index));
                if (displayed != null)
                    return displayed;
            }
        }

        string? path = row.ActiveDisplayPrompt?.GetControlPath(device);
        return path == null ? null : CleanSpeechValue(InputControlPath.ToHumanReadableString(path));
    }

    private static string? ReadVisibleControlFeedback(ControlRow row)
    {
        var feedback = row.Feedback;
        return feedback == null || !feedback.gameObject.activeInHierarchy
            ? null
            : CleanSpeechValue(feedback.text);
    }

    private string GetControlRowLabel(ControlRow row)
    {
        return row.ActionName switch
        {
            "Bop" => "Bop",
            "Twist" => "Twist",
            "Pull" => "Pull",
            "Spin" => "Spin",
            "Flick" => "Flick",
            "AltBop" => "Bop, Player 2",
            _ => string.IsNullOrWhiteSpace(row.ActionName) ? "Control" : row.ActionName
        };
    }

    private string WithControlsIntroduction(string message)
    {
        message = WithControlsRowIndex(message);
        if (!_controlsIntroductionPending)
            return message;

        _controlsIntroductionPending = false;
        return $"Controls. Use Back to return to Settings. {message}";
    }

    private string WithControlsRowIndex(string message)
    {
        if (!_indexingEnabled || _mainMenu?.controlsPanel == null)
            return message;

        var rootIds = new HashSet<int>();
        int focusedRootId = 0;
        void AddRow(GameObject? root, int focusId)
        {
            if (root == null || !root.activeInHierarchy)
                return;
            int rootId = root.GetInstanceID();
            rootIds.Add(rootId);
            if (focusId == _lastFocusedControlsRowId)
                focusedRootId = rootId;
        }

        if (_controlsRows != null)
        {
            foreach (ControlRow row in _controlsRows)
            {
                if (row != null && row.enabled)
                    AddRow(row.gameObject, row.GetInstanceID());
            }
        }
        if (_controlsResetRow != null)
            AddRow(_controlsResetRow.gameObject, _controlsResetRow.GetInstanceID());
        foreach (AddedLeaderboardControlRow row in _leaderboardControlRows.Values)
            AddRow(row.Root, row.Root.GetInstanceID());
        if (_descriptionControlRow != null)
            AddRow(_descriptionControlRow.Root,
                _descriptionControlRow.Root.GetInstanceID());
        if (_scoreControlRow != null)
            AddRow(_scoreControlRow.Root, _scoreControlRow.Root.GetInstanceID());
        if (_toggleSpeechControlRow != null)
            AddRow(_toggleSpeechControlRow.Root,
                _toggleSpeechControlRow.Root.GetInstanceID());

        int index = -1;
        int count = 0;
        foreach (Transform child in _mainMenu.controlsPanel.GetComponentsInChildren<Transform>(true))
        {
            int id = child.gameObject.GetInstanceID();
            if (!rootIds.Contains(id))
                continue;
            if (id == focusedRootId)
                index = count;
            count++;
        }
        return WithMenuIndex(message, index, count);
    }

    private void ResetControlsFocus()
    {
        _controlsRebindingManager = null;
        _controlsRows = null;
        _controlsResetRow = null;
        _lastFocusedControlsRowId = 0;
        _lastObservedControlsSelectionId = 0;
        _lastControlsBinding = null;
        _lastControlsFeedback = null;
        _lastControlsResetSnapshot = null;
        _lastControlsResetDevice = null;
        _lastControlsRebinding = false;
        _controlsBindingChangedDuringRebind = false;
        _controlsWasVisible = false;
        _controlsIntroductionPending = false;
        _controlsOpenedAt = 0;
    }
}
