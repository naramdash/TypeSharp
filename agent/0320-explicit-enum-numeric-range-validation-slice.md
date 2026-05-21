# Task 0320 Explicit Enum Numeric Range Validation Slice

Status: In Progress
Priority: Q2
Source: Task 0319 roadmap refresh
Started: 2026-05-21

## Objective

Validate explicit numeric member initializers on TypeSharp-owned enums before generated C# emission.

## Scope

- Treat a TypeSharp enum with no explicit underlying type as `int` for numeric range validation.
- Treat a TypeSharp enum with `: byte`, `: sbyte`, `: short`, `: ushort`, `: int`, `: uint`, `: long`, or `: ulong` as using that C# enum range.
- Report deterministic `TS2201` diagnostics for explicit member values that are outside the selected underlying type range.
- Report deterministic `TS2201` diagnostics for explicit member values that are numeric tokens but not integer literals.
- Keep enum member names, type checking, and match exhaustiveness name/member based.

## Non-Goals

- Do not add computed enum expressions.
- Do not add enum aliases or duplicate numeric value policy.
- Do not add flag-style enum algebra.
- Do not add enum member attributes.
- Do not add imported C# enum numeric or underlying-type metadata reasoning.

## Implementation Plan

1. Extend `TypeSharpTypeChecker.CheckEnumDeclaration` to validate explicit enum member numeric initializers after underlying type validation.
2. Add type-checker negative fixtures for default `int` overflow, explicit unsigned range failures, and non-integral numeric tokens.
3. Update canonical grammar, reference, type-system, lowering, diagnostics, feature-status, and work-ledger docs for the implemented boundary.
4. Run focused enum diagnostics and full compiler/docs verification before rolling this task up.

## Done Criteria

- Invalid explicit enum numeric values fail during TypeSharp checking with stable `TS2201` diagnostics.
- Valid explicit enum numeric values and underlying types continue to parse, check, lower, and build.
- Docs and agent ledgers describe the completed boundary and the remaining enum backlog.
- Changes are committed and pushed after verification.
