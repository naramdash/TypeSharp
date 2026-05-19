# Task 0164: Source Module Path Identity

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:49:38 +09:00
End Time: 2026-05-19 17:51:19 +09:00

## Objective

Move TypeSharp's module graph foundation forward by giving every discovered `.tysh` source file a source-root-relative module path and rejecting duplicate module paths before check/build can continue.

## Scope

In:
- Preserve project-relative path, source root, source-root-relative path, and module path on `SourceFile`.
- Derive module path by removing the `.tysh` extension from the source-root-relative path.
- Add a stable project diagnostic for duplicate source module paths.
- Verify source discovery, CLI JSON diagnostics, and build no-emission behavior.
- Update CLI/module grammar/docs-site/checklist/traceability documentation.
- Commit and push when this task is completed.

Out:
- Import specifier to source module resolution.
- Export binding or public surface filtering.
- Project-wide source symbol graph.
- Cross-file type checking.

## Acceptance Criteria

- [x] Source discovery returns `SourceRootRelativePath` and `ModulePath` for discovered files.
- [x] Duplicate source-root-relative module paths report `TS0111`.
- [x] `typesharp check --diagnostic-format json` reports `TS0111` for duplicate source modules.
- [x] `typesharp build` stops before generated C# source/project/assembly emission when `TS0111` is present.
- [x] Documentation explains the source module path identity rule and duplicate diagnostic.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source discovery"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "duplicate source module"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:49:38 +09:00: Started from `docs/goal.md` module graph requirements after parser/import/export safety slices left source files without an explicit source-root-relative module identity.
- 2026-05-19 17:51:19 +09:00: Added source-root-relative module path data to `SourceFile`, `TS0111` duplicate source module diagnostics, CLI JSON/build no-emission smokes, and docs for the source module identity rule.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source discovery"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "duplicate source module"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` in `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
