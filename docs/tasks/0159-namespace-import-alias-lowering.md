# Task 0159: Namespace Import Alias Lowering

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 17:15:17 +09:00
End Time: 2026-05-19 17:19:54 +09:00

## Objective

Move the documented TypeScript-style namespace import form from grammar-only status into compiler behavior by parsing `import * as Alias from "Namespace"` and lowering it to a C# namespace alias directive.

## Scope

In:
- Parse `import * as Alias from "Namespace"` as a dedicated namespace import declaration.
- Bind the namespace import alias as a local value/type import symbol for current single-file analysis.
- Emit namespace imports as deterministic C# `using Alias = Namespace;` directives.
- Keep named, type, static, open, and named alias import behavior stable.
- Add parser fixture, backend fixture, CLI build smoke, and docs evidence.
- Commit and push when this task is completed.

Out:
- Relative source module namespace import resolution.
- Namespace member completion or project-wide symbol resolution.
- Export star or export alias declarations.
- Duplicate/conflicting import alias diagnostics.

## Acceptance Criteria

- [x] Parser emits an `ImportNamespaceDeclaration` for `import * as Alias from "Namespace"`.
- [x] Binder/type checker register `Alias` as the local import symbol.
- [x] C# backend emits `using Alias = Namespace;` for namespace imports.
- [x] Existing named/type/static/open import behavior remains unchanged.
- [x] Parser/backend/CLI smoke coverage pins the namespace import behavior.
- [x] Docs explain the implemented namespace import alias slice and remaining module-resolution work.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "namespace import"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:15:17 +09:00: Started after completing task 0158; selected namespace import alias lowering because `docs/grammar/modules.md` and `docs/grammar/ambiguity.md` already specify `import * as Alias`, but the parser still recovered it as a skipped named import.
- 2026-05-19 17:19:54 +09:00: Added `ImportNamespaceDeclaration` parsing, binder/type-checker alias symbol handling, C# namespace alias using lowering, runtime import header ordering, parser/backend fixtures, CLI smoke coverage, and docs evidence.

## Verification

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "namespace import"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture snapshots match"`: passed.
- `npm run build` in `docs-site`: passed; existing Astro warning `Entry docs -> 404 was not found` was printed.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.

## Follow-up

- Duplicate/conflicting import alias diagnostics were covered by [0161-import-alias-conflict-diagnostics.md](0161-import-alias-conflict-diagnostics.md). Relative source module namespace imports, namespace member completion, project-wide symbol resolution, and export star/alias declarations remain future work.
