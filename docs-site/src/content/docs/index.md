---
title: TypeSharp
description: Official documentation entry point for the TypeSharp language, compiler, CLI, and tooling.
---

TypeSharp targets generated `.NET Framework 4.8` artifacts while bringing modern static language features, CLI workflows, and VS Code tooling into long-lived .NET Framework projects.

TypeSharp is currently a preview project. The compiler, CLI, generated C# backend, runtime libraries, VS Code extension, and smoke-tested examples are in the repository, but the language is still being built and stabilized.

This site is the human-facing publishing surface for the repository documentation. The source-of-truth design documents remain in `docs/`, but the pages here explain the concepts directly before linking to the canonical documents and verified examples.

## Start Here

- [Start Here](start-here/) helps new users choose the right path.
- [Learning Paths](learning-paths/) gives beginner, C# maintainer, TypeScript, F#, and advanced routes.
- [Language Tour](language-tour/) explains the language before the formal reference.
- [Tutorials](tutorials/) lists runnable, sequential learning paths.
- [Guides](guides/) explains everyday project, CLI, interop, and modeling tasks.
- [Project Configuration](project-configuration/) explains `TypeSharp.toml`, source roots, generated output, references, and build shape.
- [Modules And Imports](modules/) explains source module paths, imports, exports, namespaces, and generated containers.
- [Type System](type-system/) explains inference, null safety, `unknown`, `dynamic`, structural shapes, unions, generics, and public ABI boundaries.
- [.NET Interop](dotnet-interop/) covers `net48`, local C# DLLs, generated assemblies, and host compatibility.
- [Cookbook](cookbook/) gives short recipes for common actions.
- [Fundamentals](fundamentals/) summarizes values, modules, types, records, unions, async, and diagnostics.
- [Grammar](grammar/) links the stable grammar surface and coverage matrix.
- [API And CLI Reference](api/) collects CLI, manifest, runtime/core, generated assembly, and VS Code reference material.
- [Advanced Topics](advanced/) connects lowering, public ABI, metadata, diagnostics, LSP, and regression strategy.
- [Troubleshooting](troubleshooting/) maps common failures to diagnostics and checks.

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
