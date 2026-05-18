# Task: Null Safety Diagnostics

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:07:31 +09:00
End Time: 2026-05-19 04:13:22 +09:00

## Objective

TypeSharp should report dedicated nullability contract diagnostics when `null` or nullable values flow into non-null TypeSharp positions.

## Source Of Truth

- [../goal.md](../goal.md)
- [../diagnostics.md](../diagnostics.md)
- [../grammar/types.md](../grammar/types.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- allocate and implement `TS2202` nullability contract violation.
- report `TS2202` for `null` assigned or returned to non-null annotated positions.
- report `TS2202` for nullable `T?` values assigned or returned to non-null `T` positions.
- type checker fixtures and CLI JSON/build no-emission smoke coverage.

Out:
- flow-sensitive definite assignment/null analysis.
- member access null dereference diagnostics.
- nullable metadata emit for generated C#.
- nullable C# interop beyond the existing `TS2404` unknown nullability warning.

## Acceptance Criteria

- [x] `DiagnosticDescriptors.All` includes `TS2202`.
- [x] Null literal assignment to non-null type reports `TS2202`.
- [x] Nullable value assignment to non-null type reports `TS2202`.
- [x] Returning `null` or nullable value from a non-null function reports `TS2202`.
- [x] `typesharp check` reports `TS2202` in JSON output.
- [x] `typesharp build` stops before generated emission on `TS2202`.
- [x] Checklist and traceability record null safety progress.

## Verification

Command:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- dedicated null safety diagnostics pass through type checker fixtures and CLI paths.
- existing type mismatch diagnostics remain `TS2201`.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added `TS2202` descriptor and descriptor registry coverage.
- Split nullability contract violations from generic `TS2201` type mismatches in return and local assignment checks.
- Added null literal and nullable value diagnostics for non-null local/return positions.
- Added type checker fixtures and CLI JSON/build no-emission smokes for `TS2202`.
- Marked null safety complete in checklist and added traceability evidence.

Remaining:
- Flow-sensitive null analysis, member access null dereference diagnostics, and nullable metadata emit remain separate follow-up work.

Blocked:
- None.
