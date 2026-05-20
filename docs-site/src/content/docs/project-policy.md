---
title: Project Policy
description: Architecture, dependency, target framework, regression, feature review, parser fixture, and release policy for TypeSharp.
---

This is the canonical docs-site ledger for project policy that is too implementation-facing for the user guides but still durable enough to keep in the public docs site.

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
- `tests/TypeSharp.Compiler.Tests`: compiler, CLI, runtime, interop, backend, diagnostics, host, and docs smoke suite.

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
| `docs-site` | Astro static site | `astro`, `@astrojs/starlight`, `typescript` | Documentation only; locked by `docs-site/package-lock.json` and not deployed with generated `net48` artifacts. |

Compatibility audit:

- `TypeSharp.Core` and `TypeSharp.Runtime` project files target `net48`.
- Core/runtime projects do not use `PackageReference` or `packages.config`.
- Static scans guard against obvious .NET 5+ or package-backed APIs in core/runtime source.
- `dotnet build` for core/runtime remains the primary compatibility check.
- `docs-site` dependencies are installed with `npm ci` and verified with `npm run build`.

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

Parser fixtures live under `tests/fixtures/parser/`.

```text
tests/fixtures/parser/
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
- Microsoft Learn C# language versioning: <https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning>
- Microsoft Learn C# release notes: <https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/>
- Microsoft Learn F# release notes: <https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/>
- TypeScript official blog release notes: <https://devblogs.microsoft.com/typescript/>
- Windows lifecycle documentation when target-framework deployment depends on installed OS baselines: <https://learn.microsoft.com/en-us/lifecycle/>

Before changing release baselines, target profiles, or preview-watch classifications, refresh the official sources rather than relying on an old bridge file snapshot.
