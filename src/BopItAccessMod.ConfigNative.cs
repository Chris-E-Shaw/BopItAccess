using System.Globalization;
using Il2Cpp;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // Launch configuration writes only the settings exposed by the game's
    // Settings panel. Internal quality/gyro flags are deliberately preserved.
    private static bool NativeConfigSettingsReady(Settings settings) =>
        settings.SettingsData != null && settings.fmodBusManager != null &&
        settings.Locales != null && settings.Locales.Count != 0 &&
        settings.Resolutions != null && settings.Resolutions.Count != 0;

    private void ApplyNativeConfig(Settings settings,
        IReadOnlyDictionary<string, string> values)
    {
        SettingsData data = settings.SettingsData;
        bool changed = false;
        bool audioChanged = false;
        bool languageChanged = false;
        bool displayChanged = false;

        void SetVolume(string key, float current, Action<float> setter)
        {
            if (!TryNativeConfigFloat(values, key, 0, 100, out float next) ||
                next == current)
                return;
            setter(next);
            changed = audioChanged = true;
        }

        SetVolume("Game.MusicVolume", data.MusicVolume, value => data.MusicVolume = value);
        SetVolume("Game.SfxVolume", data.SFXVolume, value => data.SFXVolume = value);
        SetVolume("Game.VoiceOverVolume", data.VoiceOverVolume, value => data.VoiceOverVolume = value);
        if (TryNativeConfigBool(values, "Game.Vibration", out bool vibration) &&
            data.Vibration != vibration)
        {
            data.Vibration = vibration;
            changed = true;
        }
        if (TryNativeConfigBool(values, "Game.Fullscreen", out bool fullscreen) &&
            data.FullScreen != fullscreen)
        {
            data.FullScreen = fullscreen;
            changed = displayChanged = true;
        }
        // The native latency control clamps from 0 through 400 milliseconds.
        if (TryNativeConfigFloat(values, "Game.AudioLatencyMilliseconds", 0, 400,
                out float latency) && data.Latency != latency)
        {
            data.Latency = latency;
            changed = true;
        }
        if (values.TryGetValue("Game.Language", out string? language))
        {
            string? requested = NormalizeGameLocale(language);
            string? matched = null;
            foreach (string locale in settings.Locales)
            {
                if ((requested != null && string.Equals(requested,
                        NormalizeGameLocale(locale), StringComparison.OrdinalIgnoreCase)) ||
                    string.Equals(locale, language.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    matched = locale;
                    break;
                }
            }
            if (matched == null)
                WriteStatus("Config ignored Game.Language: use an installed game locale code, for example en.");
            else if (!string.Equals(data.Language, matched, StringComparison.Ordinal))
            {
                data.Language = matched;
                changed = languageChanged = true;
            }
        }
        if (values.TryGetValue("Game.Resolution", out string? resolution))
        {
            string requested = resolution.Trim().Replace(" ", string.Empty)
                .Replace('×', 'x').ToLowerInvariant();
            string? matched = null;
            foreach (string choice in settings.Resolutions)
            {
                if (string.Equals(choice.Replace(" ", string.Empty).ToLowerInvariant(),
                        requested, StringComparison.Ordinal))
                {
                    matched = choice;
                    break;
                }
            }
            if (matched == null)
                WriteStatus("Config ignored Game.Resolution: choose a resolution supported by this display.");
            else if (!string.Equals(data.Resolution, matched, StringComparison.Ordinal))
            {
                data.Resolution = matched;
                changed = displayChanged = true;
            }
        }

        // Use the same public update paths as the native menu rather than
        // changing only its labels or invoking offsets into game memory.
        if (audioChanged)
            settings.UpdateAudio(data);
        if (languageChanged)
            settings.UpdateLanguage(data);
        if (displayChanged)
            settings.UpdateResolution(data);
        if (changed)
        {
            settings.Save();
            WriteStatus("Applied configured game audio, language, display and accessibility settings.");
        }
    }

    private static Dictionary<string, string> CaptureNativeConfig(Settings settings)
    {
        SettingsData data = settings.SettingsData;
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Game.MusicVolume"] = data.MusicVolume.ToString("0.###", CultureInfo.InvariantCulture),
            ["Game.SfxVolume"] = data.SFXVolume.ToString("0.###", CultureInfo.InvariantCulture),
            ["Game.VoiceOverVolume"] = data.VoiceOverVolume.ToString("0.###", CultureInfo.InvariantCulture),
            ["Game.Language"] = NormalizeGameLocale(data.Language) ?? data.Language ?? "en",
            ["Game.Vibration"] = data.Vibration ? "On" : "Off",
            ["Game.Fullscreen"] = data.FullScreen ? "On" : "Off",
            ["Game.Resolution"] = data.Resolution ?? string.Empty,
            ["Game.AudioLatencyMilliseconds"] = data.Latency.ToString("0.###", CultureInfo.InvariantCulture)
        };
    }

    private bool TryNativeConfigFloat(IReadOnlyDictionary<string, string> values,
        string key, float minimum, float maximum, out float value)
    {
        value = 0;
        if (!values.TryGetValue(key, out string? text))
            return false;
        if (float.TryParse(text.Trim().TrimEnd('%'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out value) && float.IsFinite(value) &&
            value >= minimum && value <= maximum)
            return true;
        WriteStatus($"Config ignored {key}: expected a number from {minimum} to {maximum}.");
        return false;
    }

    private bool TryNativeConfigBool(IReadOnlyDictionary<string, string> values,
        string key, out bool value)
    {
        value = false;
        if (!values.TryGetValue(key, out string? text))
            return false;
        switch (text.Trim().ToLowerInvariant())
        {
            case "on": case "true": case "yes": case "1": value = true; return true;
            case "off": case "false": case "no": case "0": return true;
            default:
                WriteStatus($"Config ignored {key}: expected On or Off.");
                return false;
        }
    }
}
