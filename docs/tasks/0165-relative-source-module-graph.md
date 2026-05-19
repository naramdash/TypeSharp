# Task 0165: Relative Source Module Graph

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:56:57 +09:00
End Time: 2026-05-19 17:58:54 +09:00

## Objective

Use the source module path identity from task 0164 to build a first source module graph slice from relative import/export module specifiers, while keeping unsupported cross-file source import binding from silently passing check/build.

## Scope

In:
- Add a source module graph API that records modules and relative source dependency edges.
- Resolve `./` and `../` source module specifiers against the importing file's module path.
- Report `TS0112` for unresolved relative source module specifiers.
- Report `TS0113` for resolved relative source imports until project-wide source import binding/lowering exists.
- Wire graph diagnostics into `typesharp check` and `typesharp build`.
- Verify CLI JSON diagnostics and build no-emission behavior.
- Update CLI/module grammar/docs-site/checklist/traceability documentation.
- Commit and push when this task is completed.

Out:
- Binding imported source declarations.
- Export public surface filtering.
- C# lowering for source imports or re-exports.
- Cross-file type checking.

## Acceptance Criteria

- [x] `SourceModuleGraph` records relative source import dependencies.
- [x] Missing relative source modules report `TS0112`.
- [x] Resolved relative source imports report `TS0113` before check/build can silently succeed.
- [x] `typesharp check --diagnostic-format json` reports both unsupported and unresolved source module diagnostics.
- [x] `typesharp build` stops before generated C# source/project/assembly emission when `TS0113` is present.
- [x] Documentation explains relative source module graph resolution and current unsupported binding/lowering boundary.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module graph"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module import"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unresolved source module"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:56:57 +09:00: Started after task 0164 established source-root-relative module path identity but left relative source module specifiers unresolved.
- 2026-05-19 17:58:54 +09:00: Added `SourceModuleGraph`, relative source dependency records, `TS0112` unresolved source module diagnostics, `TS0113` unsupported source import diagnostics, check/build graph diagnostics, and documentation.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module graph"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "source module import"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unresolved source module"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` in `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
