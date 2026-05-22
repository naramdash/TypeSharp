# Task: imported-csharp-null-conditional-member-read-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 13:11:45 +09:00
End Time: TBD

## Objective

Implement bounded imported C# `receiver?.Member` read expressions for readable metadata-backed instance fields/properties, after extension-property-specific null-conditional target diagnostics are in place.

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
- Type-check `receiver?.Member` when the receiver is a nullable/reference-like metadata-backed imported C# instance type and `Member` resolves to a readable public instance field/property.
- Infer a nullable-compatible result type and report deterministic diagnostics for unsupported member shapes.
- Lower accepted reads to C# 7.3-compatible generated source.
- Preserve existing imported C# null-conditional assignment behavior and extension-property-specific diagnostics.
- Add focused parser/type-checker/backend or CLI smoke coverage and docs/ledgers.

Out:
- `receiver?[index]` null-conditional indexer reads.
- Null-conditional method/delegate invocation or chained null-conditional member access.
- Compound assignment, increment/decrement, events, static targets, local binding targets, TypeSharp-owned member/indexer/extension-property targets.
- Nullable receiver lifting for TypeSharp-owned extension properties.
- Full C# nullable flow analysis or broader assignment target analysis.

## Acceptance Criteria

- [ ] Imported C# `receiver?.Member` reads type-check for readable metadata-backed instance fields/properties.
- [ ] Unsupported null-conditional read targets produce deterministic diagnostics.
- [ ] Accepted reads lower to C# 7.3-compatible generated source without changing generated artifact package policy.
- [ ] Existing imported null-conditional assignment and TypeSharp-authored extension-property diagnostics stay covered.
- [ ] Docs, ledgers, focused tests, and diff checks are updated.

## Verification

Command: TBD
Expected: focused compiler/tests/docs checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
