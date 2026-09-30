using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string OutputModePreferenceKey = "BopItAccess.OutputMode";
    private const string SapiVoicePreferenceKey = "BopItAccess.SapiVoice";
    private const string SapiVolumePreferenceKey = "BopItAccess.SapiVolume";
    private const string SapiRatePreferenceKey = "BopItAccess.SapiRate";
    private const string SapiPitchPreferenceKey = "BopItAccess.SapiPitch";
    private static readonly string[] OutputModes =
    {
        "Auto", "SAPI", "JAWS", "Window-Eyes", "NVDA", "System Access", "ZoomText"
    };

    private string _outputMode = "Auto";
    private string _sapiVoiceId = string.Empty;
    private int _sapiVolume = 100;
    private int _sapiRate = 50;
    private int _sapiPitch = 50;
    private List<SpeechVoiceOption> _sapiVoices = new();
    private int _sapiSettingsVersion;

    // These objects are owned and touched only by the existing STA speech
    // worker. Unity/PlayerPrefs and the menu stay on the main thread.
    private object? _sapiComVoice;
    private int _sapiAppliedSettingsVersion = -1;
    private string _sapiAppliedVoiceId = string.Empty;
    private OutputBackend _lastOutputBackend;
    private string? _lastFallbackNoticeMode;
    private long _nextSapiErrorLogAt;

    private sealed record SpeechVoiceOption(string Id, string Name);
    private enum OutputBackend { None, Tolk, Sapi, Nvda }

    private void InitializeSpeechBackendPreferencesOnMainThread()
    {
        try
        {
            string saved = PlayerPrefs.GetString(OutputModePreferenceKey, "Auto");
            _outputMode = Array.Find(OutputModes,
                mode => string.Equals(mode, saved, StringComparison.OrdinalIgnoreCase)) ?? "Auto";
            _sapiVoiceId = PlayerPrefs.GetString(SapiVoicePreferenceKey, string.Empty);
            _sapiVolume = Math.Clamp(PlayerPrefs.GetInt(SapiVolumePreferenceKey, 100), 5, 100);
            _sapiRate = Math.Clamp(PlayerPrefs.GetInt(SapiRatePreferenceKey, 50), 0, 100);
            _sapiPitch = Math.Clamp(PlayerPrefs.GetInt(SapiPitchPreferenceKey, 50), 0, 100);
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read speech output preferences: " + ex.Message);
        }

        RefreshSapiVoices();
        WriteStatus($"Output mode: {_outputMode}; SAPI voice: " +
            (string.IsNullOrEmpty(_sapiVoiceId) ? "system default" : _sapiVoiceId) +
            $"; volume {_sapiVolume}, rate {_sapiRate}, pitch {_sapiPitch}.");
    }

    private void RefreshSapiVoices()
    {
        List<SpeechVoiceOption> found = new() { new(string.Empty, "System default") };
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
        foreach (RegistryHive hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
        {
            try
            {
                using RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using RegistryKey? tokens = root.OpenSubKey(@"SOFTWARE\Microsoft\Speech\Voices\Tokens");
                if (tokens == null)
                    continue;

                foreach (string tokenName in tokens.GetSubKeyNames())
                {
                    using RegistryKey? token = tokens.OpenSubKey(tokenName);
                    if (token == null)
                        continue;
                    string id = (hive == RegistryHive.LocalMachine
                        ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER") +
                        @"\SOFTWARE\Microsoft\Speech\Voices\Tokens\" + tokenName;
                    if (!ids.Add(id))
                        continue;
                    using RegistryKey? attributes = token.OpenSubKey("Attributes");
                    string name = token.GetValue(null) as string ??
                        attributes?.GetValue("Name") as string ?? tokenName;
                    found.Add(new(id, name.Trim()));
                }
            }
            catch (Exception ex)
            {
                WriteStatus($"Could not enumerate {hive} SAPI voices: {ex.Message}");
            }
        }

        found.Sort((a, b) => string.IsNullOrEmpty(a.Id) ? -1 :
            string.IsNullOrEmpty(b.Id) ? 1 :
            StringComparer.CurrentCultureIgnoreCase.Compare(a.Name, b.Name));
        _sapiVoices = found;
        if (!string.IsNullOrEmpty(_sapiVoiceId) &&
            !found.Any(v => string.Equals(v.Id, _sapiVoiceId, StringComparison.OrdinalIgnoreCase)))
        {
            WriteStatus("Saved SAPI voice is no longer installed; using the system default.");
            _sapiVoiceId = string.Empty;
            _sapiSettingsVersion++;
            try
            {
                PlayerPrefs.DeleteKey(SapiVoicePreferenceKey);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                WriteStatus("Could not clear an unavailable SAPI voice: " + ex.Message);
            }
        }
    }

    private void SetOutputModeFromMenu(string mode)
    {
        if (!OutputModes.Contains(mode, StringComparer.OrdinalIgnoreCase) ||
            string.Equals(_outputMode, mode, StringComparison.OrdinalIgnoreCase))
            return;
        lock (_speechLock)
        {
            _outputMode = mode;
            _silenceRequested = true;
            _speechGeneration++;
            _pendingSpeech = null;
            _pendingPrioritySpeech = null;
            _pendingPriorityFollowUpSpeech = null;
            _sequentialSpeech.Clear();
            _lastFallbackNoticeMode = null;
        }
        SaveSpeechPreference(OutputModePreferenceKey, mode);
        WriteStatus("Output mode changed to " + mode + ".");
        _speechRequested.Set();
    }

    private void SetSapiVoiceFromMenu(string voiceId)
    {
        if (!_sapiVoices.Any(v => string.Equals(v.Id, voiceId, StringComparison.OrdinalIgnoreCase)) ||
            string.Equals(_sapiVoiceId, voiceId, StringComparison.OrdinalIgnoreCase))
            return;
        lock (_speechLock)
        {
            _sapiVoiceId = voiceId;
            _sapiSettingsVersion++;
        }
        SaveSpeechPreference(SapiVoicePreferenceKey, voiceId);
        WriteStatus("SAPI voice changed to " + (voiceId.Length == 0 ? "system default" : voiceId) + ".");
    }

    private void SetSapiVolumeFromMenu(int value) => SetSapiNumberFromMenu(
        SapiVolumePreferenceKey, ref _sapiVolume, Math.Clamp(value, 5, 100), "volume");

    private void SetSapiRateFromMenu(int value) => SetSapiNumberFromMenu(
        SapiRatePreferenceKey, ref _sapiRate, value, "rate");

    private void SetSapiPitchFromMenu(int value) => SetSapiNumberFromMenu(
        SapiPitchPreferenceKey, ref _sapiPitch, value, "pitch");

    private void SetSapiNumberFromMenu(string key, ref int field, int value, string name)
    {
        value = Math.Clamp(value, 0, 100);
        if (field == value)
            return;
        lock (_speechLock)
        {
            field = value;
            _sapiSettingsVersion++;
        }
        try
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus($"Could not save SAPI {name}: {ex.Message}");
        }
        WriteStatus($"SAPI {name} changed to {value}.");
    }

    private static void SaveSpeechPreference(string key, string value)
    {
        try
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus($"Could not save {key}: {ex.Message}");
        }
    }

    private bool OutputSpeechOnWorker(string text, bool interrupt)
    {
        string mode;
        lock (_speechLock)
            mode = _outputMode;

        string? detected = Marshal.PtrToStringUni(TolkNative.Tolk_DetectScreenReader());
        if (string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            if (detected != null && TolkNative.Tolk_Output(text, interrupt))
            {
                _lastOutputBackend = OutputBackend.Tolk;
                return true;
            }
            return SpeakSapiOnWorker(text, interrupt) || TryTolkAsLastResort(text, interrupt);
        }

        if (string.Equals(mode, "SAPI", StringComparison.OrdinalIgnoreCase))
        {
            if (SpeakSapiOnWorker(text, interrupt))
            {
                _lastFallbackNoticeMode = null;
                return true;
            }
            if (detected == null)
                return false;
            string notice = _lastFallbackNoticeMode == "SAPI" ? text :
                $"SAPI is unavailable. Using {detected}. {text}";
            if (!TryTolkAsLastResort(notice, interrupt))
                return false;
            _lastFallbackNoticeMode = "SAPI";
            return true;
        }

        if (string.Equals(mode, "NVDA", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (NvdaNative.nvdaController_testIfRunning() == 0)
                {
                    if (interrupt)
                        NvdaNative.nvdaController_cancelSpeech();
                    if (NvdaNative.nvdaController_speakText(text) == 0)
                    {
                        _lastOutputBackend = OutputBackend.Nvda;
                        _lastFallbackNoticeMode = null;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSapiError("NVDA direct output failed", ex);
            }
        }

        if (string.Equals(mode, detected, StringComparison.OrdinalIgnoreCase))
        {
            if (TolkNative.Tolk_Output(text, interrupt))
            {
                _lastOutputBackend = OutputBackend.Tolk;
                _lastFallbackNoticeMode = null;
                return true;
            }
        }

        bool announceFallback = _lastFallbackNoticeMode != mode;
        string sapiText = announceFallback
            ? $"{mode} is unavailable. Using SAPI. {text}" : text;
        if (SpeakSapiOnWorker(sapiText, interrupt))
        {
            _lastFallbackNoticeMode = mode;
            return true;
        }
        if (detected == null)
            return false;
        string readerText = announceFallback
            ? $"{mode} and SAPI are unavailable. Using {detected}. {text}" : text;
        if (!TryTolkAsLastResort(readerText, interrupt))
            return false;
        _lastFallbackNoticeMode = mode;
        return true;
    }

    private bool TryTolkAsLastResort(string text, bool interrupt)
    {
        if (!TolkNative.Tolk_Output(text, interrupt))
            return false;
        _lastOutputBackend = OutputBackend.Tolk;
        return true;
    }

    private bool SpeakSapiOnWorker(string text, bool interrupt)
    {
        try
        {
            if (!EnsureSapiOnWorker())
                return false;
            dynamic voice = _sapiComVoice!;
            int pitch;
            lock (_speechLock)
                pitch = _sapiPitch;
            int mappedPitch = (int)Math.Round((pitch - 50) / 5.0);
            string xml = $"<pitch absmiddle=\"{mappedPitch:+0;-0;0}\">" +
                SecurityElement.Escape(text) + "</pitch>";
            // SVSFlagsAsync | SVSFIsXML, plus SVSFPurgeBeforeSpeak when
            // this message should interrupt an earlier SAPI utterance.
            voice.Speak(xml, interrupt ? 11 : 9);
            _lastOutputBackend = OutputBackend.Sapi;
            return true;
        }
        catch (Exception ex)
        {
            LogSapiError("SAPI output failed", ex);
            ReleaseSapiOnWorker();
            return false;
        }
    }

    private bool EnsureSapiOnWorker()
    {
        string voiceId;
        int volume;
        int rate;
        int version;
        lock (_speechLock)
        {
            voiceId = _sapiVoiceId;
            volume = _sapiVolume;
            rate = _sapiRate;
            version = _sapiSettingsVersion;
        }
        // A fresh SpVoice starts with the system default. Recreate it when
        // switching voices so selecting System default genuinely resets it.
        if (_sapiComVoice != null &&
            !string.Equals(voiceId, _sapiAppliedVoiceId, StringComparison.OrdinalIgnoreCase))
            ReleaseSapiOnWorker();

        if (_sapiComVoice == null)
        {
            Type? type = Type.GetTypeFromProgID("SAPI.SpVoice");
            if (type == null)
                return false;
            _sapiComVoice = Activator.CreateInstance(type);
            _sapiAppliedSettingsVersion = -1;
        }
        if (_sapiComVoice == null)
            return false;

        if (version == _sapiAppliedSettingsVersion)
            return true;

        dynamic voice = _sapiComVoice;
        if (!string.IsNullOrEmpty(voiceId))
        {
            try
            {
                dynamic tokens = voice.GetVoices();
                bool found = false;
                for (int i = 0; i < (int)tokens.Count; i++)
                {
                    dynamic token = tokens.Item(i);
                    if (!string.Equals((string)token.Id, voiceId,
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                    voice.Voice = token;
                    found = true;
                    break;
                }
                if (!found)
                    WriteStatus("Selected SAPI voice token is unavailable; using current voice.");
            }
            catch (Exception ex)
            {
                LogSapiError("Could not select SAPI voice", ex);
            }
        }
        voice.Volume = volume;
        voice.Rate = (int)Math.Round((rate - 50) / 5.0);
        _sapiAppliedVoiceId = voiceId;
        _sapiAppliedSettingsVersion = version;
        return true;
    }

    private bool SilenceOutputOnWorker()
    {
        try
        {
            if (_lastOutputBackend == OutputBackend.Sapi && _sapiComVoice != null)
            {
                dynamic voice = _sapiComVoice;
                voice.Speak(string.Empty, 3); // asynchronous, purge previous speech
                return true;
            }
            if (_lastOutputBackend == OutputBackend.Tolk)
                return TolkNative.Tolk_Silence();
            if (_lastOutputBackend == OutputBackend.Nvda)
                return NvdaNative.nvdaController_cancelSpeech() == 0;
        }
        catch (Exception ex)
        {
            LogSapiError("Could not silence speech", ex);
        }
        return false;
    }

    private void ReleaseSapiOnWorker()
    {
        if (_sapiComVoice == null)
            return;
        try
        {
            if (Marshal.IsComObject(_sapiComVoice))
                Marshal.FinalReleaseComObject(_sapiComVoice);
        }
        catch (Exception ex)
        {
            LogSapiError("Could not release SAPI voice", ex);
        }
        finally
        {
            _sapiComVoice = null;
            _sapiAppliedSettingsVersion = -1;
            _sapiAppliedVoiceId = string.Empty;
            if (_lastOutputBackend == OutputBackend.Sapi)
                _lastOutputBackend = OutputBackend.None;
        }
    }

    private void LogSapiError(string message, Exception ex)
    {
        long now = Environment.TickCount64;
        if (now < _nextSapiErrorLogAt)
            return;
        _nextSapiErrorLogAt = now + 5000;
        WriteStatus(message + ": " + ex);
        MelonLoader.MelonLogger.Warning(message + ": " + ex.Message);
    }

    private static class NvdaNative
    {
        private const string LibraryName = "nvdaControllerClient64.dll";

        [DllImport(LibraryName, CallingConvention = CallingConvention.StdCall,
            ExactSpelling = true)]
        internal static extern int nvdaController_testIfRunning();

        [DllImport(LibraryName, CallingConvention = CallingConvention.StdCall,
            ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int nvdaController_speakText(
            [MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport(LibraryName, CallingConvention = CallingConvention.StdCall,
            ExactSpelling = true)]
        internal static extern int nvdaController_cancelSpeech();
    }
}
