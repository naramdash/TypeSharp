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

## Extension Properties

TypeSharp-authored extension properties are a bounded C# 14-inspired surface over the existing explicit-receiver `extension` declaration. The implemented shape is getter-only and requires a declaration receiver name, an explicit property type, and an initializer:

```tysh
public extension string text {
  public let WordCount: int =
    text.Length
}
```

Member access resolves an extension property only after ordinary imported/instance members and structural shape members are considered, and only for an exact known non-null receiver type. The checker reports `TS2201` before backend emission when a TypeSharp-authored extension property is declared on a nullable receiver type, duplicates an existing exact receiver/name pair, would be hidden by the currently implemented ordinary/structural member precedence, would generate a `GetName` helper that collides with a TypeSharp-authored extension method or another generated extension property helper in the same extension container, is used as an assignment target, is accessed through a nullable receiver expression before nullable receiver lifting is implemented, or is targeted by null-conditional `?.` reads or simple assignment before TypeSharp-owned null-conditional extension-property lowering is implemented. Setters, static extension properties, operators, imported C# extension property metadata, nullable receiver lifting, and richer conflict ranking remain backlog.

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
- `enum` for CLR-visible named finite constants.
- `delegate` for callback interop.
- nominal `union` for closed domain alternatives.

If a compile-time-only type-level union, intersection alias, or structural shape appears in a public boundary, the compiler reports a public ABI diagnostic.

Simple TypeSharp-owned enums expose ordinary CLR enum metadata. Declarations can use explicit integral underlying types, and members can use explicit integer numeric values, aliases to previously declared members, or enum initializer-local composite-or expressions over previously declared same-enum members and integer literals. Declaration and member attribute lists lower to generated C# enum metadata, including `[FlagsAttribute]` shape, but they do not change current enum reasoning. The checker validates explicit member values and numeric operands against the selected underlying type range, defaulting to `int` when no underlying type is declared, validates identifier operands before emission, and reasons about enum values by enum name and member name. The checker accepts same-enum member values such as `Color.Green` where `Color` is expected, accepts expression-level same-enum value `|`, `&`, `^`, and unary `~` forms such as `Color.Red | Color.Blue`, `value & Color.Blue`, `value ^ Color.Red`, and `~value`, rejects unrelated enum values or enum/numeric mixes for enum value operators, reports missing enum members, and checks enum match exhaustiveness over TypeSharp-owned enum members. Named imported C# enums participate in the same match exhaustiveness path when referenced assembly metadata exposes finite public enum members. Imported C# enum metadata also records the underlying type name and literal numeric member values for future interop decisions, but current TypeSharp enum checking remains name/member based.

Integral numeric `|`, `&`, `^`, unary `~`, `<<`, `>>`, and `>>>` expressions are accepted for known non-null primitive integral operands. `|`, `&`, and `^` use the supported C# integral promotion rules; shifts use the left operand for the result type, promote small left operands to `int`, and require a non-null `byte`, `sbyte`, `short`, `ushort`, or `int` count so generated C# remains C# 7.3-compatible. Logical unsigned `>>>` keeps the same result shape as other shifts and lowers signed operands through explicit unsigned casts. Boolean `|`, `&`, and `^` expressions are accepted for known non-null `bool` operands and produce `bool`. Local identifier assignment requires `let mut`; known simple assignments validate nullability, structural, and ordinary assignability; bitwise compound assignments `|=`, `&=`, and `^=` validate known local enum/integral/bool operands; shift assignments `<<=` and `>>=` validate known local primitive integral targets and int-compatible counts before lowering to C# assignment operators; and logical unsigned shift assignment `>>>=` accepts the same mutable local primitive integral target/count set plus readable/writable metadata-backed imported C# instance/static field and property member targets and metadata-backed imported C# instance indexer targets whose selected public indexer exposes both getter and setter. Imported `>>>=` still requires a known non-null primitive integral target value type, a `byte`, `sbyte`, `short`, `ushort`, or `int` count, and for indexers a supported argument list; event, unresolved imported member/indexer, missing-setter indexer, nullable, non-integral, enum, record, ambiguous/mismatched indexer, and unsupported-count cases report deterministic diagnostics before emission.

Null-conditional assignment `receiver?.Member = value` accepts nullable or reference-like metadata-backed imported C# instance receivers when `Member` resolves to a writable public instance field or property; `receiver?[index] = value` accepts the same receiver boundary when a metadata-backed imported C# instance indexer resolves to a public getter/setter pair with supported arguments. Null-conditional reads `receiver?.Member` and `receiver?[index]` accept nullable or reference-like metadata-backed imported C# instance receivers when the member or indexer resolves to a readable public instance field/property/indexer with supported arguments, and the expression type is nullable-compatible so value-type reads can return absence. Null-conditional additive compound assignment `receiver?.Member += value`, `receiver?.Member -= value`, `receiver?[index] += value`, and `receiver?[index] -= value` accepts the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, both operands are known non-null primitive integral numeric values whose promoted result can be assigned back to the member or indexer value type, and indexer arguments are supported. Null-conditional bitwise compound assignment `receiver?.Member |= value`, `receiver?.Member &= value`, `receiver?.Member ^= value`, `receiver?[index] |= value`, `receiver?[index] &= value`, and `receiver?[index] ^= value` accepts the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, the existing primitive integral, enum, or bool bitwise compound target/value policy is satisfied, and indexer arguments are supported. Null-conditional logical unsigned shift assignment `receiver?.Member >>>= count` and `receiver?[index] >>>= count` accept the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, the existing `>>>=` primitive target/count policy is satisfied, and indexer arguments are supported. The right side of null-conditional simple assignment, supported member/indexer additive compound assignment, and supported member/indexer bitwise compound assignment, plus the `count` side of supported null-conditional `>>>=`, are checked against the target type and lowered so they are evaluated only when the receiver is non-null; null-conditional indexer arguments are also evaluated only inside the non-null branch. Other null-conditional compound assignment beyond the bounded member/indexer additive, member/indexer bitwise, and member/indexer `>>>=` slices, increment/decrement, invocation, chains, events, static targets, local binding targets, and TypeSharp-owned member/indexer targets report deterministic diagnostics before backend emission. Invalid value-shaped `>>`, `<<`, and `>>>` operands such as nullable values, booleans, strings, enums, records, function-shaped composition candidates under `>>>`, or unsupported count types report `TS2201` before backend emission. Unary boolean complement, flag algebra beyond same-enum value `|`/`&`/`^`/`~`, arbitrary/general computed enum values, broad attribute target validation, numeric pattern algebra, and flag-style reasoning over imported numeric metadata are not part of the implemented slice.

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
- richer exhaustiveness remains a functional correctness priority; known nominal unions, TypeSharp-owned enums, named imported C# enums, `bool`, and local type-level unions including literal unions report missing cases/members, `_` can cover the remainder, and guarded arms do not prove coverage without an unguarded cover;
- struct-backed value options, recursive union ergonomics, and helper APIs such as bind/map/default are stable backlog items once their `net48` ABI and allocation tradeoffs are documented;
- general currying, partial application, computation-expression-style workflows, active-pattern-style extractors, units of measure, and type providers stay backlog or experimental until TypeSharp has deterministic lowering, diagnostics, and security boundaries.

The default TypeSharp functional model must not require `FSharp.Core` at runtime. Direct F# option, tuple, record, or union interop can be added later as a compatibility layer.

Direct TypeSharp-declared function calls are checked when the target has a known parameter list. For `f(args...)`, supplied arguments must fit the declared required count, optional/default suffix, or final `params` tail, and each known argument type must be assignable to the corresponding parameter type. A final `params name: T[]` parameter accepts either one exact `T[]` argument or expanded trailing `T` arguments, including zero trailing arguments after fixed parameters; non-final, repeated, or non-array `params` declarations report `TS2201`. Trailing defaulted parameters use `name: Type = literal` and can be omitted from direct calls or first-argument pipeline lowering when the default literal is string, numeric, `true`, `false`, or `null`; generic direct `fun` defaults are accepted only when every defaulted parameter has an explicit concrete TypeSharp type that does not reference a type parameter. Non-trailing defaults, unsupported default expressions, default/type mismatches, `params` interaction, and generic defaulted parameter types such as `T`, `T[]`, or `List<T>` report `TS2201`. TypeSharp-owned direct calls may use named arguments after any positional prefix, and first-argument pipeline calls may name non-piped arguments. The checker binds those names to declared parameters, reports `TS2201` for unknown names, duplicates, positional arguments after named arguments, missing required parameters, wrong argument types, inconsistent generic inference, or `params` named combinations, and leaves imported C# named arguments on the metadata-backed interop path. Direct generic function calls support explicit type arguments plus inference/substitution when declared type parameters appear directly as parameter/return types or inside bounded constructed shapes such as `T[]` and matching single-argument generic wrappers like `List<T>`; repeated inferred type parameters must infer consistently, including through the implemented `params` tail when named arguments are not used. Generic named direct calls reuse the same bounded inference/substitution shapes except for generic `params` combinations, and concrete optional/default suffixes can still be omitted. Direct pipeline compatibility is checked when the pipeline target is a TypeSharp-declared function with a known parameter list. For `value |> f` and `value |> f(args...)`, `value` must be assignable to `f`'s first parameter, and the lowered call must supply arguments compatible with the known target parameter list, optional/default suffix, final `params` tail, or supported named non-piped arguments. Generic TypeSharp pipeline targets infer and substitute the same bounded simple, array, and matching single-argument generic wrapper shapes from the piped input plus non-piped arguments, including concrete optional/default suffixes and named non-piped arguments; the implemented `params` tail remains positional-only for generic targets. Direct named unary function composition is checked when both TypeSharp-declared function signatures are known: for `f >> g`, `f` must return a type assignable to `g`'s single parameter, and for `g << f`, `f` must return a type assignable to `g`'s single parameter. Generic TypeSharp composition targets infer and substitute bounded simple, array, and matching single-argument generic wrapper shapes from that composition edge when one side provides enough concrete type information. Explicit unary function-type annotations on direct composition value declarations validate the annotated input against the first composed function parameter and the composed result against the annotation return type before generated C# assignment. Unannotated non-exported top-level direct composition values infer concrete generated delegate types only when the bounded composition signature is fully known; public/exported direct composition values without an explicit function type annotation report `TS2201`. These checks keep common F#-style function/pipeline/composition mistakes deterministic without committing to higher-order function inference, imported generic function inference, imported composition inference, broader unannotated composition expression inference, public ABI inference for composition expressions, currying, partial application, broader type-constructor unification, pipeline overload ranking, TypeSharp function overload ranking, or multi-parameter composition.

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
