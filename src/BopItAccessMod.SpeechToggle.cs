using System.Threading;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string SpeechEnabledPreferenceKey = "BopItAccess.SpeechEnabled";
    private readonly ManualResetEventSlim _startupSpeechReady = new(false);
    private volatile bool _speechEnabled = true;
    private bool _speechToggleInitialized;
    private string _startupSpeechAnnouncement =
        "Bop It Access speech is ready. The game is still loading. Wait for the main menu announcement before using the controls.";
    private string? _pendingToggleSpeechNotice;
    private volatile bool _speechSuppressedForBackground;
    private bool _backgroundSpeechFocusKnown;
    private bool _backgroundRecoveryNoticeNeeded;

    // Called from OnUpdate on Unity's main thread. The Tolk worker waits for
    // this before its first announcement so an OFF preference never produces
    // the ordinary Ready announcement.
    private void InitializeSpeechToggleOnMainThread()
    {
        if (_speechToggleInitialized)
            return;

        bool enabled = true;
        try
        {
            enabled = PlayerPrefs.GetInt(SpeechEnabledPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read the speech preference: " + ex.Message);
        }

        string recovery = GetSpeechToggleRecoveryInstruction();

        InitializeSpeechBackendPreferencesOnMainThread();
        InitializeIndexingPreferenceOnMainThread();
        InitializeReadControlTypesPreferenceOnMainThread();
        InitializeSliderRangesPreferenceOnMainThread();
        InitializeHintsTypePreferenceOnMainThread();
        InitializeOneOnOneFeedbackPreferenceOnMainThread();

        lock (_speechLock)
        {
            _speechEnabled = enabled;
            _startupSpeechAnnouncement = enabled
                ? "Bop It Access speech is ready. The game is still loading. Wait for the main menu announcement before using the controls."
                : "Bop It Access speech is off. " + recovery;
        }

        // The first focus reading must precede the worker's startup signal.
        // A game launched in the background must not speak its ready message.
        UpdateBackgroundSpeechFocus();
        _speechToggleInitialized = true;
        _startupSpeechReady.Set();
        WriteStatus(enabled ? "Speech preference loaded: on." :
            "Speech preference loaded: off; startup recovery instructions prepared.");
    }

    private void UpdateSpeechToggleFromInput()
    {
        if (!_speechToggleInitialized || !WasToggleSpeechPressed())
            return;

        SetSpeechEnabled(!_speechEnabled);
    }

    private void SetSpeechEnabledFromMenu(bool enabled)
    {
        if (_speechToggleInitialized)
            SetSpeechEnabled(enabled);
    }

    private void SetSpeechEnabled(bool enabled)
    {
        if (enabled == _speechEnabled)
            return;

        string notice = enabled ? "Speech on." :
            "Speech off. " + GetSpeechToggleRecoveryInstruction();

        lock (_speechLock)
        {
            _speechEnabled = enabled;
            // Discard all speech generated under the previous state. The
            // notice has its own slot so it can still speak while OFF.
            _pendingSpeech = null;
            _pendingPrioritySpeech = null;
            _pendingPriorityFollowUpSpeech = null;
            _pendingSpeechIsDescription = false;
            _sequentialSpeech.Clear();
            _descriptionSpeechMayBeActive = false;
            _pendingToggleSpeechNotice = _speechSuppressedForBackground
                ? null : notice;
            if (_speechSuppressedForBackground)
                _backgroundRecoveryNoticeNeeded = !enabled;
            _silenceRequested = true;
            _speechGeneration++;
            _sapiRenderSerial++;
        }

        if (!enabled)
        {
            // A protected automatic result can still hold menu text outside
            // the worker queue. Drop it when muting so it cannot surface
            // later if speech is restored before that timer expires.
            Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
            Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
            _deferredGameOverResultUpdates.Clear();
            _deferredGameOverMenuUpdate = null;
        }

        try
        {
            PlayerPrefs.SetInt(SpeechEnabledPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save the speech preference: " + ex.Message);
        }

        WriteStatus(enabled ? "Speech toggled on." : "Speech toggled off.");
        _speechRequested.Set();
    }

    private void UpdateBackgroundSpeechFocus() =>
        ApplyBackgroundSpeechFocus(Application.isFocused);

    private void ApplyBackgroundSpeechFocus(bool focused)
    {
        // Until Unity supplies its first focus state, the worker assumes
        // speech is allowed. The focus event handles suspended Update loops.
        bool suppress = _muteSpeechInBackground && !focused;

        if (!_backgroundSpeechFocusKnown)
        {
            _backgroundSpeechFocusKnown = true;
            _speechSuppressedForBackground = suppress;
            if (suppress && !_speechEnabled)
                _backgroundRecoveryNoticeNeeded = true;
            return;
        }
        if (suppress == _speechSuppressedForBackground)
            return;

        if (suppress)
        {
            lock (_speechLock)
            {
                _speechSuppressedForBackground = true;
                if (!_speechEnabled || _pendingToggleSpeechNotice != null)
                    _backgroundRecoveryNoticeNeeded = !_speechEnabled;
                _pendingSpeech = null;
                _pendingPrioritySpeech = null;
                _pendingPriorityFollowUpSpeech = null;
                _pendingToggleSpeechNotice = null;
                _pendingSpeechIsDescription = false;
                _sequentialSpeech.Clear();
                _descriptionSpeechMayBeActive = false;
                _silenceRequested = true;
                _speechGeneration++;
                _sapiRenderSerial++;
            }

            // Score/menu follow-ups held outside the worker queue are also
            // stale once the game is no longer the foreground window.
            Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
            Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
            _deferredGameOverResultUpdates.Clear();
            _deferredGameOverMenuUpdate = null;
            _speechRequested.Set();
            WriteStatus("Game window lost focus; speech paused and queued announcements cleared.");
            return;
        }

        _speechSuppressedForBackground = false;
        if (_backgroundRecoveryNoticeNeeded && !_speechEnabled)
        {
            string notice = "Speech is off. " + GetSpeechToggleRecoveryInstruction();
            lock (_speechLock)
            {
                _pendingToggleSpeechNotice = notice;
                _speechGeneration++;
            }
            _speechRequested.Set();
        }
        _backgroundRecoveryNoticeNeeded = false;
        WriteStatus("Game window regained focus; speech output resumed.");
    }

    // A player can rebind or reset this control while speech is OFF. Announce
    // the new recovery inputs so that changing the binding cannot strand them.
    private void AnnounceSpeechToggleRecoveryNow()
    {
        if (_speechEnabled || !_speechToggleInitialized)
            return;

        string notice = "Speech is off. " + GetSpeechToggleRecoveryInstruction();
        lock (_speechLock)
        {
            if (_speechEnabled)
                return;
            _pendingToggleSpeechNotice = _speechSuppressedForBackground
                ? null : notice;
            if (_speechSuppressedForBackground)
                _backgroundRecoveryNoticeNeeded = true;
            _silenceRequested = true;
            _speechGeneration++;
        }

        _speechRequested.Set();
    }

    private string GetSpeechToggleRecoveryInstruction()
    {
        try
        {
            return ReadToggleSpeechRecoveryInstruction();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read Toggle Speech binding display: " + ex.Message);
            return ReadToggleSpeechFallbackRecoveryInstruction();
        }
    }
}
