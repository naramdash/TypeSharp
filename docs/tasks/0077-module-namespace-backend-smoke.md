# Task: Module Namespace Backend Smoke

Status: Done
Queue: Q2
Start Time: 2026-05-19 02:13:55 +09:00
End Time: 2026-05-19 02:18:20 +09:00

## Objective

문서화된 `module Name { ... }` 선언을 lexer/parser/binder/type checker/C# backend의 최소 경로에 추가하고, namespace 안의 module이 generated `net48` assembly에서 C# static class로 노출되는지 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/modules.md](../grammar/modules.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Parsing`
- `src/TypeSharp.Compiler/Binding`
- `src/TypeSharp.Compiler/TypeChecking`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `module` keyword and `ModuleDeclaration` parser support.
- module member binding/type checking for existing function/literal/value declaration subset.
- generated C# static class emission for module declarations inside file-scoped namespace.
- backend golden fixture and generated `net48` build smoke.
- C# `net48` consumer smoke for public module function.
- checklist, traceability, and task queue updates.

Out:
- nested module declarations.
- module path resolution across files.
- `open` declarations.
- module alias imports.
- member lookup from TypeSharp source as `ModuleName.member`.
- module-level class/interface/record lowering beyond existing parser skeletons.

## Acceptance Criteria

- [x] lexer recognizes `module`.
- [x] parser produces `ModuleDeclaration`.
- [x] binder/type checker traverse module members.
- [x] backend emits module as C# static class in the file namespace.
- [x] CLI build smoke compiles generated `net48` assembly with module members.
- [x] C# `net48` consumer smoke calls a generated public module method.
- [x] checklist and traceability reflect module/namespace implementation status.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- test project builds.
- backend fixture snapshots pass.
- `CLI build compiles module namespace` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including `parser parses module declaration without diagnostics`, backend fixture snapshots, and `CLI build compiles module namespace`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added `module` keyword and `ModuleDeclaration` parser support.
- Added binder/type checker traversal for module members.
- Added generated C# static class emission for module declarations.
- Added backend fixture `0009-module-namespace`.
- Added generated `net48` build and C# `net48` consumer smoke for public module members.
- Marked `module/namespace` complete in the MVP checklist.

Remaining:
- Nested module declarations.
- Module path resolution across files.
- `open` declarations.
- TypeSharp source member lookup as `ModuleName.member`.
- Module-level class/interface/record lowering beyond current parser skeletons.

Blocked:
- None.
