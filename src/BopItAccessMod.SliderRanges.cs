using System.Globalization;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string SliderRangesPreferenceKey = "BopItAccess.SliderRanges";
    private bool _sliderRangesEnabled;

    private void InitializeSliderRangesPreferenceOnMainThread()
    {
        try
        {
            _sliderRangesEnabled = PlayerPrefs.GetInt(SliderRangesPreferenceKey, 0) != 0;
        }
        catch (Exception ex)
        {
            _sliderRangesEnabled = false;
            WriteStatus("Could not read the slider-ranges preference: " + ex.Message);
        }
    }

    private void SetSliderRangesFromMenu(bool enabled)
    {
        if (_sliderRangesEnabled == enabled)
            return;

        _sliderRangesEnabled = enabled;
        try
        {
            PlayerPrefs.SetInt(SliderRangesPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save the slider-ranges preference: " + ex.Message);
        }
        WriteStatus("Slider range announcements " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    // Called only while constructing a focus announcement, after the displayed
    // value and before WithMenuIndex. Changes on an already focused slider
    // remain value-only.
    private string WithSliderRange(string announcement, string label,
        string controlType)
    {
        if (!_sliderRangesEnabled || controlType != "slider")
            return announcement;

        string? range = ReadSliderRange(label);
        return range == null ? announcement : announcement.TrimEnd() + ", range " + range;
    }

    private string? ReadSliderRange(string label)
    {
        switch (label.ToUpperInvariant())
        {
            case "MUSIC":
            case "SFX":
            case "VOICE OVER":
                return "0 to 100";
            case "LANGUAGE":
                return ReadNativeSettingChoiceRange(language: true);
            case "RESOLUTION":
                return ReadNativeSettingChoiceRange(language: false);
            case "LIMIT FPS":
                return "30 to UNLIMITED";
            case "BUTTON HINTS DELAY":
                return "None to 60 seconds";
            case "REPEAT BUTTON HINTS":
                return "Off to Infinitely";
            case "REPEAT INTERVAL":
                return "15 seconds to 60 seconds";
            case "HINTS TYPE":
                return "Automatic to Both";
            case "OUTPUT MODE":
                return OutputModes[0] + " to " + OutputModes[^1];
            case "SAPI VOICE":
                return _sapiVoices.Count == 0 ? null :
                    _sapiVoices[0].Name + " to " + _sapiVoices[^1].Name;
            case "SAPI VOLUME":
                return "5% to 100%";
            case "SAPI RATE":
            case "SAPI PITCH":
                return "0 to 100";
            default:
                return null;
        }
    }

    private static string? ReadNativeSettingChoiceRange(bool language)
    {
        try
        {
            Settings? settings = UnityEngine.Object.FindFirstObjectByType<Settings>();
            if (settings == null)
                return null;
            var choices = language ? settings.Locales : settings.Resolutions;
            if (choices == null || choices.Count == 0)
                return null;
            string? first = CleanSpeechValue(choices[0]);
            string? last = CleanSpeechValue(choices[choices.Count - 1]);
            return first == null || last == null ? null : first + " to " + last;
        }
        catch
        {
            // These lists are populated asynchronously by the game. Avoid
            // announcing a guessed range while they are still unavailable.
            return null;
        }
    }

    private string WithUnitySliderRange(string announcement, Selectable? control)
    {
        if (!_sliderRangesEnabled || control == null)
            return announcement;
        if (control is Scrollbar)
            return announcement.TrimEnd() + ", range 0 to 1";
        if (control is not Slider slider)
            return announcement;
        string min = slider.minValue.ToString("0.##", CultureInfo.InvariantCulture);
        string max = slider.maxValue.ToString("0.##", CultureInfo.InvariantCulture);
        return announcement.TrimEnd() + ", range " + min + " to " + max;
    }
}
