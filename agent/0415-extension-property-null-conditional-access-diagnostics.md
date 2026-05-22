# Task: extension-property-null-conditional-access-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 14:20:00 +09:00
End Time: TBD

## Objective

Add deterministic diagnostics when null-conditional member access syntax targets getter-only TypeSharp-authored extension properties before nullable receiver lifting or TypeSharp-owned null-conditional lowering is implemented.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Detect `receiver?.Property` when `Property` resolves to a getter-only TypeSharp-authored extension property for the known receiver shape.
- Detect simple `receiver?.Property = value` assignment targets when the target resolves to a getter-only TypeSharp-authored extension property.
- Report deterministic `TS2201` diagnostics that explain nullable receiver lifting and TypeSharp-owned null-conditional extension-property access are not implemented yet.
- Preserve existing metadata-backed imported C# null-conditional assignment behavior and existing exact non-null extension-property reads.
- Add focused negative fixture coverage and update canonical docs/ledgers.

Out:
- Implementing nullable receiver lifting.
- Lowering TypeSharp-owned null-conditional extension-property reads or assignments.
- Adding extension property setters, static extension members, operators, imported C# extension property metadata, or fuller extension receiver ranking.
- Changing generated artifact targets, test-host package selection, or Task 0401 CI behavior.

## Acceptance Criteria

- [ ] Extension-property `?.` reads report a deterministic `TS2201` diagnostic before backend emission.
- [ ] Extension-property `?.` assignment targets report a deterministic `TS2201` diagnostic before backend emission.
- [ ] Existing imported C# null-conditional assignment fixtures still pass.
- [ ] Focused type-checker diagnostics, docs build, and diff checks pass.

## Verification

Command: TBD
Expected: focused diagnostics, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
