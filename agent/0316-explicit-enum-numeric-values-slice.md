# Task 0316 Explicit Enum Numeric Values Slice

## Goal

Implement explicit numeric member values for TypeSharp-owned enum declarations while keeping the existing finite enum-member type-checking and exhaustiveness behavior.

## Scope

- Parse enum members with optional numeric initializers, for example `Red = 1`.
- Preserve duplicate enum member name diagnostics.
- Keep enum type checking based on member names, not numeric values.
- Lower explicit enum values to ordinary C# 7.3-compatible enum member assignments.
- Add parser/backend fixtures and generated `net48` build coverage.
- Update grammar, reference, type-system, lowering, feature-status, work-ledger, tasks, and traceability docs.

## Out Of Scope

- Explicit enum underlying types such as `enum Color : byte`.
- String enum values.
- Computed enum member expressions.
- Flags/set algebra.
- Duplicate numeric value or alias diagnostics.
- Imported C# enum value extraction beyond the existing member-name metadata.

## Done Criteria

- Parser accepts explicit numeric enum member values and rejects neither implicit members nor comma-separated existing enum syntax.
- C# backend emits explicit numeric assignments for TypeSharp-owned enums without requiring newer C# syntax.
- Existing enum match exhaustiveness remains name/member based.
- Focused fixtures/smokes pass, followed by the full compiler test project and docs build.
- Completed task is rolled into `agent/tasks-rollup.md`, this packet is removed, and the change is committed and pushed.
