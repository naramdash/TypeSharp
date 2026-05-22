# Task: extension-property-nullable-receiver-access-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 13:22:00 +09:00
End Time: TBD

## Objective

Add deterministic diagnostics when a nullable receiver expression accesses a getter-only TypeSharp-authored extension property whose implemented receiver match is currently exact non-null.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Report a deterministic `TS2201` diagnostic when member access resolves only after stripping nullable receiver state from a getter-only TypeSharp-authored extension property receiver.
- Cover read expressions and assignment targets consistently, preserving the current getter-only assignment diagnostic for exact non-null receivers.
- Add focused negative fixture coverage.
- Update canonical docs and task ledgers.

Out:
- Implementing nullable receiver lifting or null-guard lowering for extension properties.
- Implementing extension property setters, static extension properties, operators, imported C# extension property metadata, or richer conversion/ranking.
- Changing the generated `net48`/C# 7.3 baseline or test-host package selection.
- Implementing Task 0401's GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] Nullable receiver access to a getter-only TypeSharp-authored extension property reports `TS2201` before backend emission.
- [ ] Exact non-null getter-only extension property reads and assignment diagnostics keep their current behavior.
- [ ] Focused diagnostic fixture coverage proves the new negative path.
- [ ] Canonical docs and task ledgers describe the nullable receiver access boundary.
- [ ] Focused compiler tests, docs build, and diff checks pass.

## Verification

Command: TBD
Expected: compiler focused tests, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
