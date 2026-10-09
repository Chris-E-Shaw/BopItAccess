using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BopItAccess.BopItAccessMod), "Bop It Access", "0.9.13", "Bop It Access project")]

namespace BopItAccess;

[SupportedOSPlatform("windows")]
public sealed partial class BopItAccessMod : MelonMod
{
    private readonly ManualResetEventSlim _shutdownRequested = new(false);
    private readonly AutoResetEvent _speechRequested = new(false);
    private readonly object _speechLock = new();
    private Thread? _speechThread;
    private volatile bool _modStopping;
    private long _nextSpeechWorkerErrorAt;
    private string? _pendingSpeech;
    private long _pendingSpeechQueuedAt;
    private string? _pendingPrioritySpeech;
    private string? _pendingPriorityFollowUpSpeech;
    private readonly Queue<string> _sequentialSpeech = new();
    private bool _pendingSpeechInterrupt = true;
    private bool _pendingSpeechIsDescription;
    private bool _descriptionSpeechMayBeActive;
    private bool _silenceRequested;
    private long _speechGeneration;
    private MainMenuUIManager? _mainMenu;
    private SettingsPanel? _settingsPanel;
    private SettingOption[]? _settingsOptions;
    private int _lastFocusedButtonId;
    private string? _lastFocusedMenuLabel;
    private int _lastFocusedSettingRowId;
    private int _lastObservedSettingsSelectionId;
    private string? _lastSettingsValue;
    private long _nextMenuSearchAt;
    private long _nextSettingsSearchAt;
    private long _nextFocusErrorLogAt;
    private long _nextModalFocusErrorLogAt;
    private long _nextSettingsErrorLogAt;
    private bool _mainMenuWasVisible;
    private bool _settingsWasVisible;
    private static readonly object StatusLogLock = new();
    private static readonly Regex TmpTagPattern = new("<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex WhitespacePattern = new("\\s+", RegexOptions.Compiled);

    private static readonly string StatusLogPath = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty) ??
        Environment.CurrentDirectory, "Mods", "BopItAccess.log");

    public override void OnInitializeMelon()
    {
        Volatile.Write(ref _activeMenuSelectionAudioMod, this);
        PrepareFirstRunNativeAudioDefaults();
        PrepareSettingsConfig();
        WriteStatus("Mod loaded; starting the Prism speech thread.");
        MelonLogger.Msg("Starting Prism speech and braille support on a background thread...");
        _speechThread = new Thread(InitializePrismAndAnnounce)
        {
            IsBackground = true,
            Name = "Bop It Access Prism"
        };

        try
        {
            _speechThread.SetApartmentState(ApartmentState.STA);
            _speechThread.Start();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Could not start the Prism speech thread: {ex}");
        }
    }

    public override void OnLateUpdate()
    {
        if (_modStopping)
            return;
        try
        {
            UpdateGameLocale();
            ReadScreenFocus();
        }
        finally
        {
            FinishMenuInputDiagnosticFrame();
            // Focus can move after OnUpdate. Dispatch due hints only after
            // the frame's focus and value announcements have reset the timer.
            UpdateRepeatButtonHints();
            UpdateSettingsConfigExport();
        }
    }

    private void ReadScreenFocus()
    {
        ObserveResultRank();

        // These mod panels sit above the native main menu and must own
        // focus before any underlying game panel can announce itself.
        try
        {
            bool welcomePending = EnsureWelcomeBeforeMainMenuSpeech();
            if (ReadGuideFocus() || ReadWelcomeScreenFocus() || welcomePending)
            {
                ResetUncoveredPanelFocus();
                ResetSettingsFocus();
                ResetMenuFocus();
                return;
            }
        }
        catch (Exception ex)
        {
            LogModalFocusError("Guide or welcome", ex);
        }

        bool titleVisible = false;
        try
        {
            titleVisible = ReadTitleScreen();
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextTitleScreenErrorLogAt)
            {
                WriteStatus($"Title screen speech check failed: {ex}");
                MelonLogger.Warning($"Title screen speech check failed: {ex.Message}");
                _nextTitleScreenErrorLogAt = now + 5000;
            }
            _titleScreen = null;
            _lastTitleScreenId = 0;
        }
        if (titleVisible)
        {
            ResetUncoveredPanelFocus();
            ResetSettingsFocus();
            ResetMenuFocus();
            return;
        }

        bool speechMenuVisible = false;
        try
        {
            speechMenuVisible = ReadSpeechMenuFocus();
        }
        catch (Exception ex)
        {
            LogModalFocusError("Mod Settings", ex);
            ResetSpeechMenuFocus();
        }
        if (speechMenuVisible)
        {
            ResetUncoveredPanelFocus();
            ResetSettingsFocus();
            ResetMenuFocus();
            return;
        }

        bool panelClaimed = false;
        foreach (FocusRoute route in FocusRoutes)
        {
            if (panelClaimed)
            {
                route.Reset();
                continue;
            }
            try
            {
                panelClaimed = route.Read();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= route.NextErrorLogAt)
                {
                    WriteStatus(route.Name + " speech check failed: " + ex);
                    MelonLogger.Warning(route.Name + " speech check failed: " + ex.Message);
                    route.NextErrorLogAt = now + 5000;
                }
                route.Recover?.Invoke();
                route.Reset();
            }
        }
        if (panelClaimed)
        {
            ResetUncoveredPanelFocus();
            ResetSettingsFocus();
            ResetMenuFocus();
            return;
        }

        bool settingsVisible = false;
        try
        {
            settingsVisible = ReadSettingsFocus();
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextSettingsErrorLogAt)
            {
                WriteStatus($"Settings focus check failed: {ex}");
                MelonLogger.Warning($"Settings focus check failed: {ex.Message}");
                _nextSettingsErrorLogAt = now + 5000;
            }

            _settingsPanel = null;
            _settingsOptions = null;
            ResetSettingsFocus();
            _nextSettingsSearchAt = now + 1000;
        }

        if (settingsVisible)
            ResetUncoveredPanelFocus();
        else
        {
            try
            {
                if (ReadUncoveredPanelFocus())
                {
                    ResetMenuFocus();
                    return;
                }
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextUncoveredPanelErrorLogAt)
                {
                    WriteStatus($"Additional panel speech check failed: {ex}");
                    MelonLogger.Warning($"Additional panel speech check failed: {ex.Message}");
                    _nextUncoveredPanelErrorLogAt = now + 5000;
                }
                ResetUncoveredPanelFocus();
            }
        }

        try
        {
            if (settingsVisible)
                ResetMenuFocus();
            else
                ReadMainMenuFocus();
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextFocusErrorLogAt)
            {
                WriteStatus($"Main menu focus check failed: {ex}");
                MelonLogger.Warning($"Main menu focus check failed: {ex.Message}");
                _nextFocusErrorLogAt = now + 5000;
            }

            _mainMenu = null;
            ResetMenuFocus();
            _nextMenuSearchAt = now + 1000;
        }
    }

    private void LogModalFocusError(string name, Exception error)
    {
        long now = Environment.TickCount64;
        if (now < _nextModalFocusErrorLogAt)
            return;
        _nextModalFocusErrorLogAt = now + 5000;
        WriteStatus(name + " focus check failed: " + error);
    }

    private sealed class FocusRoute
    {
        internal readonly string Name;
        internal readonly Func<bool> Read;
        internal readonly Action Reset;
        internal readonly Action? Recover;
        internal long NextErrorLogAt;
        internal FocusRoute(string name, Func<bool> read, Action reset,
            Action? recover = null) => (Name, Read, Reset, Recover) =
                (name, read, reset, recover);
    }

    private FocusRoute[]? _focusRoutes;
    // Cache delegates once. Preserve native panel priority and reset readers
    // hidden by a higher-priority panel so the next visit announces afresh.
    private FocusRoute[] FocusRoutes => _focusRoutes ??= new[]
    {
        new FocusRoute("Audio calibration", ReadCalibrationFocus, ResetCalibrationFocus),
        new FocusRoute("Controls", ReadControlsFocus, ResetControlsFocus),
        new FocusRoute("Leaderboard", ReadLeaderboardsFocus, ResetLeaderboardsFocus,
            () => { _leaderboardMainMenu = null; _leaderboardGameUi = null; }),
        new FocusRoute("Achievements", ReadAchievementsFocus, ResetAchievementsFocus,
            () => _achievementsPanel = null),
        new FocusRoute("Credits", ReadCreditsFocus, ResetCreditsFocus),
        new FocusRoute("Pause menu", ReadPauseMenuFocus, ResetPauseMenuFocus,
            () => _pauseMenuPanel = null),
        new FocusRoute("Game over", ReadGameOverFocus, ResetGameOverFocus,
            () => { _gameOverUi = null; _gameOverApp = null; }),
        new FocusRoute("Song selection", ReadTrackSelectFocus, ResetTrackSelectFocus,
            () => { _trackSelectUi = null; _trackSelectApp = null; }),
        new FocusRoute("Play mode", ReadPlayModesFocus, ResetPlayModesFocus)
    };

    private bool ReadSettingsFocus()
    {
        if (_settingsPanel == null)
        {
            long now = Environment.TickCount64;
            if (now < _nextSettingsSearchAt)
                return false;

            _nextSettingsSearchAt = now + 500;
            _settingsPanel = UnityEngine.Object.FindFirstObjectByType<SettingsPanel>();
            if (_settingsPanel == null)
                return false;

            WriteStatus("Found the settings panel; waiting for settings focus.");
            ResetSettingsFocus();
        }

        if (!_settingsPanel.IsVisible || !_settingsPanel.gameObject.activeInHierarchy)
        {
            ResetSettingsFocus();
            return false;
        }

        if (!_settingsWasVisible)
        {
            // Serialized row references are guaranteed to be ready by the time
            // the panel becomes visible, even if we found it earlier in a scene.
            // The added rows normally probe on 250 ms timers in OnUpdate. If
            // Settings opens between probes, the first focused item would be
            // announced with only the native row count. Add them now, before
            // taking the option snapshot used for this first announcement.
            EnsureCustomSettingsRowsBeforeFocus(_settingsPanel);
            _settingsOptions = CreateSettingsOptions(_settingsPanel);
            WriteStatus("Settings panel is visible; monitoring its settings rows.");
            _settingsWasVisible = true;
        }

        EventSystem? eventSystem = EventSystem.current;
        GameObject? selected = eventSystem == null ? null : eventSystem.currentSelectedGameObject;
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        if (selectedId != _lastObservedSettingsSelectionId)
        {
            WriteStatus($"Settings selected object: {(selected == null ? "none" : selected.name)}.");
            _lastObservedSettingsSelectionId = selectedId;
        }

        SettingOption? focused = GetFocusedSettingsOption(selected);
        if (focused == null)
        {
            _lastFocusedSettingRowId = 0;
            _lastSettingsValue = null;
            return true;
        }

        string? value = focused.ReadValue();
        if (string.Equals(value, focused.Label, StringComparison.OrdinalIgnoreCase))
            value = null;

        if (focused.Id != _lastFocusedSettingRowId)
        {
            _lastFocusedSettingRowId = focused.Id;
            _lastSettingsValue = value;
            string label = WithControlType(
                ReadSettingRowLabel(focused.Row, focused.Label),
                focused.ControlType);
            string announcement = value == null ? label : $"{label}, {value}";
            announcement = WithSliderRange(announcement, focused.Label,
                focused.ControlType);
            int index = -1;
            int count = 0;
            foreach (SettingOption option in _settingsOptions!)
            {
                if (option.Row == null || !option.Row.gameObject.activeInHierarchy)
                    continue;
                if (option.Id == focused.Id)
                    index = count;
                count++;
            }
            QueueFocusSpeech(WithMenuIndex(announcement, index, count));
            return true;
        }

        if (value != null && !string.Equals(value, _lastSettingsValue, StringComparison.Ordinal))
        {
            _lastSettingsValue = value;
            RecordButtonHintUiActivity(Environment.TickCount64);
            QueueSpeech(value);
        }

        return true;
    }

    private void EnsureCustomSettingsRowsBeforeFocus(SettingsPanel panel)
    {
        int panelId = panel.GetInstanceID();
        if (_backgroundAudioToggle == null ||
            _backgroundAudioSettingsPanelId != panelId)
        {
            _nextBackgroundAudioSettingsProbeAt = 0;
            UpdateBackgroundAudioSettingsRow();
        }

        if (_fpsSettingsSlider == null || _fpsSettingsPanelId != panelId)
        {
            _nextFpsSettingsProbeAt = 0;
            UpdateFpsLimitSetting();
        }

        if (_speechSettingsButton == null || _speechSettingsPanelId != panelId)
        {
            _nextSpeechSettingsProbeAt = 0;
            UpdateSpeechMenuUi();
        }
    }

    private SettingOption? GetFocusedSettingsOption(GameObject? selected)
    {
        if (_settingsOptions == null)
            return null;

        SettingsRow? selectedRow = selected == null ? null : selected.GetComponentInParent<SettingsRow>();
        int selectedRowId = selectedRow == null ? 0 : selectedRow.GetInstanceID();
        if (selectedRowId != 0)
        {
            foreach (SettingOption option in _settingsOptions)
            {
                if (option.Id == selectedRowId && option.Row != null && option.Row.gameObject.activeInHierarchy)
                    return option;
            }
        }

        // Some custom settings controls move focus inside a row without selecting
        // the row GameObject itself. The game's own focus flag covers that case.
        if (selected != null && !selected.transform.IsChildOf(_settingsPanel!.transform))
            return null;

        foreach (SettingOption option in _settingsOptions)
        {
            if (option.Row != null && option.Row.gameObject.activeInHierarchy && option.Row.IsFocused)
                return option;
        }

        return null;
    }

    private SettingOption[] CreateSettingsOptions(SettingsPanel panel)
    {
        GoOnlineButton? goOnlineController = panel.GetComponentInChildren<GoOnlineButton>(true);
        SettingsButton? goOnline = goOnlineController == null ? null : goOnlineController.button;

        return new[]
        {
            SliderOption("MUSIC", panel.music),
            SliderOption("SFX", panel.sfx),
            SliderOption("VOICE OVER", panel.voiceOver),
            ToggleOption("MUTE AUDIO IN BACKGROUND", _backgroundAudioToggle),
            SliderOption("LANGUAGE", panel.language),
            ToggleOption("VIBRATION", panel.vibration),
            ToggleOption("FULLSCREEN", panel.fullscreen),
            SliderOption("RESOLUTION", panel.resolution),
            SliderOption("LIMIT FPS", _fpsSettingsSlider),
            ActionOption("AUDIO LATENCY", panel.audioLatency),
            ActionOption("CONTROLS", panel.controls),
            ActionOption("MOD SETTINGS", _speechSettingsButton),
            ActionOption("GO ONLINE", goOnline)
        };
    }

    private static SettingOption SliderOption(string label, SettingsSlider? row) =>
        new(label, "slider", row,
            () => row == null ? null : ReadDisplayedValue(row, row.Value));

    private static SettingOption ToggleOption(string label, SettingsToggle? row) =>
        new(label, "toggle", row, () => row == null ? null :
            row.IsOn ? L("On") : L("Off"));

    private static SettingOption ActionOption(string label, SettingsButton? row) =>
        new(label, "button", row, () => null);

    private static string ReadSettingRowLabel(SettingsRow? row, string fallback)
    {
        if (row != null)
        {
            TMP_Text? active = FindFpsLabel(row.ActiveContainer,
                row.ActiveValueText);
            TMP_Text? regular = FindFpsLabel(row.DefaultContainer,
                row.ValueText);
            string? rendered = CleanSpeechValue(active?.text) ??
                CleanSpeechValue(regular?.text);
            if (rendered != null && (CurrentGameLocale == "en" ||
                !string.Equals(rendered, fallback,
                    StringComparison.OrdinalIgnoreCase)))
                return rendered;
        }
        return L(fallback);
    }

    private static string? ReadDisplayedValue(SettingsRow row, string? gameValue)
    {
        // Prefer the text actually drawn in the focused row. The scene's Value
        // objects start blank and are filled by the game at runtime.
        var activeText = row.ActiveValueText;
        string? value = activeText == null ? null : CleanSpeechValue(activeText.text);
        if (value != null)
            return value;

        var regularText = row.ValueText;
        value = regularText == null ? null : CleanSpeechValue(regularText.text);
        return value ?? CleanSpeechValue(gameValue);
    }

    private static string? CleanSpeechValue(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        string withoutTags = TmpTagPattern.Replace(raw, string.Empty);
        string value = WhitespacePattern.Replace(withoutTags, " ").Trim();
        return value.Length == 0 || value == "--" ? null : value;
    }

    private void ResetSettingsFocus()
    {
        _lastFocusedSettingRowId = 0;
        _lastObservedSettingsSelectionId = 0;
        _lastSettingsValue = null;
        _settingsWasVisible = false;
    }

    private sealed class SettingOption
    {
        private readonly Func<string?> _readValue;

        internal SettingOption(string label, string controlType, SettingsRow? row,
            Func<string?> readValue)
        {
            Label = label;
            ControlType = controlType;
            Row = row;
            _readValue = readValue;
            Id = row == null ? 0 : row.GetInstanceID();
        }

        internal string Label { get; }
        internal string ControlType { get; }
        internal SettingsRow? Row { get; }
        internal int Id { get; }
        internal string? ReadValue() => _readValue();
    }

    private void ReadMainMenuFocus()
    {
        if (_mainMenu == null)
        {
            long now = Environment.TickCount64;
            if (now < _nextMenuSearchAt)
                return;

            _nextMenuSearchAt = now + 500;
            _mainMenu = UnityEngine.Object.FindFirstObjectByType<MainMenuUIManager>();
            if (_mainMenu == null)
                return;

            WriteStatus("Found the main menu manager; waiting for menu focus.");
            ResetMenuFocus();
        }

        Panel? panel = _mainMenu.mainMenuPanel;
        if (panel == null || !panel.IsVisible || !panel.gameObject.activeInHierarchy)
        {
            ResetMenuFocus();
            return;
        }

        if (!_mainMenuWasVisible)
        {
            WriteStatus("Main menu is visible; monitoring the six menu buttons.");
            _mainMenuWasVisible = true;
        }

        EventSystem? eventSystem = EventSystem.current;
        GameObject? selected = eventSystem == null ? null : eventSystem.currentSelectedGameObject;
        Button? focusedButton = selected == null ? null : selected.GetComponentInParent<Button>();
        if (focusedButton == null)
        {
            _lastFocusedButtonId = 0;
            _lastFocusedMenuLabel = null;
            return;
        }

        int focusedButtonId = focusedButton.GetInstanceID();
        string? label = GetMainMenuLabel(focusedButtonId);
        if (label == null)
        {
            _lastFocusedButtonId = 0;
            _lastFocusedMenuLabel = null;
            return;
        }

        if (focusedButtonId == _lastFocusedButtonId &&
            string.Equals(label, _lastFocusedMenuLabel, StringComparison.Ordinal))
            return;

        WriteStatus($"Main menu selected object: {(selected == null ? "none" : selected.name)}.");
        _lastFocusedButtonId = focusedButtonId;
        _lastFocusedMenuLabel = label;
        Button?[] menuButtons =
        {
            _mainMenu!.playButton, _mainMenu.leaderboardButton,
            _mainMenu.achievementsButton, _mainMenu.settingsButton,
            _mainMenu.creditsButton, _mainMenu.quitButton
        };
        int index = -1;
        int count = 0;
        foreach (Button? button in menuButtons)
        {
            if (button == null || !button.gameObject.activeInHierarchy)
                continue;
            if (button.GetInstanceID() == focusedButtonId)
                index = count;
            count++;
        }
        string announcement = WithMenuIndex(WithControlType(label, "button"), index, count);
        if (_welcomeRecoveryIntro != null)
        {
            announcement = _welcomeRecoveryIntro + " " + announcement;
            _welcomeRecoveryIntro = null;
        }
        QueueFocusSpeech(announcement);
    }

    private string? GetMainMenuLabel(int focusedButtonId)
    {
        if (Matches(_mainMenu!.playButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.playButton, "PLAY");
        if (Matches(_mainMenu.leaderboardButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.leaderboardButton, "LEADERBOARDS");
        if (Matches(_mainMenu.achievementsButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.achievementsButton, "ACHIEVEMENTS");
        if (Matches(_mainMenu.settingsButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.settingsButton, "SETTINGS");
        if (Matches(_mainMenu.creditsButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.creditsButton, "CREDITS");
        if (Matches(_mainMenu.quitButton, focusedButtonId))
            return ReadNativeButtonLabel(_mainMenu.quitButton, "QUIT");
        return null;
    }

    private static string ReadNativeButtonLabel(Button? button, string fallback)
    {
        if (button != null)
            foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(true))
                if (text.gameObject.activeInHierarchy)
                {
                    string? label = CleanSpeechValue(text.text);
                    if (label != null)
                        return CurrentGameLocale != "en" &&
                            string.Equals(label, fallback,
                                StringComparison.OrdinalIgnoreCase)
                            ? L(fallback) : label;
                }
        return L(fallback);
    }

    private static bool Matches(Button? button, int focusedButtonId) =>
        button != null && button.GetInstanceID() == focusedButtonId;

    private void ResetMenuFocus()
    {
        _lastFocusedButtonId = 0;
        _lastFocusedMenuLabel = null;
        _mainMenuWasVisible = false;
    }

    private void QueueSpeech(string text, bool interrupt = true)
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            // Keep the newest focus announcement when navigation is faster than speech.
            _pendingSpeech = text;
            _pendingSpeechQueuedAt = Environment.TickCount64;
            _pendingSpeechInterrupt = interrupt;
            _pendingSpeechIsDescription = false;
            if (interrupt)
            {
                _sapiRenderSerial++;
                _sequentialSpeech.Clear();
            }
        }

        _speechRequested.Set();
    }

    private void QueueDescriptionSpeech(string text)
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            _pendingSpeech = text;
            _pendingSpeechQueuedAt = Environment.TickCount64;
            _pendingSpeechInterrupt = true;
            _pendingSpeechIsDescription = true;
            _sapiRenderSerial++;
            _sequentialSpeech.Clear();
        }

        _speechRequested.Set();
    }

    private void CancelDescriptionSpeech()
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            bool hadDescription = _pendingSpeechIsDescription ||
                _descriptionSpeechMayBeActive;
            if (_pendingSpeechIsDescription)
            {
                _pendingSpeech = null;
                _pendingSpeechQueuedAt = 0;
                _pendingSpeechIsDescription = false;
            }

            // Prism is used only by its worker thread. Silence a description
            // that has already reached the screen reader, while preserving a
            // later score or menu announcement if one has been dispatched.
            if (_descriptionSpeechMayBeActive)
                _silenceRequested = true;
            _descriptionSpeechMayBeActive = false;
            if (hadDescription)
                _sapiRenderSerial++;
        }

        _speechRequested.Set();
    }

    private void StopSpeechForGameStart()
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            // A round has started or resumed. Drop every unsent menu, hint,
            // description and result, and silence already dispatched speech.
            _pendingSpeech = null;
            _pendingSpeechQueuedAt = 0;
            _pendingPrioritySpeech = null;
            _pendingPriorityFollowUpSpeech = null;
            _pendingSpeechIsDescription = false;
            _sequentialSpeech.Clear();
            _descriptionSpeechMayBeActive = false;
            // A just-enabled status message must not mask the game's verbal
            // cues if Bop starts play immediately afterward.
            _pendingToggleSpeechNotice = null;
            Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
            Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
            _silenceRequested = true;
            _speechGeneration++;
            _sapiRenderSerial++;
        }

        _speechRequested.Set();
    }

    private void QueueSequentialSpeech(string text)
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            _sequentialSpeech.Enqueue(text);
        }

        _speechRequested.Set();
    }

    private void QueueScoreThenMenu(string score, string? menu)
    {
        if (_shutdownRequested.IsSet || !_speechEnabled ||
            _speechSuppressedForBackground)
            return;

        lock (_speechLock)
        {
            if (!_speechEnabled || _speechSuppressedForBackground)
                return;
            // The score has its own slot, so a focus change cannot replace it
            // before the Prism worker picks up the request.
            _pendingPrioritySpeech = score;
            _pendingPriorityFollowUpSpeech = menu;
            _pendingSpeech = null;
            _pendingSpeechQueuedAt = 0;
            _pendingSpeechInterrupt = false;
            _pendingSpeechIsDescription = false;
            _sapiRenderSerial++;
            _sequentialSpeech.Clear();
        }

        _speechRequested.Set();
    }

    private void InitializePrismAndAnnounce()
    {
        try
        {
            while (!_shutdownRequested.IsSet)
            {
                try
                {
                    InitializePrismOnWorker();
                    break;
                }
                catch (Exception ex)
                {
                    ReportSpeechWorkerError("Prism initialization failed; retrying in five seconds", ex);
                    if (_shutdownRequested.Wait(5000))
                        return;
                }
            }
            if (_shutdownRequested.IsSet)
                return;

            // PlayerPrefs and Input Actions must be read on Unity's thread.
            // Wait for the first Update so the persisted OFF state and its
            // current recovery bindings are known before speaking anything.
            WaitHandle[] startupSignals =
                { _shutdownRequested.WaitHandle, _startupSpeechReady.WaitHandle };
            if (WaitHandle.WaitAny(startupSignals) != 1)
                return;

            string startupAnnouncement;
            long startupGeneration;
            lock (_speechLock)
            {
                startupAnnouncement = _pendingToggleSpeechNotice ??
                    _startupSpeechAnnouncement;
                startupGeneration = _speechGeneration;
                if (_pendingToggleSpeechNotice != null)
                    _silenceRequested = false;
                _pendingToggleSpeechNotice = null;
            }

            // A toggle can happen between preparing startup text and sending
            // it. Let the normal worker loop announce the newer state instead.
            if (startupGeneration == Interlocked.Read(ref _speechGeneration) &&
                !_speechSuppressedForBackground)
            {
                bool queued = false;
                try { queued = OutputSpeechOnWorker(startupAnnouncement, true); }
                catch (Exception ex) { ReportSpeechWorkerError("Startup announcement failed", ex); }
                if (queued)
                {
                    WriteStatus($"Prism accepted the startup announcement '{startupAnnouncement}'.");
                    MelonLogger.Msg("Prism accepted the startup announcement.");
                }
                else
                {
                    WriteStatus("Prism could not dispatch the startup announcement.");
                    MelonLogger.Warning("Prism could not dispatch the startup announcement.");
                }
            }

            try
            {
                PreparePrismBackendOnWorker();
                CaptureConfigVoiceChoicesOnWorker();
            }
            catch (Exception ex)
            {
                ReportSpeechWorkerError("Speech backend preparation failed", ex);
            }

            WaitHandle[] signals = { _shutdownRequested.WaitHandle, _speechRequested };
            while (WaitHandle.WaitAny(signals) != 0)
            {
                string? priority = null;
                bool priorityDispatchCompleted = false;
                try
                {
                    string? priorityFollowUp;
                    string? announcement;
                    long announcementQueuedAt;
                    List<string>? sequential;
                    bool interrupt;
                    bool silence;
                    bool description;
                    string? toggleNotice;
                    long generation;
                    lock (_speechLock)
                    {
                        generation = _speechGeneration;
                        silence = _silenceRequested;
                        _silenceRequested = false;
                        toggleNotice = _pendingToggleSpeechNotice;
                        _pendingToggleSpeechNotice = null;
                        priority = _pendingPrioritySpeech;
                        _pendingPrioritySpeech = null;
                        priorityFollowUp = _pendingPriorityFollowUpSpeech;
                        _pendingPriorityFollowUpSpeech = null;
                        announcement = _pendingSpeech;
                        announcementQueuedAt = _pendingSpeechQueuedAt;
                        _pendingSpeech = null;
                        _pendingSpeechQueuedAt = 0;
                        description = _pendingSpeechIsDescription;
                        _pendingSpeechIsDescription = false;
                        interrupt = _pendingSpeechInterrupt;
                        sequential = _sequentialSpeech.Count == 0
                            ? null : new List<string>(_sequentialSpeech);
                        _sequentialSpeech.Clear();
                        if (priority != null || priorityFollowUp != null ||
                            (announcement != null && !description) || sequential != null)
                            _descriptionSpeechMayBeActive = false;
                        else if (description)
                            _descriptionSpeechMayBeActive = true;
                    }

                    if (silence)
                    {
                        bool silenced = SilenceOutputOnWorker();
                        WriteStatus($"Stopped screen-reader speech: {silenced}.");
                    }

                    // An output-mode change must refresh the effective backend
                    // even when speech is disabled or background-muted. The menu
                    // reads this worker-published capability snapshot.
                    string requestedOutputMode;
                    lock (_speechLock)
                        requestedOutputMode = _outputMode;
                    if (!string.Equals(_prismSelectedMode, requestedOutputMode,
                            StringComparison.OrdinalIgnoreCase))
                        ResolvePrismSpeechBackendOnWorker(requestedOutputMode, force: true);

                    if (toggleNotice != null &&
                        generation == Interlocked.Read(ref _speechGeneration))
                    {
                        bool accepted = OutputSpeechOnWorker(toggleNotice, true);
                        WriteStatus($"Speech toggle notice '{toggleNotice}' {(accepted ? "accepted" : "rejected")} by output backend.");
                    }

                    if (priority != null)
                    {
                        bool scoreAccepted = _speechEnabled &&
                            !_speechSuppressedForBackground &&
                            generation == Interlocked.Read(ref _speechGeneration) &&
                            OutputSpeechOnWorker(priority, true,
                                protectedCapture: true);
                        CompleteGameOverScoreSpeechDispatch(priority, scoreAccepted,
                            generation);
                        priorityDispatchCompleted = true;
                        WriteStatus($"Priority speech announcement '{priority}' {(scoreAccepted ? "accepted" : "rejected")} by output backend.");
                    }

                    if (priorityFollowUp != null && _speechEnabled &&
                        !_speechSuppressedForBackground &&
                        generation == Interlocked.Read(ref _speechGeneration))
                    {
                        bool menuAccepted = OutputSpeechOnWorker(priorityFollowUp, false);
                        WriteStatus($"Queued menu announcement '{priorityFollowUp}' {(menuAccepted ? "accepted" : "rejected")} by output backend.");
                    }

                    if (announcement != null && _speechEnabled &&
                        !_speechSuppressedForBackground &&
                        generation == Interlocked.Read(ref _speechGeneration))
                    {
                        // A newer focus request may have arrived while a priority
                        // score or its follow-up occupied the worker. The normal
                        // pending slot is latest-wins, including this brief gap
                        // between dequeue and dispatch.
                        bool superseded;
                        lock (_speechLock)
                            superseded = _pendingSpeech != null;
                        if (!superseded)
                        {
                            // A score sent in this batch always goes first. The
                            // following menu speech is queued without interruption.
                            bool followUpInterrupt = toggleNotice == null &&
                                priority == null && priorityFollowUp == null && interrupt;
                            long dispatchStartedAt = Environment.TickCount64;
                            bool accepted = OutputSpeechOnWorker(announcement, followUpInterrupt);
                            if (accepted)
                                LogPrismQueueDelayOnWorker(announcementQueuedAt, dispatchStartedAt);
                            WriteStatus($"Speech announcement '{announcement}' (interrupt {followUpInterrupt}) {(accepted ? "accepted" : "rejected")} by output backend.");
                        }
                        else if (description)
                        {
                            lock (_speechLock)
                                _descriptionSpeechMayBeActive = false;
                        }
                    }

                    if (sequential != null)
                    {
                        foreach (string line in sequential)
                        {
                            if (!_speechEnabled || _speechSuppressedForBackground ||
                                generation != Interlocked.Read(ref _speechGeneration))
                                break;
                            bool accepted = OutputSpeechOnWorker(line, false);
                            WriteStatus($"Queued sequential announcement '{line}' {(accepted ? "accepted" : "rejected")} by output backend.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // A backend error must not end all speech for this session.
                    // Release a failed automatic-score hold so later menus can speak.
                    lock (_speechLock)
                    {
                        if (priority != null && !priorityDispatchCompleted &&
                            _pendingPrioritySpeech == null)
                        {
                            Volatile.Write(ref _gameOverScoreDispatchPendingUntil, 0);
                            Volatile.Write(ref _gameOverScoreSpeechProtectedUntil, 0);
                        }
                        _descriptionSpeechMayBeActive = false;
                    }
                    ReportSpeechWorkerError("Speech dispatch failed; continuing with the next request", ex);
                }
            }
        }
        catch (Exception ex)
        {
            WriteStatus($"Prism speech worker failed: {ex}");
            MelonLogger.Error($"Prism speech worker failed: {ex}");
        }
        finally
        {
            try { ShutdownPrismOnWorker(); }
            catch (Exception ex) { WriteStatus("Prism shutdown failed: " + ex.Message); }
            try { ReleaseSapiOnWorker(); }
            catch (Exception ex) { WriteStatus("SAPI shutdown failed: " + ex.Message); }
        }
    }

    private void ReportSpeechWorkerError(string operation, Exception error)
    {
        long now = Environment.TickCount64;
        if (now < _nextSpeechWorkerErrorAt)
            return;
        _nextSpeechWorkerErrorAt = now + 5000;
        WriteStatus(operation + ": " + error);
        MelonLogger.Error(operation + ": " + error.Message);
    }

    public override void OnDeinitializeMelon()
    {
        if (_modStopping)
            return;
        _modStopping = true;
        // Signal before calling external code: a cleanup exception must never
        // leave the worker running or allow later Unity callbacks to queue work.
        _shutdownRequested.Set();
        lock (_speechLock)
        {
            _speechGeneration++;
            _sapiRenderSerial++;
            _pendingSpeech = null;
            _pendingPrioritySpeech = null;
            _pendingPriorityFollowUpSpeech = null;
            _pendingToggleSpeechNotice = null;
            _sequentialSpeech.Clear();
        }
        _speechRequested.Set();
        ClearNativeHookOwners();
        Interlocked.CompareExchange(ref _activeWelcomeMod, null, this);
        RunShutdownCleanup("menu selection audio", StopMenuSelectionAudio);
        RunShutdownCleanup("settings save", () => FlushSettingsConfig(force: true));
        RunShutdownCleanup("result listener", DetachResultRankListener);
        RunShutdownCleanup("input resources", CancelAndReleaseOwnedControlResources);
        RunShutdownCleanup("guide music filter", () => SetGuideMusicFilter(false));
        RunShutdownCleanup("background audio", StopBackgroundAudio);
        // Native output may be blocked inside a third-party driver. Do not
        // hang game exit, or release its context from the wrong thread.
        if (_speechThread?.IsAlive == true && !_speechThread.Join(500))
            WriteStatus("Speech worker is still finishing native shutdown.");
        WriteStatus("Mod shutdown requested.");
    }

    private static void RunShutdownCleanup(string name, Action cleanup)
    {
        try { cleanup(); }
        catch (Exception ex) { WriteStatus("Shutdown " + name + " failed: " + ex.Message); }
    }

    private const long MaximumStatusLogBytes = 8 * 1024 * 1024;
    private static void WriteStatus(string message)
    {
        try
        {
            lock (StatusLogLock)
            {
                string folder = Path.GetDirectoryName(StatusLogPath)!;
                Directory.CreateDirectory(folder);
                // Avoid following a substituted log file or Mods directory.
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0 ||
                    IsLinkedStatusFile(StatusLogPath))
                    return;
                if (File.Exists(StatusLogPath) &&
                    new FileInfo(StatusLogPath).Length >= MaximumStatusLogBytes)
                {
                    string previous = StatusLogPath + ".previous";
                    if (IsLinkedStatusFile(previous))
                        return;
                    File.Move(StatusLogPath, previous, overwrite: true);
                }
                if (message.Length > 8192)
                    message = message[..8192] + " [log entry truncated]";
                File.AppendAllText(StatusLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never interfere with startup or game play.
        }
    }

    private static bool IsLinkedStatusFile(string path) =>
        File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
}
