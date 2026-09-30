using System.Threading;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string SpeechEnabledPreferenceKey = "BopItAccess.SpeechEnabled";
    private readonly ManualResetEventSlim _startupSpeechReady = new(false);
    private volatile bool _speechEnabled = true;
    private bool _speechToggleInitialized;
    private string _startupSpeechAnnouncement = "Bop It Access Ready";
    private string? _pendingToggleSpeechNotice;

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

        lock (_speechLock)
        {
            _speechEnabled = enabled;
            _startupSpeechAnnouncement = enabled
                ? "Bop It Access Ready"
                : "Bop It Access speech is off. " + recovery;
        }

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
            _pendingToggleSpeechNotice = notice;
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
            _soloBackInstructionPending = false;
            _gameOverReadScoreInstructionPending = false;
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
            _pendingToggleSpeechNotice = notice;
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
