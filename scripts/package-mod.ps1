#Requires -Version 7.0
<#
.SYNOPSIS
Packages an already compiled mod for manual installation and installer releases.
.DESCRIPTION
Run after building the mod. Only the mod DLL, documentation, pinned Prism
runtime/licenses, loader preference template and uninstall shortcut are copied.
Game binaries, generated proxies, MelonLoader and Microsoft .NET stay outside
the ZIP. Nothing is published and the game is never launched.
.EXAMPLE
pwsh -File .\scripts\package-mod.ps1
.EXAMPLE
pwsh -File .\scripts\package-mod.ps1 -ReleaseTag v1.0
Packages a compiled 1.0.0 mod as BopItAccess-v1.0.zip. The release tag must
represent the same numeric version as the compiled mod.
#>
param(
    [string]$BuildDirectory,
    [string]$OutputDirectory,
    [ValidatePattern('^v\d+\.\d+(?:\.\d+){0,2}$')]
    [string]$ReleaseTag
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ([string]::IsNullOrWhiteSpace($BuildDirectory)) { $BuildDirectory = Join-Path $repoRoot 'src\bin\Release\net6.0' }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $repoRoot 'build\release' }
$buildRoot = [IO.Path]::GetFullPath($BuildDirectory)
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)

function Assert-PlainPath([string]$candidate) {
    $current = [IO.Path]::GetFullPath($candidate)
    while ($current) {
        if (Test-Path -LiteralPath $current) {
            if (((Get-Item -LiteralPath $current -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Linked paths cannot be packaged: $current"
            }
        }
        $parent = [IO.Path]::GetDirectoryName($current)
        if ($parent -eq $current) { break }
        $current = $parent
    }
}

# Read the assembly's MelonInfo metadata without loading the mod or its game
# dependencies. The archive name must describe the DLL actually being shipped.
function Read-ModVersion([string]$dllPath) {
    $stream = [IO.File]::OpenRead($dllPath)
    $pe = [Reflection.PortableExecutable.PEReader]::new($stream)
    try {
        $metadata = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
        $assembly = $metadata.GetAssemblyDefinition()
        if ($metadata.GetString($assembly.Name) -cne 'BopItAccess') { throw 'The compiled DLL is not BopItAccess.' }
        foreach ($handle in $assembly.GetCustomAttributes()) {
            $attribute = $metadata.GetCustomAttribute($handle)
            if ($attribute.Constructor.Kind -ne [Reflection.Metadata.HandleKind]::MemberReference) { continue }
            $member = $metadata.GetMemberReference([Reflection.Metadata.MemberReferenceHandle]$attribute.Constructor)
            if ($member.Parent.Kind -ne [Reflection.Metadata.HandleKind]::TypeReference) { continue }
            $type = $metadata.GetTypeReference([Reflection.Metadata.TypeReferenceHandle]$member.Parent)
            if ($metadata.GetString($type.Namespace) -cne 'MelonLoader' -or $metadata.GetString($type.Name) -cne 'MelonInfoAttribute') { continue }
            $blob = $metadata.GetBlobReader($attribute.Value)
            if ($blob.ReadUInt16() -ne 1) { throw 'Invalid mod metadata.' }
            $null = $blob.ReadSerializedString() # Mod type
            $modName = $blob.ReadSerializedString()
            $version = $blob.ReadSerializedString()
            if ($modName -cne 'Bop It Access' -or $version -notmatch '^\d+\.\d+(?:\.\d+)*$') { throw 'Invalid mod name or version.' }
            return $version
        }
        throw 'The compiled DLL has no Bop It Access version metadata.'
    }
    finally { $pe.Dispose(); $stream.Dispose() }
}

# GitHub tags may omit zero patch/revision components (v1.0 versus 1.0.0).
# Normalize only numeric versions; a release tag can never add a path or name
# an archive that disagrees with the DLL being shipped.
function Normalize-ReleaseVersion([string]$numericVersion) {
    $parsed = $null
    if (-not [Version]::TryParse($numericVersion, [ref]$parsed)) {
        throw "Unsupported numeric release version: $numericVersion"
    }
    return [Version]::new($parsed.Major, $parsed.Minor,
        [Math]::Max(0, $parsed.Build), [Math]::Max(0, $parsed.Revision))
}

$modDll = Join-Path $buildRoot 'BopItAccess.dll'
Assert-PlainPath $modDll
Assert-PlainPath $outputRoot
if (-not (Test-Path -LiteralPath $modDll -PathType Leaf)) { throw 'Build the Release mod before packaging it.' }
$version = Read-ModVersion $modDll
$sourceText = [IO.File]::ReadAllText((Join-Path $repoRoot 'src\BopItAccessMod.cs'))
$sourceVersion = [regex]::Match($sourceText, 'MelonInfo\([^\r\n]*?,\s*"Bop It Access"\s*,\s*"(?<version>[0-9.]+)"').Groups['version'].Value
if ($version -cne $sourceVersion) { throw "The compiled mod is $version but source is $sourceVersion. Rebuild before packaging." }
$archiveTag = "v$version"
if ($PSBoundParameters.ContainsKey('ReleaseTag')) {
    if ((Normalize-ReleaseVersion ($ReleaseTag.Substring(1))) -ne (Normalize-ReleaseVersion $version)) {
        throw "Release tag $ReleaseTag does not match the compiled mod version $version."
    }
    $archiveTag = $ReleaseTag
}
$archivePath = Join-Path $outputRoot "BopItAccess-$archiveTag.zip"
Assert-PlainPath $archivePath
if (Test-Path -LiteralPath $archivePath) { throw "An archive already exists: $archivePath. Keep it or choose another output directory." }
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
$stage = Join-Path $outputRoot ('stage-' + [Guid]::NewGuid().ToString('N'))
Assert-PlainPath $stage
New-Item -ItemType Directory -Path $stage | Out-Null

$copies = @(
    @{ Source=$modDll; Relative='Mods\BopItAccess.dll' },
    @{ Source=(Join-Path $repoRoot 'configuration\Loader.cfg'); Relative='UserData\Loader.cfg' },
    @{ Source=(Join-Path $repoRoot 'installer\uninstall.ps1'); Relative='BopItAccess-uninstall.ps1' },
    @{ Source=(Join-Path $repoRoot 'LICENSE'); Relative='documentation\BopItAccess-LICENSE.txt' }
)
$documentNames = @('BopItAccess-user-guide.html','THIRD-PARTY-NOTICES.txt')
foreach ($name in $documentNames) { $copies += @{ Source=(Join-Path $repoRoot $name); Relative=('documentation\' + $name) } }
$documentRoot = Join-Path $repoRoot 'documentation'
Assert-PlainPath $documentRoot
foreach ($locale in @('de','es','es-MX','fr','it','ja','ko','pt-BR','zh')) {
    foreach ($name in $documentNames) {
        $relative = Join-Path $locale $name
        $copies += @{ Source=(Join-Path $documentRoot $relative); Relative=('documentation\' + $relative) }
    }
}
foreach ($copy in $copies) {
    Assert-PlainPath $copy.Source
    if (-not (Test-Path -LiteralPath $copy.Source -PathType Leaf)) { throw ('Missing package file: ' + $copy.Source) }
    $target = [IO.Path]::GetFullPath((Join-Path $stage $copy.Relative))
    if (-not $target.StartsWith($stage + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'A package path escaped staging.' }
    New-Item -ItemType Directory -Force -Path ([IO.Path]::GetDirectoryName($target)) | Out-Null
    Copy-Item -LiteralPath $copy.Source -Destination $target
}

# The existing helper verifies the official Prism archive's pinned checksum.
& (Join-Path $PSScriptRoot 'prepare_prism.ps1') -StageDirectory $stage
$dlls = @(Get-ChildItem -LiteralPath $stage -File -Recurse -Filter '*.dll')
if ($dlls.Count -ne 2 -or -not (Test-Path -LiteralPath (Join-Path $stage 'prism.dll'))) { throw 'Only BopItAccess.dll and prism.dll may be included.' }
foreach ($file in Get-ChildItem -LiteralPath $stage -File -Recurse) {
    Assert-PlainPath $file.FullName
    if ($file.Extension -in @('.exe','.pdb') -or $file.Name -in @('Tolk.dll','nvdaControllerClient64.dll',
            'BopItAccess-build-history.html','BopItAccess-release-review.html','GIT-WORKFLOW.md','README.md','README.txt')) {
        throw ('Unexpected package file: ' + $file.Name)
    }
}
[IO.Compression.ZipFile]::CreateFromDirectory($stage, $archivePath, [IO.Compression.CompressionLevel]::Optimal, $false)
Write-Output ('Prepared local compiled package: ' + $archivePath)
Write-Output ('SHA256: ' + (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash)
Write-Output ('Retained package staging files for inspection: ' + $stage)
Write-Output 'No release was published and no game or installer was launched.'
