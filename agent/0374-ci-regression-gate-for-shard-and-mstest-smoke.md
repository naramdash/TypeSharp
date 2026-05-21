# Task: ci-regression-gate-for-shard-and-mstest-smoke

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 01:21:36 +09:00
End Time: TBD

## Objective

Add a bounded GitHub Actions regression gate that proves the package-free TypeSharp compiler shard runner and the hardened MSTest SDK/Microsoft Testing Platform smoke path on the supported Windows CI environment without changing generated `net48` artifacts, generated C# 7.3 compatibility, or runtime/core package policy.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0373-roadmap-refresh-after-test-host-nuget-hardening)
- [test README](../test/README.md)
- Existing workflows under [`.github/workflows`](../.github/workflows)
- Official .NET testing and GitHub Actions documentation where needed.

## Scope

In:
- Add or update a GitHub Actions workflow for push/PR regression evidence that runs on a Windows runner with installed `dotnet` and `node`.
- Restore the hardened MSTest bridge in locked mode so `NuGet.config`, package source mapping, audit, and `packages.lock.json` are exercised in CI.
- Build the package-free main runner and four shard projects.
- Run the four shard projects in a CI-safe way that preserves isolated `test/tmp` workspaces and fails the job if any shard fails.
- Run the focused MSTest bridge smoke for `CatalogIsExposedForPackageRunners`; consider full MSTest catalog only if runtime cost is acceptable and redundant evidence is justified.
- Keep docs build coverage either in the existing docs workflow or clearly separated from the regression workflow.
- Document the workflow in `Project Policy`, `Work Ledger`, or `test/README.md` if commands or CI expectations change.

Out:
- Replacing the package-free shard runner with `dotnet test`.
- Adding xUnit.net v3.
- Changing test catalog membership or test semantics.
- Implementing compiler `references.packages` restore.
- Changing generated artifact target frameworks, runtime dependencies, or package-free Core/Runtime policy.

## Acceptance Criteria

- [ ] CI workflow exists for relevant push/PR paths and can be run manually.
- [ ] The workflow restores the MSTest bridge in locked mode through the checked-in NuGet controls.
- [ ] The package-free main/shard runner path remains the release-confidence gate and is exercised in CI.
- [ ] The MSTest bridge smoke is exercised as package-based discovery evidence.
- [ ] Docs/agent ledgers record the new CI gate and any residual exclusions.
- [ ] Verification includes local syntax/diff checks plus the strongest feasible local dry-run or command validation.

## Verification

Command: `dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal`
Expected: locked restore succeeds under root `NuGet.config`.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: package-free main runner builds.
Result: TBD

Command: shard build/run commands from `test/README.md`
Expected: all four shards pass or the workflow-equivalent local command fails.
Result: TBD

Command: `dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"`
Expected: focused MSTest smoke passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds if docs are changed.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0373 confirmed no generated-artifact baseline change after test-host NuGet hardening and selected CI regression gating as the next bounded adoption slice.

Remaining:
- Implement and verify the CI regression gate.

Blocked:
- None.
