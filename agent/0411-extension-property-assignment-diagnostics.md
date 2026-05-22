# Task: extension-property-assignment-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 12:42:00 +09:00
End Time: TBD

## Objective

Add deterministic diagnostics when getter-only TypeSharp-authored extension properties are used as assignment targets, preserving the current C# 7.3 helper-lowering boundary until extension setters are designed.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Detect ordinary, bitwise compound, shift compound, and logical unsigned shift assignment targets that resolve to TypeSharp-authored getter-only extension properties.
- Report a deterministic `TS2201` before backend emission instead of allowing the assignment path to fall through as an imported/member target.
- Add focused negative fixture coverage for extension property assignment targets.
- Preserve existing getter-only extension property access, helper lowering, duplicate/conflict/helper-name/nullable-receiver diagnostics, and imported C# writable member/indexer assignment behavior.

Out:
- Implementing extension property setters.
- Implementing static extension members, operators, nullable receiver lifting, imported C# extension property metadata, or richer extension ranking.
- Changing generated artifact targets, test-host package selection, or GitHub Actions Task 0401 without explicit approval.

## Acceptance Criteria

- [ ] Assigning to a TypeSharp-authored getter-only extension property reports a focused `TS2201` diagnostic.
- [ ] Accepted extension property reads and C# 7.3 helper lowering remain unchanged.
- [ ] Imported C# assignment targets and existing assignment diagnostics remain unchanged.
- [ ] Focused compiler tests pass.
- [ ] Docs and task ledgers are updated.

## Verification

Command: TBD
Expected: focused type-checker diagnostics, extension-property lowering smoke, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
