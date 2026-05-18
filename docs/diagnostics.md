# Diagnostics

문서 기준일: 2026-05-19

이 문서는 TypeSharp diagnostic code taxonomy, descriptor metadata, explanation surface, and golden diagnostic fixture policy를 고정한다. CLI, VS Code language server, test fixtures는 같은 diagnostic descriptor registry를 공유해야 한다.

## Code Ranges

Diagnostic code는 `TS` + 4자리 숫자다. 한 번 공개된 code는 같은 의미를 유지한다.

| Range | Category | Purpose |
| --- | --- | --- |
| `TS0001`-`TS0099` | Tooling/Internal | CLI usage, tool orchestration, compiler internal error reporting |
| `TS0100`-`TS0999` | Project | `TypeSharp.toml`, source discovery, project/reference configuration |
| `TS1000`-`TS1999` | Parser | lexer, parser, syntax recovery, unsupported stable grammar tokens |
| `TS2000`-`TS2199` | Binding | scope, declaration binding, imports, duplicate symbols, unresolved names |
| `TS2200`-`TS2399` | Type Checking | type inference, nullability, generic constraints, public type surface checks |
| `TS2400`-`TS2599` | Interop | C# metadata, overload resolution, byref/params/delegate/event interop, public ABI metadata checks |
| `TS3000`-`TS3499` | Lowering | IR/lowering failures, feature lowering restrictions, source mapping |
| `TS3500`-`TS3999` | Backend | C# source backend, future IL backend, emitted artifact validation |

Current allocated descriptors:

| Code | Category | Severity | Title |
| --- | --- | --- | --- |
| `TS0100` | Project | Error | TypeSharp manifest not found |
| `TS0101` | Project | Error | TypeSharp manifest cannot be read |
| `TS0102` | Project | Error | Invalid manifest syntax |
| `TS0103` | Project | Error | Invalid manifest value |
| `TS0110` | Project | Warning | Source root not found |
| `TS1000` | Parser | Error | Unexpected character |
| `TS1001` | Parser | Error | Missing function body |
| `TS1002` | Parser | Error | Unterminated string literal |
| `TS1003` | Parser | Error | Missing expression |
| `TS1004` | Parser | Error | Unexpected token |
| `TS2001` | Binding | Error | Unresolved name |
| `TS2201` | Type Checking | Error | Type mismatch |
| `TS2204` | Type Checking | Error | Type-level union or structural shape leaked through public boundary |
| `TS2401` | Interop | Error | Missing referenced assembly or namespace |
| `TS2402` | Interop | Error | Ambiguous C# overload |
| `TS2403` | Interop | Error | Invalid byref interop use |
| `TS2404` | Interop | Warning | Unknown C# nullability |
| `TS3500` | Backend | Error | Unsupported executable entry point |
| `TS3501` | Backend | Error | Generated C# project build failed |

Reserved semantic examples:

| Code | Intended use |
| --- | --- |
| `TS2002` | Duplicate declaration in the same scope |
| `TS2202` | Nullability contract violation |
| `TS2203` | Non-exhaustive match |

Reserved examples are not implemented diagnostics yet. They reserve stable code positions for upcoming binder, type checker, and interop fixtures.

## Descriptor Metadata

Each diagnostic descriptor must provide:

- `Code`: stable `TS####` identifier.
- `Title`: short user-facing name for `typesharp explain`.
- `DefaultSeverity`: `error`, `warning`, or `info`.
- `Category`: one code range category.
- `MessageTemplate`: short diagnostic message shape.
- `Explanation`: why the compiler reports this diagnostic.
- `SuggestedAction`: the next action a user should take.

Rules:

- Call sites may add source-specific detail to the emitted message, but the descriptor code and severity remain stable.
- CLI text and JSON diagnostics print the emitted message, source file, and 1-based source span.
- VS Code diagnostics use the same code and source span as `typesharp check`.
- `typesharp explain <code>` will read descriptor metadata, not parse emitted diagnostic text.

## Golden Diagnostic Policy

Parser fixtures keep `expected.diagnostics.json` next to `expected.tree` because recovery shape and diagnostics are tightly coupled.

Semantic diagnostics use separate golden fixtures once binder/type checker exists:

```text
tests/fixtures/diagnostics/
  binder/
    positive/
      basic-name-resolution/
        input.tysh
        expected.diagnostics.json
    negative/
      unresolved-name/
        input.tysh
        expected.diagnostics.json
  type-checker/
    positive/
      basic-annotations/
        input.tysh
        expected.diagnostics.json
    negative/
      type-mismatch/
        input.tysh
        expected.diagnostics.json
  interop/
    public-boundary-union/
      input.tysh
      expected.diagnostics.json
```

Golden diagnostics must assert:

- stable `code`
- severity
- user-facing message
- file path
- start/end source span
- any related information once the diagnostic model grows it

Descriptor registry smoke tests must also assert that allocated codes are unique and that title, message template, explanation, and suggested action are present.

## Current Implementation

The compiler exposes descriptor metadata through `TypeSharp.Compiler.Diagnostics.DiagnosticDescriptors`. Current tests pin the allocated descriptor code list and require explanation metadata for every descriptor.
