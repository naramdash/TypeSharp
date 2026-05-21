# Task: logical-unsigned-shift-assignment-imported-indexer-targets

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 09:15:00 +09:00
End Time: TBD

## Objective

Implement bounded metadata-backed imported C# indexer `>>>=` targets with C# 7.3-compatible explicit assignment/cast lowering and a single-evaluation receiver/index-argument policy.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0395-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-member-targets](tasks-rollup.md#task-0395-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-member-targets)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Accept metadata-backed imported C# instance indexer `target[index] >>>= count` when overload resolution selects a public getter/setter pair, the indexer value type is a known non-null primitive integral value, and `count` is `byte`, `sbyte`, `short`, `ushort`, or `int`.
- Reuse existing imported indexer argument validation, numeric literal conversion, null filtering, relationship ranking, and ambiguous-candidate diagnostics where possible.
- Lower through C# 7.3-compatible explicit assignment/cast forms with no emitted C# `>>>` or `>>>=`.
- Evaluate non-trivial receivers and index arguments once before the get/assign sequence.
- Add generated `net48` C# consumer evidence and keep existing imported field/property `>>>=` behavior stable.

Out:
- Event `>>>=`.
- User-defined operators and imported operator overload resolution.
- Enum flag algebra and enum-valued shift assignment.
- TypeSharp-owned member/indexer assignment policy.
- Broad class-member body analysis.
- Static indexer support beyond ordinary C# metadata reality.
- Broad multi-dimensional or richer indexer conversion policy beyond the existing imported-indexer validation slice.

## Acceptance Criteria

- [ ] Checker accepts supported imported C# indexer `>>>=` targets and rejects missing setter, ambiguous, mismatched, unsupported count, nullable, non-integral, enum, record, and unresolved cases deterministically.
- [ ] Backend emits C# 7.3-compatible get/assign lowering without `>>>`/`>>>=` tokens and without duplicate receiver or index-argument evaluation.
- [ ] Existing local and imported field/property `>>>=` tests remain stable.
- [ ] Shared catalog, MSTest catalog exposure, docs, Work Ledger, tasks queue, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: build succeeds.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "imported logical unsigned shift assignment indexer"`
Expected: focused imported indexer `>>>=` coverage succeeds after implementation.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after ledger/spec updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0395 rechecked official language/platform/package/test/editor/CI signals, confirmed the generated `net48`/C# 7.3 baseline and existing `net10.0` MSTest SDK/MTP package bridge, and selected imported C# indexer `>>>=` lowering next.

Remaining:
- Implement imported C# indexer `>>>=` checker/backend lowering, add targeted fixtures and generated `net48` C# evidence, update docs and task ledgers, verify, commit, and push.

Blocked:
- None.
