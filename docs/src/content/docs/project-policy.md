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
- Compiler, CLI, and language server hosts may use a modern .NET LTS runtime.
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
- Generated C# project emission and external `dotnet build` orchestration remain ordered unless a task proves output paths, generated project files, and external process logs cannot conflict.
- The main `TypeSharp.Compiler.Tests` runner remains the full release-confidence path. Shard projects may link the same runner and execute disjoint ordinal slices in parallel when shared generated inputs use process-safe caches and every test keeps a unique workspace under `test/tmp`.
- Future MSTest SDK/Microsoft Testing Platform or xUnit.net v3 adoption is a test-host tooling concern only. It must preserve the same catalog coverage as the custom runner, keep generated `net48` artifacts package-free, and use normal NuGet dependency controls before package references are added.

## Dependency And Target Policy

Core rule: generated TypeSharp assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` must be referenceable from ordinary `net48` projects without hidden package requirements.

Current inventory:

| Artifact | Target | External NuGet | Notes |
| --- | --- | --- | --- |
| generated TypeSharp project | `net48` | None | `typesharp build` emits an offline-friendly generated C# project. |
| `TypeSharp.Core` | `net48` | None | User-facing `Option<T>`, `Result<T,E>`, `Unit` surface. |
| `TypeSharp.Runtime` | `net48` | None | Compiler-generated helper surface with no host-specific framework dependency. |
| `TypeSharp.Compiler` | modern .NET host | None | Host-side compiler implementation. |
| `TypeSharp.Cli` | modern .NET host | None | Depends on compiler project reference. |
| `TypeSharp.Compiler.Tests` | modern .NET host | None | Project references and linked core sources. |
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

Official .NET/NuGet/VS Code and .NET test-platform ecosystem sources refreshed on 2026-05-22:

- Microsoft Learn [target framework monikers](https://learn.microsoft.com/en-us/dotnet/standard/frameworks): `net48` and `net481` are .NET Framework TFMs, and `.NET Framework 4.8.1` maps to `net481`.
- Microsoft Learn [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies) and the [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework): `.NET Framework 4.8.1` is the latest .NET Framework version; Windows 10 22H2 includes 4.8 and can install 4.8.1, while recent Windows 11 releases include 4.8.1.
- Microsoft Learn NuGet docs for [`PackageReference` and lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies), [`dotnet restore` auditing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities), [package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping), and [trusted signers](https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-trusted-signers).
- Microsoft Learn and VS Code docs for [`dotnet new` custom templates](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates), [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide), and [VSIX/Marketplace publishing with `vsce`](https://code.visualstudio.com/api/working-with-extensions/publishing-extension).
- Microsoft Learn [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview) and [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests), plus xUnit.net [v3 package guidance](https://xunit.net/docs/nuget-packages-v3): MTP/MSTest SDK and xUnit.net v3 are valid modern test-host choices, but TypeSharp must first expose the existing custom test catalog as reusable discoverable cases before switching package frameworks.

## .NET Ecosystem Tooling Roadmap

This roadmap defines gates before TypeSharp grows beyond explicit local DLL references and repository-local VSIX/template workflows.

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
- Release artifacts must include CLI zip, runtime/core `net48` zip, VSIX, release notes, and SHA-256 checksums.
- NuGet package publication, template pack publication, and Marketplace publication remain separate adoption milestones with explicit credentials and security gates.
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
| Docs-only policy | Linked docs, task packet verification, stale-path scan, and `git diff --check`. |

Snapshot changes are source changes. Parser `expected.tree`, diagnostic JSON, and backend `expected.cs` updates must be justified in the same task packet or linked docs. Large mechanical snapshot refreshes should be separate when practical.

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

Current parser coverage includes example-derived fixtures for CLI basics, modules/records, unions/patterns, structural narrowing, async interop, public API surface, pipeline/collections, C# interop, literals/attributes, public boundary normalization, capability boundaries, and parser-only fixtures for interfaces, partial declarations, ambient signatures, `open`, import aliases, namespace imports, export specifiers, generic invocations, `nameof`, checked/unchecked expressions, composition, `satisfies`, `yield`, `lock`, extension methods, intersections, collection spread, record spread, `keyof`, and indexed access types.

## Feature Review Gate

Every feature task that touches syntax, semantics, interop, public API, backend, CLI, LSP, runtime, or release behavior should answer the applicable gate questions with concrete evidence.

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

- `0.x`: pre-1.0 development line; breaking changes require task packet, traceability, release-note surface, and migration path.
- `1.0`: MVP stable line for generated `net48` assemblies, core/runtime ABI, CLI command surface, diagnostics schema, and stable grammar subset.
- Patch: compatible bug fixes, diagnostic text clarification, docs correction, and implementation fixes.
- Minor: backward-compatible features, diagnostics, CLI options, standard library helpers, or lowering support.
- Major: stable source compatibility, public ABI, manifest semantics, CLI JSON schema, generated metadata shape, or deployment-shape break.

Breaking changes include stable source rejection, diagnostic code/severity/schema incompatibility, CLI option or exit-code breakage, manifest meaning changes, generated public metadata changes, core/runtime public signature breakage, generated deployment-shape breakage, or interop resolution changes that alter existing valid behavior.

Breaking change gates:

- task packet names changed surface, reason, migration path, and verification,
- traceability records changed evidence or boundary,
- migration guide or release notes include migration notes,
- public ABI changes review the runtime ABI policy and update ABI version when required,
- release notes include a `Breaking Changes` section.

Release integrity and security:

- release artifacts need SHA-256 checksums in a release manifest,
- NuGet packages, VSIX files, zip archives, and standalone CLI binaries are covered by checksums,
- signing may be added, but checksums are the minimum gate,
- generated user assemblies are not signed by TypeSharp by default,
- compiler work must not execute arbitrary user code during parse/check/build/scaffold emission,
- new dependencies require license and vulnerability review,
- generated projects remain offline-friendly by default.

Release automation:

- `.github/workflows/release-artifacts.yml` runs on `v*.*.*` tag pushes and manual dispatch for an existing tag.
- The release job runs on `windows-latest` because `TypeSharp.Core`, `TypeSharp.Runtime`, generated projects, and host smoke tests must preserve the `net48` artifact path.
- The workflow publishes `typesharp-cli-dotnet-<tag>.zip`, `typesharp-runtime-net48-<tag>.zip`, `typesharp-vscode-<tag>.vsix`, `SHA256SUMS.txt`, and generated release notes to the GitHub Release.
- The CLI asset is framework-dependent and uses `UseAppHost=false`; it is a modern .NET tool-host artifact, not a generated `net48` runtime artifact.
- The runtime asset contains the package-free `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` `net48` libraries used by generated or host projects.
- The workflow also uploads the same release folder as a GitHub Actions workflow artifact for run-level inspection before or after release publication.
- Release publication uses `GITHUB_TOKEN` with `contents: write`; all other release integrity requirements still come from this policy.

Release note template:

```text
# TypeSharp <version>

Date:
Channel:
Runtime ABI:
Default target framework:

## Summary

## Compatibility Matrix

## Breaking Changes

## Migration Notes

## Stable Features

## Preview Features

## Diagnostics And Tooling

## Security

## Checksums And Signing

## Verification
```

The `Breaking Changes`, `Preview Features`, `Security`, `Checksums And Signing`, and `Verification` sections are mandatory. Use `None` when there is nothing to report.

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
