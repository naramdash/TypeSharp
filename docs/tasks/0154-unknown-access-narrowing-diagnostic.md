# Task 0154: Unknown Access Narrowing Diagnostic

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 16:34:13 +09:00
End Time: 2026-05-19 16:41:00 +09:00

## Objective

Enforce the safe gradual-typing rule that `unknown` values cannot be used through member or indexer access until code proves a narrower type or structural shape.

## Scope

In:
- Add a stable type-checking diagnostic for member/indexer access on `unknown`.
- Report the diagnostic for direct member access on `unknown`.
- Report the diagnostic when member access on `unknown` is used as a call callee.
- Report the diagnostic for indexer access on `unknown`.
- Keep structural-shape member access valid after a value has a structural type.
- Add type-checker fixture coverage and a CLI JSON smoke.
- Update diagnostics, grammar/types, feature map, checklist, traceability, feature-spec, docs-site, and task index docs.
- Commit and push when this task is completed.

Out:
- Full control-flow narrowing for record patterns over `unknown`.
- User-defined type guard functions.
- `is` expression narrowing.
- Exhaustive structural-union narrowing beyond existing type-level union support.

## Acceptance Criteria

- [x] `DiagnosticDescriptors` includes a stable `TS2209` descriptor for unknown access narrowing.
- [x] The type checker reports `TS2209` for member access on `unknown`.
- [x] The type checker reports `TS2209` when member access on `unknown` is used as a call callee.
- [x] The type checker reports `TS2209` for indexer access on `unknown`.
- [x] Structural-shape member access remains accepted after the value has a structural shape type.
- [x] Golden diagnostics fixture coverage verifies direct member, call-callee, indexer, and allowed structural-shape cases.
- [x] CLI JSON smoke verifies `TS2209` is emitted through `typesharp check`.
- [x] Docs explain that `unknown` must be narrowed before member or indexer access.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unknown access"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:34:13 +09:00: Started task after selecting the `unknown`-centered safe gradual typing goal from `docs/goal.md`.
- 2026-05-19 16:41:00 +09:00: Added `TS2209`, enforced unknown member/indexer access boundaries, added fixture and CLI JSON smoke coverage, and updated diagnostics/grammar/docs-site/checklist/traceability docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unknown access"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "type checker fixture diagnostics match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build`: passed.
- `git diff --check`: passed with expected line-ending normalization warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed; no tracked generated binaries were listed.

## Follow-Up

- Full control-flow narrowing from runtime shape tests, user-defined guards, and `is` expressions remains future work.
