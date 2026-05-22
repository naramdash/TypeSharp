# Task: imported-csharp-null-conditional-logical-unsigned-shift-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 14:27:20 +09:00
End Time: TBD

## Objective

Implement the first bounded C# 14 null-conditional compound-assignment slice by supporting imported C# `receiver?.Member >>>= count` assignment targets.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:
- Accept `receiver?.Member >>>= count` when `Member` resolves to a readable/writable metadata-backed imported C# instance field or property.
- Reuse the existing logical unsigned shift assignment primitive integral target and count policy.
- Preserve single receiver evaluation.
- Skip `count` evaluation when the receiver is null.
- Lower to C# 7.3-compatible explicit null guards plus the existing unsigned-cast assignment shape; emit no C# `?.`, `>>>`, or `>>>=`.
- Add focused positive generated `net48` C# consumer coverage and negative checker coverage.
- Update docs, task ledger, traceability, and package shard expectations if the shared catalog count changes.

Out:
- `receiver?[index] >>>= count`.
- Null-conditional `|=`, `&=`, `^=`, `<<=`, `>>=`, `+=`, `-=`, increment, or decrement.
- Event, static, local, TypeSharp-owned, method-call, chained, extension-property, and user-defined operator targets.
- Nullable receiver lifting beyond the currently accepted imported C# receiver policy.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit approval.
- Changing the `net10.0` MSTest.Sdk/MTP package bridge or generated `net48` package-free baseline.

## Acceptance Criteria

- [ ] Accepted imported C# field/property `receiver?.Member >>>= count` targets type-check with the existing `>>>=` primitive/count rules.
- [ ] Unsupported targets report deterministic diagnostics before backend emission.
- [ ] Generated C# evaluates the receiver once, evaluates `count` only when the receiver is non-null, compiles as C# 7.3 for `net48`, and emits no C# 14 syntax.
- [ ] Existing null-conditional simple assignment/read and non-null-conditional `>>>=` behavior remains covered.
- [ ] Docs and agent ledgers are updated.
- [ ] Verification commands pass.

## Verification

Command: TBD
Expected: focused compiler tests, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
