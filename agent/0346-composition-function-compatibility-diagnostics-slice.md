# Task 0346: Composition Function Compatibility Diagnostics Slice

Status: In Progress
Priority: Q2
Source: Task 0345 roadmap refresh
Start Time: 2026-05-21 19:04:52 +09:00

## Objective

Add bounded diagnostics for direct named-function `>>` and `<<` composition so incompatible unary function pairs fail clearly before generated C# emission.

## Scope

- Preserve current `>>` and `<<` parsing and C# 7.3-compatible composition lowering.
- Track enough function signature information for TypeSharp-declared functions with exactly one parameter and a known return type.
- For direct named-function composition such as `f >> g` and `g << f`, validate that the left function return type can flow into the right function's first parameter.
- Report deterministic `TS2201` diagnostics for known incompatible function pairs.
- Keep existing positive composition parser/backend fixtures passing unchanged.
- Add negative type-checker fixture coverage for incompatible named-function composition.
- Update grammar/reference/type-system/lowering/diagnostics/feature-status docs with the limited direct-function boundary.

## Out Of Scope

- Higher-order function type inference beyond direct named function declarations.
- Curried, partial-application, multi-parameter, generic, overloaded, imported C#, or function-valued `let` composition inference.
- Numeric shifts, shift assignment, user-defined operators, and broader operator overload policy.
- TypeSharp member assignment policy, class-member body analysis, flag-aware enum algebra, numeric pattern algebra, and richer pattern algebra.

## Done Criteria

- Positive composition fixtures still pass unchanged.
- Negative type-checker fixture covers an incompatible direct named-function pair.
- Backend composition fixture remains unchanged.
- Docs and ledgers describe the bounded direct named-function compatibility boundary and remaining higher-order backlog.
- Run focused type-checker/backend/parser checks, full compiler test runner, docs build, and `git diff --check`.
