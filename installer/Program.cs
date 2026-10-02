namespace BopItAccess.Installer;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        bool startUninstall = args.Any(arg =>
            string.Equals(arg, "--uninstall", StringComparison.OrdinalIgnoreCase));
        if (startUninstall) Environment.ExitCode = 1;
        Application.Run(new InstallerForm(new InstallerService(), startUninstall));
    }
}
