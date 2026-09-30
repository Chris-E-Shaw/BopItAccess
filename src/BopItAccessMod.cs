using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(BopItAccess.BopItAccessMod), "Bop It Access", "0.2.0", "Bop It Access project")]

namespace BopItAccess;

[SupportedOSPlatform("windows")]
public sealed class BopItAccessMod : MelonMod
{
    private readonly ManualResetEventSlim _shutdownRequested = new(false);
    private readonly AutoResetEvent _speechRequested = new(false);
    private readonly object _speechLock = new();
    private Thread? _tolkThread;
    private string? _pendingSpeech;
    private MainMenuUIManager? _mainMenu;
    private int _lastFocusedButtonId;
    private int _lastObservedSelectionId;
    private long _nextMenuSearchAt;
    private long _nextFocusErrorLogAt;
    private bool _mainMenuWasVisible;
    private static readonly object StatusLogLock = new();

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
        try
        {
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

    private void QueueSpeech(string text)
    {
        if (_shutdownRequested.IsSet)
            return;

        lock (_speechLock)
        {
            // Keep the newest focus announcement when navigation is faster than speech.
            _pendingSpeech = text;
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
                string? announcement;
                lock (_speechLock)
                {
                    announcement = _pendingSpeech;
                    _pendingSpeech = null;
                }

                if (announcement == null)
                    continue;

                bool accepted = TolkNative.Tolk_Output(announcement, true);
                WriteStatus($"Main menu announcement '{announcement}' {(accepted ? "accepted" : "rejected")} by Tolk.");
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
    }
}
