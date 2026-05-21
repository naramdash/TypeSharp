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

## TypeScript-Style Structural Roadmap

TypeSharp follows TypeScript's structural typing as a local proof model, not as a public CLR metadata model. The practical roadmap is:

- keep shape checks, `satisfies`, intersections, local type-level unions, limited `keyof`, and indexed access local-only unless wrapped in a named public type;
- support bounded TypeScript-style discriminant narrowing for `value.Tag == "literal"` and `value.Tag != "literal"` when a local type-level union's structural members expose a required literal tag property;
- add optional-member and index-signature behavior only after the checker can report deterministic missing/possibly-undefined member diagnostics;
- treat mapped, conditional, template-literal, and utility types as backlog until TypeSharp has the budgeted evaluator contract below implemented with diagnostics;
- prefer generated nominal interfaces or wrappers for any future structural public ABI adapter.

## Advanced Type Operator Evaluator Budget

Mapped, conditional, template-literal, and utility types are accepted design direction only as bounded compile-time evaluation. They must never become an open-ended TypeScript compatibility layer or a way to smuggle structural/computed shapes into public CLR metadata.

Initial evaluator inputs should be restricted to TypeSharp-owned type facts:

- primitive, nominal, array, tuple, function, literal, nullable, and local type-level union types;
- records, classes, interfaces, delegates, nominal unions, and named structural shapes known to the current compilation;
- current limited `keyof` and indexed access facts over known records and named shapes;
- generic type parameters with explicit constraints when the constraint has a finite property/member set.

Initial evaluator outputs must normalize to one of:

- a single CLR-visible type such as a primitive, nominal declaration, delegate-compatible function type, array, or nullable form;
- a local compile-time-only structural shape, literal union, type-level union, or key set;
- a deterministic diagnostic when evaluation would be ambiguous, too large, cyclic, unsupported, or public-ABI unsafe.

Budget limits for the first implementation:

| Budget | Limit | Purpose |
| --- | --- | --- |
| Alias instantiation depth | 16 nested type-operator expansions | Stops recursive utility aliases and mutually recursive conditional aliases. |
| Total evaluator steps per root alias | 512 reductions | Keeps `typesharp check` responsive and deterministic. |
| Normalized union width | 64 members | Prevents distributive conditionals and template products from exploding. |
| Mapped key count | 64 keys | Keeps mapped record/shape transforms inspectable. |
| Conditional distribution branches | 64 branches | Allows small local unions while rejecting unbounded distribution. |
| Template literal product | 128 generated strings | Requires ahead-of-time generation or nominal alternatives for larger string sets. |
| Diagnostic cascade per root alias | 8 direct evaluator diagnostics | Reports the root cause without flooding the user. |

Evaluation rules:

- Cache evaluations by alias symbol, normalized type arguments, and source scope so repeated references do not multiply work.
- Detect cycles through the active instantiation stack before counting budget exhaustion.
- Distribute conditional types over type-level unions only when the branch count stays under budget.
- Do not infer through C# overload sets, dynamic calls, reflection, imported declaration files, or runtime values.
- Do not evaluate user code, invoke generators, restore packages, or read JavaScript/TypeScript declaration files during type computation.
- Prefer rejecting an advanced operator with a deterministic diagnostic over partially evaluating it and emitting misleading C#.

Utility type admission:

- `Pick`, `Omit`, `Readonly`, `Mutable`, `Partial`, `Required`, `Record`, `Extract`, `Exclude`, `NonNullable`, `ReturnType`, and `Parameters` can be considered only when each has a TypeSharp-owned lowering-independent definition inside the evaluator.
- Utilities that depend on JavaScript object semantics, declaration merging, broad `any`, overload-last inference, or ambient TypeScript library behavior stay out of the stable contract.
- A utility type may be documented as implemented only after it has parser/checker coverage, public-boundary diagnostics, and generated `net48` smoke evidence when it can affect emitted signatures.

Public boundary rule:

- An evaluated advanced type may appear in public API only after it fully normalizes to a CLR-visible type that TypeSharp can emit and C# consumers can understand.
- If the normalized result is structural, a type-level union, a literal-only key set, a template-generated string union, or an unresolved computed form, public use reports the public-boundary diagnostic before generated C# emission.
- Future structural public ABI adapters must use generated nominal wrappers or interfaces with a versioned naming policy; they are separate from the evaluator budget.

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

## F#-Style Functional Roadmap

TypeSharp follows F#'s functional consistency where the behavior can remain explainable to C# and .NET Framework consumers:

- immutable values, expression-oriented functions, records, nominal unions, `Option<T>`, `Result<T,E>`, pattern matching, pipeline, and composition are part of the MVP path;
- richer exhaustiveness is the next functional correctness priority; known nominal unions and local type-level unions report missing cases/members, `_` can cover the remainder, and guard interactions plus bool/enum cases remain next;
- struct-backed value options, recursive union ergonomics, and helper APIs such as bind/map/default are stable backlog items once their `net48` ABI and allocation tradeoffs are documented;
- general currying, partial application, computation-expression-style workflows, active-pattern-style extractors, units of measure, and type providers stay backlog or experimental until TypeSharp has deterministic lowering, diagnostics, and security boundaries.

The default TypeSharp functional model must not require `FSharp.Core` at runtime. Direct F# option, tuple, record, or union interop can be added later as a compatibility layer.

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
