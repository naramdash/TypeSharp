---
title: C# And CLR Type Model
description: Detailed TypeSharp mapping for C# strong typing, CLR metadata, value/reference types, generics, nullability, and public ABI rules.
---

This page follows the detailed-topic style used by Microsoft Learn C# docs and adapts it to TypeSharp's `net48` generated artifact model.

Official sources reviewed on 2026-05-21:

- [C# language documentation](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [The C# type system](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/)
- [Object-oriented techniques in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- [C# language reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/)

TypeSharp is not a C# dialect. It emits C# 7.3-compatible source and `net48` assemblies, so every public TypeSharp type must eventually become understandable CLR metadata.

## Strong Typing And Metadata

C# documentation explains language behavior through types, expressions, compiler checks, and CLR metadata. TypeSharp uses the same deployment reality but with stricter source-level boundaries:

- every expression has a TypeSharp type before lowering,
- every public declaration must have a CLR-compatible metadata shape,
- compile-time-only types are allowed locally,
- public metadata cannot depend on structural shapes, inferred anonymous function types, or type-level unions,
- generated assemblies must load from ordinary .NET Framework 4.8 projects without a custom loader.

The compiler should reject unclear public metadata before generated C# emission.

```tysh
namespace Company.Billing

public record InvoiceId(value: string)

type LocalInput = string | InvoiceId

fun normalize(input: LocalInput): InvoiceId =
  match input {
    text: string => InvoiceId(value: text)
    id: InvoiceId => id
  }

export fun parseInvoiceId(text: string): InvoiceId =
  normalize(text)
```

`LocalInput` is useful during TypeSharp checking. It is not exposed in the public function signature.

## Value Types And Reference Types

C# separates value types from reference types because assignment, nullability, boxing, and metadata differ. TypeSharp follows that distinction when it maps to CLR types.

| TypeSharp Shape | CLR Category | Public Boundary Rule |
| --- | --- | --- |
| `bool`, numeric primitives, `char` | Value type | Direct primitive mapping. |
| `decimal` | Value type | Direct `System.Decimal` mapping for financial data. |
| `string`, `object` | Reference type | Non-null by default in TypeSharp source unless annotated nullable. |
| `T?` where `T` is a value type | `System.Nullable<T>` | Public metadata can represent this directly. |
| `T?` where `T` is a reference type | Nullable reference contract | Runtime type is still `T`; metadata annotations can be incomplete in legacy assemblies. |
| `record`, `class`, `interface`, nominal `union` | Named CLR type | Preferred TypeSharp-authored public API shapes. Imported C# delegates are also supported through metadata. |
| structural shape, type-level union, intersection alias | Compile-time only | Rejected directly at public CLR boundaries. |

## Built-In Type Mapping

| TypeSharp | C# / CLR Metadata | Notes |
| --- | --- | --- |
| `bool` | `System.Boolean` | Not convertible to an integer. |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | CLR integral types | Numeric overload checks use exact and fitting conversions in the implemented subset. |
| `float`, `double`, `decimal` | `System.Single`, `System.Double`, `System.Decimal` | `decimal` is preferred for money-like examples. |
| `char` | `System.Char` | Character primitive. |
| `string` | `System.String` | Reference type; non-null in TypeSharp unless nullable. |
| `object` | `System.Object` | Explicit wide boundary, not a default fallback. |
| `unit` return | `void` | Return-position `unit` lowers to `void`. |
| `unit` value | `TypeSharp.Core.Unit` | Used in value and generic positions. |
| `T[]` | CLR array | Direct array interop. |
| `Task<T>` | `System.Threading.Tasks.Task<T>` | Async public interop shape. |
| `Option<T>`, `Result<T,E>` | `TypeSharp.Core` nominal unions | Standard absence and success/failure modeling. |

## Named Types

C# documentation treats classes, structs, records, interfaces, delegates, and enums as named type declarations. TypeSharp exposes a smaller, `net48`-friendly public set today:

| C# Concept | TypeSharp Public Shape | Current Rule |
| --- | --- | --- |
| Class | `public class` | Use for object identity, mutable state, or C# consumer object APIs. |
| Interface | `public interface` | Use for C#-friendly contracts. |
| Record | `public record` | Use for immutable data and stable generated properties. |
| Delegate | `public delegate`, imported C# delegate metadata, or explicit `System.Func`/`System.Action` shape | TypeSharp-authored named delegates lower to CLR delegate metadata, including declaration attributes, in the implemented 1.0 slice; imported C# delegates and function-type annotations remain supported. |
| Enum | `public enum` or imported C# enum metadata | TypeSharp-owned enums lower to CLR enums in the implemented subset; imported C# enums are metadata-backed. |
| Struct | Imported C# struct metadata | Native struct declaration remains future work unless canonical docs change. |
| Nominal union | `public union` | Generated as a named CLR-visible domain type backed by runtime helpers. |

Use nominal types when a C# project will reference the generated assembly.

```tysh
namespace Company.Billing

public record Money(amount: decimal, currency: string)

public interface DiscountPolicy {
  fun apply(input: Money): Money
}
```

## Public Declaration ABI Matrix

This matrix is the current 1.0 ABI ledger for TypeSharp-authored declarations. "Public ABI slice" means the documented subset lowers to C# 7.3-compatible `net48` metadata and has generated build or C# consumer evidence. "Deferred from 1.0" means the syntax or model can remain parser-visible, but TypeSharp must not claim it as a stable public ABI form until a later version adds lowering, diagnostics, and C# consumer evidence. "Compile-time-only" means it can help TypeSharp source checking but must not cross a public CLR boundary directly.

| TypeSharp declaration form | 1.0 public ABI status | CLR-visible shape | Current evidence and boundary |
| --- | --- | --- | --- |
| `fun` | Public ABI slice | Static method on generated module/container; `params` and optional/default metadata for the supported suffix rules. | Generic function plus generated public ABI snapshots and generated `net48` C# consumer smokes cover the current explicit, `params`, optional/default, generic constraint, and generic optional/default parameter subsets. Function overload ranking and broader higher-order inference remain backlog. |
| `record` | Public ABI slice | Named immutable CLR class with declaration attributes, `partial` when declared, constructor/properties plus generated equality/hash members. | Backend snapshots and C# consumer smokes cover immutable records, declaration attributes, partial modifier preservation, record updates, and record expression construction. |
| `class` | Public ABI slice, MVP limited | Named CLR class with optional generic parameters/constraints, declaration attributes, `partial`, an implicit public parameterless constructor when no parameter list is declared, an explicit constructor parameter list with CLR-visible parameter types, public instance/static `fun` methods, typed instance/static `let` and `let mut` values, typed instance/static getter-only properties with explicit initializers, typed instance/static mutable get/set auto-properties with explicit initializers, typed instance/static `event` members backed by named delegate types, and supported member attributes on emitted methods, values, properties, and events. | Class API, class declaration attribute, class member attribute, generic type, generic constraint metadata snapshots, partial declaration, constructor parameter-list, instance/static method members, instance/static value members, instance/static getter-only and get/set property members, instance/static event members, unsupported member diagnostic, generated public ABI snapshots, and C# consumer smokes cover the 1.0 subset. TypeSharp-authored constructor bodies, constructor parameter capture, constructor default parameters, custom property accessors, explicit inheritance/implementation clauses, static member forms beyond class static methods/values/events/properties, abstract/virtual/override members, indexers, operators, accessor/parameter attributes, nested types, partial methods, broader attribute target validation, and broader member-body analysis remain backlog. |
| `interface` | Public ABI slice, MVP limited | Named CLR interface with optional generic parameters/constraints, declaration attributes, `partial`, method signatures, typed instance getter-only properties, typed instance mutable get/set properties, typed instance `event` members backed by named delegate types, and supported member attributes on emitted methods, properties, and events. | Interface API, interface declaration attribute, interface member attribute, interface getter-only and get/set properties, interface event, generic constraint, partial declaration, unsupported member diagnostic, generated public ABI snapshots, and C# consumer smokes cover the 1.0 subset. Broader member kinds, custom property accessors, custom add/remove events, interface static events, indexers, operators, inheritance clauses, default implementations, accessor/parameter attributes, nested types, partial methods, and broader attribute target validation remain backlog. |
| `delegate` | Public ABI slice | Named CLR delegate with optional generic parameters, supported C# 7.3-compatible generic constraints, declaration attributes, typed parameters, optional `params`, and an explicit or `void` return. | Delegate declaration backend snapshots, generated public ABI snapshots, and generated `net48` C# consumer smokes cover the current subset, including declaration attribute, generic constraint, and `params` metadata. Delegate overload ranking, defaulted parameters, variance annotations, and nested delegates remain backlog. |
| `event` | Public ABI slice, MVP limited | CLR event member on a generated class or interface using a named delegate type; class events may be instance or static. | Class/interface event backend snapshots, generated public ABI snapshots, generated metadata checks, and generated `net48` C# consumer smokes cover subscription to generated class instance/static and interface event metadata. Custom add/remove accessors, event invocation helpers, interface static events, and nullable receiver/event access remain backlog. |
| `enum` | Public ABI slice, MVP limited | CLR enum with declaration/member attributes, optional integral underlying type, and selected constant/member forms. | Enum declaration API, declaration/member attribute metadata, match exhaustiveness, same-enum bitwise operations, generated `net48` build, and C# consumer smokes cover the current subset. Arbitrary computed enum values and flag-aware pattern algebra remain backlog. |
| `union` | Public ABI slice, MVP limited | Named abstract CLR base type with declaration attributes, `partial` when declared, nested sealed case types, and runtime helper metadata. | Nominal union API, declaration attribute, partial modifier preservation, union match lowering, runtime helper, and C# consumer smokes cover the current class-hierarchy representation. Alternative representations and richer recursive ergonomics remain backlog. |
| `type` alias to named CLR-visible type | Source ABI convenience, not a new CLR type | Alias erased to the referenced named type or generated using-alias shape. | Useful for source modules and imports, but it does not create a stable independent binary contract. Public APIs should expose the underlying named CLR-visible type directly when binary identity matters. |
| `type` alias, public parameter, public return, or public value using union, structural shape, intersection, `keyof`, indexed access, `unknown`, or anonymous shape | Compile-time-only | No direct CLR metadata shape. | Public boundary diagnostics reject these forms before generated C# emission. Wrap them in a nominal union, record, class, interface, imported C# delegate, or explicit function type before export. |
| Explicit-receiver extension method | Public ABI slice, MVP limited | Static C# extension method in generated helper container. | Backend snapshots and C# consumer smokes cover explicit non-null receiver methods. Nullable receiver lifting, richer conversion ranking, static extension members, and operators remain backlog. |
| Getter-only extension property | Public ABI slice, MVP limited | Static helper method such as `GetName(this T receiver)`; not a CLR property. | Backend snapshots and C# consumer smokes cover getter-only helper lowering and diagnostics for assignment/collisions. Setters, nullable receiver lifting, imported extension property metadata, and C# 14 extension-block emission remain backlog. |

Do not treat parser visibility as public ABI stability. A declaration form becomes a 1.0 public ABI claim only after the compiler rejects unsupported public shapes before emission and the generated `net48` artifact can be referenced from an ordinary C# consumer.

## Compile-Time-Only Types

TypeScript-style flexibility stays local unless it is wrapped in a named public type.

```tysh
type DraftShape = {
  CustomerId: string,
  Amount: decimal
}

type DraftInput = DraftShape | string
```

These shapes help TypeSharp check source. They do not have stable CLR metadata names. Public functions must use named records, classes, interfaces, nominal unions, framework types, core library types, imported C# delegates, or explicit `System.Func`/`System.Action` function-type annotations.

## Nullability

TypeSharp nullability is a source-level contract. The compiler should catch incompatible null flow before generated C# runs.

| Situation | TypeSharp Rule |
| --- | --- |
| Return `null` from non-null function | Diagnostic. |
| Assign nullable value to non-null target | Diagnostic. |
| Import legacy C# metadata with unknown nullability | Report or warn according to strictness and context. |
| Public nullable value type | Lower to `System.Nullable<T>`. |
| Public nullable reference type | Keep CLR type and preserve nullable contract where metadata supports it. |

When interop metadata is incomplete, isolate the boundary with a wrapper function and return a clear nominal type.

The 1.0 warning-versus-error policy is fixed: strict-mode imported C# reference returns with missing nullable metadata produce warning `TS2404`, while TypeSharp-owned nullability contract violations produce error `TS2202`. Public `unknown` and other compile-time-only public boundaries produce error `TS2204`, and member or indexer access on an unproved `unknown` value produces error `TS2209`.

## Generics And Constraints

Generic TypeSharp APIs must lower to C#-readable type parameters and constraints for the implemented subset.

```tysh
namespace Company.Billing

export fun identity<T>(value: T): T = value
```

Interop with C# generic methods and constructors validates:

- generic arity,
- explicit type arguments,
- direct inferred type arguments,
- selected constructed generic argument positions,
- imported metadata constraints,
- base/interface relationship constraints.

If TypeSharp cannot prove a C# constraint, it reports a diagnostic before generated C# build.

## Arrays, Collections, And Tuples

CLR arrays map directly. BCL collections such as `List<T>` and `Dictionary<TKey,TValue>` are imported from .NET namespaces.

```tysh
import { List } from "System.Collections.Generic"

export fun countNames(names: List<string>): int =
  names.Count
```

Tuple and anonymous object public boundaries remain unsuitable unless a TypeSharp design update explicitly defines their CLR metadata policy. Prefer named records for public data.

## Function Types And Delegates

Local function types can be convenient. In the 1.0 public ABI slice, public callbacks can use TypeSharp-authored named delegates, imported C# delegates, or explicit function type annotations that lower to `System.Func`/`System.Action`.

```tysh
namespace Company.Billing

public record InvoiceId(value: string)

export let onInvoiceChanged: object -> InvoiceId -> unit = sender => id => ()
```

Function-valued `export let` declarations lower only when explicitly annotated and currently use `System.Func` or `System.Action` shapes. Use `public delegate` when the public ABI should expose a stable named callback type; imported C# delegates are still supported through metadata-backed interop.

## Async Tasks

Public async TypeSharp APIs use .NET `Task` or `Task<T>` so C# consumers can await them from ordinary .NET code.

```tysh
import type { Task } from "System.Threading.Tasks"

public record InvoiceId(value: string)

export async fun refreshAsync(id: InvoiceId): Task<InvoiceId> {
  return id
}
```

Generated code must remain compatible with .NET Framework 4.8. Do not document APIs that require newer runtime-only async features unless they are explicitly preview-gated and backed by compatibility evidence.

## Public Type Decision Matrix

| Need | Prefer | Avoid |
| --- | --- | --- |
| Immutable data for C# consumers | `public record` | anonymous object, structural shape |
| Mutable object or service facade | `public class` | global mutable state |
| Host-provided behavior contract | `public interface` | structural shape in public signature |
| Callback or event handler | TypeSharp-authored `public delegate`, TypeSharp-authored class/interface `event`, imported C# delegate/event, or explicit function type annotation | inferred function type or custom/interface-static event declarations before event lowering evidence |
| Success/failure result | `Result<T,E>` | throwing for ordinary domain failure |
| Optional value | `Option<T>` or nullable when CLR-friendly | sentinel strings or untyped `object` |
| Local flexible parsing | type-level union or shape | exposing those directly as public ABI |

## Related Pages

- [Type System](../type-system/)
- [.NET Interop](../dotnet-interop/)
- [Runtime Artifacts](../runtime-artifacts/)
- [Members And Overloads](../csharp-members-overloads/)
- [Feature Status](../feature-status/)
