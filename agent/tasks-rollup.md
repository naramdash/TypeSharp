# Task Rollup: Project Work Ledger

Status: Done
Queue: Q0-Q5
Start Time: 2026-05-20 02:17:44 +09:00
End Time: 2026-05-21 01:19:17 +09:00

## Objective

Keep one compact completed-work ledger for agent handoff without preserving every historical task packet as a separate file.

## Compression Rule

This rollup replaces individual completed task packet files for work 0001 through 0271. Future completed active packets should be folded into this file, then removed from `agent/`.

## State At Compression

| Area | State |
| --- | --- |
| Completed work covered | 0001-0271 |
| Active task packet at compression | None |
| Generated artifact target | `net48` generated assemblies and runtime/core libraries |
| Host/tool target | Modern .NET host for compiler, CLI, LSP, and tests |
| Web docs target | Astro Starlight docs with GitHub Pages-compatible static output |

## Foundation Parser And Semantic Skeleton

Completed foundation work established:

- Lexer/parser, syntax tree spans, parser recovery, manifest/source discovery, compiler core skeleton, CLI skeleton, diagnostics taxonomy, binder skeleton, type checker skeleton.
- Parser fixture policy and stable parser fixture snapshots.
- Binder/name-resolution diagnostics including duplicate symbols, unresolved names, unresolved `nameof`, import alias conflicts, duplicate local exports, unsupported export forwarding, and missing source module exports.
- Type checker positive/negative fixture harness and core mismatch, structural shape, null safety, public boundary, capability marker, union exhaustiveness, record/collection/yield/lock/extension/keyof/indexed-access diagnostics.

Primary evidence:

- `test/fixtures/parser`
- `test/fixtures/diagnostics`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Runtime Build Backend And Language Lowering

Completed backend/runtime work established:

- `TypeSharp.Core` and `TypeSharp.Runtime` `net48` libraries with `Option<T>`, `Result<T,E>`, `Unit`, async/equality/pattern/union helpers.
- C# 7.3 source backend, generated project scaffold, runtime import lowering, generated `net48` compile smoke, generated project reference propagation.
- Runtime artifact architecture docs now explain the tool-host/runtime-artifact boundary, generated project layout, Core/Runtime reference flow, and host deployment set with Mermaid diagrams.
- Lowering and fixture coverage for functions, blocks, imports, modules/namespaces, public API, classes/interfaces/records, generic types/functions/constraints, immutable records, nominal unions, pattern matching, type-level union narrowing, async `Task`, pipeline/composition, collection expressions and spread, indexer, `nameof`, checked/unchecked, `satisfies`, `yield`, `lock`, extension methods, record construction/spread, limited `keyof`, and limited indexed access.

Primary evidence:

- `test/fixtures/backend/csharp`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- `lang/TypeSharp.Core`
- `lang/TypeSharp.Runtime`

## CSharp Interop And Metadata Diagnostics

Completed interop work established:

- Framework and local DLL reference resolution.
- Metadata reader indexes for public types, fields, properties, indexers, generic methods, constraints, extension methods, params/byref/property accessors, static members, and nullability markers.
- C# constructor, member, field, property, indexer, delegate, event, attribute, generic type/method, nullable, overload, params/optional/named/byref, extension method, local/framework missing symbol, and generic constraint diagnostics.
- Imported class-to-interface/base assignment validation and metadata relationship ranking.
- Unsupported NuGet/package reference diagnostics and capability markers for `dynamic`, `reflect`, `interop`, and `unsafe`.

Primary evidence:

- C# interop tests in `test/TypeSharp.Compiler.Tests/Program.cs`
- Interop-focused docs pages: [.NET Interop](../docs/src/content/docs/dotnet-interop.md), [Diagnostics](../docs/src/content/docs/diagnostics.md)

## CLI VSCode And Tooling

Completed tooling work established:

- CLI `version`, `check`, `build`, `run`, `format`, `format --check`, `explain`, and `lsp`.
- Manifest option parsing and semantic value validation.
- Warning gates, common options, target/configuration/verbosity handling, JSON/text diagnostics, generated output handling.
- Language server diagnostics, hover, definition, completion, formatting.
- VS Code extension activation, TextMate grammar, LSP client/server launch contract, mocked/live/extension-host smoke tests.
- Runnable example project catalog and ASP.NET/WCF/worker-style host compatibility smokes.

Primary evidence:

- `test/TypeSharp.Compiler.Tests/Program.cs`
- `vscode/typesharp`
- `examples/runnable`
- [CLI](../docs/src/content/docs/cli.md), [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md)

## Documentation Process Release And Adoption

Completed docs/adoption work established:

- docs became the canonical standard language/project ledger surface.
- `agent/` was reduced to flat agent-work files only.
- GitHub Pages/Astro Starlight documentation IA, tutorials/guides/cookbook, examples, migration, advanced topics, project policy, release, security, compatibility, diagnostics, and ledger pages were added or consolidated.
- Official docs benchmark artifacts moved to `docs/research`.
- Root README became the human entry point.
- Agent workflow now requires task-end commit/push handoff and compressed task history.
- Docs package dependencies are pinned to the current npm registry latest tags for Astro, Starlight, and TypeScript, with package contract coverage.
- Docs-owned site configuration is TypeScript and the docs contract rejects committed docs-owned JavaScript source/config files.
- TypeSharp source examples in docs use `tysh` code fences, and Starlight/Shiki reuses the VS Code TextMate grammar for syntax highlighting.
- Docs Mermaid rendering is enabled for architecture pages through docs-only `astro-mermaid` and `mermaid` dependencies.
- The Writing Guide adapts the Vue Docs Writing Guide into TypeSharp-specific authoring rules, `tysh` example project guidance, emoji usage, and review checks.
- Release artifact automation publishes tagged CLI, `net48` runtime library, VSIX, release notes, and SHA-256 manifest assets to GitHub Releases.
- Microsoft Learn C#-style detailed TypeSharp reference pages now explain CLR type mapping, value/reference/nullability/generic public type rules, member lookup, overload ranking, byref calls, delegates, events, extension methods, exception boundaries, and interop diagnostics.
- Runnable TypeSharp example projects now use realistic invoice, billing public API, local C# billing interop, ASP.NET/WCF greeting, worker work-item, and nullable customer profile scenarios, with smoke-tested commands and host compilation where applicable.
- Runnable example READMEs now explain every command, output, TypeSharp, C#, and XML code block before the block appears, with docs authoring guidance and contract tests enforcing that pattern.

Primary evidence:

- `docs/src/content/docs`
- `examples/runnable`
- [Examples](../docs/src/content/docs/examples.md)
- [Writing Guide](../docs/src/content/docs/writing-guide.md)
- `.github/workflows/release-artifacts.yml`
- [Writing Guide](../docs/src/content/docs/writing-guide.md)
- [C# And CLR Type Model](../docs/src/content/docs/csharp-type-model.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- `docs/research`
- [agent.md](../agent.md)
- [tasks.md](tasks.md)

## Language Safety Modules And Import Export

Completed source module/safety work established:

- Source module path identity and duplicate path diagnostics.
- Relative source import graph collection and missing target/export diagnostics.
- Local and relative named function/value/type/module import/export/re-export alias forwarding and remapping.
- Namespace imports, named module imports, relative star re-exports over currently lowerable surfaces.
- Public ABI checks for structural/type-level constructs.
- `unknown` narrowing and capability marker diagnostics.

Primary evidence:

- Source module tests in `test/TypeSharp.Compiler.Tests/Program.cs`
- Parser/backend/diagnostic fixtures under `test/fixtures`
- [Modules And Imports](../docs/src/content/docs/modules.md), [Type System](../docs/src/content/docs/type-system.md)

## Task 0256 Test Suite Quality Audit

Completed test quality audit work established:

- Parser, binder, type-checker, and C# backend fixture directories now have enforced scenario README coverage for every `input.tysh` fixture.
- Missing scenario README files were added for backend indexer, record expression, partial declaration, `nameof`, and extension method lowering fixtures; binder unresolved `nameof`; type-checker collection/extension/record mismatch diagnostics; parser partial declaration and extension declaration fixtures.
- Parser, binder, and type-checker diagnostic fixtures now enforce polarity: positive fixtures must keep empty expected diagnostics, and negative fixtures must keep at least one structured diagnostic with code, message, file, and start/end locations.
- Runnable example CLI smoke helper now requires successful `check` and `build` commands to leave stderr empty.
- VS Code Extension Host smoke runner now fails on signal termination or null/nonzero exit status instead of converting a signal-terminated process result to success.
- `test/tmp` was confirmed ignored and untracked, so generated residue is not part of the regression fixture set.

Audit inventory result:

- Parser fixtures: snapshot and diagnostic expectations are backed by README coverage and positive/negative polarity checks.
- Backend C# golden fixtures: `expected.cs` snapshots, generated C# compile smokes, and scenario README coverage protect lowering shape.
- Binder/type-checker diagnostic fixtures: JSON diagnostic snapshots are now guarded for polarity and diagnostic code/message/file/location shape.
- CLI/build/run/format/explain tests: manual runner covers exit codes, text/JSON diagnostics, generated output, no-emission-on-error behavior, formatting, explanation metadata, and runtime output; runnable helper was hardened for stderr.
- Metadata/C# interop tests: reference resolution, metadata reader, overload resolution, nullable/byref/generic constraints, public ABI, local DLL, framework, extension, indexer, constructor, delegate, event, field, and host consumption smokes remain covered by scenario tests.
- Runtime/Core behavior tests: `Option`, `Result`, runtime ABI constants, union/pattern/equality/async helpers, package-free `net48` artifacts, and host compatibility smokes remain covered.
- VS Code extension tests: mocked, live bundled language server, and real Extension Host smokes cover activation, diagnostics, hover, go-to-definition, completion, formatting, and shutdown.
- Runnable examples and host compatibility tests: example catalog, console/library/C# interop/null-safety examples, worker host, ASP.NET/WCF host, generated assembly loading, runtime/core reference copying, and `net48` application-model host smokes remain covered.
- docs/documentation build smoke: docs package/config/content contract is covered by the C# runner and `npm run build` succeeds.

Verification:

```powershell
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
npm run check          # in vscode/typesharp
npm run check:smoke    # in vscode/typesharp
npm run check:live     # in vscode/typesharp
npm run check:host     # in vscode/typesharp
npm run test:smoke     # in vscode/typesharp
npm run test:live      # in vscode/typesharp
npm run test:host      # in vscode/typesharp
npm run build          # in docs
```

Primary evidence:

- `test/TypeSharp.Compiler.Tests/Program.cs`
- `test/fixtures`
- `vscode/typesharp/test`
- `examples/runnable`
- `docs/src/content/docs`

## Task 0257 Docs Agent Directory Rename

Completed directory ownership work established:

- The public Astro Starlight documentation source moved from `docs-site/` to `docs/`.
- The temporary agentic goal work surface moved from `docs/` to `agent/`.
- `agent.md`, agent operating files, README repository map, docs ownership pages, project ledger, work ledger, and agent workflow pages now state that `docs/` owns standard language/project records and `agent/` owns temporary task/control records.
- GitHub Pages workflow paths, npm cache path, docs working directory, artifact path, docs package name, and C# docs contract tests now target `docs/`.
- Fixture README and research links now point to docs canonical pages or `agent/tasks-rollup.md` according to the new ownership boundary.

Verification:

```powershell
npm ci                 # in docs
npm run build          # in docs
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Primary evidence:

- `docs`
- `agent`
- `.github/workflows/docs.yml`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Document Ownership](../docs/src/content/docs/document-ownership.md)

## Task 0258 Codex Skills Configuration

Completed Codex skill configuration work established:

- Vendored `playwright`, `gh-fix-ci`, `security-threat-model`, `typesharp-dotnet`, and `typesharp-language-engineering` into `.codex/skills`.
- Adapted `gh-fix-ci` to use `gh` directly without a project-local helper script runtime.
- Removed the redundant skill index document; `.codex/skills/*/SKILL.md` is the project-local skill source of truth.

Verification:

```powershell
Get-ChildItem -Directory .codex\skills
npm run build          # in docs
git diff --check
```

Primary evidence:

- `.codex/skills/playwright`
- `.codex/skills/gh-fix-ci`
- `.codex/skills/security-threat-model`
- `.codex/skills/typesharp-dotnet`
- `.codex/skills/typesharp-language-engineering`

## Task 0259 Parallel Execution Optimization

Completed parallel execution work established:

- `TypeSharpChecker.Check` now parses source files and runs per-source interop/binder/type-check validation in parallel after manifest/source/reference setup.
- `TypeSharpBuilder.Build` now performs source-file parse and per-file semantic validation in parallel before deterministic module graph validation and ordered generated C# emission.
- Both paths use ordered parallel processing so diagnostics remain appended in source discovery order.
- The test suite now includes a source-order diagnostic regression covering parallel checker diagnostics.
- Project Policy records the compiler parallel execution boundary, and Agentic Execution records safe parallel task execution rules for future goal work.
- `.gitignore` ignores miscellaneous `.codex` state while allowing `.codex/skills` to be tracked as the project-local skill source.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parallel diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "compiler check performance"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build uses module path containers"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Checking/TypeSharpChecker.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [agentic-execution.md](agentic-execution.md)

## Task 0260 Docs Dependency Update

Completed docs dependency update work established:

- `docs/package.json` now pins `astro` to `6.3.6`, `@astrojs/starlight` to `0.39.2`, and `typescript` to `6.0.3`, matching the current npm registry `latest` versions checked for this task.
- `docs/package-lock.json` was regenerated by npm for the updated Astro and TypeScript packages.
- The docs site contract test now validates the TypeScript devDependency alongside the Astro and Starlight dependencies.
- `npm outdated --json` for `docs` now reports an empty object.

Verification:

```powershell
npm view astro version
npm view @astrojs/starlight version
npm view typescript version
npm view @astrojs/starlight peerDependencies --json
npm install --save-exact astro@6.3.6 @astrojs/starlight@0.39.2 typescript@6.0.3
npm run build          # in docs
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "GitHub Pages workflow contract"
npm outdated --json    # in docs
```

Primary evidence:

- `docs/package.json`
- `docs/package-lock.json`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0261 Docs TypeScript Config Conversion

Completed docs TypeScript conversion work established:

- Renamed `docs/astro.config.mjs` to `docs/astro.config.ts`.
- Kept the Starlight configuration under TypeScript validation with `satisfies Parameters<typeof starlight>[0]`.
- Strengthened the docs site contract so it requires `astro.config.ts`, rejects `astro.config.mjs`, and fails if docs-owned source/config files use `.js`, `.mjs`, `.cjs`, `.jsx`, or `.mjsx`.
- Confirmed no docs-owned JavaScript source files remain; generated `dist`, `.astro`, and `node_modules` output are excluded from that contract.

Verification:

```powershell
npm run build          # in docs
rg --files docs | rg "\.(mjs|js|cjs|jsx|mjsx)$"
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "GitHub Pages workflow contract"
```

Primary evidence:

- `docs/astro.config.ts`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0262 VS Code Syntax Highlighting Extension Install Guide

Completed VS Code syntax highlighting and install-guide work established:

- The `vscode/typesharp` extension package now exposes `npm run package:vsix`, repository metadata for Marketplace link rewriting, and includes `MARKETPLACE.md` in the VSIX package files.
- The TextMate grammar now explicitly covers stable TypeSharp tokens and scopes for `satisfies`, `keyof`, `nameof`, `checked`, `unchecked`, and block-level `lock`.
- `vscode/typesharp/README.md` documents local VSIX installation with `code --install-extension` and points user-owned Marketplace publishing work to `MARKETPLACE.md`.
- `vscode/typesharp/MARKETPLACE.md` records the temporary publishing guide for PAT, publisher id, `@vscode/vsce` login, package, manual upload, and publish flows.
- [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md) documents syntax highlighting extension packaging, local install, smoke commands, and the temporary Marketplace guide.
- The regression suite now checks the package shape, install/publishing docs, and stable grammar token coverage.

Verification:

```powershell
npm run check          # in vscode/typesharp
npm run test:smoke     # in vscode/typesharp
npm run package:vsix   # in vscode/typesharp
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code extension package shape"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code syntax grammar"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
npm run build          # in docs
git diff --check
```

Primary evidence:

- `vscode/typesharp/package.json`
- `vscode/typesharp/syntaxes/typesharp.tmLanguage.json`
- `vscode/typesharp/README.md`
- `vscode/typesharp/MARKETPLACE.md`
- [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0263 Docs tysh Syntax Highlighting

Completed docs syntax highlighting work established:

- `docs/astro.config.ts` registers the VS Code `typesharp.tmLanguage.json` TextMate grammar as the Starlight/Shiki `typesharp` language.
- Starlight accepts both `tysh` and `typesharp` aliases for TypeSharp code fences.
- TypeSharp source examples in API, CLI, cookbook, .NET interop, fundamentals, guides, language tour, lowering, migration, modules, and type-system pages now use `tysh` fences.
- CLI commands, diagnostic output, file trees, and pipeline diagrams remain `text` fences.
- The docs site contract now checks the Shiki language registration and rejects TypeSharp-looking source blocks left under `text` fences.
- Built public docs output renders `data-language="tysh"` code blocks with token spans from the shared grammar.

Verification:

```powershell
npm run build          # in docs
rg -F 'data-language="tysh"' docs/dist/language-tour/index.html docs/dist/fundamentals/index.html docs/dist/modules/index.html
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
```

Primary evidence:

- `docs/astro.config.ts`
- `docs/src/content/docs/*.md`
- `vscode/typesharp/syntaxes/typesharp.tmLanguage.json`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0264 Runtime Artifact Architecture Docs

Completed runtime artifact documentation work established:

- Added the [Runtime Artifacts](../docs/src/content/docs/runtime-artifacts.md) docs page to explain how a TypeSharp project becomes generated C# source, a generated SDK-style `net48` project, and a deployable `.dll` or `.exe`.
- Added three Mermaid diagrams covering the modern tool-host versus `net48` artifact boundary, the `typesharp build` sequence, and the host deployment set.
- Documented that the current preview uses manifest `references.paths` for `TypeSharp.Core.dll`, `TypeSharp.Runtime.dll`, and local `net48` DLLs rather than hidden package restore.
- Connected Runtime Artifacts from the Starlight sidebar, Project Ledger, Project Configuration, and .NET Interop pages.
- Enabled docs-only Mermaid rendering with `astro-mermaid@2.0.1` and `mermaid@11.15.0`, pinned in `docs/package.json` and `docs/package-lock.json`.
- Strengthened the docs site contract so the new page, sidebar entry, Mermaid integration, dependencies, and key artifact invariants are checked.

Verification:

```powershell
npm install --save-exact astro-mermaid@2.0.1 mermaid@11.15.0  # in docs
npm run build                                                  # in docs
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
```

Notes:

- `npm install` reported `found 0 vulnerabilities`.
- `npm run build` completed and generated 33 pages; Vite emitted a non-fatal chunk-size warning after adding Mermaid's client renderer.

Primary evidence:

- `docs/src/content/docs/runtime-artifacts.md`
- `docs/astro.config.ts`
- `docs/package.json`
- `docs/package-lock.json`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0265 Docs Writing Guide

Completed docs writing guide work established:

- Reviewed the upstream Vue Docs Writing Guide and adapted its low-cognitive-load, problem-first, concrete-example guidance to TypeSharp docs.
- Added [Writing Guide](../docs/src/content/docs/writing-guide.md) with TypeSharp page-type rules, heading/flow guidance, `tysh` example project requirements, emoji usage policy, and review checklist.
- Documented that runnable `tysh` project examples should include `TypeSharp.toml`, `src/Main.tysh`, `typesharp check`, `typesharp build`, expected artifacts, `net48`, and Core/Runtime references when relevant.
- Connected the guide from the Starlight sidebar, Project Ledger, and Examples authoring principles.
- Strengthened the docs site contract so the writing guide, sidebar entry, Vue source attribution, `tysh` fence, manifest, `net48`, Core/Runtime references, emoji policy, and verification commands are checked.

Verification:

```powershell
npm run build          # in docs
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
```

Notes:

- `npm run build` completed and generated 34 pages; Vite emitted the existing non-fatal chunk-size warning from the Mermaid client bundle.

Primary evidence:

- `docs/src/content/docs/writing-guide.md`
- `docs/astro.config.ts`
- `docs/src/content/docs/project-ledger.md`
- `docs/src/content/docs/examples.md`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0266 Release Artifacts Workflow

Completed release workflow work established:

- Added `.github/workflows/release-artifacts.yml` to publish release assets on `v*.*.*` tag pushes or manual dispatch for an existing tag.
- The workflow runs on `windows-latest`, sets up .NET `10.0.x` and Node `24`, validates release tags, builds the test project, and runs release-focused smoke tests for `net48`, runnable examples, and VS Code package shape.
- Release assets include `typesharp-cli-dotnet-<tag>.zip`, `typesharp-runtime-net48-<tag>.zip`, `typesharp-vscode-<tag>.vsix`, generated release notes, and `SHA256SUMS.txt`.
- The runtime zip contains package-free `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` `net48` libraries; the CLI zip is framework-dependent with `UseAppHost=false` so it stays a modern tool-host artifact.
- The workflow uploads the release folder as a run artifact and publishes or updates the GitHub Release with `GITHUB_TOKEN` and `contents: write`.
- Project Policy now records the release automation surface and checksum requirements, and `.gitignore` ignores locally packaged VSIX files.
- The workflow contract test now covers the release workflow and updates the existing GitHub Pages workflow contract to current checked-in action versions.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "GitHub Pages workflow contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "release artifacts workflow contract"
npm run build          # in docs
git diff --check
```

Notes:

- `npm run build` completed and generated 34 pages; Vite emitted the existing non-fatal chunk-size warning from the Mermaid client bundle.

Primary evidence:

- `.github/workflows/release-artifacts.yml`
- `.gitignore`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0267 CSharp Docs Parity Detail

Completed C# documentation parity work established:

- Reviewed Microsoft Learn C# language documentation, the C# type system, object-oriented techniques, language reference, keywords, operators, and exception handling pages on 2026-05-21.
- Added [C# And CLR Type Model](../docs/src/content/docs/csharp-type-model.md) with detailed TypeSharp rules for strong typing, CLR metadata, value/reference types, built-in mapping, named public types, compile-time-only types, nullability, generics, collections, delegates, async tasks, and public type decisions.
- Added [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md) with detailed TypeSharp guidance for C# member surfaces, imports, constructors, overload ranking, named/optional/params arguments, byref calls, properties, fields, indexers, delegates, lambdas, events, extension methods, exception boundaries, and diagnostics.
- Connected the new detailed pages from the Reference sidebar, Grammar And Language Reference, Type System, .NET Interop, and Project Ledger pages.
- Strengthened the docs site contract so the new pages, sidebar slugs, source attribution, `tysh` examples, interop links, and detailed C# topic sections are checked.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
npm run build          # in docs
git diff --check
```

Notes:

- `npm run build` completed and generated 36 pages; Vite emitted the existing non-fatal chunk-size warning from the Mermaid client bundle.

Primary evidence:

- `docs/src/content/docs/csharp-type-model.md`
- `docs/src/content/docs/csharp-members-overloads.md`
- `docs/astro.config.ts`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0268 Realistic Runnable Tysh Examples

Completed runnable example realism work established:

- Reworked `examples/runnable/console-hello` from a one-line greeting into an invoice-style `net48` console project with `InvoiceLine`, `InvoiceDraft`, total calculation, `System.Text.StringBuilder` rendering, and verified `typesharp run` output.
- Reworked `examples/runnable/library-public-api` into a C#-friendly billing API with public account, quote, decision, and calculator types; added a C# `net48` host smoke project that references the generated TypeSharp assembly plus Core/Runtime dependencies.
- Reworked `examples/runnable/csharp-interop` into a local legacy billing DLL scenario covering customer repository metadata, named optional arguments, `params`, and `out` interop calls from TypeSharp.
- Reworked host examples so ASP.NET/WCF calls a generated greeting request renderer and the worker host calls a generated billing work-item label.
- Reworked `diagnostics-null-safety` into a nullable customer profile boundary example that still emits `TS2202` JSON diagnostics.
- Updated runnable catalog docs and docs tutorials so the public examples page describes the smoke-tested realistic scenarios instead of toy examples.
- Strengthened the C# test contract so the catalog, example source contents, local C# DLL, C# host consumers, docs examples page, and docs tutorial references are checked.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "runnable example project commands"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "runnable example catalog"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
npm run build          # in docs
git diff --check
```

Primary evidence:

- `examples/runnable`
- `docs/src/content/docs/examples.md`
- `docs/src/content/docs/tutorials.md`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0269 Runnable Example Code Explanations

Completed runnable example explanation work established:

- Added guided `Code Walkthrough` sections to each runnable project README so TypeSharp, C# host, legacy C# DLL, XML config, command, and expected-output code blocks are introduced by an explanation before the block appears.
- Explained the `console-hello` invoice records, total calculation, `StringBuilder` rendering, executable entry point, commands, and expected output.
- Explained the `library-public-api` public billing records/classes, exported smoke surface, and C# `net48` consumer proof.
- Explained the `csharp-interop` legacy C# billing DLL, named/optional/params calls, and byref `out` call.
- Explained the ASP.NET/WCF and worker host examples from both TypeSharp source and host C# consumption perspectives.
- Explained why the diagnostics null-safety example is intentionally invalid and how `TS2202` is expected.
- Updated the Examples page and Writing Guide so runnable example READMEs must explain every code block before the block appears.
- Strengthened the runnable catalog contract to require `Code Walkthrough` sections and to validate that every Markdown code fence in runnable project READMEs has a preceding explanation ending with a colon.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "runnable example catalog"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "runnable example project commands"
npm run build          # in docs
git diff --check
```

Primary evidence:

- `examples/runnable/*/README.md`
- `examples/runnable/README.md`
- `docs/src/content/docs/examples.md`
- `docs/src/content/docs/writing-guide.md`
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0270 Monorepo Folder Structure

Completed monorepo layout work established:

- Split repository-owned implementation paths so the CLI host lives under `cli/TypeSharp.Cli`, language/compiler/LSP/runtime projects live under `lang/`, and the regression suite plus fixtures live under `test/`.
- Removed the root `src/` and `tests/` implementation folders after moving tracked files, while preserving project source-root semantics such as user project `src/Main.tysh`.
- Added top-level folder README contracts for `cli/`, `lang/`, `test/`, `docs/`, and `vscode/`; existing `agent/` and `examples/` README files continue to describe those folder roles.
- Updated project references, release workflow commands, VS Code language-server publish and development fallback paths, docs pages, runnable example commands, `.gitignore`, agent records, and project-local Codex skill instructions to the new paths.
- Added a `repository monorepo layout is stable` regression test that asserts the folder map, README presence, project references, `.gitignore`, release workflow, and VS Code development fallback.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "repository monorepo layout"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "release artifacts workflow contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code extension package shape"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
npm run check          # in vscode/typesharp
git diff --check
```

Primary evidence:

- `README.md`
- `cli/README.md`
- `lang/README.md`
- `test/README.md`
- `docs/README.md`
- `vscode/README.md`
- `.github/workflows/release-artifacts.yml`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- `docs/src/content/docs/project-ledger.md`

## Task 0271 VS Code LSP Feedback

Completed VS Code/LSP feedback work established:

- The language server now advertises `textDocumentSync` with `openClose: true` and full-document change sync instead of only the numeric sync mode.
- The server handles `textDocument/didClose`, removes the closed URI from the open-document cache, and publishes an empty diagnostic set so stale editor diagnostics are cleared.
- Closed documents no longer answer hover requests from stale cached text; semantic hover, definition, completion, and diagnostics continue to use the current open document text.
- The VS Code client smoke now verifies close notification forwarding and diagnostic collection deletion in addition to diagnostics, hover, go-to-definition, completion, formatting, and shutdown.
- Public VS Code/LSP docs and the extension README now document document lifecycle synchronization and stale diagnostic clearing as part of the LSP-backed editor loop.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "language server clears diagnostics on didClose"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "language server publishes diagnostics on didOpen"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "language server returns hover"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "language server returns definition"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "language server returns completion"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code extension activates LSP client"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run check          # in vscode/typesharp
npm run check:smoke    # in vscode/typesharp
npm run check:live     # in vscode/typesharp
npm run check:host     # in vscode/typesharp
npm run test:smoke     # in vscode/typesharp
npm run build          # in docs
git diff --check
```

Primary evidence:

- `lang/TypeSharp.LanguageServer/TypeSharpLanguageServer.cs`
- `vscode/typesharp/extension.js`
- `vscode/typesharp/test/extension-smoke.js`
- `vscode/typesharp/README.md`
- [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Verification Summary

Representative commands used across the completed range:

```powershell
dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Representative focused smoke areas:

- Parser, binder, type checker, backend fixture snapshots.
- CLI build/check/run/format/explain/lsp.
- Generated `net48` compile/run and host compatibility.
- C# interop metadata, overload, diagnostics, generic constraints, nullable, extension methods.
- VS Code extension mocked/live/host smokes.
- docs static build.

## Handoff

Done:

- Completed historical work through task 0271 is compressed here.
- `agent/tasks.md` is the active task pointer.
- `agent/tasks-rollup.md` is the only completed task rollup file.

Remaining:

- Continue the active task in [tasks.md](tasks.md).
- Fold each completed active task back into this file and remove the completed packet.

Blocked:

- None.
