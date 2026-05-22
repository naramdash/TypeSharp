# Task: imported-csharp-null-conditional-member-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 05:55:00 +09:00
Source: Task 0452 roadmap refresh after imported C# regular indexer floating-point and decimal multiplicative compound assignment policy

## Objective

Extend imported C# null-conditional instance field/property member multiplicative compound assignment targets from the primitive integral policy to the bounded known non-null integral, floating-point, and decimal assign-back policy already used by local mutable targets and regular imported C# member/indexer targets.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0452 rollup](tasks-rollup.md#task-0452-roadmap-refresh-after-imported-csharp-regular-indexer-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Extend imported C# null-conditional instance field/property member targets `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` to the bounded numeric assign-back policy.
- Accept readable/writable metadata-backed imported C# instance fields/properties when the receiver is nullable-capable and target/value operands are known non-null primitive numeric values supported by the existing integral/floating-point/decimal policy.
- Preserve skipped right-side evaluation when the receiver is null, single receiver evaluation, and explicit C# 7.3-compatible null guard lowering with no emitted C# 14 null-conditional assignment.
- Preserve deterministic diagnostics for unsupported operands, nullable operands, mixed decimal-floating operands, narrowing assign-back, readonly fields, missing setters, events, static members, indexers, locals, TypeSharp-owned targets, unresolved members, and unsupported shapes.
- Add focused checker/backend/generated `net48` C# consumer evidence without changing framework/package baselines unless the implementation genuinely requires it.

Out:

- Null-conditional indexer floating-point/decimal multiplicative expansion.
- Checked-overflow policy changes.
- User-defined multiplicative operator resolution.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.
- Package/framework/generated artifact baseline changes.

## Acceptance Criteria

- [ ] `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` accept supported imported C# instance fields/properties for integral, same-family floating-point, and decimal/integral operand combinations where the promoted result is assignable back to the member type.
- [ ] Null receivers skip the right side; non-trivial receivers are evaluated once; emitted C# remains C# 7.3-compatible and package-free `net48`.
- [ ] Unsupported/null/mixed/narrowing/readonly/missing-setter/event/static/indexer/local/TypeSharp-owned/unresolved shapes report deterministic diagnostics before backend emission.
- [ ] Focused generated C# consumer and negative checker coverage prove floating-point and decimal behavior.
- [ ] Existing catalog/shard/MTP package minimums are preserved unless new shared cases are intentionally added and documented.
- [ ] Docs and operational ledgers record the implemented surface and remaining paired null-conditional indexer, checked-overflow, and user-defined operator follow-ups.

## Verification

Command: TBD
Expected: focused imported null-conditional member multiplicative tests, broader multiplicative tests, generated `net48` evidence, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0452 rechecked official language/platform/package/testing/editor/CI signals, preserved the generated package-free `net48`/C# 7.3 and MSTest.Sdk/MTP test-host baselines, kept Task 0401 blocked, and selected this implementation slice.

Remaining:

- Implement the checker/backend/tests/docs updates for imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
