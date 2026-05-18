# Task Rollup: Public API Declaration Backend Smokes

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:24:29 +09:00
End Time: 2026-05-19 02:51:06 +09:00

## Compressed Tasks

- `0079-core-option-result-generic-api-smoke.md`
- `0080-generic-function-backend-smoke.md`
- `0081-class-declaration-backend-smoke.md`
- `0082-interface-declaration-backend-smoke.md`
- `0083-generic-type-declaration-backend-smoke.md`

## Objective

Generated C# `net48` backend public API coverage를 `Option<T>`/`Result<T,E>` generic signatures, generic functions, class/interface declarations, and generic type declarations까지 확장하고 C# `net48` consumers가 해당 metadata를 호출하거나 구현할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/types.md](../grammar/types.md)
- [../grammar/coverage.md](../grammar/coverage.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Parsing`
- `src/TypeSharp.Compiler/Binding/TypeSharpBinder.cs`
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `src/TypeSharp.Core`
- `tests/fixtures/parser`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

Completed:
- generic type annotation emission in generated C# signatures.
- simple type checker representation for comparable generic type names.
- `TypeSharp.Core.Option<T>` and `Result<T,E>` pass-through generated public APIs.
- function type parameter binding for signatures and bodies.
- generated C# generic method signature emission.
- top-level class emission and class `fun` member emission as instance methods.
- `interface` keyword, `InterfaceDeclaration` parser support, and body-less interface function signatures.
- generated C# 7.3-compatible interface emission.
- type declaration type parameter binding for nested member signatures.
- generated C# generic class signature emission.
- parser fixture `0012-interface-declaration`.
- backend fixtures `0010-core-option-result-api` through `0014-generic-type-declaration-api`.
- CLI build and C# `net48` consumer smokes for Core generic APIs, generic functions, class APIs, interface implementation, and generic class APIs.
- checklist completion for `Option<T>`, `Result<T,E>`, `class/interface declaration`, and `generic type/function`.
- traceability entries for Core generic APIs, generic functions, class APIs, interface APIs, and generic type declarations.

Out:
- generic constraints and `where` clauses.
- generic call expression type argument syntax.
- TypeSharp semantic model type argument inference.
- primary constructor lowering.
- fields, properties, events, and accessor lowering.
- interface inheritance/properties/events/static abstract members.
- generic record/union lowering.
- pattern matching over `Option<T>`/`Result<T,E>`.
- nullability conversion helpers.

## Verification

Commands run across the compressed packet set:

```text
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Result:
- PASS `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`.
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`, including parser/backend fixture snapshots, `CLI build compiles core option result APIs`, `CLI build compiles generic function API`, `CLI build compiles class declaration API`, `CLI build compiles interface declaration API`, and `CLI build compiles generic type declaration API`.
- PASS `git diff --check`.

## Handoff

Done:
- Generated public API metadata now covers basic generic type annotations, generic methods, generated classes, generated interfaces, and generated generic classes.
- C# `net48` consumer compatibility is covered for calling generated APIs and implementing generated interfaces.

Remaining:
- Implement immutable record lowering.
- Implement nominal closed union lowering and pattern matching.
- Implement type-level union alias diagnostics and narrowing.
- Implement null safety diagnostics beyond imported C# unknown nullability.
- Implement structural type checking and public boundary diagnostics.
- Implement async `Task`/`Task<T>` lowering.

Blocked:
- None.
