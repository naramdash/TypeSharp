# Task: local-multiplicative-compound-assignment-expressions

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 23:59:00 +09:00
Source: Task 0436 roadmap refresh after imported C# null-conditional shift compound assignment indexer targets

## Objective

Implement the next bounded C# compound-assignment foundation slice: local mutable `*=`, `/=`, and `%=` expressions over known non-null primitive integral numeric values, while preserving generated package-free `net48` artifacts and C# 7.3-compatible generated source.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0436 rollup](tasks-rollup.md#task-0436-roadmap-refresh-after-imported-csharp-null-conditional-shift-compound-assignment-indexer-targets)

## Scope

In:

- Add lexer/parser support for `*=`, `/=`, and `%=` as assignment operators.
- Type-check local identifier targets only when they resolve to `let mut` bindings.
- Reuse the existing bounded primitive integral numeric policy shape used by additive compound assignment: operands must be known non-null primitive integral numeric values, and the result must be assignable back to the mutable local target type.
- Lower accepted local assignments to ordinary C# 7.3-compatible compound assignment operators.
- Add focused positive and negative coverage, generated `net48` C# consumer evidence, catalog/shard-count updates if the shared catalog changes, and docs updates for the grammar/type-system/lowering boundary.

Out:

- Imported C# member/indexer `*=`, `/=`, or `%=` assignment targets.
- Null-conditional member/indexer multiplicative compound assignment targets.
- Floating-point or decimal policy expansion beyond the current bounded primitive integral numeric assignment policy.
- User-defined operators, checked overflow policy changes, events, static targets, TypeSharp-owned members/indexers, and broader class-member body analysis.
- Implementing the Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] `*=`, `/=`, and `%=` parse as assignment operators.
- [ ] Mutable local primitive integral numeric targets type-check for accepted operand/result shapes.
- [ ] Immutable locals, unsupported targets, nullable/non-integral operands, and non-assignable promoted results report deterministic diagnostics before backend emission.
- [ ] Accepted forms lower to C# 7.3-compatible generated source and compile in generated `net48` consumer evidence.
- [ ] Docs and operational ledgers reflect the implemented boundary and remaining follow-ups.

## Verification

Command: TBD
Expected: focused parser/checker/backend/generated-consumer coverage, package-free catalog/shard checks as needed, compiler build, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0436 rechecked official signals and selected local multiplicative compound assignment as the next bounded implementation slice.

Remaining:

- Implement and verify the local `*=`, `/=`, and `%=` slice.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
