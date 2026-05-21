# Task: enum-declaration-implementation-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 12:33:42 +09:00
End Time: TBD

## Objective

Implement the first TypeSharp-owned enum declaration slice so enum syntax, symbols, and C# 7.3-compatible lowering have a stable baseline before enum match exhaustiveness work.

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
- Parse simple TypeSharp enum declarations with stable syntax tree output.
- Bind enum declarations and report deterministic duplicate enum member diagnostics where appropriate.
- Type-check enum type annotations and same-enum value use in local expressions.
- Lower supported enum declarations and member references to C# 7.3-compatible source.
- Add parser, binder/type-checker, backend, and CLI/build smoke coverage.
- Update canonical docs and operational ledgers.

Out:
- Enum match exhaustiveness.
- Flag enums, explicit underlying types, explicit numeric member values, attributes on enum members, or imported C# enum exhaustiveness.
- Public ABI policy changes beyond ordinary CLR-visible enum lowering.
- C# preview syntax emission.

## Acceptance Criteria

- [ ] Parser accepts simple enum declarations with stable tree output.
- [ ] Binder records enum declarations and members and reports duplicate enum members deterministically.
- [ ] Type checker accepts same-enum member values where a known enum type is expected and reports mismatches for unrelated values.
- [ ] Backend lowers supported enum declarations and member references to C# 7.3-compatible source.
- [ ] CLI/build smoke compiles a generated `net48` assembly exposing an enum-backed public API.
- [ ] Docs and ledgers describe the implemented enum boundary and keep enum match exhaustiveness as follow-up.
- [ ] Relevant dotnet/docs verification and diff hygiene pass.

## Verification

Command: TBD
Expected: TBD
Result: TBD

## Handoff

Done:
- Task 0309 refreshed official source signals after literal match exhaustiveness and selected this enum declaration groundwork slice.

Remaining:
- Inspect enum-related parser/binder/type-checker/backend gaps, implement the bounded slice, verify, roll up, commit, and push.

Blocked:
- None.
