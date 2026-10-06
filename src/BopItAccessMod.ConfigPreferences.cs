using System.Globalization;
using Il2Cpp;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // The editable INI is an interface to the existing preferences rather
    // than a second set of settings. Import on Unity's thread before the
    // ordinary startup readers; menu changes keep using their established
    // save paths, and export reads those same preferences.
    private static readonly (string ConfigKey, string Preference, bool Default)[]
        ConfigModToggleOptions =
    {
        ("Mod.SpeechOutput", SpeechEnabledPreferenceKey, true),
        ("Mod.BrailleOutput", BrailleOutputPreferenceKey, true),
        ("Mod.MuteSpeechInBackground", MuteSpeechInBackgroundPreferenceKey, false),
        ("Mod.SpeakMenuIndexes", IndexingPreferenceKey, true),
        ("Mod.FormatSpeech", FilterCapitalisationPreferenceKey, true),
        ("Mod.ReadControlTypes", ReadControlTypesPreferenceKey, true),
        ("Mod.SliderRanges", SliderRangesPreferenceKey, false),
        ("Mod.OneOnOneFeedback", OneOnOneFeedbackPreferenceKey, true),
        ("Mod.MuteAudioInBackground", BackgroundAudioPreference, false),
        ("Mod.WelcomeDismissed", WelcomeDismissedPreferenceKey, false),
        ("Hints.AutoSpeak", ReadButtonHintsPreferenceKey, true)
    };

    private static readonly (string ConfigKey, string Preference, string DefaultPath,
        string ActionName, int Index)[] ConfigModBindingOptions =
    {
        ("ModBindings.ReadDescriptions.Keyboard", DescriptionKeyboardKey,
            "<Keyboard>/g", "ReadDescriptions", 0),
        ("ModBindings.ReadDescriptions.Controller", DescriptionGamepadKey,
            "<Gamepad>/leftTrigger", "ReadDescriptions", 1),
        ("ModBindings.ReadScore.Keyboard", ScoreKeyboardKey,
            "<Keyboard>/t", "ReadScore", 0),
        ("ModBindings.ReadScore.Controller", ScoreGamepadKey,
            "<Gamepad>/leftStickPress", "ReadScore", 1),
        ("ModBindings.ToggleSpeech.Keyboard", ToggleSpeechKeyboardKey,
            "<Keyboard>/f8", "ToggleSpeech", 0),
        ("ModBindings.ToggleSpeech.Controller", ToggleSpeechGamepadKey,
            "<Gamepad>/select", "ToggleSpeech", 1),
        ("ModBindings.SpeakHints.Keyboard", SpeakHintsKeyboardKey,
            "<Keyboard>/h", "SpeakHints", 0),
        ("ModBindings.SpeakHints.Controller", SpeakHintsGamepadKey,
            "<Gamepad>/rightStickPress", "SpeakHints", 1),
        ("ModBindings.ChangeSpeechOutput.Keyboard", ChangeSpeechOutputKeyboardKey,
            "<Keyboard>/f9", "ChangeSpeechOutput", 0),
        ("ModBindings.ChangeSpeechOutput.Controller", ChangeSpeechOutputGamepadKey,
            "<Gamepad>/buttonWest", "ChangeSpeechOutput", 1)
    };

    private readonly Dictionary<string, string> _pendingModConfigBindings =
        new(StringComparer.OrdinalIgnoreCase);
    private string[] _configOneCoreVoiceChoices = Array.Empty<string>();
    private volatile bool _configVoiceChoicesReady;

    private bool HasPendingModConfigBindings => _pendingModConfigBindings.Count != 0;
    private bool ConfigVoiceChoicesReady => _configVoiceChoicesReady;

    private void ApplyModConfig(IReadOnlyDictionary<string, string> values)
    {
        foreach (var option in ConfigModToggleOptions)
        {
            if (!ConfigModTryValue(values, option.ConfigKey, out string value))
                continue;
            if (ConfigModTryBool(value, out bool enabled))
                PlayerPrefs.SetInt(option.Preference, enabled ? 1 : 0);
            else
                ConfigModInvalid(option.ConfigKey, "On or Off");
        }

        ConfigModApplyChoice(values, "Mod.OutputMode", OutputModePreferenceKey,
            OutputModes, automaticAlias: "Auto");
        ConfigModApplyChoice(values, "Hints.Type", HintsTypePreferenceKey,
            HintsTypes, automaticAlias: "Automatic");
        ConfigModApplyChoiceInteger(values, "Mod.LimitFps", FpsLimitPreference,
            FpsLimitChoices, "Unlimited", -1);
        ConfigModApplyChoiceInteger(values, "Hints.DelaySeconds",
            ButtonHintsDelayPreferenceKey, new[] { 0, 5, 10, 15, 30, 60 }, "None", 0);
        ConfigModApplyRepeatCount(values);
        ConfigModApplyChoiceInteger(values, "Hints.RepeatIntervalSeconds",
            RepeatButtonHintsIntervalPreferenceKey, new[] { 15, 30, 45, 60 });
        ConfigModApplyProfile(values, "OneCore", OneCoreVoicePreferenceKey,
            OneCoreVolumePreferenceKey, OneCoreRatePreferenceKey, OneCorePitchPreferenceKey);
        ConfigModApplyProfile(values, "SAPI", SapiVoicePreferenceKey,
            SapiVolumePreferenceKey, SapiRatePreferenceKey, SapiPitchPreferenceKey);

        foreach (var option in ConfigModBindingOptions)
        {
            if (!ConfigModTryValue(values, option.ConfigKey, out string value))
                continue;
            string? path = ConfigModBindingPath(value, option.Index == 1,
                option.DefaultPath);
            if (path == null)
            {
                ConfigModInvalid(option.ConfigKey,
                    "a keyboard key or controller button; Default restores its original input");
                continue;
            }
            _pendingModConfigBindings[option.ConfigKey] = path;
        }

        PlayerPrefs.Save();
        // These two readers can run before Settings.Load completes. Ensure
        // the first post-import frame uses the newly imported preferences.
        _fpsLimitInitialized = false;
        _backgroundAudioPreferenceLoaded = false;
        WriteStatus("Editable configuration imported mod preferences without menu speech.");
    }

    // Native overrides must be imported first. The established guard checks
    // those live bindings, every mod action, and reserved default paths. No
    // new menu announcements are generated by configuration import.
    private void ApplyModConfigBindings(InputRebindingManager manager)
    {
        bool changed = false;
        foreach (var option in ConfigModBindingOptions)
        {
            if (!_pendingModConfigBindings.TryGetValue(option.ConfigKey, out string? path))
                continue;
            InputAction action = ConfigModAction(option.ActionName);
            if (IsBindingAssignedElsewhere(manager, action, option.Index, path,
                    out string owner) ||
                ConfigModHasLayoutConflict(manager, action, option.Index, path, out owner))
            {
                WriteStatus("Editable configuration rejected " + option.ConfigKey +
                    ": that input is already assigned to " + owner + ".");
                _pendingModConfigBindings.Remove(option.ConfigKey);
                continue;
            }

            bool wasEnabled = action.enabled;
            try
            {
                if (wasEnabled)
                    action.Disable();
                InputActionRebindingExtensions.ApplyBindingOverride(action, option.Index, path);
                PlayerPrefs.SetString(option.Preference, path);
                changed = true;
                _pendingModConfigBindings.Remove(option.ConfigKey);
            }
            finally
            {
                if (wasEnabled)
                    action.Enable();
            }
        }
        if (!changed)
            return;
        PlayerPrefs.Save();
        InputRebindingEvents.RefreshPrompts?.Invoke();
        _cachedButtonHintContext = null;
        _nextButtonHintContextProbeAt = 0;
        _nextAssignedButtonHintControlsRefreshAt = 0;
        WriteStatus("Editable configuration applied validated mod input bindings.");
    }

    private InputAction ConfigModAction(string name) => name switch
    {
        "ReadDescriptions" => EnsureDescriptionAction(),
        "ReadScore" => EnsureScoreAction(),
        "ToggleSpeech" => EnsureToggleSpeechAction(),
        "SpeakHints" => EnsureSpeakHintsAction(),
        "ChangeSpeechOutput" => EnsureChangeSpeechOutputAction(),
        _ => throw new ArgumentException("Unknown mod input action.", nameof(name))
    };

    // The ordinary guard resolves aliases through connected devices. Also
    // compare generic/subclass Gamepad paths without requiring the player's
    // controller to be plugged in while editing this recovery file.
    private bool ConfigModHasLayoutConflict(InputRebindingManager manager,
        InputAction target, int targetIndex, string candidate, out string owner)
    {
        owner = string.Empty;
        List<InputAction> actions = new();
        InputActionAsset? native = manager.playerInput?.actions ?? manager.inputActions;
        if (native != null)
            foreach (InputActionMap map in native.actionMaps)
                foreach (InputAction action in map.actions)
                    actions.Add(action);
        foreach (string actionName in ConfigModBindingOptions.Select(option =>
                     option.ActionName).Distinct())
            actions.Add(ConfigModAction(actionName));
        string normalizedCandidate = ConfigModConflictPath(candidate);
        foreach (InputAction action in actions)
        {
            for (int index = 0; index < action.bindings.Count; index++)
            {
                if (action.id == target.id && index == targetIndex)
                    continue;
                InputBinding binding = action.bindings[index];
                if (binding.isComposite || BindingIsModifierChordPart(action, index))
                    continue;
                if (!normalizedCandidate.Equals(ConfigModConflictPath(binding.effectivePath),
                        StringComparison.OrdinalIgnoreCase) &&
                    !normalizedCandidate.Equals(ConfigModConflictPath(binding.path),
                        StringComparison.OrdinalIgnoreCase))
                    continue;
                owner = SpokenBindingOwner(action.name);
                return true;
            }
        }
        return false;
    }

    private static string ConfigModConflictPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
        string? layout = HintDeviceLayout(path);
        if (layout == null)
            return path;
        bool gamepad = layout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase);
        if (!gamepad)
        {
            try { gamepad = InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad"); }
            catch { }
        }
        if (!gamepad)
            return path;
        string leaf = path[(path.IndexOf('>') + 2)..];
        leaf = leaf.ToLowerInvariant() switch
        {
            "select" => "selectButton", "start" => "startButton", _ => leaf
        };
        return "<Gamepad>/" + leaf;
    }

    private Dictionary<string, string> CaptureModConfig()
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (var option in ConfigModToggleOptions)
            result[option.ConfigKey] = PlayerPrefs.GetInt(option.Preference,
                option.Default ? 1 : 0) != 0 ? "On" : "Off";
        result["Mod.OutputMode"] = ConfigModReadChoice(OutputModePreferenceKey,
            OutputModes, "Auto");
        result["Hints.Type"] = ConfigModReadChoice(HintsTypePreferenceKey,
            HintsTypes, "Automatic");
        int fps = PlayerPrefs.GetInt(FpsLimitPreference, 60);
        if (!FpsLimitChoices.Contains(fps))
            fps = 60;
        result["Mod.LimitFps"] = fps == -1 ? "Unlimited" : ConfigModNumber(fps);
        int delay = PlayerPrefs.HasKey(ButtonHintsDelayPreferenceKey)
            ? PlayerPrefs.GetInt(ButtonHintsDelayPreferenceKey, 10)
            : PlayerPrefs.GetInt(LegacyRepeatButtonHintsPreferenceKey, 10);
        if (delay is not (0 or 5 or 10 or 15 or 30 or 60))
            delay = 10;
        result["Hints.DelaySeconds"] = delay == 0 ? "None" : ConfigModNumber(delay);
        int repeats = PlayerPrefs.GetInt(RepeatButtonHintsCountPreferenceKey, -1);
        if (repeats is not (-1 or 0 or 2 or 3 or 4 or 5))
            repeats = -1;
        result["Hints.RepeatCount"] = repeats switch
        {
            -1 => "Infinitely", 0 => "Off", _ => ConfigModNumber(repeats) + "x"
        };
        int interval = PlayerPrefs.GetInt(RepeatButtonHintsIntervalPreferenceKey, 30);
        result["Hints.RepeatIntervalSeconds"] = ConfigModNumber(
            interval is 15 or 30 or 45 or 60 ? interval : 30);
        ConfigModCaptureProfile(result, "OneCore", OneCoreVoicePreferenceKey,
            OneCoreVolumePreferenceKey, OneCoreRatePreferenceKey, OneCorePitchPreferenceKey);
        ConfigModCaptureProfile(result, "SAPI", SapiVoicePreferenceKey,
            SapiVolumePreferenceKey, SapiRatePreferenceKey, SapiPitchPreferenceKey);
        foreach (var option in ConfigModBindingOptions)
        {
            string path = PlayerPrefs.GetString(option.Preference, string.Empty);
            result[option.ConfigKey] = string.IsNullOrWhiteSpace(path)
                ? option.DefaultPath : path;
        }
        return result;
    }

    private static Dictionary<string, string> CaptureModConfigDescriptions()
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (var option in ConfigModToggleOptions)
            result[option.ConfigKey] = "On or Off.";
        result["Mod.WelcomeDismissed"] =
            "Off shows the welcome screen next launch. On means it has been dismissed.";
        result["Mod.OutputMode"] = "Available: " + string.Join(", ", OutputModes) +
            ". Auto prefers a running screen reader, then OneCore, then SAPI.";
        result["Mod.LimitFps"] = "Available: 30, 60, 120, 240, Unlimited.";
        result["Hints.Type"] = "Available: Automatic, Keyboard, Controller, Both.";
        result["Hints.AutoSpeak"] = "On or Off. Speak Hints remains available on demand.";
        result["Hints.DelaySeconds"] = "Available: None, 5, 10, 15, 30, 60. Values are seconds.";
        result["Hints.RepeatCount"] =
            "Available: Off, 2x, 3x, 4x, 5x, Infinitely. Counts total hint readings.";
        result["Hints.RepeatIntervalSeconds"] = "Available: 15, 30, 45, 60. Values are seconds.";
        foreach (string engine in new[] { "OneCore", "SAPI" })
        {
            result[engine + ".Voice"] = engine == "OneCore"
                ? "System default resets the voice. Or copy a listed name | language entry."
                : "System default resets the voice. Or copy a listed voice name or its full registry ID.";
            result[engine + ".Volume"] = "5 to 100 percent. The minimum preserves audible speech.";
            result[engine + ".Rate"] = "0 to 100; 50 is the default.";
            result[engine + ".Pitch"] = "0 to 100; 50 is the default.";
        }
        foreach (var option in ConfigModBindingOptions)
            result[option.ConfigKey] = "Input path; Default restores " + option.DefaultPath +
                ". Duplicate assignments are rejected; unbinding is not supported here.";
        return result;
    }

    private void ConfigModApplyProfile(IReadOnlyDictionary<string, string> values,
        string engine, string voiceKey, string volumeKey, string rateKey, string pitchKey)
    {
        ConfigModApplyRange(values, engine + ".Volume", volumeKey, 5, 100);
        ConfigModApplyRange(values, engine + ".Rate", rateKey, 0, 100);
        ConfigModApplyRange(values, engine + ".Pitch", pitchKey, 0, 100);
        if (!ConfigModTryValue(values, engine + ".Voice", out string voice))
            return;
        if (voice.Length == 0 || voice.Equals("System default", StringComparison.OrdinalIgnoreCase) ||
            voice.Equals("Default", StringComparison.OrdinalIgnoreCase))
        {
            PlayerPrefs.SetString(voiceKey, string.Empty);
            return;
        }
        if (engine == "OneCore")
        {
            int divider = voice.IndexOf('|');
            if (divider > 0 && divider == voice.LastIndexOf('|'))
            {
                string name = Uri.UnescapeDataString(voice[..divider].Trim());
                string language = Uri.UnescapeDataString(voice[(divider + 1)..].Trim());
                PlayerPrefs.SetString(voiceKey, OneCoreVoiceIdentity(name, language));
                return;
            }
            ConfigModInvalid(engine + ".Voice", "System default or a listed name | language entry");
            return;
        }
        // This existing enumeration reads only registry metadata, without
        // starting synthesis or touching the worker's COM objects.
        RefreshSapiVoices();
        SpeechVoiceOption[] matches = _sapiVoices.Where(option =>
            !string.IsNullOrEmpty(option.Id) &&
            (option.Id.Equals(voice, StringComparison.OrdinalIgnoreCase) ||
             option.Name.Equals(voice, StringComparison.OrdinalIgnoreCase))).ToArray();
        if (matches.Length == 1)
            PlayerPrefs.SetString(voiceKey, matches[0].Id);
        else
            ConfigModInvalid(engine + ".Voice", "System default or one uniquely matching installed voice");
    }

    private void ConfigModCaptureProfile(Dictionary<string, string> result, string engine,
        string voiceKey, string volumeKey, string rateKey, string pitchKey)
    {
        string voice = PlayerPrefs.GetString(voiceKey, string.Empty);
        if (voice.Length == 0)
            voice = "System default";
        else if (engine == "OneCore")
            voice = ConfigModReadableOneCoreVoice(voice);
        else
        {
            SpeechVoiceOption? match = _sapiVoices.Find(option => option.Id.Equals(voice,
                StringComparison.OrdinalIgnoreCase));
            // Preserve IDs if names are ambiguous, so an export/import cycle
            // always selects the same voice rather than a similarly named one.
            if (match != null && _sapiVoices.Count(option => option.Name.Equals(match.Name,
                    StringComparison.OrdinalIgnoreCase)) == 1)
                voice = match.Name;
        }
        result[engine + ".Voice"] = voice;
        result[engine + ".Volume"] = ConfigModNumber(Math.Clamp(PlayerPrefs.GetInt(volumeKey, 100), 5, 100));
        result[engine + ".Rate"] = ConfigModNumber(Math.Clamp(PlayerPrefs.GetInt(rateKey, 50), 0, 100));
        result[engine + ".Pitch"] = ConfigModNumber(Math.Clamp(PlayerPrefs.GetInt(pitchKey, 50), 0, 100));
    }

    private static string ConfigModReadableOneCoreVoice(string identity)
    {
        int divider = identity.IndexOf('|');
        return divider < 0 ? identity : Uri.UnescapeDataString(identity[..divider]) +
            " | " + Uri.UnescapeDataString(identity[(divider + 1)..]);
    }

    // Call once on the Prism worker after the first startup announcement.
    // An independent backend avoids altering the player's selected engine
    // while still listing OneCore voices when a screen reader is active.
    private void CaptureConfigVoiceChoicesOnWorker()
    {
        IntPtr backend = IntPtr.Zero;
        List<string> found = new();
        try
        {
            if (_prismContext == IntPtr.Zero ||
                !PrismNative.RegistryExists(_prismContext, PrismNative.BackendIds.OneCore))
                return;
            backend = PrismNative.Create(_prismContext, PrismNative.BackendIds.OneCore);
            if (backend == IntPtr.Zero)
                return;
            PrismNative.Error initialized = PrismNative.InitializeBackend(backend);
            if (initialized != PrismNative.Error.Ok &&
                initialized != PrismNative.Error.AlreadyInitialized)
                return;
            if (PrismNative.CountVoices(backend, out nuint count) != PrismNative.Error.Ok)
                return;
            for (nuint index = 0; index < count; index++)
            {
                if (PrismNative.GetVoiceName(backend, index, out string? name) !=
                        PrismNative.Error.Ok || string.IsNullOrWhiteSpace(name))
                    continue;
                string language = PrismNative.GetVoiceLanguage(backend, index,
                    out string? languageName) == PrismNative.Error.Ok
                    ? languageName ?? string.Empty : string.Empty;
                found.Add(name.Trim() + " | " + language.Trim());
            }
        }
        catch (Exception ex)
        {
            WriteStatus("Could not list OneCore voices in editable configuration: " + ex.Message);
        }
        finally
        {
            if (backend != IntPtr.Zero)
            {
                try { PrismNative.FreeBackend(backend); }
                catch (Exception ex)
                { WriteStatus("Could not release configuration voice enumeration: " + ex.Message); }
            }
            lock (_speechLock)
                _configOneCoreVoiceChoices = found.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            _configVoiceChoicesReady = true;
        }
    }

    private Dictionary<string, string[]> CaptureModConfigVoiceChoices()
    {
        string[] oneCore;
        lock (_speechLock)
            oneCore = _configOneCoreVoiceChoices.ToArray();
        return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["OneCore.Voice"] = new[] { "System default" }.Concat(oneCore).ToArray(),
            ["SAPI.Voice"] = new[] { "System default" }.Concat(_sapiVoices
                .Where(voice => !string.IsNullOrEmpty(voice.Id))
                .Select(voice => _sapiVoices.Count(other => other.Name.Equals(voice.Name,
                    StringComparison.OrdinalIgnoreCase)) == 1 ? voice.Name : voice.Id))
                .ToArray()
        };
    }

    private void ConfigModApplyChoice(IReadOnlyDictionary<string, string> values,
        string configKey, string preference, string[] choices, string automaticAlias)
    {
        if (!ConfigModTryValue(values, configKey, out string value))
            return;
        if (value.Equals("Automatic", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            value = automaticAlias;
        string? canonical = Array.Find(choices, option => option.Equals(value,
            StringComparison.OrdinalIgnoreCase));
        if (canonical != null)
            PlayerPrefs.SetString(preference, canonical);
        else
            ConfigModInvalid(configKey, string.Join(", ", choices));
    }

    private void ConfigModApplyChoiceInteger(IReadOnlyDictionary<string, string> values,
        string configKey, string preference, int[] choices, string? textAlias = null,
        int aliasValue = 0)
    {
        if (!ConfigModTryValue(values, configKey, out string value))
            return;
        if (textAlias != null && value.Equals(textAlias, StringComparison.OrdinalIgnoreCase))
            value = ConfigModNumber(aliasValue);
        if (ConfigModTryInteger(value, out int parsed) && choices.Contains(parsed))
            PlayerPrefs.SetInt(preference, parsed);
        else
            ConfigModInvalid(configKey, string.Join(", ", choices.Select(ConfigModNumber)) +
                (textAlias == null ? string.Empty : ", " + textAlias));
    }

    private void ConfigModApplyRepeatCount(IReadOnlyDictionary<string, string> values)
    {
        const string key = "Hints.RepeatCount";
        if (!ConfigModTryValue(values, key, out string value))
            return;
        if (value.Equals("Off", StringComparison.OrdinalIgnoreCase))
            value = "0";
        else if (value.Equals("Infinitely", StringComparison.OrdinalIgnoreCase) ||
                 value.Equals("Infinite", StringComparison.OrdinalIgnoreCase))
            value = "-1";
        else
            value = value.TrimEnd('x', 'X');
        if (ConfigModTryInteger(value, out int parsed) && parsed is -1 or 0 or 2 or 3 or 4 or 5)
            PlayerPrefs.SetInt(RepeatButtonHintsCountPreferenceKey, parsed);
        else
            ConfigModInvalid(key, "Off, 2x, 3x, 4x, 5x, Infinitely");
    }

    private void ConfigModApplyRange(IReadOnlyDictionary<string, string> values,
        string configKey, string preference, int minimum, int maximum)
    {
        if (!ConfigModTryValue(values, configKey, out string value))
            return;
        if (ConfigModTryInteger(value.TrimEnd('%').TrimEnd(), out int parsed) &&
            parsed >= minimum && parsed <= maximum)
            PlayerPrefs.SetInt(preference, parsed);
        else
            ConfigModInvalid(configKey, ConfigModNumber(minimum) + " to " + ConfigModNumber(maximum));
    }

    private void ConfigModInvalid(string key, string expected) =>
        WriteStatus("Editable configuration ignored " + key + "; expected " + expected +
            ". Previous setting preserved.");

    private static string ConfigModReadChoice(string preference, string[] choices,
        string defaultValue)
    {
        string value = PlayerPrefs.GetString(preference, defaultValue);
        return Array.Find(choices, option => option.Equals(value,
            StringComparison.OrdinalIgnoreCase)) ?? defaultValue;
    }

    private static bool ConfigModTryValue(IReadOnlyDictionary<string, string> values,
        string key, out string value)
    {
        if (values.TryGetValue(key, out string? found))
        {
            value = found.Trim();
            return true;
        }
        foreach (var entry in values)
        {
            if (!entry.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                continue;
            value = entry.Value.Trim();
            return true;
        }
        value = string.Empty;
        return false;
    }

    private static bool ConfigModTryBool(string value, out bool result)
    {
        result = value.Equals("On", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1" ||
            value.Equals("Yes", StringComparison.OrdinalIgnoreCase);
        return result || value.Equals("Off", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("False", StringComparison.OrdinalIgnoreCase) || value == "0" ||
            value.Equals("No", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ConfigModTryInteger(string value, out int result) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

    private static string ConfigModNumber(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string? ConfigModBindingPath(string value, bool controller, string defaultPath)
    {
        if (value.Equals("Default", StringComparison.OrdinalIgnoreCase))
            return defaultPath;
        if (value.Length == 0 || value.Any(char.IsControl) ||
            value.IndexOfAny(new[] { '*', '{', '}', ',', ';' }) >= 0)
            return null;
        string path = value;
        if (!path.StartsWith('<'))
        {
            if (controller)
            {
                string button = value.Replace(" ", string.Empty).Replace("-", string.Empty)
                    .ToLowerInvariant() switch
                {
                    "a" or "south" => "buttonSouth", "b" or "east" => "buttonEast",
                    "x" or "west" => "buttonWest", "y" or "north" => "buttonNorth",
                    "lb" => "leftShoulder", "rb" => "rightShoulder",
                    "lt" => "leftTrigger", "rt" => "rightTrigger",
                    "leftstickpress" or "ls" => "leftStickPress",
                    "rightstickpress" or "rs" => "rightStickPress",
                    "select" or "back" or "view" => "selectButton",
                    "start" or "menu" => "startButton",
                    "dpadup" => "dpad/up", "dpaddown" => "dpad/down",
                    "dpadleft" => "dpad/left", "dpadright" => "dpad/right", _ => value
                };
                path = "<Gamepad>/" + button;
            }
            else
            {
                string key = value.Replace(" ", string.Empty).Replace("-", string.Empty)
                    .ToLowerInvariant() switch
                {
                    "return" => "enter", "esc" => "escape", "ctrl" => "leftCtrl",
                    "left" => "leftArrow", "right" => "rightArrow",
                    "up" => "upArrow", "down" => "downArrow", _ => value.Replace(" ", string.Empty)
                };
                path = "<Keyboard>/" + key;
            }
        }
        int close = path.IndexOf('>');
        if (close < 2 || close + 2 >= path.Length || path[close + 1] != '/')
            return null;
        string layout = path[1..close];
        string leaf = path[(close + 2)..];
        if (controller)
        {
            bool knownGamepad = layout.Equals("Gamepad", StringComparison.OrdinalIgnoreCase);
            if (!knownGamepad)
            {
                try { knownGamepad = InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad"); }
                catch { return null; }
            }
            if (!knownGamepad)
                return null;
            leaf = leaf.ToLowerInvariant() switch
            {
                "select" => "selectButton", "start" => "startButton", _ => leaf
            };
            string[] buttons = { "buttonSouth", "buttonNorth", "buttonEast", "buttonWest",
                "leftShoulder", "rightShoulder", "leftTrigger", "rightTrigger", "selectButton", "startButton",
                "leftStickPress", "rightStickPress", "dpad/up", "dpad/down", "dpad/left", "dpad/right" };
            string? canonical = Array.Find(buttons, button => button.Equals(leaf,
                StringComparison.OrdinalIgnoreCase));
            return canonical == null ? null : "<" + layout + ">/" + canonical;
        }
        if (!layout.Equals("Keyboard", StringComparison.OrdinalIgnoreCase) || leaf.Contains('/'))
            return null;
        // A connected keyboard exposes both canonical names and aliases.
        // The enum fallback keeps configuration valid during device startup.
        InputControl? connected = InputSystem.FindControl("<Keyboard>/" + leaf);
        if (connected != null)
            return "<Keyboard>/" + connected.name.ToLowerInvariant();
        if ((leaf.Length == 1 && leaf[0] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9') ||
            Enum.GetNames(typeof(Key)).Any(name => name != "None" &&
                name.Equals(leaf, StringComparison.OrdinalIgnoreCase)))
            return "<Keyboard>/" + (leaf.StartsWith("Digit", StringComparison.OrdinalIgnoreCase)
                && leaf.Length == 6 ? leaf[5..] : leaf.ToLowerInvariant());
        return null;
    }
}
