using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string ReadControlTypesPreferenceKey = "BopItAccess.ReadControlTypes";
    private bool _readControlTypesEnabled = true;

    private void InitializeReadControlTypesPreferenceOnMainThread()
    {
        try
        {
            _readControlTypesEnabled =
                PlayerPrefs.GetInt(ReadControlTypesPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            _readControlTypesEnabled = true;
            WriteStatus("Could not read the control-type preference: " + ex.Message);
        }
    }

    private void SetReadControlTypesFromMenu(bool enabled)
    {
        if (_readControlTypesEnabled == enabled)
            return;

        _readControlTypesEnabled = enabled;
        try
        {
            PlayerPrefs.SetInt(ReadControlTypesPreferenceKey, enabled ? 1 : 0);
            SaveModPreferencesAndConfig();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save the control-type preference: " + ex.Message);
        }
        WriteStatus("Control-type announcements " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    // Use this on focus before adding a control's value or position. Changes
    // to a focused slider or toggle continue to announce just the new value.
    private string WithControlType(string label, string type)
    {
        if (!_readControlTypesEnabled || string.IsNullOrWhiteSpace(type))
            return label;

        string stem = label.TrimEnd();
        if (stem.Length > 0 && (stem[^1] is '.' or '。'))
            stem = stem[..^1];
        return LF("{0} {1}", stem, L(type));
    }
}
