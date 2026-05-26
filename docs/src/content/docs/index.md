---
title: TypeSharp
description: Official documentation entry point for the TypeSharp language, compiler, CLI, and tooling.
---

TypeSharp targets generated `.NET Framework 4.8` artifacts while bringing modern static language features, CLI workflows, and VS Code tooling into long-lived .NET Framework projects.

TypeSharp is currently a preview project. The compiler, CLI, generated C# backend, runtime libraries, VS Code extension, and smoke-tested examples are in the repository, but the language is still being built and stabilized.

This site is the canonical publishing surface for TypeSharp language, tooling, and project documentation.

## Start Here

- [Install](install/) shows the NuGet .NET global tool install, first project flow, and runtime dependency options.
- [Start Here](start-here/) helps new users choose the right path.
- [Learning Paths](learning-paths/) gives beginner, C# maintainer, TypeScript, F#, and advanced routes.
- [From TypeScript](from-typescript/), [From C#](from-csharp/), and [From F#](from-fsharp/) translate familiar concepts into TypeSharp rules.
- [TypeSharp Syntax Guide](syntax-guide/) teaches the grammar in a developer-friendly order.
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

Preview CLI releases are distributed as the `TypeSharp.Tool` NuGet .NET global tool:

```powershell
dotnet tool install --global TypeSharp.Tool --version 0.1.0-preview.5
typesharp version
```

The tool host runs on modern .NET. Generated TypeSharp projects, generated assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` remain `net48`. The `TypeSharp.Tool` NuGet package is the CLI distribution and the matching Core/Runtime DLL distribution; use `typesharp runtime-path` when explicit DLL paths are needed.

Start with [Install](install/) for the `dotnet tool`, `typesharp version`, project creation, dependency, build, and runtime-library flow. The source-built commands below are for contributors changing TypeSharp itself, not the normal install path.

## Contributor Source-Built Development Path

Use this path only when you are editing this repository or validating a local compiler/tooling change.

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
