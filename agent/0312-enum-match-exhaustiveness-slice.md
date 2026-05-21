# Task: enum-match-exhaustiveness-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 13:01:26 +09:00
End Time: TBD

## Objective

Implement enum match exhaustiveness over TypeSharp-owned enum declarations now that enum syntax, symbols, type checking, and C# lowering exist.

## Source Of Truth

- [tasks.md](tasks.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)

## Scope

In:
- Type-check enum match arms over TypeSharp-owned enum types and report deterministic missing member diagnostics.
- Treat unguarded `_` as covering remaining enum members.
- Keep guarded enum member arms non-covering unless a later unguarded arm or discard covers the remainder.
- Lower supported enum matches to C# 7.3-compatible ordered enum comparisons.
- Add type-checker negative/positive, backend, and CLI/build smoke coverage.
- Update canonical docs and operational ledgers.

Out:
- Imported C# enum exhaustiveness.
- Flag enums, explicit underlying types, explicit numeric member values, enum member attributes, and `[Flags]` semantics.
- Richer pattern algebra beyond the existing guarded-arm/discard behavior.
- C# preview syntax.

## Acceptance Criteria

- [ ] Type checker reports non-exhaustive TypeSharp enum matches with missing member names.
- [ ] Unguarded enum member arms and `_` discard interact with guards consistently with nominal, bool, and local type-level union matches.
- [ ] Backend lowers enum matches to C# 7.3-compatible source.
- [ ] CLI/build smoke compiles generated `net48` enum match code.
- [ ] Docs and ledgers describe enum match exhaustiveness as implemented while preserving remaining enum backlog boundaries.
- [ ] Relevant dotnet/docs verification and diff hygiene pass.

## Verification

Command: TBD
Expected: TBD
Result: TBD

## Handoff

Done:
- Task 0310 implemented simple TypeSharp-owned enum declarations and ordinary C# enum lowering.
- Task 0311 refreshed official source signals and selected this enum match exhaustiveness slice.

Remaining:
- Implement checker/backend support, add focused fixtures and smoke coverage, update docs, verify, roll up, commit, and push.

Blocked:
- None.
