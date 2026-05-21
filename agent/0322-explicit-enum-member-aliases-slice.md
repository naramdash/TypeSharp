# Task 0322 Explicit Enum Member Aliases Slice

Status: In Progress
Priority: Q2
Source: Task 0321 roadmap refresh
Started: 2026-05-21

## Objective

Allow a TypeSharp-owned enum member to alias a previously declared member of the same enum.

## Scope

- Parse enum member initializers of the form `Alias = ExistingMember` alongside existing integer initializers.
- Validate alias targets against previously declared members in the same enum and report deterministic `TS2201` diagnostics for missing, self, or forward aliases.
- Lower valid aliases to ordinary C# enum assignments such as `Crimson = Red`.
- Preserve numeric range validation, same-enum type checking, and enum match exhaustiveness by keeping reasoning name/member based.

## Non-Goals

- Do not add arbitrary computed enum expressions.
- Do not add bitwise or flag-style enum algebra.
- Do not add enum member attributes.
- Do not add duplicate numeric value policy beyond explicit aliases.
- Do not add imported C# enum numeric or underlying-type metadata reasoning.

## Implementation Plan

1. Extend enum parsing so an initializer can hold either a signed numeric token or a direct identifier alias target.
2. Extend enum type checking with a small alias-target validation pass.
3. Extend C# enum lowering to emit alias target identifiers.
4. Add parser, type-checker positive/negative, backend snapshot, docs, and ledger updates.

## Done Criteria

- `Alias = ExistingMember` parses, checks, lowers, and builds for TypeSharp-owned enums.
- Missing, self, or forward alias targets fail before generated C# emission with stable diagnostics.
- Numeric enum initializer behavior and range validation stay unchanged.
- Changes are verified, rolled up, committed, and pushed.
