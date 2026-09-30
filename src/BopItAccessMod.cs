using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using MelonLoader;

[assembly: MelonInfo(typeof(BopItAccess.BopItAccessMod), "Bop It Access", "0.1.0", "Bop It Access project")]

namespace BopItAccess;

[SupportedOSPlatform("windows")]
public sealed class BopItAccessMod : MelonMod
{
    private readonly ManualResetEventSlim _shutdownRequested = new(false);
    private Thread? _tolkThread;
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

            _shutdownRequested.Wait();
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
