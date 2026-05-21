---
title: Lowering
description: Canonical TypeSharp-to-C# 7.3 lowering contract, runtime dependencies, public ABI boundaries, and fixture evidence.
---

This page is the docs canonical lowering ledger established by task `0251-docs-canonical-language-ledger`. The former repository bridge file `docs/lowering.md` was removed after its durable content was folded here.

## Backend Contract

The MVP backend emits deterministic C# 7.3-compatible source and builds a generated SDK-style `net48` project. Direct IL emission remains future backend work.

Required lowering rules:

- Generated C# must compile with the C# 7.3 compiler profile used by `.NET Framework 4.8` projects.
- Generated assemblies must be referenceable from C# `net48` consumers as ordinary library or executable outputs.
- Type-level unions, structural shapes, structural intersections, `keyof`, and indexed access types are compile-time tools unless they lower to a nominal CLR-visible type.
- Public TypeSharp APIs must expose records, classes, interfaces, delegates, nominal unions, or other CLR-understandable shapes.
- Runtime helper dependencies must stay inside package-free `net48`-compatible `TypeSharp.Core` or `TypeSharp.Runtime` surfaces.
- Generated output belongs under the configured generated output root and is rebuildable.

## Lowering Pipeline

The compiler runs the lowering pipeline before C# source emission.

Pipeline requirements:

- Pass order is explicit.
- Passes should be idempotent and deterministic.
- Passes must not introduce duplicate helper imports or unstable generated names.
- Backend output stability is part of regression testing.

Implemented pipeline behavior includes the `csharp-runtime-import` pass, which adds the `TypeSharp.Runtime` import when union declarations or match expressions need runtime helpers.

## Generated Shape Map

| TypeSharp Surface | Generated C# Shape |
| --- | --- |
| Single-source top-level functions | Static methods on generated `Module`. |
| Multi-source top-level functions | Static methods on deterministic module-path containers such as `ModuleMain` or `ModuleFeature_Helper`. |
| `export` declarations | Public C# surface when the declaration is CLR-representable. |
| Local `let` | C# local `var`. |
| Final block expression | `return` when the enclosing function expects a value. |
| `if` expression | C# `if`/`else` inside an immediately invoked `System.Func<T>` when expression-position lowering needs a value. |
| Named imports | C# `using` or alias `using` directives. |
| `import static` | C# `using static`. |
| Manifest references | Generated project `<Reference>` items. |
| Records | Immutable sealed C# classes with constructor, get-only properties, equality, and hash code. |
| Enums, assignment, and bitwise operators | Ordinary C# enum declarations; explicit integral underlying types lower as C# enum base types; explicit integer member values, aliases, and enum initializer-local composite values such as `ReadWrite = Read \| Write` lower as C# enum assignments; enum member access remains `EnumName.Member`; same-enum value `\|`, `&`, `^`, and unary `~` expressions lower as C# `\|`, `&`, `^`, and `~`; primitive integral `<<` and `>>` shifts lower as C# shift operators when the right count is C# 7.3-compatible; mutable local identifier assignments lower as ordinary C# assignments after `let mut` and known compatibility checks; bitwise compound assignments lower as C# `\|=`, `&=`, and `^=`; shift assignments lower as C# `<<=` and `>>=` when mutable local operands satisfy the primitive integral shift policy; logical unsigned shift assignment `>>>=` lowers for mutable local primitive integral targets, readable/writable metadata-backed imported C# field/property member targets, and metadata-backed imported C# instance indexer targets with a selected public getter/setter as explicit C# 7.3-compatible assignment/cast forms with no emitted `>>>` or `>>>=`; bounded null-conditional imported member assignment `receiver?.Member = value` lowers through explicit `System.Func<TReceiver,TMember>` null guards with no emitted `?.`. |
| Nominal unions | Abstract base type plus sealed case types and runtime pattern helper metadata. |
| Pattern matching over nominal unions | Ordered C# case checks using runtime pattern helpers; `_` lowers to an unconditional fallback arm. |
| Pattern matching over TypeSharp-owned and named imported C# enums | Ordered C# enum member comparisons; `_` lowers to an unconditional fallback arm. |
| Pattern matching over bool and local literal unions | Ordered C# conditional comparisons using `object.Equals`; `_` lowers to an unconditional fallback arm. |
| Type-level unions in local code | Local erased representation, usually `object`, with narrowing checks. |
| Structural shape proofs | Compile-time checks; no public metadata shape. |
| `async fun` | C# `async` method returning `Task` or `Task<T>`. |
| `yield` | C# `yield return` inside explicit CLR enumerable functions. |
| `lock` | C# `lock (expr) { ... }` statement. |
| Parenthesized expressions | Preserve grouping as C# parentheses. |
| Unary logical not | C# `!expr`. |
| Unary numeric sign | C# `+expr` or `-expr`. |
| Pipeline | Nested first-argument calls. |
| Composition | Unary delegate lambdas. |
| `satisfies` | Erases to the left expression after compile-time proof. |
| Collection expressions | Array creation, `List<T>` initializer, or LINQ concatenation for supported spreads. |
| Indexer expressions | C# array or indexer access. |
| `nameof` | C# `nameof(...)` for ordinary name references; string constants for unbound generic type targets such as `nameof(List<>)`. |
| `checked` / `unchecked` | C# overflow context expressions. |
| `keyof` aliases | Local string-based implementation details. |
| Indexed access type aliases | Selected member runtime type for single-member local aliases; type-level union behavior for multi-member aliases. |
| Explicit-receiver extension methods | C# extension methods with `this` on the receiver parameter. |
| Function type values | `System.Func` or `System.Action` when arity and return shape are representable; unannotated top-level lambda values use conservative `Func<object, TResult>` inference and can lower supported block and collection return bodies. Unannotated non-exported direct composition values infer concrete `Func<TInput,TResult>` or `Action<TInput>` only for fully known direct TypeSharp-declared unary signatures; public/exported direct composition values require explicit function type annotations. |

## Function, Module, And Import Lowering

Top-level functions lower to generated static methods. Expression-bodied functions become block-bodied C# methods with `return`. A valid final `params name: T[]` parameter lowers to a C# 7.3-compatible `params T[] name` signature. Multi-source projects use source-module-path containers so two files in the same namespace do not collide.

Named imports from metadata namespaces lower to `using` directives; aliases lower to C# alias directives; manifest references are copied into generated project reference items.

Top-level function-valued `let` declarations lower to generated delegate fields when they have an explicit function type annotation or a lambda initializer. Unannotated lambda-valued exports use a conservative CLR delegate shape: the parameter side is `object`, and simple return bodies, including compatible block final expressions, `if` branch bodies, and homogeneous collection expression bodies, infer `TResult` where possible. This keeps the generated API CLR-representable while nudging precise public callback contracts toward explicit annotations.

Evidence:

- `test/fixtures/backend/csharp/positive/0001-string-return`
- `test/fixtures/backend/csharp/positive/0002-import-directives`
- `test/fixtures/backend/csharp/positive/0003-call-expression`
- `test/fixtures/backend/csharp/positive/0008-basic-semantics`
- `test/fixtures/backend/csharp/positive/0045-params-parameter-lowering`
- CLI smokes for basic semantics, framework calls, local DLL calls, manifest reference propagation, and module-path containers.
- CLI smokes for annotated and inferred function-valued top-level `let` exports, including collection return bodies.

## Records, Enums, Public Types, And Partial Declarations

Records lower to immutable C# classes with constructor parameters, get-only properties, value equality, and hash code. Record construction, record update, and nominal record spread lower to constructor calls in record parameter order.

Simple enum declarations lower to ordinary generated C# enum declarations. Explicit integral underlying types such as `: byte` are preserved as C# enum base types, explicit integer member values such as `Red = 1` are validated against the selected underlying type before being preserved as C# enum member assignments, aliases such as `Crimson = Red` are preserved after same-enum validation, and enum initializer-local composites such as `ReadWrite = Read | Write` are preserved as C# enum member assignments after each identifier operand is validated. Declaration and member attributes lower before the generated C# enum or member, including `[FlagsAttribute]` metadata shape, but flags do not affect matching or numeric reasoning yet. Member references such as `Color.Green` stay as direct C# enum member access, same-enum value expressions such as `Color.Red | Color.Blue`, `value & Color.Blue`, `value ^ Color.Red`, and `~value` lower to ordinary C# `|`, `&`, `^`, and `~` expressions after type checking, integral numeric `|`, `&`, `^`, unary `~`, `<<`, and `>>` expressions lower to the same C# operators for supported primitive integral operands, logical unsigned `>>>` expressions lower to explicit unsigned casts plus ordinary `>>`, and boolean `|`, `&`, and `^` expressions lower to ordinary C# non-short-circuit boolean operators. Shift lowering preserves C# 7.3 compatibility by requiring the count expression to be a non-null `byte`, `sbyte`, `short`, `ushort`, or `int`; small left operands lower with C#'s ordinary promotion to `int`, and signed `>>>` lowers with `unchecked` cast shapes such as `unchecked((int)(unchecked((uint)(value)) >> count))` or `unchecked((long)(unchecked((ulong)(value)) >> count))`. Local identifier assignments lower only after the checker confirms the target is a `let mut` binding and validates known simple assignment compatibility; bitwise compound assignments `|=`, `&=`, and `^=` additionally validate known local enum/integral/bool operands before lowering directly to C# assignment operators; shift assignments `<<=` and `>>=` validate known local primitive integral targets and int-compatible counts before lowering directly to C# assignment operators. Logical unsigned shift assignment `>>>=` validates the same mutable local primitive integral target/count set and lowers with explicit assignment/cast forms such as `result = unchecked((int)(unchecked((uint)(result)) >> count))`, `result = unchecked((long)(unchecked((ulong)(result)) >> count))`, or `result = unchecked((byte)(unchecked((uint)(result)) >> count))`; unsigned `uint`/`ulong` targets use ordinary `>>` on the unsigned value. Readable/writable metadata-backed imported C# instance/static field and property member `>>>=` targets use the same explicit cast strategy; simple receivers lower as ordinary assignments, and non-trivial receivers lower through a generated `System.Func<TReceiver,TMember>` expression so the receiver expression is evaluated once before getter/setter use. Metadata-backed imported C# instance indexer `>>>=` targets with a selected public getter/setter use the same cast strategy; simple receiver/index argument shapes lower as ordinary indexer assignments, and non-trivial receiver or index arguments lower through a generated `System.Func<...>` expression so the receiver and each index argument are evaluated once before getter/setter use. Generated C# never emits `>>>` or `>>>=`. Imported C# event, unresolved member/indexer, and unsupported indexer `>>>=` targets remain rejected before emission. Null-conditional assignment `receiver?.Member = value` for metadata-backed imported C# instance fields/properties lowers to an immediately invoked `System.Func<TReceiver,TMember>` whose body checks the receiver for `null`, returns `default(TMember)` when null, and evaluates `value` only inside the non-null assignment branch. This makes both statement and expression positions valid C# 7.3 while evaluating non-trivial receivers once and never emitting `?.`. Null-conditional indexers, compound assignment, increment/decrement, events, static targets, local binding targets, TypeSharp-owned member targets, and null-conditional reads remain rejected before emission. Exhaustive enum matches over TypeSharp-owned enums and named imported C# enums lower to ordered `object.Equals` comparisons against generated or imported enum members. Imported C# enum underlying/numeric metadata is captured for future interop decisions, but current lowering remains name/member based. Unary boolean complement, arbitrary/general computed enum member values, flag algebra beyond same-enum value `|`/`&`/`^`/`~`, broad attribute target validation, numeric pattern algebra, and flag-style reasoning over imported numeric metadata are not lowered yet.

Public class, interface, generic type, generic function, and delegate-compatible function value surfaces lower only when their CLR shape is explicit. `partial` is preserved for declarations that lower to generated C# type declarations: modules, records, unions, classes, and interfaces.

Evidence:

- `test/fixtures/backend/csharp/positive/0011-generic-function-api`
- `test/fixtures/backend/csharp/positive/0012-class-declaration-api`
- `test/fixtures/backend/csharp/positive/0013-interface-declaration-api`
- `test/fixtures/backend/csharp/positive/0014-generic-type-declaration-api`
- `test/fixtures/backend/csharp/positive/0015-immutable-record-api`
- `test/fixtures/backend/csharp/positive/0016-record-update-lowering`
- `test/fixtures/backend/csharp/positive/0025-record-expression-construction`
- `test/fixtures/backend/csharp/positive/0026-partial-declarations`
- `test/fixtures/backend/csharp/positive/0035-record-spread-lowering`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/fixtures/backend/csharp/positive/0040-enum-match-exhaustiveness-lowering`
- `test/fixtures/backend/csharp/positive/0041-integral-bitwise-lowering`
- `test/fixtures/backend/csharp/positive/0042-boolean-bitwise-lowering`
- `test/fixtures/backend/csharp/positive/0043-bitwise-compound-assignment-lowering`
- `test/fixtures/backend/csharp/positive/0050-integral-shift-lowering`
- `test/fixtures/backend/csharp/positive/0051-shift-assignment-lowering`
- `test/fixtures/backend/csharp/positive/0053-logical-unsigned-shift-assignment-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs` enum declaration API CLI build smoke
- `test/TypeSharp.Compiler.Tests/Program.cs` enum match exhaustiveness CLI build smoke
- `test/TypeSharp.Compiler.Tests/Program.cs` imported C# enum match CLI build smoke

## Union And Pattern Lowering

Nominal unions lower to an abstract base type with sealed nested case types. Payload-free cases expose static properties; payload cases expose factory methods. Generated cases implement runtime helper metadata so pattern helpers can inspect tags, names, and payloads.

Nominal union matches lower to ordered C# checks. Payload extraction uses runtime helpers. `when` guards lower to nested C# conditionals after the arm's payload or type binding is available, guarded `_` arms lower to conditional fallbacks, and unguarded `_` arms lower to unconditional fallback returns in source order. Non-exhaustive matches should be reported before backend emission.

Type-level unions are local compile-time constructs. Their type-pattern matches lower to C# type checks where supported, and their literal-union matches lower to ordered `object.Equals` comparisons. Guarded arms evaluate after the relevant type binding or literal comparison, and public boundary leaks report diagnostics.

Evidence:

- `test/fixtures/backend/csharp/positive/0017-nominal-union-api`
- `test/fixtures/backend/csharp/positive/0018-nominal-union-match-lowering`
- `test/fixtures/backend/csharp/positive/0019-type-level-union-narrowing`
- `test/fixtures/backend/csharp/positive/0038-literal-match-lowering`
- `test/fixtures/diagnostics/type-checker/positive/match-guards`
- `test/fixtures/diagnostics/type-checker/positive/literal-match-exhaustiveness`
- `test/fixtures/diagnostics/type-checker/negative/literal-match-non-exhaustive`
- `test/fixtures/diagnostics/type-checker/negative/guarded-only-non-exhaustive-match`
- `test/fixtures/diagnostics/type-checker/negative/non-exhaustive-union-match`
- `test/fixtures/diagnostics/type-checker/negative/public-boundary-union-alias`

## Functional And Control Lowering

Pipeline expressions lower left-to-right into nested first-argument calls:

```tysh
value |> f |> g(arg)
```

becomes equivalent to:

```tysh
g(f(value), arg)
```

Direct TypeSharp-declared function calls check known arity and argument types before ordinary C# call emission. Direct calls to functions with a valid final `params T[]` parameter accept either one exact array argument or expanded trailing element arguments; generated calls stay ordinary C# invocations. Direct calls to functions with trailing literal defaults may omit the defaulted suffix; generated signatures emit C# optional parameters such as `string name = "world"`, `int count = 1`, `bool enabled = true`, or `string value = null`, and generated TypeSharp calls stay ordinary shorter C# invocations. Public exported functions expose the same optional metadata to C# consumers. Generic direct `fun` declarations may also use those defaults when each defaulted parameter has an explicit concrete TypeSharp type that does not reference a type parameter, so `choose<T>(value: T, fallback: string = "fallback")` lowers to a C# 7.3-compatible generic method with ordinary optional metadata. TypeSharp-owned direct calls may use named arguments, and first-argument pipeline calls may name non-piped arguments; accepted calls are reordered before emission so generated source remains ordinary positional C# such as `decorate("value", "[", ">")` or `choose<string>("c", "d")`. Defaulted parameters do not lower when their parameter type references a generic type parameter, or on ambient/`extern` signatures, `params` parameters, constructors, delegates, union cases, function types, lambdas, or higher-order values in the current slice. Direct generic TypeSharp-declared function calls infer or accept type arguments for direct type-parameter parameter/return positions and bounded constructed shapes such as arrays, matching single-argument generic wrappers, or the implemented final-array `params` tail before the same C# 7.3-compatible call emission. Named generic direct calls use the same inference/substitution path except for generic `params` combinations, which diagnose before emission. Pipeline expressions with direct TypeSharp-declared function targets check the piped input, lowered argument count, non-piped call arguments, optional/default suffix, final `params` tail, and supported named non-piped arguments before lowering to `f(value, args...)`; generic TypeSharp pipeline targets use the same bounded inference/substitution before that unchanged call emission, including concrete optional/default suffixes and named non-piped arguments except for generic `params` combinations. Composition expressions lower to unary delegate lambdas when operands are function-shaped. Known primitive integral `>>` and `<<` value operands lower to C# shifts; known primitive integral logical unsigned `>>>` lowers to C# 7.3-compatible explicit unchecked unsigned casts plus ordinary `>>`, such as `unchecked((int)(unchecked((uint)(value)) >> count))` or `unchecked((long)(unchecked((ulong)(value)) >> count))`; and local logical unsigned shift assignment `>>>=` lowers with the same cast strategy in assignment form. Other value-shaped operands report `TS2201` before lowering so invalid shifts are not emitted as composition or invalid generated C#. Direct named unary function pairs with known TypeSharp signatures are checked before lowering so incompatible `f >> g` or `g << f` pairs stop before invalid generated C#; generic TypeSharp unary targets use bounded edge inference/substitution for simple, array, and matching single-argument wrapper shapes before the same delegate-lambda lowering. Explicit unary function-type annotations on direct composition value declarations validate the composed input and result before the same generated delegate assignment. Unannotated non-exported direct composition value declarations with fully known composed signatures lower as concrete `System.Func<TInput,TResult>` or `System.Action<TInput>` fields; public/exported direct composition values without explicit function type annotations report `TS2201` before emission. Value-producing `if` expressions lower to C# 7.3-compatible `if`/`else` blocks inside an immediately invoked `System.Func<T>` when they appear where C# needs an expression value. Block-bodied lambdas lower to C# block lambdas and return the final block expression for value-returning delegate targets. Collection expression lambda bodies use the delegate return target when it is an array or `List<T>`, so `text => [text]` lowers with the expected element type. `yield` lowers to C# iterator statements when the function return type is an explicit `IEnumerable<T>` or `IEnumerator<T>`. `lock` lowers to C# lock statements and requires a non-null reference gate. F#-inspired computation-expression workflows, active patterns, and general partial application are not lowered yet; they need explicit TypeSharp semantics before generated C# can stay readable and deterministic.

Evidence:

- `test/fixtures/backend/csharp/positive/0023-pipeline-lowering`
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- `test/fixtures/backend/csharp/positive/0044-direct-generic-function-call-lowering`
- `test/fixtures/backend/csharp/positive/0045-params-parameter-lowering`
- `test/fixtures/backend/csharp/positive/0046-optional-default-parameter-lowering`
- `test/fixtures/backend/csharp/positive/0047-direct-named-argument-lowering`
- `test/fixtures/backend/csharp/positive/0048-generic-named-argument-lowering`
- `test/fixtures/backend/csharp/positive/0049-generic-optional-default-parameter-lowering`
- `test/fixtures/diagnostics/type-checker/positive/params-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/negative/params-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/positive/optional-default-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/negative/optional-default-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/positive/direct-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/negative/direct-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/positive/generic-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/negative/generic-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/positive/integral-shift-expression`
- `test/fixtures/diagnostics/type-checker/positive/logical-unsigned-shift-expression`
- `test/fixtures/diagnostics/type-checker/negative/logical-unsigned-shift-invalid`
- `test/fixtures/backend/csharp/positive/0052-logical-unsigned-shift-lowering`
- `test/fixtures/diagnostics/type-checker/positive/logical-unsigned-shift-assignment-expression`
- `test/fixtures/diagnostics/type-checker/negative/logical-unsigned-shift-assignment-invalid`
- `test/fixtures/backend/csharp/positive/0053-logical-unsigned-shift-assignment-lowering`
- `test/fixtures/diagnostics/type-checker/negative/direct-function-call-arity-and-argument`
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-function-call-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-function-call-inference`
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-pipeline-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-pipeline-inference`
- `test/fixtures/diagnostics/type-checker/negative/pipeline-function-input-compatibility`
- `test/fixtures/diagnostics/type-checker/negative/pipeline-target-arity-and-argument`
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-composition-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-composition-inference`
- `test/fixtures/diagnostics/type-checker/positive/composition-function-type-annotation-compatibility`
- `test/fixtures/diagnostics/type-checker/negative/composition-function-type-annotation-compatibility`
- `test/fixtures/diagnostics/type-checker/negative/composition-shift-ambiguity`
- `test/fixtures/diagnostics/type-checker/negative/composition-function-compatibility`
- `test/TypeSharp.Compiler.Tests/Program.cs` if-expression, block lambda, and collection lambda delegate plus inferred function-valued export CLI build smokes
- `test/fixtures/backend/csharp/positive/0031-yield-expression-lowering`
- `test/fixtures/backend/csharp/positive/0032-lock-statement-lowering`
- `test/fixtures/diagnostics/type-checker/negative/yield-mismatch`
- `test/fixtures/diagnostics/type-checker/negative/lock-value-type`

## Structural Proof And Type Operator Lowering

Structural shapes and intersections are compile-time proofs. `satisfies` validates the proof and emits the left expression. Public ABI cannot expose structural shapes or intersections directly.

Limited `keyof T` over known records or named shapes is compile-time-only and lowers to string implementation details when private/internal. Limited `T["Member"]` selects known record/shape member types and lowers to the selected runtime type for single-member local aliases.

Evidence:

- `test/fixtures/backend/csharp/positive/0030-satisfies-expression-lowering`
- `test/fixtures/backend/csharp/positive/0036-keyof-type-lowering`
- `test/fixtures/backend/csharp/positive/0037-indexed-access-type-lowering`
- `test/fixtures/diagnostics/type-checker/positive/intersection-structural-shape`
- `test/fixtures/diagnostics/type-checker/negative/public-boundary-intersection-alias`
- `test/fixtures/diagnostics/type-checker/negative/indexed-access-record-mismatch`

## Collections, Indexers, Intrinsics, And Extensions

Collection expressions lower to C# 7.3-compatible array creation or `List<T>` initializers. Spread elements over known arrays or `List<T>` lower through `System.Linq.Enumerable.Concat<T>` with target-specific array/list materialization. Homogeneous collection expression arguments to imported C# array overloads and single collection expression arguments in `params T[]` positions participate in metadata-backed overload filtering before emission.

Indexer expressions preserve C# array or indexer access and validate imported C# indexer arguments where metadata is known. Parenthesized indexer arguments preserve their generated grouping while metadata validation uses the enclosed expression type.

Parenthesized expressions preserve grouping in generated C#. Metadata validation uses the enclosed expression type for implemented imported overload and indexer argument checks. Unary logical-not and numeric sign expressions lower directly to C# `!expr`, `+expr`, or `-expr`. Ordinary `nameof` targets lower to C# `nameof(...)`; unbound generic type targets such as `nameof(Box<>)` and `nameof(Pair<,>)` lower to string constants so generated `net48` source remains C# 7.3-compatible. `checked` and `unchecked` lower directly to C# 7.3-compatible intrinsics. Explicit-receiver extension methods lower to C# extension methods in a static helper container.

Evidence:

- `test/fixtures/backend/csharp/positive/0022-collection-expression-lowering`
- `test/fixtures/backend/csharp/positive/0024-indexer-expression-lowering`
- `test/fixtures/backend/csharp/positive/0027-nameof-intrinsic`
- `test/fixtures/backend/csharp/positive/0028-checked-unchecked-expression`
- `test/TypeSharp.Compiler.Tests/Program.cs` parenthesized expression CLI build smoke
- `test/fixtures/parser/positive/0033-logical-not-expression`
- `test/TypeSharp.Compiler.Tests/Program.cs` logical-not lambda delegate CLI build smoke
- `test/TypeSharp.Compiler.Tests/Program.cs` unary numeric lambda delegate and inferred function-valued export CLI build smokes
- `test/fixtures/backend/csharp/positive/0033-extension-method-lowering`
- `test/fixtures/backend/csharp/positive/0034-collection-spread-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs` imported collection expression overload CLI build smoke

## Async And .NET Interop Lowering

`async fun` lowers to a C# `async` method. `await` lowers to C# `await`. Public async APIs must currently spell `Task` or `Task<T>` explicitly.

C# interop lowering preserves supported constructor calls, method calls, member access, field/property/indexer access, delegates, events, attributes, generic method calls, and byref arguments as ordinary generated C# where metadata validation has already succeeded.

Evidence:

- `test/fixtures/backend/csharp/positive/0020-async-task-interop`
- C# interop CLI smokes listed in [Work Ledger](../work-ledger/)
- [.NET Interop](../dotnet-interop/) for metadata validation boundaries.

## Unsupported Or Compile-Time-Only

These constructs must not appear directly in public CLR metadata:

- type-level union aliases,
- structural shape aliases,
- structural intersection aliases,
- anonymous object or record shapes,
- unlowered inferred anonymous function types,
- `unknown`,
- marker-free `dynamic`.

Use a nominal union, record, class, interface, delegate, or explicit wrapper before crossing a public boundary.

## Related Pages

- [Advanced Topics](../advanced/)
- [.NET Interop](../dotnet-interop/)
- [Type System](../type-system/)
- [Feature Status](../feature-status/)
- [Grammar And Language Reference](../reference/)
