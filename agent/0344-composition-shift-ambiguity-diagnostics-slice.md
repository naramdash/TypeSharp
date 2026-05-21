# Task 0344: Composition Shift Ambiguity Diagnostics Slice

Status: In Progress
Priority: Q2
Source: Task 0343 roadmap refresh
Start Time: 2026-05-21 18:42:36 +09:00

## Objective

Add bounded diagnostics for value-shaped `>>` and `<<` expressions so numeric shift-looking code fails clearly instead of lowering through TypeSharp composition.

## Scope

- Preserve `>>` and `<<` as TypeSharp function composition operators.
- Detect composition expressions whose known operands are ordinary values such as numeric, bool, string, enum, or nullable values.
- Report deterministic type-checker diagnostics explaining that numeric shifts and shift assignment are not implemented and `>>`/`<<` are composition syntax.
- Keep valid function composition fixtures and generated C# lowering unchanged.
- Update grammar/reference/type-system/lowering/diagnostics/feature-status docs with the explicit ambiguity boundary.

## Out Of Scope

- Numeric shift expression support.
- Shift assignment `<<=` and `>>=`.
- User-defined operators or C# operator overload policy.
- Composition type inference for higher-order function values beyond the current lowering-compatible surface.
- Flag-aware enum algebra, numeric pattern algebra, broad attribute target validation, and richer pattern algebra.

## Done Criteria

- Positive composition fixtures still pass unchanged.
- Negative type-checker fixture covers numeric literal and known local value `>>`/`<<` ambiguity diagnostics.
- Backend composition fixture remains unchanged.
- Docs and ledgers describe the ambiguity boundary and remaining shift backlog.
- Run focused type-checker/backend/parser checks, full compiler test runner, docs build, and `git diff --check`.
