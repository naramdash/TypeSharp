# Task: logical-unsigned-shift-expressions

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 05:11:39 +09:00
End Time: TBD

## Objective

Implement bounded `>>>` logical unsigned right-shift expressions for known non-null primitive integral operands while preserving generated `net48` C# 7.3-compatible output.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0389-roadmap-refresh-after-shift-assignment-expressions](tasks-rollup.md#task-0389-roadmap-refresh-after-shift-assignment-expressions)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:
- Parse `>>>` as a value expression operator without perturbing existing `>>`/`<<` function composition, ordinary shift expressions, or `>>=` assignment parsing.
- Accept `left >>> count` only for the same known non-null primitive integral left operands and int-compatible non-null count operands used by existing shift expressions.
- Infer the same result shape as existing shifts: small left operands promote to `int`; `int`, `uint`, `long`, and `ulong` keep the left operand shape.
- Lower signed operands with explicit unsigned casts using C# 7.3-compatible syntax instead of emitting C# `>>>`.
- Add parser, type-checker, backend, generated `net48`, docs, task ledger, and traceability evidence.

Out:
- `>>>=` logical unsigned shift assignment.
- User-defined operators or imported operator overload resolution.
- Enum logical shifts, flag algebra, or imported enum numeric reasoning.
- Broad assignment target analysis.
- Changing `>>`/`<<` function composition semantics.

## Acceptance Criteria

- [ ] `>>>` parses deterministically without changing existing `>>`, `<<`, `<<=`, or `>>=` behavior.
- [ ] Known non-null primitive integral operands type-check with the existing count policy and deterministic result types.
- [ ] Nullable, non-integral, enum, bool, string, record, unsupported-count, and ambiguous composition-shaped cases report deterministic diagnostics before backend emission.
- [ ] Generated C# remains C# 7.3-compatible and generated projects still build for `net48`.
- [ ] Grammar, reference, type system, lowering, diagnostics, Feature Status, Work Ledger, task ledger, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test project builds.
Result: TBD

Command: targeted parser/type-checker/backend/generated `net48` smoke filters
Expected: logical unsigned shift coverage passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after language and ledger updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0389 rechecked official sources after shift assignment expressions, preserved the generated `net48`/C# 7.3 baseline, and selected bounded `>>>` expressions next.

Remaining:
- Implement parser, checker, backend, fixtures, smokes, and docs for `>>>`.

Blocked:
- None.
