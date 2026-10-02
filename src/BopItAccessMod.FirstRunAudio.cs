using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // 0 means a first run is waiting for the game's settings to load, 1 means
    // the levels were applied, and 2 marks an installation predating this
    // feature. Only the pending state may alter native levels.
    private const string NativeAudioFirstRunStateKey = "BopItAccess.NativeAudioFirstRunState";

    private static readonly string[] PriorModPreferenceKeys =
    {
        SpeechEnabledPreferenceKey, BrailleOutputPreferenceKey,
        BackgroundAudioPreference, FilterCapitalisationPreferenceKey,
        ReadControlTypesPreferenceKey, FpsLimitPreference, HintsTypePreferenceKey,
        IndexingPreferenceKey, OneOnOneFeedbackPreferenceKey,
        SliderRangesPreferenceKey, OutputModePreferenceKey,
        SapiVoicePreferenceKey, SapiVolumePreferenceKey,
        SapiRatePreferenceKey, SapiPitchPreferenceKey,
        SapiTrimSilencePreferenceKey, LegacyRepeatButtonHintsPreferenceKey,
        ReadButtonHintsPreferenceKey, MuteSpeechInBackgroundPreferenceKey,
        ButtonHintsDelayPreferenceKey, RepeatButtonHintsCountPreferenceKey,
        RepeatButtonHintsIntervalPreferenceKey,
        DescriptionKeyboardKey, DescriptionGamepadKey,
        ScoreKeyboardKey, ScoreGamepadKey,
        SpeakHintsKeyboardKey, SpeakHintsGamepadKey,
        ToggleSpeechKeyboardKey, ToggleSpeechGamepadKey,
        ChangeSpeechOutputKeyboardKey, ChangeSpeechOutputGamepadKey
    };

    private static BopItAccessMod? _activeNativeAudioMod;
    private bool _nativeAudioHadPreviousLog;
    private bool _nativeAudioPreferenceChecked;
    private bool _nativeAudioShouldApply;
    private bool _nativeAudioHandled;
    private bool _nativeSettingsLoadObserved;
    private Settings? _nativeLoadedSettings;
    private long _nextNativeAudioErrorLogAt;

    private void PrepareFirstRunNativeAudioDefaults()
    {
        // WriteStatus creates this file, so take the snapshot first. Earlier
        // builds always wrote a log even when their preferences stayed default.
        _nativeAudioHadPreviousLog = File.Exists(StatusLogPath);
        _activeNativeAudioMod = this;
    }

    internal static void NoteNativeSettingsLoaded(Settings settings)
    {
        BopItAccessMod? mod = _activeNativeAudioMod;
        if (mod == null)
            return;

        // Settings.Load is synchronous, but Settings.Start is a coroutine.
        // A postfix on Load observes both the saved-file and no-file paths,
        // after the game has finished restoring its native settings.
        mod._nativeLoadedSettings = settings;
        mod._nativeSettingsLoadObserved = true;
    }

    private void UpdateFirstRunNativeAudioDefaults()
    {
        if (_nativeAudioHandled)
            return;

        try
        {
            if (!_nativeAudioPreferenceChecked)
            {
                int state = PlayerPrefs.GetInt(NativeAudioFirstRunStateKey, -1);
                if (state is 1 or 2)
                {
                    _nativeAudioHandled = true;
                    return;
                }

                if (state != 0 &&
                    (_nativeAudioHadPreviousLog || HasPriorModPreference()))
                {
                    PlayerPrefs.SetInt(NativeAudioFirstRunStateKey, 2);
                    PlayerPrefs.Save();
                    _nativeAudioHandled = true;
                    WriteStatus("Previous Bop It Access installation detected; preserving native audio levels.");
                    return;
                }

                if (state != 0)
                {
                    // Persist the intention before waiting. If the player
                    // closes the game while it loads, next launch resumes
                    // instead of mistaking this run's log for an old install.
                    PlayerPrefs.SetInt(NativeAudioFirstRunStateKey, 0);
                    PlayerPrefs.Save();
                }
                _nativeAudioShouldApply = true;
                _nativeAudioPreferenceChecked = true;
                WriteStatus("First Bop It Access run detected; waiting for native settings load before lowering audio levels.");
            }

            if (!_nativeAudioShouldApply || !_nativeSettingsLoadObserved)
                return;

            Settings? settings = _nativeLoadedSettings;
            SettingsData? data = settings == null ? null : settings.SettingsData;
            if (settings == null || data == null)
                return;

            // Native values are percentages. Update the game data and its FMOD
            // buses together; SettingsSlider.SetValue changes only the label.
            data.MusicVolume = 30f;
            data.SFXVolume = 30f;
            data.VoiceOverVolume = 30f;
            settings.UpdateAudio(data);
            settings.Save();

            // If Settings is already open, refresh the text too. Normally the
            // panel is still hidden and Show() reads these new native values.
            SettingsPanel? panel = _settingsPanel;
            if (panel != null && panel.IsVisible)
            {
                panel.music?.SetValue("30");
                panel.sfx?.SetValue("30");
                panel.voiceOver?.SetValue("30");
            }

            PlayerPrefs.SetInt(NativeAudioFirstRunStateKey, 1);
            PlayerPrefs.Save();
            _nativeAudioHandled = true;
            WriteStatus("First-run native audio levels set to Music 30, SFX 30, Voice Over 30 and saved.");
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextNativeAudioErrorLogAt)
            {
                _nextNativeAudioErrorLogAt = now + 5000;
                WriteStatus("First-run native audio setup failed; will retry: " + ex);
                MelonLoader.MelonLogger.Warning("First-run native audio setup failed; will retry: " + ex.Message);
            }
        }
    }

    private static bool HasPriorModPreference()
    {
        foreach (string key in PriorModPreferenceKeys)
        {
            if (PlayerPrefs.HasKey(key))
                return true;
        }

        return false;
    }
}

[HarmonyPatch(typeof(Settings), "Load")]
[SupportedOSPlatform("windows")]
internal static class NativeSettingsLoadPatch
{
    [HarmonyPostfix]
    private static void AfterLoad(Settings __instance) =>
        BopItAccessMod.NoteNativeSettingsLoaded(__instance);
}
