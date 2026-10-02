using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace BopItAccess.Installer;

/// <summary>Removes this mod's PlayerPrefs values from local Windows user profiles.</summary>
internal static class ModPreferenceCleaner
{
    private const string ProfileListPath =
        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
    private const string PlayerPrefsPath = @"Software\Alliance\Bop It!";
    private const string PreferencePrefix = "BopItAccess.";
    private const uint TokenAdjustPrivileges = 0x0020;
    private const uint TokenQuery = 0x0008;
    private const uint SePrivilegeEnabled = 0x0002;

    internal static IReadOnlyList<string> RemoveAcrossProfiles(Action<string> log)
    {
        ArgumentNullException.ThrowIfNull(log);
        var warnings = new List<string>();
        void Warn(string message)
        {
            warnings.Add(message);
            log($"Warning: {message}");
        }

        TokenPrivilegeScope? privileges = null;
        string? privilegeFailure = null;
        bool privilegeAttempted = false;
        try
        {
            using RegistryKey machine = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey? profiles = machine.OpenSubKey(ProfileListPath, writable: false);
            if (profiles is null)
            {
                Warn("Windows user profiles could not be enumerated: ProfileList is missing.");
                return warnings;
            }

            using RegistryKey users = RegistryKey.OpenBaseKey(
                RegistryHive.Users, RegistryView.Registry64);
            foreach (string sid in profiles.GetSubKeyNames())
            {
                if (!IsHumanProfileSid(sid)) continue;
                try
                {
                    // A logged-in user's hive is already mounted by Windows.
                    bool loaded;
                    using (RegistryKey? mounted = users.OpenSubKey(sid, writable: false))
                        loaded = mounted is not null;
                    if (loaded)
                    {
                        RemoveValues(users, sid, sid, log);
                        continue;
                    }

                    using RegistryKey? profile = profiles.OpenSubKey(sid, writable: false);
                    string? rawPath = profile?.GetValue("ProfileImagePath", null,
                        RegistryValueOptions.DoNotExpandEnvironmentNames) as string;
                    if (string.IsNullOrWhiteSpace(rawPath))
                    {
                        Warn($"Could not clean profile {sid}: its profile path is missing.");
                        continue;
                    }

                    // ProfileImagePath normally uses %SystemDrive%. Do not expand
                    // arbitrary variables from the elevated caller's environment:
                    // those could resolve to a different user's directory.
                    string expandedPath = rawPath;
                    const string systemDriveVariable = "%SystemDrive%";
                    if (expandedPath.StartsWith(systemDriveVariable,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        string? systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?
                            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        if (string.IsNullOrEmpty(systemDrive))
                        {
                            Warn($"Could not clean profile {sid}: the Windows system drive could not be determined.");
                            continue;
                        }
                        expandedPath = systemDrive + expandedPath[systemDriveVariable.Length..];
                    }
                    if (expandedPath.Contains('%'))
                    {
                        Warn($"Could not clean profile {sid}: its path contains an unsupported environment variable.");
                        continue;
                    }
                    if (!Path.IsPathFullyQualified(expandedPath))
                    {
                        Warn($"Could not clean profile {sid}: its profile path is not absolute.");
                        continue;
                    }
                    string directory = Path.GetFullPath(expandedPath);
                    string hiveFile = Path.Combine(directory, "NTUSER.DAT");
                    // RegLoadKey creates a new hive file when the path does not exist.
                    if (!File.Exists(hiveFile))
                    {
                        Warn($"Could not clean profile {sid}: NTUSER.DAT is missing or inaccessible.");
                        continue;
                    }
                    if ((File.GetAttributes(hiveFile) & FileAttributes.ReparsePoint) != 0)
                    {
                        Warn($"Could not clean profile {sid}: NTUSER.DAT is a reparse point.");
                        continue;
                    }

                    if (!privilegeAttempted)
                    {
                        privilegeAttempted = true;
                        try { privileges = TokenPrivilegeScope.Enable(); }
                        catch (Exception ex) when (IsProfileError(ex))
                        {
                            privilegeFailure = ex.Message;
                        }
                    }
                    if (privileges is null)
                    {
                        Warn($"Could not clean offline profile {sid}: registry hive privileges are unavailable: {privilegeFailure}");
                        continue;
                    }

                    string mount = "BopItAccessCleanup_" + Guid.NewGuid().ToString("N");
                    int status = RegLoadKeyW(users.Handle, mount, hiveFile);
                    if (status != 0)
                    {
                        // The user may have signed in between the loaded check and RegLoadKey.
                        using RegistryKey? nowLoaded = users.OpenSubKey(sid, writable: false);
                        if (nowLoaded is not null)
                        {
                            RemoveValues(users, sid, sid, log);
                            continue;
                        }
                        Warn($"Could not load profile {sid}: {new Win32Exception(status).Message} (Windows error {status}).");
                        continue;
                    }

                    log($"Loaded offline profile {sid} for mod preference cleanup.");
                    try
                    {
                        RemoveValues(users, mount, sid, log);
                    }
                    finally
                    {
                        // Only unload our uniquely named mount; never unload a user's live hive.
                        int unloadStatus = RegUnLoadKeyW(users.Handle, mount);
                        if (unloadStatus == 0)
                            log($"Unloaded offline profile {sid} after mod preference cleanup.");
                        else
                            Warn($"Could not unload profile {sid} from HKU\\{mount}: " +
                                 $"{new Win32Exception(unloadStatus).Message} (Windows error {unloadStatus}).");
                    }
                }
                catch (Exception ex) when (IsProfileError(ex))
                {
                    Warn($"Could not clean profile {sid}: {ex.Message}");
                }
            }
        }
        catch (Exception ex) when (IsProfileError(ex))
        {
            Warn($"Windows user profiles could not be enumerated: {ex.Message}");
        }
        finally
        {
            if (privileges is not null)
            {
                try { privileges.Dispose(); }
                catch (Win32Exception ex)
                {
                    Warn($"Could not restore the installer's registry hive privileges: {ex.Message}");
                }
            }
        }
        return warnings;
    }

    private static void RemoveValues(RegistryKey users, string hive, string sid,
        Action<string> log)
    {
        using RegistryKey? key = users.OpenSubKey($@"{hive}\{PlayerPrefsPath}", writable: true);
        if (key is null)
        {
            log($"No Bop It! preferences were found for profile {sid}.");
            return;
        }

        int removed = 0;
        foreach (string name in key.GetValueNames())
        {
            if (!name.StartsWith(PreferencePrefix, StringComparison.OrdinalIgnoreCase)) continue;
            key.DeleteValue(name, throwOnMissingValue: false);
            removed++;
            log($"Removed mod preference from profile {sid}: {name}");
        }
        if (removed == 0) log($"No mod preferences were found for profile {sid}.");
    }

    private static bool IsHumanProfileSid(string value)
    {
        // These identify local/domain and Microsoft Entra user accounts. Ignore
        // service accounts, .bak entries, class hives, and profile templates.
        if (!value.StartsWith("S-1-5-21-", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("S-1-12-1-", StringComparison.OrdinalIgnoreCase))
            return false;
        try { return new SecurityIdentifier(value).Value.Equals(value, StringComparison.OrdinalIgnoreCase); }
        catch (ArgumentException) { return false; }
    }

    private static bool IsProfileError(Exception ex) => ex is
        IOException or UnauthorizedAccessException or System.Security.SecurityException or
        Win32Exception or ArgumentException or InvalidOperationException or NotSupportedException;

    private sealed class TokenPrivilegeScope : IDisposable
    {
        private readonly SafeAccessTokenHandle _token;
        private readonly TokenPrivileges _previous;
        private bool _disposed;

        private TokenPrivilegeScope(SafeAccessTokenHandle token, TokenPrivileges previous)
        {
            _token = token;
            _previous = previous;
        }

        internal static TokenPrivilegeScope Enable()
        {
            if (!OpenProcessToken(GetCurrentProcess(), TokenAdjustPrivileges | TokenQuery,
                    out SafeAccessTokenHandle token))
                throw new Win32Exception(Marshal.GetLastPInvokeError());

            try
            {
                var requested = new TokenPrivileges
                {
                    PrivilegeCount = 2,
                    First = new LuidAndAttributes
                    {
                        Luid = Lookup("SeBackupPrivilege"), Attributes = SePrivilegeEnabled
                    },
                    Second = new LuidAndAttributes
                    {
                        Luid = Lookup("SeRestorePrivilege"), Attributes = SePrivilegeEnabled
                    }
                };
                bool adjusted = AdjustTokenPrivileges(token, false, ref requested,
                    (uint)Marshal.SizeOf<TokenPrivileges>(), out TokenPrivileges previous, out _);
                int error = Marshal.GetLastPInvokeError();
                if (!adjusted || error != 0)
                {
                    if (previous.PrivilegeCount > 0)
                    {
                        try { Restore(token, previous); }
                        catch (Win32Exception) { /* Preserve the original privilege error. */ }
                    }
                    throw new Win32Exception(error == 0 ? 1 : error);
                }
                return new TokenPrivilegeScope(token, previous);
            }
            catch
            {
                token.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { Restore(_token, _previous); }
            finally { _token.Dispose(); }
        }

        private static Luid Lookup(string name)
        {
            if (!LookupPrivilegeValueW(null, name, out Luid luid))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            return luid;
        }

        private static void Restore(SafeAccessTokenHandle token, TokenPrivileges previous)
        {
            if (previous.PrivilegeCount == 0) return;
            if (!AdjustTokenPrivileges(token, false, ref previous, 0, IntPtr.Zero, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            int error = Marshal.GetLastPInvokeError();
            if (error != 0) throw new Win32Exception(error);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Luid
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LuidAndAttributes
    {
        public Luid Luid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TokenPrivileges
    {
        public uint PrivilegeCount;
        public LuidAndAttributes First;
        public LuidAndAttributes Second;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess,
        out SafeAccessTokenHandle tokenHandle);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeValueW(string? systemName, string name, out Luid luid);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(SafeAccessTokenHandle tokenHandle,
        [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges, ref TokenPrivileges newState,
        uint bufferLength, out TokenPrivileges previousState, out uint returnLength);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(SafeAccessTokenHandle tokenHandle,
        [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges, ref TokenPrivileges newState,
        uint bufferLength, IntPtr previousState, IntPtr returnLength);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int RegLoadKeyW(SafeRegistryHandle root, string subKey, string file);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int RegUnLoadKeyW(SafeRegistryHandle root, string subKey);
}
