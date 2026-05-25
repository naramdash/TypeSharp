---
title: Fundamentals
description: Core TypeSharp concepts for new users.
---

Start from [Install](../install/) before running local commands: open the tag-specific GitHub Release notes, confirm the exact asset names, download `typesharp-cli-dotnet-<tag>.zip`, verify it with `SHA256SUMS.txt`, extract the CLI, put `typesharp.cmd` on `PATH`, and run `typesharp version`. Use the matching `typesharp-runtime-net48-<tag>.zip` and verify it with the same manifest when Core/Runtime DLLs are needed.

## Values And Functions

Use `let` for immutable values and `fun` for functions.

```tysh
let greeting = "Hello"

export fun greet(name: string): string =
  greeting + ", " + name
```

Functions can use expression bodies or block bodies.

## Modules And Imports

Every source file is part of a module graph. `namespace` gives generated C# a stable namespace. `import` brings TypeSharp or C# types into scope.

```tysh
namespace Samples.Billing

import { LegacyFormatter } from "Legacy.Tools"
```

Use [Modules And Imports](../modules/) for source module paths, relative specifiers, export surface rules, and generated C# container naming. Use [Project Configuration](../project-configuration/) for source roots and manifest settings.

## Type Inference

The current inference engine handles common local literal, identifier, direct call, binary expression, and pipeline flows. Public APIs should still use explicit annotations where C# metadata matters.

`unknown` is the safe gradual-typing boundary. Accessing a member or indexer on an `unknown` value produces `TS2209` until code proves a structural shape or narrower type.

## Structural Shapes Versus Nominal Public API

Structural shapes are useful inside TypeSharp code:

```tysh
type Named = { Name: string }

fun keep(customer: Customer): Customer =
  customer satisfies Named
```

`satisfies` checks that a value fits a named shape while keeping the value's original type. Intersection aliases such as `Named & Aged` can compose named structural shapes for local checking, limited `keyof` can derive local member-name string literal unions from known records or shapes, and limited indexed access types such as `Customer["Name"]` can read known member types. Public .NET metadata must be nominal. Expose a record, class, interface, wrapper, or nominal union instead of a compile-time-only structural shape, intersection alias, `keyof` alias, indexed access alias, or type-level union.

## Records, Classes, Interfaces, And Delegates

Records are immutable-first data types. Expected nominal record expressions can construct records directly and can spread another nominal record value before explicit overrides. Classes and interfaces lower to C#-friendly public shapes for the implemented subset. Delegate declarations and delegate-typed interop are supported in smoke tests.

## Collections, Pipelines, And Async

Simple homogeneous collection expressions lower to arrays by default, or to `List<T>` when an explicit target type is present. Collection spread elements merge known arrays or `List<T>` values while still lowering to C# 7.3-compatible code. Iterator functions can use block-level `yield` with an explicit `IEnumerable<T>` return type, block-level `lock` statements lower to C# monitor locks, and explicit-receiver `extension` methods lower to C# extension methods. Pipeline expressions lower to nested function calls, composition expressions lower to delegate lambdas, `satisfies` erases after type checking, `nameof` lowers to C# `nameof(...)`, and `checked(...)`/`unchecked(...)` lower to C# overflow-context expressions.

```tysh
export fun total(): int {
  let values: int[] = [1, 2, 3]
  values[0]
}
```

```tysh
import { IEnumerable } from "System.Collections.Generic"

export fun names(): IEnumerable<string> {
  yield "Ada"
  yield "Grace"
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
- `TS2228`/`TS2229`/`TS2230`: return, initializer, and assignment value mismatches
- `TS2401`: missing referenced assembly
- `TS2404`: unknown C# nullability

Use [Diagnostics](../diagnostics/) and the release-installed `typesharp explain TS2202` command for explanations.

Use [Type System](../type-system/) for a deeper view of inference, null safety, `unknown`, `dynamic`, structural shapes, unions, generics, async, and public ABI boundaries.
