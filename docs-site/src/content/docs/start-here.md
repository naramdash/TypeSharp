---
title: Start Here
description: Choose the right TypeSharp learning path based on your background.
---

TypeSharp is for teams that need to keep `.NET Framework 4.8` deployment and hosting, but want a more modern typed language for new code.

## Pick A Path

For a complete study sequence by background and experience level, use [Learning Paths](../learning-paths/). For a one-page explanation of the language before the formal reference, use [Language Tour](../language-tour/).

### I Maintain .NET Framework Applications

Start with:

1. [.NET Interop](../dotnet-interop/) to understand `net48`, generated assemblies, runtime dependencies, and C# host boundaries.
2. [Tutorials](../tutorials/) to build a small console or library project.
3. [Guides](../guides/) to understand `TypeSharp.toml`, generated C# output, and `net48` assembly layout.
4. [Examples](../examples/) to see ASP.NET/WCF-style and worker-style host compatibility smoke projects.

### I Know C#

TypeSharp keeps C# interop explicit. Learn:

- [.NET Interop](../dotnet-interop/) for local DLL references, public API shape, overload/nullability validation, and host compatibility.
- [Fundamentals](../fundamentals/) for values, records, unions, and async.
- [Cookbook](../cookbook/) for calling local C# DLLs and exposing TypeSharp APIs to C#.
- [API And CLI Reference](../api/) for generated assembly layout and runtime/core library roles.

### I Know F#

TypeSharp borrows from expression-oriented and functional programming, but emits C# 7.3-compatible `net48` code. Learn:

- [Language Tour](../language-tour/) for the high-level model.
- [Fundamentals](../fundamentals/) for immutable values, records, unions, match, Option/Result, and pipeline lowering.
- [Grammar](../grammar/) for the current stable syntax surface.
- [Migration](../migration/) for adoption boundaries.

### I Know TypeScript

TypeSharp uses structural shape checking and type-level union ideas, but public .NET metadata must be nominal. Learn:

- [Language Tour](../language-tour/) for structural shapes, type-level unions, and public boundary rules.
- [Fundamentals](../fundamentals/) for structural shapes versus nominal public API.
- [Diagnostics](../diagnostics/) for public boundary and nullability errors.
- [Guides](../guides/) for C# interop and generated output.

### I Am Evaluating The Compiler Or Tooling

Start with:

- [Advanced Topics](../advanced/) for compiler pipeline, lowering, metadata, diagnostics, LSP, and regression strategy.
- [Grammar And Language Reference](../reference/) for syntax and public ABI rules.
- [Core Goal](../goal/) for the project constraints.

## Current Maturity

Implemented features are backed by smoke tests or fixtures. Planned and backlog items are documented separately and should not be treated as production guarantees. The canonical status map is [feature-map.md](https://github.com/naramdash/TypeSharp/blob/main/docs/feature-map.md).

## Fast First Command

```powershell
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
```
