# Task: logical-unsigned-shift-assignment-imported-member-targets

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 07:20:00 +09:00
End Time: TBD

## Objective

Implement bounded logical unsigned shift assignment `>>>=` for imported C# member targets using C# 7.3-compatible explicit assignment/cast lowering with a single-evaluation receiver policy.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0393-roadmap-refresh-after-logical-unsigned-shift-assignment-expressions](tasks-rollup.md#task-0393-roadmap-refresh-after-logical-unsigned-shift-assignment-expressions)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Accept `target.member >>>= count` for supported metadata-backed imported C# field/property member targets when the target type is a known non-null primitive integral value and the count is `byte`, `sbyte`, `short`, `ushort`, or `int`.
- Preserve the existing local `>>>=` policy and all ordinary imported assignment behavior.
- Lower signed targets through explicit unchecked unsigned casts and ordinary `>>`, never emitted C# `>>>` or `>>>=`.
- Preserve C# compound-assignment receiver behavior by evaluating non-trivial receivers once before getter/setter use.
- Add focused type-checker diagnostics, backend snapshots, generated `net48` build evidence, and C# consumer evidence.

Out:
- Indexer `>>>=` targets.
- Event `>>>=` targets.
- User-defined operators or imported operator overload resolution.
- Enum flag algebra or enum-valued shift assignment.
- TypeSharp member assignment policy and broad class-member body analysis.
- Changing the selected `net10.0` MSTest SDK/MTP package bridge.

## Acceptance Criteria

- [ ] Type checker accepts supported imported C# instance/static field and property member access `>>>=` targets and rejects unsupported nullable, non-integral, enum, record, indexer, event, or unsupported count cases with deterministic diagnostics.
- [ ] Backend emits C# 7.3-compatible explicit assignment/cast code with no generated `>>>` or `>>>=`.
- [ ] Backend does not duplicate a non-trivial imported member receiver expression while lowering the read/shift/write sequence.
- [ ] Existing local `>>>=`, `>>>`, `<<=`, `>>=`, bitwise assignment, and imported ordinary assignment fixtures remain stable.
- [ ] Docs, task ledger, and traceability record the implemented boundary and remaining follow-ups.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test project builds.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"`
Expected: updated type-checker fixtures pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"`
Expected: updated backend fixtures pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles logical unsigned shift assignment imported member API"`
Expected: generated `net48` and C# consumer evidence pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`
Expected: full shared catalog passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after documentation updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0393 confirmed no official-source baseline change and selected imported C# member `>>>=` lowering as the next bounded slice.

Remaining:
- Inspect current imported assignment/member metadata paths, implement checker/backend support, add fixtures and generated `net48` evidence, update docs/ledgers, verify, commit, and push.

Blocked:
- None.
