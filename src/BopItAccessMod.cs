using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BopItAccess.BopItAccessMod), "Bop It Access", "0.5.9", "Bop It Access project")]

namespace BopItAccess;

[SupportedOSPlatform("windows")]
public sealed partial class BopItAccessMod : MelonMod
{
    private readonly ManualResetEventSlim _shutdownRequested = new(false);
    private readonly AutoResetEvent _speechRequested = new(false);
    private readonly object _speechLock = new();
    private Thread? _tolkThread;
    private string? _pendingSpeech;
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
    private int _lastObservedSelectionId;
    private int _lastFocusedSettingRowId;
    private int _lastObservedSettingsSelectionId;
    private string? _lastSettingsValue;
    private long _nextMenuSearchAt;
    private long _nextSettingsSearchAt;
    private long _nextFocusErrorLogAt;
    private long _nextSettingsErrorLogAt;
    private long _nextCalibrationErrorLogAt;
    private long _nextControlsErrorLogAt;
    private long _nextTrackSelectErrorLogAt;
    private long _nextPlayModesErrorLogAt;
    private long _nextGameOverErrorLogAt;
    private long _nextLeaderboardsErrorLogAt;
    private long _nextAchievementsErrorLogAt;
    private long _nextCreditsErrorLogAt;
    private bool _mainMenuWasVisible;
    private bool _settingsWasVisible;
    private static readonly object StatusLogLock = new();
    private static readonly Regex TmpTagPattern = new("<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex WhitespacePattern = new("\\s+", RegexOptions.Compiled);

    private static string StatusLogPath
    {
        get
        {
            string gameDirectory = Path.GetDirectoryName(Environment.ProcessPath ?? string.Empty)
                ?? Environment.CurrentDirectory;
            return Path.Combine(gameDirectory, "Mods", "BopItAccess.log");
        }
    }

    public override void OnInitializeMelon()
    {
        WriteStatus("Mod loaded; starting the Tolk background thread.");
        MelonLogger.Msg("Starting Tolk screen-reader support on a background thread...");
        _tolkThread = new Thread(InitializeTolkAndAnnounce)
        {
            IsBackground = true,
            Name = "Bop It Access Tolk"
        };

        try
        {
            _tolkThread.SetApartmentState(ApartmentState.STA);
            _tolkThread.Start();
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Could not start the Tolk background thread: {ex}");
        }
    }

    public override void OnLateUpdate()
    {
        ObserveResultRank();

        bool calibrationVisible = false;
        try
        {
            calibrationVisible = ReadCalibrationFocus();
        }
        catch (Exception ex)
        {
            long now = Environment.TickCount64;
            if (now >= _nextCalibrationErrorLogAt)
            {
                WriteStatus($"Audio calibration speech check failed: {ex}");
                MelonLogger.Warning($"Audio calibration speech check failed: {ex.Message}");
                _nextCalibrationErrorLogAt = now + 5000;
            }

            ResetCalibrationFocus();
        }

        bool controlsVisible = false;
        if (!calibrationVisible)
        {
            try
            {
                controlsVisible = ReadControlsFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextControlsErrorLogAt)
                {
                    WriteStatus($"Controls speech check failed: {ex}");
                    MelonLogger.Warning($"Controls speech check failed: {ex.Message}");
                    _nextControlsErrorLogAt = now + 5000;
                }

                ResetControlsFocus();
            }
        }
        else
        {
            ResetControlsFocus();
        }

        bool leaderboardsVisible = false;
        if (!calibrationVisible && !controlsVisible)
        {
            try
            {
                leaderboardsVisible = ReadLeaderboardsFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextLeaderboardsErrorLogAt)
                {
                    WriteStatus($"Leaderboard speech check failed: {ex}");
                    MelonLogger.Warning($"Leaderboard speech check failed: {ex.Message}");
                    _nextLeaderboardsErrorLogAt = now + 5000;
                }

                _leaderboardMainMenu = null;
                _leaderboardGameUi = null;
                ResetLeaderboardsFocus();
            }
        }
        else
        {
            ResetLeaderboardsFocus();
        }

        bool achievementsVisible = false;
        if (!calibrationVisible && !controlsVisible && !leaderboardsVisible)
        {
            try
            {
                achievementsVisible = ReadAchievementsFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextAchievementsErrorLogAt)
                {
                    WriteStatus($"Achievements speech check failed: {ex}");
                    MelonLogger.Warning($"Achievements speech check failed: {ex.Message}");
                    _nextAchievementsErrorLogAt = now + 5000;
                }

                _achievementsPanel = null;
                ResetAchievementsFocus();
            }
        }
        else
        {
            ResetAchievementsFocus();
        }

        bool creditsVisible = false;
        if (!calibrationVisible && !controlsVisible && !leaderboardsVisible && !achievementsVisible)
        {
            try
            {
                creditsVisible = ReadCreditsFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextCreditsErrorLogAt)
                {
                    WriteStatus($"Credits speech check failed: {ex}");
                    MelonLogger.Warning($"Credits speech check failed: {ex.Message}");
                    _nextCreditsErrorLogAt = now + 5000;
                }

                ResetCreditsFocus();
            }
        }
        else
        {
            ResetCreditsFocus();
        }

        bool gameOverVisible = false;
        if (!calibrationVisible && !controlsVisible && !leaderboardsVisible &&
            !achievementsVisible && !creditsVisible)
        {
            try
            {
                gameOverVisible = ReadGameOverFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextGameOverErrorLogAt)
                {
                    WriteStatus($"Game over speech check failed: {ex}");
                    MelonLogger.Warning($"Game over speech check failed: {ex.Message}");
                    _nextGameOverErrorLogAt = now + 5000;
                }

                _gameOverUi = null;
                _gameOverApp = null;
                ResetGameOverFocus();
            }
        }
        else
        {
            ResetGameOverFocus();
        }

        bool trackSelectVisible = false;
        if (!calibrationVisible && !controlsVisible && !leaderboardsVisible &&
            !achievementsVisible && !creditsVisible && !gameOverVisible)
        {
            try
            {
                trackSelectVisible = ReadTrackSelectFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextTrackSelectErrorLogAt)
                {
                    WriteStatus($"Song selection speech check failed: {ex}");
                    MelonLogger.Warning($"Song selection speech check failed: {ex.Message}");
                    _nextTrackSelectErrorLogAt = now + 5000;
                }

                _trackSelectUi = null;
                _trackSelectApp = null;
                ResetTrackSelectFocus();
            }
        }
        else
        {
            ResetTrackSelectFocus();
        }

        bool playModesVisible = false;
        if (!calibrationVisible && !controlsVisible && !leaderboardsVisible &&
            !achievementsVisible && !creditsVisible && !gameOverVisible && !trackSelectVisible)
        {
            try
            {
                playModesVisible = ReadPlayModesFocus();
            }
            catch (Exception ex)
            {
                long now = Environment.TickCount64;
                if (now >= _nextPlayModesErrorLogAt)
                {
                    WriteStatus($"Play mode speech check failed: {ex}");
                    MelonLogger.Warning($"Play mode speech check failed: {ex.Message}");
                    _nextPlayModesErrorLogAt = now + 5000;
                }

                ResetPlayModesFocus();
            }
        }
        else
        {
            ResetPlayModesFocus();
        }

        if (calibrationVisible || controlsVisible || leaderboardsVisible || achievementsVisible ||
            creditsVisible || gameOverVisible || trackSelectVisible || playModesVisible)
        {
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
            QueueSpeech(value == null ? focused.Label : $"{focused.Label}, {value}");
            return true;
        }

        if (value != null && !string.Equals(value, _lastSettingsValue, StringComparison.Ordinal))
        {
            _lastSettingsValue = value;
            QueueSpeech(value);
        }

        return true;
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

    private static SettingOption[] CreateSettingsOptions(SettingsPanel panel)
    {
        GoOnlineButton? goOnlineController = panel.GetComponentInChildren<GoOnlineButton>(true);
        SettingsButton? goOnline = goOnlineController == null ? null : goOnlineController.button;

        return new[]
        {
            SliderOption("MUSIC", panel.music),
            SliderOption("SFX", panel.sfx),
            SliderOption("VOICE OVER", panel.voiceOver),
            SliderOption("LANGUAGE", panel.language),
            ToggleOption("VIBRATION", panel.vibration),
            ToggleOption("FULLSCREEN", panel.fullscreen),
            SliderOption("RESOLUTION", panel.resolution),
            ActionOption("AUDIO LATENCY", panel.audioLatency),
            ActionOption("CONTROLS", panel.controls),
            ActionOption("GO ONLINE", goOnline)
        };
    }

    private static SettingOption SliderOption(string label, SettingsSlider? row) =>
        new(label, row, () => row == null ? null : ReadDisplayedValue(row, row.Value));

    private static SettingOption ToggleOption(string label, SettingsToggle? row) =>
        new(label, row, () => row == null ? null : row.IsOn ? "On" : "Off");

    private static SettingOption ActionOption(string label, SettingsButton? row) =>
        new(label, row, () => null);

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

        internal SettingOption(string label, SettingsRow? row, Func<string?> readValue)
        {
            Label = label;
            Row = row;
            _readValue = readValue;
            Id = row == null ? 0 : row.GetInstanceID();
        }

        internal string Label { get; }
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
        int selectedId = selected == null ? 0 : selected.GetInstanceID();
        if (selectedId == _lastObservedSelectionId)
            return;

        WriteStatus($"Main menu selected object: {(selected == null ? "none" : selected.name)}.");
        _lastObservedSelectionId = selectedId;

        Button? focusedButton = selected == null ? null : selected.GetComponentInParent<Button>();
        if (focusedButton == null)
        {
            _lastFocusedButtonId = 0;
            return;
        }

        int focusedButtonId = focusedButton.GetInstanceID();
        string? label = GetMainMenuLabel(focusedButtonId);
        if (label == null)
        {
            _lastFocusedButtonId = 0;
            return;
        }

        if (focusedButtonId == _lastFocusedButtonId)
            return;

        _lastFocusedButtonId = focusedButtonId;
        QueueSpeech(label);
    }

    private string? GetMainMenuLabel(int focusedButtonId)
    {
        if (Matches(_mainMenu!.playButton, focusedButtonId)) return "PLAY";
        if (Matches(_mainMenu.leaderboardButton, focusedButtonId)) return "LEADERBOARDS";
        if (Matches(_mainMenu.achievementsButton, focusedButtonId)) return "ACHIEVEMENTS";
        if (Matches(_mainMenu.settingsButton, focusedButtonId)) return "SETTINGS";
        if (Matches(_mainMenu.creditsButton, focusedButtonId)) return "CREDITS";
        if (Matches(_mainMenu.quitButton, focusedButtonId)) return "QUIT";
        return null;
    }

    private static bool Matches(Button? button, int focusedButtonId) =>
        button != null && button.GetInstanceID() == focusedButtonId;

    private void ResetMenuFocus()
    {
        _lastFocusedButtonId = 0;
        _lastObservedSelectionId = 0;
        _mainMenuWasVisible = false;
    }

    private void QueueSpeech(string text, bool interrupt = true)
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            // Keep the newest focus announcement when navigation is faster than speech.
            _pendingSpeech = text;
            _pendingSpeechInterrupt = interrupt;
            _pendingSpeechIsDescription = false;
            if (interrupt)
                _sequentialSpeech.Clear();
        }

        _speechRequested.Set();
    }

    private void QueueDescriptionSpeech(string text)
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            _pendingSpeech = text;
            _pendingSpeechInterrupt = true;
            _pendingSpeechIsDescription = true;
            _sequentialSpeech.Clear();
        }

        _speechRequested.Set();
    }

    private void CancelDescriptionSpeech()
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            if (_pendingSpeechIsDescription)
            {
                _pendingSpeech = null;
                _pendingSpeechIsDescription = false;
            }

            // Tolk is used only by its worker thread. Silence a description
            // that has already reached the screen reader, while preserving a
            // later score or menu announcement if one has been dispatched.
            if (_descriptionSpeechMayBeActive)
                _silenceRequested = true;
            _descriptionSpeechMayBeActive = false;
        }

        _speechRequested.Set();
    }

    private void StopSpeechForGameStart()
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            // Bop has started play. Drop any unsent song-selection message
            // and silence one that has already reached the screen reader.
            _pendingSpeech = null;
            _pendingPrioritySpeech = null;
            _pendingPriorityFollowUpSpeech = null;
            _pendingSpeechIsDescription = false;
            _sequentialSpeech.Clear();
            _descriptionSpeechMayBeActive = false;
            _silenceRequested = true;
            _speechGeneration++;
        }

        _speechRequested.Set();
    }

    private void QueueSequentialSpeech(string text)
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
            _sequentialSpeech.Enqueue(text);

        _speechRequested.Set();
    }

    private void QueueScoreThenMenu(string score, string? menu)
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            // The score has its own slot, so a focus change cannot replace it
            // before the Tolk worker picks up the request.
            _pendingPrioritySpeech = score;
            _pendingPriorityFollowUpSpeech = menu;
            _pendingSpeech = null;
            _pendingSpeechInterrupt = false;
            _pendingSpeechIsDescription = false;
            _sequentialSpeech.Clear();
        }

        _speechRequested.Set();
    }

    private void InitializeTolkAndAnnounce()
    {
        bool tolkLoaded = false;
        try
        {
            WriteStatus("Calling Tolk_TrySAPI(true).");
            TolkNative.Tolk_TrySAPI(true);
            WriteStatus("Tolk_TrySAPI returned; calling Tolk_Load.");
            TolkNative.Tolk_Load();
            tolkLoaded = TolkNative.Tolk_IsLoaded();

            if (!tolkLoaded)
            {
                WriteStatus("Tolk_IsLoaded returned false; no announcement was sent.");
                MelonLogger.Error("Tolk did not initialize. No ready announcement was sent.");
                return;
            }

            string? reader = Marshal.PtrToStringUni(TolkNative.Tolk_DetectScreenReader());
            WriteStatus($"Tolk initialized; active output driver: {reader ?? "none detected"}.");
            MelonLogger.Msg($"Tolk initialized. Active output driver: {reader ?? "none detected"}.");

            bool queued = TolkNative.Tolk_Output("Bop It Access Ready", true);
            if (queued)
            {
                WriteStatus("Tolk accepted the 'Bop It Access Ready' announcement.");
                MelonLogger.Msg("Tolk accepted the 'Bop It Access Ready' announcement.");
            }
            else
            {
                WriteStatus("Tolk returned false for the ready announcement.");
                MelonLogger.Warning("Tolk initialized, but it could not send the ready announcement.");
            }

            WaitHandle[] signals = { _shutdownRequested.WaitHandle, _speechRequested };
            while (WaitHandle.WaitAny(signals) != 0)
            {
                string? priority;
                string? priorityFollowUp;
                string? announcement;
                List<string>? sequential;
                bool interrupt;
                bool silence;
                bool description;
                long generation;
                lock (_speechLock)
                {
                    generation = _speechGeneration;
                    silence = _silenceRequested;
                    _silenceRequested = false;
                    priority = _pendingPrioritySpeech;
                    _pendingPrioritySpeech = null;
                    priorityFollowUp = _pendingPriorityFollowUpSpeech;
                    _pendingPriorityFollowUpSpeech = null;
                    announcement = _pendingSpeech;
                    _pendingSpeech = null;
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
                    bool silenced = TolkNative.Tolk_Silence();
                    WriteStatus($"Stopped screen-reader speech on song-selection exit: {silenced}.");
                }

                if (priority != null && generation == Interlocked.Read(ref _speechGeneration))
                {
                    bool scoreAccepted = TolkNative.Tolk_Output(priority, true);
                    WriteStatus($"Priority speech announcement '{priority}' {(scoreAccepted ? "accepted" : "rejected")} by Tolk.");
                }

                if (priorityFollowUp != null &&
                    generation == Interlocked.Read(ref _speechGeneration))
                {
                    bool menuAccepted = TolkNative.Tolk_Output(priorityFollowUp, false);
                    WriteStatus($"Queued menu announcement '{priorityFollowUp}' {(menuAccepted ? "accepted" : "rejected")} by Tolk.");
                }

                if (announcement != null &&
                    generation == Interlocked.Read(ref _speechGeneration))
                {
                    // A score sent in this batch always goes first. The
                    // following menu speech is queued without interruption.
                    bool followUpInterrupt = priority == null && priorityFollowUp == null && interrupt;
                    bool accepted = TolkNative.Tolk_Output(announcement, followUpInterrupt);
                    WriteStatus($"Speech announcement '{announcement}' (interrupt {followUpInterrupt}) {(accepted ? "accepted" : "rejected")} by Tolk.");
                }

                if (sequential != null)
                {
                    foreach (string line in sequential)
                    {
                        if (generation != Interlocked.Read(ref _speechGeneration))
                            break;
                        bool accepted = TolkNative.Tolk_Output(line, false);
                        WriteStatus($"Queued sequential announcement '{line}' {(accepted ? "accepted" : "rejected")} by Tolk.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            WriteStatus($"Tolk startup failed: {ex}");
            MelonLogger.Error($"Tolk startup failed: {ex}");
        }
        finally
        {
            if (tolkLoaded)
            {
                try
                {
                    TolkNative.Tolk_Unload();
                    WriteStatus("Tolk unloaded.");
                }
                catch (Exception ex)
                {
                    WriteStatus($"Tolk shutdown reported an error: {ex.Message}");
                    MelonLogger.Warning($"Tolk shutdown reported an error: {ex.Message}");
                }
            }
        }
    }

    public override void OnDeinitializeMelon()
    {
        WriteStatus("Mod shutdown requested.");
        _shutdownRequested.Set();
    }

    private static void WriteStatus(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(StatusLogPath)!);
            lock (StatusLogLock)
            {
                File.AppendAllText(StatusLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never interfere with startup or game play.
        }
    }

    private static class TolkNative
    {
        private const string LibraryName = "Tolk.dll";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void Tolk_Load();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Tolk_IsLoaded();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void Tolk_Unload();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void Tolk_TrySAPI([MarshalAs(UnmanagedType.I1)] bool trySapi);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr Tolk_DetectScreenReader();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Tolk_Output([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool Tolk_Silence();
    }
}
