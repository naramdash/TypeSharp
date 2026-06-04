[CmdletBinding()]
param(
  [string] $Version,
  [string] $Configuration = "Release",
  [string] $EnvFile = ".nuget-publish.env",
  [string] $ArtifactsDir = "artifacts\release",
  [switch] $Push,
  [switch] $SkipBuild
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

function Import-DotEnv {
  param([string] $Path)

  if (-not (Test-Path -LiteralPath $Path)) {
    return
  }

  foreach ($line in Get-Content -LiteralPath $Path) {
    $trimmed = $line.Trim()
    if ($trimmed.Length -eq 0 -or $trimmed.StartsWith("#")) {
      continue
    }

    if ($trimmed -notmatch "^([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*)$") {
      throw "Invalid env line in '$Path': $line"
    }

    $name = $matches[1]
    $value = $matches[2].Trim()
    if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
      $value = $value.Substring(1, $value.Length - 2)
    }

    Set-Item -Path "Env:$name" -Value $value
  }
}

function Get-ProjectVersion {
  $projectPath = Join-Path $repoRoot "cli\TypeSharp.Cli\TypeSharp.Cli.csproj"
  [xml] $project = Get-Content -LiteralPath $projectPath
  $versionNode = $project.Project.PropertyGroup.Version | Select-Object -First 1
  if ([string]::IsNullOrWhiteSpace($versionNode)) {
    throw "Could not resolve Version from '$projectPath'."
  }

  return [string] $versionNode
}

function Get-SourceRevision {
  try {
    return (git rev-parse --short=12 HEAD).Trim()
  }
  catch {
    return "unknown"
  }
}

function Assert-PackageContains {
  param(
    [string] $PackagePath,
    [string[]] $ExpectedEntries
  )

  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $zip = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
  try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace("/", "\") })
    foreach ($expected in $ExpectedEntries) {
      if ($entries -notcontains $expected) {
        throw "Package '$PackagePath' is missing expected entry '$expected'."
      }
    }
  }
  finally {
    $zip.Dispose()
  }
}

function New-LocalNuGetConfig {
  param(
    [string] $PackageSource,
    [string] $OutputPath
  )

  $escapedPackageSource = [System.Security.SecurityElement]::Escape((Resolve-Path -LiteralPath $PackageSource).Path)
  Set-Content -LiteralPath $OutputPath -Encoding utf8 -Value @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-typesharp" value="$escapedPackageSource" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-typesharp">
      <package pattern="TypeSharp.Tool" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@
}

Import-DotEnv -Path $EnvFile

if ([string]::IsNullOrWhiteSpace($Version)) {
  $Version = $env:TYPESHARP_PACKAGE_VERSION
}

if ([string]::IsNullOrWhiteSpace($Version)) {
  $Version = Get-ProjectVersion
}

$nugetSource = $env:NUGET_SOURCE
if ([string]::IsNullOrWhiteSpace($nugetSource)) {
  $nugetSource = "https://api.nuget.org/v3/index.json"
}

$sourceRevision = Get-SourceRevision
$buildMetadata = "local"
try {
  $exactTag = (git describe --tags --exact-match 2>$null).Trim()
  if (-not [string]::IsNullOrWhiteSpace($exactTag)) {
    $buildMetadata = $exactTag
  }
}
catch {
  $buildMetadata = "local"
}

$resolvedArtifactsDir = Join-Path $repoRoot $ArtifactsDir
New-Item -ItemType Directory -Force -Path $resolvedArtifactsDir | Out-Null

if (-not $SkipBuild) {
  dotnet restore cli\TypeSharp.Cli\TypeSharp.Cli.csproj
  dotnet build cli\TypeSharp.Cli\TypeSharp.Cli.csproj -c $Configuration --no-restore
}

dotnet pack cli\TypeSharp.Cli\TypeSharp.Cli.csproj `
  -c $Configuration `
  --no-restore `
  -p:Version=$Version `
  -p:TypeSharpBuildMetadata=$buildMetadata `
  -p:TypeSharpSourceRevision=$sourceRevision `
  -o $resolvedArtifactsDir

$packagePath = Join-Path $resolvedArtifactsDir "TypeSharp.Tool.$Version.nupkg"
if (-not (Test-Path -LiteralPath $packagePath)) {
  throw "TypeSharp.Tool package was not produced at '$packagePath'."
}

Assert-PackageContains -PackagePath $packagePath -ExpectedEntries @(
  "tools\net10.0\any\typesharp.dll",
  "tools\net10.0\any\runtime\net48\TypeSharp.Core.dll",
  "tools\net10.0\any\runtime\net48\TypeSharp.Runtime.dll"
)

$toolSmokePath = Join-Path $repoRoot "artifacts\tool-smoke"
if (Test-Path -LiteralPath $toolSmokePath) {
  Remove-Item -LiteralPath $toolSmokePath -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $toolSmokePath | Out-Null

$localNuGetConfig = Join-Path $toolSmokePath "NuGet.config"
New-LocalNuGetConfig -PackageSource $resolvedArtifactsDir -OutputPath $localNuGetConfig

dotnet tool install TypeSharp.Tool `
  --tool-path $toolSmokePath `
  --configfile $localNuGetConfig `
  --version $Version

$typesharp = Join-Path $toolSmokePath "typesharp.exe"
& $typesharp version
$versionJson = & $typesharp version --json | ConvertFrom-Json
if ($versionJson.artifactKind -ne "dotnet-tool") {
  throw "Packed tool artifact kind '$($versionJson.artifactKind)' did not match dotnet-tool."
}

if ($versionJson.targetDefault -ne "net48" -or $versionJson.runtimeTargetFramework -ne "net48") {
  throw "Packed tool did not preserve generated/runtime net48 metadata."
}

$runtimePathJson = & $typesharp runtime-path --json | ConvertFrom-Json
if ($runtimePathJson.targetFramework -ne "net48") {
  throw "Packed tool runtime target '$($runtimePathJson.targetFramework)' did not match net48."
}

foreach ($runtimeDll in @($runtimePathJson.core, $runtimePathJson.runtime)) {
  if (-not (Test-Path -LiteralPath $runtimeDll)) {
    throw "Packed tool runtime DLL was missing: $runtimeDll"
  }
}

if ($Push) {
  if ([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) {
    throw "NUGET_API_KEY is required when -Push is specified. Put it in '$EnvFile' or the process environment."
  }

  dotnet nuget push $packagePath `
    --api-key $env:NUGET_API_KEY `
    --source $nugetSource `
    --skip-duplicate
}
else {
  Write-Host "Package ready: $packagePath"
  Write-Host "Local tool smoke passed. Re-run with -Push after setting NUGET_API_KEY to push to '$nugetSource'."
}
