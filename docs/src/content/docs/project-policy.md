---
title: Project Policy
description: Architecture, dependency, target framework, regression, feature review, parser fixture, and release policy for TypeSharp.
---

This is the canonical docs ledger for project policy that is too implementation-facing for the user guides but still durable enough to keep in the public docs site.

## Architecture

TypeSharp is organized around a compiler pipeline that preserves source information until diagnostics, lowering, generated C# source, and generated `net48` artifacts can be verified.

```text
source files
  -> lexer/parser
  -> syntax tree
  -> binder/name resolver
  -> semantic model
  -> type checker/inference
  -> lowering
  -> TypeSharp IR
  -> backend
       -> C# 7.3-compatible source for MVP
       -> IL assembly for net48 later
  -> runtime library
  -> .NET Framework 4.8 execution
```

Project separation:

- `TypeSharp.Compiler`: lexer, parser, binder, semantic model, checker, lowering, backend interfaces, metadata reading, diagnostics.
- `TypeSharp.Core`: user-facing `net48` public types such as `Option<T>`, `Result<T,E>`, and `Unit`.
- `TypeSharp.Runtime`: generated-code helper surface for unions, pattern matching, equality, hashing, and async.
- `TypeSharp.Cli`: manifest loading, commands, generated project build/run orchestration.
- `TypeSharp.LanguageServer`: LSP host that reuses compiler diagnostics and semantic model.
- `vscode/typesharp`: VS Code extension, language registration, TextMate grammar, LSP client, formatter integration.
- `test/TypeSharp.Compiler.Tests`: compiler, CLI, runtime, interop, backend, diagnostics, host, and docs smoke suite.

Host target policy:

- Generated assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` target `net48`.
- Compiler, CLI, and language server hosts may use a modern .NET runtime.
- The primary CLI distribution channel is the `TypeSharp.Tool` NuGet .NET tool, installed with `dotnet tool install --global TypeSharp.Tool`.
- MSI/EXE installers are optional convenience work, not the default developer install path.
- The host runtime target is not the success criterion. The generated artifact and runtime/core `net48` compatibility are.
- Host/runtime/source-backend projects stay separated so modern tooling dependencies do not leak into generated runtime deployment.

Backend strategy:

- MVP backend emits C# 7.3-compatible source and builds a generated `net48` project.
- Direct IL emission remains Stable Backlog.
- Backend abstraction must allow future assembly artifacts without rewriting the language frontend.
- Generated source shape and generated public metadata are contracts and need regression coverage.

Risk controls:

| Risk | Control |
| --- | --- |
| .NET Framework metadata cannot represent every TypeSharp feature. | Keep compile-time-only features out of public ABI and report boundary diagnostics. |
| Structural types and overloads can make calls unpredictable. | Prefer nominal resolution and require annotations when candidates are ambiguous. |
| Generated C# diagnostics can be confusing. | Catch known errors before emission and preserve source spans. |
| Runtime helper churn can break generated assemblies. | Use the runtime ABI policy in [.NET Interop](../dotnet-interop/). |
| Preview feature drift can destabilize the language. | Use [Feature Status](../feature-status/) and explicit preview gates. |

Parallel execution policy:

- Source-file parse and per-file semantic validation can run in parallel after source discovery, reference resolution, and metadata loading have completed.
- Diagnostics must still be appended in deterministic source order so CLI, LSP, fixture, and JSON output remain stable.
- Cross-file module graph validation runs after all parse-clean source modules have been collected.
- Generated C# project emission and external `dotnet build` orchestration remain ordered unless a change proves output paths, generated project files, and external process logs cannot conflict.
- The main `TypeSharp.Compiler.Tests` runner remains the package-free full release-confidence path. Shard projects may link the same runner and execute disjoint ordinal slices in parallel when shared generated inputs use process-safe caches and every test keeps a unique workspace under `test/tmp`.
- `TypeSharp.Compiler.Tests.MSTest` is a `net10.0` MSTest SDK/Microsoft Testing Platform bridge over `TypeSharpCompilerTestCases.All`. It exists for `dotnet test` discovery and ecosystem integration, not as the fastest release gate; CI runs its four package shard assemblies with MTP `--test-modules` and `--max-parallel-test-modules`, while the package-free shard runner remains the release-confidence path.
- The shared catalog currently asserts 586 package-free cases. The MTP package-shard gate uses `--minimum-expected-tests 590` because each of the four shard assemblies also contributes one `CatalogIsExposedForPackageRunners` bridge smoke, so the package-based discovery floor is 586 catalog cases plus four bridge smokes.
- Test framework packages are test-host tooling only. They must reuse the extracted catalog, keep generated `net48` artifacts package-free, and stay pinned and documented. The MSTest bridge introduced the first test-host package SDK; the package-selection follow-up kept MSTest SDK/MTP as the selected bridge, kept xUnit.net v3 as a future candidate, and added lock/source-mapping/audit posture before broader CI adoption. Future framework adoption must preserve the same catalog coverage rather than duplicating the list.

Test-host NuGet package selection:

| Candidate | Fit | TypeSharp decision |
| --- | --- | --- |
| `MSTest.Sdk/4.2.3` | Microsoft-supported SDK package, defaults to Microsoft Testing Platform, uses the repo's `.NET 10` `dotnet test` MTP mode, and exposes the extracted catalog with minimal bridge code. | Selected for the package-based discovery bridge. It is pinned in `Project Sdk`, checked by `packages.lock.json` for its generated adapter/framework graph, covered by root NuGet source controls, and split into package-based shard projects for parallel `dotnet test`/MTP coverage. NuGet listed `4.2.3` as the current package version when rechecked on 2026-05-23, and MTP `--test-modules` runs the four package shard assemblies in parallel with a 590-test minimum: 586 shared catalog cases plus one bridge smoke in each shard assembly. The project uses NuGet packages at the test-host boundary, not in generated `net48` artifacts. |
| `xunit.v3` / `xunit.v3.mtp-v2` | Viable general-purpose .NET test framework with native Microsoft Testing Platform support and broad ecosystem familiarity. NuGet listed stable `xunit.v3` 3.2.2, while the flat-container feed exposed newer prerelease 4.0.0-pre.108, when package signals were rechecked. | Keep as a future bridge candidate. Adding it now would duplicate the existing `net10.0` package-host coverage without improving the measured release-confidence path or generated `net48` compatibility; if adopted, it must be a separate bridge over `TypeSharpCompilerTestCases.All`, not a replacement catalog. |
| `NUnit` / `NUnit3TestAdapter` | Established .NET test framework with `dotnet test` support through the NUnit adapter and MTP bridge options. NuGet listed NUnit 4.6.1 and NUnit3TestAdapter 6.2.0 when package signals were rechecked. | Keep as a future bridge candidate. Adding it now would duplicate the current extracted catalog evidence and add another adapter/package graph without changing generated `net48` compatibility or the faster package-free shard gate. |

Why the package boundary is split:

- Test projects may use NuGet because they are developer/CI host tooling and can be locked, source-mapped, audited, restored into a repo-local cache, and excluded from generated user artifacts.
- Generated TypeSharp assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` stay package-free because they are the user deployment contract for ordinary `net48` hosts. Pulling test or restore packages into that path would change deployment shape and require the full compiler restore/security gate below.
- A second test framework such as xUnit.net v3 or NUnit is useful only if it catches a distinct class of issues. Over the same extracted catalog, it currently adds restore/runtime cost and another adapter surface without improving the package-free shard runner or generated `net48` compatibility evidence.

Test-host NuGet restore controls:

- Root `NuGet.config` clears inherited package sources, uses `nuget.org` as the package and audit source, maps every current MSTest bridge package prefix to that source, and sends restored packages to repo-local `.nuget/packages` so package source mapping is not bypassed by a user-wide global package cache.
- `TypeSharp.Compiler.Tests.MSTest` enables `RestorePackagesWithLockFile`, checks in `packages.lock.json`, enables NuGet audit for direct and transitive packages, and enables `RestoreLockedMode` when `ContinuousIntegrationBuild=true`.
- The lock file captures the package graph generated by `MSTest.Sdk/4.2.3`: direct `MSTest.TestAdapter` and `MSTest.TestFramework` plus their Microsoft Testing Platform transitives. The `MSTest.Sdk` MSBuild SDK package itself is restored before the project graph and is not listed as a normal lock-file dependency; the compensating controls are the exact `Project Sdk="MSTest.Sdk/4.2.3"` pin, root package source mapping for `MSTest.*`, repo-local package cache, and locked restore for the emitted graph.
- `.github/workflows/regression.yml` exercises the package-free four-shard runner as the Windows CI release-confidence path and the MSTest bridge as a focused package-based discovery smoke. It also runs the four MSTest package shard assemblies through `dotnet test --test-modules` with MTP module-level parallelism and a 590-test minimum, matching 586 shared catalog cases plus four shard-local bridge smokes. Full single-project MSTest catalog execution remains optional unless its extra runtime provides distinct evidence beyond the shard runner.

## Dependency And Target Policy

Core rule: generated TypeSharp assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` must be referenceable from ordinary `net48` projects without hidden package requirements.

Distribution rule: CLI delivery should follow the developer ecosystem. The CLI is a NuGet .NET tool package; generated artifacts remain ordinary `net48` outputs. Runtime/Core dependency handling should prefer the least surprising user flow that preserves version alignment and deterministic `net48` builds.

Current inventory:

| Artifact | Target | External NuGet | Notes |
| --- | --- | --- | --- |
| generated TypeSharp project | `net48` | None | `typesharp build` emits an offline-friendly generated C# project. |
| `TypeSharp.Core` | `net48` | None | User-facing `Option<T>`, `Result<T,E>`, `Unit` surface. |
| `TypeSharp.Runtime` | `net48` | None | Compiler-generated helper surface with no host-specific framework dependency. |
| `TypeSharp.Compiler` | modern .NET host | None | Host-side compiler implementation. |
| `TypeSharp.Cli` | modern .NET host | None | Depends on compiler project reference. |
| `TypeSharp.Compiler.Tests` | modern .NET host | None | Project references and linked core sources. |
| `TypeSharp.Compiler.Tests.MSTest` | modern .NET test host | `MSTest.Sdk/4.2.3` | Test-host-only MTP bridge over `TypeSharpCompilerTestCases.All`; not referenced by generated artifacts. |
| `docs` | Astro static site | `astro`, `@astrojs/starlight`, `typescript` | Documentation only; locked by `docs/package-lock.json` and not deployed with generated `net48` artifacts. |

Compatibility audit:

- `TypeSharp.Core` and `TypeSharp.Runtime` project files target `net48`.
- Core/runtime projects do not use `PackageReference` or `packages.config`.
- Static scans guard against obvious .NET 5+ or package-backed APIs in core/runtime source.
- `dotnet build` for core/runtime remains the primary compatibility check.
- `docs` dependencies are installed with `npm ci` and verified with `npm run build`.

Future dependency gate:

- package id and version,
- direct or transitive role,
- `net48` asset availability,
- license,
- runtime deployment shape for ASP.NET/WCF/worker hosts,
- checksum, lock file, or signing strategy,
- no-dependency alternative considered.

Target framework rule:

- The broad compatibility profile is `net48`.
- `net481` may be used only when the deployment profile guarantees a sufficiently new .NET Framework installation.
- Older profiles such as `net462` require a separate compatibility profile and explicit cost review.
- Compiler/CLI/LSP modern .NET targets must not be confused with generated runtime targets.

Official .NET/NuGet/VS Code and .NET test-platform ecosystem sources were refreshed and reaffirmed on 2026-05-23 after the recent null-conditional assignment, extension-property, multiplicative compound-assignment, checked/unchecked overflow, and imported C# user-defined multiplicative operator work:

- Microsoft Learn [target framework monikers](https://learn.microsoft.com/en-us/dotnet/standard/frameworks): `net48` and `net481` are .NET Framework TFMs, and `.NET Framework 4.8.1` maps to `net481`.
- Microsoft Learn [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies) and the [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework): `.NET Framework 4.8.1` is the latest .NET Framework version; Windows 10 22H2 includes 4.8 and can install 4.8.1, while recent Windows 11 releases include 4.8.1.
- .NET downloads for [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) and [.NET 11.0](https://dotnet.microsoft.com/en-us/download/dotnet/11.0): .NET 10 remains the active LTS host signal at runtime patch 10.0.8 with SDK 10.0.300, while .NET 11 remains preview at 11.0.0-preview.4 with SDK 11.0.100-preview.4. These host SDK signals do not change the generated package-free `net48` target.
- Microsoft Learn NuGet docs for [`PackageReference` and lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies), [`dotnet restore` auditing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities), [package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping), and [trusted signers](https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-trusted-signers).
- Microsoft Learn and VS Code docs for [`dotnet new` custom templates](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates), [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide), and [VSIX/Marketplace publishing with `vsce`](https://code.visualstudio.com/api/working-with-extensions/publishing-extension).
- Microsoft Learn [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview), [`dotnet test` with MTP](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test), [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests), [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk), NuGet [`MSTest.Sdk`](https://www.nuget.org/packages/MSTest.Sdk), NuGet [`xunit.v3`](https://www.nuget.org/packages/xunit.v3), xUnit.net [MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform), NUnit [MTP guidance](https://docs.nunit.org/articles/vs-test-adapter/NUnit-And-Microsoft-Test-Platform.html), NuGet [`NUnit`](https://www.nuget.org/packages/NUnit), and NuGet [`NUnit3TestAdapter`](https://www.nuget.org/packages/NUnit3TestAdapter): MTP/MSTest SDK, xUnit.net v3, and NUnit are valid modern test-host choices. TypeSharp currently uses a pinned MSTest SDK bridge for package-based discovery while keeping the package-free main/shard runners as the release-confidence path; the latest package comparison kept MSTest.Sdk/MTP as the current package path and preserved the native MTP `--test-modules` module-parallelism minimum at 590 tests.
- GitHub [`actions/runner-images`](https://github.com/actions/runner-images), [`actions/setup-dotnet`](https://github.com/actions/setup-dotnet), [`actions/setup-node`](https://github.com/actions/setup-node), and the [Windows Server 2025 + Visual Studio 2026 migration issue](https://github.com/actions/runner-images/issues/14017): `windows-latest` is a supported GitHub-hosted Windows label, setup-dotnet sets up requested SDKs, and setup-node sets up requested Node.js versions and npm caching/authentication. The Windows Server 2025 + Visual Studio 2026 migration window beginning 2026-06-08 and completing 2026-06-15 remains an open CI-watch item, not a generated-artifact baseline change. The C# test-runner process launch now routes `npm` commands through `cmd.exe /d /s /c` on Windows while keeping non-Windows `npm` execution direct. The first post-fix regression run `26324313960` succeeded and passed the previously failing `Run shard runners in parallel` step; later listed Docs and Regression runs also succeeded. The VS Code stable release API reports 1.121.0, while the 1.122 release-notes page exists as an editor/tooling watch signal; neither changes the generated-artifact baseline.

## .NET Ecosystem Tooling Roadmap

This roadmap defines gates before TypeSharp grows beyond explicit local DLL references and repository-local VSIX/template workflows.

### 1.0 Dependency Acquisition Scope

TypeSharp 1.0 dependency acquisition is intentionally offline-friendly and package-free for generated artifacts:

- framework assemblies through `references.assemblies`,
- explicit local `net48` DLLs through `references.paths`,
- direct TypeSharp project references through `[projectReferences]`,
- matching TypeSharp Core/Runtime DLLs from the installed `TypeSharp.Tool` package, located with `typesharp runtime-path`.

NuGet package restore is post-1.0. `references.packages` remains a reserved manifest surface and reports `TS2405` during `check` and `build`; neither command may silently restore packages, execute package build targets, or add transitive package assets to the generated project. Users who need a package dependency in the 1.0 slice must restore or build it outside TypeSharp and reference a compatible local DLL through `references.paths`.

### NuGet Restore And Security

`references.packages` stays reserved until all gates below are implemented:

- Package references must use deterministic `PackageReference`-style metadata, not TypeScript declaration lookup or npm-style package resolution.
- Restore must support a checked-in lock file equivalent to `packages.lock.json` and a locked mode for CI so dependency graph changes are intentional.
- Restore must use an explicit `NuGet.config` generated or selected by TypeSharp. Package sources must be cleared by default, then added intentionally.
- Package source mapping is required when more than one source is configured so package IDs cannot silently come from an unexpected feed.
- Audit sources must be configurable separately from package sources, so vulnerability data can come from a public source even when packages come from an internal upstream feed.
- Vulnerability auditing must have a policy for direct and transitive packages, severity threshold, and CI behavior. The stable gate should fail high/critical advisories unless the project has a documented exception.
- License inventory, package ID/version inventory, and runtime deployment inventory are required for generated `net48` host compatibility.
- Package signatures or trusted-signers policy are recommended before public release; SHA-256 checksums remain the minimum artifact-integrity gate.
- Restore must never execute package build targets, analyzers, generators, or user code during `typesharp check`. If future `typesharp build` restore needs MSBuild targets, that execution boundary must be opt-in and documented.
- The generated project must stay offline-friendly when no packages are used, preserving the current generated `NuGet.config` with sources cleared.

### Target Profiles

`net48` remains the default generated target because it is the broad compatibility profile for existing .NET Framework hosts.

`net481` is a qualified profile, not an automatic upgrade:

- It may be enabled only through an explicit manifest or CLI target once the compiler, Core, Runtime, generated project scaffold, C# consumer smoke, and host compatibility smoke all pass for `net481`.
- The profile must document deployment assumptions, including supported Windows client/server baselines and whether the target machine already includes .NET Framework 4.8.1 or must install it.
- `net481` must not change TypeSharp language semantics. Generated C# should remain C# 7.3-compatible unless a separate backend policy changes.
- Core/Runtime may need multi-targeting before TypeSharp can claim first-class `net481`; until then, `net48` remains the stable release target.
- Older .NET Framework targets such as `net462` stay separate compatibility projects because they can reduce available APIs and broaden host-matrix cost.

### Templates And Scaffolding

`typesharp new` remains the primary built-in scaffolding path. Future `dotnet new` template packs are useful only after the TypeSharp CLI and generated artifact story are stable enough to package.

Template gates:

- Each template emits `TypeSharp.toml`, source files, `.gitignore`, and smoke commands that work without hidden global tools beyond installed `dotnet`, `node`, and the TypeSharp CLI.
- Console, library, interop-library, and host-consumer templates should share the same manifest semantics as hand-authored projects.
- Templates must not enable preview language features, NuGet restore, `net481`, or host packaging by default.
- Template package publication requires the same release integrity policy as CLI/runtime/VSIX artifacts.

### VS Code And LSP Parity

The VS Code extension should remain a thin client over the TypeSharp compiler/LSP contract. Parity milestones:

- CLI and LSP diagnostics must share codes, spans, severity, and source ordering.
- Formatter behavior must match `typesharp format --check` before the extension advertises stronger formatting guarantees.
- Completion, hover, and go-to-definition must be backed by the same semantic model used by `typesharp check`.
- A packaged VSIX must pass mocked extension-client tests, live language-server tests, and real Extension Host smoke tests before release.
- Marketplace publishing requires a publisher id, token ownership, `engines.vscode` compatibility, HTTPS/readme asset compliance, and `vsce` packaging checks. The repository can automate packaging, but credentialed publish remains a release-owner action.

### Release And Adoption Gates

Before TypeSharp can call the .NET ecosystem workflow stable:

- `typesharp new`, `check`, `build`, `run`, `format`, `explain`, and `lsp` must work from a clean Windows machine with installed `dotnet` and `node`.
- A generated library must build for `net48`, be consumed by a C# `net48` project, and run in host-style ASP.NET/WCF/worker smoke shapes.
- Release artifacts must include the `TypeSharp.Tool` NuGet package with bundled `runtime/net48` Core/Runtime DLLs. VSIX, template pack, and Marketplace publication remain separate adoption milestones.
- NuGet.org publication remains a release-owner milestone with explicit account ownership, scoped credentials, and security gates before automation.
- Every new ecosystem feature must include a rollback story: disable restore, fall back to local DLL references, install VSIX manually, or use explicit generated project references.

## Regression Policy

Regression evidence must prove the changed behavior, not just that the full suite still passes.

| Change Kind | Minimum Evidence |
| --- | --- |
| New stable syntax | Parser positive fixture, grammar/reference update, and coverage note. |
| Parser recovery or diagnostic change | Parser negative fixture with diagnostic JSON and tree snapshot. |
| Binder/name-resolution behavior | Binder positive or negative diagnostic fixture. |
| Type checking behavior | Type checker positive or negative fixture; CLI JSON smoke when user-facing. |
| Public boundary restriction | Type checker negative fixture and CLI build no-emission smoke. |
| Lowering change | C# backend snapshot and generated `net48` compile/build smoke. |
| Public API lowering | Backend snapshot, generated `net48` assembly build, and C# consumer smoke. |
| Runtime/helper behavior | Runtime smoke and generated build/consumer smoke if generated code calls it. |
| C# interop metadata/overload behavior | Metadata smoke plus generated build or diagnostic no-emission smoke. |
| Public ABI shape change | Public ABI metadata smoke and runtime ABI policy review. |
| CLI command behavior | Direct CLI smoke with stdout/stderr/exit-code assertion. |
| VS Code/LSP behavior | LSP request/notification smoke using zero-based ranges. |
| Docs-only policy | Linked docs, stale-path scan, and `git diff --check`. |

Snapshot changes are source changes. Parser `expected.tree`, diagnostic JSON, and backend `expected.cs` updates must be justified in linked docs or the surrounding change description. Large mechanical snapshot refreshes should be separate when practical.

## Parser Fixture Policy

Parser fixtures live under `test/fixtures/parser/`.

```text
test/fixtures/parser/
  positive/
    0001-hello-cli/
      input.tysh
      expected.diagnostics.json
      expected.tree
      README.md
  negative/
    0001-missing-function-body/
      input.tysh
      expected.diagnostics.json
      expected.tree
      README.md
```

Rules:

- A fixture directory has one input and its expected outputs.
- Fixture names use `NNNN-short-kebab-name`.
- Positive fixtures have no parse diagnostics.
- Negative fixtures test parser diagnostics and recovery only; semantic errors belong in semantic diagnostics fixtures.
- `expected.diagnostics.json` uses the CLI JSON diagnostics shape.
- line/column positions are 1-based; zero-length diagnostics have equal `start` and `end`.
- `expected.tree` records concrete syntax tree nodes, tokens, source spans, missing tokens, skipped tokens, and trivia summary.
- Snapshot updates require an explicit update command or deliberate patch.

Current parser coverage includes example-derived fixtures for CLI basics, modules/records, unions/patterns, structural narrowing, async interop, public API surface, pipeline/collections, C# interop, literals/attributes, public boundary normalization, capability boundaries, and parser-only fixtures for interfaces, partial declarations, ambient signatures, `open`, import aliases, namespace imports, export specifiers, generic invocations, `nameof`, checked/unchecked expressions, composition, `satisfies`, `yield`, `lock`, extension methods, intersections, collection spread, record spread, `keyof`, indexed access types, `params` parameters, optional/default parameter declarations, logical unsigned shift expressions, logical unsigned shift assignment expressions, and multiplicative compound assignment expressions.

## Feature Review Gate

Every feature change that touches syntax, semantics, interop, public API, backend, CLI, LSP, runtime, or release behavior should answer the applicable gate questions with concrete evidence.

| Question | Required Evidence |
| --- | --- |
| Does it run on .NET Framework 4.8, or is it compile-time-only? | Generated `net48` build, runtime/core `net48` audit, or explicit compile-time-only classification. |
| Can it be represented in public .NET metadata? | Public ABI snapshot, C# consumer smoke, or public-boundary diagnostic. |
| Is lowering documented? | [Lowering](../lowering/), backend snapshot, generated C# shape note, or no-lowering note. |
| Can C# consumers understand it? | C# `net48` consumer smoke or documented not-public reason. |
| Do diagnostics explain the next action? | Descriptor metadata, golden fixture, CLI smoke, or LSP diagnostic smoke. |
| Is preview/experimental behavior separated from stable behavior? | [Feature Status](../feature-status/), release gate, manifest/CLI feature gate, and release note classification. |
| Does tooling remain tractable? | Grammar/formatting/LSP impact note or explicit backlog note. |
| Are positive and negative tests covered? | Regression evidence from the closest fixture or smoke category. |

Docs-only policy changes can answer with linked documentation and diff checks when no runtime behavior changes.

## Release Policy

Release versions use `MAJOR.MINOR.PATCH[-preview.N]`.

- `0.x`: pre-1.0 development line; breaking changes require linked docs, release-note surface, and migration path.
- `1.0`: MVP stable line for generated `net48` assemblies, core/runtime ABI, CLI command surface, diagnostics schema, and stable grammar subset.
- Patch: compatible bug fixes, diagnostic text clarification, docs correction, and implementation fixes.
- Minor: backward-compatible features, diagnostics, CLI options, standard library helpers, or lowering support.
- Major: stable source compatibility, public ABI, manifest semantics, CLI JSON schema, generated metadata shape, or deployment-shape break.

Breaking changes include stable source rejection, diagnostic code/severity/schema incompatibility, CLI option or exit-code breakage, manifest meaning changes, generated public metadata changes, core/runtime public signature breakage, generated deployment-shape breakage, or interop resolution changes that alter existing valid behavior.

Breaking change gates:

- linked docs name the changed surface, reason, migration path, and verification,
- migration guide or release notes include migration notes,
- public ABI changes review the runtime ABI policy and update ABI version when required,
- release notes include a `Breaking Changes` section.

Release integrity and security:

- release packages need integrity verification appropriate to the channel,
- NuGet packages, VSIX files, and future standalone binaries need an explicit integrity/signing policy before publication,
- signing may be added once ownership and credential policy are settled,
- generated user assemblies are not signed by TypeSharp by default,
- compiler work must not execute arbitrary user code during parse/check/build/scaffold emission,
- new dependencies require license and vulnerability review,
- generated projects remain offline-friendly by default.

Release automation:

- `.github/workflows/release-artifacts.yml` runs on `v*.*.*` tag pushes and manual dispatch for an existing tag.
- The workflow validates release tags against `vMAJOR.MINOR.PATCH` or `vMAJOR.MINOR.PATCH-preview.N`; arbitrary prerelease labels are rejected so the published channel policy cannot drift from the documented version line. The workflow also verifies the checkout `HEAD` is the target commit for the release tag before building artifacts, so manual dispatch cannot build a tag-named package from the wrong revision.
- The release job runs on `windows-latest` because `TypeSharp.Core`, `TypeSharp.Runtime`, generated projects, and host smoke tests must preserve the `net48` artifact path.
- The only official CLI/runtime publication channel is the `TypeSharp.Tool` NuGet .NET global tool package. The package contains the modern CLI host plus `runtime/net48/TypeSharp.Core.dll` and `runtime/net48/TypeSharp.Runtime.dll` for generated or C# consumer projects that need explicit runtime references.
- The workflow restores and builds the test project, runs release gate tests for `net48` Core/Runtime targets, package-free runtime libraries, generated C# `net48` compilation, runnable example smokes, docs site contracts, and VS Code extension package shape, then builds the Astro docs site and runs the rendered public-docs verifier before packaging.
- The workflow packs `cli/TypeSharp.Cli/TypeSharp.Cli.csproj` as `TypeSharp.Tool`, injects `Version`, `TypeSharpBuildMetadata=<tag>`, and `TypeSharpSourceRevision=<short commit>`, and verifies the package can be installed from the local release folder with a temporary NuGet config before any real package publication is attempted.
- The local tool smoke verifies `typesharp version --json` reports `artifactKind = dotnet-tool`, `targetDefault = net48`, and `runtimeTargetFramework = net48`, then verifies `typesharp runtime-path --json` exposes packaged `runtime/net48/TypeSharp.Core.dll` and `runtime/net48/TypeSharp.Runtime.dll`.
- NuGet.org account setup, package ownership, API key creation, and first publish remain release-owner tasks documented in `agent/nuget-deploy.md`. The workflow intentionally stops after pack and local install smoke until those deployment prerequisites are handled.
- `docs.yml` stops after the Pages deployment result; it relies on the rendered public-docs verifier before upload.
- The rendered public-docs verifier rejects the configured exact legacy 404 marker set across the 34 rendered sidebar public-docs pages before upload, so marker regressions fail locally and in `docs.yml` before a Pages artifact can become the public deployment.
- The rendered public-docs verifier reads broader public-docs routes before Pages upload, requiring those pages to link back to Install, name `TypeSharp.Tool`, document `dotnet tool install --global TypeSharp.Tool`, preserve the generated `net48` artifact boundary, and reject repo-local CLI commands.
- The rendered public-docs verifier also reads CLI, Project Configuration, Runtime Artifacts, VS Code And LSP, Migration, and Troubleshooting before Pages upload, requiring those support routes to link back to Install, preserve `TypeSharp.Tool` and `typesharp runtime-path` guidance, show runtime `net48` DLL paths when applicable, show generated-output paths where applicable, and reject repo-local CLI commands.
- Rollback uses `dotnet tool update --global TypeSharp.Tool --version <previous-version>` or uninstall/install for the previous package version. Generated projects must keep the matching Runtime ABI and Core/Runtime DLLs from the same installed tool package.
Current release compatibility matrix:

| CLI line | Language | Runtime ABI | Generated target | Runtime source |
| --- | --- | --- | --- | --- |
| `0.1.0-preview` | `preview` | `0` preview | `net48` | `TypeSharp.Tool` package `runtime/net48` |

Release note template:

```text
# TypeSharp <version>

Date:
Channel:
Build metadata:
Source revision:
Runtime ABI:
Runtime ABI status:
Default target framework:

## Summary

## Compatibility Matrix

| CLI line | Language | Runtime ABI | Generated target | Runtime source |
| --- | --- | --- | --- | --- |
| <cli-line> | <language-channel> | <runtime-abi> <status> | <target-framework> | <runtime-source> |

## Breaking Changes

## Migration Notes

## Stable Features

## Preview Features

## Diagnostics And Tooling

## Security

## Package Integrity

## Artifacts

## Verification

## Rollback
```

The release-note template permits exactly one top-level `# TypeSharp <version>` title, the mandatory `##` section set below, and the expected artifact-description rows for the NuGet tool package in canonical order. The `Summary`, `Compatibility Matrix`, `Breaking Changes`, `Migration Notes`, `Stable Features`, `Preview Features`, `Diagnostics And Tooling`, `Security`, `Package Integrity`, `Artifacts`, `Verification`, and `Rollback` sections are mandatory and must stay in that order with no extra `##` sections. The `Compatibility Matrix` section must include exactly the `CLI line`, `Language`, `Runtime ABI`, `Generated target`, and `Runtime source` header row, the standard Markdown separator row, and the current release row in that order with no extra table rows. Use the exact body line `None.` when there is nothing to report.

## Official Reference Tracking

When TypeSharp policy depends on external platform or language state, use official vendor documentation and record the date of the decision. The reference set includes:

- Microsoft Learn .NET Framework installation and support documentation: <https://learn.microsoft.com/en-us/dotnet/framework/install/on-windows-and-server>
- Microsoft Learn .NET Framework versions and dependencies: <https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies>
- .NET Framework support policy: <https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework>
- Microsoft Learn target framework monikers: <https://learn.microsoft.com/en-us/dotnet/standard/frameworks>
- Microsoft Learn C# language versioning: <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning>
- Microsoft Learn C# release notes: <https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/>
- Microsoft Learn F# documentation and strategy: <https://learn.microsoft.com/en-us/dotnet/fsharp/> and <https://learn.microsoft.com/en-us/dotnet/fsharp/strategy>
- Microsoft Learn F# release notes: <https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/>
- Microsoft Learn NuGet `PackageReference` and lock files: <https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies>
- Microsoft Learn NuGet package source mapping: <https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping>
- Microsoft Learn `dotnet restore` package auditing: <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities>
- Microsoft Learn NuGet trusted signers: <https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-trusted-signers>
- Microsoft Learn `dotnet new` custom templates: <https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates>
- VS Code Language Server Extension Guide: <https://code.visualstudio.com/api/language-extensions/language-server-extension-guide>
- VS Code Publishing Extensions guide: <https://code.visualstudio.com/api/working-with-extensions/publishing-extension>
- TypeScript official blog release notes: <https://devblogs.microsoft.com/typescript/>
- Windows lifecycle documentation when target-framework deployment depends on installed OS baselines: <https://learn.microsoft.com/en-us/lifecycle/>

Before changing release baselines, target profiles, or preview-watch classifications, refresh the official sources rather than relying on an old bridge file snapshot.
