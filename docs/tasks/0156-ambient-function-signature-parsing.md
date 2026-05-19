# Task 0156: Ambient Function Signature Parsing

Status: Done
Queue: Q2-Q4
Start Time: 2026-05-19 16:52:02 +09:00
End Time: 2026-05-19 16:56:56 +09:00

## Objective

Move the explicit ambient/global-scope isolation goal from documentation-only grammar into compiler behavior by parsing ambient function signatures and keeping them out of generated C# output.

## Scope

In:
- Add `ambient` as a lexer keyword and declaration modifier.
- Parse `ambient` function signatures without requiring a function body.
- Preserve ambient declarations in binding/type-checking so signatures can be referenced by later semantic work.
- Skip ambient declarations during C# source emission.
- Ensure ambient declarations cannot satisfy generated executable `main`.
- Add parser fixture coverage and CLI build smoke coverage.
- Update module grammar, feature specs, checklist, traceability, docs-site reference, and task index docs.
- Commit and push when this task is completed.

Out:
- Full ambient namespace/type/value surface.
- Manifest-level ambient file partitioning.
- Source-module import/export resolution for ambient declarations.
- Runtime validation against external host APIs.

## Acceptance Criteria

- [x] Lexer recognizes `ambient` as a stable keyword.
- [x] Parser emits an `AmbientModifier` on ambient function declarations.
- [x] Ambient function signatures do not produce missing-function-body diagnostics.
- [x] C# backend omits ambient declarations.
- [x] Executable `main` lookup ignores ambient function signatures.
- [x] Parser fixture coverage pins the ambient function signature syntax tree.
- [x] CLI build smoke verifies unused ambient signatures do not generate C# members.
- [x] Docs explain the implemented ambient function-signature slice and remaining ambient work.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "ambient"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 16:52:02 +09:00: Started after auditing `docs/goal.md`; selected the explicit ambient/global isolation slice because grammar documented `ambient` but the parser/backend did not yet implement an ambient declaration modifier.
- 2026-05-19 16:56:56 +09:00: Added `ambient` keyword/modifier parsing, allowed ambient function signatures without bodies, skipped ambient declarations in C# emission, ignored ambient executable mains, and updated parser/build smokes plus docs.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "ambient"`: passed.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "parser fixture snapshots match"`: passed.
- `npm run build`: passed.
- `git diff --check`: passed with expected line-ending normalization warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed; no tracked generated binaries were listed.

## Follow-Up

- Full ambient namespace/type/value declarations, manifest-level ambient file partitioning, source-module import/export resolution, and runtime validation against external host APIs remain future work.
