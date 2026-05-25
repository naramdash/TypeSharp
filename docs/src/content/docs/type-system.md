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

## 1.0 Nullability And Unknown Boundary

The 1.0 nullability and `unknown` policy is conservative: nullable flow and uncertain values must be resolved before generated C# emission for public or non-null-sensitive TypeSharp positions.

| Surface | 1.0 Rule |
| --- | --- |
| TypeSharp source nullability | Reference-like types are non-null by default. `T?` admits `null`; assigning or returning `null` or a nullable expression into a non-null target reports `TS2202` before backend emission. |
| Public TypeSharp APIs | Public nullable value types lower to `System.Nullable<T>`, and public nullable reference types keep the CLR type while preserving TypeSharp's source contract where metadata supports it. Public `unknown`, structural, intersection, type-level union, `keyof`, indexed-access, unresolved computed, or anonymous public boundaries report `TS2204`. |
| Imported C# nullability | In `language.nullable = "strict"`, imported C# reference-return metadata without nullable annotations reports warning `TS2404`; it does not block builds by itself. The recommended 1.0 pattern is to wrap the interop boundary and return a clear nominal or nullable TypeSharp type. |
| `unknown` values | `unknown` is a safe local gradual-typing boundary. Member or indexer access on `unknown` reports `TS2209` until code proves a narrower nominal or structural shape. `unknown` is not a public CLR ABI type. |
| Nullable receivers and null-conditional forms | Implemented imported C# null-conditional member/indexer read and assignment slices lower through C# 7.3-compatible guards. TypeSharp-owned nullable receiver lifting, null-conditional extension-property access, broader null-conditional chains, invocation, events, and local-binding assignment remain post-1.0 unless separately promoted. |

This means the stable 1.0 distinction is warning versus error: unknown imported C# nullability is a strict-mode warning (`TS2404`) so users can isolate legacy metadata, while TypeSharp-owned nullability contract violations (`TS2202`), unsafe `unknown` access (`TS2209`), and compile-time-only public leakage including public `unknown` (`TS2204`) are errors.

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

The `satisfies` expression checks the proof and then keeps the original expression type. Generated C# emits only the left-hand expression. A value type that cannot assign to the target type after nullability and structural proof checks reports `TS2227`; nullability violations remain `TS2202`, and structural proof failures remain `TS2220`.

Structural shapes are compile-time tools. They must not leak directly through public .NET boundaries. Public APIs should expose records, classes, interfaces, wrappers, or nominal unions.

## Extension Properties

TypeSharp-authored extension properties are a bounded C# 14-inspired surface over the existing explicit-receiver `extension` declaration. The implemented shape is getter-only and requires a declaration receiver name, an explicit property type, and an initializer:

```tysh
public extension string text {
  public let WordCount: int =
    text.Length
}
```

Member access resolves an extension property only after ordinary imported/instance members and structural shape members are considered, and only for an exact known non-null receiver type. The checker reports `TS2213` before backend emission when a TypeSharp-authored extension property is declared on a nullable receiver type, duplicates an existing exact receiver/name pair, would be hidden by the currently implemented ordinary/structural member precedence, would generate a `GetName` helper that collides with a TypeSharp-authored extension method or another generated extension property helper in the same extension container, is used as an assignment target, is accessed through a nullable receiver expression before nullable receiver lifting is implemented, or is targeted by null-conditional `?.` reads or simple assignment before TypeSharp-owned null-conditional extension-property lowering is implemented. Setters, static extension properties, operators, imported C# extension property metadata, nullable receiver lifting, and richer conflict ranking remain backlog.

## 1.0 Extension Member Policy

The 1.0 TypeSharp-authored extension member surface is limited to explicit-receiver extension methods and getter-only extension properties. Extension methods lower to ordinary C# extension methods with a `this` receiver parameter. Getter-only extension properties lower to C# 7.3-compatible `GetName(this T receiver)` helper methods, and TypeSharp member access lowers to calls to those helpers rather than emitted C# 14 extension-member syntax.

Extension method declarations require an extension receiver type plus a first non-`params` method parameter whose type exactly matches that receiver; extension-method receiver-shape failures report `TS2221` before backend emission. Extension property lookup is exact and deliberately narrower than imported C# extension method overload ranking: the receiver must be a known non-null exact receiver type, ordinary imported/instance members and structural proof members win first, and duplicate/conflict/helper-collision/assignment/null-conditional/nullable-receiver cases report deterministic `TS2213` diagnostics before backend emission. Setters, static extension members, extension operators, imported C# extension property metadata, nullable receiver lifting, richer conversion/ranking, and C# 14 `extension(...)` block emission are post-1.0 unless separately promoted with lowering and C# consumer evidence.

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
- report `TS2220` when a local structural proof is impossible because a required member is missing, a member type is incompatible, a structural member does not exist, or a type-level-union discriminant literal cannot match any member;
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

The 1.0 implemented gate covers the currently executable subset: type alias chains deeper than 16 and recursive type alias cycles report `TS2212`; local type-level union aliases wider than 64 distinct members report `TS2212`; `keyof` and indexed access aliases with more than 64 keys report `TS2212`; and structural intersection aliases with more than 64 resulting members report `TS2212`. Mapped, conditional, template-literal, and utility type syntax remains post-1.0 and must not be documented as implemented until the full evaluator has parser/checker coverage, public-boundary diagnostics, and generated `net48` evidence where emitted signatures can change.

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
- If the normalized result is structural, a type-level union, a literal-only key set, a template-generated string union, `unknown`, or an unresolved computed form, public use reports the public-boundary diagnostic before generated C# emission.
- Future structural public ABI adapters must use generated nominal wrappers or interfaces with a versioned naming policy; they are separate from the evaluator budget.

## Nominal Public API

C# consumers see generated CLR metadata. That means public TypeSharp APIs need stable nominal shapes:

- `record` for immutable public data.
- `class` and `interface` for object-oriented interop.
- `enum` for CLR-visible named finite constants.
- `delegate` for callback interop.
- nominal `union` for closed domain alternatives.

If a compile-time-only type-level union, intersection alias, structural shape, or `unknown` type appears in a public boundary, the compiler reports a public ABI diagnostic.

## 1.0 Class Interface And Member Boundary

The 1.0 TypeSharp-authored class and interface surface is intentionally small. Accepted class declarations lower to named CLR classes with optional type parameters, supported C# 7.3-compatible generic constraints, `partial`, an implicit public parameterless constructor, public instance `fun` methods with supported parameter and return types, and typed `event` members backed by named delegate types. Accepted interface declarations lower to named CLR interfaces with optional type parameters, supported generic constraints, `partial`, method signatures, and typed `event` members backed by named delegate types. C# `net48` consumer smokes cover constructing the generated class, calling its methods, subscribing to generated class events, implementing generated interfaces with events, using generic classes, and satisfying generated generic interface/class constraints.

TypeSharp-authored class constructors, class fields, class properties, custom add/remove events, static events, generated event invocation helpers, explicit inheritance or interface implementation clauses, static/abstract/virtual/override members, interface default implementations, property setters, indexers, operators, attributes on individual class/interface members beyond the emitted declaration subset, partial methods, nested type declarations, and broader member-body analysis are post-1.0. Unsupported class and interface member forms now report deterministic diagnostics before backend emission instead of being silently ignored by generated C# lowering.

Simple TypeSharp-owned enums expose ordinary CLR enum metadata. Declarations can use explicit integral underlying types, and members can use explicit integer numeric values, aliases to previously declared members, or enum initializer-local composite-or expressions over previously declared same-enum members and integer literals. Declaration and member attribute lists lower to generated C# enum metadata, including `[FlagsAttribute]` shape, but they do not change current enum reasoning. The checker validates explicit member values and numeric operands against the selected underlying type range, defaulting to `int` when no underlying type is declared, validates identifier operands before emission, and reasons about enum values by enum name and member name. The checker accepts same-enum member values such as `Color.Green` where `Color` is expected, accepts expression-level same-enum value `|`, `&`, `^`, and unary `~` forms such as `Color.Red | Color.Blue`, `value & Color.Blue`, `value ^ Color.Red`, and `~value`, rejects unrelated enum values or enum/numeric mixes for enum value operators, reports missing enum members, and checks enum match exhaustiveness over TypeSharp-owned enum members. Named imported C# enums participate in the same match exhaustiveness path when referenced assembly metadata exposes finite public enum members. Imported C# enum metadata also records the underlying type name and literal numeric member values for future interop decisions, but current TypeSharp enum checking remains name/member based.

Integral numeric `|`, `&`, `^`, unary `~`, `<<`, `>>`, and `>>>` expressions are accepted for known non-null primitive integral operands. `|`, `&`, and `^` use the supported C# integral promotion rules; shifts use the left operand for the result type, promote small left operands to `int`, and require a non-null `byte`, `sbyte`, `short`, `ushort`, or `int` count so generated C# remains C# 7.3-compatible. Logical unsigned `>>>` keeps the same result shape as other shifts and lowers signed operands through explicit unsigned casts. Boolean `|`, `&`, and `^` expressions are accepted for known non-null `bool` operands and produce `bool`. Local identifier assignment requires `let mut`; immutable local assignment, non-binding assignment targets, unsupported imported C# member/indexer compound-assignment targets, and unsupported null-conditional assignment target shapes report `TS2216` before backend emission. Known simple assignments validate nullability, structural, and ordinary assignability; local multiplicative compound assignments `*=`, `/=`, and `%=` validate known non-null primitive integral numeric operands, a bounded local `float`, `double`, and `decimal` policy, or one selected imported C# public static binary `operator *`, `operator /`, or `operator %` whose result assigns back to the mutable local target. Regular readable/writable imported C# field/property member targets, null-conditional imported C# field/property member targets, regular imported C# indexer targets, and null-conditional imported C# indexer targets with selected public getter/setter pairs plus supported arguments use the same primitive policy and can also use one selected imported public static binary operator when its result assigns back to the target value type. The local/member/indexer floating/decimal policy accepts same-family floating operands, decimal operands with integral right operands, and primitive integral right operands whose promoted result remains assignable back to the target; it rejects nullable operands, bool/string/enum/record targets, mixed decimal-floating operands, and narrowing assign-back results before backend emission. The imported static-operator slice accepts only ordinary C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata; checked user-defined operator metadata such as `op_CheckedMultiply`, `op_CheckedDivision`, and `op_CheckedModulus` is not part of 1.0 because generated output remains C# 7.3-compatible. Missing, ambiguous, nullable, checked-only, instance-compound-only, and non-assignable user-defined operator shapes report `TS2217` before backend emission, while unsupported target shapes report `TS2216`. Local `checked(...)` and `unchecked(...)` wrappers over mutable local multiplicative compound assignments, readable/writable metadata-backed imported C# instance/static field/property member targets, metadata-backed imported C# instance indexer targets with selected public getter/setter pairs plus supported arguments, imported C# instance field/property null-conditional member targets, and imported C# instance indexer null-conditional targets with selected public getter/setter pairs plus supported arguments reuse the same operand and assign-back policy including the ordinary imported static-operator handling for null-conditional indexer targets. Accepted imported user-defined operator assignments lower as ordinary compound assignments inside the generated `checked` or `unchecked` block; a type that exposes only checked operator metadata is rejected instead of depending on newer C# syntax. Bitwise compound assignments `|=`, `&=`, and `^=` validate known local enum/integral/bool operands; shift assignments `<<=` and `>>=` validate known local primitive integral targets and int-compatible counts before lowering to C# assignment operators; and logical unsigned shift assignment `>>>=` accepts the same mutable local primitive integral target/count set plus readable/writable metadata-backed imported C# instance/static field and property member targets and metadata-backed imported C# instance indexer targets whose selected public indexer exposes both getter and setter. True C# 14 instance compound-assignment operators, TypeSharp-authored operators, checked user-defined operators, and broader operator ranking remain backlog. Imported `>>>=` still requires a known non-null primitive integral target value type, a `byte`, `sbyte`, `short`, `ushort`, or `int` count, and for indexers a supported argument list; event, unresolved imported member/indexer, missing-setter indexer, nullable, non-integral, enum, record, ambiguous/mismatched indexer, and unsupported-count cases report deterministic diagnostics before emission.

## 1.0 Enum And Bitwise Algebra Boundary

The 1.0 enum and bitwise algebra boundary is intentionally finite. Accepted enum forms are TypeSharp-owned CLR enum declarations with optional integral underlying types, explicit integer values, aliases to previously declared same-enum members, enum initializer-local composite-or expressions over previously declared same-enum members and integer literals, same-enum value `|`, `&`, `^`, unary `~`, and match exhaustiveness for TypeSharp-owned and named imported C# enum members. Accepted bitwise forms are primitive integral `|`, `&`, `^`, unary `~`, `<<`, `>>`, logical unsigned `>>>`, boolean `|`, `&`, `^`, local enum/integral/bool bitwise compound assignment, primitive integral shift assignment, and `>>>=` for mutable locals plus the bounded imported field/property/indexer targets described above. Imported enum metadata records underlying type names and literal numeric member values, but 1.0 enum reasoning stays enum/member-name based.

Enum-valued shifts and shift assignments, flag algebra beyond same-enum value `|`/`&`/`^`/`~`, flag-aware match or pattern reasoning, imported numeric enum flag reasoning, arbitrary/general computed enum values outside the initializer-local composite-or subset, numeric pattern algebra, numeric enum patterns, and broad attribute target validation are post-1.0. Unsupported enum and bitwise algebra shapes report deterministic `TS2214` diagnostics before backend emission instead of depending on generated C# failures.

TypeSharp-authored operator declarations are not 1.0 syntax, public ABI, or overload candidates. A TypeSharp record, class, interface, union, or extension declaration cannot introduce `operator +`, `operator *`, conversion operators, checked operators, or true compound-assignment operators for later TypeSharp expressions. Operator-like expressions over TypeSharp-authored nominal values are accepted only where the explicit built-in enum, numeric, boolean, assignment, pipeline, or composition rules above apply; otherwise the checker reports the existing deterministic operand diagnostic before backend emission. Future TypeSharp-authored operator support requires a separate syntax, overload ranking, checked/unchecked policy, public CLR metadata policy, and C# 7.3-compatible lowering or direct IL backend decision.

Null-conditional assignment `receiver?.Member = value` accepts nullable or reference-like metadata-backed imported C# instance receivers when `Member` resolves to a writable public instance field or property; `receiver?[index] = value` accepts the same receiver boundary when a metadata-backed imported C# instance indexer resolves to a public getter/setter pair with supported arguments. Null-conditional reads `receiver?.Member` and `receiver?[index]` accept nullable or reference-like metadata-backed imported C# instance receivers when the member or indexer resolves to a readable public instance field/property/indexer with supported arguments, and the expression type is nullable-compatible so value-type reads can return absence. Null-conditional additive compound assignment `receiver?.Member += value`, `receiver?.Member -= value`, `receiver?[index] += value`, and `receiver?[index] -= value` accepts the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, both operands are known non-null primitive integral numeric values whose promoted result can be assigned back to the member or indexer value type, and indexer arguments are supported. Null-conditional member multiplicative compound assignment `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` accepts the same receiver boundary when the selected metadata-backed public instance field/property is readable/writable and either the bounded integral/floating-point/decimal assign-back policy is satisfied or exactly one imported C# public static binary `operator *`, `operator /`, or `operator %` matches and its result assigns back to the member value type. Null-conditional indexer multiplicative compound assignment `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` accepts the same receiver boundary when the selected metadata-backed public instance indexer is readable/writable, supported indexer arguments are present, and either the bounded integral/floating-point/decimal assign-back policy is satisfied or exactly one imported C# public static binary `operator *`, `operator /`, or `operator %` matches and its result assigns back to the indexer value type. Statement-form `checked(...)` and `unchecked(...)` wrappers are also accepted for the null-conditional member and indexer multiplicative targets; indexer wrappers preserve the outer receiver guard and inner supported-argument guard while placing the checked/unchecked assignment body inside the guarded non-null branch. Null-conditional bitwise compound assignment `receiver?.Member |= value`, `receiver?.Member &= value`, `receiver?.Member ^= value`, `receiver?[index] |= value`, `receiver?[index] &= value`, and `receiver?[index] ^= value` accepts the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, the existing primitive integral, enum, or bool bitwise compound target/value policy is satisfied, and indexer arguments are supported. Null-conditional shift compound assignment `receiver?.Member <<= count`, `receiver?.Member >>= count`, `receiver?[index] <<= count`, and `receiver?[index] >>= count` accepts the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, the existing primitive target/count policy is satisfied, and indexer arguments are supported. Null-conditional logical unsigned shift assignment `receiver?.Member >>>= count` and `receiver?[index] >>>= count` accept the same receiver boundary when the selected metadata-backed public instance member or indexer is readable/writable, the existing `>>>=` primitive target/count policy is satisfied, and indexer arguments are supported. The right side of null-conditional simple assignment, supported member/indexer additive compound assignment, supported member/indexer multiplicative compound assignment including checked/unchecked member/indexer wrappers, and supported member/indexer bitwise compound assignment, plus the `count` side of supported null-conditional member/indexer `<<=`/`>>=` and member/indexer `>>>=`, are checked against the target type and lowered so they are evaluated only when the receiver is non-null; null-conditional indexer arguments are also evaluated only inside the non-null branch. Other null-conditional compound assignment beyond the bounded member/indexer additive, multiplicative, bitwise, regular shift, and `>>>=` slices, increment/decrement, invocation, chains, events, static targets, local binding targets, and TypeSharp-owned member/indexer targets report deterministic diagnostics before backend emission. Invalid value-shaped `>>`, `<<`, and `>>>` operands such as nullable values, booleans, strings, enums, records, function-shaped composition candidates under `>>>`, or unsupported count types report `TS2214` before backend emission. Unary boolean complement, flag algebra beyond same-enum value `|`/`&`/`^`/`~`, arbitrary/general computed enum values, broad attribute target validation, numeric pattern algebra, and flag-style reasoning over imported numeric metadata are not part of the implemented slice.

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

## 1.0 Pattern Matching Boundary

The 1.0 `match` boundary is finite and lowering-backed. Exhaustiveness diagnostics run for nominal unions, TypeSharp-owned enums, named imported C# enums, `bool`, non-literal local type-level unions, and literal-only local type-level unions. Supported arm patterns are union case names with an optional single identifier payload capture, enum member names, `true`/`false`, literal members of a literal-only type-level union, typed member patterns such as `value: string` for non-literal type-level unions, and `_` discard arms. `when` guards are type-checked in arm scope, non-boolean guard predicates report `TS2218`, and guarded arms do not prove coverage; add an unguarded arm or `_` for coverage.

Record and structural match patterns, nested payload destructuring, extractor-style patterns, standalone identifier binding patterns, numeric patterns over ordinary numeric inputs, numeric enum patterns, flag-style enum reasoning, flag algebra in patterns, and broader guard analysis are post-1.0. These forms now report `TS2211` before backend emission or remain parser-only syntax until deterministic semantics and C# 7.3-compatible lowering are implemented.

## F#-Style Functional Roadmap

TypeSharp follows F#'s functional consistency where the behavior can remain explainable to C# and .NET Framework consumers:

- immutable values, expression-oriented functions, records, nominal unions, `Option<T>`, `Result<T,E>`, pattern matching, pipeline, and composition are part of the MVP path;
- richer exhaustiveness remains a functional correctness priority inside the 1.0 pattern matching boundary; known nominal unions, TypeSharp-owned enums, named imported C# enums, `bool`, and local type-level unions including literal unions report missing cases/members, `_` can cover the remainder, and guarded arms do not prove coverage without an unguarded cover;
- struct-backed value options, recursive union ergonomics, and helper APIs such as bind/map/default are stable backlog items once their `net48` ABI and allocation tradeoffs are documented;
- general currying, partial application, computation-expression-style workflows, active-pattern-style extractors, units of measure, and type providers stay backlog or experimental until TypeSharp has deterministic lowering, diagnostics, and security boundaries.

The default TypeSharp functional model must not require `FSharp.Core` at runtime. Direct F# option, tuple, record, or union interop can be added later as a compatibility layer.

## 1.0 Functional Scope Boundary

The 1.0 functional scope is intentionally bounded rather than F#-complete. Supported functional ergonomics are immutable values, expression-oriented direct `fun` declarations, explicit function types that lower to CLR delegates, nominal unions, `Option<T>`/`Result<T,E>` Core types, pattern matching inside the documented 1.0 match boundary, direct first-argument pipeline calls, unary named-function composition, lambdas in supported delegate contexts, iterator `yield` over explicit enumerable return types, and `Task`-based async. These shapes either lower to ordinary C# 7.3 calls/delegates/iterators/tasks or use the documented TypeSharp Core/Runtime helper surface.

The 1.0 boundary does not include F#-complete higher-order inference, automatic currying, general partial application, imported pipeline or composition target inference, pipeline overload ranking, computation expressions, active patterns, units of measure, type providers, F# collection/workflow interop, or broad public ABI inference for function-shaped values. Public/exported function-shaped values and direct composition values require explicit function type annotations where the current lowering needs stable CLR metadata; unsupported shapes report deterministic `TS2215` diagnostics before generated C# emission.

Direct TypeSharp-declared function calls are checked when the target has a known parameter list. For `f(args...)`, supplied arguments must fit the declared required count, optional/default suffix, or final `params` tail, and each known argument type must be assignable to the corresponding parameter type. A final `params name: T[]` parameter accepts either one exact `T[]` argument or expanded trailing `T` arguments, including zero trailing arguments after fixed parameters; non-final, repeated, or non-array `params` declarations report `TS2215`. Trailing defaulted parameters use `name: Type = literal` and can be omitted from direct calls or first-argument pipeline lowering when the default literal is string, numeric, `true`, `false`, or `null`; generic direct `fun` defaults are accepted only when every defaulted parameter has an explicit concrete TypeSharp type that does not reference a type parameter. Non-trailing defaults, unsupported default expressions, default/type mismatches, `params` interaction, and generic defaulted parameter types such as `T`, `T[]`, or `List<T>` report `TS2215`. TypeSharp-owned direct calls may use named arguments after any positional prefix, and first-argument pipeline calls may name non-piped arguments. The checker binds those names to declared parameters, reports `TS2215` for unknown names, duplicates, positional arguments after named arguments, missing required parameters, wrong argument types, inconsistent generic inference, or `params` named combinations, and leaves imported C# named arguments on the metadata-backed interop path. Direct generic function calls support explicit type arguments plus inference/substitution when declared type parameters appear directly as parameter/return types or inside bounded constructed shapes such as `T[]` and matching single-argument generic wrappers like `List<T>`; repeated inferred type parameters must infer consistently, including through the implemented `params` tail when named arguments are not used. Generic named direct calls reuse the same bounded inference/substitution shapes except for generic `params` combinations, and concrete optional/default suffixes can still be omitted. Direct pipeline compatibility is checked when the pipeline target is a TypeSharp-declared function with a known parameter list. For `value |> f` and `value |> f(args...)`, `value` must be assignable to `f`'s first parameter, and the lowered call must supply arguments compatible with the known target parameter list, optional/default suffix, final `params` tail, or supported named non-piped arguments. Generic TypeSharp pipeline targets infer and substitute the same bounded simple, array, and matching single-argument generic wrapper shapes from the piped input plus non-piped arguments, including concrete optional/default suffixes and named non-piped arguments; the implemented `params` tail remains positional-only for generic targets. Direct named unary function composition is checked when both TypeSharp-declared function signatures are known: for `f >> g`, `f` must return a type assignable to `g`'s single parameter, and for `g << f`, `f` must return a type assignable to `g`'s single parameter. Generic TypeSharp composition targets infer and substitute bounded simple, array, and matching single-argument generic wrapper shapes from that composition edge when one side provides enough concrete type information. Explicit unary function-type annotations on direct composition value declarations validate the annotated input against the first composed function parameter and the composed result against the annotation return type before generated C# assignment. Unannotated non-exported top-level direct composition values infer concrete generated delegate types only when the bounded composition signature is fully known; public/exported direct composition values without an explicit function type annotation report `TS2215`. These checks keep common F#-style function/pipeline/composition mistakes deterministic without committing to higher-order function inference, imported generic function inference, imported composition inference, broader unannotated composition expression inference, public ABI inference for composition expressions, currying, partial application, broader type-constructor unification, pipeline overload ranking, TypeSharp function overload ranking, or multi-parameter composition.

## 1.0 Collection And Object Construction Boundary

The 1.0 construction boundary is limited to lowering-backed shapes with explicit CLR results. Collection expressions are accepted when the target is a known array or `System.Collections.Generic.List<T>`, including empty literals, homogeneous element literals, spread elements from known arrays or `List<T>`, imported C# overload arguments selected as array/List targets, imported `params` array arguments, and lambda return bodies whose delegate return type is a known array or `List<T>`. Nominal record expressions and record spread are accepted for expected TypeSharp record types and lower to constructor calls in record parameter order. Collection element mismatches, non-array/List collection spreads, record field mismatches, missing or unknown record fields, and non-record record spreads report `TS2219` before backend emission. Imported C# constructor calls, including selected generic constructors and named/optional/params constructor arguments, stay on the metadata-backed constructor path and report deterministic overload diagnostics before generated C# emission.

Dictionary literals, set literals, collection-expression constructor or factory arguments beyond the documented array/List and imported overload slices, object initializer syntax, arbitrary class object construction, inferred anonymous object construction, contextual collection inference without a known array/List target, general collection-builder protocols, and record/class/object initializer mutation are post-1.0. Unsupported construction forms report deterministic diagnostics such as `TS2219` before backend emission instead of relying on generated C# failures.

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
