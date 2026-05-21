# Task: test-catalog-extraction-for-framework-migration

Status: In Progress
Queue: Q4
Start Time: 2026-05-22 00:28:29 +09:00
End Time: TBD

## Objective

Extract the custom `TypeSharp.Compiler.Tests` runner catalog into reusable harness types so the current main and shard runners keep identical behavior while a future MSTest SDK/Microsoft Testing Platform or xUnit.net v3 bridge can discover per-test cases without duplicating the catalog.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [test README](../test/README.md)
- [tasks-rollup.md](tasks-rollup.md#task-0369-roadmap-refresh-after-test-suite-runtime-reduction)
- Official .NET test platform, MSTest SDK, and xUnit.net v3 package guidance recorded in task 0369

## Scope

In:
- Move the `(string Name, Action Body)` test catalog and runner settings into reusable internal harness types/files.
- Keep the existing package-free console runner and four shard projects behavior unchanged.
- Preserve stable test names, filter behavior, shard selection, pass/fail output, and exit codes.
- Add focused coverage proving catalog count/name/shard selection remains stable after extraction.
- Record the follow-up package-framework adoption decision after the catalog is reusable.

Out:
- Full MSTest/xUnit migration.
- Removing the custom runner.
- Changing test semantics or fixture expectations.
- Adding new NuGet test packages before the extracted catalog shape is verified.

## Acceptance Criteria

- [ ] Test catalog is reusable outside top-level `Program.cs`.
- [ ] Main runner and shard projects still consume the same catalog with no duplicated test list.
- [ ] Existing `--filter` and `--shard` behavior remains stable.
- [ ] Follow-up MSTest SDK/Microsoft Testing Platform or xUnit.net v3 bridge decision is documented.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: build succeeds.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --nologo --verbosity quiet` and equivalent shard 1-3 commands
Expected: shard builds succeed.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "test runner shard selection"`
Expected: focused runner settings test passes.
Result: TBD

Command: parallel shard run from `test/README.md`
Expected: all catalog tests pass across shards.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0369 confirmed test framework NuGet packages are viable test-host-only tooling, but should follow catalog extraction.

Remaining:
- Extract the catalog and keep the current runner/shard behavior intact.

Blocked:
- None.
