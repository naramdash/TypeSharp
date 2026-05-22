# Task: imported-csharp-null-conditional-shift-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 23:47:00 +09:00
Source: Task 0434 roadmap refresh after imported C# null-conditional shift compound assignment member targets

## Objective

Implement imported C# null-conditional shift compound assignment indexer targets so `receiver?[index] <<= count` and `receiver?[index] >>= count` type-check and lower for readable/writable metadata-backed imported C# instance indexers with supported arguments, while preserving generated package-free `net48` artifacts and C# 7.3-compatible generated source.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Task 0434 rollup](tasks-rollup.md#task-0434-roadmap-refresh-after-imported-csharp-null-conditional-shift-compound-assignment-member-targets)

## Scope

In:

- Accept `receiver?[index] <<= count` and `receiver?[index] >>= count` only when the target resolves to a readable/writable metadata-backed imported C# instance indexer with a selected public getter/setter and supported index arguments.
- Reuse the existing primitive integral shift assignment policy for target values and counts.
- Lower accepted targets through the existing C# 7.3-compatible null-conditional indexer guard/index-capture shape with ordinary `<<=` and `>>=` in the non-null branch.
- Preserve single receiver evaluation, skipped index-argument evaluation on null receivers, and skipped `count` evaluation on null receivers.
- Add generated `net48` consumer coverage and deterministic negative checker coverage.
- Update docs, task ledgers, traceability, catalog counts, shard expectations, and package-shard MTP minimum if catalog count changes.

Out:

- User-defined operators.
- Null-conditional indexer operators beyond `<<=` and `>>=` in this slice.
- TypeSharp-owned indexer assignment policy.
- Invocation, chains, events, static targets, local binding targets, or broader class-member body analysis.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit user approval.

## Acceptance Criteria

- [ ] Checker accepts supported imported C# null-conditional indexer `<<=` and `>>=` assignment targets and rejects unsupported targets before backend emission.
- [ ] Backend emits C# 7.3-compatible guard/index-capture lowering with no C# `?[]`.
- [ ] Generated `net48` consumer coverage proves assignment, non-trivial receiver/index evaluation, skipped index/count evaluation on null receivers, and C# consumer compatibility.
- [ ] Negative coverage covers invalid operands/counts, getter-only indexers, mismatched/ambiguous indexer arguments, unresolved indexers, member/static/event/local/TypeSharp-owned targets, and still-unsupported forms.
- [ ] Docs, tasks, traceability, catalog counts, shard expectations, workflow/test docs, and Work Ledger agree.

## Verification

Command: TBD
Expected: targeted compiler tests, full package-free catalog, MSTest/MTP package shards when catalog counts change, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0434 rechecked official sources and selected this bounded implementation slice.

Remaining:

- Implement checker/backend support and focused regression coverage.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
