# Task 0126: Binder Duplicate Symbol Diagnostics

Status: Done
Queue: Q2
Start Time: 2026-05-19 12:00:00 +09:00
End Time: 2026-05-19 12:04:00 +09:00

## Objective

Close the binder correctness gap for duplicate declarations by reporting same-scope value/type symbol collisions through a stable diagnostic shared by CLI, tests, and LSP.

## Scope

In:
- `TS2002` descriptor metadata.
- Same-scope duplicate detection for value and type namespaces.
- Duplicate top-level value/function declarations.
- Duplicate top-level type declarations.
- Duplicate function parameters, type parameters, lambda parameters, locals, and pattern bindings in the same scope.
- Binder golden fixture and CLI JSON smoke coverage.

Out:
- Cross-file project-wide duplicate detection.
- Metadata/source unified symbol graph.
- Overload-set binding rules.
- Flow-sensitive shadowing diagnostics.

## Acceptance Criteria

- [x] Duplicate symbols in the same scope report `TS2002`.
- [x] Nested scopes may still shadow parent-scope declarations.
- [x] Binder fixture snapshots pin duplicate symbol JSON diagnostics.
- [x] CLI `check --diagnostic-format json` reports duplicate symbols.
- [x] Diagnostic descriptor registry and docs include `TS2002`.
- [x] Checklist and traceability mention duplicate symbol evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "duplicate symbol"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "binder fixture"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS duplicate symbol focused smoke.
- PASS binder fixture snapshots.
- PASS full compiler test suite.

## Handoff

Done:
- Added `TS2002` duplicate symbol descriptor.
- Updated binder symbol declaration paths to check same-scope duplicates before adding symbols.
- Added a binder negative fixture and CLI JSON smoke.
- Updated diagnostics docs, checklist, traceability, and task index.

Remaining:
- Cross-file duplicate detection remains future work with the project-wide symbol graph.

Blocked:
- None.
