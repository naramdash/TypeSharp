# Task 0169: Local Export List Public Surface

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 22:32:23 +09:00
End Time: 2026-05-19 22:36:47 +09:00

## Objective

Support same-file `export { Name }` and `export type { TypeName }` lists as public surface markers while keeping re-export and renamed export forms blocked until cross-file export lowering exists.

## Scope

In:
- Allow unaliased local `export { Name }` and `export type { TypeName }` through binder/check/build.
- Resolve local export list entries and report `TS2001` for missing values/types.
- Treat local export-listed declarations as public boundary declarations for type checking.
- Emit local export-listed functions and types as generated C# `public`.
- Keep re-export forms and renamed export specifiers blocked by `TS2003`.
- Add smoke tests and update module/docs/checklist/traceability.
- Commit and push when this task is completed.

Out:
- Cross-file re-export lowering.
- Export alias lowering.
- Duplicate export diagnostics.
- Cross-file public surface filtering.

## Acceptance Criteria

- [x] `export { visible }` can mark a same-file function as generated C# public.
- [x] `export type { Customer }` can mark a same-file type as generated C# public.
- [x] Non-exported same-file declarations remain internal by default.
- [x] Missing local export list entries report `TS2001`.
- [x] Re-export and renamed export specifiers still report `TS2003`.
- [x] Documentation describes local export list support and remaining unsupported forms.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "local export"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported export forwarding"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 22:32:23 +09:00: Started after relative source imports left export lists as the next module public-surface gap.
- 2026-05-19 22:36:47 +09:00: Added binder validation for local export lists, backend/type-checker public-surface recognition, tests, and docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "local export"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported export forwarding"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry"`: passed.
- `npm run build` from `docs-site`: passed and generated 24 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
