using System.Runtime.InteropServices;
using System.Text;

namespace BopItAccess;

// C ABI for Prism v0.18.3 (include/prism.h). Keep native backend calls on the
// speech worker: a PrismBackend instance is not safe for concurrent use.
internal static class PrismNative
{
    private const string LibraryName = "prism.dll";
    // Ill-formed UTF-16 from game or mod text becomes U+FFFD. Prism requires
    // valid UTF-8, and one malformed character should not end the speech worker.
    private static readonly UTF8Encoding Utf8 = new(false, false);

    [Flags]
    internal enum BackendFeature : ulong
    {
        IsSupportedAtRuntime = 1UL << 0,
        SupportsSpeak = 1UL << 2,
        SupportsSpeakToMemory = 1UL << 3,
        SupportsBraille = 1UL << 4,
        SupportsOutput = 1UL << 5,
        SupportsIsSpeaking = 1UL << 6,
        SupportsStop = 1UL << 7,
        SupportsPause = 1UL << 8,
        SupportsResume = 1UL << 9,
        SupportsSetVolume = 1UL << 10,
        SupportsGetVolume = 1UL << 11,
        SupportsSetRate = 1UL << 12,
        SupportsGetRate = 1UL << 13,
        SupportsSetPitch = 1UL << 14,
        SupportsGetPitch = 1UL << 15,
        SupportsRefreshVoices = 1UL << 16,
        SupportsCountVoices = 1UL << 17,
        SupportsGetVoiceName = 1UL << 18,
        SupportsGetVoiceLanguage = 1UL << 19,
        SupportsGetVoice = 1UL << 20,
        SupportsSetVoice = 1UL << 21,
        PerformsSilenceTrimmingOnSpeak = 1UL << 25,
        PerformsSilenceTrimmingOnSpeakToMemory = 1UL << 26,
    }

    internal enum Error
    {
        Ok = 0,
        NotInitialized,
        InvalidParam,
        NotImplemented,
        NoVoices,
        VoiceNotFound,
        SpeakFailure,
        MemoryFailure,
        RangeOutOfBounds,
        Internal,
        NotSpeaking,
        NotPaused,
        AlreadyPaused,
        InvalidUtf8,
        InvalidOperation,
        AlreadyInitialized,
        BackendNotAvailable,
        Unknown,
        InvalidAudioFormat,
        InternalBackendLimitExceeded,
        BackendEnteredUndefinedState,
        LibraryLoadFailed,
        LibraryInvalid,
        IncompatibleAbi,
    }

    internal static class BackendIds
    {
        internal const ulong Invalid = 0;
        internal const ulong Sapi = 0x1D6DF72422CEEE66;
        internal const ulong AvSpeech = 0x28E3429577805C24;
        internal const ulong VoiceOver = 0xCB4897961A754BCB;
        internal const ulong SpeechDispatcher = 0xE3D6F895D949EBFE;
        internal const ulong Nvda = 0x89CC19C5C4AC1A56;
        internal const ulong Jaws = 0xAC3D60E9BD84B53E;
        internal const ulong OneCore = 0x6797D32F0D994CB4;
        internal const ulong Orca = 0x10AA1FC05A17F96C;
        internal const ulong Uia = 0x6238F019DB678F8E;
        internal const ulong Zdsr = 0x3D93C56C9E7F2A2E;
        internal const ulong ZoomText = 0xAE439D62DC7B1479;
        internal const ulong BoyPcReader = 0x285ABA1C16F3300F;
        internal const ulong PcTalker = 0x344B951962E3B835;
        internal const ulong SenseReader = 0xED4760890B55C2F2;
        internal const ulong SystemAccess = 0x8380F2A37B2C3EB6;
        internal const ulong WindowEyes = 0x9120D89908785C13;
        internal const ulong Spiel = 0x478B44F14AD3D89C;
    }

    // The header and API reference explicitly permit a null config, which
    // selects the global registry without starting an availability poll thread.
    internal static IntPtr CreateContext() => prism_init(IntPtr.Zero);
    internal static void Shutdown(IntPtr context) => prism_shutdown(context);

    internal static nuint RegistryCount(IntPtr context) => prism_registry_count(context);
    internal static ulong RegistryIdAt(IntPtr context, nuint index) =>
        prism_registry_id_at(context, index);
    internal static ulong RegistryId(IntPtr context, string name) =>
        prism_registry_id(context, Encode(name));
    internal static string? RegistryName(IntPtr context, ulong id) =>
        Marshal.PtrToStringUTF8(prism_registry_name(context, id));
    internal static int RegistryPriority(IntPtr context, ulong id) =>
        prism_registry_priority(context, id);
    internal static bool RegistryExists(IntPtr context, ulong id) =>
        prism_registry_exists(context, id);

    // Best variants are already initialized. Specific Acquire may return an
    // uninitialized cached instance; call InitializeBackend and accept
    // AlreadyInitialized as success. Free every returned handle once.
    internal static IntPtr AcquireBest(IntPtr context) => prism_registry_acquire_best(context);
    internal static IntPtr Acquire(IntPtr context, ulong id) => prism_registry_acquire(context, id);
    internal static IntPtr CreateBest(IntPtr context) => prism_registry_create_best(context);
    internal static IntPtr Create(IntPtr context, ulong id) => prism_registry_create(context, id);
    internal static Error InitializeBackend(IntPtr backend) => prism_backend_initialize(backend);
    internal static void FreeBackend(IntPtr backend) => prism_backend_free(backend);
    internal static string? BackendName(IntPtr backend) =>
        Marshal.PtrToStringUTF8(prism_backend_name(backend));
    // The C API exposes a backend's name, not its ID. Registered backend
    // names are unique, so use the registry's exact-name lookup to recover it.
    internal static ulong BackendId(IntPtr context, IntPtr backend)
    {
        string? name = BackendName(backend);
        return name == null ? BackendIds.Invalid : RegistryId(context, name);
    }
    internal static BackendFeature GetFeatures(IntPtr backend) =>
        prism_backend_get_features(backend);
    internal static bool IsAvailable(IntPtr backend) =>
        (GetFeatures(backend) & BackendFeature.IsSupportedAtRuntime) != 0;

    internal static Error Speak(IntPtr backend, string text, bool interrupt) =>
        prism_backend_speak(backend, Encode(text), interrupt);
    internal static Error Output(IntPtr backend, string text, bool interrupt) =>
        prism_backend_output(backend, Encode(text), interrupt);
    internal static Error Braille(IntPtr backend, string text) =>
        prism_backend_braille(backend, Encode(text));
    internal static Error Stop(IntPtr backend) => prism_backend_stop(backend);
    internal static Error IsSpeaking(IntPtr backend, out bool speaking)
    {
        Error error = prism_backend_is_speaking(backend, out byte nativeSpeaking);
        speaking = nativeSpeaking != 0;
        return error;
    }

    // Prism normalizes volume, rate and pitch to [0, 1]. Neutral rate and
    // pitch are 0.5. Probe the feature mask before using these functions.
    internal static Error SetVolume(IntPtr backend, float volume) =>
        prism_backend_set_volume(backend, volume);
    internal static Error SetRate(IntPtr backend, float rate) =>
        prism_backend_set_rate(backend, rate);
    internal static Error SetPitch(IntPtr backend, float pitch) =>
        prism_backend_set_pitch(backend, pitch);
    internal static Error GetVolume(IntPtr backend, out float volume) =>
        prism_backend_get_volume(backend, out volume);
    internal static Error GetRate(IntPtr backend, out float rate) =>
        prism_backend_get_rate(backend, out rate);
    internal static Error GetPitch(IntPtr backend, out float pitch) =>
        prism_backend_get_pitch(backend, out pitch);

    internal static Error RefreshVoices(IntPtr backend) => prism_backend_refresh_voices(backend);
    internal static Error CountVoices(IntPtr backend, out nuint count) =>
        prism_backend_count_voices(backend, out count);
    internal static Error GetVoiceName(IntPtr backend, nuint voiceId, out string? name)
    {
        Error error = prism_backend_get_voice_name(backend, voiceId, out IntPtr value);
        name = error == Error.Ok ? Marshal.PtrToStringUTF8(value) : null;
        return error;
    }
    internal static Error GetVoiceLanguage(IntPtr backend, nuint voiceId, out string? language)
    {
        Error error = prism_backend_get_voice_language(backend, voiceId, out IntPtr value);
        language = error == Error.Ok ? Marshal.PtrToStringUTF8(value) : null;
        return error;
    }
    internal static Error SetVoice(IntPtr backend, nuint voiceId) =>
        prism_backend_set_voice(backend, voiceId);
    internal static Error GetVoice(IntPtr backend, out nuint voiceId) =>
        prism_backend_get_voice(backend, out voiceId);

    internal static uint Version() => prism_version();
    internal static string VersionString() =>
        Marshal.PtrToStringUTF8(prism_version_string()) ?? string.Empty;
    internal static string ErrorString(Error error) =>
        Marshal.PtrToStringUTF8(prism_error_string(error)) ?? error.ToString();

    private static byte[] Encode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        // Embedded NUL would truncate a C string; preserve following text.
        byte[] utf8 = Utf8.GetBytes(text.Replace('\0', ' '));
        byte[] terminated = new byte[utf8.Length + 1];
        Buffer.BlockCopy(utf8, 0, terminated, 0, utf8.Length);
        return terminated;
    }

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_init(IntPtr config);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern void prism_shutdown(IntPtr context);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern nuint prism_registry_count(IntPtr context);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern ulong prism_registry_id_at(IntPtr context, nuint index);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern ulong prism_registry_id(IntPtr context, byte[] name);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_registry_name(IntPtr context, ulong id);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern int prism_registry_priority(IntPtr context, ulong id);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool prism_registry_exists(IntPtr context, ulong id);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_registry_acquire_best(IntPtr context);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_registry_acquire(IntPtr context, ulong id);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_registry_create_best(IntPtr context);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_registry_create(IntPtr context, ulong id);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_initialize(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern void prism_backend_free(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_backend_name(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern BackendFeature prism_backend_get_features(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_speak(IntPtr backend, byte[] text,
        [MarshalAs(UnmanagedType.I1)] bool interrupt);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_output(IntPtr backend, byte[] text,
        [MarshalAs(UnmanagedType.I1)] bool interrupt);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_braille(IntPtr backend, byte[] text);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_stop(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_is_speaking(IntPtr backend, out byte speaking);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_set_volume(IntPtr backend, float volume);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_set_rate(IntPtr backend, float rate);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_set_pitch(IntPtr backend, float pitch);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_volume(IntPtr backend, out float volume);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_rate(IntPtr backend, out float rate);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_pitch(IntPtr backend, out float pitch);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_refresh_voices(IntPtr backend);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_count_voices(IntPtr backend, out nuint count);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_voice_name(IntPtr backend, nuint voiceId,
        out IntPtr name);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_voice_language(IntPtr backend, nuint voiceId,
        out IntPtr language);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_set_voice(IntPtr backend, nuint voiceId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern Error prism_backend_get_voice(IntPtr backend, out nuint voiceId);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern uint prism_version();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_version_string();
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    private static extern IntPtr prism_error_string(Error error);
}
