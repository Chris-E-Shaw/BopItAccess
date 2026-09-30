using System.Globalization;
using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private CalibratePanel? _calibrationPanel;
    private Transform? _calibrateAction;
    private Transform? _backCalibrationAction;
    private Transform? _bopCalibrationAction;
    private CalibrateState? _lastCalibrationState;
    private int _lastCalibrationActionId;
    private string? _lastCalibrationResult;
    private string? _lastSpokenCalibrationCountdown;
    private long _lastCalibrationCountdownAt;
    private long _nextCalibrationSearchAt;
    private bool _calibrationWasVisible;

    // True means this panel owns the current screen, even when it has no selected button.
    private bool ReadCalibrationFocus()
    {
        if (_calibrationPanel == null)
        {
            long now = Environment.TickCount64;
            if (now < _nextCalibrationSearchAt)
                return false;

            _nextCalibrationSearchAt = now + 500;
            Panel? knownPanel = _mainMenu == null ? null : _mainMenu.calibrationPanel;
            _calibrationPanel = knownPanel == null ? null : knownPanel.GetComponent<CalibratePanel>();
            if (_calibrationPanel == null)
                _calibrationPanel = UnityEngine.Object.FindFirstObjectByType<CalibratePanel>();
            if (_calibrationPanel == null)
                return false;

            WriteStatus("Found the audio calibration panel; waiting for it to become visible.");
        }

        CalibratePanel panel = _calibrationPanel;
        if (!panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetCalibrationFocus();
            return false;
        }

        if (!_calibrationWasVisible)
        {
            _calibrationWasVisible = true;
            _calibrateAction = panel.actionContainer == null
                ? null : panel.actionContainer.transform.Find("Calibrate");
            _backCalibrationAction = panel.actionContainer == null
                ? null : panel.actionContainer.transform.Find("Back");
            _bopCalibrationAction = panel.visualiser == null
                ? null : panel.visualiser.transform.Find("Device");
            WriteStatus("Audio calibration panel is visible; monitoring actions and result.");
        }

        CalibrateState state = panel.State;
        string? result = state == CalibrateState.Result && panel.latencyContainer?.IsVisible == true
            ? CleanSpeechValue(panel.latency == null ? null : panel.latency.text)
            : null;
        string? countdown = state == CalibrateState.Warmup
            ? ReadCalibrationCountdown(panel)
            : null;

        GameObject? selected = EventSystem.current == null
            ? null : EventSystem.current.currentSelectedGameObject;
        (int actionId, string? actionLabel) = GetFocusedCalibrationAction(selected, state);
        bool stageChanged = !_lastCalibrationState.HasValue || state != _lastCalibrationState.Value;
        bool focusChanged = actionId != _lastCalibrationActionId;
        bool resultChanged = result != null && !string.Equals(result, _lastCalibrationResult, StringComparison.Ordinal);

        _lastCalibrationState = state;
        _lastCalibrationActionId = actionId;
        _lastCalibrationResult = result;

        string? announcement = null;
        if (stageChanged)
        {
            announcement = state switch
            {
                CalibrateState.Start => "Audio calibration. Bop to the beat.",
                CalibrateState.Warmup => "Get ready. Bop to the beat.",
                CalibrateState.Calibrate => "Bop to the beat.",
                CalibrateState.Finished => "Calibration finished. Calculating latency.",
                CalibrateState.Result => result == null
                    ? "Audio calibration complete."
                    : $"Audio calibration complete. Latency, {result}.",
                _ => null
            };

            WriteStatus($"Audio calibration state changed to {state}.");
            if (state == CalibrateState.Warmup)
            {
                _lastSpokenCalibrationCountdown = countdown;
                _lastCalibrationCountdownAt = Environment.TickCount64;
                if (countdown != null)
                    announcement = $"{announcement} {countdown}.";
            }
            else
            {
                _lastSpokenCalibrationCountdown = null;
            }
        }
        else if (resultChanged)
        {
            announcement = $"Latency, {result}.";
        }

        if (focusChanged && actionLabel != null)
        {
            WriteStatus($"Audio calibration focused action: {actionLabel}.");
            announcement = announcement == null ? actionLabel : $"{announcement} {actionLabel}.";
        }

        // The warmup countdown is useful, but speech during the actual beat
        // matching would mask the audio cues. Announce at most once per second.
        if (state == CalibrateState.Warmup && !stageChanged && countdown != null &&
            !string.Equals(countdown, _lastSpokenCalibrationCountdown, StringComparison.Ordinal))
        {
            long now = Environment.TickCount64;
            if (now - _lastCalibrationCountdownAt >= 900)
            {
                _lastSpokenCalibrationCountdown = countdown;
                _lastCalibrationCountdownAt = now;
                announcement = announcement == null ? countdown : $"{announcement} {countdown}.";
            }
        }

        if (announcement != null)
            QueueSpeech(announcement);

        return true;
    }

    private static string? ReadCalibrationCountdown(CalibratePanel panel)
    {
        if (panel.countdown == null || !panel.countdown.gameObject.activeInHierarchy)
            return null;

        string? text = CleanSpeechValue(panel.countdown.text);
        // Long text is an instruction, not a countdown tick.
        if (text == null || text.Length > 8)
            return null;

        // Some builds format a countdown as a fraction of a second. Speak only
        // whole remaining seconds, so speech never chases every frame.
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double seconds) ||
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds))
        {
            if (seconds >= 0 && seconds <= 60)
                return Math.Ceiling(seconds).ToString(CultureInfo.InvariantCulture);
        }

        return text;
    }

    private (int Id, string? Label) GetFocusedCalibrationAction(GameObject? selected, CalibrateState state)
    {
        if (selected == null)
            return (0, null);

        var choices = new List<(Transform Action, string Label)>(3);
        if (_calibrationPanel?.actionContainer?.IsVisible == true)
        {
            AddAvailableCalibrationAction(choices, _calibrateAction, "Calibrate");
            AddAvailableCalibrationAction(choices, _backCalibrationAction, "Back");
        }
        if (state == CalibrateState.Warmup || state == CalibrateState.Calibrate)
            AddAvailableCalibrationAction(choices, _bopCalibrationAction, "Bop");

        Transform selectedTransform = selected.transform;
        for (int index = 0; index < choices.Count; index++)
        {
            (Transform action, string label) = choices[index];
            if (IsCalibrationAction(selectedTransform, action))
                return (action.GetInstanceID(),
                    WithMenuIndex(WithControlType(label, "button"), index, choices.Count));
        }
        return (0, null);
    }

    private static void AddAvailableCalibrationAction(
        List<(Transform Action, string Label)> choices, Transform? action, string label)
    {
        if (action != null && action.gameObject.activeInHierarchy)
            choices.Add((action, label));
    }

    private static bool IsCalibrationAction(Transform selected, Transform? action) =>
        action != null && action.gameObject.activeInHierarchy &&
        (selected == action || selected.IsChildOf(action));

    private void ResetCalibrationFocus()
    {
        _calibrationWasVisible = false;
        _calibrateAction = null;
        _backCalibrationAction = null;
        _bopCalibrationAction = null;
        _lastCalibrationState = null;
        _lastCalibrationActionId = 0;
        _lastCalibrationResult = null;
        _lastSpokenCalibrationCountdown = null;
        _lastCalibrationCountdownAt = 0;
    }
}
