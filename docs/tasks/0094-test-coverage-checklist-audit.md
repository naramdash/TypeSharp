# Task: Test Coverage Checklist Audit

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:40:05 +09:00
End Time: 2026-05-19 04:42:12 +09:00

## Objective

Audit existing test evidence and align the checklist with implemented lowering golden tests, runtime unit tests, and C# interop tests without claiming unverified release or ABI coverage.

## Source Of Truth

- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `tests/TypeSharp.Compiler.Tests/Program.cs`
- `tests/fixtures/backend/csharp/positive`

## Scope

In:
- lowering golden test checklist evidence
- runtime/core helper unit-style smoke evidence
- C# interop test evidence
- traceability rows for these aggregate test categories

Out:
- public ABI snapshot tests
- performance benchmarks
- regression test policy
- release policy items

## Acceptance Criteria

- [x] Checklist marks only test categories backed by concrete existing tests.
- [x] Traceability names the test commands, fixture families, or smoke names used as evidence.
- [x] Public ABI snapshot, performance, and regression policy remain unchecked.

## Verification

Command:

```text
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- existing test suite remains green.
- documentation-only checklist audit has no whitespace errors.

Result:
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- Marked lowering golden tests complete based on C# backend fixture snapshot coverage through fixture `0020`.
- Marked runtime unit tests complete based on Core/Runtime helper behavior tests and net48 packaging checks.
- Marked C# interop tests complete based on reference resolver, metadata, interop validator, generated build, C# consumer, and application host smokes.
- Added traceability rows for each aggregate test category.

Remaining:
- At this task's close, public ABI snapshot tests, performance smoke benchmark, and regression test policy were intentionally left unchecked.
- Follow-up complete: [0098-public-abi-snapshot-smoke.md](0098-public-abi-snapshot-smoke.md), [0099-performance-smoke-benchmark.md](0099-performance-smoke-benchmark.md), and [0100-regression-test-policy.md](0100-regression-test-policy.md) now close those remaining test coverage items.

Blocked:
- None.
