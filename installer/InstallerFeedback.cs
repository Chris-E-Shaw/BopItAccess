using System.Runtime.InteropServices;
using System.Windows.Forms.Automation;

namespace BopItAccess.Installer;

/// <summary>Native Windows screen-reader notifications; never starts a second speech engine.</summary>
internal static class InstallerFeedback
{
    private static long _requestGeneration;
    private static long _postedGeneration;

    internal static long PostedGeneration => Interlocked.Read(ref _postedGeneration);

    internal static bool Announce(Control source, string message, bool important = false,
        bool logContent = true, nint activeNativeDialog = 0, bool allowBackground = false)
    {
        if (string.IsNullOrWhiteSpace(message) || source.IsDisposed || source.Disposing || !source.IsHandleCreated) return false;
        long generation = Interlocked.Increment(ref _requestGeneration);
        if (source.InvokeRequired)
        {
            nint foreground = GetForegroundWindow();
            try
            {
                source.BeginInvoke((Action)(() =>
                {
                    // A newer request or a window change makes this queued
                    // message stale. Do not let it cancel fresh user feedback.
                    if (generation != Interlocked.Read(ref _requestGeneration) || GetForegroundWindow() != foreground) return;
                    PostNotification(source, message, important, logContent, activeNativeDialog, allowBackground);
                }));
            }
            catch (InvalidOperationException) { return false; }
            return false; // Queued is not confirmation that Windows posted it.
        }
        return PostNotification(source, message, important, logContent, activeNativeDialog, allowBackground);
    }

    private static bool PostNotification(Control source, string message, bool important,
        bool logContent, nint activeNativeDialog, bool allowBackground)
    {
        if (source.IsDisposed || source.Disposing || !source.IsHandleCreated) return false;
        try
        {
            if (!CanAnnounce(source, activeNativeDialog, allowBackground)) return false;
            // Every installer announcement replaces earlier speech. Never use
            // ImportantAll or CurrentThenMostRecent, which can queue behind it.
            AutomationNotificationProcessing processing = important
                ? AutomationNotificationProcessing.ImportantMostRecent
                : AutomationNotificationProcessing.MostRecent;
            bool raised = source.AccessibilityObject.RaiseAutomationNotification(
                AutomationNotificationKind.Other,
                processing,
                message);
            if (raised) Interlocked.Increment(ref _postedGeneration);
            InstallerDiagnostics.Current?.Write("ACCESSIBILITY", raised
                ? logContent ? "Posted Windows screen-reader notification: " + message : "Posted controller accessibility notification."
                : logContent ? "Windows could not post the screen-reader notification; the message remains visible: " + message
                    : "Windows could not post a controller accessibility notification.");
            return raised;
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Runtime.InteropServices.COMException or NotSupportedException)
        {
            InstallerDiagnostics.Current?.Error("Screen-reader notification unavailable", ex);
            return false;
        }
    }

    private static bool CanAnnounce(Control source, nint activeNativeDialog, bool allowBackground)
    {
        Form? form = source as Form ?? source.FindForm();
        if (form is null || form.IsDisposed || form.Disposing || !form.IsHandleCreated || !form.Visible) return false;
        if (allowBackground) return true; // Only explicit window-attention requests use this.
        nint foreground = GetForegroundWindow();
        if (foreground == form.Handle) return true;
        if (activeNativeDialog == 0 || foreground != activeNativeDialog) return false;
        GetWindowThreadProcessId(foreground, out uint process);
        if (process != Environment.ProcessId) return false;
        // Controller copy/select feedback can use the installer provider while
        // its owned native Browse/Save dialog is active, never another process.
        for (nint owner = foreground; owner != 0; owner = GetWindow(owner, 4))
            if (owner == form.Handle) return true;
        return false;
    }

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")] private static extern nint GetWindow(nint window, uint command);

    // Compiler output, checksums and per-file details belong in diagnostics.
    // The reviewable status field instead tells the player what is happening.
    internal static string? UserStatus(string raw)
    {
        if (raw.StartsWith("Build stdout:", StringComparison.Ordinal) || raw.StartsWith("Build stderr:", StringComparison.Ordinal) ||
            raw.Contains(" stdout:", StringComparison.Ordinal) || raw.Contains(" stderr:", StringComparison.Ordinal)) return null;

        if (raw.StartsWith("Searching Steam", StringComparison.Ordinal)) return "Looking for Bop It! in your Steam libraries.";
        if (raw.StartsWith("Found Bop It! at ", StringComparison.Ordinal)) return raw;
        if (raw.StartsWith("Verified Bop It! game folder:", StringComparison.Ordinal)) return "Bop It! game folder selected.";
        if (raw.StartsWith("That folder does not contain", StringComparison.Ordinal)) return "Bop It! was not found in that folder. Choose the folder that contains the game.";
        if (raw.StartsWith("Checking GitHub for", StringComparison.Ordinal)) return "Checking for the latest Bop It Access release.";
        if (raw.StartsWith("Latest release:", StringComparison.Ordinal)) return "The latest Bop It Access release is available.";
        if (raw.StartsWith("No downloadable release", StringComparison.Ordinal)) return "A public release is not available yet. Show advanced to install the alpha version.";
        const string releaseCheckFailure = "Could not check GitHub releases: ";
        if (raw.StartsWith(releaseCheckFailure, StringComparison.Ordinal)) return "Could not check for updates. " + raw[releaseCheckFailure.Length..];
        if (raw.StartsWith("Could not check GitHub", StringComparison.Ordinal)) return "Could not check for updates. Try again or save diagnostics for review.";
        if (raw.StartsWith("Preparing a single-pass", StringComparison.Ordinal)) return "Preparing installation. You can launch the game yourself when installation finishes.";
        if (raw.StartsWith("Reusing an existing SDK", StringComparison.Ordinal)) return "The required Microsoft .NET components are already installed.";
        if (raw.StartsWith("Verified the existing .NET", StringComparison.Ordinal)) return "The required Microsoft .NET runtime is already installed.";
        if (raw.StartsWith("Downloading Microsoft's .NET 6 SDK", StringComparison.Ordinal)) return "Downloading the Microsoft .NET components needed to build the alpha version.";
        if (raw.StartsWith("Downloading only Microsoft's .NET", StringComparison.Ordinal)) return "Downloading the Microsoft .NET runtime.";
        if (raw.StartsWith("Installing .NET 6 SDK", StringComparison.Ordinal)) return "Installing Microsoft .NET components. These shared components stay installed after removing the mod.";
        if (raw.StartsWith("Prepared the .NET 6 runtime", StringComparison.Ordinal)) return "Microsoft .NET runtime downloaded and ready to install.";
        if (raw.StartsWith("Microsoft's dependency installer requested", StringComparison.Ordinal)) return "Microsoft .NET is installed. Restart Windows before launching the game.";
        if (raw.StartsWith("Abort requested. Waiting for Microsoft's", StringComparison.Ordinal)) return "Canceling. Waiting for Microsoft .NET installation to finish safely.";
        if (raw.StartsWith("Verifying MelonLoader checksum", StringComparison.Ordinal)) return "MelonLoader download complete.";
        if (raw.StartsWith("Verifying Prism checksum", StringComparison.Ordinal)) return "Prism download complete.";
        if (raw.StartsWith("Verifying MelonLoader", StringComparison.Ordinal)) return "Checking MelonLoader, which loads the mod.";
        if (raw.StartsWith("MelonLoader 0.7.3", StringComparison.Ordinal)) return "MelonLoader is already installed.";
        if (raw.StartsWith("Verifying official Prism", StringComparison.Ordinal)) return "Checking Prism speech support.";
        if (raw.StartsWith("Official Prism", StringComparison.Ordinal)) return "Prism speech support is already installed.";
        if (raw.StartsWith("Downloading MelonLoader", StringComparison.Ordinal)) return "Downloading MelonLoader.";
        if (raw.StartsWith("Downloading Prism", StringComparison.Ordinal)) return "Downloading Prism speech support.";
        if (raw.StartsWith("Checking the latest commit", StringComparison.Ordinal)) return "Checking for the latest alpha version.";
        if (raw.StartsWith("Downloading release package", StringComparison.Ordinal)) return "Downloading Bop It Access.";
        if (raw.StartsWith("Reusing complete game-generated", StringComparison.Ordinal) || raw.StartsWith("Reusing matching game proxies", StringComparison.Ordinal)) return "The files needed to build the alpha version are already available.";
        if (raw.StartsWith("Generating temporary alpha", StringComparison.Ordinal)) return "Preparing the alpha version from your game files. The game stays closed.";
        if (raw.StartsWith("Verified temporary build references", StringComparison.Ordinal)) return "The game files needed for the alpha version are ready.";
        if (raw.StartsWith("Compiling the latest source", StringComparison.Ordinal)) return "Building the latest Bop It Access alpha version.";
        if (raw == "Source build completed.") return "Bop It Access alpha is built and ready to install.";
        if (raw.StartsWith("Installing MelonLoader files", StringComparison.Ordinal)) return "Installing MelonLoader.";
        if (raw.StartsWith("Installing BopItAccess.dll", StringComparison.Ordinal)) return "Installing Bop It Access.";
        if (raw.StartsWith("Installing the required runtime", StringComparison.Ordinal)) return "Installing the Microsoft .NET runtime for the game.";
        if (raw.StartsWith("Installing verified Prism", StringComparison.Ordinal)) return "Installing Prism speech support.";
        if (raw.StartsWith("Installed Prism and bundled", StringComparison.Ordinal)) return "Prism speech support and its license information are installed.";
        if (raw.StartsWith("Installed game documentation", StringComparison.Ordinal)) return "The user's guide and translated documentation are installed.";
        if (raw.StartsWith("Registered an Apps & Features", StringComparison.Ordinal) || raw.StartsWith("Registered Bop It Access in Windows", StringComparison.Ordinal)) return "Bop It Access is registered in Windows Installed Apps.";
        if (raw.StartsWith("Committing the installation", StringComparison.Ordinal)) return "Finishing installation.";
        if (raw.StartsWith("Bop It Access installation completed", StringComparison.Ordinal)) return "Bop It Access is installed. Launch Bop It! when you are ready. The first launch may take a minute or longer while the game prepares the mod.";
        if (raw.StartsWith("Reversing changes made", StringComparison.Ordinal)) return "Undoing this installation's changes.";
        if (raw.StartsWith("Reversed installer changes", StringComparison.Ordinal)) return "Installation changes have been undone.";
        if (raw.StartsWith("Recovered and reversed", StringComparison.Ordinal)) return "An interrupted installation was found and safely undone.";
        if (raw.StartsWith("Bop It! was opened independently", StringComparison.Ordinal)) return "Close Bop It! so this installation can be safely undone.";
        if (raw.StartsWith("Game closed;", StringComparison.Ordinal)) return "The game is closed. Undoing installation changes.";
        if (raw.StartsWith("Temporary download cleanup needs", StringComparison.Ordinal)) return "Some temporary download files could not be removed. See diagnostics for details.";
        if (raw.StartsWith("Could not read installation ledger", StringComparison.Ordinal)) return "The installation record could not be read. See diagnostics for details.";
        if (raw.StartsWith("This mod installation predates", StringComparison.Ordinal)) return "Removing this manually installed copy of Bop It Access.";
        if (raw.StartsWith("Reading the installed-file", StringComparison.Ordinal)) return "Checking the files to remove.";
        if (raw.StartsWith("Uninstall completed.", StringComparison.Ordinal)) return "Bop It Access has been uninstalled. Shared Microsoft .NET components remain installed.";
        if (raw.StartsWith("Mod file removal finished", StringComparison.Ordinal)) return "The mod was removed, but some settings could not be removed. Choose Uninstall to try again.";
        if (raw.StartsWith("Cleanup reported", StringComparison.Ordinal) || raw.StartsWith("Preference cleanup reported", StringComparison.Ordinal)) return "Some settings could not be removed. Choose Uninstall to try again.";
        if (raw.StartsWith("Automatic diagnostic files could", StringComparison.Ordinal)) return "Some diagnostic files could not be removed. Choose Uninstall to try again.";
        if (raw.StartsWith("Other mods are present", StringComparison.Ordinal)) return "Other mods still need MelonLoader. Shared mod support will be kept.";
        if (raw.StartsWith("Removing mod preferences for everyone", StringComparison.Ordinal)) return "Removing saved mod settings for all Windows users.";
        if (raw.StartsWith("Removing mod preferences for the selected", StringComparison.Ordinal)) return "Removing saved mod settings for the selected Windows user.";
        if (raw.StartsWith("Continuing the previously chosen", StringComparison.Ordinal)) return "Continuing the previous preference cleanup choice.";
        if (raw.StartsWith("Removed installer-owned MelonLoader", StringComparison.Ordinal)) return "MelonLoader and its generated files have been removed.";
        if (raw.StartsWith("Removed MelonLoader preferences", StringComparison.Ordinal)) return "MelonLoader settings have been removed.";
        if (raw.StartsWith("Removed the unused ", StringComparison.Ordinal)) return raw;
        if (raw.StartsWith("Kept unrecognised files in ", StringComparison.Ordinal)) return raw;
        if (raw.StartsWith("Removed the Windows Installed Apps", StringComparison.Ordinal)) return "The Windows Installed Apps entry has been removed.";
        if (raw.StartsWith("Removed identifiable legacy mod files", StringComparison.Ordinal)) return "Bop It Access files have been removed. MelonLoader files installed separately are kept.";
        if (raw.StartsWith("The game folder was already removed", StringComparison.Ordinal)) return "The game folder is already gone. Removing the remaining mod settings.";
        if (raw.StartsWith("Retrying remaining mod preference", StringComparison.Ordinal)) return "Removing remaining mod settings.";
        if (raw.StartsWith("Scheduled cleanup of", StringComparison.Ordinal)) return "The remaining uninstall helper files will be removed when this window closes.";
        if (raw.StartsWith("Bop It! was not found", StringComparison.Ordinal) || raw.StartsWith("Keeping the game folder", StringComparison.Ordinal)) return raw;
        // The service owns its friendly phase announcements. Unknown messages
        // from nested file operations remain available in full diagnostics.
        return null;
    }

    internal static string? ProgressStatus(string step, bool completed) => step switch
    {
        "Downloading latest source" => completed ? "Bop It Access source download complete." : "Downloading the latest Bop It Access alpha source.",
        "Downloading release" => completed ? "Bop It Access download complete." : "Downloading Bop It Access.",
        "Downloading .NET 6 SDK and targeting pack" => completed ? "Microsoft .NET component download complete." : null,
        "Installing .NET 6 SDK and targeting pack" => completed ? "Microsoft .NET components installed." : null,
        "Downloading .NET runtime" => completed ? "Microsoft .NET runtime download complete." : null,
        "Downloading offline assembly tool" => completed ? "Game analysis tool downloaded." : "Downloading the game analysis tool needed to build the alpha version.",
        "Downloading offline assembly plugin" => completed ? "Game analysis support files downloaded." : "Downloading support files for game analysis.",
        "Downloading Unity build libraries" => completed ? "Game engine build files downloaded." : "Downloading game engine files needed to build the alpha version.",
        "Building offline reference helper" => completed ? null : "Preparing tools to build the alpha version.",
        "Reading game assemblies offline" => completed ? null : "Reading game files to prepare the alpha version. The game stays closed.",
        "Generating temporary interop references" => completed ? "The alpha build preparation is complete." : "Preparing the alpha build. This may take a minute or longer.",
        _ => null
    };

    internal static string FailureStatus(Exception error)
    {
        if (error.Message.StartsWith("Close Bop It!", StringComparison.Ordinal))
            return "Close Bop It! before continuing, then try again.";
        if (error.Message.StartsWith("Bop It! is still open", StringComparison.Ordinal))
            return "Close Bop It!, then reopen this installer to finish undoing the installation.";
        if (error is UnauthorizedAccessException)
            return "Windows did not allow access to a needed file. Close the game and check that you have permission to change its folder.";
        if (error is HttpRequestException request)
            return NetworkFailureStatus(request);
        if (error is HttpIOException response)
            return NetworkTransportFailureStatus(response.HttpRequestError);
        if (error is InvalidDataException && error.Message.StartsWith("GitHub ", StringComparison.Ordinal))
            return "GitHub returned information the installer could not use. Save diagnostics for review.";
        if (error is TimeoutException && error.Message.StartsWith("GitHub ", StringComparison.Ordinal))
            return "GitHub took too long to respond. Try again or save diagnostics for review.";
        if (error is TimeoutException && error.Message.StartsWith("The download stopped responding", StringComparison.Ordinal))
            return "The download stopped responding. Try again or save diagnostics for review.";
        if (error.Message.StartsWith("The game drive is unavailable", StringComparison.Ordinal))
            return "The game drive is unavailable. Reconnect it and try again.";
        if (error.Message.StartsWith("Uninstall stopped because installed files", StringComparison.Ordinal))
            return "Uninstall stopped because some files have changed. No files were removed.";
        if (error.Message.StartsWith("Finish the pending uninstall", StringComparison.Ordinal))
            return "Choose Uninstall to finish removing saved settings before installing again.";
        return "The operation could not finish.";
    }

    private static string NetworkFailureStatus(HttpRequestException request)
    {
        // An HTTP failure means a server answered. It does not establish that
        // the computer is offline. Keep request details in diagnostics.
        if (request.StatusCode is System.Net.HttpStatusCode.TooManyRequests)
            return "The download service is limiting requests. Wait a few minutes and try again.";
        if (request.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            return "The download service refused this request. Try again later or save diagnostics for review.";
        if (request.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
            return "The requested download is unavailable. Try a newer installer or save diagnostics for review.";
        if (request.StatusCode is { } status && (int)status >= 500)
            return "The download service returned an error. Try again later.";
        if (request.StatusCode != null)
            return "The download service could not complete this request. Try again or save diagnostics for review.";

        return NetworkTransportFailureStatus(request.HttpRequestError);
    }

    private static string NetworkTransportFailureStatus(HttpRequestError error) =>
        error switch
        {
            HttpRequestError.NameResolutionError => "The download server's address could not be found. Check your network settings and try again.",
            HttpRequestError.ConnectionError => "The installer could not connect to the download server. Check your network or firewall settings and try again.",
            HttpRequestError.SecureConnectionError => "A secure connection to the download server could not be established. Save diagnostics for review.",
            HttpRequestError.UserAuthenticationError or HttpRequestError.ProxyTunnelError => "The network or proxy refused this connection. Check its settings or save diagnostics for review.",
            HttpRequestError.ConfigurationLimitExceeded => "The server response exceeded an installer limit. Save diagnostics for review.",
            HttpRequestError.InvalidResponse or HttpRequestError.ResponseEnded or HttpRequestError.HttpProtocolError => "The server's response could not be read completely. Try again or save diagnostics for review.",
            _ => "The download request failed. Try again or save diagnostics for review."
        };
}

/// <summary>Weighted whole-operation progress. Percentages never reset between steps.</summary>
internal sealed class OverallInstallerProgress
{
    private readonly object _lock = new();
    private string _phase = "Preparing installation";
    private int _start;
    private int _end;
    private int _value;
    private int _published = -1;

    internal InstallerProgress Begin(string phase)
    {
        lock (_lock)
        {
            _phase = phase;
            _start = _end = _value = 0;
            _published = 0;
            return new(phase, 0, 100);
        }
    }

    internal InstallerProgress? SetPhase(string phase, int start, int end)
    {
        lock (_lock)
        {
            _phase = phase;
            _start = Math.Clamp(start, 0, 100);
            _end = Math.Clamp(end, _start, 100);
            return Update(_start);
        }
    }

    internal InstallerProgress? Report(string step, long completed, long? total)
    {
        lock (_lock)
        {
            (int start, int end) = StepRange(step);
            double fraction = total is > 0 ? Math.Clamp((double)completed / total.Value, 0, 1) : 0;
            return Update(start + (int)((end - start) * fraction));
        }
    }

    private InstallerProgress? Update(int candidate)
    {
        _value = Math.Max(_value, Math.Clamp(candidate, 0, 100));
        // Twenty meaningful updates, rather than every file/download chunk.
        int published = _value == 100 ? 100 : _value / 5 * 5;
        if (published <= _published) return null;
        _published = published;
        return new(_phase, published, 100);
    }

    private (int, int) StepRange(string step) => step switch
    {
        "Downloading .NET 6 SDK and targeting pack" => (3, 15),
        "Installing .NET 6 SDK and targeting pack" => (15, 25),
        "Downloading .NET runtime" => (3, 20),
        "Extracting .NET runtime" => (20, 25),
        "Building offline reference helper" => (45, 48),
        "Downloading offline assembly tool" => (48, 52),
        "Downloading offline assembly plugin" => (52, 53),
        "Downloading Unity build libraries" => (53, 57),
        "Extracting Unity build libraries" => (57, 59),
        "Reading game assemblies offline" => (59, 63),
        "Generating temporary interop references" => (63, 68),
        _ => (_start, _end)
    };
}
