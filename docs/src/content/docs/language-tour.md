---
title: Language Tour
description: A guided tour of TypeSharp language features before the formal reference.
---

TypeSharp is a statically checked language that emits `.NET Framework 4.8` artifacts through generated C# source. It is designed for teams that need long-lived .NET Framework compatibility while adding modern language features and a stricter compile-time workflow.

Start from [Install](../install/) before running tour commands: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. When a tour project needs TypeSharp Core/Runtime DLLs, use `typesharp runtime-path` to locate the matching DLLs bundled with the installed tool.

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

Use the release-installed `typesharp` command: run `typesharp check` while editing and `typesharp build` when you want generated C# and a generated `net48` assembly.

## Namespaces And Imports

`namespace` controls the generated .NET namespace. `import` brings TypeSharp or C# symbols into scope.

```tysh
namespace Company.Billing

import { LegacyFormatter } from "Legacy.Tools"
```

Use stable namespaces for public APIs because C# consumers see the generated names.

## Values And Functions

Use `let` for values and `fun` for functions.

```tysh
let prefix = "Invoice"

export fun label(number: int): string =
  prefix + "-" + number
```

Public functions should use explicit parameter and return types. This keeps generated CLR metadata predictable and makes diagnostics clearer.

## Type Inference

TypeSharp infers common local expression types, including literals, identifiers, direct calls, binary expressions, and pipeline flows.

```tysh
let count = 3
let next = count + 1
```

Prefer explicit annotations at module boundaries, exported functions, and interop boundaries.

## Records

Records are immutable-first data shapes for public data.

```tysh
export record Customer(Name: string, Age: int)
```

Use records when C# callers should see a named data type instead of an anonymous structural shape.

## Classes, Interfaces, And Delegates

Classes and interfaces represent nominal .NET shapes. Delegates let TypeSharp interact with C# callback APIs.

```tysh
export interface IRule {
  fun validate(value: string): bool
}

export delegate Transform(value: string): string
```

Use nominal declarations for public contracts because they lower to C#-understandable metadata.

## Option And Result

Use `Option<T>` for explicit absence and `Result<T, E>` for recoverable failure. This is clearer than passing `null` through code and relying on runtime checks.

```tysh
export fun findName(id: int): Option<string> =
  None
```

Nullability diagnostics report when a nullable value flows into a non-null contract.

## Nominal Unions And Pattern Matching

Nominal unions model closed alternatives.

```tysh
export union LookupResult {
  Found(Customer)
  Missing(string)
}
```

`match` expressions let you handle each case. The checker reports missing union cases for implemented exhaustive paths.

## Structural Shapes

Structural shapes describe required members for compile-time checks.

```tysh
type Named = { Name: string }

fun keep(customer: Customer): Customer =
  customer satisfies Named
```

Use structural shapes inside TypeSharp code when you need shape-based checking. `satisfies` verifies that a value fits a named shape while preserving the value's original type and erasing to that value in generated C#. Named shapes can be composed locally with `A & B` intersection aliases. Do not expose structural shapes or intersection aliases directly through public .NET APIs; use a named record, class, or interface instead.

## Type-Level Unions

Type-level unions are compile-time-only unions used for local narrowing.

```tysh
type Input = Customer | string
```

They are useful for local logic, but public boundaries must use CLR-visible alternatives such as nominal unions, interfaces, records, or wrapper classes.

## Collections And Indexers

Simple homogeneous collection expressions lower to arrays by default. If the target type is `List<T>`, they lower to a C# collection initializer. `...` spread elements can merge known arrays or `List<T>` values and lower through C# 7.3-compatible LINQ concatenation.

```tysh
let numbers: int[] = [1, 2, 3]
let first = numbers[0]
```

Indexer expressions are preserved as C#-compatible array or indexer access.

## Record Expressions

Expected nominal record expressions lower to constructor calls. A record expression can spread another nominal record value and then override copied fields explicitly.

```tysh
export fun older(customer: Customer): Customer =
  { ...customer, Age: customer.Age + 1 }
```

Iterator functions can use block-level `yield` when the return type is an explicit CLR enumerable.

```tysh
import { IEnumerable } from "System.Collections.Generic"

export fun names(): IEnumerable<string> {
  yield "Ada"
  yield "Grace"
}
```

Each yielded expression is checked against the enumerable element type and lowers to C# `yield return`.

Block-level `lock` statements lower to C# `lock` blocks and require a non-null reference gate when the gate type is known.

Extension methods use an explicit receiver parameter and lower to C# extension methods for C# consumers.

## Pipelines

Pipeline expressions pass a value into a function call. Composition expressions build unary delegate values from callable targets.

```tysh
export fun total(): int =
  [1, 2, 3] |> sum

export let formatAfterIncrement: int -> string = increment >> format
```

The current lowering rewrites pipelines to nested function calls and composition to C# delegate lambdas that generated `net48` projects can compile.

## Nameof

`nameof` keeps C#-style name references refactoring-safe while still lowering to C# 7.3-compatible source.

```tysh
export fun fieldName(): string = nameof(Customer.Name)
```

## Checked And Unchecked

Overflow context expressions preserve the inner expression type and lower directly to C# `checked(...)` or `unchecked(...)`.

```tysh
export fun checkedAdd(value: int): int = checked(value + 1)
```

## Async And Tasks

`async fun` lowers to .NET `Task` or `Task<T>`, which lets C# callers use normal task-based async patterns.

```tysh
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

```tysh
import { LegacyFormatter } from "Legacy.Tools"
```

See [.NET Interop](../dotnet-interop/) for the full workflow and current validation boundaries.

## Diagnostics

Diagnostics use stable codes and are available from the CLI and language server. Examples:

- `TS2201`: type mismatch
- `TS2202`: nullability contract violation
- `TS2203`: non-exhaustive match
- `TS2204`: compile-time-only type leaked through public boundary
- `TS2228`/`TS2229`/`TS2230`: return, initializer, and assignment value mismatches
- `TS2401`: missing local C# DLL reference
- `TS2404`: unknown C# nullability metadata

Use `typesharp explain TS2202` for command-line explanations.

## Current Stability Model

Implemented features are backed by fixtures, smoke tests, or runnable examples. Planned features are documented separately, but should not be treated as stable user-facing behavior until tests and docs are added.
