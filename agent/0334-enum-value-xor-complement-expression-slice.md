# Task: enum-value-xor-complement-expression-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 16:41:48 +09:00
End Time: TBD

## Objective

Add expression-level same-enum value `^` and unary `~` over enum values with C# 7.3-compatible lowering.

## Source Of Truth

- [agent/tasks.md](tasks.md)
- [agent/tasks-rollup.md](tasks-rollup.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

## Scope

In:

- Parse expression-level `^` and unary `~`.
- Type-check `^` operands as known non-null values of the same enum type.
- Type-check unary `~` operand as a known non-null enum value.
- Lower accepted expressions to C# `^` and `~`.
- Add parser, type-checker positive/negative, backend, generated `net48`, docs, and ledger coverage.

Out:

- Numeric/general bitwise operators.
- Shift operators and compound assignment.
- Flag-aware match exhaustiveness or pattern algebra.
- Imported enum flag algebra beyond same-type validation.
- Arbitrary/general computed enum member declarations.

## Acceptance Criteria

- [ ] Parser accepts `favorite ^ Color.Blue` and `~favorite` in expression positions.
- [ ] Type checker infers the enum type for valid same-enum `^` and unary `~` expressions.
- [ ] Type checker reports deterministic diagnostics for mixed enum, non-enum `^`, and non-enum `~`.
- [ ] C# backend emits C# 7.3-compatible `^` and `~` expressions.
- [ ] Generated `net48` compile and C# consumer smokes cover the new operators.
- [ ] Canonical docs and agent ledgers describe the completed boundary.

## Verification

Command:
TBD

Expected:
TBD

Result:
TBD

## Handoff

Done:
TBD

Remaining:
Implement the scoped compiler, fixture, docs, and ledger changes.

Blocked:
None.
