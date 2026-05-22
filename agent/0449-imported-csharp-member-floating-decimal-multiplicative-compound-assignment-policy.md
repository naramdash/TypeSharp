# Task: imported-csharp-member-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 04:00:56 +09:00
Source: Task 0448 roadmap refresh after local floating-point and decimal multiplicative compound assignment policy

## Objective

Extend imported C# regular member `*=`, `/=`, and `%=` targets from the existing primitive integral policy to a bounded floating-point and decimal policy. Reuse the local `float`, `double`, and `decimal` assign-back rules from Task 0447, preserve generated package-free `net48`/C# 7.3 output, and keep the current `net10.0` MSTest.Sdk/MTP package bridge unchanged.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0448 rollup](tasks-rollup.md#task-0448-roadmap-refresh-after-local-floating-point-and-decimal-multiplicative-compound-assignment-policy)
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/diagnostics/type-checker`
- `test/fixtures/backend/csharp`

## Scope

In:

- Imported C# instance/static field and property member targets of the form `receiver.Member *= value`, `receiver.Member /= value`, and `receiver.Member %= value`.
- Readable and writable metadata-backed members whose target and right-side operands fit the bounded known non-null `float`, `double`, or `decimal` policy from local Task 0447.
- Deterministic diagnostics for unsupported operands and targets, including nullable operands, mixed decimal/floating operands, narrowing assign-back, readonly members, events, indexers, null-conditional targets, TypeSharp-owned targets, and user-defined operator shapes.
- Generated package-free `net48` C# consumer evidence, focused positive/negative checker fixtures, backend fixture coverage, catalog/shard count updates, and docs/ledger updates if counts change.

Out:

- Imported C# indexer floating-point or decimal multiplicative compound assignment.
- Null-conditional floating-point or decimal multiplicative compound assignment.
- Checked-overflow policy changes.
- User-defined multiplicative operators or imported operator overload resolution.
- Task 0401 GitHub Actions CI implementation without explicit user approval.
- Test-host package/framework changes beyond count updates required by new catalog entries.

## Acceptance Criteria

- [ ] Imported C# regular field/property member `*=`, `/=`, and `%=` targets accept supported `float`, `double`, and `decimal` combinations that can assign back to the member target.
- [ ] Unsupported floating/decimal member combinations diagnose before backend emission and do not loosen the existing primitive integral diagnostics.
- [ ] Backend lowering remains C# 7.3-compatible, preserves ordinary member compound-assignment output, and keeps non-trivial receivers emitted once.
- [ ] Focused positive/negative/generated `net48` consumer coverage is added and catalog/shard/MTP counts are updated if needed.
- [ ] Feature Status, Work Ledger, tasks, traceability, and rollup evidence reflect the completed slice.
- [ ] Task 0401 remains blocked unless explicit user approval is present.

## Verification

Command: TBD
Expected: focused imported member floating/decimal tests, broader multiplicative coverage, generated `net48` consumer evidence, package-free catalog/shard checks, MSTest bridge count checks, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0448 refreshed official language/platform/package/tooling signals, reaffirmed generated package-free `net48`/C# 7.3 plus the current MSTest.Sdk/MTP package bridge, kept Task 0401 blocked, and selected this imported C# regular member floating/decimal slice.

Remaining:

- Implement the checker/backend/test/docs updates for imported C# regular member floating-point and decimal multiplicative compound assignment.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
