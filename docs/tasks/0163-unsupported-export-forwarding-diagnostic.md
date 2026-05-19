# Task 0163: Unsupported Export Forwarding Diagnostic

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:42:56 +09:00
End Time: 2026-05-19 17:44:52 +09:00

## Objective

Prevent parser-visible export specifier declarations from being silently ignored by `typesharp check` or `typesharp build` before source module graph resolution and C# re-export lowering are implemented.

## Scope

In:
- Add a stable binding diagnostic for unsupported export forwarding.
- Report the diagnostic for `export { ... }`, `export type { ... }`, and `export * from "..."`.
- Verify CLI JSON diagnostics and build no-emission behavior.
- Update diagnostics, grammar, docs-site, checklist, and traceability documentation.
- Commit and push when this task is completed.

Out:
- Source module graph resolution.
- Export binding or duplicate export diagnostics.
- Public surface filtering from export lists.
- C# backend lowering for re-export declarations.

## Acceptance Criteria

- [x] `DiagnosticDescriptors` exposes a stable `TS2003` descriptor with explanation and suggested action.
- [x] Binder reports `TS2003` for named, type-only, and star export specifier declarations.
- [x] `typesharp check --diagnostic-format json` reports `TS2003` for export forwarding syntax.
- [x] `typesharp build` stops before generated C# source/project/assembly emission when `TS2003` is present.
- [x] Docs explain that export specifier syntax is parsed but blocked until source module graph support exists.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported export forwarding"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:42:56 +09:00: Started after export specifier parsing made the syntax parser-visible while source module graph resolution and re-export lowering remained future work.
- 2026-05-19 17:44:52 +09:00: Added `TS2003`, binder reporting for parser-visible export forwarding declarations, CLI JSON/build no-emission smokes, and docs explaining the parser-visible but unsupported contract.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "unsupported export forwarding"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "diagnostic descriptor registry is stable"`: passed.
- `npm run build` in `docs-site`: passed and generated 21 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
