# Task: extension-property-nullable-receiver-diagnostics

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 11:40:17 +09:00
End Time: TBD

## Objective

Add deterministic diagnostics for unsupported nullable receiver declarations in getter-only TypeSharp-authored extension properties.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

## Scope

In:
- Report a clear `TS2201` diagnostic when a getter-only TypeSharp-authored extension property is declared on a nullable receiver type.
- Keep the current exact known non-null receiver matching and C# 7.3 helper lowering unchanged.
- Add focused negative fixture or catalog coverage that proves nullable receiver declarations fail before backend emission.
- Update the matching docs and ledgers.

Out:
- Implementing nullable receiver lifting or nullable receiver overload ranking.
- Implementing static extension members, setters, operators, or imported C# extension property metadata.
- Changing generated artifact targets, test-host package selection, or Task 0401's CI fix.

## Acceptance Criteria

- [ ] Nullable extension-property receiver declarations report deterministic `TS2201` diagnostics.
- [ ] Existing non-null extension property lowering and helper-name collision coverage still passes.
- [ ] Docs and task ledgers describe the nullable receiver boundary.
- [ ] Relevant compiler/docs verification passes.

## Verification

Command: TBD
Expected: focused diagnostics, extension-property regression coverage, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
