---
title: Language Tour
description: A guided tour of TypeSharp language features before the formal reference.
---

TypeSharp is a statically checked language that emits `.NET Framework 4.8` artifacts through generated C# source. It is designed for teams that need long-lived .NET Framework compatibility while adding modern language features and a stricter compile-time workflow.

## Files And Projects

TypeSharp source files use the `.tysh` extension. A project is described by `TypeSharp.toml`, and the CLI discovers source files from the configured source root or the default `src` folder.

```toml
[project]
name = "BillingRules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing"
generatedOutputRoot = "generated"
```

Use `typesharp check` while editing and `typesharp build` when you want generated C# and a generated `net48` assembly.

## Namespaces And Imports

`namespace` controls the generated .NET namespace. `import` brings TypeSharp or C# symbols into scope.

```text
namespace Company.Billing

import { LegacyFormatter } from "Legacy.Tools"
```

Use stable namespaces for public APIs because C# consumers see the generated names.

## Values And Functions

Use `let` for values and `fun` for functions.

```text
let prefix = "Invoice"

export fun label(number: int): string =
  prefix + "-" + number
```

Public functions should use explicit parameter and return types. This keeps generated CLR metadata predictable and makes diagnostics clearer.

## Type Inference

TypeSharp infers common local expression types, including literals, identifiers, direct calls, binary expressions, and pipeline flows.

```text
let count = 3
let next = count + 1
```

Prefer explicit annotations at module boundaries, exported functions, and interop boundaries.

## Records

Records are immutable-first data shapes for public data.

```text
export record Customer(Name: string, Age: int)
```

Use records when C# callers should see a named data type instead of an anonymous structural shape.

## Classes, Interfaces, And Delegates

Classes and interfaces represent nominal .NET shapes. Delegates let TypeSharp interact with C# callback APIs.

```text
export interface IRule {
  fun validate(value: string): bool
}

export delegate Transform(value: string): string
```

Use nominal declarations for public contracts because they lower to C#-understandable metadata.

## Option And Result

Use `Option<T>` for explicit absence and `Result<T, E>` for recoverable failure. This is clearer than passing `null` through code and relying on runtime checks.

```text
export fun findName(id: int): Option<string> =
  None
```

Nullability diagnostics report when a nullable value flows into a non-null contract.

## Nominal Unions And Pattern Matching

Nominal unions model closed alternatives.

```text
export union LookupResult {
  Found(Customer)
  Missing(string)
}
```

`match` expressions let you handle each case. The checker reports missing union cases for implemented exhaustive paths.

## Structural Shapes

Structural shapes describe required members for compile-time checks.

```text
type Named = { Name: string }
```

Use structural shapes inside TypeSharp code when you need shape-based checking. Do not expose structural shapes directly through public .NET APIs; use a named record, class, or interface instead.

## Type-Level Unions

Type-level unions are compile-time-only unions used for local narrowing.

```text
type Input = Customer | string
```

They are useful for local logic, but public boundaries must use CLR-visible alternatives such as nominal unions, interfaces, records, or wrapper classes.

## Collections And Indexers

Simple homogeneous collection expressions lower to arrays by default. If the target type is `List<T>`, they lower to a C# collection initializer.

```text
let numbers: int[] = [1, 2, 3]
let first = numbers[0]
```

Indexer expressions are preserved as C#-compatible array or indexer access.

## Pipelines

Pipeline expressions pass a value into a function call.

```text
export fun total(): int =
  [1, 2, 3] |> sum
```

The current lowering rewrites pipelines to nested function calls that generated C# can compile.

## Async And Tasks

`async fun` lowers to .NET `Task` or `Task<T>`, which lets C# callers use normal task-based async patterns.

```text
export async fun loadName(): Task<string> {
  return "Ada"
}
```

Use async when the public API should interoperate with .NET task-based code.

## C# Interop

TypeSharp can reference local C# DLLs and call supported constructors, methods, properties, fields, indexers, delegates, events, attributes, and generic types.

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
```

```text
import { LegacyFormatter } from "Legacy.Tools"
```

See [.NET Interop](../dotnet-interop/) for the full workflow and current validation boundaries.

## Diagnostics

Diagnostics use stable codes and are available from the CLI and language server. Examples:

- `TS2201`: type mismatch
- `TS2202`: nullability contract violation
- `TS2203`: non-exhaustive match
- `TS2204`: compile-time-only type leaked through public boundary
- `TS2401`: missing local C# DLL reference
- `TS2404`: unknown C# nullability metadata

Use `typesharp explain TS2202` for command-line explanations.

## Current Stability Model

Implemented features are backed by fixtures, smoke tests, or runnable examples. Planned features are documented in design files and task packets, but should not be treated as stable user-facing behavior until tests and docs are added.
