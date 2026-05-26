---
title: Guides
description: Practical TypeSharp guides for projects, CLI use, interop, and modeling.
---

Start from [Install](../install/) before using these guides: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. When a guide needs TypeSharp Core/Runtime DLLs, use `typesharp runtime-path` to locate the matching DLLs bundled with the installed tool.

## Project Structure

A TypeSharp project is described by `TypeSharp.toml`. The manifest points at source files, output type, target framework, root namespace, generated output root, and local C# references.

```toml
[project]
name = "HelloTypeSharp"
targetFramework = "net48"
outputType = "exe"
rootNamespace = "Samples.Hello"
generatedOutputRoot = "generated"
```

Source files use the `.tysh` extension and are discovered from the configured source root or the default `src` folder.

Use [Project Configuration](../project-configuration/) for the full manifest, source root, generated output, reference, configuration, and target override contract.

## CLI Workflow

Use the release-installed `typesharp` command and run `check` before `build` when iterating:

```powershell
typesharp check TypeSharp.toml
typesharp build TypeSharp.toml
typesharp run TypeSharp.toml -- alpha beta
typesharp format TypeSharp.toml --check
```

`build` runs the same diagnostics path before emitting generated C#.

## Generated C# And `net48`

The current backend emits C# 7.3-compatible source and an SDK-style generated C# project targeting `net48`. Generated artifacts belong under the configured generated output root and should not be committed.

Use [API And CLI Reference](../api/) for the generated assembly layout.

## C# References And Imports

Local C# DLL references are declared in the manifest:

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Tools.dll"]
packages = []

[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

TypeSharp imports C# types with namespace-based imports:

```tysh
import { LegacyFormatter } from "Legacy.Tools"
```

Implemented interop coverage includes constructors, static/instance calls, property and field reads, indexers, delegates, events, attributes, generic types, and selected overload validation.

NuGet package restore for arbitrary generated-project dependencies is post-1.0. Keep dependencies explicit through framework assemblies, local `net48` DLL paths, direct TypeSharp project references, and matching Core/Runtime DLLs from the installed `TypeSharp.Tool` package.

Use [Modules And Imports](../modules/) for source module identity, relative module resolution, export forwarding status, and generated C# module containers.

## Null Safety

TypeSharp treats nullability as a compile-time contract. Returning `null` from a non-null function or assigning nullable values to non-null locals produces diagnostics.

Unknown nullability from legacy C# metadata is reported separately as interop warning `TS2404` in strict mode.

## Option, Result, Records, And Unions

Use `Option<T>` and `Result<T, E>` for explicit absence and failure modeling. Use immutable records for public data and nominal unions for closed domain alternatives.

```tysh
export record Customer(Name: string, Age: int)

export union LookupResult {
  Found(Customer)
  Missing(string)
}
```

## Pattern Matching And Async

`match` expressions lower to C# 7.3-compatible code for the implemented nominal union and type-level union paths. `async fun` lowers to `Task` or `Task<T>` compatible with .NET Framework.

## Migration

For existing C# projects, start by exposing small TypeSharp libraries to C# consumers before moving application entry points. Use [Migration](../migration/) for the adoption model.
