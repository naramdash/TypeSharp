# Task: imported-csharp-null-conditional-indexer-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0474 roadmap refresh after imported C# null-conditional member user-defined multiplicative operator policy

## Objective

Extend the imported C# static binary user-defined multiplicative operator precursor to null-conditional imported C# readable/writable indexer `*=`, `/=`, and `%=` assignment while preserving skipped index/right-side evaluation on null receivers, generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and the current 574/578 baselines unless focused coverage deliberately changes them.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata and enabled mutable local `*=`, `/=`, and `%=` through one selected imported static binary operator.
- Task 0469 extended that precursor to regular readable/writable imported C# field/property member targets.
- Task 0471 extended the precursor to regular readable/writable imported C# indexer targets with supported arguments.
- Task 0473 extended the precursor to null-conditional readable/writable imported C# field/property member targets.
- Task 0474 rechecked official language/platform/package/testing/editor/CI signals and selected the paired null-conditional indexer slice as the next bounded implementation step before checked user-defined operators, TypeSharp-authored operators, true C# 14 instance compound-assignment operators, or broader overload-ranking expansion.

## Scope

In:

- Accept `receiver?[index] *= value`, `receiver?[index] /= value`, and `receiver?[index] %= value` for readable/writable metadata-backed imported C# instance indexers with supported arguments when exactly one imported public static binary multiplicative operator matches the indexer value and right operand and the operator result assigns back to the indexer value type.
- Preserve existing primitive integral/floating-point/decimal null-conditional indexer multiplicative behavior and existing indexer argument validation/ranking diagnostics.
- Preserve skipped index-argument and right-side evaluation when the receiver is null, single receiver evaluation, package-free generated `net48`, and C# 7.3-compatible guard lowering.
- Add focused positive generated C# consumer coverage and negative checker coverage for missing, ambiguous, nullable, non-assignable, missing-setter, unsupported argument, unresolved/static-like, TypeSharp-owned/local, and unsupported target shapes.
- Update Feature Status, Type System, Lowering, Diagnostics, .NET Interop, Work Ledger, tasks, and traceability if behavior or backlog wording changes.

Out:

- Checked/unchecked user-defined operator lookup.
- TypeSharp-authored operator declarations.
- True C# 14 instance compound-assignment operators.
- Broader C# overload/operator ranking beyond the selected imported static binary operator policy.
- New dependencies, target framework changes, generated C# language-version changes, or test-host package changes.

## Acceptance

- [ ] Accepted null-conditional imported C# indexer user-defined multiplicative cases lower to C# 7.3-compatible null/index guards and bind through ordinary C# compound-assignment syntax in the non-null branch.
- [ ] Index arguments and the right side are evaluated only when the receiver is non-null, and the receiver is evaluated once.
- [ ] Primitive null-conditional indexer multiplicative behavior remains unchanged.
- [ ] Unsupported user-defined/operator/indexer/nullability/assign-back shapes report deterministic diagnostics before backend emission.
- [ ] Generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and 574/578 baselines are preserved unless focused catalog coverage deliberately raises them.
- [ ] Relevant compiler, package-shard, docs, stale-reference, and whitespace verification passes.

## References

- [Task 0474 rollup](tasks-rollup.md#task-0474-roadmap-refresh-after-imported-csharp-null-conditional-member-user-defined-multiplicative-operator-policy)
- [Task 0473 rollup](tasks-rollup.md#task-0473-imported-csharp-null-conditional-member-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Handoff Notes

- Do not use Python.
- Reuse the existing regular indexer user-defined operator policy and null-conditional primitive indexer guard lowering rather than introducing a new operator-resolution path.
- Keep generated artifacts package-free `net48` and C# 7.3-compatible.
