# Task: Interface Declaration Backend Smoke

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:42:27 +09:00
End Time: 2026-05-19 02:47:08 +09:00

## Objective

TypeSharp `interface` declaration과 body 없는 `fun` signature를 parser/backend에 추가하고 generated C# `net48` interface를 C# consumer가 구현할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/coverage.md](../grammar/coverage.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Parsing`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/parser`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `interface` keyword and `InterfaceDeclaration` syntax node.
- parser support for interface declarations with function signatures.
- generated C# interface emission for function signatures.
- parser and backend fixtures.
- CLI build and C# `net48` consumer smoke implementing the generated interface.

Out:
- interface properties/events.
- inheritance/base clauses.
- static abstract members.
- explicit interface implementation from TypeSharp classes.
- interface member semantic model beyond existing parser/binder coverage.

## Acceptance Criteria

- [x] lexer recognizes `interface`.
- [x] parser emits `InterfaceDeclaration` with body-less function signatures.
- [x] backend emits a C# 7.3-compatible interface.
- [x] parser and backend fixtures cover a simple interface method.
- [x] CLI build smoke compiles generated `net48` assembly with a public interface.
- [x] C# `net48` consumer smoke implements the generated interface.
- [x] checklist and traceability reflect minimal class/interface declaration coverage.
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
- parser and backend fixture snapshots pass.
- `CLI build compiles interface declaration API` passes.
- whitespace check has no errors.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including parser/backend fixture snapshots and `CLI build compiles interface declaration API`.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added `interface` keyword and `InterfaceDeclaration` parser support.
- Added body-less function signature parsing for interface members.
- Added generated C# interface emission.
- Added parser fixture `0012-interface-declaration`.
- Added backend fixture `0013-interface-declaration-api`.
- Added CLI build and C# `net48` consumer implementation smoke for generated interface public API.
- Marked minimal class/interface declaration coverage complete in checklist and traceability.

Remaining:
- Interface properties/events.
- Interface inheritance/base clauses.
- Class/interface semantic model integration.
- Explicit TypeSharp class implementation syntax/lowering.

Blocked:
- None.
