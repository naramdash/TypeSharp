# Task: enum-value-bitwise-and-expression-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 16:18:40 +09:00
End Time: TBD

## Objective

Add bounded expression-level enum value bitwise AND for same-enum operands, lowering to C# `&`.

## Source Of Truth

- [tasks.md](tasks.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:
- Parse `&` as an expression operator for enum value composition.
- Type-check operands as the same enum type and infer that enum type.
- Lower accepted expressions such as `permission & Permission.Read` to C# `&`.
- Add parser, type-checker, backend, generated `net48`, and docs coverage.

Out:
- Numeric/general bitwise operators.
- `^`, `~`, shifts, compound assignment, or operator overloading.
- Flag-aware match exhaustiveness or enum flag set algebra.
- Numeric pattern algebra.
- Imported enum flag reasoning beyond same-type expression validation if existing metadata already supplies the enum type.
- Arbitrary/general computed enum member declarations beyond the existing enum initializer-local `|` slice.

## Acceptance Criteria

- [ ] Parser accepts enum value `&` expressions without changing type-intersection or pattern-and parsing.
- [ ] Type checker accepts same-enum operands and rejects mismatched or non-enum operands.
- [ ] C# backend emits valid C# 7.3-compatible `&` expressions.
- [ ] Fixtures and generated `net48` smoke cover positive and negative behavior.
- [ ] Docs and ledgers describe the narrow boundary.

## Verification

Command: TBD
Expected: Relevant compiler/docs checks pass.
Result: TBD

## Handoff

Done: 0331 roadmap refresh selected this slice.
Remaining: Implement parser/type-checker/backend/tests/docs.
Blocked: None.
