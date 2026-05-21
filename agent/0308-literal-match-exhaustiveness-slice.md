# Task: literal-match-exhaustiveness-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 12:09:51 +09:00
End Time: TBD

## Objective

Implement literal pattern parsing plus bool and local literal-union match exhaustiveness and C# 7.3-compatible lowering.

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
- Parse literal patterns for `true`, `false`, string literals, and numeric literals in match arms.
- Type-check literal patterns against `bool` and local type-level literal unions.
- Report deterministic non-exhaustive diagnostics for missing bool and literal-union cases.
- Preserve guard behavior: guarded literal arms do not prove coverage without an unguarded cover.
- Lower supported literal matches to C# 7.3-compatible conditionals.
- Add parser, type-checker, backend, and CLI/build smoke coverage where appropriate.
- Update canonical docs and operational ledgers.

Out:
- Enum exhaustiveness.
- General pattern algebra, nested literal pattern exhaustiveness, or active-pattern extractors.
- Public ABI changes.
- C# preview syntax emission.

## Acceptance Criteria

- [ ] Parser accepts literal match patterns with stable tree output.
- [ ] Type checker covers exhaustive and non-exhaustive `bool` matches.
- [ ] Type checker covers exhaustive and non-exhaustive local literal-union matches.
- [ ] Guarded literal arms do not satisfy exhaustiveness unless an unguarded arm/discard covers the remaining set.
- [ ] Backend lowers supported literal matches to C# 7.3-compatible source.
- [ ] Tests cover positive literal matches, missing cases, and guarded-only non-coverage.
- [ ] Docs and ledgers describe the implemented boundary.
- [ ] Relevant dotnet/docs verification and diff hygiene pass.

## Verification

Command: TBD
Expected: TBD
Result: TBD

## Handoff

Done:
- Active packet created from task 0307 roadmap refresh.

Remaining:
- Inspect implementation, patch parser/checker/backend/tests/docs, verify, roll up, commit, and push.

Blocked:
- None.
