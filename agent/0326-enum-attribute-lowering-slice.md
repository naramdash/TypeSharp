# Task 0326 Enum Attribute Lowering Slice

Status: In Progress
Priority: Q3
Source: Task 0325 roadmap refresh
Started: 2026-05-21

## Objective

Lower TypeSharp-owned enum declaration and enum member attributes to generated C# metadata shape.

## Scope

- Parse attribute lists on enum members inside TypeSharp-owned enum declarations.
- Preserve existing declaration attribute syntax on enum declarations and lower supported enum attributes to generated C#.
- Emit enum member attributes before generated C# enum members.
- Add parser/backend/generated `net48` coverage for `[FlagsAttribute]`-style enum metadata and member attributes.
- Document that this is metadata lowering only; current enum matching and numeric reasoning remain name/member based.

## Non-Goals

- Do not implement flag-style enum algebra or flag-aware exhaustiveness.
- Do not add numeric pattern matching over enum values.
- Do not add broad attribute target validation outside the enum declaration/member slice.
- Do not add arbitrary computed enum member expressions.
- Do not change imported C# enum reasoning beyond existing metadata capture.

## Implementation Plan

1. Extend enum member parsing to accept leading attribute lists before member names.
2. Teach C# source lowering to render enum declaration and enum member attributes in the existing C# 7.3-compatible style.
3. Add parser/backend fixture coverage and a generated `net48` compile smoke for enum attribute metadata.
4. Update grammar, reference, type-system, lowering, feature-status, work-ledger, and rollup docs for the metadata-only boundary.

## Done Criteria

- TypeSharp-owned enum declaration/member attributes parse and lower to generated C#.
- `[FlagsAttribute]` can be emitted on a TypeSharp-owned enum without changing flag semantics.
- Existing enum numeric values, aliases, range validation, match exhaustiveness, and imported enum behavior stay unchanged.
- Tests and docs cover the metadata-only boundary.
- Changes are verified, rolled up, committed, and pushed.
