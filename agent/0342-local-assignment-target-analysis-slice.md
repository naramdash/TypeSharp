# Task 0342: Local Assignment Target Analysis Slice

Status: In Progress
Priority: Q2
Source: Task 0341 roadmap refresh
Start Time: 2026-05-21 18:21:53 +09:00

## Objective

Add bounded type-checker diagnostics for TypeSharp local assignment targets so `let mut` controls identifier mutation before generated C# emission.

## Scope

- Track local value mutability in the type-checker scope.
- Validate identifier assignment targets for `=`, `+=`, `-=`, `|=`, `&=`, and `^=`.
- Reject assignment to immutable local bindings and function parameters.
- Validate simple assignment RHS compatibility for known local target types, including nullability and structural checks.
- Validate bitwise compound assignment compatibility for known local enum, integral, and bool targets.
- Diagnose obvious non-assignable local expression targets such as literals, calls, and binary expressions.
- Leave imported C# member, indexer, static member, and event assignment on the existing metadata-backed interop validator path.

## Out Of Scope

- Shift assignment and shift expression syntax.
- User-defined operators and C# 14 explicit compound assignment operators.
- General class-member body analysis beyond the currently checked TypeSharp function/block path.
- Broad .NET attribute target validation.
- Flag-aware enum match algebra, numeric pattern algebra, imported enum flag reasoning, and richer pattern algebra.

## Done Criteria

- Compiler tracks `let mut` mutability without changing existing inference for values and functions.
- Positive fixture covers mutable local simple and bitwise compound assignments.
- Negative fixture covers immutable local assignment, parameter assignment, invalid target expression, mismatched simple assignment, and invalid bitwise compound assignment.
- Existing imported C# assignment smokes still pass.
- Docs and ledgers describe the narrowed implemented boundary and remaining assignment backlog.
- Run focused type-checker fixtures, backend/build smokes affected by assignment, full compiler test runner, docs build, and `git diff --check`.
