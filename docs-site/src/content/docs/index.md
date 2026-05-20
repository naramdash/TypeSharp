---
title: TypeSharp
description: Official documentation entry point for the TypeSharp language, compiler, CLI, and tooling.
---

TypeSharp targets generated `.NET Framework 4.8` artifacts while bringing modern static language features, CLI workflows, and VS Code tooling into long-lived .NET Framework projects.

TypeSharp is currently a preview project. The compiler, CLI, generated C# backend, runtime libraries, VS Code extension, and smoke-tested examples are in the repository, but the language is still being built and stabilized.

This site is the canonical publishing surface for standard language and project ledger documentation since task `0251-docs-site-canonical-language-ledger`. The `docs/` directory remains the temporary operating area for agentic goal work, bridge files, task packets, rollups, and handoff state.

## Start Here

- [Start Here](start-here/) helps new users choose the right path.
- [Learning Paths](learning-paths/) gives beginner, C# maintainer, TypeScript, F#, and advanced routes.
- [Language Tour](language-tour/) explains the language before the formal reference.
- [Tutorials](tutorials/) lists runnable, sequential learning paths.
- [Guides](guides/) explains everyday project, CLI, interop, and modeling tasks.
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
- [Project Ledger](project-ledger/) groups the canonical project records and transition bridge files.
- [Project Requirements](requirements/) lists required platform, language, compiler, interop, tooling, quality, and security constraints.
- [Document Ownership](document-ownership/) tracks which legacy `docs/` files are docs-site canonical, temporary agentic files, or archive/remove candidates.
- [Work Ledger](work-ledger/) shows the current long-running task state and compressed completed-work rollup.
- [Agentic Workflow](agentic-workflow/) explains how Codex CLI goal and long-running agents should use the canonical records without treating the website as the task queue.

## Fastest Safe Check

From the repository root:

```powershell
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
```

Then create a starter project:

```powershell
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll new console HelloTypeSharp --target net48 --output .\scratch\HelloTypeSharp
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll check .\scratch\HelloTypeSharp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll build .\scratch\HelloTypeSharp\TypeSharp.toml
```

`typesharp run` is supported for executable projects. If local antivirus blocks the generated `.exe`, `check` and `build` still prove the compiler path.
