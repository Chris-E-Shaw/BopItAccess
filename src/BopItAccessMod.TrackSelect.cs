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
    private long _trackSelectStartTransitionUntil;

    private const string ShapesDescription =
        "An abstract digital scene with no recognizable room or ground, styled like a nostalgic 1990s pizza shop turned neon DJ night. " +
        "Hot pink, purple, and teal fill the space. Squares, angular forms, rounded cubes, spheres, and pyramids mingle with floating squiggles and other geometric patterns. " +
        "The shapes drift and bounce in a playful, screensaver-like motion, making the level feel lively and rhythmic.";
    private const string SpaceDescription =
        "A bright, playful galaxy surrounds the Bop It device. Deep blue, purple, and black suggest open space, with stars, cosmic clouds, planets, and floating rocky forms adding depth. " +
        "Friendly animated aliens drift nearby and cheer the player on. Their glowing green, silver, and neon-blue accents stand out against the dark backdrop, giving the scene a cheerful sci-fi atmosphere.";
    private const string CityDescription =
        "A layered cartoon city glows at twilight, with dark blue buildings and warm orange and yellow lights. Skyscrapers, lit windows, streetlights, billboards, and playful signs create a busy downtown scene. " +
        "Some signs use music-themed lettering, including street names such as “Bop It Blvd.” and “Spin It Street.” The featured billboard cat bops and rolls as the city moves around it, adding a goofy animated focal point.";
    private const string OfficeDescription =
        "A whimsical workplace scene centers on a desk and computer. The monitor becomes an aquarium: fish swim inside it, surrounded by colorful aquatic imagery. " +
        "A keyboard, leafy plants, and small flowers add detail around the workstation.";

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

        GameManager? game = _trackSelectUi.gameManager;
        if (game != null && game.GameState == GameState.Playing)
        {
            if (_trackSelectWasVisible ||
                Environment.TickCount64 <= _trackSelectStartTransitionUntil)
            {
                StopSpeechForGameStart();
                _descriptionWasRequestedOnTrackSelect = false;
                WriteStatus("Gameplay started; cleared and interrupted song-selection speech.");
            }
            _trackSelectStartTransitionUntil = 0;
            ResetTrackSelectFocus();
            return false;
        }

        StartScreenPanel? panel = _trackSelectUi.startScreen;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            // The start panel can close a frame before GameState becomes
            // Playing. Remember that visit through the transition.
            if (_trackSelectWasVisible && game != null &&
                game.GameState == GameState.WaitingToStart)
                _trackSelectStartTransitionUntil = Environment.TickCount64 + 10000;
            ResetTrackSelectFocus();
            return false;
        }

        _trackSelectStartTransitionUntil = 0;

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
        (int focusedId, string? focusedLabel, string? focusedType,
            int focusedIndex, int focusedCount) =
            ReadTrackSelectSelectedControl(panel);
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
            if (focusedLabel != null && (_indexingEnabled || _readControlTypesEnabled ||
                !string.Equals(focusedLabel, "Start", StringComparison.OrdinalIgnoreCase)))
            {
                string focusedControl = WithControlType(focusedLabel,
                    focusedType ?? "button");
                introduction += " " + WithMenuIndex(focusedControl,
                    focusedIndex, focusedCount) + ".";
            }
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
            {
                string indexedLabel = WithMenuIndex(WithControlType(focusedLabel,
                    focusedType ?? "button"), focusedIndex, focusedCount);
                changed = changed == null ? indexedLabel : $"{changed}. {indexedLabel}";
            }
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

    private (int Id, string? Label, string? Type, int Index, int Count)
        ReadTrackSelectSelectedControl(
        StartScreenPanel panel)
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
            return (0, null, null, -1, 0);

        Selectable? control = selected.GetComponentInParent<Selectable>();
        if (control == null || !control.transform.IsChildOf(panel.transform))
            return (0, null, null, -1, 0);

        string? label = ReadTrackSelectControlLabel(control);
        if (label == null)
            return (control.GetInstanceID(), null, null, -1, 0);

        int index = -1;
        int count = 0;
        int focusedId = control.GetInstanceID();
        foreach (Selectable candidate in panel.GetComponentsInChildren<Selectable>(true))
        {
            if (!candidate.gameObject.activeInHierarchy || !candidate.interactable ||
                ReadTrackSelectControlLabel(candidate) == null)
                continue;

            if (candidate.GetInstanceID() == focusedId)
                index = count;
            count++;
        }

        return (focusedId, label, ReadTrackSelectControlType(control), index, count);
    }

    private static string ReadTrackSelectControlType(Selectable control)
    {
        if (control is Toggle) return "toggle";
        if (control is Slider || control is Scrollbar) return "slider";
        if (control is Dropdown || control is TMP_Dropdown) return "dropdown";
        if (control is InputField || control is TMP_InputField) return "text field";
        return "button";
    }

    private static string? ReadTrackSelectControlLabel(Selectable control)
    {
        string? label = CleanSpeechValue(control.GetComponentInChildren<TMP_Text>(true)?.text);
        if (label != null)
            return label;
        if (control.name.Contains("Start", StringComparison.OrdinalIgnoreCase) ||
            control.name.Contains("Play", StringComparison.OrdinalIgnoreCase))
            return "Start";
        if (control.name.Contains("Back", StringComparison.OrdinalIgnoreCase))
            return "Back";
        return null;
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
