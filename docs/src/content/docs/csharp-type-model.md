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
| `record`, `class`, `interface`, `delegate`, nominal `union` | Named CLR type | Preferred public API shapes. |
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
| Delegate | `public delegate` | Use for callbacks and events. |
| Enum | Imported C# enum metadata | Native TypeSharp enum declaration remains future work unless canonical docs change. |
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

## Compile-Time-Only Types

TypeScript-style flexibility stays local unless it is wrapped in a named public type.

```tysh
type DraftShape = {
  CustomerId: string,
  Amount: decimal
}

type DraftInput = DraftShape | string
```

These shapes help TypeSharp check source. They do not have stable CLR metadata names. Public functions must use named records, classes, interfaces, delegates, nominal unions, framework types, or core library types.

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

Local function types can be convenient, but public callbacks should prefer named delegates because C# consumers understand delegate metadata and event patterns.

```tysh
namespace Company.Billing

public record InvoiceId(value: string)

public delegate InvoiceChanged(sender: object, id: InvoiceId): unit
```

Function-valued `export let` declarations lower only when explicitly annotated and currently use `System.Func` or `System.Action` shapes. For long-lived public API, a named delegate is clearer.

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
| Callback or event handler | `public delegate` | inferred function type |
| Success/failure result | `Result<T,E>` | throwing for ordinary domain failure |
| Optional value | `Option<T>` or nullable when CLR-friendly | sentinel strings or untyped `object` |
| Local flexible parsing | type-level union or shape | exposing those directly as public ABI |

## Related Pages

- [Type System](../type-system/)
- [.NET Interop](../dotnet-interop/)
- [Runtime Artifacts](../runtime-artifacts/)
- [Members And Overloads](../csharp-members-overloads/)
- [Feature Status](../feature-status/)
