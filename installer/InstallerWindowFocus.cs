using System.Runtime.InteropServices;

namespace BopItAccess.Installer;

internal static class InstallerWindowFocus
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(nint window);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint window, int command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(string? className, string title);
    [DllImport("user32.dll")]
    private static extern nint GetLastActivePopup(nint window);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);

    internal static bool Activate(Form form)
    {
        if (form.WindowState == FormWindowState.Minimized) form.WindowState = FormWindowState.Normal;
        form.BringToFront();
        form.Activate();
        return SetForegroundWindow(form.Handle);
    }

    internal static bool ActivateExisting()
    {
        nint window = FindWindow(null, "Bop It Access Installer");
        if (window == 0) return false;
        ShowWindow(window, 9); // SW_RESTORE
        // A second launch must show the open confirmation, not its disabled owner.
        nint popup = GetLastActivePopup(window);
        if (popup != 0 && IsWindowVisible(popup)) window = popup;
        return SetForegroundWindow(window);
    }
}
