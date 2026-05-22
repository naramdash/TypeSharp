# Task: imported-csharp-null-conditional-indexer-read-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 13:45:41 +09:00
End Time: TBD

## Objective

Implement bounded imported C# `receiver?[index]` read expressions for readable metadata-backed instance indexers after member reads are in place.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Type-check `receiver?[index]` when the receiver is a nullable/reference-like metadata-backed imported C# instance type and overload resolution selects a readable public instance indexer.
- Reuse existing imported indexer argument validation and ranking where possible.
- Infer nullable-compatible result types and deterministic diagnostics.
- Lower to C# 7.3-compatible generated source preserving single receiver and index argument evaluation and emitting no `?[]`.
- Preserve existing null-conditional assignment, member-read, and extension-property diagnostics.

Out:
- Invocation, chains, compound assignment, increment/decrement, events, static targets, local binding targets, TypeSharp-owned targets, nullable receiver lifting, full flow analysis, and broad target analysis.

## Acceptance Criteria

- [ ] Parser/checker/backend support or diagnostics for `receiver?[index]` read expressions are implemented in the bounded imported C# instance-indexer scope.
- [ ] Positive coverage proves generated `net48` C# consumer behavior and single-evaluation lowering for non-trivial receiver/index arguments.
- [ ] Negative coverage proves unsupported target diagnostics remain deterministic before backend emission.
- [ ] Existing null-conditional assignment/member-read and extension-property diagnostics remain covered.
- [ ] Docs and task ledgers are updated.
- [ ] Verification commands pass.

## Verification

Command: TBD
Expected: compiler build, focused tests, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
