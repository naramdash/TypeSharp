---
title: Diagnostics
description: Diagnostic codes, metadata, JSON shape, and explanation surface.
---

This is the canonical docs ledger for TypeSharp diagnostic code ranges, descriptor metadata, explanation output, JSON/text shape, and golden diagnostic fixture policy.

Command examples assume the release install route from [Install](../install/): open the tag-specific GitHub Release notes, confirm the exact asset names, download `typesharp-cli-dotnet-<tag>.zip`, verify `SHA256SUMS.txt`, extract `typesharp.cmd` onto `PATH`, and run `typesharp version`. Use the matching `typesharp-runtime-net48-<tag>.zip` and verify it with the same manifest when reproducing diagnostics that involve TypeSharp Core/Runtime DLL references.

Diagnostics are shared by:

- compiler and checker internals,
- `typesharp check` and `typesharp build`,
- `typesharp explain`,
- Language Server Protocol diagnostics,
- golden fixture snapshots.

Example explanation command:

```text
typesharp explain TS2204
```

## Code Ranges

Diagnostic codes use `TS` plus four digits. Once a public code is allocated, it should keep the same meaning.

| Range | Category | Purpose |
| --- | --- | --- |
| `TS0001`-`TS0099` | Tooling/Internal | CLI usage, tool orchestration, compiler internal error reporting. |
| `TS0100`-`TS0999` | Project | `TypeSharp.toml`, source discovery, project/reference configuration. |
| `TS1000`-`TS1999` | Parser | Lexer, parser, syntax recovery, unsupported stable grammar tokens. |
| `TS2000`-`TS2199` | Binding | Scope, declaration binding, imports, duplicate symbols, unresolved names. |
| `TS2200`-`TS2399` | Type Checking | Type inference, nullability, generic constraints, public type surface checks. |
| `TS2400`-`TS2599` | Interop | C# metadata, overload resolution, byref/params/delegate/event interop, public ABI metadata checks. |
| `TS3000`-`TS3499` | Lowering | IR/lowering failures, feature lowering restrictions, source mapping. |
| `TS3500`-`TS3999` | Backend | C# source backend, future IL backend, emitted artifact validation. |

## Allocated Descriptors

| Code | Category | Severity | Title |
| --- | --- | --- | --- |
| `TS0100` | Project | Error | TypeSharp manifest not found |
| `TS0101` | Project | Error | TypeSharp manifest cannot be read |
| `TS0102` | Project | Error | Invalid manifest syntax |
| `TS0103` | Project | Error | Invalid manifest value |
| `TS0110` | Project | Warning | Source root not found |
| `TS0111` | Project | Error | Duplicate source module path |
| `TS0112` | Project | Error | Unresolved source module |
| `TS0113` | Project | Error | Unsupported source module import |
| `TS0114` | Project | Error | Missing source module import or re-export target |
| `TS1000` | Parser | Error | Unexpected character |
| `TS1001` | Parser | Error | Missing function body |
| `TS1002` | Parser | Error | Unterminated string literal |
| `TS1003` | Parser | Error | Missing expression |
| `TS1004` | Parser | Error | Unexpected token |
| `TS2001` | Binding | Error | Unresolved name |
| `TS2002` | Binding | Error | Duplicate symbol |
| `TS2003` | Binding | Error | Unsupported export forwarding |
| `TS2004` | Binding | Error | Duplicate export |
| `TS2201` | Type Checking | Error | Type mismatch |
| `TS2202` | Type Checking | Error | Nullability contract violation |
| `TS2203` | Type Checking | Error | Non-exhaustive match over a known finite match input |
| `TS2204` | Type Checking | Error | Compile-time-only type leaked through public boundary |
| `TS2205` | Type Checking | Error | Unsupported generic constraint |
| `TS2206` | Type Checking | Error | Dynamic capability required |
| `TS2207` | Type Checking | Error | Dynamic call requires capability |
| `TS2208` | Type Checking | Error | Capability call requires marker |
| `TS2209` | Type Checking | Error | Unknown access requires narrowing |
| `TS2210` | Type Checking | Error | Unsupported TypeSharp class or interface member |
| `TS2211` | Type Checking | Error | Unsupported match pattern |
| `TS2212` | Type Checking | Error | Type operator limit exceeded |
| `TS2213` | Type Checking | Error | Unsupported extension property |
| `TS2214` | Type Checking | Error | Unsupported enum or bitwise operation |
| `TS2215` | Type Checking | Error | Unsupported TypeSharp function application |
| `TS2216` | Type Checking | Error | Unsupported assignment target |
| `TS2217` | Type Checking | Error | Unsupported imported C# operator |
| `TS2218` | Type Checking | Error | Invalid match guard |
| `TS2219` | Type Checking | Error | Unsupported construction expression |
| `TS2220` | Type Checking | Error | Structural proof failed |
| `TS2221` | Type Checking | Error | Unsupported extension method |
| `TS2222` | Type Checking | Error | Invalid arithmetic compound assignment |
| `TS2223` | Type Checking | Error | Unsupported null-conditional access |
| `TS2224` | Type Checking | Error | Invalid yield expression |
| `TS2225` | Type Checking | Error | Invalid lock expression |
| `TS2226` | Type Checking | Error | Invalid match case |
| `TS2227` | Type Checking | Error | Invalid satisfies expression |
| `TS2228` | Type Checking | Error | Invalid return expression |
| `TS2229` | Type Checking | Error | Invalid value initializer |
| `TS2230` | Type Checking | Error | Invalid assignment value |
| `TS2401` | Interop | Error | Missing referenced assembly or namespace |
| `TS2402` | Interop | Error | Ambiguous C# overload or constructor |
| `TS2403` | Interop | Error | Invalid byref interop use |
| `TS2404` | Interop | Warning | Unknown C# nullability |
| `TS2405` | Interop | Error | Unsupported package reference |
| `TS2406` | Interop | Error | No matching C# overload or constructor |
| `TS2407` | Interop | Error | Missing C# method |
| `TS2408` | Interop | Error | Missing C# type |
| `TS2409` | Interop | Error | Missing C# static member |
| `TS2410` | Interop | Error | Missing C# instance member |
| `TS2411` | Interop | Error | Missing or mismatched C# instance indexer |
| `TS2412` | Interop | Error | Missing C# instance property setter |
| `TS2413` | Interop | Error | Read-only C# instance field assignment |
| `TS2414` | Interop | Error | Missing C# static property setter |
| `TS2415` | Interop | Error | Read-only C# static field assignment |
| `TS2416` | Interop | Error | Missing C# instance event |
| `TS2417` | Interop | Error | Unsatisfied C# generic constraint |
| `TS3500` | Backend | Error | Unsupported executable entry point |
| `TS3501` | Backend | Error | Generated C# project build failed |

`TS2002` also reports duplicate enum members in the same enum declaration. `TS2201` remains the broad type-mismatch descriptor in the registry for compatibility, but current high-frequency assignability and unsupported-boundary paths have narrower descriptors. Function return expression mismatches use `TS2228` when the body expression type is known but not assignable to the declared return type, explicit value initializer mismatches use `TS2229`, and simple assignment value mismatches use `TS2230`. `TS2203` treats guarded nominal, TypeSharp-owned enum, imported C# enum, `bool`, and type-level union arms as non-covering unless a later unguarded arm or `_` discard covers the remaining closed set.

`TS2210` reports TypeSharp-authored class/interface member forms outside the 1.0 subset, including class constructor parameters missing explicit CLR-visible types, constructor `params` parameter shape failures, class getter-only properties missing explicit CLR-visible types or initializers, class property setter/custom accessor shapes, class/interface events without an explicit delegate type, class values missing explicit CLR-visible types or initializers, and unsupported skipped class/interface member syntax. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish unsupported class/interface member surface from ordinary type mismatch diagnostics.

`TS2211` reports executable match patterns outside the 1.0 pattern boundary, including record/structural patterns, nested union payload destructuring, extractor-style patterns, standalone identifier binding patterns, ordinary numeric patterns, numeric enum patterns, incompatible bool literal patterns, and unsupported type-level-union pattern shapes. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish unsupported pattern syntax from ordinary expression type mismatches.

`TS2212` reports 1.0 type-operator budget failures: alias cycles, alias depth beyond 16, local union width beyond 64 distinct members, `keyof` or indexed access selection beyond 64 keys, and structural intersection aliases beyond 64 resulting members. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish compile-time type-operator budget boundaries from ordinary value type mismatches.

`TS2213` reports TypeSharp-authored extension-property forms outside the 1.0 getter-only exact-receiver subset, including nullable receiver declarations or accesses, duplicate exact receiver/name declarations, ordinary/structural precedence conflicts, helper-name collisions, assignment targets, missing required property shape, accessor blocks, and null-conditional extension-property reads or assignments. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish extension-property policy boundaries from ordinary value type mismatches.

`TS2214` reports enum and bitwise algebra forms outside the 1.0 lowerable boundary, including unsupported enum underlying types, invalid enum member numeric values, invalid enum member aliases or composite initializer operands, missing enum members, invalid same-enum value `|`, `&`, `^`, or unary `~` operands, primitive integral bitwise/shift operand failures, boolean bitwise operand failures, and local bitwise or shift compound operand failures. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish enum/bitwise policy boundaries from ordinary value type mismatches.

`TS2215` reports TypeSharp-owned function application forms outside the 1.0 lowerable boundary, including invalid `params` or defaulted parameter declarations, direct call arity and argument failures, named argument binding failures, bounded generic inference failures, first-argument pipeline input/arity/argument failures, direct composition compatibility failures, explicit composition function-type annotation failures, and public direct-composition values that omit required function type annotations. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish TypeSharp function application policy boundaries from ordinary value type mismatches.

`TS2216` reports assignment target forms outside the 1.0 lowerable boundary, including immutable local bindings, non-binding assignment targets, unsupported imported C# member/indexer compound-assignment targets, and unsupported null-conditional imported C# member/indexer assignment targets. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish target-shape policy boundaries from ordinary assigned-value type mismatches.

`TS2217` reports imported C# operator forms outside the 1.0 lowerable boundary, including missing selected public static binary `operator *`, `operator /`, or `operator %` metadata for imported operand types, checked-only metadata such as `op_CheckedMultiply`, `op_CheckedDivision`, or `op_CheckedModulus`, instance-compound-only operator metadata, ambiguous imported static binary operators, nullable imported operator operands, and imported operator results that cannot assign back to the target. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish selected imported operator policy boundaries from primitive operand type mismatches.

`TS2218` reports match guard expressions that are known not to be non-null `bool`, including nullable, null, string, numeric, record, enum, or other non-boolean guard expressions. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish invalid guard predicates from ordinary arm result or expression type mismatches.

`TS2219` reports collection and record construction expressions outside the 1.0 lowerable boundary, including inconsistent collection literal element types, collection elements that do not match a known array/List target element type, spread elements that are not arrays or `List<T>`, record expressions missing required fields, record expressions with fields outside the expected nominal record, record field type mismatches, and record spreads over non-record values. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish construction-shape failures from ordinary expression type mismatches.

`TS2220` reports local structural proof failures, including missing required structural members, optional members where a required member is needed, structural member type mismatches, missing member access through a known structural shape, and impossible type-level-union discriminant literal checks. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish TypeScript-style structural proof failures from ordinary nominal assignment mismatches.

`TS2221` reports TypeSharp-authored extension-method receiver-shape failures, including missing extension receiver types, missing or untyped first receiver parameters, `params` receiver parameters, and first receiver parameters whose type does not exactly match the extension declaration receiver. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish extension-method policy failures from ordinary value type mismatches.

`TS2222` reports primitive arithmetic compound-assignment failures, including unsupported non-null primitive operand families, nullable operands, mixed decimal/floating-point multiplicative operands, and promoted `+=`, `-=`, `*=`, `/=`, or `%=` results that cannot assign back to the target. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish arithmetic compound-assignment failures from ordinary assignment mismatches. Imported C# operator metadata failures remain `TS2217`, and unsupported assignment target shapes remain `TS2216`.

`TS2223` reports null-conditional read shapes outside the 1.0 lowerable boundary, including unsupported `?.` member reads and `?[]` indexer reads that are not readable metadata-backed imported C# instance field/property/indexer targets. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish null-conditional read policy failures from ordinary type mismatches. Null-conditional assignment target failures remain `TS2216`, and null-conditional extension-property failures remain `TS2213`.

`TS2224` reports iterator yield expressions outside the 1.0 lowerable boundary, including `yield` inside a function that does not return `IEnumerable<T>` or `IEnumerator<T>` and yielded values that cannot assign to the iterator element type. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish iterator yield contract failures from ordinary expression type mismatches. Yield nullability violations remain `TS2202`, and structural yield proof failures remain `TS2220`.

`TS2225` reports lock gate expressions outside the 1.0 lowerable boundary, including known primitive, value-type, nullable, or null synchronization expressions that cannot lower to a valid C# `lock` gate. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish lock gate policy failures from ordinary expression type mismatches.

`TS2226` reports nominal-union match case-name patterns that do not name a declared case on the matched union. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish invalid match case failures from ordinary expression type mismatches. Unsupported pattern shapes remain `TS2211`, invalid guard predicates remain `TS2218`, and missing exhaustive coverage remains `TS2203`.

`TS2227` reports `satisfies` expressions whose value type is not assignable to the target type after nullability and structural proof checks. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish satisfies proof failures from ordinary local, assignment, or return type mismatches. Satisfies nullability violations remain `TS2202`, and structural proof failures remain `TS2220`.

`TS2228` reports function body expressions whose value type is not assignable to the declared return type after nullability and structural proof checks. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish return expression failures from local variable annotation or assignment mismatches. Return nullability violations remain `TS2202`, and structural return proof failures remain `TS2220`.

`TS2229` reports value declaration initializers whose value type is not assignable to the explicit annotation after nullability and structural proof checks. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish initializer annotation failures from later assignment mismatches. Initializer nullability violations remain `TS2202`, and structural initializer proof failures remain `TS2220`.

`TS2230` reports simple assignment values whose value type is not assignable to the mutable target after nullability and structural proof checks. These cases previously reused broad `TS2201`; they now have a dedicated code so tools can distinguish later assignment value failures from declaration initializers or unsupported assignment targets. Assignment nullability violations remain `TS2202`, structural assignment proof failures remain `TS2220`, and unsupported assignment targets remain `TS2216`.

For null-conditional imported C# field/property member and indexer `*=`, `/=`, and `%=` targets, the multiplicative diagnostics also include selected imported C# public static binary `operator *`, `operator /`, and `operator %` candidates whose result must assign back to the target value type; missing, ambiguous, nullable, and non-assignable operator shapes report `TS2217` before backend emission, while getter-only, readonly, static/event, TypeSharp-owned/local, and other unsupported target shapes report `TS2216`.

No reserved examples are currently listed. Future diagnostics should be added to this table only when implemented.

## Planned Advanced Type Operator Diagnostics

Mapped, conditional, template-literal, and utility type computation is a documented design boundary, not implemented behavior. Current limited type-operator budget diagnostics use `TS2212` for alias cycles, alias depth, local union width, `keyof`/indexed key count, and structural intersection member count. Future evaluator diagnostics belong in the `TS2200`-`TS2399` type-checking range, but new descriptor codes should be allocated only when the parser/checker implementation lands.

Expected diagnostic classes:

- unsupported advanced type-operator syntax or unsupported utility alias;
- recursive or cyclic type-operator instantiation;
- evaluator budget exceeded, including alias depth, total reductions, union width, mapped key count, conditional distribution branches, or template literal product size;
- unresolved mapped key, indexed member, conditional constraint, or utility input;
- computed type result rejected at a public CLR boundary unless it fully normalizes to a CLR-visible type;
- ambiguous or non-deterministic evaluation caused by imported metadata, dynamic values, overload sets, declaration-file assumptions, or runtime-dependent facts.

Until those codes are allocated, docs should refer to these as planned diagnostic classes rather than stable public codes.

## Descriptor Metadata

Every diagnostic descriptor must provide:

- `Code`: stable `TS####` identifier,
- `Title`: short user-facing name for `typesharp explain`,
- `DefaultSeverity`: `error`, `warning`, or `info`,
- `Category`: one code range category,
- `MessageTemplate`: short diagnostic message shape,
- `Explanation`: why the compiler reports this diagnostic,
- `SuggestedAction`: the next action a user should take.

Call sites may add source-specific details to emitted messages, but descriptor code and severity remain stable. `typesharp explain <code>` reads descriptor metadata rather than parsing emitted diagnostic text.

## Output Shape

Text diagnostics:

```text
src/Main.tysh(8,15): error TS2204: Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.
```

JSON diagnostics:

```json
{
  "diagnostics": [
    {
      "code": "TS2204",
      "severity": "error",
      "message": "Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.",
      "file": "src/Main.tysh",
      "start": { "line": 8, "column": 15 },
      "end": { "line": 8, "column": 33 }
    }
  ]
}
```

Rules:

- code is stable,
- source span uses 1-based line and column,
- CLI text and JSON diagnostics print emitted message plus source location,
- VS Code diagnostics use the same code and span as `typesharp check`,
- JSON diagnostics must convert to LSP diagnostics without loss.

## Golden Diagnostic Policy

Parser fixtures keep `expected.diagnostics.json` next to `expected.tree` because syntax recovery and diagnostic shape are coupled.

Semantic diagnostics use golden fixtures under:

```text
test/fixtures/diagnostics/
  binder/
    positive/
    negative/
  type-checker/
    positive/
    negative/
  interop/
```

Golden diagnostics assert:

- stable code,
- severity,
- user-facing message,
- file path,
- start/end source span,
- related information when the diagnostic model grows it.

Descriptor registry smoke tests must assert allocated code uniqueness and require title, message template, explanation, and suggested action for every descriptor.

## Current Implementation

The descriptor registry keeps diagnostic code, severity, category, explanation, and suggested action stable for tools. Current project diagnostics include manifest/source discovery/module graph errors like missing project-reference manifests with `TS0100`, invalid source aliases, project-reference cycles, duplicate direct project names, and incompatible project-reference target frameworks with `TS0103`, duplicate source module paths with `TS0111`, unresolved source modules, alias targets, or direct project source module specifiers with `TS0112`, unsupported or ambiguous source module import forms with `TS0113`, and missing source module import, re-export, or direct project referenced export targets with `TS0114`; current binding diagnostics include unresolved names like `TS2001`, duplicate symbols like `TS2002`, including same-file import alias conflicts and duplicate enum members, unsupported export forwarding like `TS2003`, and duplicate local exports like `TS2004`; current type-checking diagnostics include broad type mismatches like `TS2201`, nullability assignment and return violations like `TS2202`, non-exhaustive matches like `TS2203`, public-boundary leaks like `TS2204`, C# 7.3 backend constraint limits like `TS2205`, dynamic annotation boundary errors like `TS2206`, dynamic call propagation errors like `TS2207`, reflect/interop/unsafe capability call marker errors like `TS2208`, unknown member/indexer access errors like `TS2209`, unsupported TypeSharp-authored class/interface member forms like `TS2210`, unsupported match patterns like `TS2211`, type-operator budget failures like `TS2212`, extension-property policy failures like `TS2213`, enum/bitwise policy failures like `TS2214`, TypeSharp function application policy failures like `TS2215`, assignment target policy failures like `TS2216`, imported C# operator policy failures like `TS2217`, match guard predicate failures like `TS2218`, construction-shape failures like `TS2219`, structural proof failures like `TS2220`, extension-method policy failures like `TS2221`, arithmetic compound-assignment failures like `TS2222`, null-conditional read policy failures like `TS2223`, iterator yield contract failures like `TS2224`, lock gate policy failures like `TS2225`, invalid match case failures like `TS2226`, satisfies proof failures like `TS2227`, return expression failures like `TS2228`, value initializer failures like `TS2229`, and assignment value failures like `TS2230`; current interop diagnostics include missing references like `TS2401`, ambiguous C# overloads, constructors, or imported indexer candidates like `TS2402`, no matching C# overloads or constructors like `TS2406`, missing C# methods like `TS2407`, missing C# types like `TS2408`, missing C# static members like `TS2409`, missing C# instance members like `TS2410`, missing or mismatched C# instance indexers like `TS2411`, missing C# instance property setters like `TS2412`, readonly C# instance field assignment errors like `TS2413`, missing C# static property setter errors like `TS2414`, readonly C# static field assignment errors like `TS2415`, missing C# instance event errors like `TS2416`, and unsatisfied explicit or inferred C# generic constraint errors like `TS2417`.
The null-conditional member/indexer multiplicative entries include both the bounded primitive assign-back policy and the selected imported static binary operator policy for readable/writable metadata-backed imported C# field/property members and indexers.

The compiler exposes descriptor metadata through `TypeSharp.Compiler.Diagnostics.DiagnosticDescriptors`. `typesharp explain <code>` reads that registry directly. Unknown descriptor codes return exit code `1`; CLI usage errors return exit code `2`.

## Related Pages

- [CLI](../cli/)
- [Troubleshooting](../troubleshooting/)
- [VS Code And LSP](../vscode-lsp/)
