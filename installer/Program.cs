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
        using var diagnostics = new InstallerDiagnostics(startUninstall);
        Application.ThreadException += (_, e) =>
        {
            diagnostics.Error("Unexpected UI error", e.Exception);
            MessageBox.Show("An unexpected installer error occurred: " + e.Exception.Message +
                "\n\nUse Save diagnostics to keep the details for review.",
                "Bop It Access Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            diagnostics.Write("FATAL", "Unhandled error; terminating=" + e.IsTerminating +
                ". " + e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) =>
            diagnostics.Error("Unobserved background task error", e.Exception);
        try
        {
            using var form = new InstallerForm(new InstallerService(diagnostics, startUninstall), startUninstall);
            Application.Run(form);
        }
        catch (Exception ex)
        {
            Environment.ExitCode = 1;
            diagnostics.Error("Installer startup or message loop failed", ex);
            MessageBox.Show(ex.Message + "\n\nDiagnostic log: " +
                (diagnostics.FilePath ?? "Automatic recording was unavailable."),
                "Bop It Access Installer", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            diagnostics.Dispose(); // Close the log before another instance or uninstall helper can acquire the mutex.
            instance.ReleaseMutex();
        }
    }
}
