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
            _controlsRows = new ControlRow[foundRows.Length];
            for (int i = 0; i < foundRows.Length; i++)
                _controlsRows[i] = foundRows[i];
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

            string message = binding == null
                ? GetControlRowLabel(row)
                : $"{GetControlRowLabel(row)}, {binding}";
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
            QueueSpeech(WithControlsIntroduction("Reset to Default"));
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

        string[] values = new string[_controlsRows.Length];
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

        return string.Join("\u001f", values);
    }

    private static string? ReadControlBinding(ControlRow row, InputRebindingManager? manager)
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

    private static string GetControlRowLabel(ControlRow row) => row.ActionName switch
    {
        "Bop" => "Bop",
        "Twist" => "Twist",
        "Pull" => "Pull",
        "Spin" => "Spin",
        "Flick" => "Flick",
        "AltBop" => "Bop, Player 2",
        _ => string.IsNullOrWhiteSpace(row.ActionName) ? "Control" : row.ActionName
    };

    private string WithControlsIntroduction(string message)
    {
        if (!_controlsIntroductionPending)
            return message;

        _controlsIntroductionPending = false;
        return $"Controls. Use Back to return to Settings. {message}";
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
