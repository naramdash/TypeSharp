# Task 0161: Import Alias Conflict Diagnostics

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:28:21 +09:00
End Time: 2026-05-19 17:31:50 +09:00

## Objective

Make duplicate or conflicting import alias handling an explicit compiler contract by pinning `TS2002` diagnostics for import names that collide with the same file scope.

## Scope

In:
- Verify named import aliases collide with existing declarations through `TS2002`.
- Verify namespace import aliases collide with other import aliases through `TS2002`.
- Add CLI JSON smoke coverage for import alias conflicts.
- Update docs/checklist/traceability/task follow-ups so this is no longer undocumented future work.
- Commit and push when this task is completed.

Out:
- Cross-file/project-wide import conflict analysis.
- Relative source module resolution.
- Export specifier alias diagnostics.
- New diagnostic code allocation beyond the existing duplicate-symbol `TS2002` contract.

## Acceptance Criteria

- [x] TypeSharp checker reports `TS2002` when a named import alias conflicts with an existing declaration in the same file scope.
- [x] TypeSharp checker reports `TS2002` when namespace import aliases reuse the same local alias.
- [x] `typesharp check --diagnostic-format json` surfaces the import alias conflict diagnostic.
- [x] Docs identify import alias conflicts as covered by duplicate symbol diagnostics while keeping cross-file/source-module conflicts out of scope.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "import alias conflict"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "duplicate symbol"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:28:21 +09:00: Started after tasks 0158 and 0159 left duplicate/conflicting import alias diagnostics as future work, even though import aliases now participate in the file-scope binder symbol table.
- 2026-05-19 17:31:50 +09:00: Added checker and CLI JSON smokes for named/namespace import alias conflicts, updated diagnostics/grammar/checklist/traceability/docs-site references, and reclassified duplicate import alias diagnostics as covered by the same-file `TS2002` contract.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "import alias conflict"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "duplicate symbol"`: passed.
- `npm run build` in `docs-site`: passed.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
