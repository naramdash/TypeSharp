# Task 0324 Imported Enum Numeric Metadata Slice

Status: In Progress
Priority: Q3
Source: Task 0323 roadmap refresh
Started: 2026-05-21

## Objective

Capture imported C# enum underlying type and literal numeric member values in TypeSharp metadata.

## Scope

- Extend metadata symbols for imported C# enum underlying type name.
- Extend imported C# enum metadata with deterministic member numeric values when literal metadata exposes them.
- Add metadata reader coverage using local `net48` C# enum fixtures.
- Document that this is metadata capture only; current match exhaustiveness remains name/member based.

## Non-Goals

- Do not add numeric pattern matching over imported enum values.
- Do not add flag-style enum algebra.
- Do not change TypeSharp-owned enum parsing or lowering.
- Do not expose imported enum numeric values as TypeSharp constants yet.
- Do not add arbitrary computed enum expressions.

## Implementation Plan

1. Extend `MetadataTypeSymbol` and field metadata with enum underlying and literal value metadata.
2. Populate the data in `TypeSharpMetadataReader` from `value__` and literal enum fields.
3. Add or extend metadata reader tests for local imported C# enum underlying/numeric values.
4. Update .NET interop, type-system, feature-status, work-ledger, and rollup docs.

## Done Criteria

- Imported enum metadata preserves underlying type name and per-member literal values where available.
- Existing imported enum exhaustiveness and generated C# lowering behavior stay unchanged.
- Tests and docs cover the metadata-only boundary.
- Changes are verified, rolled up, committed, and pushed.
