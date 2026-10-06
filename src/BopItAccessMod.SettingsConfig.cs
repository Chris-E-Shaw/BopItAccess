using System.Runtime.Versioning;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private SettingsConfigFile? _settingsConfigDocument;
    private bool _settingsConfigModImported;
    private bool _settingsConfigNativeImported;
    private bool _settingsConfigBindingsImported;
    private bool _settingsConfigApplying;
    private bool _settingsConfigExternalChanges;
    private volatile bool _settingsConfigDirty = true;
    private long _settingsConfigStartedAt;
    private long _nextSettingsConfigProbeAt;
    private long _nextSettingsConfigErrorAt;
    private long _settingsConfigRetryAfter;
    private long _settingsConfigSaveAfter;
    private int _lastSettingsConfigWriteFrame = -1;
    private InputRebindingManager? _settingsConfigBindingManager;
    private readonly Dictionary<string, string> _settingsConfigLastBindings =
        new(StringComparer.OrdinalIgnoreCase);

    private static string SettingsConfigPath => Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty) ??
        Environment.CurrentDirectory, "UserData", SettingsConfigFile.FileName);

    private void PrepareSettingsConfig()
    {
        try
        {
            _settingsConfigDocument = SettingsConfigFile.Load(SettingsConfigPath, WriteStatus);
            WriteStatus(_settingsConfigDocument.Fingerprint == null
                ? "No editable settings file yet; current settings will be captured after startup."
                : "Read editable settings file: " + SettingsConfigPath + ".");
        }
        catch (Exception ex)
        {
            // A locked/unreadable file must not be replaced with defaults.
            // Keep normal saved preferences available so the game can start.
            _settingsConfigExternalChanges = true;
            _settingsConfigModImported = true;
            WriteStatus("Could not read editable settings; preserving the file and saved preferences: " + ex.Message);
        }
    }

    private void UpdateSettingsConfigStartup()
    {
        long now = Environment.TickCount64;
        if (_settingsConfigStartedAt == 0) _settingsConfigStartedAt = now;
        if (_settingsConfigDocument == null) return;
        try
        {
            if (!_settingsConfigModImported)
            {
                // First-run detection must see the original preferences,
                // before an INI can add mod keys and mimic an old install.
                if (!_nativeAudioPreferenceChecked && !_nativeAudioHandled) return;
                // PlatformIO restores the Steam/cloud preference store after
                // its singleton appears. Import after that hydration, before
                // initializing speech. Keep recovery available if startup
                // itself fails to complete.
                if (PlatformIO.Instance?.IsInitialized != true &&
                    now - _settingsConfigStartedAt < 8000) return;
                _settingsConfigApplying = true;
                try { ApplyModConfig(_settingsConfigDocument.Values); }
                finally { _settingsConfigApplying = false; }
                _settingsConfigModImported = true;
            }
            if (_settingsConfigBindingsImported && _settingsConfigNativeImported) return;
            if (now < _nextSettingsConfigProbeAt) return;
            _nextSettingsConfigProbeAt = now + 250;
            Settings? settings = _nativeLoadedSettings;
            if (!_nativeSettingsLoadObserved || settings == null ||
                !_nativeAudioHandled || !NativeConfigSettingsReady(settings)) return;

            _settingsConfigApplying = true;
            try
            {
                if (!_settingsConfigNativeImported)
                {
                    ApplyNativeConfig(settings, _settingsConfigDocument.Values);
                    _settingsConfigNativeImported = true;
                    _nextLocaleProbeAt = 0;
                }
                MainMenuUIManager? main = _mainMenu ??
                    UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
                // Unity's destroyed-object check is different from ??=;
                // reacquire a panel manager if its previous scene was unloaded.
                if (_settingsConfigBindingManager == null)
                    _settingsConfigBindingManager = main?.controlsPanel?
                        .GetComponentInChildren<InputRebindingManager>(true);
                InputRebindingManager? manager = _settingsConfigBindingManager;
                if (manager == null) return;
                if (!NativeConfigBindingsReady(manager))
                {
                    // The Controls object can be inactive until its first
                    // visit. Restore its saved native profile before applying
                    // edits; do not save a default/partially loaded asset.
                    if (!NativeConfigBindingsCanInitialize(manager)) return;
                    manager.LoadBindings();
                    if (!NativeConfigBindingsReady(manager)) return;
                }
                ApplyNativeConfigBindings(manager, _settingsConfigDocument.Values);
                ApplyModConfigBindings(manager);
                _settingsConfigBindingsImported = !HasPendingModConfigBindings;
                if (_settingsConfigBindingsImported)
                {
                    _settingsConfigDirty = true;
                    WriteStatus("Editable settings applied for this launch.");
                }
            }
            finally { _settingsConfigApplying = false; }
        }
        catch (Exception ex) { ReportSettingsConfigError("Startup settings import", ex); }
    }

    private bool SettingsConfigAllowsSpeechStartup => _settingsConfigModImported &&
        (_settingsConfigDocument == null ||
         (_settingsConfigNativeImported &&
          (!HasPendingModConfigBindings || _settingsConfigBindingsImported)) ||
         Environment.TickCount64 - _settingsConfigStartedAt >= 8000);

    private static void SaveModPreferencesAndConfig()
    {
        PlayerPrefs.Save();
        try { NoteSettingsConfigNativeSaved(); }
        catch (Exception ex) { WriteStatus("Could not synchronize editable settings: " + ex.Message); }
    }

    internal static void NoteSettingsConfigNativeSaved()
    {
        BopItAccessMod? mod = _activeNativeAudioMod;
        if (mod == null || mod._settingsConfigApplying) return;
        mod._settingsConfigDirty = true;
        mod._settingsConfigSaveAfter = Environment.TickCount64 + 250;
        // Save callbacks can run during initialization or a rebind. Snapshot
        // Unity state only after the current update has finished, on its thread.
    }

    private void UpdateSettingsConfigExport()
    {
        if (Environment.TickCount64 < _settingsConfigRetryAfter) return;
        if (_settingsConfigDirty)
        {
            FlushSettingsConfig(force: false);
            return;
        }
        long now = Environment.TickCount64;
        if (now < _nextSettingsConfigProbeAt) return;
        _nextSettingsConfigProbeAt = now + 1000;
        FlushSettingsConfig(force: false);
    }

    private void FlushSettingsConfig(bool force)
    {
        if (_settingsConfigDocument == null || _settingsConfigExternalChanges ||
            _settingsConfigApplying || !_settingsConfigNativeImported ||
            !_settingsConfigBindingsImported || !_speechToggleInitialized) return;
        if (Environment.TickCount64 < _settingsConfigRetryAfter) return;
        if (!force && (Environment.TickCount64 < _settingsConfigSaveAfter ||
            _lastSettingsConfigWriteFrame == Time.frameCount)) return;
        Settings? settings = _nativeLoadedSettings;
        if (settings == null || settings.SettingsData == null) return;
        try
        {
            _settingsConfigApplying = true;
            if (!_settingsConfigDocument.MatchesDisk(SettingsConfigPath))
                throw new ConfigChangedOutsideGameException();
            Dictionary<string, string> values = CaptureNativeConfig(settings);
            foreach (var pair in CaptureModConfig()) values[pair.Key] = pair.Value;
            InputRebindingManager? manager = _settingsConfigBindingManager;
            if (manager != null && NativeConfigBindingsReady(manager))
            {
                foreach (var pair in CaptureNativeConfigBindings(manager))
                    _settingsConfigLastBindings[pair.Key] = pair.Value;
            }
            foreach (var pair in _settingsConfigLastBindings) values[pair.Key] = pair.Value;
            Dictionary<string, string> descriptions = CaptureModConfigDescriptions();
            descriptions["Game.MusicVolume"] = "Music volume: 0 to 100 percent.";
            descriptions["Game.SfxVolume"] = "Sound effects volume: 0 to 100 percent.";
            descriptions["Game.VoiceOverVolume"] = "Game voice cues: 0 to 100 percent.";
            descriptions["Game.Language"] = "Language code, for example en (English). Installed choices are listed below.";
            descriptions["Game.Vibration"] = "Controller vibration: On or Off.";
            descriptions["Game.Fullscreen"] = "On or Off.";
            descriptions["Game.Resolution"] = "Use an available width x height from the list below.";
            descriptions["Game.AudioLatencyMilliseconds"] = "Audio timing correction: 0 to 400 milliseconds.";
            foreach (string key in _settingsConfigLastBindings.Keys)
                descriptions[key] = "Input path or key/button name; Default restores the game binding; None unbinds it.";
            Dictionary<string, string[]> choices = CaptureModConfigVoiceChoices();
            List<string> locales = new();
            foreach (string locale in settings.Locales)
                locales.Add(NormalizeGameLocale(locale) ?? locale);
            choices["Game.Language"] = locales.ToArray();
            List<string> resolutions = new();
            foreach (string resolution in settings.Resolutions)
                resolutions.Add(resolution);
            choices["Game.Resolution"] = resolutions.ToArray();
            string text = _settingsConfigDocument.Render(values, descriptions, choices);
            _settingsConfigDirty = false;
            if (_settingsConfigDocument.HasSameText(text)) return;
            bool creating = _settingsConfigDocument.Fingerprint == null;
            _settingsConfigDocument.SaveAtomic(SettingsConfigPath, text);
            _lastSettingsConfigWriteFrame = Time.frameCount;
            WriteStatus(creating ? "Created editable settings file: " + SettingsConfigPath + "."
                : "Updated editable settings from the current game and mod preferences.");
        }
        catch (ConfigChangedOutsideGameException)
        {
            _settingsConfigExternalChanges = true;
            WriteStatus("Settings file changed outside the game. Autosaving is suspended to protect your edits; restart to load them.");
        }
        catch (Exception ex)
        {
            _settingsConfigDirty = true;
            _settingsConfigRetryAfter = Environment.TickCount64 + 10000;
            ReportSettingsConfigError("Settings file export", ex);
        }
        finally { _settingsConfigApplying = false; }
    }

    private void ReportSettingsConfigError(string operation, Exception ex)
    {
        long now = Environment.TickCount64;
        if (now < _nextSettingsConfigErrorAt) return;
        _nextSettingsConfigErrorAt = now + 10000;
        WriteStatus(operation + " failed; saved preferences remain available: " + ex.Message);
    }
}

[HarmonyPatch(typeof(Settings), "Save")]
[SupportedOSPlatform("windows")]
internal static class EditableSettingsSavePatch
{
    [HarmonyPostfix]
    private static void AfterSave()
    {
        try { BopItAccessMod.NoteSettingsConfigNativeSaved(); }
        catch { /* Config export cannot prevent native settings persistence. */ }
    }
}
