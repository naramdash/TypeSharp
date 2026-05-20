---
title: Type System
description: Type inference, null safety, unknown, dynamic, structural shapes, unions, generics, and public ABI boundaries.
---

TypeSharp combines TypeScript-style local flexibility, F#-style expression-oriented modeling, and C#/.NET metadata constraints. The result is a type system that is strict by default but honest about what can be emitted as public CLR metadata.

For a C# documentation-style breakdown of strong typing, CLR metadata, value/reference categories, built-in mapping, generics, nullability, and public ABI decisions, use [C# And CLR Type Model](../csharp-type-model/).

## Local Inference

The compiler infers common local values and expression flows so small functions do not need annotation noise.

```tysh
let count = 3
let label = "items"
```

Public functions and public data shapes should still use explicit types because generated C# metadata must be predictable for C# consumers.

## Null Safety

Nullability is a compile-time contract. Returning `null` from a non-null function or assigning nullable data into a non-null location reports a diagnostic instead of relying on runtime failure.

```tysh
export fun displayName(name: string): string = name
```

Interop with legacy C# metadata can produce unknown nullability warnings when the referenced assembly does not declare enough information.

## `unknown`

`unknown` is the safe gradual-typing boundary. Code can receive uncertain data, but it must prove a narrower type or structural shape before member or indexer access.

Accessing an unknown member without narrowing reports `TS2209`.

## `dynamic`

`dynamic` is an explicit escape hatch for .NET dynamic dispatch, reflection-style calls, COM interop, or similar host boundaries. It is not the default fallback type.

TypeSharp keeps dynamic code behind capability markers. Using `dynamic` outside a marked boundary reports diagnostics such as `TS2206` or `TS2207`.

## Structural Shapes

Structural shapes let TypeSharp code describe required members without introducing a nominal type.

```tysh
type Named = { Name: string }

fun keep(customer: Customer): Customer =
  customer satisfies Named
```

The `satisfies` expression checks the proof and then keeps the original expression type. Generated C# emits only the left-hand expression.

Structural shapes are compile-time tools. They must not leak directly through public .NET boundaries. Public APIs should expose records, classes, interfaces, wrappers, or nominal unions.

Intersection aliases compose named structural shapes locally:

```tysh
type Named = { Name: string }
type Aged = { Age: int }
type PersonLike = Named & Aged
```

Limited `keyof` can derive a local string literal union from a known record or named structural shape:

```tysh
record Customer(Name: string, Age: int)
type CustomerKey = keyof Customer
type CustomerName = Customer["Name"]
```

Limited indexed access types can read a known record or shape member type by string literal key. A `keyof`-derived key union produces a local type-level union of the selected member types.

## Nominal Public API

C# consumers see generated CLR metadata. That means public TypeSharp APIs need stable nominal shapes:

- `record` for immutable public data.
- `class` and `interface` for object-oriented interop.
- `delegate` for callback interop.
- nominal `union` for closed domain alternatives.

If a compile-time-only type-level union, intersection alias, or structural shape appears in a public boundary, the compiler reports a public ABI diagnostic.

## Type-Level And Nominal Unions

Type-level unions are useful for local inference, narrowing, and overload reasoning. Nominal unions are the public domain-modeling feature.

`TypeSharp.Core.Option<T>` and `TypeSharp.Core.Result<T,E>` are the standard nominal union types for explicit absence and success/failure modeling. `TypeSharp.Core.Unit` is the standard value representation for `unit` in value or generic position; return-position `unit` lowers to C# `void`.

Use [API And CLI Reference](../api/) for the canonical standard library namespace and helper policy, and [.NET Interop](../dotnet-interop/) for public ABI/runtime ABI rules.

## Iterator Element Checks

Iterator functions use explicit CLR enumerable return types. A block-level `yield` expression is checked against the `IEnumerable<T>` or `IEnumerator<T>` element type before generated C# emission.

```tysh
import { IEnumerable } from "System.Collections.Generic"

export fun names(): IEnumerable<string> {
  yield "Ada"
  yield "Grace"
}
```

```tysh
export union LookupResult {
  Found(Customer)
  Missing(string)
}
```

Pattern matching over nominal unions is the path toward exhaustive domain logic.

## Generics

Generic functions and generic public types lower to C#-compatible shapes for the implemented subset. Generic constraints are represented with C# `where` clauses when supported by the backend.

Use generics for reusable code, but keep public constraints simple enough for C# consumers to understand.

## Async And Tasks

`async fun` lowers to .NET `Task` or `Task<T>`. This keeps TypeSharp async APIs usable from .NET Framework projects.

## Related Pages

- [Fundamentals](../fundamentals/)
- [Modules And Imports](../modules/)
- [C# And CLR Type Model](../csharp-type-model/)
- [.NET Interop](../dotnet-interop/)
- [Diagnostics](../diagnostics/)
