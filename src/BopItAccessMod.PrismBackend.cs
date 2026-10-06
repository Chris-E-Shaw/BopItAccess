namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private static readonly Dictionary<string, ulong> PrismModeIds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SAPI"] = PrismNative.BackendIds.Sapi,
            ["OneCore"] = PrismNative.BackendIds.OneCore,
            ["NVDA"] = PrismNative.BackendIds.Nvda,
            ["JAWS"] = PrismNative.BackendIds.Jaws,
            ["UI Automation"] = PrismNative.BackendIds.Uia,
            ["ZDSR"] = PrismNative.BackendIds.Zdsr,
            ["ZoomText"] = PrismNative.BackendIds.ZoomText,
            ["Boy PC Reader"] = PrismNative.BackendIds.BoyPcReader,
            ["PC Talker"] = PrismNative.BackendIds.PcTalker,
            ["Sense Reader"] = PrismNative.BackendIds.SenseReader,
            ["System Access"] = PrismNative.BackendIds.SystemAccess,
            ["Window-Eyes"] = PrismNative.BackendIds.WindowEyes
        };

    // All handles in this section belong to the one speech worker. Prism
    // backend instances are not safe to call from concurrent threads.
    private IntPtr _prismContext;
    private IntPtr _prismSpeechBackend;
    private ulong _prismSpeechBackendId;
    private IntPtr _prismBrailleBackend;
    private string _prismSelectedMode = string.Empty;
    private long _nextPrismSelectionAt;
    private long _nextPrismBrailleSelectionAt;
    private long _nextPrismErrorLogAt;
    private long _nextSlowPrismQueueLogAt;
    private int _prismAppliedSapiSettingsVersion = -1;
    private int _prismAppliedOneCoreSettingsVersion = -1;
    private int _prismAttemptedSapiSettingsVersion = -1;
    private int _prismAttemptedOneCoreSettingsVersion = -1;
    private long _nextPrismSapiSettingsAttemptAt;
    private long _nextPrismOneCoreSettingsAttemptAt;
    private nuint _prismDefaultSapiVoice;
    private bool _prismDefaultSapiVoiceKnown;
    private nuint _prismDefaultOneCoreVoice;
    private bool _prismDefaultOneCoreVoiceKnown;
    private PrismSpeechControlSnapshot? _prismSpeechControlSnapshot;
    private int _prismSpeechControlRevision;
    private readonly List<ulong> _prismBrailleCandidates = new();
    private readonly List<ulong> _prismReaderCandidates = new();

    private void InitializePrismOnWorker()
    {
        // A failed startup can be retried on this worker. Do not accumulate
        // duplicate registry entries across attempts.
        _prismBrailleCandidates.Clear();
        _prismReaderCandidates.Clear();
        try
        {
            _prismContext = PrismNative.CreateContext();
            if (_prismContext == IntPtr.Zero)
                throw new InvalidOperationException("Prism could not create a context.");

            string version = PrismNative.VersionString();
            WriteStatus("Prism initialized: " + version + ".");
            MelonLoader.MelonLogger.Msg("Prism initialized: " + version + ".");
            nuint count = PrismNative.RegistryCount(_prismContext);
            if (count > 1024)
                throw new InvalidOperationException("Prism reported an invalid backend count.");
            for (nuint i = 0; i < count; i++)
            {
                ulong id = PrismNative.RegistryIdAt(_prismContext, i);
                if (id != PrismNative.BackendIds.Invalid)
                {
                    _prismBrailleCandidates.Add(id);
                    if (id != PrismNative.BackendIds.Sapi &&
                        id != PrismNative.BackendIds.OneCore &&
                        id != PrismNative.BackendIds.Uia)
                        _prismReaderCandidates.Add(id);
                }
            }
            _prismBrailleCandidates.Sort((left, right) =>
                PrismNative.RegistryPriority(_prismContext, right).CompareTo(
                    PrismNative.RegistryPriority(_prismContext, left)));
            _prismReaderCandidates.Sort((left, right) =>
                PrismNative.RegistryPriority(_prismContext, right).CompareTo(
                    PrismNative.RegistryPriority(_prismContext, left)));
            WriteStatus("Prism registered " + count + " output backends.");
        }
        catch
        {
            // A retry must never inherit a half-initialized context.
            try { ShutdownPrismOnWorker(); }
            catch (Exception ex)
            { WriteStatus("Prism cleanup after failed initialization failed: " + ex); }
            _prismBrailleCandidates.Clear();
            _prismReaderCandidates.Clear();
            throw;
        }
    }

    private void PreparePrismBackendOnWorker()
    {
        if (_prismContext == IntPtr.Zero || _shutdownRequested.IsSet)
            return;
        string mode;
        lock (_speechLock)
            mode = _outputMode;
        ResolvePrismSpeechBackendOnWorker(mode, force: true);
    }

    private bool OutputPrismOnWorker(string text, bool interrupt)
    {
        if (_prismContext == IntPtr.Zero)
            return false;
        string mode;
        lock (_speechLock)
            mode = _outputMode;

        if (!ResolvePrismSpeechBackendOnWorker(mode) ||
            _prismSpeechBackend == IntPtr.Zero)
            return false;

        bool fallback = !string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase) &&
            PrismModeIds.TryGetValue(mode, out ulong requestedId) &&
            _prismSpeechBackendId != requestedId;
        string dispatchedText = fallback && _lastFallbackNoticeMode != mode
            ? LF("{0} is unavailable. Using {1}. {2}", L(mode),
                L(PrismSpeechBackendNameOnWorker()), text)
            : text;

        if (_prismSpeechBackendId == PrismNative.BackendIds.Sapi)
            ApplyPrismSapiSettingsOnWorker();
        else if (_prismSpeechBackendId == PrismNative.BackendIds.OneCore)
            ApplyPrismOneCoreSettingsOnWorker();

        PrismNative.Error result = PrismNative.Speak(_prismSpeechBackend,
            dispatchedText, interrupt);
        if (result != PrismNative.Error.Ok)
        {
            ulong failedId = _prismSpeechBackendId;
            LogPrismErrorOnWorker("Speech through " +
                PrismSpeechBackendNameOnWorker() + " failed: " +
                PrismNative.ErrorString(result));
            ReleasePrismSpeechBackendOnWorker();
            // A reader can close while a menu is open. Re-evaluate Prism's
            // priority list immediately and make one fallback attempt.
            if (!AcquireAutomaticPrismBackendOnWorker(mode, failedId) ||
                _prismSpeechBackend == IntPtr.Zero)
                return false;
            if (_prismSpeechBackendId == PrismNative.BackendIds.Sapi)
                ApplyPrismSapiSettingsOnWorker();
            else if (_prismSpeechBackendId == PrismNative.BackendIds.OneCore)
                ApplyPrismOneCoreSettingsOnWorker();
            fallback = !string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase) &&
                PrismModeIds.TryGetValue(mode, out requestedId) &&
                _prismSpeechBackendId != requestedId;
            dispatchedText = fallback && _lastFallbackNoticeMode != mode
                ? LF("{0} is unavailable. Using {1}. {2}", L(mode),
                    L(PrismSpeechBackendNameOnWorker()), text)
                : text;
            result = PrismNative.Speak(_prismSpeechBackend, dispatchedText, interrupt);
            if (result != PrismNative.Error.Ok)
            {
                LogPrismErrorOnWorker("Fallback speech failed: " +
                    PrismNative.ErrorString(result));
                ReleasePrismSpeechBackendOnWorker();
                return false;
            }
        }

        _lastFallbackNoticeMode = fallback ? mode : null;
        SendPrismBrailleOnWorker(dispatchedText);
        return true;
    }

    private bool ResolvePrismSpeechBackendOnWorker(string mode, bool force = false)
    {
        long now = Environment.TickCount64;
        if (!force && _prismSpeechBackend != IntPtr.Zero &&
            string.Equals(_prismSelectedMode, mode, StringComparison.OrdinalIgnoreCase) &&
            now < _nextPrismSelectionAt)
            return true;

        IntPtr candidate = IntPtr.Zero;
        ulong candidateId = PrismNative.BackendIds.Invalid;
        if (!string.Equals(mode, "Auto", StringComparison.OrdinalIgnoreCase) &&
            PrismModeIds.TryGetValue(mode, out ulong preferredId))
        {
            candidate = AcquirePrismBackendOnWorker(preferredId);
            if (candidate != IntPtr.Zero)
                candidateId = preferredId;
        }
        if (candidate == IntPtr.Zero)
        {
            candidate = AcquireAutomaticPrismCandidateOnWorker(out candidateId);
        }
        bool selectedModeChanged = !string.Equals(_prismSelectedMode, mode,
            StringComparison.OrdinalIgnoreCase);
        _prismSelectedMode = mode;
        _nextPrismSelectionAt = now + 1000;
        if (candidate == IntPtr.Zero)
        {
            ReleasePrismSpeechBackendOnWorker();
            LogPrismErrorOnWorker("No Prism speech backend is currently available.");
            return false;
        }

        if (_prismSpeechBackend != IntPtr.Zero &&
            _prismSpeechBackendId == candidateId)
        {
            PrismNative.FreeBackend(candidate);
            if (selectedModeChanged)
                PublishPrismSpeechControlsOnWorker();
            return true;
        }

        ReleasePrismSpeechBackendOnWorker();
        _prismSpeechBackend = candidate;
        _prismSpeechBackendId = candidateId;
        _prismAppliedSapiSettingsVersion = -1;
        _prismAppliedOneCoreSettingsVersion = -1;
        _prismAttemptedSapiSettingsVersion = -1;
        _prismAttemptedOneCoreSettingsVersion = -1;
        CaptureDefaultPrismVoiceOnWorker(candidate, candidateId);
        PublishPrismSpeechControlsOnWorker();
        WriteStatus("Prism active speech backend: " + PrismSpeechBackendNameOnWorker() +
            "; selected output mode: " + mode + ".");
        return true;
    }

    private bool AcquireAutomaticPrismBackendOnWorker(string mode,
        ulong excludedId = PrismNative.BackendIds.Invalid)
    {
        IntPtr candidate = AcquireAutomaticPrismCandidateOnWorker(out ulong id,
            excludedId);
        if (candidate == IntPtr.Zero)
            return false;
        _prismSpeechBackend = candidate;
        _prismSpeechBackendId = id;
        _prismSelectedMode = mode;
        _nextPrismSelectionAt = Environment.TickCount64 + 1000;
        _prismAppliedSapiSettingsVersion = -1;
        _prismAppliedOneCoreSettingsVersion = -1;
        _prismAttemptedSapiSettingsVersion = -1;
        _prismAttemptedOneCoreSettingsVersion = -1;
        CaptureDefaultPrismVoiceOnWorker(candidate, id);
        PublishPrismSpeechControlsOnWorker();
        return true;
    }

    private IntPtr AcquireAutomaticPrismCandidateOnWorker(out ulong id,
        ulong excludedId = PrismNative.BackendIds.Invalid)
    {
        // Prefer a running reader, then Prism's preferred Windows voice
        // engine. SAPI remains available if OneCore cannot be initialized.
        foreach (ulong readerId in _prismReaderCandidates)
        {
            if (readerId == excludedId)
                continue;
            IntPtr reader = AcquirePrismBackendOnWorker(readerId);
            if (reader == IntPtr.Zero)
                continue;
            id = readerId;
            return reader;
        }
        foreach (ulong fallbackId in new[]
                 { PrismNative.BackendIds.OneCore, PrismNative.BackendIds.Sapi })
        {
            if (fallbackId == excludedId)
                continue;
            IntPtr fallback = AcquirePrismBackendOnWorker(fallbackId);
            if (fallback == IntPtr.Zero)
                continue;
            id = fallbackId;
            return fallback;
        }
        // Some platforms have other native providers. Probe them explicitly
        // so a cached, departed reader is never returned by AcquireBest.
        foreach (ulong otherId in _prismBrailleCandidates)
        {
            if (otherId == excludedId ||
                _prismReaderCandidates.Contains(otherId) ||
                otherId == PrismNative.BackendIds.Sapi ||
                otherId == PrismNative.BackendIds.OneCore)
                continue;
            IntPtr other = AcquirePrismBackendOnWorker(otherId);
            if (other == IntPtr.Zero)
                continue;
            id = otherId;
            return other;
        }
        id = PrismNative.BackendIds.Invalid;
        return IntPtr.Zero;
    }

    private void CaptureDefaultPrismVoiceOnWorker(IntPtr backend, ulong id)
    {
        if ((id != PrismNative.BackendIds.Sapi || _prismDefaultSapiVoiceKnown) &&
            (id != PrismNative.BackendIds.OneCore || _prismDefaultOneCoreVoiceKnown))
            return;
        if ((PrismNative.GetFeatures(backend) &
             PrismNative.BackendFeature.SupportsGetVoice) == 0 ||
            PrismNative.GetVoice(backend, out nuint voice) != PrismNative.Error.Ok)
            return;
        if (id == PrismNative.BackendIds.Sapi)
        {
            _prismDefaultSapiVoice = voice;
            _prismDefaultSapiVoiceKnown = true;
        }
        else if (id == PrismNative.BackendIds.OneCore)
        {
            _prismDefaultOneCoreVoice = voice;
            _prismDefaultOneCoreVoiceKnown = true;
        }
    }

    private sealed record PrismSpeechControlSnapshot(string SelectedMode,
        ulong BackendId, PrismNative.BackendFeature Features,
        SpeechVoiceOption[] OneCoreVoices, int Revision);

    private void PublishPrismSpeechControlsOnWorker()
    {
        PrismNative.BackendFeature features = _prismSpeechBackend == IntPtr.Zero
            ? 0 : PrismNative.GetFeatures(_prismSpeechBackend);
        SpeechVoiceOption[] voices = Array.Empty<SpeechVoiceOption>();
        if (_prismSpeechBackendId == PrismNative.BackendIds.OneCore &&
            (features & (PrismNative.BackendFeature.SupportsCountVoices |
                         PrismNative.BackendFeature.SupportsGetVoiceName)) ==
            (PrismNative.BackendFeature.SupportsCountVoices |
             PrismNative.BackendFeature.SupportsGetVoiceName) &&
            PrismNative.CountVoices(_prismSpeechBackend, out nuint count) ==
                PrismNative.Error.Ok)
        {
            List<SpeechVoiceOption> found = new() { new(string.Empty, "System default") };
            for (nuint index = 0; index < count; index++)
            {
                if (PrismNative.GetVoiceName(_prismSpeechBackend, index,
                        out string? name) != PrismNative.Error.Ok ||
                    string.IsNullOrWhiteSpace(name))
                    continue;
                string language = PrismNative.GetVoiceLanguage(_prismSpeechBackend,
                    index, out string? languageName) == PrismNative.Error.Ok
                    ? languageName ?? string.Empty : string.Empty;
                string identity = OneCoreVoiceIdentity(name, language);
                string display = string.IsNullOrWhiteSpace(language)
                    ? name : name + " (" + language + ")";
                found.Add(new(identity, display));
            }
            voices = found.ToArray();
        }
        lock (_speechLock)
            _prismSpeechControlSnapshot = new PrismSpeechControlSnapshot(
                _prismSelectedMode, _prismSpeechBackendId, features, voices,
                ++_prismSpeechControlRevision);
    }

    private static string OneCoreVoiceIdentity(string name, string language) =>
        Uri.EscapeDataString(name.Trim()) + "|" +
        Uri.EscapeDataString(language.Trim());

    private IntPtr AcquirePrismBackendOnWorker(ulong id)
    {
        if (!PrismNative.RegistryExists(_prismContext, id))
            return IntPtr.Zero;
        IntPtr backend = PrismNative.Acquire(_prismContext, id);
        if (backend == IntPtr.Zero)
            return IntPtr.Zero;
        try
        {
            PrismNative.BackendFeature features = PrismNative.GetFeatures(backend);
            if ((features & PrismNative.BackendFeature.SupportsSpeak) == 0 ||
                (features & PrismNative.BackendFeature.IsSupportedAtRuntime) == 0)
                return IntPtr.Zero;
            PrismNative.Error initialized = PrismNative.InitializeBackend(backend);
            features = PrismNative.GetFeatures(backend);
            if ((initialized != PrismNative.Error.Ok &&
                 initialized != PrismNative.Error.AlreadyInitialized) ||
                (features & PrismNative.BackendFeature.IsSupportedAtRuntime) == 0 ||
                (features & PrismNative.BackendFeature.SupportsSpeak) == 0)
                return IntPtr.Zero;
            IntPtr selected = backend;
            backend = IntPtr.Zero;
            return selected;
        }
        finally
        {
            if (backend != IntPtr.Zero)
                PrismNative.FreeBackend(backend);
        }
    }

    private string PrismSpeechBackendNameOnWorker() =>
        PrismNative.BackendName(_prismSpeechBackend) ?? "another output";

    private void ApplyPrismSapiSettingsOnWorker()
    {
        int version;
        int volume;
        int rate;
        int pitch;
        string voiceId;
        lock (_speechLock)
        {
            version = _sapiSettingsVersion;
            volume = _sapiVolume;
            rate = _sapiRate;
            pitch = _sapiPitch;
            voiceId = _sapiVoiceId;
        }
        if (_prismAppliedSapiSettingsVersion == version)
            return;
        long now = Environment.TickCount64;
        if (_prismAttemptedSapiSettingsVersion == version &&
            now < _nextPrismSapiSettingsAttemptAt)
            return;
        _prismAttemptedSapiSettingsVersion = version;
        _nextPrismSapiSettingsAttemptAt = now + 5000;

        bool applied = true;
        PrismNative.BackendFeature features = PrismNative.GetFeatures(_prismSpeechBackend);
        if ((features & PrismNative.BackendFeature.SupportsSetVoice) != 0)
        {
            if (string.IsNullOrEmpty(voiceId))
            {
                if (_prismDefaultSapiVoiceKnown)
                    applied &= LogPrismSettingError("voice", PrismNative.SetVoice(
                        _prismSpeechBackend, _prismDefaultSapiVoice));
            }
            else
            {
                string? selectedName = _sapiVoices.FirstOrDefault(voice =>
                    string.Equals(voice.Id, voiceId, StringComparison.OrdinalIgnoreCase))?.Name;
                bool matched = false;
                if (!string.IsNullOrEmpty(selectedName) &&
                    PrismNative.CountVoices(_prismSpeechBackend, out nuint count) ==
                        PrismNative.Error.Ok)
                {
                    for (nuint index = 0; index < count; index++)
                    {
                        if (PrismNative.GetVoiceName(_prismSpeechBackend, index,
                                out string? name) != PrismNative.Error.Ok ||
                            !string.Equals(name?.Trim(), selectedName.Trim(),
                                StringComparison.OrdinalIgnoreCase))
                            continue;
                        applied &= LogPrismSettingError("voice", PrismNative.SetVoice(
                            _prismSpeechBackend, index));
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                    LogPrismErrorOnWorker("The saved SAPI voice is not in Prism's voice list; using the current voice.");
            }
        }
        if ((features & PrismNative.BackendFeature.SupportsSetVolume) != 0)
            applied &= LogPrismSettingError("volume", PrismNative.SetVolume(
                _prismSpeechBackend, volume / 100f));
        if ((features & PrismNative.BackendFeature.SupportsSetRate) != 0)
            applied &= LogPrismSettingError("rate", PrismNative.SetRate(
                _prismSpeechBackend, MapLegacySapiScale(rate)));
        if ((features & PrismNative.BackendFeature.SupportsSetPitch) != 0)
            applied &= LogPrismSettingError("pitch", PrismNative.SetPitch(
                _prismSpeechBackend, MapLegacySapiScale(pitch)));
        if (applied)
            _prismAppliedSapiSettingsVersion = version;
    }

    private void ApplyPrismOneCoreSettingsOnWorker()
    {
        int version;
        int volume;
        int rate;
        int pitch;
        string voiceId;
        lock (_speechLock)
        {
            version = _oneCoreSettingsVersion;
            volume = _oneCoreVolume;
            rate = _oneCoreRate;
            pitch = _oneCorePitch;
            voiceId = _oneCoreVoiceId;
        }
        if (_prismAppliedOneCoreSettingsVersion == version)
            return;
        long now = Environment.TickCount64;
        if (_prismAttemptedOneCoreSettingsVersion == version &&
            now < _nextPrismOneCoreSettingsAttemptAt)
            return;
        _prismAttemptedOneCoreSettingsVersion = version;
        _nextPrismOneCoreSettingsAttemptAt = now + 5000;

        bool applied = true;
        PrismNative.BackendFeature features = PrismNative.GetFeatures(_prismSpeechBackend);
        if ((features & PrismNative.BackendFeature.SupportsSetVoice) != 0)
        {
            if (string.IsNullOrEmpty(voiceId))
            {
                if (_prismDefaultOneCoreVoiceKnown)
                    applied &= LogPrismSettingError("OneCore", "voice", PrismNative.SetVoice(
                        _prismSpeechBackend, _prismDefaultOneCoreVoice));
            }
            else if ((features & (PrismNative.BackendFeature.SupportsCountVoices |
                                  PrismNative.BackendFeature.SupportsGetVoiceName)) ==
                     (PrismNative.BackendFeature.SupportsCountVoices |
                      PrismNative.BackendFeature.SupportsGetVoiceName) &&
                     PrismNative.CountVoices(_prismSpeechBackend, out nuint count) ==
                         PrismNative.Error.Ok)
            {
                bool matched = false;
                for (nuint index = 0; index < count; index++)
                {
                    if (PrismNative.GetVoiceName(_prismSpeechBackend, index,
                            out string? name) != PrismNative.Error.Ok ||
                        string.IsNullOrWhiteSpace(name))
                        continue;
                    string language = PrismNative.GetVoiceLanguage(_prismSpeechBackend,
                        index, out string? languageName) == PrismNative.Error.Ok
                        ? languageName ?? string.Empty : string.Empty;
                    if (!string.Equals(OneCoreVoiceIdentity(name, language), voiceId,
                            StringComparison.OrdinalIgnoreCase))
                        continue;
                    applied &= LogPrismSettingError("OneCore", "voice", PrismNative.SetVoice(
                        _prismSpeechBackend, index));
                    matched = true;
                    break;
                }
                if (!matched)
                {
                    LogPrismErrorOnWorker("The saved OneCore voice is no longer available; using the system default.");
                    if (_prismDefaultOneCoreVoiceKnown)
                        applied &= LogPrismSettingError("OneCore", "default voice",
                            PrismNative.SetVoice(_prismSpeechBackend,
                                _prismDefaultOneCoreVoice));
                }
            }
        }
        if ((features & PrismNative.BackendFeature.SupportsSetVolume) != 0)
            applied &= LogPrismSettingError("OneCore", "volume", PrismNative.SetVolume(
                _prismSpeechBackend, volume / 100f));
        if ((features & PrismNative.BackendFeature.SupportsSetRate) != 0)
            applied &= LogPrismSettingError("OneCore", "rate", PrismNative.SetRate(
                _prismSpeechBackend, rate / 100f));
        if ((features & PrismNative.BackendFeature.SupportsSetPitch) != 0)
            applied &= LogPrismSettingError("OneCore", "pitch", PrismNative.SetPitch(
                _prismSpeechBackend, pitch / 100f));
        if (applied)
            _prismAppliedOneCoreSettingsVersion = version;
    }

    private static float MapLegacySapiScale(int value)
    {
        int legacyStep = (int)Math.Round((value - 50) / 5.0);
        if (legacyStep <= -10)
            return 0f;
        if (legacyStep >= 10)
            return 1f;
        if (legacyStep == 0)
            return 0.5f;
        // Prism's SAPI backend truncates its mapped float to an integer.
        // Stay just inside the requested step despite float rounding.
        return 0.5f + (legacyStep + Math.Sign(legacyStep) * 0.01f) / 20f;
    }

    private bool LogPrismSettingError(string setting, PrismNative.Error error)
        => LogPrismSettingError("SAPI", setting, error);

    private bool LogPrismSettingError(string backend, string setting,
        PrismNative.Error error)
    {
        if (error == PrismNative.Error.Ok)
            return true;
        LogPrismErrorOnWorker("Could not apply Prism " + backend + " " + setting + ": " +
            PrismNative.ErrorString(error));
        return false;
    }

    private void SendPrismBrailleOnWorker(string text)
    {
        if (!_brailleOutputEnabled || _speechSuppressedForBackground ||
            _prismSpeechBackend == IntPtr.Zero)
            return;
        try
        {
            IntPtr target = _prismSpeechBackend;
            if ((PrismNative.GetFeatures(target) &
                 PrismNative.BackendFeature.SupportsBraille) == 0)
                target = FindPrismBrailleBackendOnWorker();
            if (target == IntPtr.Zero)
                return;
            PrismNative.Error result = PrismNative.Braille(target, text);
            if (result != PrismNative.Error.Ok)
            {
                LogPrismErrorOnWorker("Prism braille output failed: " +
                    PrismNative.ErrorString(result));
                if (target == _prismBrailleBackend)
                {
                    ReleasePrismBrailleBackendOnWorker();
                    _nextPrismBrailleSelectionAt = Environment.TickCount64 + 1000;
                }
            }
        }
        catch (Exception ex)
        {
            LogPrismErrorOnWorker("Prism braille dispatch failed: " + ex.Message);
            ReleasePrismBrailleBackendOnWorker();
            _nextPrismBrailleSelectionAt = Environment.TickCount64 + 1000;
        }
    }

    private IntPtr FindPrismBrailleBackendOnWorker()
    {
        long now = Environment.TickCount64;
        if (_prismBrailleBackend != IntPtr.Zero)
            return _prismBrailleBackend;
        if (now < _nextPrismBrailleSelectionAt)
            return IntPtr.Zero;
        // A full registry probe can be costly on a system with no running
        // braille-capable reader. Recheck periodically without holding up
        // every short SAPI menu announcement.
        _nextPrismBrailleSelectionAt = now + 3000;
        foreach (ulong id in _prismBrailleCandidates)
        {
            if (id == _prismSpeechBackendId)
                continue;
            IntPtr candidate = PrismNative.Acquire(_prismContext, id);
            if (candidate == IntPtr.Zero)
                continue;
            try
            {
                PrismNative.BackendFeature features = PrismNative.GetFeatures(candidate);
                if ((features & PrismNative.BackendFeature.SupportsBraille) == 0 ||
                    (features & PrismNative.BackendFeature.IsSupportedAtRuntime) == 0)
                    continue;
                PrismNative.Error initialized = PrismNative.InitializeBackend(candidate);
                features = PrismNative.GetFeatures(candidate);
                if ((initialized == PrismNative.Error.Ok ||
                     initialized == PrismNative.Error.AlreadyInitialized) &&
                    (features & PrismNative.BackendFeature.IsSupportedAtRuntime) != 0 &&
                    (features & PrismNative.BackendFeature.SupportsBraille) != 0)
                {
                    _prismBrailleBackend = candidate;
                    candidate = IntPtr.Zero;
                    WriteStatus("Prism braille backend: " +
                        (PrismNative.BackendName(_prismBrailleBackend) ?? "unknown") + ".");
                    return _prismBrailleBackend;
                }
            }
            finally
            {
                if (candidate != IntPtr.Zero)
                    PrismNative.FreeBackend(candidate);
            }
        }
        return IntPtr.Zero;
    }

    private bool SilencePrismOnWorker()
    {
        if (_prismSpeechBackend == IntPtr.Zero)
            return false;
        try
        {
            if (PrismNative.Stop(_prismSpeechBackend) == PrismNative.Error.Ok)
                return true;
            // A backend without Stop may still accept an interrupting empty
            // utterance. This is best effort for the new Prism providers.
            return PrismNative.Speak(_prismSpeechBackend, string.Empty,
                interrupt: true) == PrismNative.Error.Ok;
        }
        catch (Exception ex)
        {
            LogPrismErrorOnWorker("Could not stop Prism speech: " + ex.Message);
            return false;
        }
    }

    private void ReleasePrismSpeechBackendOnWorker()
    {
        if (_prismSpeechBackend == IntPtr.Zero)
            return;
        try
        {
            PrismNative.Stop(_prismSpeechBackend);
        }
        catch (Exception ex)
        {
            LogPrismErrorOnWorker("Could not stop Prism speech backend: " + ex.Message);
        }
        finally
        {
            try { PrismNative.FreeBackend(_prismSpeechBackend); }
            catch (Exception ex)
            { LogPrismErrorOnWorker("Could not free Prism speech backend: " + ex.Message); }
            finally
            {
                _prismSpeechBackend = IntPtr.Zero;
                _prismSpeechBackendId = PrismNative.BackendIds.Invalid;
                _prismAppliedSapiSettingsVersion = -1;
                _prismAppliedOneCoreSettingsVersion = -1;
                _prismAttemptedSapiSettingsVersion = -1;
                _prismAttemptedOneCoreSettingsVersion = -1;
                PublishPrismSpeechControlsOnWorker();
            }
        }
    }

    private void ReleasePrismBrailleBackendOnWorker()
    {
        if (_prismBrailleBackend == IntPtr.Zero)
            return;
        try { PrismNative.FreeBackend(_prismBrailleBackend); }
        catch (Exception ex)
        { LogPrismErrorOnWorker("Could not release Prism braille backend: " + ex.Message); }
        finally
        {
            _prismBrailleBackend = IntPtr.Zero;
        }
    }

    private void ShutdownPrismOnWorker()
    {
        ReleasePrismBrailleBackendOnWorker();
        ReleasePrismSpeechBackendOnWorker();
        if (_prismContext == IntPtr.Zero)
            return;
        try
        {
            PrismNative.Shutdown(_prismContext);
            WriteStatus("Prism shut down.");
        }
        catch (Exception ex)
        {
            WriteStatus("Prism shutdown failed: " + ex.Message);
        }
        finally { _prismContext = IntPtr.Zero; }
    }

    private void LogPrismQueueDelayOnWorker(long queuedAt, long dispatchStartedAt)
    {
        if (queuedAt <= 0)
            return;
        long delay = dispatchStartedAt - queuedAt;
        if (delay < 150 || dispatchStartedAt < _nextSlowPrismQueueLogAt)
            return;
        _nextSlowPrismQueueLogAt = Environment.TickCount64 + 3000;
        WriteStatus("Slow Prism queue: announcement waited " + delay +
            " ms before dispatch.");
    }

    private void LogPrismErrorOnWorker(string message)
    {
        long now = Environment.TickCount64;
        if (now < _nextPrismErrorLogAt)
            return;
        _nextPrismErrorLogAt = now + 5000;
        WriteStatus(message);
        MelonLoader.MelonLogger.Warning(message);
    }
}
