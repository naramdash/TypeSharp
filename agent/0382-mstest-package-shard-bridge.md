# Task: mstest-package-shard-bridge

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 14:35:00 +09:00
End Time: TBD

## Objective

Add a `net10.0` MSTest SDK/Microsoft Testing Platform shard bridge over the existing shared compiler test catalog so package-based `dotnet test` discovery can run the full catalog in parallel without replacing the faster package-free shard runner.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0381-roadmap-refresh-after-generic-typesharp-named-arguments](tasks-rollup.md#task-0381-roadmap-refresh-after-generic-typesharp-named-arguments)
- [test README](../test/README.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Scope

In:
- Reuse the existing `TypeSharpCompilerTestCases.All` catalog and `TestShardDefaults` shard selection model.
- Keep `MSTest.Sdk/4.2.3`, `net10.0`, and `.NET 10` `dotnet test` MTP mode as the package-based test path.
- Add package-based shard projects or equivalent project-level sharding so four `dotnet test` processes cover disjoint catalog slices.
- Keep the current package-free main and shard runners as the faster release-confidence path.
- Update lock files, `test/README.md`, CI workflow, docs ledger, and traceability as needed.

Out:
- Migrating the whole suite to xUnit.net v3 or NUnit.
- Removing the custom main/shard runners.
- Updating `MSTest.Sdk` beyond the currently pinned version unless the restore graph requires it.
- Changing generated TypeSharp artifact targets, runtime dependencies, or C# generated-source baseline.

## Acceptance Criteria

- [ ] Package-based MSTest shard execution covers all 520 catalog cases across four disjoint shards.
- [ ] The single MSTest bridge smoke still proves package-runner discovery.
- [ ] CI or documented commands can run package-free shards and package-based MSTest shards without duplicating build outputs unsafely.
- [ ] NuGet lock/source-mapping/audit controls remain valid for every MSTest package project.
- [ ] Docs and ledgers explain why MSTest SDK/MTP is used and why the package-free runner remains the fastest gate.

## Verification

Command: `dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal`
Expected: package bridge restores from the checked lock file.
Result: TBD

Command: `dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"`
Expected: package bridge discovery smoke passes and reports the 520-case catalog.
Result: TBD

Command: package-based shard run command(s)
Expected: four MSTest shard processes pass and together cover the catalog.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after docs/ledger updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0381 confirmed the current baseline and selected package-based MSTest sharding in response to the test optimization and NuGet-package question.

Remaining:
- Implement the MSTest package shard bridge and update CI/docs.

Blocked:
- None.
