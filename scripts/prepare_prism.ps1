<#
.SYNOPSIS
Downloads and stages the pinned Windows x64 Prism runtime.
.DESCRIPTION
Use a fresh staging directory for each package. The script refuses a staging
directory containing the Tolk or NVDA client DLLs from earlier mod versions.
.EXAMPLE
.\scripts\prepare_prism.ps1 -StageDirectory .\package\v1.0
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$StageDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$version = 'v0.18.3'
$expectedArchiveHash = '13D3C9D1D524B0737CDE261C6468845753E36B6C5C2637E7D26BFB8BF4D8783F'
$url = "https://github.com/ethindp/prism/releases/download/$version/prism-windows-x64.zip"
$repository = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$cache = Join-Path $repository "tools\prism-$version"
$archive = Join-Path $cache 'prism-windows-x64.zip'
$licenseCache = Join-Path $cache 'LICENSES'
$stage = [IO.Path]::GetFullPath($StageDirectory)
$stageLicenses = Join-Path $stage 'THIRD-PARTY-LICENSES\Prism'

foreach ($legacyDll in @('Tolk.dll', 'nvdaControllerClient64.dll')) {
    if (Test-Path -LiteralPath (Join-Path $stage $legacyDll)) {
        throw "The staging directory contains $legacyDll. Create a fresh staging directory for the Prism package."
    }
}

New-Item -ItemType Directory -Force -Path $cache | Out-Null
if (!(Test-Path -LiteralPath $archive) -or
    (Get-FileHash -Algorithm SHA256 -LiteralPath $archive).Hash -ne $expectedArchiveHash) {
    $download = Join-Path $cache ('download-' + [Guid]::NewGuid().ToString('N') + '.zip')
    try {
        Invoke-WebRequest -Uri $url -OutFile $download
        $actualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $download).Hash
        if ($actualHash -ne $expectedArchiveHash) {
            throw "Prism archive checksum mismatch: $actualHash"
        }
        Move-Item -LiteralPath $download -Destination $archive -Force
    }
    finally {
        if (Test-Path -LiteralPath $download) {
            Remove-Item -LiteralPath $download
        }
    }
}

Add-Type -AssemblyName System.IO.Compression
if ($PSVersionTable.PSVersion.Major -lt 6) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
}
$zip = [IO.Compression.ZipFile]::OpenRead($archive)
try {
    $dll = $zip.GetEntry('dynamic/release/bin/prism.dll')
    $notice = $zip.GetEntry('NOTICE')
    if ($null -eq $dll -or $null -eq $notice) {
        throw 'The pinned Prism archive is missing prism.dll or NOTICE.'
    }
    [IO.Compression.ZipFileExtensions]::ExtractToFile($dll, (Join-Path $cache 'prism.dll'), $true)
    [IO.Compression.ZipFileExtensions]::ExtractToFile($notice, (Join-Path $cache 'NOTICE'), $true)

    foreach ($entry in $zip.Entries) {
        if (!$entry.FullName.StartsWith('LICENSES/', [StringComparison]::Ordinal) -or
            $entry.FullName.EndsWith('/')) {
            continue
        }
        $relative = $entry.FullName.Substring('LICENSES/'.Length)
        if ($relative -notmatch '^[A-Za-z0-9._-]+(/[A-Za-z0-9._-]+)*$' -or
            $relative.Split('/') -contains '..') {
            throw "Unexpected Prism license path: $($entry.FullName)"
        }
        $destination = Join-Path $licenseCache ($relative.Replace('/', '\'))
        New-Item -ItemType Directory -Force -Path (Split-Path $destination -Parent) | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destination, $true)
    }
}
finally {
    $zip.Dispose()
}

New-Item -ItemType Directory -Force -Path $stageLicenses | Out-Null
Copy-Item -LiteralPath (Join-Path $cache 'prism.dll') -Destination (Join-Path $stage 'prism.dll') -Force
Copy-Item -LiteralPath (Join-Path $cache 'NOTICE') -Destination (Join-Path $stageLicenses 'NOTICE') -Force
Copy-Item -LiteralPath $licenseCache -Destination $stageLicenses -Recurse -Force
Write-Host "Staged Prism $version at $stage"
