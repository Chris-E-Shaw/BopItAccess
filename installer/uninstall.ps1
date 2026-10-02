# Bop It Access Apps & Features launcher.
# The accessible installer EXE performs the confirmed uninstall. Once that
# process exits successfully, this script removes the EXE, ownership manifest,
# backups, and itself. This script is copied to ProgramData\BopItAccess.
param(
    [switch]$CleanupOnly,
    [int]$ProcessId = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptPath = $PSCommandPath
$stateDirectory = Split-Path -Parent $scriptPath
$expectedStateDirectory = Join-Path ([Environment]::GetFolderPath('CommonApplicationData')) 'BopItAccess'
$actualCanonical = [IO.Path]::GetFullPath($stateDirectory).TrimEnd('\')
$expectedCanonical = [IO.Path]::GetFullPath($expectedStateDirectory).TrimEnd('\')
if (-not [string]::Equals($actualCanonical, $expectedCanonical, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'The uninstaller script is outside the expected Bop It Access state directory.'
}
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
        '-File', ('"' + $scriptPath + '"')
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

    # This is an interactive, screen-reader-accessible window. The EXE returns
    # 0 only after the user confirms and the uninstall backend succeeds.
    $uninstaller = Start-Process -FilePath $installerPath -ArgumentList '--uninstall' -PassThru -Wait -WindowStyle Normal
    if ($uninstaller.ExitCode -ne 0) {
        exit $uninstaller.ExitCode
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
