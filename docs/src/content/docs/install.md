---
title: Install
description: Install and run the TypeSharp CLI as a NuGet .NET tool.
---

TypeSharp's CLI distribution is the `TypeSharp.Tool` NuGet .NET global tool. The tool runs on modern .NET; generated projects, generated assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` remain `net48`.

The supported user environment is Windows with .NET Framework 4.8 installed, the targeting/build tools needed for `net48` projects, and a modern .NET SDK that can install and run .NET tools.

Contributor source builds are not part of the normal install path. Use [Start Here](../start-here/) only when you are changing TypeSharp itself.

## Install The Tool

Install the versioned CLI package:

```powershell
$version = "0.1.0-preview.5"
dotnet tool install --global TypeSharp.Tool --version $version
typesharp version
```

If the package is already installed:

```powershell
dotnet tool update --global TypeSharp.Tool --version $version
typesharp version
```

Expected metadata includes the CLI version, compiler version, language channel, release channel, runtime ABI, default generated target, CLI host target, runtime target, artifact kind, build metadata, and source revision:

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
Artifact kind dotnet-tool
Build metadata v0.1.0-preview.5
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

Generated artifacts continue to target `.NET Framework 4.8`. Framework assemblies use `references.assemblies`, local `net48` DLLs use `references.paths`, and direct TypeSharp project references use `[projectReferences]`.

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

The project direction is user convenience first, as long as the generated result remains a deterministic `net48` artifact. The CLI may resolve or copy matching TypeSharp Core/Runtime DLLs from the installed tool layout. Package restore for arbitrary user dependencies should stay explicit and auditable before it becomes a stable build behavior.

`references.packages` remains the manifest location reserved for future package references.

## Runtime Libraries

When generated code or C# consumers need TypeSharp runtime/core assemblies, use the version bundled with the installed `TypeSharp.Tool` package.

```powershell
typesharp runtime-path
typesharp runtime-path --json
```

The installed tool package contains:

```text
runtime/net48/TypeSharp.Core.dll
runtime/net48/TypeSharp.Runtime.dll
```

Reference those DLLs from `TypeSharp.toml` only when your project explicitly uses TypeSharp Core/Runtime APIs, or copy them beside a generated assembly consumed by C#. See [Runtime Artifacts](../runtime-artifacts/) for the deployable set.

## VS Code Extension

The VS Code extension can still be published separately while CLI and runtime delivery move through NuGet:

```text
typesharp-vscode-<version>.vsix
```

Use [VS Code And LSP](../vscode-lsp/) for the editor setup path.

## Uninstall

```powershell
dotnet tool uninstall --global TypeSharp.Tool
```

## Roll Back To A Previous Release

Install or update to the previous package version:

```powershell
$version = "0.1.0-preview.4"
dotnet tool update --global TypeSharp.Tool --version $version
typesharp version
```

Keep generated projects and C# consumers on the same runtime ABI as the CLI version. Because the runtime DLLs are bundled in `TypeSharp.Tool`, rolling the tool package back also rolls the matching runtime DLLs back.

## Troubleshooting

- If `typesharp` is not found, verify the .NET global tools directory is on `PATH`, then reopen the shell.
- If `dotnet tool install` cannot find `TypeSharp.Tool`, check NuGet package sources and the requested version.
- If generated `net48` builds fail because targeting packs are missing, install or repair the .NET Framework 4.8 developer targeting/build tools.
- If `typesharp run` cannot launch a generated executable, run `typesharp check` and `typesharp build` first; local security tools can block short-lived `.NET Framework` executables.
