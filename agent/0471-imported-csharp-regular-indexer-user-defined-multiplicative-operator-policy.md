# Task: imported-csharp-regular-indexer-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0470 roadmap refresh after imported C# regular member user-defined multiplicative operator policy

## Objective

Extend the imported C# static binary user-defined multiplicative operator precursor from mutable locals and regular imported C# field/property members to regular imported C# indexer assignment targets. Support `receiver[index] *= value`, `receiver[index] /= value`, and `receiver[index] %= value` when overload resolution selects a readable/writable imported C# indexer with supported arguments, exactly one imported public static binary operator matches the indexer value and right operand, and the result assigns back to the indexer value type.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata separately from ordinary methods.
- Task 0467 enabled mutable local `let mut` `*=`, `/=`, and `%=` assignment through one selected imported static binary operator when the result assigns back to the local target.
- Task 0469 extended that precursor to regular readable/writable imported C# instance/static field/property member targets.
- Imported regular indexer `*=`, `/=`, and `%=` targets already support bounded primitive integral/floating-point/decimal assignment, selected public getter/setter pairs, supported index arguments, and C# 7.3 checked/unchecked wrappers for primitive cases.
- Generated source must remain package-free `net48` and C# 7.3-compatible. Accepted regular indexer operator cases should lower to ordinary generated C# compound assignment syntax so the C# compiler binds the imported static operator while preserving indexer receiver/index single-evaluation semantics.

## Scope

- Reuse the Task 0467/0469 imported static binary operator matching policy for regular imported C# readable/writable indexer targets.
- Preserve existing primitive indexer multiplicative assignment behavior and diagnostics.
- Require known non-null target values and right operands, exactly one matching imported operator, deterministic ambiguity diagnostics, and assign-back validation against the indexer value type.
- Reuse existing regular imported indexer getter/setter selection and supported index-argument validation.
- Add focused generated package-free `net48` consumer coverage and negative checker coverage for missing, ambiguous, nullable, non-assignable, missing-setter, unsupported-indexer, and unsupported target shapes.
- Update docs and ledgers for the newly accepted regular indexer target surface and any test-count baseline changes.

## Out Of Scope

- C# 14 instance compound-assignment operators such as `op_MultiplicationAssignment`.
- Imported C# null-conditional indexer or member user-defined operator assignment targets.
- TypeSharp-authored operator declarations.
- Checked user-defined operator semantics or checked-specific imported operator lookup.
- Broader overload ranking beyond the existing exact/metadata-name bounded policy.
- Implementing Task 0401 without explicit user approval.

## Acceptance

- [ ] Imported C# regular indexer `*=`, `/=`, and `%=` targets can use one selected imported public static binary operator when indexer selection and assign-back succeed.
- [ ] Primitive regular indexer multiplicative assignment behavior remains unchanged.
- [ ] Unsupported and ambiguous operator/indexer shapes report deterministic diagnostics before backend emission.
- [ ] Generated C# remains package-free `net48` and C# 7.3-compatible.
- [ ] Focused positive/negative tests, broader multiplicative regression coverage, package-shard baseline checks, docs build, stale scan, and whitespace checks pass.

## References

- [Task 0467 rollup](tasks-rollup.md#task-0467-imported-csharp-user-defined-multiplicative-operator-policy)
- [Task 0469 rollup](tasks-rollup.md#task-0469-imported-csharp-regular-member-user-defined-multiplicative-operator-policy)
- [Task 0470 rollup](tasks-rollup.md#task-0470-roadmap-refresh-after-imported-csharp-regular-member-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Handoff Notes

- Start from `TypeSharpTypeChecker`'s regular imported indexer multiplicative assignment path and the imported static operator helper now shared by local and regular field/property member targets.
- Do not use Python.
