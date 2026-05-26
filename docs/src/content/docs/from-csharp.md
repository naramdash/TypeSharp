---
title: From C#
description: How C# and .NET Framework developers should adopt TypeSharp.
---

Start from [Install](../install/) before running examples: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. Use `typesharp runtime-path` when a C# host needs the matching TypeSharp Core/Runtime DLLs bundled with the installed tool.

TypeSharp is not a C# dialect. It is a source language that checks TypeSharp code, emits C# 7.3-compatible generated source, and builds `.NET Framework 4.8` assemblies that existing C# projects can reference.

## Mental Model

| C# Concept | TypeSharp Equivalent | Rule |
| --- | --- | --- |
| Class library project | TypeSharp library project | `TypeSharp.toml` describes source roots, references, and generated output. |
| Public DTO class | `public record` | Use records for immutable public data consumed from C#. |
| Interface contract | `public interface` | Use for C#-friendly service or object boundaries. |
| Delegate/event callback | `public delegate` and event members | Use named delegate types for C# event and callback shape. |
| Nullable return for absence | `Option<T>` or `T?` | Prefer `Option<T>` for domain absence and `T?` for interop nullability. |
| Exception for expected domain failure | `Result<T,E>` or nominal result union | Keep exceptions for infrastructure and existing C# API expectations. |
| Enum plus payload classes | `public union` | Use nominal unions for closed alternatives with payloads. |
| `Task<T>` async method | `async fun ...: Task<T>` | Public async APIs lower to normal .NET task-based APIs. |
| Static helper class | Top-level exported functions | Generated module containers expose static members to C#. |

## Generated Artifact Flow

The normal adoption path is:

1. Write `.tysh` source files.
2. Run `typesharp check` to catch TypeSharp diagnostics before C# emission.
3. Run `typesharp build` to produce generated `.g.cs`, a generated C# project, and a generated `net48` assembly.
4. Reference the generated assembly from a C# `.NET Framework 4.8` project.
5. Deploy the generated assembly with `TypeSharp.Core.dll` or `TypeSharp.Runtime.dll` when public APIs or generated code use those helpers.

Generated files are build artifacts. Treat them like `obj` or generated source output: inspect them when debugging lowering, but do not edit them as the source of truth.

## Small Public API Example

```tysh
namespace Company.Billing.Rules

public record Quote(Sku: string, Amount: decimal)

public union QuoteResult {
  Quoted(Quote)
  MissingSku(sku: string)
}

export fun quote(sku: string): QuoteResult =
  if sku == "" {
    MissingSku(sku)
  } else {
    Quoted(Quote(sku, 10.0m))
  }
```

C# consumers should see named metadata: `Quote`, `QuoteResult`, generated union case types, and an exported function on the generated module container. Use [C# And CLR Type Model](../csharp-type-model/) when public member shape matters.

## Calling Existing C# Code

Reference local `net48` DLLs explicitly:

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Billing.dll"]
```

Then import by namespace:

```tysh
namespace Company.Billing.Rules

import { LegacyCalculator } from "Legacy.Billing"

export fun legacyTotal(sku: string): decimal =
  LegacyCalculator.Calculate(sku)
```

The compiler validates the supported constructor, method, property, field, indexer, delegate, event, generic, optional, named, `params`, and byref interop shapes before generated C# build whenever it has enough metadata.

## Common Traps

| Trap | Fix |
| --- | --- |
| Starting by replacing an application entry point. | Start with a small library boundary and a C# consumer smoke project. |
| Depending on generated `.g.cs` names directly. | Depend on documented public metadata only. |
| Publishing structural shapes as C# APIs. | Use records, classes, interfaces, delegates, or nominal unions. |
| Assuming NuGet restore happens inside `typesharp check`. | Build or restore C# dependencies explicitly and reference compatible DLL paths. |
| Forgetting runtime helper DLLs. | Use `typesharp runtime-path` and deploy required Core/Runtime DLLs beside the generated assembly. |
| Assuming modern C# syntax appears in generated source. | The generated source remains C# 7.3-compatible for `net48`. |

## First Reading Path

1. [.NET Interop](../dotnet-interop/) for generated assemblies, references, metadata, and host compatibility.
2. [Migration](../migration/) for incremental C# adoption.
3. [C# And CLR Type Model](../csharp-type-model/) for public ABI shape.
4. [Runtime Artifacts](../runtime-artifacts/) for generated output and runtime DLL deployment.
5. [Cookbook](../cookbook/) for short public API and local DLL recipes.
