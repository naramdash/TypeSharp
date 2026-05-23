# Task: imported-csharp-regular-member-user-defined-multiplicative-operator-policy

Status: Ready
Priority: Q1
Created: 2026-05-23
Source: Task 0468 roadmap refresh after imported C# user-defined multiplicative operator policy

## Objective

Extend the imported C# static binary user-defined multiplicative operator precursor from mutable locals to regular imported C# field/property member assignment targets. Support `receiver.Member *= value`, `receiver.Member /= value`, `receiver.Member %= value`, and static member counterparts when exactly one imported public static binary operator matches the member value and right operand and the result assigns back to the member type.

## Context

- Task 0467 captured imported C# public static binary `op_Multiply`, `op_Division`, and `op_Modulus` metadata separately from ordinary methods.
- Mutable local `let mut` `*=`, `/=`, and `%=` assignment can already use one selected imported static binary multiplicative operator when the result assigns back to the target.
- Imported regular field/property `*=`, `/=`, and `%=` targets already support bounded primitive integral/floating-point/decimal assignment and C# 7.3 checked/unchecked wrappers for those primitive cases.
- Generated source must remain package-free `net48` and C# 7.3-compatible. Accepted member operator cases should lower to ordinary generated C# compound assignment syntax that the C# compiler binds to the imported static operator.

## Scope

- Reuse the local Task 0467 imported static binary operator matching policy for regular imported C# readable/writable field/property member targets.
- Preserve existing primitive member multiplicative assignment behavior and diagnostics.
- Require known non-null target values and right operands, exactly one matching imported operator, deterministic ambiguity diagnostics, and assign-back validation against the field/property type.
- Cover instance and static imported C# field/property targets already supported by the regular member assignment path.
- Add focused generated package-free `net48` consumer coverage and negative checker coverage for missing, ambiguous, nullable, non-assignable, readonly/missing-setter, and unsupported target shapes.
- Update docs and ledgers for the newly accepted member target surface and any test-count baseline changes.

## Out Of Scope

- C# 14 instance compound-assignment operators such as `op_MultiplicationAssignment`.
- Imported C# indexer or null-conditional user-defined operator assignment targets.
- TypeSharp-authored operator declarations.
- Checked user-defined operator semantics or checked-specific imported operator lookup.
- Broader overload ranking beyond the existing exact/metadata-name bounded policy.
- Implementing Task 0401 without explicit user approval.

## Acceptance

- [ ] Imported C# regular field/property member `*=`, `/=`, and `%=` targets can use one selected imported public static binary operator when assign-back succeeds.
- [ ] Primitive member multiplicative assignment behavior remains unchanged.
- [ ] Unsupported and ambiguous operator/member shapes report deterministic diagnostics before backend emission.
- [ ] Generated C# remains package-free `net48` and C# 7.3-compatible.
- [ ] Focused positive/negative tests, broader multiplicative regression coverage, package-shard baseline checks, docs build, stale scan, and whitespace checks pass.

## References

- [Task 0467 rollup](tasks-rollup.md#task-0467-imported-csharp-user-defined-multiplicative-operator-policy)
- [Task 0468 rollup](tasks-rollup.md#task-0468-roadmap-refresh-after-imported-csharp-user-defined-multiplicative-operator-policy)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Handoff Notes

- Start from `TypeSharpTypeChecker`'s regular imported member multiplicative assignment path and the local imported static operator helper added by Task 0467.
- Do not use Python.
