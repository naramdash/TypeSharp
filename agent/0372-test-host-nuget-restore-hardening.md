# Task: test-host-nuget-restore-hardening

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 01:03:29 +09:00
End Time: TBD

## Objective

Clarify whether the existing `net10.0` MSTest SDK/Microsoft Testing Platform bridge remains the most practical general-purpose NuGet test host for TypeSharp, compare it against xUnit.net v3 where it can reuse the extracted catalog, and implement restore controls for the selected test-host package path without adding generated-artifact package dependencies or changing the package-free main/shard runner path.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0371-roadmap-refresh-after-mstest-catalog-bridge)
- [test README](../test/README.md)
- Official NuGet PackageReference lock file, package source mapping, audit, and restore documentation
- Official MSTest SDK/Microsoft Testing Platform and xUnit.net v3 package documentation

## Scope

In:
- Record the package-selection rationale for `net10.0` test-host tooling: why the current `MSTest.Sdk/4.2.3` NuGet bridge was added, when xUnit.net v3 would be more general-purpose, and whether adding an xUnit bridge now would improve TypeSharp's release-confidence or ecosystem path.
- Inspect how `MSTest.Sdk/4.2.3` is restored as an MSBuild SDK and whether `packages.lock.json` can capture the graph for `test/TypeSharp.Compiler.Tests.MSTest`.
- If xUnit.net v3 is adopted or prototyped, keep it as a separate test-host bridge over `TypeSharpCompilerTestCases.All`, not a replacement for the extracted catalog or shard runner.
- Add lock/source-mapping/audit controls where they work with this SDK-style package bridge.
- If a normal NuGet control does not apply to MSBuild SDK restore, document the exception and the compensating control.
- Keep all controls test-host-only; generated `net48` projects, `TypeSharp.Core`, `TypeSharp.Runtime`, and the package-free main/shard runners must remain package-free.
- Update docs and test README commands if restore invocation changes.

Out:
- Changing `MSTest.Sdk` version unless the current pinned version cannot be restored safely or the package-selection review chooses a different pinned test-host package.
- Replacing the package-free main/shard release-confidence runner with a package-based runner.
- Migrating CI to `dotnet test`.
- Implementing compiler `references.packages` restore.
- Changing generated artifact target frameworks or runtime dependencies.

## Acceptance Criteria

- [ ] The reason TypeSharp uses a test-host NuGet package, and why it does not put that package into generated `net48` artifacts or the fast shard runner, is documented.
- [ ] MSTest SDK/MTP and xUnit.net v3 are compared against TypeSharp's needs: `net10.0` host support, MTP/`dotnet test` discovery, catalog reuse, execution speed, restore controllability, and maintenance cost.
- [ ] The MSTest bridge restore graph and MSBuild SDK restore behavior are understood and recorded.
- [ ] Supported restore controls are implemented and verified, or unsupported controls have a documented exception.
- [ ] Generated `net48` artifact projects and package-free custom runners remain unaffected.
- [ ] Verification includes restore/build/test checks for the MSTest bridge and docs/diff checks.

## Verification

Command: `dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj`
Expected: restore succeeds under the selected controls.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet`
Expected: build succeeds.
Result: TBD

Command: `dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"`
Expected: focused bridge smoke passes.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: package-free main runner still builds.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0371 confirmed no platform baseline change and selected test-host NuGet package selection plus restore hardening before broader CI adoption.

Remaining:
- Inspect and harden the MSTest bridge restore path.

Blocked:
- None.
