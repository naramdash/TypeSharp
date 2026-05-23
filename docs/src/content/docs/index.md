---
title: TypeSharp
description: Official documentation entry point for the TypeSharp language, compiler, CLI, and tooling.
---

TypeSharp targets generated `.NET Framework 4.8` artifacts while bringing modern static language features, CLI workflows, and VS Code tooling into long-lived .NET Framework projects.

TypeSharp is currently a preview project. The compiler, CLI, generated C# backend, runtime libraries, VS Code extension, and smoke-tested examples are in the repository, but the language is still being built and stabilized.

This site is the canonical publishing surface for TypeSharp language, tooling, and project documentation.

## Start Here

- [Start Here](start-here/) helps new users choose the right path.
- [Install](install/) shows the release zip, checksum, wrapper command, and first project flow.
- [Learning Paths](learning-paths/) gives beginner, C# maintainer, TypeScript, F#, and advanced routes.
- [Language Tour](language-tour/) explains the language before the formal reference.
- [Tutorials](tutorials/) lists runnable, sequential learning paths.
- [Guides](guides/) explains everyday project, CLI, interop, and modeling workflows.
- [Project Configuration](project-configuration/) explains `TypeSharp.toml`, source roots, generated output, references, and build shape.
- [Modules And Imports](modules/) explains source module paths, imports, exports, namespaces, and generated containers.
- [Type System](type-system/) explains inference, null safety, `unknown`, `dynamic`, structural shapes, intersection aliases, unions, generics, and public ABI boundaries.
- [Feature Status](feature-status/) maps implemented, MVP, backlog, preview, experimental, and rejected language/tooling areas.
- [.NET Interop](dotnet-interop/) covers `net48`, local C# DLLs, generated assemblies, and host compatibility.
- [Cookbook](cookbook/) gives short recipes for common actions.
- [Fundamentals](fundamentals/) summarizes values, modules, types, records, unions, async, and diagnostics.
- [Grammar](grammar/) links the stable grammar surface and coverage matrix.
- [Lowering](lowering/) explains the C# 7.3 source backend contract, generated shapes, runtime helper boundaries, and fixture evidence.
- [API And CLI Reference](api/) collects CLI, manifest, runtime/core, generated assembly, and VS Code reference material.
- [Advanced Topics](advanced/) connects lowering, public ABI, metadata, diagnostics, LSP, and regression strategy.
- [Troubleshooting](troubleshooting/) maps common failures to diagnostics and checks.
- [Project Ledger](project-ledger/) groups the canonical project records.
- [Project Requirements](requirements/) lists required platform, language, compiler, interop, tooling, quality, and security constraints.
- [Document Ownership](document-ownership/) tracks docs canonical owners.

## Install

Preview releases use GitHub Release assets:

- `typesharp-cli-dotnet-<tag>.zip` for the framework-dependent CLI host and `typesharp.cmd` wrapper,
- `typesharp-runtime-net48-<tag>.zip` for `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll`,
- `SHA256SUMS.txt` for release asset verification.

Start with [Install](install/) for the download, checksum, `typesharp version`, project creation, dependency, build, and runtime-library flow. If a versioned release asset for the tag you need is not published yet, use the source-built fallback below.

## Fastest Safe Check From Source

From the repository root:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
$tysh = "dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll"
& $tysh version
```

Then create a starter project:

```powershell
& $tysh new console HelloTypeSharp --target net48 --output .\scratch\HelloTypeSharp
& $tysh check .\scratch\HelloTypeSharp\TypeSharp.toml
& $tysh build .\scratch\HelloTypeSharp\TypeSharp.toml
```

`typesharp run` is supported for executable projects. If local antivirus blocks the generated `.exe`, `check` and `build` still prove the compiler path.
