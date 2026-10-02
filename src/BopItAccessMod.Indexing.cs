using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private const string IndexingPreferenceKey = "BopItAccess.Indexing";
    private bool _indexingEnabled = true;

    private void InitializeIndexingPreferenceOnMainThread()
    {
        try
        {
            _indexingEnabled = PlayerPrefs.GetInt(IndexingPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            _indexingEnabled = true;
            WriteStatus("Could not read the indexing preference: " + ex.Message);
        }
    }

    private void SetIndexingFromMenu(bool enabled)
    {
        if (_indexingEnabled == enabled)
            return;

        _indexingEnabled = enabled;
        try
        {
            PlayerPrefs.SetInt(IndexingPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save the indexing preference: " + ex.Message);
        }
        WriteStatus("Menu indexing " + (enabled ? "enabled" : "disabled") + ".");
    }

    // Index only the item that gained focus. Value changes on a focused
    // slider or toggle continue to announce the value alone.
    private string WithMenuIndex(string text, int zeroBasedIndex, int count)
    {
        if (!_indexingEnabled || zeroBasedIndex < 0 ||
            count <= 0 || zeroBasedIndex >= count)
            return text;

        string label = text.TrimEnd();
        if (label.EndsWith(".", StringComparison.Ordinal))
            label = label[..^1];
        return $"{label}, {zeroBasedIndex + 1} of {count}";
    }
}
