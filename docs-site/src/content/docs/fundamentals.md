---
title: Fundamentals
description: Core TypeSharp concepts for new users.
---

## Values And Functions

Use `let` for immutable values and `fun` for functions.

```text
let greeting = "Hello"

export fun greet(name: string): string =
  greeting + ", " + name
```

Functions can use expression bodies or block bodies.

## Modules And Imports

Every source file is part of a module graph. `namespace` gives generated C# a stable namespace. `import` brings TypeSharp or C# types into scope.

```text
namespace Samples.Billing

import { LegacyFormatter } from "Legacy.Tools"
```

## Type Inference

The current inference engine handles common local literal, identifier, direct call, binary expression, and pipeline flows. Public APIs should still use explicit annotations where C# metadata matters.

`unknown` is the safe gradual-typing boundary. Accessing a member or indexer on an `unknown` value produces `TS2209` until code proves a structural shape or narrower type.

## Structural Shapes Versus Nominal Public API

Structural shapes are useful inside TypeSharp code:

```text
type Named = { Name: string }
```

Public .NET metadata must be nominal. Expose a record, class, interface, wrapper, or nominal union instead of a compile-time-only structural shape or type-level union.

## Records, Classes, Interfaces, And Delegates

Records are immutable-first data types. Classes and interfaces lower to C#-friendly public shapes for the implemented subset. Delegate declarations and delegate-typed interop are supported in smoke tests.

## Collections, Pipelines, And Async

Simple homogeneous collection expressions lower to arrays by default, or to `List<T>` when an explicit target type is present. Pipeline expressions lower to nested function calls.

```text
export fun total(): int {
  let values: int[] = [1, 2, 3]
  values[0]
}
```

`async fun` lowers to .NET `Task` or `Task<T>`.

## Option, Result, And Nominal Unions

`TypeSharp.Core.Option<T>` and `Result<T, E>` support explicit absence and failure modeling. Nominal unions model closed domain alternatives and support exhaustive `match` diagnostics.

## Diagnostics And Strict Defaults

Diagnostics use stable codes. Examples:

- `TS2201`: type mismatch
- `TS2202`: nullability contract violation
- `TS2204`: compile-time type leaked through public boundary
- `TS2209`: unknown access requires narrowing
- `TS2401`: missing referenced assembly
- `TS2404`: unknown C# nullability

Use [Diagnostics](../diagnostics/) and `typesharp explain TS2202` for explanations.
