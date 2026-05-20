---
title: C# Members And Overloads
description: Detailed TypeSharp guidance for C#-style members, imports, constructors, overloads, delegates, events, byref parameters, extension methods, and diagnostics.
---

This page gives TypeSharp the kind of detailed member-level reference that Microsoft Learn provides for C# classes, language reference topics, operators, statements, and exception handling.

Official sources reviewed on 2026-05-21:

- [Object-oriented techniques in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- [C# keywords](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/)
- [Operators and expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/)
- [Exceptions and exception handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)

TypeSharp's rule is conservative: if a C# member interaction cannot be validated clearly, report a diagnostic before generated C# emission.

## Member Surface

C# named types can expose fields, constants, properties, methods, constructors, events, finalizers, indexers, operators, and nested types. TypeSharp supports or classifies those surfaces as follows.

| C# Member Kind | TypeSharp Status | Interop Rule |
| --- | --- | --- |
| Field | Imported and emitted for selected shapes | Read/write validation checks static, instance, and readonly cases. |
| Constant | Imported and emitted for literals | Public literal export aliases can lower to constants or readonly fields. |
| Property | Imported and emitted | Missing setters report diagnostics before build. |
| Method | Imported and emitted | Arity, overload, generic, byref, optional, named, and receiver checks run before build. |
| Constructor | Imported and emitted for classes | Named, optional, `params`, generic type substitution, ambiguity, and no-match cases are diagnosed. |
| Event | Imported and emitted | Delegate-compatible add/remove calls are validated for the implemented subset. |
| Indexer | Imported metadata calls | Known receiver and argument metadata drive exact and relationship ranking. |
| Operator | Limited expression lowering | Full C# operator overloading is not stable TypeSharp surface yet. |
| Nested type | Imported when metadata reader can resolve it | Native nested declarations remain future work unless canonical docs change. |
| Finalizer | Not TypeSharp-authored stable surface | Imported metadata may exist, but TypeSharp docs should not recommend authoring finalizers. |

## Import And Receiver Model

TypeSharp imports C# namespaces and public types through the same `import` syntax used for source modules.

```tysh
namespace Samples.CSharpMembers

import { Console, StringComparer } from "System"
import { StringBuilder } from "System.Text"
import static System.Math
```

Receiver metadata is preserved for:

- imported C#-typed parameters,
- constructor-created locals,
- explicit local annotations,
- simple local aliases,
- assignment-updated locals where the compiler can track the type.

That metadata lets TypeSharp validate member calls instead of waiting for the C# compiler.

## Constructors

Constructor calls use the imported type name.

```tysh
import { StringBuilder } from "System.Text"

export fun makeBuilder(seed: string): StringBuilder =
  StringBuilder(seed)
```

Validation includes:

- required argument count,
- named argument names,
- optional parameter omission,
- `params` expansion,
- generic constructor type substitution in imported metadata,
- no matching constructor diagnostics,
- ambiguous constructor diagnostics.

## Methods And Overload Ranking

TypeSharp overload resolution is deliberately narrower than full C# overload resolution.

Current ranking prefers:

1. arity and named argument match,
2. exact nominal type match,
3. nullable compatibility,
4. known literal and fitting numeric conversions,
5. imported metadata relationship distance,
6. generic inference and constraint satisfaction in implemented positions,
7. delegate/lambda contextual checks,
8. `object` fallback,
9. explicit `dynamic` boundary.

If two candidates remain equally plausible, TypeSharp reports ambiguity instead of relying on generated C# compilation.

## Named, Optional, And Params Arguments

```tysh
import { String } from "System"

export fun formatAmount(amount: decimal): string =
  String.Format(format: "{0:0.00}", arg0: amount)
```

Rules:

- named arguments must match public parameter names,
- required parameters must be supplied,
- optional parameters can be omitted only when metadata says they are optional,
- expanded `params` arguments are checked against the element type,
- invalid combinations report diagnostics before emission.

## Ref, Out, And In

C# byref parameters are part of the public member signature. TypeSharp validates selected `ref`, `out`, and `in` calls against imported metadata.

```tysh
import { Decimal } from "System"

export fun tryAmount(text: string): decimal {
  let mut parsed = 0m

  if Decimal.TryParse(text, out parsed) {
    parsed
  }
  else {
    0m
  }
}
```

Byref arguments must be addressable variables with compatible types. Passing a literal or incompatible expression reports a diagnostic.

## Properties, Fields, And Indexers

Property and field access preserve C# getter/setter and readonly rules.

```tysh
import { List } from "System.Collections.Generic"

export fun firstName(names: List<string>): string =
  names[0]
```

Rules:

- property get requires a readable property,
- property set requires a setter,
- readonly fields cannot be assigned,
- known indexer arguments are checked against available indexers,
- ambiguous indexers report diagnostics.

## Delegates And Lambdas

Delegate calls use imported delegate metadata. Lambda contextual typing supports the implemented subset of `System.Func`, `System.Action`, and imported delegate targets.

```tysh
import { Func } from "System"

export fun keepLongerThan(min: int): Func<string, bool> =
  text => text.Length > min
```

The checker validates:

- lambda arity,
- known delegate return type,
- identity lambda parameter return,
- member-chain return bodies,
- method-call return bodies,
- static method-call return bodies,
- extension method-call return bodies,
- binary predicate bodies that return `bool`,
- null-coalescing bodies with known fallback or receiver-side types.

## Events

Events are delegate-backed C# members. TypeSharp validates event add/remove when receiver metadata is known and the handler type is compatible.

```tysh
public delegate ChangedHandler(sender: object): unit

public class ObservableCounter {
  public event Changed: ChangedHandler
}
```

When consuming imported C# events, the receiver must expose a public event and the handler must match the event delegate.

## Extension Methods

Imported C# extension methods are available when the extension type namespace is imported or opened. TypeSharp-authored explicit-receiver extension methods lower to C# extension methods.

```tysh
namespace Company.Billing

public record Money(amount: decimal, currency: string)

public extension MoneyExtensions {
  public fun (money: Money).isZero(): bool =
    money.amount == 0m
}
```

Extension receiver ranking prefers closer metadata relationships. `object` receiver fallback is accepted only after more specific applicable receivers.

## Exceptions And Domain Failures

C# documentation describes `try`, `catch`, `finally`, and `throw` as runtime exception handling. TypeSharp interop must recognize that imported C# APIs can throw even when the TypeSharp type signature is valid.

Guidance:

- Use `Result<T,E>` for expected domain failures in TypeSharp APIs.
- Let truly exceptional .NET failures remain exceptions unless the boundary deliberately wraps them.
- Do not document exception swallowing as a normal interop pattern.
- Use wrapper functions when a legacy API throws for ordinary validation outcomes.

```tysh
import { Result, Ok, Error } from "TypeSharp.Core"
import { Decimal } from "System"

public union ParseError {
  InvalidDecimal(source: string)
}

export fun parseMoney(text: string): Result<decimal, ParseError> {
  let mut parsed = 0m

  if Decimal.TryParse(text, out parsed) {
    Ok(parsed)
  }
  else {
    Error(InvalidDecimal(text))
  }
}
```

## Diagnostics

| Situation | Expected Diagnostic Family |
| --- | --- |
| Unsupported `references.packages` | `TS2405` |
| Missing imported type from known metadata namespace | `TS2408` |
| Missing static method on known metadata type | `TS2407` |
| Missing static member on known metadata type | `TS2409` |
| No applicable method or constructor overload | `TS2406` |
| Ambiguous imported method or constructor candidates | `TS2402` |
| Imported generic constraint violation | `TS2417` |
| Unknown imported nullability in strict context | `TS2404` |
| Dynamic or reflection boundary without marker | `TS2206`-`TS2208` |

Diagnostics should describe the next action: add a reference, import the right namespace, annotate the type, wrap the boundary, or choose an explicit overload.

## Related Pages

- [C# And CLR Type Model](../csharp-type-model/)
- [.NET Interop](../dotnet-interop/)
- [Diagnostics](../diagnostics/)
- [Lowering](../lowering/)
- [Runtime Artifacts](../runtime-artifacts/)
