# Task: Type-Level Union Public Boundary Diagnostic

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:26:32 +09:00
End Time: 2026-05-19 03:33:36 +09:00

## Objective

Type-level union aliases should be usable as compile-time-only local type expressions, but they must not leak through public .NET metadata boundaries.

## Source Of Truth

- [../goal.md](../goal.md)
- [../diagnostics.md](../diagnostics.md)
- [../grammar/types.md](../grammar/types.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TS2204` descriptor implementation
- type checker detection for direct type-level union in exported/public type aliases and function signatures
- type checker detection for aliases that resolve to compile-time-only type-level unions
- CLI `check`/`build` smoke ensuring diagnostics are surfaced and build emission stops

Out:
- full union narrowing
- overload candidate union reasoning
- structural type public boundary diagnostics beyond shared helper shape
- generated wrapper/interface suggestions beyond diagnostic wording

## Acceptance Criteria

- [x] `DiagnosticDescriptors.All` includes `TS2204`.
- [x] Non-public functions can use type-level union aliases without diagnostics.
- [x] Exported/public type alias declarations using `A | B` report `TS2204`.
- [x] Exported/public function parameters or return types using a type-level union alias report `TS2204`.
- [x] `typesharp check` reports `TS2204` in JSON output.
- [x] `typesharp build` stops before generated emission on `TS2204`.
- [x] `docs/checklist.md` and `docs/traceability.md` record the type-level union alias progress.

## Verification

Command:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- Descriptor registry, type checker fixture, CLI check/build, and full smoke suite pass.
- Public ABI leakage of type-level union aliases is reported as `TS2204`.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `TS2204` to `DiagnosticDescriptors.All`.
- Added type checker tracking for compile-time-only type-level union aliases.
- Added public boundary checks for exported/public type aliases, function parameters, function returns, and typed values.
- Added `tests/fixtures/diagnostics/type-checker/negative/public-boundary-union-alias`.
- Added CLI JSON check and build no-emission smokes for `TS2204`.
- Marked `type-level union alias` complete in [../checklist.md](../checklist.md) and added traceability evidence.

Remaining:
- Full union narrowing and pattern matching remain separate follow-up tasks.

Blocked:
- None.
