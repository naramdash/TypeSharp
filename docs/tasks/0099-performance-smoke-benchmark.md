# Task: Performance Smoke Benchmark

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:59:24 +09:00
End Time: 2026-05-19 05:01:29 +09:00

## Objective

Add a lightweight performance smoke benchmark so the compiler check pipeline has a guardrail against accidental extreme regressions while preserving the `net48` project baseline.

## Source Of Truth

- [../goal.md](../goal.md)
- [../agentic-execution.md](../agentic-execution.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Scope

In:
- one deterministic compiler check performance smoke test
- a generous elapsed-time budget intended to catch hangs or severe regressions, not micro-performance noise
- checklist and traceability updates

Out:
- release-grade benchmark harness
- machine-specific baseline tracking
- CI performance dashboard
- optimizer work

## Acceptance Criteria

- [x] Test creates a multi-file TypeSharp project with stable generated source content.
- [x] Test runs the `TypeSharpChecker` pipeline and verifies all source files are checked without errors.
- [x] Test asserts elapsed time stays under a deliberately generous smoke budget.
- [x] Checklist and traceability record the performance smoke benchmark evidence.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- performance smoke benchmark passes.
- existing tests remain green.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`

## Handoff

Done:
- Added `CompilerCheckPerformanceSmokeStaysBounded`.
- The smoke checks 80 source files and 1200 exported functions through `TypeSharpChecker` under a 15 second budget.
- Marked performance smoke benchmark evidence in checklist, traceability, and task index.

Remaining:
- None.

Blocked:
- None.
