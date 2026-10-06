using System.Text;
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
            SaveModPreferencesAndConfig();
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
        if (label.Length > 0 && (label[^1] is '.' or '。'))
            label = label[..^1];
        return LF("{0}, {1} of {2}", label,
            LocalizedIndexNumber(zeroBasedIndex + 1),
            LocalizedIndexNumber(count));
    }

    // Keep numeric values for other locales so their existing formatting stays
    // intact. Guide row and line counters use this same speech-only conversion.
    private static object LocalizedIndexNumber(int value) => CurrentGameLocale switch
    {
        "ja" => (object)JapaneseIndexNumber(value),
        "zh" => ChineseIndexNumber(value),
        _ => value
    };

    // Kanji numerals keep Japanese position announcements in the language of
    // the surrounding text when a screen reader uses a different voice for
    // ASCII digits and punctuation.
    private static string JapaneseIndexNumber(int value)
    {
        if (value == 0)
            return "零";
        if (value < 0)
            return value.ToString();

        var result = new StringBuilder();
        if (value >= 100_000_000)
        {
            AppendJapaneseFourDigits(result, value / 100_000_000);
            result.Append('億');
            value %= 100_000_000;
        }
        if (value >= 10_000)
        {
            AppendJapaneseFourDigits(result, value / 10_000);
            result.Append('万');
            value %= 10_000;
        }
        AppendJapaneseFourDigits(result, value);
        return result.ToString();
    }

    private static void AppendJapaneseFourDigits(StringBuilder result, int value)
    {
        const string digits = "一二三四五六七八九";
        int thousands = value / 1000;
        if (thousands > 0)
        {
            if (thousands > 1)
                result.Append(digits[thousands - 1]);
            result.Append('千');
        }
        int hundreds = value / 100 % 10;
        if (hundreds > 0)
        {
            if (hundreds > 1)
                result.Append(digits[hundreds - 1]);
            result.Append('百');
        }
        int tens = value / 10 % 10;
        if (tens > 0)
        {
            if (tens > 1)
                result.Append(digits[tens - 1]);
            result.Append('十');
        }
        int ones = value % 10;
        if (ones > 0)
            result.Append(digits[ones - 1]);
    }

    private static string ChineseIndexNumber(int value)
    {
        if (value == 0)
            return "零";
        if (value < 0)
            return value.ToString();

        var result = new StringBuilder();
        int hundredMillions = value / 100_000_000;
        int tenThousands = value / 10_000 % 10_000;
        int remainder = value % 10_000;
        if (hundredMillions > 0)
        {
            AppendChineseFourDigits(result, hundredMillions);
            result.Append('亿');
        }
        if (tenThousands > 0)
        {
            if (hundredMillions > 0 && tenThousands < 1000)
                result.Append('零');
            AppendChineseFourDigits(result, tenThousands);
            result.Append('万');
        }
        if (remainder > 0)
        {
            if ((hundredMillions > 0 || tenThousands > 0) &&
                (tenThousands == 0 || remainder < 1000))
                result.Append('零');
            AppendChineseFourDigits(result, remainder);
        }
        return result.ToString();
    }

    private static void AppendChineseFourDigits(StringBuilder result, int value)
    {
        const string digits = "一二三四五六七八九";
        const string units = "千百十";
        bool emitted = false;
        bool pendingZero = false;
        for (int place = 0; place < 4; place++)
        {
            int divisor = place switch
            {
                0 => 1000,
                1 => 100,
                2 => 10,
                _ => 1
            };
            int digit = value / divisor % 10;
            if (digit == 0)
            {
                pendingZero |= emitted;
                continue;
            }
            if (pendingZero)
                result.Append('零');
            pendingZero = false;
            if (digit != 1 || place != 2 || emitted)
                result.Append(digits[digit - 1]);
            if (place < 3)
                result.Append(units[place]);
            emitted = true;
        }
    }
}
