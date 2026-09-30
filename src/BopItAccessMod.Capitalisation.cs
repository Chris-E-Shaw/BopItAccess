using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private static readonly Regex UppercaseOneOnOne = new(
        @"(?<!\p{L})ONE[ -]ON[ -]ONE(?!\p{L})", RegexOptions.Compiled);
    private static readonly HashSet<string> SpokenAcronyms = new(
        StringComparer.Ordinal)
    {
        "SAPI", "NVDA", "JAWS", "SFX", "FPS", "UI", "LT", "RT",
        "LS", "RS", "PC", "USB", "HDR", "FOV", "DPI"
    };
    private const string FilterCapitalisationPreferenceKey =
        "BopItAccess.FilterCapitalisation";
    private volatile bool _filterCapitalisationEnabled = true;

    private void InitializeFilterCapitalisationPreferenceOnMainThread()
    {
        try
        {
            _filterCapitalisationEnabled =
                PlayerPrefs.GetInt(FilterCapitalisationPreferenceKey, 1) != 0;
        }
        catch (Exception ex)
        {
            _filterCapitalisationEnabled = true;
            WriteStatus("Could not read the capitalisation filter preference: " +
                ex.Message);
        }
    }

    private void SetFilterCapitalisationFromMenu(bool enabled)
    {
        if (_filterCapitalisationEnabled == enabled)
            return;

        _filterCapitalisationEnabled = enabled;
        try
        {
            PlayerPrefs.SetInt(FilterCapitalisationPreferenceKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            WriteStatus("Could not save the capitalisation filter preference: " +
                ex.Message);
        }
        WriteStatus("Capitalisation filter " +
            (enabled ? "enabled" : "disabled") + ".");
    }

    // Only the text handed to an output driver is changed. The labels and
    // values in Unity's UI are left exactly as the game rendered them.
    private static string FilterSpeechCapitalisation(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // The game's label uses spaces; the spoken name is conventionally
        // written with hyphens. This is still limited to all-caps source text.
        text = UppercaseOneOnOne.Replace(text, "ONE-ON-ONE");
        var result = new StringBuilder(text.Length);
        bool sentenceStart = true;
        for (int index = 0; index < text.Length;)
        {
            if (!char.IsLetter(text[index]))
            {
                char punctuation = text[index];
                result.Append(punctuation);
                if (punctuation == '.')
                    sentenceStart = true;
                index++;
                continue;
            }

            int start = index;
            int letterCount = 0;
            bool allUppercase = true;
            while (index < text.Length &&
                (char.IsLetter(text[index]) || char.IsDigit(text[index])))
            {
                char character = text[index++];
                if (char.IsLetter(character))
                {
                    letterCount++;
                    allUppercase &= char.IsUpper(character);
                }
            }

            string word = text[start..index];
            if (letterCount >= 2 && allUppercase &&
                !SpokenAcronyms.Contains(word))
            {
                string spoken = word.ToLowerInvariant();
                if (sentenceStart)
                    spoken = char.ToUpperInvariant(spoken[0]) + spoken[1..];
                result.Append(spoken);
            }
            else
                result.Append(word);
            sentenceStart = false;
        }
        return result.ToString();
    }
}
