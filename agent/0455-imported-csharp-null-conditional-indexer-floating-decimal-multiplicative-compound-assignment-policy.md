# Task: imported-csharp-null-conditional-indexer-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 05:59:15 +09:00
Source: Task 0454 roadmap refresh after imported C# null-conditional member floating-point and decimal multiplicative compound assignment policy

## Objective

Extend imported C# null-conditional indexer multiplicative compound assignment to the same bounded known non-null integral, floating-point, and decimal assign-back policy used by local, regular member, regular indexer, and null-conditional member targets, while preserving generated package-free `net48` C# 7.3 output.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0454 rollup](tasks-rollup.md#task-0454-roadmap-refresh-after-imported-csharp-null-conditional-member-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `agent/tasks.md`
- `agent/traceability.md`

## Scope

In:

- Imported C# `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` targets when overload resolution selects a metadata-backed public instance indexer with both getter and setter and supported index arguments.
- Nullable-capable receiver validation that evaluates the receiver once, evaluates index arguments and the right side only when the receiver is non-null, and lowers to explicit C# 7.3-compatible null/index guards.
- Type-checker acceptance for known non-null primitive integral, same-family floating-point, and decimal/integral operand combinations when the promoted result is assignable back to the indexer value type.
- Deterministic diagnostics for nullable operands, mixed decimal-floating operands, narrowing assign-back, missing setters, bool/string/enum targets, mismatched or ambiguous indexers, static-like or local targets, TypeSharp-owned targets, unresolved indexers, unsupported index arguments, and unsupported target shapes before backend emission.
- Focused generated `net48` C# consumer, negative checker, docs, traceability, and ledger updates.

Out:

- New regular member/indexer or local multiplicative policy changes.
- Checked/unchecked overflow policy changes.
- User-defined multiplicative operator resolution.
- C# 14/15 syntax emission or generated artifact baseline changes.
- NuGet package/test-host/framework baseline changes.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` accept bounded integral/floating-point/decimal operands for selected readable/writable metadata-backed imported C# instance indexers with supported index arguments.
- [ ] Generated `net48` C# proves receiver single evaluation, skipped index-argument and right-side evaluation on null receivers, and explicit C# 7.3 guard lowering with no emitted null-conditional assignment syntax.
- [ ] Negative checker coverage rejects nullable operands, mixed decimal-floating operands, narrowing assign-back, missing setters, bool/string/enum targets, mismatched or ambiguous indexers, static-like/local/TypeSharp-owned/unresolved targets, and unsupported index arguments before backend emission.
- [ ] Docs and ledgers record the implemented policy, unchanged package/generated baselines, and remaining checked-overflow/user-defined-operator follow-ups.
- [ ] Verification covers focused filters, broader multiplicative coverage, docs build, stale-reference scan, and `git diff --check`.

## Verification

Command: TBD
Expected: focused compiler tests, broader multiplicative tests, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0454 rechecked official language/platform/package/testing/editor/CI signals after imported C# null-conditional member floating-point and decimal multiplicative compound assignment landed, preserved generated package-free `net48`/C# 7.3 and current MSTest.Sdk/MTP package-shard baselines, kept Task 0401 blocked, and selected this paired indexer slice.

Remaining:

- Implement the paired imported C# null-conditional indexer floating-point and decimal multiplicative assignment policy.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
