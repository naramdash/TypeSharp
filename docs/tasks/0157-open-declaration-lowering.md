# Task 0157: Open Declaration Lowering

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 16:59:06 +09:00
End Time: 2026-05-19 17:05:21 +09:00

## Objective

Implement the first F#-style `open` declaration slice so explicit namespace opens in TypeSharp source participate in generated C# imports instead of remaining documentation-only grammar.

## Scope

In:
- Add `open` as a lexer keyword.
- Parse root-level `open Qualified.Name` declarations.
- Emit root-level `open` declarations as deterministic C# `using` directives.
- Deduplicate `open` and import-derived `using` directives.
- Keep runtime helper import insertion ordered after namespace/import/open headers.
- Add parser fixture and CLI/backend smoke coverage.
- Update module grammar, feature specs, checklist, traceability, docs-site reference, and task index docs.
- Commit and push when this task is completed.

Out:
- Module-local `open` semantics.
- Ambiguity diagnostics for `open` name pollution.
- Import/export graph resolution across source files.
- `open type` or extension-method instance lookup.

## Acceptance Criteria

- [x] Lexer recognizes `open` as a stable keyword.
- [x] Parser emits an `OpenDeclaration` for `open Qualified.Name`.
- [x] Parser fixture coverage pins the open declaration syntax tree.
- [x] C# backend emits root-level `open` as `using Qualified.Name;`.
- [x] Duplicate `open` and import namespace usings are emitted only once.
- [x] Runtime helper import lowering keeps generated imports ordered around open declarations.
- [x] Docs explain the implemented root-level open slice and remaining open-resolution work.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "open declaration"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:59:06 +09:00: Started after auditing `docs/goal.md`; selected the explicit module graph/open slice because docs specify F#-style `open` but the compiler did not yet parse or lower open declarations.
- 2026-05-19 17:05:21 +09:00: Added `open` keyword parsing, `OpenDeclaration` syntax, C# `using` lowering with import deduplication, runtime helper header ordering, parser/backend fixtures, CLI smoke coverage, and docs evidence.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "open declaration"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`: passed.
- `npm run build` in `docs-site`: passed; existing Astro warning `Entry docs -> 404 was not found` was printed.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.

## Follow-up

- Module-local `open`, ambiguity diagnostics, full source module graph resolution, and `open type`/extension-method lookup remain future work.
