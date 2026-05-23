# Task: imported-csharp-null-conditional-member-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0472 roadmap refresh after imported C# regular indexer user-defined multiplicative operator policy

## Objective

Extend the imported C# static binary user-defined multiplicative operator precursor from mutable locals and regular imported field/property/indexer targets to null-conditional imported C# field/property member `*=`, `/=`, and `%=` assignment while preserving generated package-free `net48`, C# 7.3 lowering, deterministic diagnostics, and the current 574/578 test baselines until coverage intentionally raises them.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata and enabled mutable local `*=`, `/=`, and `%=` through one selected imported static binary operator.
- Task 0469 extended that precursor to regular readable/writable imported C# field/property member targets.
- Task 0471 extended the same precursor to regular readable/writable imported C# indexer targets with supported arguments.
- Existing null-conditional imported C# member/indexer multiplicative assignment already supports the bounded primitive integral/floating-point/decimal assign-back policy, including skipped right-side evaluation on null receivers and explicit C# 7.3 guard lowering.
- Task 0472 rechecked official language/platform/package/testing/editor/CI signals, confirmed no generated-artifact baseline change, confirmed the post-Task-0401 GitHub Actions regression run passed, and selected this null-conditional member user-defined operator slice before indexer, checked user-defined, TypeSharp-authored, true C# 14 instance compound-assignment, or broader overload-ranking expansion.

## Scope

In:

- Allow `receiver?.Member *= value`, `receiver?.Member /= value`, and `receiver?.Member %= value` for readable/writable metadata-backed imported C# instance field/property targets when exactly one imported public static binary operator matches the target value and right operand and its result assigns back to the member type.
- Preserve skipped right-side evaluation on null receivers and keep the selected operator assignment inside the generated C# 7.3 null guard.
- Preserve existing primitive null-conditional member multiplicative assignment behavior.
- Add focused positive generated C# consumer coverage and negative checker coverage for missing, ambiguous, nullable, non-assignable, readonly/getter-only/event/static, unsupported target, and TypeSharp-owned/local target shapes.
- Update Feature Status, Type System, Lowering, Diagnostics, .NET Interop, Project Policy, Work Ledger, tasks, and traceability as needed.

Out:

- Null-conditional imported C# indexer user-defined operator targets.
- Checked-specific user-defined operator lookup.
- TypeSharp-authored operator declarations.
- True C# 14 instance compound-assignment operators.
- Broader overload ranking beyond the existing selected imported static binary operator policy.
- Any dependency, generated package, or generated language-version change.

## Acceptance

- [ ] Accepted null-conditional imported member user-defined `*=`, `/=`, and `%=` cases lower to package-free `net48` C# 7.3-compatible source and evaluate the right side only when the receiver is non-null.
- [ ] Existing primitive null-conditional member multiplicative behavior remains covered and unchanged.
- [ ] Negative diagnostics are deterministic for missing/ambiguous operators, nullable operands, non-assignable results, non-writable targets, events/static targets, TypeSharp-owned/local targets, and unsupported shapes.
- [ ] Shared catalog/package-shard baselines stay at 574/578 or are deliberately raised with matching test/CI/docs constants.
- [ ] Relevant docs, tasks, rollup, and traceability records are updated.
- [ ] Focused compiler tests, shard/package-shard checks, docs build, stale-reference scans, and whitespace checks pass.

## References

- [Task 0472 rollup](tasks-rollup.md#task-0472-roadmap-refresh-after-imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy)
- [Task 0471 rollup](tasks-rollup.md#task-0471-imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Handoff Notes

- Do not use Python.
- Preserve the package-free generated artifact boundary and current test-host package split.
- Prefer reusing the existing regular member/indexer imported static binary operator checks and the existing null-conditional member primitive multiplicative guard lowering.
