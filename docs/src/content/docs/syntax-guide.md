---
title: TypeSharp Syntax Guide
description: A learnable guide to TypeSharp syntax for developers coming from TypeScript, C#, or F#.
---

Start from [Install](../install/) before running examples: install `TypeSharp.Tool` with `dotnet tool install --global TypeSharp.Tool`, run `typesharp version`, and keep generated artifacts on `net48`. Use `typesharp runtime-path` when a C# host needs the matching TypeSharp Core/Runtime DLLs bundled with the installed tool.

This guide teaches TypeSharp syntax in the order most developers need it. It is not the formal grammar ledger. Use [Grammar](../grammar/) when you need the complete syntax inventory, parser boundaries, precedence, and implementation notes.

TypeSharp syntax has one steady rule: local source code can be expressive, but exported APIs must lower to C# 7.3-compatible `.NET Framework 4.8` metadata.

## Reading The Examples

Examples use `.tysh` files:

```tysh
namespace Samples.Billing

export fun label(number: int): string =
  "Invoice-" + number
```

Keep these words separate:

| Word | Meaning |
| --- | --- |
| `export` | Makes a source declaration part of the module export surface. |
| `public` | Makes a nominal declaration or member visible through generated CLR metadata. |
| `private` | Keeps a declaration or member internal to the generated shape where supported. |
| `net48` | The generated artifact target. The CLI may run on modern .NET, but generated user assemblies remain `.NET Framework 4.8`. |
| Bounded | Syntax exists, but only a documented subset is checked and lowered today. |
| Parser-visible | The parser recognizes the syntax direction, but stable checking/lowering is not promised yet. |

## File Shape

A normal file has a namespace, imports or opens, then declarations:

```tysh
namespace Company.Billing

import { StringBuilder } from "System.Text"
import type { Task } from "System.Threading.Tasks"
open System

public record Invoice(Number: string, Amount: decimal)

export fun render(invoice: Invoice): string =
  invoice.Number + ": " + invoice.Amount.ToString()
```

Rules:

- Every `.tysh` file is a source module.
- `namespace` controls the generated C# namespace.
- `import` can target TypeSharp source modules or .NET namespaces/types.
- `import type` imports type names without value-space use.
- `open` is available for namespace/module candidates, but explicit imports usually produce clearer diagnostics.
- Declarations are the durable source of truth; generated `.g.cs` files are build artifacts.

## Imports And Exports

Use relative source imports for TypeSharp files:

```tysh
import { normalize } from "./Text"
import type { Customer } from "./Models"
```

Use .NET namespace imports for C# framework or local DLL metadata:

```tysh
import { Regex } from "System.Text.RegularExpressions"
import { LegacyFormatter as Formatter } from "Legacy.Tools"
import static System.Math
```

Use namespace imports when you want a qualified alias:

```tysh
import * as Text from "System.Text"
```

Export local names explicitly when that is clearer than exporting a declaration inline:

```tysh
fun normalize(value: string): string =
  value.Trim()

export { normalize }
export { normalize as clean }
```

Re-export lowerable source module members from another file:

```tysh
export { normalize } from "./Text"
export type { Customer } from "./Models"
export * from "./PublicRules"
```

Non-relative forwarding and non-lowerable forwarding forms are not stable user-facing behavior. Unsupported forms should diagnose before generated C# emission.

## Values

Use `let` for immutable values:

```tysh
let prefix = "Invoice"
let count = 3
```

Use `let mut` only when the local value must be assigned again:

```tysh
let mut total = 0
total = total + 10
total += 5
```

Use `literal` for compile-time constants:

```tysh
public literal ApiVersion = "1.0"
```

Rules:

- TypeSharp does not use `var`, `val`, or `const` aliases.
- Assignment to a local identifier requires `let mut`.
- Compound assignment is supported only for documented local/imported target and operand subsets.

## Functions

Expression-bodied functions use `=`:

```tysh
export fun label(number: int): string =
  "Invoice-" + number
```

Block-bodied functions use `{ ... }`:

```tysh
export fun displayName(name: string?): string {
  if name == null {
    "Anonymous"
  } else {
    name
  }
}
```

Public or exported functions should use explicit parameter and return types. That keeps generated CLR metadata predictable.

### Parameters

Use ordinary typed parameters:

```tysh
export fun add(left: int, right: int): int =
  left + right
```

Use a final `params` array for repeated arguments in the supported direct-call and pipeline subset:

```tysh
export fun count(params values: int[]): int =
  values.Length
```

Use trailing literal defaults when the public shape should behave like a C# optional parameter:

```tysh
export fun pageSize(size: int = 20): int =
  size
```

Rules:

- Defaulted parameters must form a suffix.
- Defaulted parameters cannot combine with `params`.
- Supported defaults are literal strings, numbers, `true`, `false`, and `null`.
- Generic defaulted parameters are supported only when the defaulted parameter types are explicit concrete TypeSharp types that do not reference a type parameter.
- Ambient signatures, constructors, delegates, union cases, lambdas, function types, and higher-order values do not expose TypeSharp-owned default parameters in the current stable slice.

### Calls

Call functions positionally:

```tysh
let next = add(1, 2)
```

Use named arguments after any positional prefix where supported:

```tysh
let next = add(left: 1, right: 2)
```

Use explicit type arguments only when inference is not enough:

```tysh
let names = identity<string>("Ada")
```

Direct TypeSharp-owned calls and imported C# calls use different validation paths. Imported C# calls go through metadata-backed overload validation.

## Records

Records are immutable-first data types:

```tysh
public record Customer(Name: string, Age: int)
```

Construct a record when the expected type is known:

```tysh
export fun createCustomer(): Customer =
  { Name: "Ada", Age: 36 }
```

Update a record with `with` or spread over a nominal record value:

```tysh
export fun birthday(customer: Customer): Customer =
  customer with { Age: customer.Age + 1 }

export fun rename(customer: Customer): Customer =
  { ...customer, Name: "Grace" }
```

Rules:

- Records are the preferred public data shape for C# consumers.
- Record expressions need an expected nominal record type.
- Anonymous object style public APIs are not stable CLR metadata.

## Classes, Interfaces, Delegates, And Events

Use classes when C# consumers need object identity, mutable state, or C#-shaped members:

```tysh
public class Counter {
  public let mut Value: int = 0

  public fun Increment(): unit {
    Value += 1
  }
}
```

Use interfaces for contracts:

```tysh
public interface IRule {
  fun validate(value: string): bool
}
```

Use named delegates for callbacks and events:

```tysh
public delegate ChangedHandler(sender: object): unit

public class Watcher {
  public event Changed: ChangedHandler
}
```

Rules:

- The class/interface member surface is an MVP subset.
- Supported class members include public instance/static `fun`, typed `let`/`let mut`, getter-only properties with initializers, mutable get/set auto-properties with initializers, and events backed by named delegates.
- Supported interface members include method signatures, getter-only properties, mutable get/set properties, and events.
- Constructor bodies, inheritance clauses, custom accessors, indexers, operators, nested types, abstract/virtual/override members, default interface implementations, and broad member-body analysis are backlog.

## Enums

Use enums for finite named constants:

```tysh
public enum Permission : byte {
  Read = 1
  Write = 2
  ReadWrite = Read | Write
}
```

Use same-enum bitwise expressions where the operands have the same enum type:

```tysh
export fun addWrite(value: Permission): Permission =
  value | Permission.Write
```

Rules:

- TypeSharp-owned enums support optional integral underlying types, numeric values, aliases, composite-or initializers, and match exhaustiveness.
- Arbitrary computed enum values and flag-aware pattern algebra are not stable.

## Nominal Unions And Match

Use nominal unions for closed domain alternatives:

```tysh
public union LookupResult {
  Found(Customer)
  Missing(message: string)
}
```

Match each case:

```tysh
export fun describe(result: LookupResult): string =
  match result {
    Found(customer) => customer.Name
    Missing(message) => message
  }
```

Use guards when a case needs an extra condition:

```tysh
export fun describeAdult(result: LookupResult): string =
  match result {
    Found(customer) when customer.Age >= 18 => customer.Name
    Found(customer) => "minor: " + customer.Name
    Missing(message) => message
  }
```

Rules:

- Nominal unions lower to named CLR-visible metadata.
- Unguarded union-case arms participate in exhaustiveness.
- Guarded arms do not prove exhaustiveness by themselves.
- Nested destructuring, active-pattern-style extractors, and richer pattern algebra are backlog.

## Types

Use primitive and CLR-friendly names:

```tysh
let count: int = 1
let name: string = "Ada"
let amount: decimal = 10.0m
```

Use nullable types where absence is represented as null:

```tysh
export fun fallback(name: string?): string =
  if name == null { "Anonymous" } else { name }
```

Use arrays and generic types for CLR collection interop:

```tysh
import { List } from "System.Collections.Generic"

let names: string[] = ["Ada", "Grace"]
let moreNames: List<string> = ["Katherine"]
```

Use function types for function values:

```tysh
export let Transform: string -> string =
  text => text.Trim()
```

Use structural shapes locally:

```tysh
type Named = { Name: string }
type Aged = { Age: int }
type Profile = Named & Aged
```

Use type-level unions locally:

```tysh
type Input = string | int
type Status = "draft" | "submitted"
```

Use `unknown` for safe gradual boundaries:

```tysh
fun accept(value: unknown): string =
  if value is string {
    value
  } else {
    "unsupported"
  }
```

Rules:

- Structural shapes, intersections, `keyof`, indexed access, and type-level unions are compile-time tools.
- Public .NET metadata must use records, classes, interfaces, delegates, nominal unions, or other CLR-visible named types.
- Use `dynamic` only behind explicit capability boundaries.

## Type Operators

Use limited `keyof` on known records or shapes:

```tysh
type CustomerKey = keyof Customer
```

Use limited indexed access when the member is known:

```tysh
type CustomerName = Customer["Name"]
```

Use `satisfies` to prove a value fits a type while preserving the original value type:

```tysh
type Named = { Name: string }

fun keep(customer: Customer): Customer =
  customer satisfies Named
```

Rules:

- `satisfies` erases to the left expression in generated C#.
- Advanced mapped, conditional, template-literal, and utility types are planned behind the evaluator budget, not stable grammar.

## Expressions

Most TypeSharp code is expression-oriented:

```tysh
export fun category(age: int): string =
  if age >= 18 {
    "adult"
  } else {
    "minor"
  }
```

Blocks can produce a value from their final expression:

```tysh
export fun computed(): int {
  let left = 1
  let right = 2
  left + right
}
```

Use lambdas when a delegate or function type is expected:

```tysh
export let Clean: string -> string =
  text => text.Trim()
```

Use block lambdas when the body needs multiple statements:

```tysh
export let Clean: string -> string =
  text => {
    let trimmed = text.Trim()
    trimmed
  }
```

## Collections

Use array literals when the target is an array:

```tysh
let numbers: int[] = [1, 2, 3]
```

Use `List<T>` when the target type is a list:

```tysh
import { List } from "System.Collections.Generic"

let names: List<string> = ["Ada", "Grace"]
```

Spread known arrays or lists:

```tysh
let first: int[] = [1, 2]
let second: int[] = [3, 4]
let all: int[] = [...first, ...second]
```

Rules:

- Collection expressions require a known array or `List<T>` target in the stable slice.
- Dictionary literals, set literals, general collection-builder protocols, and contextual collection inference without a known target are backlog.

## Pipelines And Composition

Use pipelines for left-to-right flow:

```tysh
export fun normalized(value: string): string =
  value |> trim |> uppercase
```

Use a call form when the target takes more arguments:

```tysh
export fun labeled(value: string): string =
  value |> addPrefix(prefix: "ID-")
```

Use composition for unary functions:

```tysh
export let Normalize: string -> string =
  trim >> uppercase
```

Rules:

- Pipeline support is bounded to first-argument calls where the target signature is known enough.
- Composition support is bounded to unary function-shaped operands with predictable delegate shape.
- General currying, partial application, imported pipeline/composition inference, and broad higher-order inference are backlog.

## Operators And Assignment

Use normal arithmetic, comparison, equality, and boolean expressions where types are known:

```tysh
let total = price + tax
let ok = total > 0
```

Use bitwise operators on supported enum, primitive integral, or bool operands:

```tysh
let flags = Permission.Read | Permission.Write
let shifted = 1 << 3
let unsignedShift = value >>> 1
```

Use compound assignment only on supported mutable or imported targets:

```tysh
let mut count = 1
count += 1
count *= 2
count <<= 1
count >>>= 1
```

Use `checked` and `unchecked` for overflow context:

```tysh
export fun checkedAdd(value: int): int =
  checked(value + 1)
```

Rules:

- `>>>` and `>>>=` lower through C# 7.3-compatible casts and `>>`; generated source does not require newer C# syntax.
- User-defined TypeSharp operators are not stable.
- Flag-aware match reasoning is not stable.

## Null-Conditional Interop

Null-conditional member and indexer syntax is recognized:

```tysh
legacy?.Name
legacy?[0]
legacy?.Count = 1
legacy?[0] += 1
```

Rules:

- Stable semantic support is intentionally tied to bounded imported C# metadata targets.
- TypeSharp-owned member/indexer null-conditional assignment, invocation chains, events, increments, and broader compound assignment forms are backlog.

## Async, Yield, And Lock

Use `async fun` for .NET task interop:

```tysh
import type { Task } from "System.Threading.Tasks"

export async fun loadName(): Task<string> {
  return "Ada"
}
```

Use `yield` inside an iterator function with an explicit CLR enumerable return:

```tysh
import { IEnumerable } from "System.Collections.Generic"

export fun names(): IEnumerable<string> {
  yield "Ada"
  yield "Grace"
}
```

Use `lock` for monitor-style synchronization:

```tysh
export fun guarded(gate: object): unit {
  lock gate {
    // protected work
  }
}
```

Rules:

- `async fun` lowers to `Task` or `Task<T>`.
- `yield` requires a compatible enumerable return type.
- `lock` requires a known non-null reference gate.

## Attributes And Partial Types

Use attributes on supported declaration/member surfaces:

```tysh
[Obsolete("Use NewCustomer instead.")]
public record OldCustomer(Name: string)
```

Use `partial` where the generated C# type declaration supports it:

```tysh
public partial record Customer(Name: string)

public partial interface IRule {
  fun validate(value: string): bool
}
```

Rules:

- `partial` is implemented for modules, records, unions, classes, and interfaces.
- Partial methods, partial constructors, partial events, source augmentation hooks, and broad attribute target validation are backlog.

## Extension Members

Use explicit receiver extension methods:

```tysh
public extension string {
  public fun HasPrefix(text: string, prefix: string): bool =
    text.StartsWith(prefix)
}
```

Use getter-only extension properties for the bounded helper shape:

```tysh
public extension string text {
  public let WordCount: int =
    text.Length
}
```

Rules:

- Extension methods lower to C# extension methods.
- Getter-only extension properties lower to helper methods.
- Setters, static extension members, extension operators, nullable receiver lifting, imported extension property metadata, richer ranking, and C# 14 extension-block emission are backlog.

## Interop Boundaries

Reference .NET Framework assemblies and local DLLs in `TypeSharp.toml`:

```toml
[references]
assemblies = ["System", "System.Core"]
paths = ["lib/Legacy.Tools.dll"]
```

Import and call metadata types from TypeSharp:

```tysh
namespace Samples.Interop

import { LegacyFormatter } from "Legacy.Tools"

export fun format(value: string): string {
  let formatter = LegacyFormatter("legacy:")
  formatter.Format(value)
}
```

Use `ref`, `out`, `in`, named arguments, optional arguments, and `params` only where the TypeSharp-owned or imported C# validation path supports them.

## Public API Checklist

Before exporting a declaration, check:

| Question | Preferred Answer |
| --- | --- |
| Does the type lower to CLR metadata? | Use records, classes, interfaces, delegates, enums, nominal unions, arrays, CLR generic types, `Task<T>`, or Core helper types. |
| Does the signature expose structural shapes? | Wrap them in a named record/interface/class. |
| Does the signature expose type-level unions? | Use a nominal union or named wrapper type. |
| Does the signature rely on inferred anonymous function types? | Add an explicit function type annotation or a named delegate. |
| Does C# need to consume the API? | Prefer explicit parameter and return types, stable namespaces, and nominal public types. |
| Does the generated code need Core/Runtime helpers? | Deploy the matching DLLs from `typesharp runtime-path`. |

## Learning Order

1. Files, namespaces, imports, and exports.
2. `let`, `literal`, and `fun`.
3. Records and public function signatures.
4. Nominal unions and `match`.
5. Type annotations, nullable types, arrays, generics, and function types.
6. Local structural shapes, type-level unions, `unknown`, and `satisfies`.
7. Collections, record expressions, pipelines, and lambdas.
8. C# interop imports and public ABI rules.
9. Classes, interfaces, delegates, events, attributes, partial types, and extension members.
10. Bounded operators, null-conditional interop, async, `yield`, and `lock`.

## Related Reference Pages

- [Grammar](../grammar/) is the canonical syntax ledger and complete quick map.
- [Grammar And Language Reference](../reference/) links each syntax family to its canonical spec.
- [Modules And Imports](../modules/) explains source module paths, imports, exports, namespaces, and generated containers.
- [Type System](../type-system/) explains inference, null safety, `unknown`, `dynamic`, structural shapes, type-level unions, generics, and public ABI boundaries.
- [Lowering](../lowering/) explains how syntax becomes C# 7.3-compatible generated source.
- [.NET Interop](../dotnet-interop/) explains how public APIs and local C# DLL references behave in `net48` projects.
