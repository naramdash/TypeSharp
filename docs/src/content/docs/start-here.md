---
title: Start Here
description: Choose the right TypeSharp learning path based on your background.
---

TypeSharp is for teams that need to keep `.NET Framework 4.8` deployment and hosting, but want a more modern typed language for new code.

## Pick A Path

For a complete study sequence by background and experience level, use [Learning Paths](../learning-paths/). For a one-page explanation of the language before the formal reference, use [Language Tour](../language-tour/).

Use [TypeSharp Syntax Guide](../syntax-guide/) when you want to learn the grammar as a normal developer guide instead of reading the full grammar ledger.

## Install First

Use [Install](../install/) for the NuGet .NET global tool route: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, create a project, add supported dependencies, and build a generated `net48` artifact. Generated artifacts still target `.NET Framework 4.8`; the global tool is only the compiler/CLI delivery mechanism.

The installed `TypeSharp.Tool` package is the CLI distribution and the matching TypeSharp Core/Runtime DLL distribution. Use `typesharp runtime-path` when explicit runtime DLL paths are needed.

Contributor source-built commands at the end of this page are for contributors changing TypeSharp itself, not a prerequisite for the normal first project path.

### I Maintain .NET Framework Applications

Start with:

1. [From C#](../from-csharp/) to understand the TypeSharp artifact flow from a C# and `.NET Framework 4.8` point of view.
2. [.NET Interop](../dotnet-interop/) to understand `net48`, generated assemblies, runtime dependencies, and C# host boundaries.
3. [Tutorials](../tutorials/) to build a small console or library project.
4. [Guides](../guides/) to understand `TypeSharp.toml`, generated C# output, and `net48` assembly layout.
5. [Examples](../examples/) to see ASP.NET/WCF-style and worker-style host compatibility smoke projects.

### I Know C#

TypeSharp keeps C# interop explicit. Learn:

- [From C#](../from-csharp/) for the TypeSharp mental model, generated artifact flow, and common adoption traps.
- [.NET Interop](../dotnet-interop/) for local DLL references, public API shape, overload/nullability validation, and host compatibility.
- [Fundamentals](../fundamentals/) for values, records, unions, and async.
- [Cookbook](../cookbook/) for calling local C# DLLs and exposing TypeSharp APIs to C#.
- [API And CLI Reference](../api/) for generated assembly layout and runtime/core library roles.

### I Know F#

TypeSharp borrows from expression-oriented and functional programming, but emits C# 7.3-compatible `net48` code. Learn:

- [From F#](../from-fsharp/) for the mapping from records, unions, options/results, match, pipelines, and async to TypeSharp.
- [Language Tour](../language-tour/) for the high-level model.
- [Fundamentals](../fundamentals/) for immutable values, records, unions, match, Option/Result, and pipeline lowering.
- [Grammar](../grammar/) for the current stable syntax surface.
- [Migration](../migration/) for adoption boundaries.

### I Know TypeScript

TypeSharp uses structural shape checking and type-level union ideas, but public .NET metadata must be nominal. Learn:

- [From TypeScript](../from-typescript/) for the mapping from TypeScript modules, structural typing, unions, `unknown`, and TSConfig expectations to TypeSharp.
- [Language Tour](../language-tour/) for structural shapes, intersection aliases, type-level unions, and public boundary rules.
- [Type System](../type-system/) for `unknown`, `dynamic`, narrowing, structural shapes, intersection aliases, and nominal public API.
- [Modules And Imports](../modules/) for TypeScript-style source files, relative module paths, imports, and exports.
- [Fundamentals](../fundamentals/) for structural shapes versus nominal public API.
- [Diagnostics](../diagnostics/) for public boundary and nullability errors.
- [Guides](../guides/) for C# interop and generated output.

### I Am Evaluating The Compiler Or Tooling

Start with:

- [Advanced Topics](../advanced/) for compiler pipeline, lowering, metadata, diagnostics, LSP, and regression strategy.
- [Project Configuration](../project-configuration/) for manifest, generated output, source roots, references, and build shape.
- [Project Requirements](../requirements/) for platform, language, compiler, interop, tooling, quality, and security constraints.
- [Feature Status](../feature-status/) for MVP, backlog, preview, experimental, and rejected feature buckets.
- [Grammar And Language Reference](../reference/) for syntax and public ABI rules.
- [Core Goal](../goal/) for the project constraints.

## Current Maturity

Implemented features are backed by smoke tests or fixtures. Planned and backlog items are documented separately and should not be treated as production guarantees. Use [Feature Status](../feature-status/) for the canonical status map. [Document Ownership](../document-ownership/) tracks canonical docs ownership.

## Contributor Source-Built Development Path

Use this path only when you are editing this repository or validating a local compiler/tooling change.

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
$tysh = "dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll"
& $tysh version
```
