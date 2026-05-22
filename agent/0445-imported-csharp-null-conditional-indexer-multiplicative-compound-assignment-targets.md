# Task: imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 02:37:50 +09:00
Source: Task 0444 roadmap refresh after imported C# null-conditional member multiplicative compound assignment targets

## Objective

Implement imported C# null-conditional indexer multiplicative compound assignment targets, completing the paired `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` slice after Task 0443's member-target implementation.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0444 rollup](tasks-rollup.md#task-0444-roadmap-refresh-after-imported-csharp-null-conditional-member-multiplicative-compound-assignment-targets)

## Scope

In:

- Accept metadata-backed imported C# instance indexer targets shaped `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` when overload resolution selects a readable/writable public getter/setter pair and the index arguments are supported.
- Reuse the existing primitive integral multiplicative assign-back policy from local, imported member, imported indexer, and null-conditional member targets.
- Reuse existing indexer argument validation/ranking and null-conditional indexer receiver semantics.
- Lower accepted targets through explicit C# 7.3-compatible null/index guards, evaluating the receiver once and evaluating index arguments plus the right-side expression only when the receiver is non-null.
- Add focused generated package-free `net48` C# consumer coverage and deterministic negative checker coverage.
- Preserve the existing `net10.0` MSTest.Sdk/MTP bridge, package-shard minimum policy, and Task 0401 blocked state.

Out:

- Floating-point or decimal multiplicative compound assignment policy expansion.
- Checked overflow policy changes.
- User-defined multiplicative or compound assignment operators.
- Null-conditional increment/decrement, invocation, chains, events, static targets, TypeSharp-owned targets, or local binding null-conditional assignment.
- Fixing Task 0401 without explicit user approval.

## Acceptance Criteria

- [ ] `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` type-check for supported metadata-backed imported C# instance indexers.
- [ ] Unsupported operands, missing getter/setter, unsupported index arguments, ambiguous indexers, event/static/unresolved/TypeSharp-owned/local targets, narrowing promoted results, and other invalid shapes report deterministic diagnostics before backend emission.
- [ ] Generated C# remains C# 7.3-compatible, emits no C# `?[]`, evaluates the receiver once, and skips index arguments plus the right side when the receiver is null.
- [ ] Generated package-free `net48` C# consumer coverage and negative checker coverage are added.
- [ ] Shared catalog counts, shard expectations, MSTest package-shard minimums, docs, tasks, traceability, and rollup are updated consistently.

## Verification

Command: TBD
Expected: focused null-conditional indexer multiplicative checks, broader multiplicative coverage, full package-free catalog, MTP package-shard run, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0444 rechecked official source signals, reaffirmed the generated package-free `net48`/C# 7.3 baseline and existing `net10.0` MSTest.Sdk/MTP package bridge, kept Task 0401 blocked, and selected this paired null-conditional indexer multiplicative slice.

Remaining:

- Implement the checker/backend/test/docs slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
