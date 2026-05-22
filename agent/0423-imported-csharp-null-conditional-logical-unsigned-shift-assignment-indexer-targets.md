# Task: imported-csharp-null-conditional-logical-unsigned-shift-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:20:00 +09:00
End Time: TBD

## Objective

Implement the bounded imported C# null-conditional logical unsigned shift assignment indexer slice: `receiver?[index] >>>= count`.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md](tasks-rollup.md)
- [traceability.md](traceability.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)

## Scope

In:
- Accept `receiver?[index] >>>= count` when the receiver resolves to a metadata-backed imported C# instance indexer with selected public getter and setter.
- Reuse the existing logical unsigned shift assignment primitive target and int-compatible count policy.
- Reuse existing imported indexer argument validation and ranking for supported argument lists.
- Preserve single receiver and index argument evaluation.
- Skip index argument and count evaluation when the receiver is null.
- Lower to C# 7.3-compatible explicit null/index guards plus the existing unsigned-cast assignment shape; do not emit C# `?[]`, `>>>`, or `>>>=`.
- Add focused generated `net48` C# consumer coverage and negative checker coverage.
- Update docs, task ledger, traceability, catalog counts, and shard expectations if coverage changes the shared catalog.

Out:
- Already implemented `receiver?.Member >>>= count`.
- Other null-conditional compound operators.
- Event `>>>=`.
- Static, local, and TypeSharp-owned null-conditional targets.
- User-defined operators.
- Invocation, chains, nullable receiver lifting, and broader TypeSharp-owned member/indexer assignment policy.
- Task 0401 GitHub Actions CI fix without explicit approval.
- Test-host package replacement or generated-project NuGet restore changes.

## Acceptance Criteria

- [ ] Checker accepts the bounded imported C# null-conditional indexer `>>>=` target and rejects unsupported targets deterministically.
- [ ] Backend emits C# 7.3-compatible guard/cast lowering with single receiver/index evaluation and skipped index/count evaluation on null receivers.
- [ ] Generated `net48` C# consumer coverage demonstrates the accepted shape.
- [ ] Negative checker coverage covers readonly/mismatched/unsupported/null-conditional indexer `>>>=` cases appropriate to this slice.
- [ ] Existing imported indexer `>>>=`, null-conditional simple assignment, member `>>>=`, and null-conditional read behavior remains covered.
- [ ] Docs, task ledger, traceability, and shard counts are updated if needed.
- [ ] Verification commands pass.

## Verification

Command: TBD
Expected: focused compiler tests, catalog stability tests, MSTest/package shard checks as needed, docs build, and diff checks pass
Result: TBD

## Handoff

Done:
- TBD

Remaining:
- TBD

Blocked:
- Task 0401 remains blocked pending explicit approval for the CI implementation fix.
