---
title: C# Members And Overloads
description: Detailed TypeSharp guidance for C#-style members, imports, constructors, overloads, delegates, events, byref parameters, extension members, and diagnostics.
---

This page gives TypeSharp the kind of detailed member-level reference that Microsoft Learn provides for C# classes, language reference topics, operators, statements, and exception handling.

Official sources reviewed on 2026-05-22:

- [Object-oriented techniques in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/)
- [C# keywords](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/)
- [Operators and expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/)
- [Exceptions and exception handling](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)

TypeSharp's rule is conservative: if a C# member interaction cannot be validated clearly, report a diagnostic before generated C# emission.

## C# Release Parity Boundary

TypeSharp tracks current C# releases as design input, but generated `net48` source stays C# 7.3-compatible. C# 14 unbound generic `nameof` is implemented through TypeSharp parsing, type-root binding, and string constant lowering; C# 14-inspired extension properties and null-conditional assignment/member-read/indexer-read plus null-conditional additive, bitwise, and logical unsigned shift member-target and indexer-target slices are implemented only where TypeSharp can lower them to C# 7.3-compatible helpers/guards. Lambda parameter modifiers, partial events/constructors, static extension members, and related ergonomics must likewise be expressed as TypeSharp semantics with C# 7.3 lowering rather than by emitting newer C# syntax. C# 15 collection expression arguments and union types are Preview Watch signals only.

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
| Operator | Limited expression lowering | TypeSharp-authored operator declarations are post-1.0. Current operator expressions use built-in numeric/enum/bool/assignment rules or selected imported C# static binary metadata; records, classes, interfaces, unions, and extension declarations cannot introduce overload or conversion operators. |
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

The 1.0 overload and conversion contract freezes the implemented ranking set instead of promising full C# compiler parity.

| Rank Input | 1.0 Rule | Primary Evidence |
| --- | --- | --- |
| Arity, named arguments, optional parameters, `params`, and byref modifiers | Candidates must match the supplied positional/named/byref shape before later ranking applies. A single compatible collection expression in the final `params T[]` position is ranked as an array argument before expanded element matching. | Imported optional/named/params/ref/out/in call smokes and no-match diagnostics. |
| Exact nominal and primitive matches | Exact metadata type matches outrank conversions, relationship matches, and `object` fallback. | Exact overload and exact overloaded indexer smokes. |
| `null` literals | `null` rejects non-nullable value-type parameters, ranks concrete nullable/reference targets ahead of `object`, and ranks nearer metadata-related reference targets ahead of farther base/interface candidates. Unrelated reference targets remain ambiguous. | Null literal overload, parenthesized null, indexer null, and ambiguous overload/indexer smokes. |
| Numeric literals and direct unary numeric literals | Fitting integral constant conversions are accepted for supported primitive numeric targets; impossible numeric literal conversions report diagnostics before emission. | Numeric literal, unary numeric, and indexer numeric conversion smokes plus no-match diagnostics. |
| Imported metadata relationship distance | Metadata-backed arguments and receivers prefer the nearest base/interface relationship over farther interface or `object` fallback candidates. | Metadata relationship overload, indexer, and extension receiver smokes. |
| Collections | Homogeneous collection expressions can infer array arguments, a single `params T[]` array argument, and implemented `T[]` generic method positions. Incompatible element targets report `TS2406`. | Collection expression overload, `params` collection array, delegate collection-return, and generic array constraint smokes. |
| Delegates and lambdas | Known `System.Func`/`System.Action` targets use arity and the implemented lambda return inference set, including member chains, method calls, extension/static calls, binary predicate/value results, `nameof`, checked/unchecked, `satisfies`, parenthesized/logical-not/unary numeric/if/block/collection/null-coalescing/indexer bodies. | Delegate lambda overload arity and return-ranking smokes. |
| Generic methods and constructors | Explicit generic arity, inferred generic positions, constructed generic positions, and C# generic constraints are validated for implemented metadata shapes. | Explicit/inferred generic method, generic array constraint, constructed generic constraint, and generic constructor smokes. |
| Extension receivers | Imported extension receiver candidates rank exact known non-null receivers first, then nearer metadata relationships, then `object` fallback. | Extension method instance call, receiver relationship, and object fallback smokes. |
| Ambiguity and unsupported conversions | Equally stable candidates report `TS2402`; no applicable method or constructor reports `TS2406`; unsatisfied generic constraints report `TS2417`; missing imported members use the specific `TS2407`-`TS2416` family; dynamic calls require explicit capability markers. TypeSharp does not fall through to generated C# compile failures for these 1.0-ranked paths. | Ambiguous/no-match build-stop smokes and diagnostic descriptor contract. |

Post-1.0 conversion and ranking work includes full C# overload conversion parity, user-defined conversion operators, TypeSharp-authored operator overload ranking, richer contextual typing, imported pipeline/composition overload ranking, nullable receiver lifting beyond documented shapes, general collection-builder conversions, function-typed value overload inference, and dynamic dispatch without explicit capability boundaries.

If two candidates remain equally plausible within the supported matrix, TypeSharp reports ambiguity instead of relying on generated C# compilation.

Homogeneous collection expression arguments such as `["Ada"]` infer an array argument type for imported C# overload filtering and ranking. They also infer implemented `T` positions for imported generic method parameters shaped like `T[]`, so constraints such as `where T : class` are validated before emission. In a `params T[]` position, a single compatible collection expression is ranked as the params array argument before falling back to expanded element matching. Incompatible array element targets report `TS2406` before generated C# emission instead of falling through to a C# compiler error.

Parenthesized overload arguments preserve generated C# grouping, but implemented metadata checks use the enclosed expression category. This includes parenthesized `null` literal specificity and parenthesized lambda delegate filtering/ranking before generated C# emission.

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
- a single compatible collection expression in the `params` position is checked against the full `params` array type,
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
- parenthesized indexer arguments are checked by the enclosed expression type before generated C# emission,
- unary numeric indexer arguments preserve their signed constant value for metadata validation,
- `null` indexer arguments reject non-nullable value-type indexers and rank concrete reference indexers before `object` fallback,
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
- binary value bodies that return known string concatenation or numeric arithmetic types,
- `nameof` bodies that return `string`,
- `checked(...)` and `unchecked(...)` bodies that preserve the inner expression type,
- `satisfies` bodies that preserve the proved expression type after proof erasure,
- parenthesized bodies that preserve the enclosed expression type,
- unary logical-not bodies that return `bool`,
- unary numeric sign bodies that preserve supported numeric operand types,
- `if` expression bodies whose branch result expressions merge to one known return type,
- block bodies whose final expression has a known return type,
- collection expression bodies whose elements match known array or `List<T>` return element types,
- null-coalescing bodies with known fallback or receiver-side types,
- indexer-expression bodies with known array or metadata-backed indexer return types.

## Events

Events are delegate-backed C# members. TypeSharp validates imported C# event add/remove when receiver metadata is known and the handler type is compatible.

```tysh
import { LegacyEvents } from "Legacy.Tools"

export fun subscribe(): string {
  let source = LegacyEvents()
  source.Transform += text => text
  source.Transform -= text => text
  source.Raise("value")
}
```

The receiver must expose a public event and the handler must match the event delegate. TypeSharp-authored `public delegate` declarations lower to named CLR delegate metadata, and TypeSharp-authored class/interface `public event` declarations now lower to generated CLR event metadata in the implemented 1.0 slice. Class events may be instance or static; interface events are instance-only in the C# 7.3-compatible slice. Custom add/remove accessors, interface static events, and generated event invocation helpers remain deferred.

## Extension Members

Imported C# extension methods are available when the extension type namespace is imported or opened. TypeSharp-authored explicit-receiver extension methods lower to C# extension methods.

The 1.0 TypeSharp-authored extension member policy is intentionally narrow: explicit-receiver extension methods and getter-only extension properties are the only promoted shapes. This keeps generated `net48` source C# 7.3-compatible and gives C# consumers ordinary static extension methods or helper methods, not C# 14 extension-block metadata. Extension method receiver-shape diagnostics report `TS2221` when the declaration omits the receiver type, omits the first receiver parameter, marks that parameter `params`, or gives it a type that does not exactly match the extension receiver.

TypeSharp-authored getter-only extension properties use a declaration receiver name and lower to static helper methods rather than C# 14 extension blocks. Member access such as `value.WordCount` is rewritten to that helper when the receiver type is an exact known non-null match. Nullable receiver declarations, nullable receiver accesses before lifting is implemented, duplicate exact receiver/name declarations, declarations that would be hidden by the currently implemented ordinary/structural member precedence, and helper names such as `GetWordCount(this string text)` that collide with TypeSharp-authored extension methods or generated property helpers in the same extension container report `TS2213` before backend emission.

```tysh
namespace Company.Billing

public extension string text {
  public let WordCount: int =
    text.Length
}

public extension string {
  public fun HasPrefix(text: string, prefix: string): bool =
    text.StartsWith(prefix)
}
```

Imported C# extension receiver ranking prefers closer metadata relationships. `object` receiver fallback is accepted only after more specific applicable receivers. TypeSharp-authored extension properties are intentionally narrower: exact non-null receiver matching first, nullable receiver declaration/access diagnostics, duplicate/conflict diagnostics over the implemented lookup precedence, and getter-only helper lowering only. Assignment to these properties reports `TS2213` before backend emission. Null-conditional `?.` reads and simple assignment targets for these properties also report `TS2213` before nullable receiver lifting or TypeSharp-owned null-conditional lowering expands the surface; nullable receiver lifting, setters, static extension properties, extension operators, imported extension property metadata, richer ranking, and C# 14 `extension(...)` block emission remain post-1.0.

## Exceptions And Domain Failures

C# documentation describes `try`, `catch`, `finally`, and `throw` as runtime exception handling. TypeSharp interop must recognize that imported C# APIs can throw even when the TypeSharp type signature is valid.

Guidance:

- Use `Result<T,E>` for expected domain failures in TypeSharp APIs.
- Let truly exceptional .NET failures remain exceptions unless the boundary deliberately wraps them.
- Do not document exception swallowing as a normal interop pattern.
- Use wrapper functions when a legacy API throws for ordinary validation outcomes.
- Current `TypeSharp.Core.Result<T,E>` construction is a C# consumer ABI through `Result<T,E>.Ok(...)` and `Result<T,E>.Error(...)`; direct TypeSharp source imports named `Ok` and `Error` are future ergonomics.

```tysh
import { Decimal } from "System"

public union ParseError {
  InvalidDecimal(source: string)
}

public union ParseMoneyResult {
  Parsed(value: decimal)
  Invalid(error: ParseError)
}

export fun parseMoney(text: string): ParseMoneyResult {
  let mut parsed = 0m

  if Decimal.TryParse(text, out parsed) {
    Parsed(parsed)
  }
  else {
    Invalid(InvalidDecimal(text))
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
