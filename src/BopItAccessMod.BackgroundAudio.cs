using Il2Cpp;
using Il2CppFMODUnity;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string BackgroundAudioPreference = "BopItAccess.MuteAudioInBackground";
    private bool _muteAudioInBackground;
    private bool _backgroundAudioPreferenceLoaded;
    private SettingsToggle? _backgroundAudioToggle;
    private UnityAction? _backgroundAudioSubmitListener;
    private int _backgroundAudioSettingsPanelId;
    private string? _backgroundAudioRenderedLocale;
    private long _nextBackgroundAudioSettingsProbeAt;
    private long _nextBackgroundAudioErrorAt;
    private Action<bool>? _managedBackgroundFocusCallback;
    private Il2CppSystem.Action<bool>? _nativeBackgroundFocusCallback;
    private bool _backgroundFocusListenerAttached;
    private bool _backgroundFmodMuted;
    private bool _backgroundUnityMuted;
    private bool _fmodMuteBeforeBackground;
    private float _unityVolumeBeforeBackground;

    private void UpdateBackgroundAudio()
    {
        if (!_backgroundAudioPreferenceLoaded)
        {
            _muteAudioInBackground =
                PlayerPrefs.GetInt(BackgroundAudioPreference, 0) == 1;
            _backgroundAudioPreferenceLoaded = true;
            WriteStatus("Mute audio in background: " +
                (_muteAudioInBackground ? "On." : "Off."));
        }

        // Unity can suspend Update when the window loses focus. Its focus
        // event still arrives, allowing the audio to mute immediately.
        if (!_backgroundFocusListenerAttached)
        {
            try
            {
                _managedBackgroundFocusCallback = OnBackgroundAudioFocusChanged;
                _nativeBackgroundFocusCallback =
                    DelegateSupport.ConvertDelegate<Il2CppSystem.Action<bool>>(
                        _managedBackgroundFocusCallback);
                Application.focusChanged += _nativeBackgroundFocusCallback;
                _backgroundFocusListenerAttached = true;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("Could not attach window focus listener", ex);
            }
        }

        // The poll also covers a missed callback during scene initialization.
        ApplyBackgroundAudioFocus(Application.isFocused);
        UpdateBackgroundAudioSettingsRow();
    }

    private void OnBackgroundAudioFocusChanged(bool focused)
    {
        try
        {
            ApplyBackgroundSpeechFocus(focused);
        }
        catch (Exception ex)
        {
            WriteStatus("Window focus speech update failed: " + ex.Message);
        }
        try
        {
            ApplyBackgroundAudioFocus(focused);
        }
        catch (Exception ex)
        {
            LogBackgroundAudioError("Window focus audio update failed", ex);
        }
    }

    private void ApplyBackgroundAudioFocus(bool focused)
    {
        if (!_muteAudioInBackground || focused)
        {
            RestoreBackgroundAudio();
            return;
        }

        if (!_backgroundFmodMuted)
        {
            try
            {
                _fmodMuteBeforeBackground = RuntimeManager.IsMuted;
                RuntimeManager.MuteAllEvents(true);
                _backgroundFmodMuted = true;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("FMOD background mute failed", ex);
            }
        }

        if (!_backgroundUnityMuted)
        {
            try
            {
                _unityVolumeBeforeBackground = AudioListener.volume;
                AudioListener.volume = 0f;
                _backgroundUnityMuted = true;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("Unity background mute failed", ex);
            }
        }
    }

    private void RestoreBackgroundAudio()
    {
        if (_backgroundFmodMuted)
        {
            try
            {
                RuntimeManager.MuteAllEvents(_fmodMuteBeforeBackground);
                _backgroundFmodMuted = false;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("FMOD background mute restoration failed", ex);
            }
        }

        if (_backgroundUnityMuted)
        {
            try
            {
                AudioListener.volume = _unityVolumeBeforeBackground;
                _backgroundUnityMuted = false;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("Unity background volume restoration failed", ex);
            }
        }
    }

    private void StopBackgroundAudio()
    {
        RestoreBackgroundAudio();
        if (_backgroundFocusListenerAttached && _nativeBackgroundFocusCallback != null)
        {
            try
            {
                Application.focusChanged -= _nativeBackgroundFocusCallback;
            }
            catch (Exception ex)
            {
                LogBackgroundAudioError("Could not detach window focus listener", ex);
            }
        }
        _backgroundFocusListenerAttached = false;
        _nativeBackgroundFocusCallback = null;
        _managedBackgroundFocusCallback = null;
    }

    private void UpdateBackgroundAudioSettingsRow()
    {
        long now = Environment.TickCount64;
        if (now < _nextBackgroundAudioSettingsProbeAt)
            return;
        _nextBackgroundAudioSettingsProbeAt = now + 250;

        SettingsPanel? panel = _settingsPanel;
        if (panel == null)
            panel = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
            return;

        if (_backgroundAudioToggle != null &&
            _backgroundAudioSettingsPanelId == panel.GetInstanceID())
        {
            if (!string.Equals(_backgroundAudioRenderedLocale,
                CurrentGameLocale, StringComparison.Ordinal))
            {
                _backgroundAudioRenderedLocale = CurrentGameLocale;
                SetSpeechRowLabel(_backgroundAudioToggle,
                    "MUTE AUDIO IN BACKGROUND");
                SetSpeechToggleDisplay(_backgroundAudioToggle,
                    _muteAudioInBackground);
            }
            // Native SettingsToggle.Start can update its visual state after
            // our listener; keep the row tied to the saved preference.
            if (_backgroundAudioToggle.IsOn != _muteAudioInBackground)
                SetSpeechToggleDisplay(_backgroundAudioToggle,
                    _muteAudioInBackground);
            return;
        }

        try
        {
            AddBackgroundAudioSettingsRow(panel);
        }
        catch (Exception ex)
        {
            LogBackgroundAudioError("MUTE AUDIO IN BACKGROUND row could not be added", ex);
        }
    }

    private void AddBackgroundAudioSettingsRow(SettingsPanel panel)
    {
        SettingsToggle? template = panel.vibration;
        SettingsSlider? voice = panel.voiceOver;
        if (template == null || voice == null || voice.transform.parent == null)
            throw new InvalidOperationException("Native audio settings rows are unavailable.");

        Transform parent = voice.transform.parent;
        GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        clone.SetActive(false);
        try
        {
            clone.name = "BopItAccess Mute Audio In Background";
            clone.transform.SetSiblingIndex(voice.transform.GetSiblingIndex() + 1);
            SettingsToggle? row = clone.GetComponent<SettingsToggle>();
            if (row == null)
                throw new InvalidOperationException("Cloned Vibration row lost SettingsToggle.");

            SetSpeechRowLabel(row, "MUTE AUDIO IN BACKGROUND");
            row.Submitted = new UnityEvent();
            row.ValueChanged = new UnityEvent<bool>();
            _backgroundAudioSubmitListener ??=
                (UnityAction)OnBackgroundAudioSubmitted;
            row.Submitted.AddListener(_backgroundAudioSubmitListener);
            SetSpeechToggleDisplay(row, _muteAudioInBackground);

            _backgroundAudioToggle = row;
            _backgroundAudioSettingsPanelId = panel.GetInstanceID();
            _backgroundAudioRenderedLocale = CurrentGameLocale;
            clone.SetActive(true);
            if (parent.GetComponent<RectTransform>() is RectTransform content)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (_settingsWasVisible)
                _settingsOptions = CreateSettingsOptions(panel);
            WriteStatus("Added MUTE AUDIO IN BACKGROUND toggle below VOICE OVER.");
        }
        catch
        {
            _backgroundAudioToggle = null;
            _backgroundAudioSettingsPanelId = 0;
            UnityEngine.Object.Destroy(clone);
            throw;
        }
    }

    private void OnBackgroundAudioSubmitted()
    {
        _muteAudioInBackground = !_muteAudioInBackground;
        PlayerPrefs.SetInt(BackgroundAudioPreference,
            _muteAudioInBackground ? 1 : 0);
        SaveModPreferencesAndConfig();
        ApplyBackgroundAudioFocus(Application.isFocused);
        WriteStatus("Mute audio in background changed to " +
            (_muteAudioInBackground ? "On." : "Off."));
    }

    private void LogBackgroundAudioError(string context, Exception ex)
    {
        long now = Environment.TickCount64;
        if (now < _nextBackgroundAudioErrorAt)
            return;
        _nextBackgroundAudioErrorAt = now + 5000;
        WriteStatus(context + ": " + ex);
        MelonLoader.MelonLogger.Warning(context + ": " + ex.Message);
    }
}
