# Task: Type-Level Union Narrowing

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 03:52:29 +09:00
End Time: 2026-05-19 04:01:14 +09:00

## Objective

TypeScript-style local type-level union aliases should be usable for compile-time narrowing through type-pattern `match` arms without leaking into public .NET metadata.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/types.md](../grammar/types.md)
- [../grammar/patterns.md](../grammar/patterns.md)
- [../feasibility.md](../feasibility.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- local type-level union aliases whose members are nominal/primitive type names.
- `match value { name: Type => ... }` narrowing over known type-level union aliases.
- non-exhaustive type-level union match diagnostics using `TS2203`.
- generated C# 7.3 `is Type name` checks for local type-level union match lowering.
- CLI check/build smoke coverage with generated `net48` assembly.

Out:
- literal string/number type syntax, because the parser does not yet parse literal types in type positions.
- structural shape union narrowing.
- `is` expression syntax narrowing.
- public ABI exposure of type-level unions; existing `TS2204` remains the boundary.
- full decision-tree lowering for nested patterns and guards.

## Acceptance Criteria

- [x] Type checker records type-level union alias members for supported local aliases.
- [x] Type-pattern match arms bind narrowed variables with the selected member type.
- [x] Missing type-level union members report `TS2203`.
- [x] Generated C# maps local type-level union alias parameters to CLR-compatible `object` and lowers type-pattern arms to `is` checks.
- [x] `typesharp build` emits a `net48` assembly for internal type-level union match code reachable through public wrapper functions.
- [x] Public boundary diagnostics for exported type-level union aliases still stop emission.
- [x] Checklist and traceability record union narrowing progress.

## Verification

Command:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- type-level union narrowing diagnostics and backend fixtures pass.
- generated C# stays `net48` compatible.
- existing `TS2204` public boundary behavior remains intact.

Result:
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Added type checker tracking for supported local type-level union alias members.
- Added type-pattern arm scope narrowing and `TS2203` diagnostics for missing type-level union members.
- Added generated C# erasure of local type-level union aliases to `object` plus `is Type variable` match lowering.
- Added type checker positive/negative fixtures, backend snapshot, and CLI `net48` build/C# consumer smoke.
- Marked `union narrowing` complete in checklist and added traceability evidence.

Remaining:
- Literal type syntax and literal-union narrowing remain separate parser/type checker work.
- Structural shape union narrowing and `is` expression narrowing remain separate follow-up work.

Blocked:
- None.
