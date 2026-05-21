# Task: null-conditional-assignment-imported-member-targets

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 10:55:00 +09:00
End Time: TBD

## Objective

Implement the first bounded C# 14-inspired null-conditional assignment slice for metadata-backed imported C# instance member targets while keeping generated C# compatible with `net48` and C# 7.3.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0397-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-indexer-targets](tasks-rollup.md#task-0397-roadmap-refresh-after-logical-unsigned-shift-assignment-imported-indexer-targets)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

## Scope

In:
- Add the minimal `?.` token/syntax support needed for null-conditional member assignment targets.
- Support statement/expression assignment forms like `receiver?.Member = value` when `receiver` is a nullable or reference-like imported C# instance receiver and `Member` resolves to a writable public imported C# field or property.
- Type-check left and right operands with the same imported member assignment compatibility and nullability constraints as ordinary imported member assignment, while skipping right-side evaluation when the receiver is null.
- Lower to C# 7.3-compatible explicit null guards with single evaluation of non-trivial receiver expressions.
- Add parser, type-checker, backend, generated `net48` C# consumer, diagnostics, docs, shared catalog, MSTest bridge count, Work Ledger, and traceability evidence.

Out:
- Null-conditional indexer assignment with `?[]`.
- Null-conditional compound assignment, `++`, or `--`.
- Event assignment, local binding assignment, TypeSharp-owned member assignment, and arbitrary assignable expression support.
- Switching generated artifacts from `net48`, using C# 14 syntax in generated code, or adding runtime/NuGet dependencies.

## Acceptance Criteria

- [ ] `?.` lexes/parses in the bounded assignment target shape without disturbing existing nullable type syntax.
- [ ] Supported imported C# instance field/property targets check and lower correctly, including non-trivial receiver single evaluation and skipped value evaluation on null receivers.
- [ ] Unsupported targets produce deterministic diagnostics before backend emission.
- [ ] Generated C# remains C# 7.3-compatible and package-free for `net48`.
- [ ] Shared catalog and MSTest bridge counts are updated.
- [ ] Grammar, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, tasks queue, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test host builds.
Result: TBD

Command: focused parser/type-checker/backend/generated consumer filters for null-conditional assignment
Expected: focused tests pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`
Expected: full package-free catalog passes.
Result: TBD

Command: `dotnet test test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress`
Expected: MSTest package bridge smoke passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0397 rechecked official language/platform/package/test/editor/CI signals after imported C# indexer `>>>=`, reaffirmed the package-free generated `net48`/C# 7.3 baseline, reaffirmed the existing `net10.0` `MSTest.Sdk/4.2.3` MTP package bridge and shard projects, and selected this C# 14-inspired bounded slice.

Remaining:
- Implement lexer/parser/syntax, type-checker, backend, fixtures, docs, and ledger updates for the bounded `receiver?.Member = value` imported member slice.

Blocked:
- None.
