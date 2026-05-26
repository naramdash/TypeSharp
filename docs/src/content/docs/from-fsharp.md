---
title: From F#
description: How F# and functional programming developers should read TypeSharp.
---

Start from [Install](../install/) before running examples: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. Use `typesharp runtime-path` when a C# host needs the matching TypeSharp Core/Runtime DLLs bundled with the installed tool.

TypeSharp uses F# as a functional-consistency benchmark, not as a syntax compatibility target. The language supports immutable-first code, records, nominal unions, `Option<T>`, `Result<T,E>`, pattern matching, pipelines, composition, and task-based async while still emitting C# 7.3-compatible `net48` artifacts.

## Mental Model

| F# Concept | TypeSharp Direction | Rule |
| --- | --- | --- |
| Immutable `let` values | `let` values | Immutable by default for ordinary values. |
| Records | `public record` or local records | Public records lower to named CLR-visible immutable data types. |
| Discriminated unions | `public union` | Nominal unions lower to C#-friendly named union metadata. |
| Pattern matching | `match` | Exhaustiveness is implemented for bounded known cases such as nominal unions and known enums. |
| Option/result modeling | `Option<T>` and `Result<T,E>` | Provided through TypeSharp Core helpers, not `FSharp.Core` by default. |
| Pipeline | `|>` | Lowers to C# 7.3-compatible nested calls for the supported first-argument shape. |
| Composition | `>>` and `<<` | Supported for bounded unary function composition with predictable delegate shape. |
| Async workflows | `async fun` returning `Task` or `Task<T>` | TypeSharp uses .NET task interop, not F# computation expressions in the MVP. |
| Partial application and currying | Backlog | Public delegate and overload shapes must stay predictable for C# consumers. |

## Data And Domain Modeling

Use records for public data:

```tysh
namespace Company.Billing

public record Customer(Name: string, Age: int)
```

Use nominal unions for closed alternatives:

```tysh
public union LookupResult {
  Found(Customer)
  Missing(message: string)
}

export fun describe(result: LookupResult): string =
  match result {
    Found(customer) => customer.Name
    Missing(message) => message
  }
```

This is close to an F# DU plus match workflow, but the emitted public shape must remain understandable to a C# `net48` consumer.

## Pipelines And Composition

Pipelines are for clear left-to-right data flow:

```tysh
export fun total(): int =
  [1, 2, 3] |> sum
```

Composition can create unary function values when the shape is explicit enough:

```tysh
export let formatAfterIncrement: int -> string = increment >> format
```

The current rule is intentionally bounded. TypeSharp does not claim full F# currying, partial application, computation expressions, or general higher-order inference as stable 1.0 behavior.

## Option, Result, And Null

Use `Option<T>` when absence is part of the domain and `Result<T,E>` when expected failure should be a value:

```tysh
export fun findName(id: int): Option<string> =
  None
```

Use nullable types at C# interop boundaries when a legacy API naturally returns or accepts null. Prefer explicit wrapping near the boundary so the rest of the TypeSharp code stays clear.

## Common Traps

| Trap | Fix |
| --- | --- |
| Expecting `FSharp.Core` union or option shape. | Use TypeSharp Core `Option<T>`, `Result<T,E>`, and TypeSharp nominal unions. |
| Expecting computation expressions. | Use `async fun` with `Task<T>` for the stable async path. |
| Expecting full currying and partial application. | Write explicit functions or delegate annotations for public/function-valued APIs. |
| Expecting active patterns or type providers. | Treat them as backlog or experimental areas, not stable syntax. |
| Exposing clever inferred function values publicly. | Add explicit function type annotations or use named delegates. |
| Ignoring C# consumers. | Design public APIs as CLR-visible records, classes, interfaces, delegates, and nominal unions. |

## First Reading Path

1. [Language Tour](../language-tour/) from records, unions, `match`, pipelines, and async onward.
2. [Fundamentals](../fundamentals/) for the current inference and expression-oriented subset.
3. [Type System](../type-system/) for nominal unions, pattern matching boundaries, generics, and async.
4. [Feature Status](../feature-status/) for F#-inspired MVP, backlog, and experimental areas.
5. [Lowering](../lowering/) when you need to understand the generated C# shape.
