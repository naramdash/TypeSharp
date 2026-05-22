# Task: imported-csharp-indexer-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 04:37:26 +09:00
Source: Task 0450 roadmap refresh after imported C# regular member floating-point and decimal multiplicative compound assignment policy

## Objective

Extend imported C# regular indexer `*=`, `/=`, and `%=` compound assignment targets from primitive integral-only operands to the bounded known non-null integral/floating-point/decimal policy already used for local mutable targets and imported C# regular field/property member targets. Preserve generated package-free `net48`/C# 7.3 output, single receiver/index argument evaluation, and the current test-host package strategy.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0450 rollup](tasks-rollup.md#task-0450-roadmap-refresh-after-imported-csharp-regular-member-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`

## Scope

In:

- Imported C# instance indexer targets shaped `receiver[index] *= value`, `receiver[index] /= value`, and `receiver[index] %= value`.
- Metadata-backed indexers where overload resolution selects a public getter/setter pair with supported index arguments.
- Known non-null primitive numeric operands accepted by the bounded integral/floating-point/decimal assign-back policy, including same-family floating operands and decimal/integral combinations.
- Deterministic diagnostics for unsupported operands, nullable operands, mixed decimal-floating operands, narrowing assign-back, missing setters, ambiguous or mismatched indexers, unsupported index arguments, and unsupported target shapes.
- Focused checker/backend or generated `net48` C# consumer evidence as needed, preserving catalog/shard/package counts unless new shared cases are added.

Out:

- Null-conditional indexer/member floating-point and decimal multiplicative expansion.
- Checked-overflow policy changes.
- User-defined operator resolution.
- Task 0401 GitHub Actions `npm` process-launch implementation without explicit user approval.
- Package, framework, or generated artifact baseline changes.

## Acceptance Criteria

- [ ] Supported imported C# regular indexer `*=`, `/=`, and `%=` targets accept bounded integral/floating-point/decimal operands when the promoted result is assignable back to the indexer value type.
- [ ] Unsupported operand, nullable, mixed decimal-floating, narrowing, getter-only, ambiguous, mismatched, and unsupported argument cases report deterministic diagnostics before backend emission.
- [ ] Generated C# remains ordinary C# 7.3-compatible compound-assignment output for accepted regular indexer targets, with receiver and index arguments emitted once.
- [ ] Tests/docs/ledger/traceability updates preserve generated package-free `net48` and current `MSTest.Sdk/4.2.3` MTP package-shard policy.
- [ ] Task 0401 remains blocked absent explicit user approval.

## Verification

Command: TBD
Expected: focused compiler/test coverage, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0450 rechecked official signals, preserved the generated package-free `net48`/C# 7.3 baseline and `MSTest.Sdk/4.2.3` MTP package bridge, recorded NUnit 4.6.1 as the current comparison version, and selected this implementation slice.

Remaining:

- Implement imported C# regular indexer floating-point and decimal multiplicative compound assignment policy.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
