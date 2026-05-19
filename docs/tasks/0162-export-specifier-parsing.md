# Task 0162: Export Specifier Parsing

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:33:56 +09:00
End Time: 2026-05-19 17:37:42 +09:00

## Objective

Move documented export specifier forms from grammar-only status into the parser by preserving `export { ... }`, `export type { ... }`, and `export * from "..."` syntax instead of recovering them as skipped tokens.

## Scope

In:
- Add syntax kinds for named, type-only, and star export declarations.
- Parse export specifier aliases with optional `as`.
- Parse optional `from "module"` specifiers for named/type-only export declarations.
- Parse `export * from "module"`.
- Add parser fixture and documentation evidence.
- Keep exported declaration modifier behavior unchanged.
- Commit and push when this task is completed.

Out:
- Source module graph resolution.
- Export binding, duplicate export diagnostics, or public surface filtering from export lists.
- C# backend lowering for re-export declarations.
- `export default` or JavaScript runtime export behavior.

## Acceptance Criteria

- [x] Parser emits `ExportNamedDeclaration` for `export { Name as Alias } from "Module"`.
- [x] Parser emits `ExportTypeDeclaration` for `export type { Name } from "Module"`.
- [x] Parser emits `ExportStarDeclaration` for `export * from "Module"`.
- [x] Existing `export fun`/`export type Alias = ...` declaration modifier parsing remains unchanged.
- [x] Parser fixture snapshots pin the export specifier forms.
- [x] Docs explain the parser-only export specifier slice and remaining module graph work.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:33:56 +09:00: Started after import/open work left export specifier aliases and re-export syntax as documented grammar without parser coverage.
- 2026-05-19 17:37:42 +09:00: Added parser syntax kinds and parsing for named/type/star export specifier forms, added parser fixture `0018-export-specifier-declarations`, and updated grammar/checklist/traceability/docs-site references while leaving source module graph resolution and C# re-export lowering out of scope.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`: passed.
- `npm run build` in `docs-site`: passed.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
