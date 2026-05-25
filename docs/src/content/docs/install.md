---
title: Install
description: Download, verify, install, and run the TypeSharp CLI release artifact.
---

TypeSharp preview releases use GitHub Release assets from this repository. The CLI archive is a framework-dependent modern .NET host; generated projects and TypeSharp runtime libraries remain `net48`.

Use the preview contributor source-built fallback on [Start Here](../start-here/) only when the versioned release asset for the tag you need is not published yet.

## Download The CLI

Choose a release tag and download the CLI zip plus checksum manifest.

```powershell
$version = "v0.1.0-preview.4"
$repo = "naramdash/TypeSharp"
$release = "https://github.com/$repo/releases/download/$version"
$releasePage = "https://github.com/$repo/releases/tag/$version"
$installRoot = Join-Path $env:LOCALAPPDATA "TypeSharp\$version"
$downloadRoot = Join-Path $env:TEMP "typesharp-$version"

New-Item -ItemType Directory -Force $installRoot, $downloadRoot | Out-Null

Invoke-WebRequest "$release/typesharp-cli-dotnet-$version.zip" -OutFile "$downloadRoot/typesharp-cli-dotnet-$version.zip"
Invoke-WebRequest "$release/SHA256SUMS.txt" -OutFile "$downloadRoot/SHA256SUMS.txt"
```

Open `$releasePage` to read the GitHub Release notes, runtime ABI, build metadata, source revision, rollback guidance, and exact asset names to verify for the same tag.

## Verify The Download

Check the SHA-256 hash before extracting.

```powershell
function Assert-ReleaseAssetHash([string] $assetName) {
  $assetPath = Join-Path $downloadRoot $assetName
  $entries = Get-Content -LiteralPath "$downloadRoot/SHA256SUMS.txt" | ForEach-Object {
    $parts = $_ -split '\s+', 2
    if ($parts.Count -ne 2 -or $parts[0] -notmatch '^[0-9a-f]{64}$') {
      throw "Malformed SHA256SUMS.txt entry: $_"
    }

    [pscustomobject]@{
      Hash = $parts[0].ToLowerInvariant()
      Name = $parts[1].Trim()
    }
  }

  $matches = @($entries | Where-Object { $_.Name -eq $assetName })
  if ($matches.Count -eq 0) {
    throw "SHA256SUMS.txt does not list $assetName."
  }

  if ($matches.Count -gt 1) {
    throw "SHA256SUMS.txt lists $assetName more than once."
  }

  $expected = $matches[0].Hash
  $actual = (Get-FileHash -Algorithm SHA256 $assetPath).Hash.ToLowerInvariant()

  if ($actual -ne $expected) {
    throw "TypeSharp checksum mismatch for $assetName."
  }
}

Assert-ReleaseAssetHash "typesharp-cli-dotnet-$version.zip"
```

Preview releases publish `SHA256SUMS.txt` as the artifact integrity gate. Detached signatures and Authenticode signing are not published yet; when signing is added, the release notes will name the signing mechanism and signer identity. Until then, install only assets whose names match the release notes and whose hashes match `SHA256SUMS.txt`.

## Install The Command

Extract the archive. The CLI zip contains `typesharp.dll` and a Windows `typesharp.cmd` wrapper that runs it through `dotnet`.

```powershell
Expand-Archive "$downloadRoot/typesharp-cli-dotnet-$version.zip" -DestinationPath $installRoot -Force

& "$installRoot/typesharp.cmd" version

$env:PATH = "$installRoot;$env:PATH"
typesharp version
```

Expected metadata includes the CLI version, compiler version, language channel, release channel, runtime ABI, default generated target, CLI host target, runtime target, artifact kind, build metadata, and source revision. The source revision is the same 12-character lowercase commit prefix recorded on the GitHub Release page:

```text
TypeSharp CLI 0.1.0-preview
Compiler 0.1.0-preview
Language preview
Release channel Preview
Runtime ABI 0
Runtime ABI status preview
Target default net48
CLI target net10.0
Runtime target net48
Artifact kind framework-dependent-dotnet
Build metadata v0.1.0-preview.4
Source revision <12-character-commit-prefix>
```

Scripts can use:

```powershell
typesharp version --json
```

## Create And Build A Project

```powershell
typesharp new console HelloTypeSharp --target net48 --output .\HelloTypeSharp
typesharp check .\HelloTypeSharp\TypeSharp.toml
typesharp build .\HelloTypeSharp\TypeSharp.toml
typesharp run .\HelloTypeSharp\TypeSharp.toml
```

For a library project:

```powershell
typesharp new library Billing.Rules --target net48 --output .\Billing.Rules
typesharp check .\Billing.Rules\TypeSharp.toml
typesharp build .\Billing.Rules\TypeSharp.toml
```

## Add Supported Dependencies

The 1.0 dependency acquisition scope is framework assemblies, explicit local `net48` DLLs, direct TypeSharp project references, and matching TypeSharp Core/Runtime DLLs from the release runtime archive. Framework assemblies use `references.assemblies`, local `net48` DLLs use `references.paths`, and direct TypeSharp project references use `[projectReferences]`.

```toml
[references]
assemblies = [
  "System",
  "System.Core"
]
paths = [
  "../lib/Legacy.Tools.dll"
]
packages = []

[projectReferences]
paths = [
  "../Shared/TypeSharp.toml"
]
```

`references.packages` is reserved and post-1.0. TypeSharp reports `TS2405` instead of restoring NuGet packages; `check` and `build` do not silently restore packages or execute package build targets.

## Runtime Libraries

When generated code or C# consumers need TypeSharp runtime/core assemblies, download and verify the matching runtime archive:

```powershell
Invoke-WebRequest "$release/typesharp-runtime-net48-$version.zip" -OutFile "$downloadRoot/typesharp-runtime-net48-$version.zip"
Assert-ReleaseAssetHash "typesharp-runtime-net48-$version.zip"

$runtimeRoot = Join-Path $env:LOCALAPPDATA "TypeSharp\runtime-$version"
Expand-Archive "$downloadRoot/typesharp-runtime-net48-$version.zip" -DestinationPath $runtimeRoot -Force
```

The runtime archive contains:

```text
lib/net48/TypeSharp.Core.dll
lib/net48/TypeSharp.Runtime.dll
```

Reference the extracted DLLs from `TypeSharp.toml` with local paths, for example `../typesharp-runtime/lib/net48/TypeSharp.Core.dll` and `../typesharp-runtime/lib/net48/TypeSharp.Runtime.dll`, or paths relative to the `$runtimeRoot` location you chose. Deploy the generated assembly beside the required TypeSharp Core/Runtime DLLs. See [Runtime Artifacts](../runtime-artifacts/) for the deployable set and release-style smoke coverage.

The 1.0 runtime resolution policy is explicit installed runtime archive references. The CLI does not implicitly discover repository build folders, auto-copy runtime assemblies, or add hidden template references for 1.0; keep the runtime zip version aligned with the CLI release and reference the extracted DLLs explicitly.

## Uninstall

Remove the extracted version directory and any persistent `PATH` entry you added.

```powershell
Remove-Item -Recurse -Force "$installRoot"
```

## Roll Back To A Previous Release

Choose the previous tag from the GitHub Releases page, set `$version` to that tag, download `typesharp-cli-dotnet-$version.zip`, `typesharp-runtime-net48-$version.zip`, and `SHA256SUMS.txt`, then run `Assert-ReleaseAssetHash` for both archives before extracting them.

```powershell
$version = "v0.1.0-preview.0"
$release = "https://github.com/$repo/releases/download/$version"
$releasePage = "https://github.com/$repo/releases/tag/$version"
$installRoot = Join-Path $env:LOCALAPPDATA "TypeSharp\$version"
$downloadRoot = Join-Path $env:TEMP "typesharp-$version"

New-Item -ItemType Directory -Force $installRoot, $downloadRoot | Out-Null

Invoke-WebRequest "$release/typesharp-cli-dotnet-$version.zip" -OutFile "$downloadRoot/typesharp-cli-dotnet-$version.zip"
Invoke-WebRequest "$release/typesharp-runtime-net48-$version.zip" -OutFile "$downloadRoot/typesharp-runtime-net48-$version.zip"
Invoke-WebRequest "$release/SHA256SUMS.txt" -OutFile "$downloadRoot/SHA256SUMS.txt"

Assert-ReleaseAssetHash "typesharp-cli-dotnet-$version.zip"
Assert-ReleaseAssetHash "typesharp-runtime-net48-$version.zip"

Expand-Archive "$downloadRoot/typesharp-cli-dotnet-$version.zip" -DestinationPath $installRoot -Force
& "$installRoot/typesharp.cmd" version
```

Keep generated projects and C# consumers on the same runtime archive tag and Runtime ABI as the CLI release notes. If you persist a `PATH` entry, point it at the rolled-back `$installRoot` and remove the newer extracted CLI folder when it is no longer needed.

## Troubleshooting

- If `typesharp` is not found, run `& "$installRoot/typesharp.cmd" version` or add the extracted directory to `PATH`.
- If checksum verification fails, delete the zip and download it again from the release page.
- If `typesharp run` cannot launch a generated executable, run `typesharp check` and `typesharp build` first; local security tools can block short-lived `.NET Framework` executables.
- If `references.packages` fails with `TS2405`, build or copy a compatible local DLL and reference it through `references.paths`.
