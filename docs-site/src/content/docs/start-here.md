---
title: Start Here
description: Choose the right TypeSharp learning path based on your background.
---

TypeSharp is for teams that need to keep `.NET Framework 4.8` deployment and hosting, but want a more modern typed language for new code.

## Pick A Path

### I Maintain .NET Framework Applications

Start with:

1. [Tutorials](../tutorials/) to build a small console or library project.
2. [Guides](../guides/) to understand `TypeSharp.toml`, generated C# output, and `net48` assembly layout.
3. [Examples](../examples/) to see ASP.NET/WCF-style and worker-style host compatibility smoke projects.

### I Know C#

TypeSharp keeps C# interop explicit. Learn:

- [Fundamentals](../fundamentals/) for values, records, unions, and async.
- [Cookbook](../cookbook/) for calling local C# DLLs and exposing TypeSharp APIs to C#.
- [API And CLI Reference](../api/) for generated assembly layout and runtime/core library roles.

### I Know F#

TypeSharp borrows from expression-oriented and functional programming, but emits C# 7.3-compatible `net48` code. Learn:

- [Fundamentals](../fundamentals/) for immutable values, records, unions, match, Option/Result, and pipeline lowering.
- [Grammar](../grammar/) for the current stable syntax surface.
- [Migration](../migration/) for adoption boundaries.

### I Know TypeScript

TypeSharp uses structural shape checking and type-level union ideas, but public .NET metadata must be nominal. Learn:

- [Fundamentals](../fundamentals/) for structural shapes versus nominal public API.
- [Diagnostics](../diagnostics/) for public boundary and nullability errors.
- [Guides](../guides/) for C# interop and generated output.

## Current Maturity

Implemented features are backed by smoke tests or fixtures. Planned and backlog items are documented separately and should not be treated as production guarantees. The canonical status map is [feature-map.md](https://github.com/naramdash/TypeSharp/blob/main/docs/feature-map.md).

## Fast First Command

```powershell
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
```
