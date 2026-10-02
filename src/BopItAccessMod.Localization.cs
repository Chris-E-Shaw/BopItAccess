using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Il2Cpp;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    // The game owns the language choice. Keep a managed snapshot so the Prism
    // worker and other static speech helpers never touch Unity objects.
    private static string _gameLocale = "en";
    private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>>
        LocaleCatalogs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, (string English, string Translation)[]>
        LocaleFragments = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string[] BindingDisplayTerms =
    {
        "Left Stick Press", "Right Stick Press", "Left Stick", "Right Stick",
        "Left Shift", "Right Shift", "Left Control", "Right Control",
        "Left Ctrl", "Right Ctrl", "Left Alt", "Right Alt",
        "Left Trigger", "Right Trigger", "Left Bumper", "Right Bumper",
        "Left Shoulder", "Right Shoulder", "D-Pad Up", "D-Pad Down",
        "D-Pad Left", "D-Pad Right", "D-Pad", "Page Up", "Page Down",
        "Up Arrow", "Down Arrow", "Left Arrow", "Right Arrow",
        "Caps Lock", "Num Lock", "Scroll Lock", "Print Screen",
        "North Button", "South Button", "East Button", "West Button",
        "Right Bracket", "Left Bracket", "Arrow Keys", "Backspace",
        "Escape", "Enter", "Space", "Tab", "Home", "End", "Insert",
        "Delete", "Select", "Start", "Menu", "Trigger", "Bumper",
        "Shoulder", "Stick", "Press", "Button", "Shift", "Control",
        "Ctrl", "Alt", "North", "South", "East", "West", "Up",
        "Down", "Left", "Right", "or", "and"
    };
    private static readonly Regex BindingDisplayPattern = new(
        @"(?<![\p{L}\p{N}])(?:" + string.Join("|", BindingDisplayTerms
            .OrderByDescending(term => term.Length).Select(Regex.Escape)) +
        @")(?![\p{L}\p{N}])",
        RegexOptions.Compiled | RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant);
    private long _nextLocaleProbeAt;
    private long _nextLocaleErrorLogAt;

    private static string CurrentGameLocale => Volatile.Read(ref _gameLocale);

    private static string L(string english)
    {
        if (string.IsNullOrEmpty(english))
            return english;
        string locale = CurrentGameLocale;
        if (locale == "en")
            return english;
        IReadOnlyDictionary<string, string> catalog = LocaleCatalogs.GetOrAdd(
            locale, LoadLocaleCatalog);
        return catalog.TryGetValue(english, out string? translated) &&
            !string.IsNullOrWhiteSpace(translated)
            ? translated : english;
    }

    // Input System generates binding display names at runtime, including
    // arbitrary combinations after rebinding. Translate only known control
    // words inside this dedicated path; letters, numbers and user names stay
    // untouched. This also supports combinations such as Left Shift+A.
    private static string LocalizeBindingDisplay(string? display)
    {
        if (string.IsNullOrWhiteSpace(display))
            return display ?? string.Empty;
        if (CurrentGameLocale == "en")
            return display;
        // Several button names are ordinary game words too. A binding must
        // use its own catalog key: "Space" here means a keyboard key, never
        // the Space stage, and "Start" means the controller button.
        string contextualKey = BindingTermCatalogKey(display);
        if (!string.Equals(contextualKey, display, StringComparison.Ordinal))
            return L(contextualKey);
        string exact = L(display);
        if (!string.Equals(exact, display, StringComparison.Ordinal))
            return exact;
        return BindingDisplayPattern.Replace(display, match =>
        {
            string canonical = BindingDisplayTerms.First(term =>
                string.Equals(term, match.Value,
                    StringComparison.OrdinalIgnoreCase));
            return L(BindingTermCatalogKey(canonical));
        });
    }

    private static string BindingTermCatalogKey(string term) => term switch
    {
        "Space" => "Space key",
        "Enter" => "Enter key",
        "Select" => "Select button",
        "Start" => "Start button",
        "Menu" => "Menu button",
        "Control" => "Control key",
        "Ctrl" => "Ctrl key",
        _ => term
    };

    private static string LF(string english, params object?[] values)
    {
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(CurrentGameLocale == "zh"
                ? "zh-CN" : CurrentGameLocale);
        }
        catch (CultureNotFoundException)
        {
            culture = CultureInfo.CurrentCulture;
        }
        try
        {
            return string.Format(culture, L(english), values);
        }
        catch (FormatException ex)
        {
            WriteStatus("Invalid localized speech format for '" + english +
                "'; using English: " + ex.Message);
            return string.Format(culture, english, values);
        }
    }

    // Final safety net for older, composed announcements. The first phrase
    // must be a known English key before fragments are substituted, so text
    // read directly from an already localized native label is left alone.
    // New code should still use L on complete strings and format templates.
    private static string LocalizeSpeechText(string text)
    {
        if (string.IsNullOrEmpty(text) || CurrentGameLocale == "en")
            return text;
        string exact = L(text);
        if (!string.Equals(exact, text, StringComparison.Ordinal))
            return exact;

        string locale = CurrentGameLocale;
        (string English, string Translation)[] fragments =
            LocaleFragments.GetOrAdd(locale, key => LocaleCatalogs
                .GetOrAdd(key, LoadLocaleCatalog)
                .Where(pair => pair.Key.Length >= 3 &&
                    !pair.Key.Contains('{') && !pair.Key.Contains('\n') &&
                    !string.Equals(pair.Key, pair.Value,
                        StringComparison.Ordinal))
                .OrderByDescending(pair => pair.Key.Length)
                .Select(pair => (pair.Key, pair.Value))
                .ToArray());
        if (fragments.Length == 0)
            return text;

        int first = 0;
        while (first < text.Length && char.IsWhiteSpace(text[first]))
            first++;
        if (first == text.Length || !TryMatchFragment(text, first, fragments,
                out _, out _))
            return text;

        var translated = new System.Text.StringBuilder(text.Length);
        int position = 0;
        while (position < text.Length)
        {
            if (TryMatchFragment(text, position, fragments,
                    out int length, out string replacement))
            {
                translated.Append(replacement);
                position += length;
            }
            else
            {
                translated.Append(text[position]);
                position++;
            }
        }
        return translated.ToString();
    }

    private static bool TryMatchFragment(string text, int position,
        (string English, string Translation)[] fragments,
        out int length, out string replacement)
    {
        foreach ((string english, string translation) in fragments)
        {
            // Bop is also the first word of the game's and mod's names.
            // Fragment fallback must never turn those proper names into a
            // translated gameplay command.
            if (english == "Bop" && text.AsSpan(position).StartsWith(
                    "Bop It".AsSpan(), StringComparison.Ordinal))
                continue;
            if (position + english.Length > text.Length ||
                !text.AsSpan(position).StartsWith(english.AsSpan(),
                    StringComparison.Ordinal))
                continue;
            if (position > 0 && char.IsLetterOrDigit(english[0]) &&
                char.IsLetterOrDigit(text[position - 1]))
                continue;
            int after = position + english.Length;
            if (after < text.Length && char.IsLetterOrDigit(english[^1]) &&
                char.IsLetterOrDigit(text[after]))
                continue;
            length = english.Length;
            replacement = translation;
            return true;
        }
        length = 0;
        replacement = string.Empty;
        return false;
    }

    private static IReadOnlyDictionary<string, string> LoadLocaleCatalog(string locale)
    {
        const string resourcePrefix = "BopItAccess.locales.";
        string resourceName = resourcePrefix + locale + ".json";
        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                WriteStatus("No embedded speech catalog for " + locale +
                    "; English speech will be used for missing strings.");
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new FormatException("The locale catalog must be a JSON object.");
            var translations = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                    continue;
                string? translated = property.Value.GetString();
                if (!string.IsNullOrWhiteSpace(translated))
                    translations[property.Name] = translated;
            }
            WriteStatus("Loaded " + translations.Count + " speech strings for " +
                locale + ".");
            return translations;
        }
        catch (Exception ex)
        {
            WriteStatus("Could not load speech catalog for " + locale + ": " + ex);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private void UpdateGameLocale(bool force = false)
    {
        long now = Environment.TickCount64;
        if (!force && now < _nextLocaleProbeAt)
            return;
        _nextLocaleProbeAt = now + 100;

        string? locale = null;
        try
        {
            // SelectedLocale is the locale actually rendering the game's GUI.
            if (LocalizationSettings.HasSettings)
                locale = NormalizeGameLocale(
                    LocalizationSettings.SelectedLocale?.Identifier.Code);
        }
        catch (Exception ex)
        {
            if (now >= _nextLocaleErrorLogAt)
            {
                _nextLocaleErrorLogAt = now + 5000;
                WriteStatus("Could not read Unity's active locale: " + ex.Message);
            }
        }

        if (locale == null)
        {
            try
            {
                Settings? settings = _nativeLoadedSettings;
                if (settings == null)
                    settings = UnityEngine.Object.FindFirstObjectByType<Settings>();
                locale = NormalizeGameLocale(settings?.SettingsData?.Language);
            }
            catch (Exception ex)
            {
                if (now >= _nextLocaleErrorLogAt)
                {
                    _nextLocaleErrorLogAt = now + 5000;
                    WriteStatus("Could not read the game's saved language: " +
                        ex.Message);
                }
            }
        }

        // Before the first announcement, the saved language is more reliable
        // than Unity's temporary English default during table initialization.
        // Once speech is initialized, the live Unity locale takes precedence
        // for language changes made in Settings.
        if (!_speechToggleInitialized && _nativeSettingsLoadObserved)
        {
            string? savedLocale = NormalizeGameLocale(
                _nativeLoadedSettings?.SettingsData?.Language);
            if (savedLocale != null)
                locale = savedLocale;
        }

        if (locale == null || locale == CurrentGameLocale)
            return;
        Volatile.Write(ref _gameLocale, locale);
        WriteStatus("Game language changed to " + locale + ".");
        // A focused native control may remain selected as its label changes.
        // Permit its new localized name to be announced on the next read.
        ResetMenuFocus();
        ResetSettingsFocus();
        _lastFocusedPlayModeButtonId = 0;
        _lastFocusedControlsRowId = 0;
        _lastSpeechMenuRowId = 0;
        _welcomeFocusedRowId = 0;
        _lastTrackSelectFocusedId = 0;
        _lastPauseMenuFocusedId = 0;
        _lastGameOverFocusedId = 0;
        _guideLastTopicRowId = 0;
        _guideLastPageFocus = null;
        RefreshDescriptionControlLocale();
        RefreshScoreControlLocale();
        _cachedButtonHintContext = null;
        _nextButtonHintContextProbeAt = 0;
    }

    private static string? NormalizeGameLocale(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        string code = raw.Trim();
        // SettingsData.Language is persisted as, for example, "English (en)".
        if (code.EndsWith(')'))
        {
            int opening = code.LastIndexOf('(');
            if (opening >= 0)
                code = code[(opening + 1)..^1].Trim();
        }
        code = code.Replace('_', '-').ToLowerInvariant();
        return code switch
        {
            "en" or "en-us" or "en-gb" => "en",
            "fr" or "fr-fr" => "fr",
            "it" or "it-it" => "it",
            "de" or "de-de" => "de",
            "es" or "es-es" => "es",
            "es-mx" or "es-419" => "es-MX",
            "ja" or "ja-jp" => "ja",
            "ko" or "ko-kr" => "ko",
            "zh" or "zh-cn" or "zh-hans" => "zh",
            "pt-br" => "pt-BR",
            _ => null
        };
    }
}
