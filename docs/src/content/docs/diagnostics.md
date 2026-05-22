---
title: Diagnostics
description: Diagnostic codes, metadata, JSON shape, and explanation surface.
---

This is the canonical docs ledger for TypeSharp diagnostic code ranges, descriptor metadata, explanation output, JSON/text shape, and golden diagnostic fixture policy.

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

`TS2002` also reports duplicate enum members in the same enum declaration. `TS2201` also reports unsupported enum underlying types, enum member numeric values outside the selected underlying type range, non-integral enum member numeric values, enum member aliases or composite initializer operands that do not reference a previously declared member of the same enum, enum value `|`, `&`, `^`, or unary `~` operands that are not valid same-enum values, integral bitwise operands that are not supported non-null primitive integral values, integral shift and logical unsigned shift operands that are not supported non-null primitive integral values or whose right count is not C# 7.3 int-compatible, local shift assignment and logical unsigned shift assignment operands that are not supported non-null primitive integral targets or whose right count is not C# 7.3 int-compatible, imported C# event, unresolved member/indexer, missing-setter indexer, nullable, non-integral, enum, record, unsupported-count, or otherwise unsupported imported member/indexer `>>>=` targets that lack a supported single-evaluation lowering policy, null-conditional member/indexer access outside the bounded imported C# instance field/property/indexer read shapes, simple imported C# instance field/property/indexer assignment shapes, imported C# instance field/property `receiver?.Member >>>= count` shape, imported C# instance indexer `receiver?[index] >>>= count` shape, imported C# instance field/property `receiver?.Member |= value`, `receiver?.Member &= value`, and `receiver?.Member ^= value` shapes, imported C# instance indexer `receiver?[index] |= value`, `receiver?[index] &= value`, and `receiver?[index] ^= value` shapes, unsupported null-conditional assignment operators or targets, boolean bitwise operands that are not both known non-null `bool`, direct TypeSharp-declared function calls whose known arity, positional/named arguments, optional/default suffix, or final `params` tail do not match the declared parameter list, invalid TypeSharp `params` declarations that are non-final, repeated, or not array-typed, invalid TypeSharp defaulted parameter declarations that are non-trailing, unsupported expressions, type/nullability mismatches, combined with `params`, or use a generic function defaulted parameter type that references a type parameter, invalid extension property declarations, nullable receiver extension property declarations and accesses, duplicate exact receiver/name extension properties, extension properties hidden by the implemented ordinary/structural member precedence, extension property helper names that collide with TypeSharp-authored extension methods or generated property helpers in the same extension container, assignments to getter-only extension properties, and null-conditional extension-property read or simple assignment targets before TypeSharp-owned nullable receiver lifting/null-conditional lowering exists, TypeSharp-owned named argument failures such as unknown names, duplicates, positional arguments after named arguments, missing required parameters, type mismatches, inconsistent generic inference, or `params` combinations, direct generic TypeSharp-declared calls whose explicit generic arity, simple inference, bounded constructed inference, or `params` tail inference is inconsistent, direct named-function composition pairs whose known unary signatures are incompatible, direct generic TypeSharp composition pairs whose bounded edge inference produces incompatible signatures, explicit function-type annotations on direct composition values whose input or result is incompatible with the composed signature, public/exported direct composition values that omit an explicit function type annotation, direct pipeline targets whose known first parameter cannot accept the pipeline input, direct pipeline targets whose known arity, optional/default suffix, final `params` tail, named non-piped arguments, or non-piped call arguments cannot lower to a valid first-argument call, direct generic TypeSharp pipeline targets whose bounded inference is inconsistent, generic named `params` pipeline targets, non-boolean `match` arm guards, literal patterns that are incompatible with the matched input, unrelated enum value use, and missing enum members. `TS2203` treats guarded nominal, TypeSharp-owned enum, imported C# enum, `bool`, and type-level union arms as non-covering unless a later unguarded arm or `_` discard covers the remaining closed set.

No reserved examples are currently listed. Future diagnostics should be added to this table only when implemented.

## Planned Advanced Type Operator Diagnostics

Mapped, conditional, template-literal, and utility type computation is a documented design boundary, not implemented behavior. Future evaluator diagnostics belong in the `TS2200`-`TS2399` type-checking range, but new descriptor codes should be allocated only when the parser/checker implementation lands.

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

The descriptor registry keeps diagnostic code, severity, category, explanation, and suggested action stable for tools. Current project diagnostics include manifest/source discovery/module graph errors like missing project-reference manifests with `TS0100`, invalid source aliases, project-reference cycles, duplicate direct project names, and incompatible project-reference target frameworks with `TS0103`, duplicate source module paths with `TS0111`, unresolved source modules, alias targets, or direct project source module specifiers with `TS0112`, unsupported or ambiguous source module import forms with `TS0113`, and missing source module import, re-export, or direct project referenced export targets with `TS0114`; current binding diagnostics include unresolved names like `TS2001`, duplicate symbols like `TS2002`, including same-file import alias conflicts and duplicate enum members, unsupported export forwarding like `TS2003`, and duplicate local exports like `TS2004`; current type-checking diagnostics include type mismatches like `TS2201`, including immutable local assignment targets, invalid local assignment targets, simple assignment mismatches, unsupported enum underlying types, enum member numeric range/integrality failures, invalid enum member aliases or composite initializer operands, enum value `|`/`&`/`^`/`~` operand failures, local bitwise compound assignment operand failures, null-conditional imported member bitwise compound target/value failures, integral bitwise operand failures, integral shift and logical unsigned shift operand/count failures, logical unsigned shift assignment operand/target failures for mutable locals plus unsupported imported C# `>>>=` targets, boolean bitwise operand failures, direct TypeSharp function call arity/argument failures including optional/default suffixes and TypeSharp-owned named arguments, invalid defaulted parameter declarations, extension property declaration, nullable receiver declaration/access, duplicate/conflict, helper-name collision, getter-only assignment, and null-conditional target failures, direct generic TypeSharp function call arity/simple/constructed inference failures including supported named argument binding, direct named-function composition compatibility failures including bounded generic unary target failures, explicit composition function-type annotation failures, public unannotated direct-composition value failures, direct pipeline input/arity/call-argument compatibility failures including optional/default suffixes and named non-piped arguments, direct generic TypeSharp pipeline inference failures including supported named non-piped arguments, unrelated enum value use, and missing enum members, nullability assignment and return violations like `TS2202`, public-boundary leaks like `TS2204`, C# 7.3 backend constraint limits like `TS2205`, dynamic annotation boundary errors like `TS2206`, dynamic call propagation errors like `TS2207`, reflect/interop/unsafe capability call marker errors like `TS2208`, and unknown member/indexer access errors like `TS2209`; current interop diagnostics include missing references like `TS2401`, ambiguous C# overloads, constructors, or imported indexer candidates like `TS2402`, no matching C# overloads or constructors like `TS2406`, missing C# methods like `TS2407`, missing C# types like `TS2408`, missing C# static members like `TS2409`, missing C# instance members like `TS2410`, missing or mismatched C# instance indexers like `TS2411`, missing C# instance property setters like `TS2412`, readonly C# instance field assignment errors like `TS2413`, missing C# static property setter errors like `TS2414`, readonly C# static field assignment errors like `TS2415`, missing C# instance event errors like `TS2416`, and unsatisfied explicit or inferred C# generic constraint errors like `TS2417`.

The compiler exposes descriptor metadata through `TypeSharp.Compiler.Diagnostics.DiagnosticDescriptors`. `typesharp explain <code>` reads that registry directly. Unknown descriptor codes return exit code `1`; CLI usage errors return exit code `2`.

## Related Pages

- [CLI](../cli/)
- [Troubleshooting](../troubleshooting/)
- [VS Code And LSP](../vscode-lsp/)
