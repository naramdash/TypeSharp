# Task 0139: Pipeline Expression Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 13:35:42 +09:00
End Time: 2026-05-19 13:36:55 +09:00

## Objective

Implement the MVP pipeline expression lowering contract already fixed in feasibility docs: `value |> f` lowers to `f(value)` and `value |> f(args...)` lowers to `f(value, args...)`.

## Scope

In:
- C# source backend lowering for binary `|>` expressions.
- Chained pipeline lowering as nested C# calls.
- Basic function return inference for identifier and call pipeline targets.
- Backend fixture and CLI build/C# consumer smoke.
- Checklist, feature map, lowering, feature-spec, traceability, and task queue updates.

Out:
- Placeholder-based partial application.
- Dedicated function composition operator.
- Full arity validation for pipeline targets.
- Rich generic/contextual inference for piped calls.

## Acceptance Criteria

- [x] `value |> f` lowers to `f(value)`.
- [x] `value |> f(args...)` lowers to `f(value, args...)`.
- [x] Chained pipeline expressions lower to nested C# 7.3-compatible calls.
- [x] Backend fixture snapshots pin generated C#.
- [x] CLI build smoke produces a generated `net48` assembly and C# consumer compiles against it.
- [x] Docs and traceability distinguish implemented pipeline lowering from composition/placeholder backlog.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "pipeline lowering"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "C# backend fixture"
```

Result:
- PASS compiler test project build.
- PASS focused pipeline build/C# consumer smoke.
- PASS C# backend fixture snapshots.

## Handoff

Done:
- Added pipeline lowering and target return inference.
- Added backend fixture, CLI build smoke, and docs/task updates.

Remaining:
- None for MVP first-argument pipeline lowering.
- Placeholder partial application, dedicated composition operator, full arity validation, and richer generic/contextual pipeline inference remain future work.

Blocked:
- None.
