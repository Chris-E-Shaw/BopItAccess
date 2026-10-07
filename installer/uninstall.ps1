# Bop It Access Apps & Features launcher.
# The accessible installer EXE performs the confirmed uninstall. Once that
# process exits successfully, this script removes the EXE, ownership manifest,
# backups, and itself. This script is copied to ProgramData\BopItAccess.
param(
    [switch]$CleanupOnly,
    [int]$ProcessId = 0,
    [string]$UninstallUserSid = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Preserve the user who requested removal, including when UAC uses another
# administrator's account. The UI still asks for confirmation and preference
# scope; this identifier only gives "Uninstall for me" its correct target.
if ([string]::IsNullOrWhiteSpace($UninstallUserSid)) {
    $UninstallUserSid = [Security.Principal.WindowsIdentity]::GetCurrent().User.Value
}
$validatedSid = [Security.Principal.SecurityIdentifier]::new($UninstallUserSid)
if (-not [string]::Equals($validatedSid.Value, $UninstallUserSid, [StringComparison]::OrdinalIgnoreCase) -or
    ($UninstallUserSid -notlike 'S-1-5-21-*' -and $UninstallUserSid -notlike 'S-1-12-1-*')) {
    throw 'The Windows user requesting uninstallation could not be identified safely.'
}

$scriptPath = $PSCommandPath
$stateDirectory = Split-Path -Parent $scriptPath
$expectedStateDirectory = Join-Path ([Environment]::GetFolderPath('CommonApplicationData')) 'BopItAccess'
$actualCanonical = [IO.Path]::GetFullPath($stateDirectory).TrimEnd('\')
$expectedCanonical = [IO.Path]::GetFullPath($expectedStateDirectory).TrimEnd('\')
if (-not [string]::Equals($actualCanonical, $expectedCanonical, [StringComparison]::OrdinalIgnoreCase)) {
    # Builds also supply this shortcut in the game folder. Delegate to the
    # installed Apps & Features launcher rather than assuming an EXE is beside
    # this document, and never perform recursive cleanup from the game folder.
    if ($CleanupOnly -or [IO.Path]::GetFileName($scriptPath) -ne 'BopItAccess-uninstall.ps1' -or
        -not (Test-Path -LiteralPath (Join-Path $stateDirectory 'BopIt!.exe') -PathType Leaf) -or
        -not (Test-Path -LiteralPath (Join-Path $stateDirectory 'BopIt!_Data') -PathType Container)) {
        throw 'The uninstaller shortcut is outside the Bop It! game folder.'
    }
    $installedScript = Join-Path $expectedStateDirectory 'uninstall.ps1'
    if (-not (Test-Path -LiteralPath $installedScript -PathType Leaf)) {
        throw 'The installed uninstaller is missing. Open the latest Bop It Access installer and choose Uninstall.'
    }
    foreach ($itemPath in @($expectedStateDirectory, $installedScript)) {
        if (((Get-Item -LiteralPath $itemPath -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw 'The installed uninstaller points to a linked file or folder.'
        }
    }
    $launcherShell = Join-Path $PSHOME 'powershell.exe'
    $launcherArguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-WindowStyle', 'Hidden',
        '-File', ('"' + $installedScript + '"'), '-UninstallUserSid', $UninstallUserSid)
    Start-Process -FilePath $launcherShell -ArgumentList $launcherArguments -WindowStyle Hidden
    exit 0
}
if (-not (Test-Path -LiteralPath $stateDirectory -PathType Container)) { exit 0 }
$stateInfo = Get-Item -LiteralPath $stateDirectory -Force
if (($stateInfo.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
    throw 'The Bop It Access state directory is a reparse point. Refusing recursive cleanup.'
}
$installerPath = Join-Path $stateDirectory 'BopItAccess.Uninstaller.exe'
$powershellPath = Join-Path $PSHOME 'powershell.exe'

$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = [Security.Principal.WindowsPrincipal]::new($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    # Windows may launch an Apps & Features UninstallString without elevation.
    # Elevate this script so the final ProgramData and HKLM cleanup can succeed.
    $arguments = @(
        '-NoProfile', '-ExecutionPolicy', 'Bypass', '-WindowStyle', 'Hidden',
        '-File', ('"' + $scriptPath + '"'), '-UninstallUserSid', $UninstallUserSid
    )
    if ($CleanupOnly) {
        $arguments += '-CleanupOnly'
        $arguments += '-ProcessId'
        $arguments += [string]$ProcessId
    }
    Start-Process -FilePath $powershellPath -ArgumentList $arguments -Verb RunAs -WindowStyle Hidden
    exit 0
}

if ($CleanupOnly) {
    if ($ProcessId -le 0) { throw 'CleanupOnly requires the installer process ID.' }
    # The UI already confirmed and completed removal. Wait for its image to
    # close before deleting the stable uninstaller copy and ownership records.
    Wait-Process -Id $ProcessId -ErrorAction SilentlyContinue
}
else {
    if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
        throw "The Bop It Access uninstaller is missing: $installerPath"
    }

    # The accessible UI first confirms removal, then asks "Uninstall for me"
    # or "Uninstall for everyone". No preference scope is silently selected
    # by this script. It stays open after completion until the user chooses Quit.
    $uninstaller = Start-Process -FilePath $installerPath -ArgumentList @('--uninstall', '--uninstall-user-sid', $UninstallUserSid) -PassThru -Wait -WindowStyle Normal
    if ($uninstaller.ExitCode -ne 0) {
        exit $uninstaller.ExitCode
    }
}

# Another installer may have opened between the UI closing and this helper
# acquiring its cleanup turn. Never delete state underneath a running installer
# or a newer installation which has reused this folder.
$cleanupMutex = [Threading.Mutex]::new($false, 'Global\BopItAccessInstaller')
$cleanupLocked = $false
try {
    try { $cleanupLocked = $cleanupMutex.WaitOne(0) }
    catch [Threading.AbandonedMutexException] { $cleanupLocked = $true }
    if (-not $cleanupLocked) { exit 0 }
    if (-not (Test-Path -LiteralPath $stateDirectory -PathType Container)) { exit 0 }
    # A reinstall may be interrupted after the completed uninstall record was
    # moved aside. Keep its rollback journal until startup recovery finishes.
    if (Test-Path -LiteralPath (Join-Path $stateDirectory 'transaction.json') -PathType Leaf) { exit 0 }
    $manifestPath = Join-Path $stateDirectory 'install-manifest.json'
    if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
        $manifestPath = Join-Path $stateDirectory 'uninstall-completed.json'
    }
    $cleanupManifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
    $completedProperty = $cleanupManifest.PSObject.Properties['UninstallCompleted']
    $removedProperty = $cleanupManifest.PSObject.Properties['UninstallFilesRemoved']
    if ($null -eq $completedProperty -or $null -eq $removedProperty -or
        $completedProperty.Value -isnot [bool] -or -not $completedProperty.Value -or
        $removedProperty.Value -isnot [bool] -or -not $removedProperty.Value) {
        throw 'Uninstall cleanup has not completed. The launcher and retry records will be preserved.'
    }
    $manifestState = [IO.Path]::GetFullPath([string]$cleanupManifest.StateDirectory).TrimEnd('\')
    if (-not [string]::Equals($manifestState, $expectedCanonical, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The cleanup manifest refers to another state folder.'
    }
    # Inspect every descendant before recursive deletion without following
    # junctions or symbolic links in the installer-owned state tree.
    $pendingFolders = [Collections.Generic.Stack[string]]::new()
    $pendingFolders.Push($actualCanonical)
    while ($pendingFolders.Count -gt 0) {
        $currentFolder = $pendingFolders.Pop()
        foreach ($entry in [IO.Directory]::EnumerateFileSystemEntries($currentFolder)) {
            $attributes = [IO.File]::GetAttributes($entry)
            if (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Uninstall cleanup encountered a linked file or folder: $entry"
            }
            if (($attributes -band [IO.FileAttributes]::Directory) -ne 0) {
                $pendingFolders.Push($entry)
            }
        }
    }

    # PowerShell has loaded the script by this point, so its file can be removed
    # together with the stable EXE and installer state. Retry briefly for antivirus
    # scanners that release the just-closed executable a moment later.
    for ($attempt = 0; $attempt -lt 5; $attempt++) {
        try {
            Remove-Item -LiteralPath $stateDirectory -Recurse -Force
            exit 0
        }
        catch {
            Start-Sleep -Milliseconds 300
        }
    }

    throw "Bop It Access was removed, but its uninstall launcher folder could not be cleaned: $stateDirectory"
}
finally {
    if ($cleanupLocked) { $cleanupMutex.ReleaseMutex() }
    $cleanupMutex.Dispose()
}
