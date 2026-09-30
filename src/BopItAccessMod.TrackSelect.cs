using Il2Cpp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Il2CppTMPro;

namespace BopItAccess;

public sealed partial class BopItAccessMod
{
    private GameUIManager? _trackSelectUi;
    private App? _trackSelectApp;
    private long _nextTrackSelectSearchAt;
    private bool _trackSelectWasVisible;
    private string? _lastTrackSelectTheme;
    private bool? _lastTrackSelectExtreme;
    private int _lastTrackSelectFocusedId;
    private int _lastTrackSelectObservedSelectionId;
    private int _lastTrackSelectPanelId;
    private bool _descriptionWasRequestedOnTrackSelect;

    private const string ShapesDescription =
        "Shapes. An abstract digital scene with no recognizable room or ground, styled like a nostalgic 1990s pizza shop turned neon DJ night. " +
        "Hot pink, purple, and teal fill the space. Squares, angular forms, rounded cubes, spheres, and pyramids mingle with floating squiggles and other geometric patterns. " +
        "The shapes drift and bounce in a playful, screensaver-like motion, making the level feel lively and rhythmic.";
    private const string SpaceDescription =
        "Space. A bright, playful galaxy surrounds the Bop It device. Deep blue, purple, and black suggest open space, with stars, cosmic clouds, planets, and floating rocky forms adding depth. " +
        "Friendly animated aliens drift nearby and cheer the player on. Their glowing green, silver, and neon-blue accents stand out against the dark backdrop, giving the scene a cheerful sci-fi atmosphere.";
    private const string CityDescription =
        "City. A layered cartoon city glows at twilight, with dark blue buildings and warm orange and yellow lights. Skyscrapers, lit windows, streetlights, billboards, and playful signs create a busy downtown scene. " +
        "Some signs use music-themed lettering, including street names such as “Bop It Blvd.” and “Spin It St.” The featured billboard cat bops and rolls as the city moves around it, adding a goofy animated focal point.";
    private const string OfficeDescription =
        "Office. A whimsical workplace scene centers on a desk and computer. The monitor becomes an aquarium: fish swim inside it, surrounded by colorful aquatic imagery. " +
        "A keyboard, leafy plants, and small flowers add detail around the workstation. The office theme brings a mundane corporate setting together with the surprising underwater scene, creating a playful contrast.";

    // The song and difficulty screen is the game's start screen in the game
    // scene. Mode selection happens in the main-menu scene immediately before it.
    private bool ReadTrackSelectFocus()
    {
        if (_trackSelectUi == null)
        {
            // Returning to the main-menu scene destroys GameUIManager. Clear
            // the previous visit even while the next search is throttled.
            ResetTrackSelectFocus();
            long now = Environment.TickCount64;
            if (now < _nextTrackSelectSearchAt)
                return false;

            _nextTrackSelectSearchAt = now + 500;
            _trackSelectUi = UnityEngine.Object.FindFirstObjectByType<GameUIManager>();
            if (_trackSelectUi == null)
                return false;
        }

        StartScreenPanel? panel = _trackSelectUi.startScreen;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetTrackSelectFocus();
            return false;
        }

        int panelId = panel.GetInstanceID();
        if (panelId != _lastTrackSelectPanelId)
        {
            ResetTrackSelectFocus();
            _lastTrackSelectPanelId = panelId;
        }

        if (_trackSelectApp == null)
            _trackSelectApp = UnityEngine.Object.FindFirstObjectByType<App>();

        string? theme = ReadTrackSelectTheme(_trackSelectApp);
        bool? extreme = ReadTrackSelectExtreme(_trackSelectUi);
        (int focusedId, string? focusedLabel) = ReadTrackSelectSelectedControl(panel);
        bool waitingToStart = _trackSelectUi.gameManager != null &&
            _trackSelectUi.gameManager.GameState == GameState.WaitingToStart;
        bool descriptionPressed = WasReadDescriptionsPressed(waitingToStart);
        if (!waitingToStart && _descriptionWasRequestedOnTrackSelect)
        {
            CancelDescriptionSpeech();
            _descriptionWasRequestedOnTrackSelect = false;
        }

        if (!_trackSelectWasVisible)
        {
            _trackSelectWasVisible = true;
            _lastTrackSelectTheme = theme;
            _lastTrackSelectExtreme = extreme;
            _lastTrackSelectFocusedId = focusedId;
            WriteStatus($"Song selection screen is visible; theme {theme ?? "unknown"}, extreme {FormatExtreme(extreme) ?? "unknown"}.");

            string introduction = "Song selection";
            if (theme != null)
                introduction += $". {theme}";
            if (extreme.HasValue)
                introduction += $". {FormatExtreme(extreme)}";
            introduction += ". Twist to change song. Pull to change difficulty. " +
                ReadDescriptionsBindingInstruction() + " Bop to start. Back to return.";
            if (focusedLabel != null && !string.Equals(focusedLabel, "Start", StringComparison.OrdinalIgnoreCase))
                introduction += $" {focusedLabel}.";
            QueueSpeech(introduction);
            return true;
        }

        string? changed = null;
        if (theme != null && !string.Equals(theme, _lastTrackSelectTheme, StringComparison.Ordinal))
        {
            changed = theme;
            _lastTrackSelectTheme = theme;
        }

        if (extreme.HasValue && extreme != _lastTrackSelectExtreme)
        {
            string state = FormatExtreme(extreme)!;
            changed = changed == null ? state : $"{changed}. {state}";
            _lastTrackSelectExtreme = extreme;
        }

        if (focusedId != _lastTrackSelectFocusedId)
        {
            _lastTrackSelectFocusedId = focusedId;
            if (focusedLabel != null)
                changed = changed == null ? focusedLabel : $"{changed}. {focusedLabel}";
        }

        if (descriptionPressed)
        {
            string? description = theme switch
            {
                "Shapes" => ShapesDescription,
                "Space" => SpaceDescription,
                "City" => CityDescription,
                "Office" => OfficeDescription,
                _ => null
            };
            if (description != null)
            {
                QueueDescriptionSpeech(description);
                _descriptionWasRequestedOnTrackSelect = true;
                WriteStatus($"Read description requested for {theme}.");
                return true;
            }
        }

        if (changed != null)
            QueueSpeech(changed);

        return true;
    }

    private static string? ReadTrackSelectTheme(App? app)
    {
        string? raw = CleanSpeechValue(app?.CurrentMusicTrack?.BackgroundSceneName);
        if (raw == null)
            return null;

        if (raw.Contains("Shapes", StringComparison.OrdinalIgnoreCase)) return "Shapes";
        if (raw.Contains("Space", StringComparison.OrdinalIgnoreCase)) return "Space";
        if (raw.Contains("City", StringComparison.OrdinalIgnoreCase)) return "City";
        if (raw.Contains("Office", StringComparison.OrdinalIgnoreCase)) return "Office";
        return raw;
    }

    private static bool? ReadTrackSelectExtreme(GameUIManager ui)
    {
        Player? player = ui.gameManager == null ? null : ui.gameManager.Player;
        if (player == null)
            player = UnityEngine.Object.FindFirstObjectByType<Player>();
        DeviceContainer? container = player?.deviceContainer;
        if (container == null)
            container = ui.startScreen?.GetComponentInChildren<PullToChangeDifficulty>(true)?.deviceContainer;
        Device? device = container?.CurrentDevice;
        if (device == null)
            return null;

        return device.Type == Il2CppBopIt.DeviceType.Extreme;
    }

    private static string? FormatExtreme(bool? extreme) =>
        extreme.HasValue ? (extreme.Value ? "Extreme mode on" : "Extreme mode off") : null;

    private (int Id, string? Label) ReadTrackSelectSelectedControl(StartScreenPanel panel)
    {
        GameObject? selected = EventSystem.current == null
            ? null : EventSystem.current.currentSelectedGameObject;
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        if (selectedId != _lastTrackSelectObservedSelectionId)
        {
            WriteStatus($"Song selection selected object: {(selected == null ? "none" : selected.name)}.");
            _lastTrackSelectObservedSelectionId = selectedId;
        }

        if (selected == null || !selected.transform.IsChildOf(panel.transform))
            return (0, null);

        Selectable? control = selected.GetComponentInParent<Selectable>();
        if (control == null || !control.transform.IsChildOf(panel.transform))
            return (0, null);

        string? label = CleanSpeechValue(control.GetComponentInChildren<TMP_Text>(true)?.text);
        if (label == null)
        {
            if (control.name.Contains("Start", StringComparison.OrdinalIgnoreCase) ||
                control.name.Contains("Play", StringComparison.OrdinalIgnoreCase))
                label = "Start";
            else if (control.name.Contains("Back", StringComparison.OrdinalIgnoreCase))
                label = "Back";
        }

        return (control.GetInstanceID(), label);
    }

    private void ResetTrackSelectFocus()
    {
        WasReadDescriptionsPressed(false);
        if (_descriptionWasRequestedOnTrackSelect)
        {
            CancelDescriptionSpeech();
            _descriptionWasRequestedOnTrackSelect = false;
        }
        _trackSelectWasVisible = false;
        _lastTrackSelectTheme = null;
        _lastTrackSelectExtreme = null;
        _lastTrackSelectFocusedId = 0;
        _lastTrackSelectObservedSelectionId = 0;
        _lastTrackSelectPanelId = 0;
    }
}
