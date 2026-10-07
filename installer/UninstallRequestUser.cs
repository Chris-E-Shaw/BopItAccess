using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace BopItAccess.Installer;

/// <summary>
/// Identifies the desktop user requesting removal, which may differ from the
/// administrator approving UAC. An explicit SID from the unelevated launcher
/// takes precedence over the shell in this process's Windows session.
/// </summary>
internal static class UninstallRequestUser
{
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const uint TokenQuery = 0x0008;

    internal static string Resolve(string? suppliedSid, Action<string>? log = null)
    {
        if (suppliedSid is not null)
            return Validate(suppliedSid);

        try
        {
            IntPtr shell = GetShellWindow();
            if (shell == IntPtr.Zero)
                throw new InvalidOperationException("The Windows desktop shell was not available.");
            if (GetWindowThreadProcessId(shell, out uint shellProcessId) == 0 || shellProcessId == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            if (!ProcessIdToSessionId(shellProcessId, out uint shellSession) ||
                !ProcessIdToSessionId((uint)Environment.ProcessId, out uint ownSession))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            if (shellSession != ownSession)
                throw new InvalidOperationException("The desktop shell belongs to another Windows session.");

            using SafeProcessHandle process = OpenProcess(ProcessQueryLimitedInformation,
                false, shellProcessId);
            if (process.IsInvalid)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            if (!OpenProcessToken(process, TokenQuery, out SafeAccessTokenHandle token))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            using (token)
            using (var identity = new WindowsIdentity(token.DangerousGetHandle()))
                return Validate(identity.User?.Value ??
                    throw new InvalidOperationException("The desktop user has no Windows identifier."));
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or
            UnauthorizedAccessException or System.Security.SecurityException or ArgumentException or InvalidDataException)
        {
            // The scope dialog names this account explicitly. An unavailable
            // shell must never broaden cleanup to every user as a fallback.
            log?.Invoke("The desktop account could not be identified. Uninstall for me will use the Windows account shown in the next choices.");
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            return Validate(identity.User?.Value ??
                throw new InvalidDataException("The Windows user could not be identified safely."));
        }
    }

    internal static string ResolveName(string sid)
    {
        Validate(sid);
        try { return new SecurityIdentifier(sid).Translate(typeof(NTAccount)).Value; }
        catch (SystemException)
        {
            return sid;
        }
    }

    private static string Validate(string sid)
    {
        if (!sid.StartsWith("S-1-5-21-", StringComparison.OrdinalIgnoreCase) &&
            !sid.StartsWith("S-1-12-1-", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The selected Windows user is not a supported user profile.");
        try
        {
            string canonical = new SecurityIdentifier(sid).Value;
            if (canonical.Equals(sid, StringComparison.OrdinalIgnoreCase)) return canonical;
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException("The selected Windows user identifier is invalid.", ex);
        }
        throw new InvalidDataException("The selected Windows user identifier is invalid.");
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ProcessIdToSessionId(uint processId, out uint sessionId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern SafeProcessHandle OpenProcess(uint desiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint processId);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(SafeProcessHandle processHandle,
        uint desiredAccess, out SafeAccessTokenHandle tokenHandle);
}
