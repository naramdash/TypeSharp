# Task 0158: Named Import Alias Lowering

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:09:12 +09:00
End Time: 2026-05-19 17:13:31 +09:00

## Objective

Move the documented TypeScript-style named import alias form from grammar-only status into compiler behavior by parsing `import { Name as Alias } from "Namespace"` and lowering it to a C# type alias directive.

## Scope

In:
- Add `as` as a lexer keyword for import specifiers.
- Parse named import specifier aliases without changing existing unaliased import snapshots unnecessarily.
- Bind and type-check the local alias name instead of the imported source name.
- Emit aliased named imports as deterministic C# `using Alias = Namespace.Name;` directives.
- Keep ordinary import namespace `using` emission and duplicate handling stable.
- Add parser fixture, backend fixture, CLI build smoke, and docs evidence.
- Commit and push when this task is completed.

Out:
- Namespace import `import * as Alias`.
- Export specifier aliases.
- Relative source module resolution.
- Duplicate/conflicting import alias diagnostics.

## Acceptance Criteria

- [x] Lexer recognizes `as` as a stable keyword.
- [x] Parser preserves `import { Name as Alias } from "Namespace"` in the syntax tree.
- [x] Binder/type checker register `Alias` as the local import symbol.
- [x] C# backend emits `using Alias = Namespace.Name;` for aliased named imports.
- [x] Existing unaliased import directive behavior remains unchanged.
- [x] Parser/backend/CLI smoke coverage pins the alias behavior.
- [x] Docs explain the implemented named import alias slice and remaining module-resolution work.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "import alias"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:09:12 +09:00: Started after completing task 0157; selected named import alias lowering because `docs/grammar/modules.md` already specifies `import_specifier ::= identifier ("as" identifier)?`, but the compiler still treated all identifiers inside braces as imported names.
- 2026-05-19 17:13:31 +09:00: Added `as` keyword lexing, named import alias parsing, binder/type-checker local alias symbol handling, C# alias using lowering, parser/backend fixtures, CLI smoke coverage, and docs evidence.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "import alias"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`: passed.
- `npm run build` in `docs-site`: passed; existing Astro warning `Entry docs -> 404 was not found` was printed.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.

## Follow-up

- Namespace import aliases, export specifier aliases, relative source module resolution, and duplicate/conflicting import alias diagnostics remain future work.
