---
title: Start Here
description: Choose the right TypeSharp learning path based on your background.
---

TypeSharp is for teams that need to keep `.NET Framework 4.8` deployment and hosting, but want a more modern typed language for new code.

## Pick A Path

For a complete study sequence by background and experience level, use [Learning Paths](../learning-paths/). For a one-page explanation of the language before the formal reference, use [Language Tour](../language-tour/).

## Install First

Use [Install](../install/) for the versioned release artifact route: open the tag-specific GitHub Release notes, confirm the exact asset names, download `typesharp-cli-dotnet-<tag>.zip`, verify it with `SHA256SUMS.txt`, extract the archive, run `typesharp version`, create a project, add supported dependencies, download the matching `typesharp-runtime-net48-<tag>.zip` when Core/Runtime DLLs are needed, verify that runtime archive with the same manifest, and build a generated `net48` artifact.

If the release asset for the tag you need is not published yet, use the preview contributor source-built fallback at the end of this page.

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

## Preview Contributor Source-Built Fallback

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
$tysh = "dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll"
& $tysh version
```
