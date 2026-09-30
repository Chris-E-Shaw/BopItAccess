using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string BrailleOutputPreferenceKey = "BopItAccess.BrailleOutput";
    private volatile bool _brailleOutputEnabled = true;

    private void InitializeBrailleOutputPreferenceOnMainThread()
    {
        try
        {
            _brailleOutputEnabled =
                PlayerPrefs.GetInt(BrailleOutputPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            _brailleOutputEnabled = true;
            WriteStatus("Could not read braille output preference: " + ex.Message);
        }
        WriteStatus("Braille output: " + (_brailleOutputEnabled ? "On" : "Off") + ".");
    }

    private void SetBrailleOutputFromMenu(bool enabled)
    {
        if (_brailleOutputEnabled == enabled)
            return;
        _brailleOutputEnabled = enabled;
        try
        {
            PlayerPrefs.SetInt(BrailleOutputPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save braille output preference: " + ex.Message);
        }
        WriteStatus("Braille output " + (enabled ? "enabled" : "disabled") + ".");
    }
}
