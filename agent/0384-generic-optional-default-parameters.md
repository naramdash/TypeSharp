# Task: generic-optional-default-parameters

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 16:15:00 +09:00
End Time: TBD

## Objective

Extend TypeSharp-owned trailing literal default parameters to bounded generic direct `fun` declarations while preserving generated `net48` C# 7.3-compatible optional-parameter metadata.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0383-roadmap-refresh-after-mstest-package-shards](tasks-rollup.md#task-0383-roadmap-refresh-after-mstest-package-shards)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

## Scope

In:
- Remove the current generic-function diagnostic only for TypeSharp-owned direct generic `fun` declarations whose defaulted parameters form a trailing suffix and do not use `params`.
- Support defaulted generic functions when each defaulted parameter has an explicit concrete TypeSharp type that does not reference a type parameter and the default expression is a supported literal: string, numeric, `true`, `false`, or `null`.
- Validate direct explicit and inferred generic calls that omit the defaulted suffix using the existing bounded generic inference/substitution shapes.
- Validate first-argument pipeline calls that omit supported generic defaulted non-piped suffix arguments.
- Lower accepted declarations to C# 7.3-compatible optional parameter metadata and accepted calls to ordinary positional C# calls.
- Add focused positive/negative type-checker fixtures, backend snapshot, generated `net48` CLI/C# consumer smoke, docs updates, catalog count updates, and MSTest bridge count evidence.

Out:
- Default values whose parameter type directly or transitively references a generic type parameter.
- Combining generic defaults with `params`.
- Ambient/`extern` signatures, constructors, delegates, union cases, function types, lambdas, higher-order values, partial application, currying, overload ranking, imported C# behavior changes, or broader generic type-constructor unification.

## Acceptance Criteria

- [ ] TypeSharp-owned generic direct `fun` declarations can declare supported trailing concrete defaults.
- [ ] Explicit and inferred generic direct calls may omit the supported defaulted suffix.
- [ ] First-argument generic pipeline calls may omit supported defaulted non-piped suffix arguments.
- [ ] Unsupported generic default shapes still report deterministic `TS2201` diagnostics before C# emission.
- [ ] Generated C# keeps `net48` and C# 7.3-compatible optional metadata and compiles in the generated assembly smoke.
- [ ] Docs and ledgers describe the new bounded generic optional/default behavior and remaining exclusions.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler/test harness builds.
Result: TBD

Command: targeted fixture/backend/generated smoke filters
Expected: new generic optional/default coverage passes.
Result: TBD

Command: `dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet`
Expected: MSTest catalog count bridge passes after catalog update.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after docs updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0383 confirmed the baseline after MSTest package shards and selected this bounded generic optional/default parameter slice.

Remaining:
- Implement the compiler/backend/docs/test changes.

Blocked:
- None.
