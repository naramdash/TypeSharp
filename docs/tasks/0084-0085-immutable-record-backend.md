# Task Rollup: Immutable Record Backend

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 02:55:34 +09:00
End Time: 2026-05-19 03:04:34 +09:00

## Compressed Tasks

- `0084-immutable-record-backend-smoke.md`
- `0085-record-update-lowering-smoke.md`

## Objective

TypeSharp `record` declaration을 generated C# `net48` immutable class shape로 낮추고, record copy/update expression을 constructor call로 낮춰 C# consumer가 immutable record workflow를 사용할 수 있음을 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/declarations.md](../grammar/declarations.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Scope

Completed:
- generated C# emission for top-level record declarations with primary parameters.
- immutable get-only property emission.
- constructor emission for record parameters.
- value equality and hash override emission for record parameters.
- backend record metadata tracking.
- function parameter type tracking for typed record update receivers.
- record update lowering to generated C# constructor calls.
- unchanged field preservation through source record property reads.
- backend fixtures `0015-immutable-record-api` and `0016-record-update-lowering`.
- CLI build and C# `net48` consumer smokes for generated immutable record APIs and copied/updated values.
- checklist completion for `immutable record`.
- traceability entries for immutable record generated API and record update lowering.

Out:
- type checker inference for arbitrary record update expressions.
- local variable type tracking without annotations.
- generic record update.
- diagnostics for unknown fields or unknown receiver type.
- record body members and nested records.

## Verification

Commands run across the compressed packet set:

```text
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Result:
- PASS `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`.
- PASS `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`, including backend fixture snapshots, `CLI build compiles immutable record API`, and `CLI build compiles record update lowering`.
- PASS `git diff --check`.

## Handoff

Done:
- Generated public record metadata now covers immutable construction, property reads, equality/hash behavior, and copy/update lowering for typed record parameters.
- C# `net48` consumer compatibility is covered for record construction/property/equality/hash and copied/updated values.

Remaining:
- Implement richer record semantic model and diagnostics.
- Implement generic record update.
- Implement record body members.

Blocked:
- None.
