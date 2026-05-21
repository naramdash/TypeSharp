# Task 0318 Explicit Enum Underlying Types Slice

## Goal

Implement explicit underlying types for TypeSharp-owned enum declarations while keeping the existing finite enum-member type-checking and exhaustiveness behavior.

## Scope

- Parse enum declarations with an optional underlying type clause, for example `enum Color : byte { Red = 1 }`.
- Preserve duplicate enum member diagnostics and name/member-based enum checking.
- Lower supported underlying type clauses to ordinary C# 7.3-compatible enum declarations.
- Keep explicit numeric member values working with the underlying type clause.
- Add parser/backend fixtures and generated `net48` build coverage.
- Update grammar, reference, type-system, lowering, feature-status, work-ledger, tasks, and traceability docs.

## Out Of Scope

- Flags/set algebra.
- Enum aliases and duplicate numeric value policy.
- Computed enum member expressions.
- Validation that every numeric value fits the declared underlying type.
- Imported C# enum underlying-type metadata.
- Enum member attributes.

## Done Criteria

- Parser accepts `enum Name : Underlying { ... }` and the existing enum forms.
- Backend emits `enum Name : Underlying` for TypeSharp-owned enums without requiring newer C# syntax.
- Existing enum match exhaustiveness remains name/member based.
- Focused fixtures/smokes pass, followed by the full compiler test project and docs build.
- Completed task is rolled into `agent/tasks-rollup.md`, this packet is removed, and the change is committed and pushed.
