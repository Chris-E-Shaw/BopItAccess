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
    private const string SapiTrimSilencePreferenceKey = "BopItAccess.SapiTrimSilence";
    private const string LegacyRepeatButtonHintsPreferenceKey =
        "BopItAccess.RepeatButtonHintsSeconds";
    private const string ReadButtonHintsPreferenceKey = "BopItAccess.ReadButtonHints";
    private const string MuteSpeechInBackgroundPreferenceKey =
        "BopItAccess.MuteSpeechInBackground";
    private const string ButtonHintsDelayPreferenceKey =
        "BopItAccess.ButtonHintsDelaySeconds";
    private const string RepeatButtonHintsCountPreferenceKey =
        "BopItAccess.RepeatButtonHintsCount";
    private const string RepeatButtonHintsIntervalPreferenceKey =
        "BopItAccess.RepeatButtonHintsIntervalSeconds";
    // Retain the experimental capture/trim path for future work, but keep it
    // unavailable and inactive in this release.
    private static readonly bool TrimSilenceExperimentAvailable = false;
    private static readonly string[] OutputModes =
    {
        "Auto", "SAPI", "JAWS", "Window-Eyes", "NVDA", "System Access", "ZoomText"
    };

    private string _outputMode = "Auto";
    private string _sapiVoiceId = string.Empty;
    private int _sapiVolume = 100;
    private int _sapiRate = 50;
    private int _sapiPitch = 50;
    private bool _trimSilence;
    private bool _readButtonHintsEnabled = true;
    private bool _muteSpeechInBackground;
    // Zero includes hints in ordinary menu announcements. Timed hints use
    // one of the positive delay options instead.
    private int _buttonHintsDelaySeconds;
    // Zero means no repeats; -1 repeats indefinitely.
    private int _repeatButtonHintsCount;
    private int _repeatButtonHintsIntervalSeconds = 15;
    private List<SpeechVoiceOption> _sapiVoices = new();
    private int _sapiSettingsVersion;
    // Incremented by the main thread when an interrupting request supersedes
    // an utterance that may still be rendering into an in-memory SAPI stream.
    private long _sapiRenderSerial;

    // These objects are owned and touched only by the existing STA speech
    // worker. Unity/PlayerPrefs and the menu stay on the main thread.
    private object? _sapiComVoice;
    private object? _sapiCaptureVoice;
    private readonly List<(int Number, object Stream)> _sapiPlaybackStreams = new();
    private int _sapiAppliedSettingsVersion = -1;
    private string _sapiAppliedVoiceId = string.Empty;
    private int _sapiCaptureAppliedSettingsVersion = -1;
    private string _sapiCaptureAppliedVoiceId = string.Empty;
    private OutputBackend _lastOutputBackend;
    private string? _lastFallbackNoticeMode;
    private long _nextSapiErrorLogAt;
    private bool? _lastSeparateBrailleDispatchAccepted;

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
            _readButtonHintsEnabled = PlayerPrefs.GetInt(ReadButtonHintsPreferenceKey, 1) != 0;
            _muteSpeechInBackground =
                PlayerPrefs.GetInt(MuteSpeechInBackgroundPreferenceKey, 0) != 0;
            // Preserve the delay existing users selected in version 0.6.2.
            // A new delay preference takes priority after they change it.
            int savedHintsDelay = PlayerPrefs.HasKey(ButtonHintsDelayPreferenceKey)
                ? PlayerPrefs.GetInt(ButtonHintsDelayPreferenceKey, 0)
                : PlayerPrefs.GetInt(LegacyRepeatButtonHintsPreferenceKey, 0);
            _buttonHintsDelaySeconds = savedHintsDelay is 0 or 5 or 10 or 15 or 30 or 60
                ? savedHintsDelay : 0;
            int savedRepeatCount = PlayerPrefs.GetInt(RepeatButtonHintsCountPreferenceKey, 0);
            _repeatButtonHintsCount = savedRepeatCount is 0 or 2 or 3 or 4 or 5 or -1
                ? savedRepeatCount : 0;
            int savedRepeatInterval = PlayerPrefs.GetInt(RepeatButtonHintsIntervalPreferenceKey, 15);
            _repeatButtonHintsIntervalSeconds = savedRepeatInterval is 15 or 30 or 45 or 60
                ? savedRepeatInterval : 15;
            int savedTrimSilence = PlayerPrefs.GetInt(SapiTrimSilencePreferenceKey, 0);
            _trimSilence = TrimSilenceExperimentAvailable && savedTrimSilence != 0;
            if (!TrimSilenceExperimentAvailable && savedTrimSilence != 0)
            {
                PlayerPrefs.SetInt(SapiTrimSilencePreferenceKey, 0);
                PlayerPrefs.Save();
            }
        }
        catch (Exception ex)
        {
            WriteStatus("Could not read speech output preferences: " + ex.Message);
        }

        RefreshSapiVoices();
        WriteStatus($"Output mode: {_outputMode}; SAPI voice: " +
            (string.IsNullOrEmpty(_sapiVoiceId) ? "system default" : _sapiVoiceId) +
            $"; volume {_sapiVolume}, rate {_sapiRate}, pitch {_sapiPitch}; " +
            "auto-speak button hints " + (_readButtonHintsEnabled ? "on" : "off") +
            "; mute speech in background " +
            (_muteSpeechInBackground ? "on" : "off") +
            "; button hints delay " + ButtonHintsDelayValue(_buttonHintsDelaySeconds) +
            "; repeat button hints " + RepeatButtonHintsValue(_repeatButtonHintsCount) +
            "; repeat interval " + _repeatButtonHintsIntervalSeconds + " seconds." +
            (TrimSilenceExperimentAvailable
                ? $" Trim silence {(_trimSilence ? "on" : "off")}."
                : string.Empty));
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
            _sapiRenderSerial++;
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

    private void SetTrimSilenceFromMenu(bool enabled)
    {
        if (!TrimSilenceExperimentAvailable)
            return;
        if (_trimSilence == enabled)
            return;
        lock (_speechLock)
            _trimSilence = enabled;
        try
        {
            PlayerPrefs.SetInt(SapiTrimSilencePreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save SAPI trim silence setting: " + ex.Message);
        }
        WriteStatus("SAPI trim silence " + (enabled ? "enabled" : "disabled") + ".");
    }

    private void SetReadButtonHintsFromMenu(bool enabled)
    {
        if (_readButtonHintsEnabled == enabled)
            return;
        _readButtonHintsEnabled = enabled;
        SaveButtonHintsPreference(ReadButtonHintsPreferenceKey, enabled ? 1 : 0);
        WriteStatus("Auto-speak button hints " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    private void SetMuteSpeechInBackgroundFromMenu(bool enabled)
    {
        if (_muteSpeechInBackground == enabled)
            return;
        _muteSpeechInBackground = enabled;
        SaveButtonHintsPreference(MuteSpeechInBackgroundPreferenceKey,
            enabled ? 1 : 0);
        UpdateBackgroundSpeechFocus();
        WriteStatus("Mute speech in background " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    private void SetButtonHintsDelayFromMenu(int seconds)
    {
        if (seconds is not (0 or 5 or 10 or 15 or 30 or 60) ||
            _buttonHintsDelaySeconds == seconds)
            return;
        _buttonHintsDelaySeconds = seconds;
        SaveButtonHintsPreference(ButtonHintsDelayPreferenceKey, seconds);
        WriteStatus("Button hints delay changed to " + ButtonHintsDelayValue(seconds) + ".");
    }

    private void SetRepeatButtonHintsFromMenu(int count)
    {
        if (count is not (0 or 2 or 3 or 4 or 5 or -1) ||
            _repeatButtonHintsCount == count)
            return;
        _repeatButtonHintsCount = count;
        SaveButtonHintsPreference(RepeatButtonHintsCountPreferenceKey, count);
        WriteStatus("Repeat button hints changed to " + RepeatButtonHintsValue(count) + ".");
    }

    private void SetRepeatButtonHintsIntervalFromMenu(int seconds)
    {
        if (seconds is not (15 or 30 or 45 or 60) ||
            _repeatButtonHintsIntervalSeconds == seconds)
            return;
        _repeatButtonHintsIntervalSeconds = seconds;
        SaveButtonHintsPreference(RepeatButtonHintsIntervalPreferenceKey, seconds);
        WriteStatus("Repeat button hints interval changed to " + seconds + " seconds.");
    }

    private static void SaveButtonHintsPreference(string key, int value)
    {
        try
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save " + key + ": " + ex.Message);
        }
    }

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

    private bool OutputSpeechOnWorker(string text, bool interrupt,
        bool protectedCapture = false)
    {
        string mode;
        lock (_speechLock)
        {
            if (_speechSuppressedForBackground)
                return false;
            mode = _outputMode;
        }

        string? detected = Marshal.PtrToStringUni(TolkNative.Tolk_DetectScreenReader());
        if (string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase))
        {
            if (detected != null && OutputTolkOnWorker(text, interrupt))
            {
                _lastOutputBackend = OutputBackend.Tolk;
                return true;
            }
            return SpeakSapiOnWorker(text, interrupt, protectedCapture) ||
                TryTolkAsLastResort(text, interrupt);
        }

        if (string.Equals(mode, "SAPI", StringComparison.OrdinalIgnoreCase))
        {
            if (SpeakSapiOnWorker(text, interrupt, protectedCapture))
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
                        SendBrailleForSeparateSpeechOnWorker(text);
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
            if (OutputTolkOnWorker(text, interrupt))
            {
                _lastOutputBackend = OutputBackend.Tolk;
                _lastFallbackNoticeMode = null;
                return true;
            }
        }

        bool announceFallback = _lastFallbackNoticeMode != mode;
        string sapiText = announceFallback
            ? $"{mode} is unavailable. Using SAPI. {text}" : text;
        if (SpeakSapiOnWorker(sapiText, interrupt, protectedCapture))
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
        if (!OutputTolkOnWorker(text, interrupt))
            return false;
        _lastOutputBackend = OutputBackend.Tolk;
        return true;
    }

    private bool OutputTolkOnWorker(string text, bool interrupt) =>
        _brailleOutputEnabled
            ? TolkNative.Tolk_Output(text, interrupt)
            : TolkNative.Tolk_Speak(text, interrupt);

    // Tolk_Output already routes text to both speech and braille. Direct SAPI
    // and NVDA speech bypass that API, so these paths need one braille call.
    private void SendBrailleForSeparateSpeechOnWorker(string text)
    {
        if (!_brailleOutputEnabled || _speechSuppressedForBackground)
            return;
        try
        {
            if (!TolkNative.Tolk_IsLoaded() || !TolkNative.Tolk_HasBraille())
            {
                if (_lastSeparateBrailleDispatchAccepted != false)
                    WriteStatus("Tolk braille driver is unavailable for separate speech output.");
                _lastSeparateBrailleDispatchAccepted = false;
                return;
            }
            bool accepted = TolkNative.Tolk_Braille(text);
            if (_lastSeparateBrailleDispatchAccepted != accepted)
                WriteStatus("Tolk braille dispatch for separate speech output " +
                    (accepted ? "accepted" : "rejected") + ".");
            _lastSeparateBrailleDispatchAccepted = accepted;
        }
        catch (Exception ex)
        {
            if (_lastSeparateBrailleDispatchAccepted != false)
                WriteStatus("Tolk braille dispatch failed: " + ex.Message);
            _lastSeparateBrailleDispatchAccepted = false;
        }
    }

    private bool SpeakSapiOnWorker(string text, bool interrupt,
        bool protectedCapture)
    {
        try
        {
            if (!EnsureSapiOnWorker())
                return false;
            dynamic voice = _sapiComVoice!;
            int pitch;
            bool trimSilence;
            long generation;
            long renderSerial;
            lock (_speechLock)
            {
                pitch = _sapiPitch;
                trimSilence = _trimSilence;
                generation = _speechGeneration;
                renderSerial = _sapiRenderSerial;
            }
            int mappedPitch = (int)Math.Round((pitch - 50) / 5.0);
            string xml = $"<pitch absmiddle=\"{mappedPitch:+0;-0;0}\">" +
                SecurityElement.Escape(text) + "</pitch>";

            if (trimSilence)
            {
                try
                {
                    TrimmedSpeechResult result = SpeakTrimmedSapiOnWorker(xml,
                        interrupt, generation, renderSerial, protectedCapture);
                    if (result != TrimmedSpeechResult.Failed)
                    {
                        if (result == TrimmedSpeechResult.Played)
                            SendBrailleForSeparateSpeechOnWorker(text);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogSapiError("SAPI in-memory silence trimming failed; using direct output", ex);
                    ReleaseSapiCaptureOnWorker();
                }

                // A capture failure must not make the menu or score silent.
                if (SapiCaptureIsStale(generation, renderSerial, protectedCapture))
                    return true;
            }

            // SVSFlagsAsync | SVSFIsXML, plus SVSFPurgeBeforeSpeak when
            // this message should interrupt an earlier SAPI utterance.
            voice.Speak(xml, interrupt ? 11 : 9);
            ReleaseCompletedSapiPlaybackStreamsOnWorker();
            _lastOutputBackend = OutputBackend.Sapi;
            SendBrailleForSeparateSpeechOnWorker(text);
            return true;
        }
        catch (Exception ex)
        {
            LogSapiError("SAPI output failed", ex);
            ReleaseSapiOnWorker();
            return false;
        }
    }

    private enum TrimmedSpeechResult { Played, Cancelled, Failed }

    private TrimmedSpeechResult SpeakTrimmedSapiOnWorker(string xml,
        bool interrupt, long generation, long renderSerial,
        bool protectedCapture)
    {
        if (!EnsureSapiCaptureOnWorker())
            return TrimmedSpeechResult.Failed;

        object? captureFormat = null;
        object? captureStream = null;
        object? playbackStream = null;
        try
        {
            Type? formatType = Type.GetTypeFromProgID("SAPI.SpAudioFormat");
            Type? streamType = Type.GetTypeFromProgID("SAPI.SpMemoryStream");
            if (formatType == null || streamType == null)
                return TrimmedSpeechResult.Failed;

            captureFormat = Activator.CreateInstance(formatType);
            captureStream = Activator.CreateInstance(streamType);
            if (captureFormat == null || captureStream == null)
                return TrimmedSpeechResult.Failed;

            dynamic format = captureFormat;
            dynamic stream = captureStream;
            dynamic captureVoice = _sapiCaptureVoice!;
            // SAFT22kHz16BitMono is 22. Fix the output format before assigning
            // the stream so the byte array is known to be signed 16-bit PCM.
            format.Type = 22;
            stream.Format = format;
            captureVoice.AllowAudioOutputFormatChangesOnNextSet = false;
            captureVoice.AudioOutputStream = stream;
            // Render asynchronously to keep the worker responsive to a newer
            // interrupt, speech-off command, or game start while SAPI renders.
            captureVoice.Speak(xml, 9); // asynchronous, XML
            long startedAt = Environment.TickCount64;
            while (!(bool)captureVoice.WaitUntilDone(20))
            {
                if (SapiCaptureIsStale(generation, renderSerial, protectedCapture))
                {
                    PurgeSapiCaptureOnWorker();
                    return TrimmedSpeechResult.Cancelled;
                }
                if (Environment.TickCount64 - startedAt > 15000)
                {
                    PurgeSapiCaptureOnWorker();
                    WriteStatus("SAPI in-memory capture exceeded 15 seconds; using direct output.");
                    return TrimmedSpeechResult.Failed;
                }
            }
            if (SapiCaptureIsStale(generation, renderSerial, protectedCapture))
                return TrimmedSpeechResult.Cancelled;

            dynamic actualFormat = stream.Format;
            if ((int)actualFormat.Type != 22)
            {
                WriteStatus("SAPI changed the capture format; using direct output.");
                return TrimmedSpeechResult.Failed;
            }

            if (stream.GetData() is not byte[] pcm || pcm.Length < 2 ||
                (pcm.Length & 1) != 0 ||
                (pcm.Length >= 4 && pcm[0] == (byte)'R' &&
                 pcm[1] == (byte)'I' && pcm[2] == (byte)'F' &&
                 pcm[3] == (byte)'F'))
            {
                WriteStatus("SAPI in-memory capture returned unsupported PCM; using direct output.");
                return TrimmedSpeechResult.Failed;
            }

            byte[]? trimmed = TrimSapiPcmSilence(pcm);
            if (trimmed == null)
            {
                WriteStatus("SAPI in-memory capture contained no audible PCM; using direct output.");
                return TrimmedSpeechResult.Failed;
            }
            playbackStream = Activator.CreateInstance(streamType);
            if (playbackStream == null)
                return TrimmedSpeechResult.Failed;
            dynamic playback = playbackStream;
            playback.Format = format;
            playback.SetData(trimmed); // also rewinds the stream for playback
            if (SapiCaptureIsStale(generation, renderSerial, protectedCapture))
                return TrimmedSpeechResult.Cancelled;

            dynamic outputVoice = _sapiComVoice!;
            // SpeakStream uses SAPI's audio device and existing queue. Purge
            // on interrupt; otherwise append behind the current utterance.
            ReleaseCompletedSapiPlaybackStreamsOnWorker();
            int configuredRate = (int)outputVoice.Rate;
            int streamNumber;
            try
            {
                // The capture voice already applied the requested rate while
                // synthesizing PCM. Keep playback neutral so SpeakStream does
                // not apply the same rate adjustment a second time.
                outputVoice.Rate = 0;
                streamNumber = (int)outputVoice.SpeakStream(playback,
                    interrupt ? 3 : 1);
            }
            finally
            {
                try
                {
                    outputVoice.Rate = configuredRate;
                }
                catch (Exception ex)
                {
                    _sapiAppliedSettingsVersion = -1;
                    LogSapiError("Could not restore SAPI playback rate", ex);
                }
            }
            // SAPI plays this asynchronously. Keep the managed COM wrapper
            // alive until the voice has advanced past this stream or stopped.
            _sapiPlaybackStreams.Add((streamNumber, playbackStream));
            playbackStream = null;
            ReleaseCompletedSapiPlaybackStreamsOnWorker();
            _lastOutputBackend = OutputBackend.Sapi;
            if (protectedCapture)
                ExtendGameOverScoreProtectionForSapiPlayback(trimmed.Length);
            return TrimmedSpeechResult.Played;
        }
        finally
        {
            if (_sapiCaptureVoice != null)
            {
                try
                {
                    dynamic captureVoice = _sapiCaptureVoice;
                    captureVoice.AudioOutputStream = null;
                }
                catch (Exception ex)
                {
                    LogSapiError("Could not detach SAPI capture stream", ex);
                    ReleaseSapiCaptureOnWorker();
                }
            }
            ReleaseTemporarySapiComObject(playbackStream);
            ReleaseTemporarySapiComObject(captureStream);
            ReleaseTemporarySapiComObject(captureFormat);
        }
    }

    private bool SapiCaptureIsStale(long generation, long renderSerial,
        bool protectedCapture)
    {
        lock (_speechLock)
            return _shutdownRequested.IsSet || generation != _speechGeneration ||
                (!protectedCapture && renderSerial != _sapiRenderSerial);
    }

    private static byte[]? TrimSapiPcmSilence(byte[] pcm)
    {
        const int silenceThreshold = 128; // about -48 dBFS
        const int samplesPerSecond = 22050;
        const int paddingSamples = samplesPerSecond / 50; // preserve 20 ms
        int first = -1;
        int last = -1;
        int sampleCount = pcm.Length / 2;
        for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            int offset = sampleIndex * 2;
            short amplitude = (short)(pcm[offset] | pcm[offset + 1] << 8);
            if (Math.Abs((int)amplitude) < silenceThreshold)
                continue;
            if (first < 0)
                first = sampleIndex;
            last = sampleIndex;
        }
        if (first < 0)
            return null; // uncertain capture: use direct SAPI output

        int startSample = Math.Max(0, first - paddingSamples);
        int endSample = Math.Min(sampleCount, last + paddingSamples + 1);
        if (startSample == 0 && endSample == sampleCount)
            return pcm;
        byte[] trimmed = new byte[(endSample - startSample) * 2];
        Buffer.BlockCopy(pcm, startSample * 2, trimmed, 0, trimmed.Length);
        return trimmed;
    }

    private bool EnsureSapiCaptureOnWorker()
    {
        string voiceId;
        int rate;
        int version;
        lock (_speechLock)
        {
            voiceId = _sapiVoiceId;
            rate = _sapiRate;
            version = _sapiSettingsVersion;
        }
        if (_sapiCaptureVoice != null &&
            !string.Equals(voiceId, _sapiCaptureAppliedVoiceId,
                StringComparison.OrdinalIgnoreCase))
            ReleaseSapiCaptureOnWorker();

        if (_sapiCaptureVoice == null)
        {
            Type? type = Type.GetTypeFromProgID("SAPI.SpVoice");
            if (type == null)
                return false;
            _sapiCaptureVoice = Activator.CreateInstance(type);
            _sapiCaptureAppliedSettingsVersion = -1;
        }
        if (_sapiCaptureVoice == null)
            return false;
        if (version == _sapiCaptureAppliedSettingsVersion)
            return true;

        dynamic voice = _sapiCaptureVoice;
        if (!string.IsNullOrEmpty(voiceId))
        {
            try
            {
                dynamic tokens = voice.GetVoices();
                for (int i = 0; i < (int)tokens.Count; i++)
                {
                    dynamic token = tokens.Item(i);
                    if (!string.Equals((string)token.Id, voiceId,
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                    voice.Voice = token;
                    break;
                }
            }
            catch (Exception ex)
            {
                LogSapiError("Could not select SAPI capture voice", ex);
            }
        }
        // Render at full volume. The output voice applies the user's volume
        // during SpeakStream, avoiding attenuation twice.
        voice.Volume = 100;
        voice.Rate = (int)Math.Round((rate - 50) / 5.0);
        _sapiCaptureAppliedVoiceId = voiceId;
        _sapiCaptureAppliedSettingsVersion = version;
        return true;
    }

    private void ReleaseSapiCaptureOnWorker()
    {
        object? capture = _sapiCaptureVoice;
        _sapiCaptureVoice = null;
        _sapiCaptureAppliedSettingsVersion = -1;
        _sapiCaptureAppliedVoiceId = string.Empty;
        ReleaseTemporarySapiComObject(capture);
    }

    private void PurgeSapiCaptureOnWorker()
    {
        if (_sapiCaptureVoice == null)
            return;
        try
        {
            dynamic captureVoice = _sapiCaptureVoice;
            captureVoice.Speak(string.Empty, 3); // asynchronous purge
            // Let the capture engine stop writing before its memory stream is
            // detached and released by the caller's finally block.
            if (!(bool)captureVoice.WaitUntilDone(100))
                ReleaseSapiCaptureOnWorker();
        }
        catch (Exception ex)
        {
            LogSapiError("Could not purge SAPI capture", ex);
            ReleaseSapiCaptureOnWorker();
        }
    }

    private static void ReleaseTemporarySapiComObject(object? value)
    {
        try
        {
            if (value != null && Marshal.IsComObject(value))
                Marshal.FinalReleaseComObject(value);
        }
        catch (Exception ex)
        {
            WriteStatus("Could not release a temporary SAPI object: " + ex.Message);
        }
    }

    private void ReleaseCompletedSapiPlaybackStreamsOnWorker()
    {
        if (_sapiPlaybackStreams.Count == 0 || _sapiComVoice == null)
            return;
        try
        {
            dynamic voice = _sapiComVoice;
            if ((bool)voice.WaitUntilDone(0))
            {
                ReleaseAllSapiPlaybackStreamsOnWorker();
                return;
            }

            // A stream number lower than the one playing has finished. Keep
            // the current and all queued memory streams alive for SAPI.
            dynamic status = voice.Status;
            int current = (int)status.CurrentStreamNumber;
            if (current <= 0)
                return;
            for (int i = _sapiPlaybackStreams.Count - 1; i >= 0; i--)
            {
                if (_sapiPlaybackStreams[i].Number >= current)
                    continue;
                ReleaseTemporarySapiComObject(_sapiPlaybackStreams[i].Stream);
                _sapiPlaybackStreams.RemoveAt(i);
            }
        }
        catch (Exception ex)
        {
            // The audio remains usable even when a voice does not expose
            // status. Retain streams until a later poll or voice release.
            WriteStatus("Could not inspect SAPI playback progress: " + ex.Message);
        }
    }

    private void ReleaseAllSapiPlaybackStreamsOnWorker()
    {
        foreach (var item in _sapiPlaybackStreams)
            ReleaseTemporarySapiComObject(item.Stream);
        _sapiPlaybackStreams.Clear();
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
                ReleaseCompletedSapiPlaybackStreamsOnWorker();
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
        ReleaseSapiCaptureOnWorker();
        if (_sapiComVoice == null)
        {
            ReleaseAllSapiPlaybackStreamsOnWorker();
            return;
        }
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
            ReleaseAllSapiPlaybackStreamsOnWorker();
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
