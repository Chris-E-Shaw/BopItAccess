using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
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
    private static BopItAccessMod? _activeCalibrationMod;
    private int _calibrationFinalBeatObserved;
    private int _calibrationAcceptedInput;
    private float _calibrationLastAcceptedInputTime;
    private int _calibrationEndNoticePending;
    private bool _calibrationEndNoticeSpoken;
    private bool _calibrationAttemptFailed;

    // True means this panel owns the current screen, even when it has no selected button.
    private bool ReadCalibrationFocus()
    {
        Volatile.Write(ref _activeCalibrationMod, this);
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
        int endNotice = Interlocked.Exchange(ref _calibrationEndNoticePending, 0);
        if (endNotice != 0 && !_calibrationEndNoticeSpoken)
            AnnounceCalibrationEnd(endNotice == 2);
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
                CalibrateState.Start => L("Audio calibration."),
                CalibrateState.Warmup => L("Get ready. Bop to the beat."),
                CalibrateState.Calibrate => L("Bop to the beat."),
                CalibrateState.Finished => _calibrationEndNoticeSpoken ? null :
                    L("Done!"),
                CalibrateState.Result when _calibrationAttemptFailed => null,
                CalibrateState.Result => result == null
                    ? L("Audio calibration complete.")
                    : LF("Audio calibration complete. Latency, {0}.", result),
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
        else if (resultChanged && !_calibrationAttemptFailed)
        {
            announcement = LF("Latency, {0}.", result);
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
        {
            // The native Finished state can become Result within the same
            // call. Keep its result behind the short end notice even if the
            // Prism worker has not picked that notice up yet.
            if (_calibrationEndNoticeSpoken && (stageChanged || resultChanged) &&
                (state == CalibrateState.Finished || state == CalibrateState.Result))
                QueueSequentialSpeech(announcement);
            else if (focusChanged && actionLabel != null &&
                state != CalibrateState.Warmup && state != CalibrateState.Calibrate &&
                state != CalibrateState.Finished)
                QueueFocusSpeech(announcement);
            else
                QueueSpeech(announcement);
        }

        return true;
    }

    private void AnnounceCalibrationEnd(bool failed)
    {
        _calibrationEndNoticeSpoken = true;
        _calibrationAttemptFailed = failed;
        string notice = failed ? L("Calibration failed.") : L("Done!");
        WriteStatus(failed
            ? "Audio calibration ended without any accepted calibration-phase input."
            : "Audio calibration final input/end reached; no more beat inputs are needed.");
        QueueSpeech(notice);
    }

    private bool IsObservedCalibrationPanel(CalibratePanel panel) =>
        !ReferenceEquals(_calibrationPanel, null) &&
        panel.Pointer == _calibrationPanel.Pointer;

    internal static void NoteCalibrationStarted(CalibratePanel panel)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeCalibrationMod);
        if (mod == null || !mod.IsObservedCalibrationPanel(panel))
            return;
        Volatile.Write(ref mod._calibrationFinalBeatObserved, 0);
        Volatile.Write(ref mod._calibrationAcceptedInput, 0);
        Volatile.Write(ref mod._calibrationLastAcceptedInputTime, float.NegativeInfinity);
        Interlocked.Exchange(ref mod._calibrationEndNoticePending, 0);
        mod._calibrationEndNoticeSpoken = false;
        mod._calibrationAttemptFailed = false;
    }

    internal static void NoteCalibrationMarker(CalibratePanel panel,
        Il2CppFMOD.Studio.TIMELINE_MARKER_PROPERTIES marker)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeCalibrationMod);
        if (mod == null || !mod.IsObservedCalibrationPanel(panel) ||
            panel.State != CalibrateState.Calibrate)
            return;

        // The shipped calibration chart repeats its Kick/Snare pair while
        // LoopCounter is below 7. Its End transition exits on the eighth loop.
        // The final Snare therefore identifies the final required beat, not
        // an arbitrary total of button presses (extra taps cannot finish it).
        // Observe only; do not alter FMOD parameters, timing, or game inputs.
        try
        {
            string name = marker.name;
            if (!string.Equals(name, "Snare", StringComparison.Ordinal) ||
                panel.eventInstance.getParameterByName("LoopCounter", out float loop,
                    out float finalLoop) != Il2CppFMOD.RESULT.OK ||
                Math.Max(loop, finalLoop) < 7f)
                return;
            Volatile.Write(ref mod._calibrationFinalBeatObserved, 1);

            // A player can tap slightly before the audible beat. An already
            // accepted tap within a quarter beat also belongs to this last
            // beat; announce from Unity's next update instead of waiting End.
            float earlyTapAge = panel.calibrationTime -
                Volatile.Read(ref mod._calibrationLastAcceptedInputTime);
            if (Volatile.Read(ref mod._calibrationAcceptedInput) != 0 &&
                earlyTapAge >= 0f && earlyTapAge <= panel.beatPeriod * 0.25f)
                Interlocked.CompareExchange(ref mod._calibrationEndNoticePending, 1, 0);
        }
        catch
        {
            // Older/different game charts may lack this optional metadata.
            // The native OnFinished observation remains the safe fallback.
        }
    }

    internal static void NoteCalibrationInput(CalibratePanel panel,
        CalibrateState previousState, int previousCount)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeCalibrationMod);
        if (mod == null || !mod.IsObservedCalibrationPanel(panel) ||
            previousState != CalibrateState.Calibrate ||
            panel.playerInputTimes == null || panel.playerInputTimes.Count <= previousCount)
            return;

        Volatile.Write(ref mod._calibrationAcceptedInput, 1);
        Volatile.Write(ref mod._calibrationLastAcceptedInputTime,
            panel.playerInputTimes[panel.playerInputTimes.Count - 1]);
        if (Volatile.Read(ref mod._calibrationFinalBeatObserved) != 0 &&
            !mod._calibrationEndNoticeSpoken && panel.IsVisible &&
            panel.gameObject.activeInHierarchy)
            mod.AnnounceCalibrationEnd(failed: false);
    }

    internal static void NoteCalibrationEnded(CalibratePanel panel)
    {
        BopItAccessMod? mod = Volatile.Read(ref _activeCalibrationMod);
        if (mod == null || !mod.IsObservedCalibrationPanel(panel) ||
            panel.State != CalibrateState.Calibrate)
            return;

        // FMOD can call this on its audio thread. Only post an atomic notice;
        // localization, Unity visibility checks, and speech happen on Unity's
        // thread. Native no-input attempts also produce a numeric latency, so
        // zero milliseconds is not a safe way to identify a failed attempt.
        Interlocked.CompareExchange(ref mod._calibrationEndNoticePending,
            Volatile.Read(ref mod._calibrationAcceptedInput) == 0 ? 2 : 1, 0);
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
        {
            string visible = CleanSpeechValue(
                action.GetComponentInChildren<TMP_Text>(true)?.text) ?? L(label);
            choices.Add((action, visible));
        }
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
        Volatile.Write(ref _calibrationFinalBeatObserved, 0);
        Volatile.Write(ref _calibrationAcceptedInput, 0);
        Volatile.Write(ref _calibrationLastAcceptedInputTime, float.NegativeInfinity);
        Interlocked.Exchange(ref _calibrationEndNoticePending, 0);
        _calibrationEndNoticeSpoken = false;
        _calibrationAttemptFailed = false;
    }
}

[HarmonyPatch(typeof(CalibratePanel), "OnStart")]
[SupportedOSPlatform("windows")]
internal static class CalibrationStartNoticePatch
{
    [HarmonyPrefix]
    private static void BeforeStart(CalibratePanel __instance)
    {
        try { BopItAccessMod.NoteCalibrationStarted(__instance); }
        catch { } // An optional announcement must never abort native calibration.
    }
}

[HarmonyPatch(typeof(CalibratePanel), "OnMarker")]
[SupportedOSPlatform("windows")]
internal static class CalibrationFinalBeatPatch
{
    [HarmonyPostfix]
    private static void AfterMarker(CalibratePanel __instance,
        Il2CppFMOD.Studio.TIMELINE_MARKER_PROPERTIES marker)
    {
        try { BopItAccessMod.NoteCalibrationMarker(__instance, marker); }
        catch { } // Preserve FMOD's callback and its native timing.
    }
}

[HarmonyPatch]
[SupportedOSPlatform("windows")]
internal static class CalibrationAcceptedInputPatch
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(CalibratePanel), "InputBop");
        yield return AccessTools.Method(typeof(CalibratePanel), "OnInput");
    }

    [HarmonyPrefix]
    private static void BeforeInput(CalibratePanel __instance,
        out (CalibrateState State, int Count) __state)
    {
        __state = (CalibrateState.Start, 0);
        try { __state = (__instance.State, __instance.playerInputTimes?.Count ?? 0); }
        catch { } // Native input must still run if observation is unavailable.
    }

    [HarmonyPostfix]
    private static void AfterInput(CalibratePanel __instance,
        (CalibrateState State, int Count) __state)
    {
        try { BopItAccessMod.NoteCalibrationInput(__instance, __state.State, __state.Count); }
        catch { }
    }
}

[HarmonyPatch(typeof(CalibratePanel), "OnFinished")]
[SupportedOSPlatform("windows")]
internal static class CalibrationEndNoticePatch
{
    [HarmonyPrefix]
    private static void BeforeFinished(CalibratePanel __instance)
    {
        try { BopItAccessMod.NoteCalibrationEnded(__instance); }
        catch { } // Native latency estimation and result display must still run.
    }
}
