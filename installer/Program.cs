namespace BopItAccess.Installer;

internal static class Program
{
    internal const string InstallerMutexName = @"Global\BopItAccessInstaller";
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        using var instance = new Mutex(false, InstallerMutexName);
        bool acquired;
        try { acquired = instance.WaitOne(0); }
        catch (AbandonedMutexException) { acquired = true; }
        if (!acquired)
        {
            Environment.ExitCode = 1;
            MessageBox.Show("Bop It Access Installer is already open. Use its existing window.",
                "Bop It Access Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        bool startUninstall = args.Any(arg =>
            string.Equals(arg, "--uninstall", StringComparison.OrdinalIgnoreCase));
        if (startUninstall) Environment.ExitCode = 1;
        try { Application.Run(new InstallerForm(new InstallerService(startUninstall), startUninstall)); }
        finally { instance.ReleaseMutex(); }
    }
}
