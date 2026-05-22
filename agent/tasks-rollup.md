# Task Rollup: Project Work Ledger

Status: Done
Queue: Q0-Q5
Start Time: 2026-05-20 02:17:44 +09:00
End Time: 2026-05-22 01:03:29 +09:00

## Objective

Keep one compact completed-work ledger for agent handoff without preserving every historical task packet as a separate file.

## Compression Rule

This rollup replaces individual completed task packet files for work 0001 through 0294. Future completed active packets should be folded into this file, then removed from `agent/`.

## State At Compression

| Area | State |
| --- | --- |
| Completed work covered | 0001-0294 |
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
- Direct unary numeric imported C# method and indexer arguments now infer their signed constant values for overload/indexer validation before generated C# emission.
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
- `agent/tasks.md` now keeps `User Task Inbox` user-editable at any time and limits the visible `Agent Task Queue` to the latest five rows.
- Goal-mode continuation now treats an empty active/user/queue/checklist state as a roadmap-refresh trigger rather than full project completion.
- Docs package dependencies are pinned to the current npm registry latest tags for Astro, Starlight, and TypeScript, with package contract coverage.
- Docs-owned site configuration is TypeScript and the docs contract rejects committed docs-owned JavaScript source/config files.
- TypeSharp source examples in docs use `tysh` code fences, and Starlight/Shiki reuses the VS Code TextMate grammar for syntax highlighting.
- Docs Mermaid rendering is enabled for architecture pages through docs-only `astro-mermaid` and `mermaid` dependencies.
- The Writing Guide adapts the Vue Docs Writing Guide into TypeSharp-specific authoring rules, `tysh` example project guidance, emoji usage, and review checks.
- Release artifact automation publishes tagged CLI, `net48` runtime library, VSIX, release notes, and SHA-256 manifest assets to GitHub Releases.
- Microsoft Learn C#-style detailed TypeSharp reference pages now explain CLR type mapping, value/reference/nullability/generic public type rules, member lookup, overload ranking, byref calls, delegates, events, extension methods, exception boundaries, and interop diagnostics.
- Feature Status now records the 2026-05-21 C# stable/preview parity review, including C# 14 Stable Backlog/Replacement/Experimental classifications and C# 15 Preview Watch boundaries.
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

## Task 0272 Inferred Lambda-Valued Export Let

Completed compiler/module work established:

- Unannotated lambda-valued top-level `let` declarations are now exportable and lowerable when used with direct `export`, local export aliases, source module imports, and import aliases.
- The source module graph now treats lambda-valued top-level values as lowerable export surface, including aliases, so relative imports can validate them instead of reporting `TS0114`.
- The C# backend now emits conservative delegate fields/properties for unannotated lambda-valued top-level values. The generated CLR shape uses `System.Func<object, TResult>`, with simple return inference for literals, `nameof`, checked expressions, and comparison expressions; identity or otherwise unknown bodies return `object`.
- Existing explicitly annotated function-valued `let` exports still preserve precise `System.Func<T, TResult>` or `System.Action<T>` metadata.
- `TS2003` descriptor metadata, Modules, Reference, Grammar, Lowering, and Work Ledger docs now distinguish supported unannotated lambda-valued exports from still-unsupported non-relative and non-lowerable forwarding forms.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "source module graph collects inferred function value export surface"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers inferred function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "source module graph collects function value export surface"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports unsupported export forwarding diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI check emits JSON unsupported export forwarding diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic descriptor registry"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Binding/TypeSharpBinder.cs`
- `lang/TypeSharp.Compiler/Projects/SourceModuleGraph.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs`
- [Modules And Imports](../docs/src/content/docs/modules.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0273 Lambda Null-Coalescing Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now infers lambda body return types for null-coalescing expressions such as `item => item.Name ?? "fallback"` when the fallback or receiver-side expression has a known metadata-backed type.
- The overload resolver rejects incompatible delegate return targets instead of treating unknown `??` lambda bodies as broadly applicable, so invalid calls report `TS2406` before generated C# emission.
- Compatible overload candidates rank through the existing known-return scoring path, preserving C# 7.3-compatible generated lambda syntax and normal `??` lowering.
- The local legacy metadata fixture now includes string/int delegate overloads and an int-only negative target for null-coalescing lambda bodies.
- .NET interop and C# members docs now list null-coalescing lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate coalesce return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda coalesce return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload coalesce return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda coalesce return overload"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0274 Lambda Indexer Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now infers lambda body return types for indexer expressions such as `item => item[0]` when the receiver is a known array or an imported metadata-backed C# type with a compatible public indexer.
- Imported C# indexer return metadata participates in delegate return filtering/ranking, so incompatible delegate return targets report `TS2406` before generated C# emission.
- Indexer argument matching reuses the existing known literal, numeric conversion, object fallback, and metadata relationship scoring path used by C# overload resolution.
- The local legacy metadata fixture now includes string/int delegate overloads over `LegacyFormatter` indexer returns and an int-only negative target.
- .NET interop and C# members docs now list indexer-expression lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate indexer return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda indexer return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload indexer return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda indexer return overload"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0275 Lambda Binary Value Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now infers lambda body return types for binary value expressions such as `item => item.Name + "!"` when operands have known literal, metadata-backed member, or recursively inferred lambda body types.
- String concatenation returns `string`, and numeric arithmetic over known numeric operands returns the promoted numeric result for delegate return filtering/ranking.
- Incompatible binary value delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes string/int delegate overloads for `PickBinaryValueReturn` and an int-only negative target.
- .NET interop and C# members docs now list binary value lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate binary value return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda binary value return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload binary value return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda binary value return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0276 Lambda Nameof Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now infers lambda body return types for `nameof` expressions such as `item => nameof(item.Name)`.
- `nameof` lambda bodies return `string` for delegate return filtering/ranking, so incompatible `Func<T, int>` targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes string/int delegate overloads for `PickNameofReturn` and an int-only negative target.
- .NET interop and C# members docs now list `nameof` lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate nameof return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda nameof return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload nameof return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda nameof return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0277 Lambda Checked Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now unwraps checked/unchecked overflow-context lambda bodies such as `text => checked(1 + 1)` and `text => unchecked(1 + 1)`.
- The inner expression type participates in delegate return filtering/ranking, so incompatible targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes checked/unchecked string/int delegate overloads and an unchecked string-only negative target.
- .NET interop and C# members docs now list checked/unchecked lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate checked return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate unchecked return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda unchecked return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload checked return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda unchecked return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0278 Lambda Satisfies Overload Inference

Completed C# interop work established:

- C# delegate overload filtering now unwraps `satisfies` proof expressions in lambda bodies such as `text => text satisfies string`.
- The proved expression type participates in delegate return filtering/ranking while generated C# keeps proof-erasure behavior.
- Incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes string/int delegate overloads for `PickSatisfiesReturn` and an int-only negative target.
- .NET interop and C# members docs now list `satisfies` lambda body return inference as part of the implemented contextual delegate subset.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate satisfies return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda satisfies return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload satisfies return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda satisfies return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`

## Task 0279 Lambda Parenthesized Overload Inference

Completed parser, lowering, and C# interop work established:

- Parenthesized expressions now have a dedicated `ParenthesizedExpression` syntax node instead of being misclassified as `IdentifierExpression`.
- Generated C# preserves grouping parentheses for ordinary expressions such as `(1 + 1)`.
- Type inference and top-level lambda-valued export inference unwrap parenthesized expressions while preserving the enclosed expression type.
- C# delegate overload filtering/ranking now unwraps parenthesized lambda bodies such as `text => (text)`, so incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes string/int delegate overloads for `PickParenthesizedReturn` and an int-only negative target.
- Grammar, reference, lowering, .NET interop, and C# members docs now list parenthesized expression lowering and parenthesized lambda body return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate parenthesized return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda parenthesized return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles parenthesized expression lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload parenthesized return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda parenthesized return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/fixtures/parser/positive/0032-parenthesized-expression`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0280 Lambda Logical-Not Overload Inference

Completed parser, lowering, and C# interop work established:

- The lexer now tokenizes standalone `!` as `BangToken` while preserving longest-match lexing for `!=`.
- The parser treats `!expr` as a unary expression using the existing unary `BinaryExpression` shape.
- Generated C# emits unary logical-not expressions as `!operand`.
- Type inference and top-level lambda-valued export inference treat `!boolOperand` bodies as `bool`.
- C# delegate overload filtering/ranking now treats lambda bodies such as `flag => !flag` as `bool` when the operand is known `bool`, so incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `PickLogicalNotReturn` overloads and a `RequiresLogicalNotReturnString` negative target.
- Grammar, reference, lowering, .NET interop, and C# members docs now list unary logical-not lowering and logical-not lambda body return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate logical not return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda logical not return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload logical not return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda logical not return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/fixtures/parser/positive/0033-logical-not-expression`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0281 Lambda Unary Numeric Overload Inference

Completed lowering, inference, and C# interop work established:

- Type inference now preserves supported unary numeric sign expression result types for `+expr` and `-expr`, including integral promotion to `int` for smaller signed/unsigned operands.
- Top-level unannotated lambda-valued exports infer `System.Func<object, int>` for unary numeric literal bodies such as `text => -1`.
- C# delegate overload filtering/ranking now treats lambda bodies such as `value => -value` and `value => +value` as the supported numeric operand return type when the operand is known from delegate metadata.
- Incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `PickUnaryNumericReturn` overloads and a `RequiresUnaryNumericReturnString` negative target.
- Grammar, reference, lowering, .NET interop, and C# members docs now list unary numeric sign lowering and unary numeric lambda body return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers inferred function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate unary numeric return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda unary numeric return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload unary numeric return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda unary numeric return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0282 Lambda If Overload Inference

Completed lowering, inference, and C# interop work established:

- Value-producing `if` expressions now lower in expression position through a C# 7.3-compatible immediately invoked `System.Func<T>` block.
- Top-level unannotated lambda-valued exports infer `System.Func<object, string>` for compatible `if` branch bodies such as `text => if true { "yes" } else { "no" }`.
- C# delegate overload filtering/ranking now treats lambda bodies such as `text => if text == "Ada" { "match" } else { "fallback" }` as the merged branch result type when branches infer one known type.
- Incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `PickIfReturn` overloads and a `RequiresIfReturnInt` negative target.
- Grammar, reference, lowering, .NET interop, and C# members docs now list value-producing `if` expression lowering and `if` lambda body return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers inferred function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate if return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda if return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload if return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda if return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0283 Lambda Block Overload Inference

Completed lowering, inference, and C# interop work established:

- Block-bodied lambdas now lower to C# block lambdas and return the final block expression for value-returning delegate targets.
- Top-level unannotated lambda-valued exports infer `System.Func<object, TResult>` through block final expressions such as `text => { "block" }`.
- C# delegate overload filtering/ranking now unwraps block lambda bodies and checks the final expression type, including parenthesized identity returns such as `text => { (text) }`.
- Incompatible delegate return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `PickBlockReturn` overloads and a `RequiresBlockReturnInt` negative target.
- Grammar, reference, lowering, .NET interop, and C# members docs now list block lambda lowering and block final-expression return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda block return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate block return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda block return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload block return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers inferred function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0284 Lambda Collection Overload Inference

Completed lowering, inference, and C# interop work established:

- Collection expression lambda bodies now infer array/List-compatible return types for C# delegate overload filtering/ranking.
- Non-block lambda lowering now passes known delegate parameter and return context into body emission, so annotated function-valued exports like `text => [text]` lower to `new string[] { text }`.
- Unannotated lambda-valued exports infer `System.Func<object, string[]>` for literal collection bodies like `text => ["tag"]`.
- Incompatible delegate collection return targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `PickCollectionReturn` overloads and `RequiresCollectionReturnIntArray`.
- Grammar, reference, lowering, .NET interop, and C# members docs now list collection lambda return inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# delegate lambda collection return overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters lambda delegate collection return type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on no matching C# delegate lambda collection return overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload collection return match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build lowers inferred function value exports"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0285 Collection Argument Overload Inference

Completed C# interop work established:

- Homogeneous collection expression arguments now infer array argument types for imported C# overload filtering/ranking.
- Incompatible collection expression array targets report `TS2406` before generated C# emission.
- The local legacy metadata fixture now includes `LegacyCollectionOverloads.PickValues` string/int array overloads and a string-array-only negative target.
- CLI build coverage proves `LegacyCollectionOverloads.PickValues(["Ada"])` emits and compiles as a `net48` imported C# call.
- Grammar, lowering, .NET interop, and C# members docs now list collection expression array argument overload inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# overload for collection expression argument diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters collection expression argument type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on collection expression C# overload mismatch"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported collection expression overload match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0286 Null Indexer Overload Inference

Completed C# interop work established:

- Imported C# instance indexer validation now infers `null` literal argument types.
- `null` indexer arguments reject non-nullable value-type indexers and report `TS2411` before generated C# emission.
- Overloaded imported indexers rank concrete reference/nullable `null` targets ahead of `object` fallback and use metadata specificity to break applicable ties.
- The local legacy metadata fixture now includes `LegacyNullIndexer` with `string`, `object`, and `int` indexers.
- CLI build coverage proves `LegacyNullIndexer()[null]` emits and compiles as a `net48` imported C# indexer access.
- .NET interop and C# members docs now list `null` literal indexer filtering/ranking as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports mismatched C# instance indexer null literal diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker accepts imported C# indexer null literal ranking"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on mismatched C# instance indexer null literal"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported indexer null literal match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0287 Collection Generic Array Inference

Completed C# interop work established:

- Homogeneous collection expression arguments now infer `T` for imported C# generic method parameters shaped like `T[]`.
- Generic array candidates with metadata parameter `!!0[]` remain applicable when the collection argument infers a known array type.
- Incompatible inferred `T` constraints such as `RequireClassArray([1])` report `TS2417` before generated C# emission.
- The local legacy metadata fixture now includes `IdentityArray<T>(T[] values)` and `RequireClassArray<T>(T[] values) where T : class`.
- CLI build coverage proves `RequireClassArray(["Ada"])` emits and compiles as `new string[] { "Ada" }` in `net48` generated C#.
- .NET interop and C# members docs now list collection expression generic array inference as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker accepts inferred C# generic array constraints"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports unsatisfied inferred C# generic array constraint diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on unsatisfied inferred C# generic array constraint"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported inferred generic array constraint call"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0288 Params Collection Expression Array Overload

Completed C# interop work established:

- A single homogeneous collection expression in an imported C# `params T[]` position is now checked against the full params array type before falling back to expanded element matching.
- `LegacyParamsOverloads.Pick(",", ["a", "b"])` ranks the `params string[]` candidate ahead of the expanded `params object[]` fallback.
- Expanded params calls such as `LegacyParamsOverloads.Pick(",", "a", "b")` keep their element-type matching path.
- Checker coverage proves compatible params collection expression calls do not report `TS2402` or `TS2406`.
- CLI build coverage proves the call emits and compiles as `LegacyParamsOverloads.Pick(",", new string[] { "a", "b" })` in `net48` generated C#.
- C# members, lowering, .NET interop, and Work Ledger docs list single collection expression params-array ranking as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver treats collection expression as params array"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker accepts imported params collection expression array overload"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported params collection expression array match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters collection expression argument type"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles exact expanded params overload match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports ambiguous expanded params overload diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported params call"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0289 Parenthesized Indexer Argument Validation

Completed C# interop work established:

- Imported C# interop validation now unwraps parenthesized argument expressions before metadata-backed argument inference.
- Parenthesized imported indexer arguments such as `formatter[(true)]` now preserve generated C# grouping but use the enclosed `bool` expression for metadata validation.
- Mismatched parenthesized indexer arguments report `TS2411` before generated C# emission instead of falling through to generated project build failure `TS3501`.
- Compatible parenthesized indexer arguments such as `formatter[(2)]` continue to emit and compile as grouped C# indexer access.
- C# members, lowering, .NET interop, and Work Ledger docs list parenthesized imported indexer argument validation as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports mismatched C# instance indexer parenthesized argument diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on mismatched C# instance indexer parenthesized argument"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported indexer parenthesized argument"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports mismatched C# instance indexer diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on mismatched C# instance indexer"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported indexer access"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0290 Parenthesized Overload Argument Unwrapping

Completed C# interop work established:

- Imported C# overload resolution now unwraps parenthesized argument expressions through the same common path used for named and byref arguments.
- Parenthesized `null` overload arguments such as `LegacyNullOverloads.DescribeNamed((null))` participate in metadata-specificity ranking and select the nearest reference target before generated C# emission.
- Parenthesized lambda arguments such as `LegacyDelegateOverloads.PickReturn("Ada", (text => text))` participate in delegate arity and return filtering/ranking.
- Generated C# still preserves source grouping for parenthesized overload arguments.
- C# members, lowering, .NET interop, and Work Ledger docs list parenthesized overload argument unwrapping as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver ranks parenthesized null literal nearest metadata reference"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters parenthesized lambda delegate argument"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles parenthesized null literal metadata relationship overload match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported delegate lambda overload parenthesized argument match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0291 Task Queue Retention Policy

Completed agent workflow work established:

- `agent/tasks.md` now states that users may edit `User Task Inbox` at any time during agent execution.
- `agent/tasks.md` now keeps only the latest five visible `Agent Task Queue` rows.
- Older completed queue rows remain available through this compressed rollup instead of staying in the active task index.
- `agent/agentic-execution.md` and docs Agentic Workflow now define the same inbox ownership and queue retention rules.
- Docs Work Ledger now reports completed work through task 0291 and no active task selected.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [tasks.md](tasks.md)
- [agentic-execution.md](agentic-execution.md)
- [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

## Task 0292 Unary Numeric CSharp Argument Inference

Completed C# interop work established:

- Direct unary numeric imported C# method arguments such as `LegacyNumeric.FormatSByte(-1)` now infer their signed constant value for overload filtering/ranking.
- Impossible unary numeric constant conversions such as `LegacyNumeric.FormatByte(-1)` report `TS2406` before generated C# emission.
- Imported C# indexer validation now applies the same signed unary numeric inference, so mismatches such as `LegacyByteIndexer()[-1]` report `TS2411` before emission.
- Signed integral constant conversion checks now preserve lower bounds for `byte`, `sbyte`, `short`, `ushort`, `uint`, and `ulong` instead of treating all integral literal text as unsigned.
- .NET interop and C# members docs list direct unary numeric overload arguments and unary numeric imported indexer argument validation as implemented behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# overload resolver filters unary numeric argument conversion"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports no matching C# overload for unary numeric argument diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports mismatched C# instance indexer unary numeric diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on unary numeric C# overload mismatch"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build stops before emission on mismatched C# instance indexer unary numeric"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles unary numeric constant conversion"
npm run build
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Interop/TypeSharpCSharpOverloadResolver.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)

## Task 0293 Language Ecosystem Roadmap

Completed roadmap and agent workflow work established:

- Refreshed official C#, F#, TypeScript, .NET Framework, modern .NET, NuGet, and VS Code ecosystem sources on 2026-05-21.
- Recorded the key TypeSharp implication: parent-language features are adopted only when they improve the goal and have clear `net48` lowering, public ABI behavior, diagnostics, and tooling impact.
- Classified C# as the interop/ABI anchor, F# as the functional-consistency benchmark, TypeScript as the flexible type/module/tooling benchmark, and .NET/NuGet/VS Code as the adoption ecosystem constraint set.
- Updated `agent/tasks.md` with the next latest-five roadmap slice: C# stable/preview parity, TypeScript structural/module planning, F# functional consistency planning, and .NET ecosystem tooling planning.
- Updated `agent.md`, `agent/agentic-execution.md`, and docs Agentic Workflow so goal-mode agents create a Q1 roadmap-refresh task when the active task, user inbox, agent queue, and checklist would otherwise look empty.
- Marked the user inbox request complete while preserving the task text.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [agent.md](../agent.md)
- [tasks.md](tasks.md)
- [agentic-execution.md](agentic-execution.md)
- [Agentic Workflow](../docs/src/content/docs/agentic-workflow.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- Official references listed in [Project Policy](../docs/src/content/docs/project-policy.md)

## Task 0294 CSharp Stable Preview Parity Plan

Completed C# roadmap work established:

- Refreshed official Microsoft C# language versioning, C# 14, and C# 15 sources on 2026-05-21.
- Recorded the target boundary that .NET Framework projects default to C# 7.3, so TypeSharp generated `net48` source remains C# 7.3-compatible even when later C# releases inform TypeSharp design.
- Added a C# stable/preview parity review to Feature Status.
- Classified C# 14 extension members, null-conditional assignment, unbound generic `nameof`, simple lambda parameter modifiers, partial constructors/events, and user-defined compound assignment as Stable Backlog where TypeSharp semantics and C# 7.3 lowering can be designed independently.
- Classified C# 14 field-backed properties as Replacement, C# 14 file-based app directives as Rejected for MVP, C# 14 `Span<T>` conversions as Experimental, and C# 15 collection expression arguments and union types as Preview Watch.
- Updated C# members and .NET interop docs to state that C# 14/15 syntax must not be emitted for ordinary `net48` artifacts.
- Queued `0298 C# unbound generic nameof parity` as the next concrete C# follow-up because it can lower to C# 7.3-compatible string constants.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [tasks.md](tasks.md)

## Task 0295 TypeScript Structural Module Roadmap

Completed TypeScript roadmap work established:

- Refreshed official TypeScript sources on 2026-05-21, including Type Compatibility, Narrowing, Creating Types from Types, Modules Reference, Project References, Type Declarations, TSConfig Reference, and TypeScript 6.0/5.9 release notes.
- Added a Feature Status TypeScript structural/module review that keeps TypeSharp's structural typing local, deterministic, and separate from public CLR metadata.
- Classified structural object compatibility, `unknown`/type guards/discriminated narrowing, type aliases/interfaces, `keyof`, indexed access, mapped/conditional/template-literal/utility types, ES modules, project references, declaration files, TSConfig, and TypeScript 6.0/5.9 signals against TypeSharp's `net48` artifact boundary.
- Updated Type System docs with the TypeScript-style structural roadmap: local shape proofs first, bounded structural discriminant narrowing next, optional/index-signature diagnostics later, and advanced type operators behind a budgeted evaluator.
- Updated Modules and Project Configuration docs to keep TypeSharp source modules and `TypeSharp.toml` manifest behavior distinct from Node, bundler, CommonJS, package `exports`, `paths`, JavaScript/JSX source inclusion, and npm declaration lookup.
- Queued follow-up tasks 0300, 0301, and 0302 for structural discriminant narrowing, source module alias/project-reference policy, and advanced type-operator evaluator budgeting.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Modules And Imports](../docs/src/content/docs/modules.md)
- [Project Configuration](../docs/src/content/docs/project-configuration.md)
- [tasks.md](tasks.md)

## Task 0296 FSharp Functional Consistency Roadmap

Completed F# roadmap work established:

- Refreshed official Microsoft F# documentation, strategy, F# 10 release notes, functions, pattern matching, discriminated unions, options, computation expressions, and task expressions on 2026-05-21.
- Added a Feature Status F# functional consistency review that keeps TypeSharp independent of F# syntax compatibility and `FSharp.Core` runtime dependencies by default.
- Classified immutable values, expression-result functions, local inference, first-class functions, records, nominal unions, `Option<T>`, `Result<T,E>`, pattern matching, pipeline, composition, and direct `Task` interop against the MVP path.
- Classified richer currying/partial application, computation-expression-style workflows, active-pattern-style extractors, F# interop layers, and recursive union ergonomics as Stable Backlog until generated delegate shapes, lowering, and diagnostics are explicit.
- Kept type providers and units of measure Experimental because they need security, reproducibility, erasure, operator, ABI, and diagnostic policies.
- Updated Grammar, Type System, Lowering, Project Policy, Work Ledger, and task state to reflect the roadmap.
- Queued `0299 Match exhaustiveness expansion` as the next bounded F#-driven Q2 implementation slice.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [tasks.md](tasks.md)

## Task 0298 CSharp Unbound Generic Nameof Parity

Completed language/backend work established:

- Added parser support for unbound generic arity targets inside `nameof`, including `nameof(Box<>)` and higher-arity placeholders such as `nameof(Pair<,>)`.
- Kept unbound generic syntax unsupported outside `nameof` through parser diagnostics.
- Extended `nameof` binding so unbound generic targets resolve their type-root name instead of being treated as ordinary value expressions, with unresolved roots still reporting `TS2001`.
- Lowered unbound generic `nameof` targets to string constants such as `"Box"` and `"Pair"` so generated `net48` source remains C# 7.3-compatible and never emits C# 14 `nameof(Box<>)` syntax.
- Updated Feature Status, Grammar, Reference, Lowering, C# Members And Overloads, .NET Interop, and Work Ledger docs for the implemented boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "parser parses unbound generic nameof without diagnostics"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "parser rejects unbound generic outside nameof"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "binder reports unresolved unbound generic nameof target"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "CLI build compiles nameof intrinsic"
npm run build
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpBinder.cs](../lang/TypeSharp.Compiler/Binding/TypeSharpBinder.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [Program.cs](../test/TypeSharp.Compiler.Tests/Program.cs)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [tasks.md](tasks.md)

## Task 0299 Match Exhaustiveness Expansion

Completed match exhaustiveness work established:

- Added nominal union `_` discard-arm handling to the type checker so a known union match can cover remaining cases without reporting `TS2203`.
- Preserved deterministic missing-case diagnostics for nominal unions that still omit known cases, and kept existing local type-level union exhaustiveness behavior intact.
- Lowered nominal union `_` arms to unconditional C# fallback returns in source order, matching the existing type-level union discard lowering shape while preserving `net48` C# 7.3 output.
- Added a positive type-checker fixture for nominal union discard exhaustiveness and expanded the nominal union backend snapshot to cover discard fallback lowering.
- Updated diagnostic descriptor metadata and canonical docs for the supported boundary, including marking `when` guards as planned until parser/checker support lands together.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "type checker fixture diagnostics match"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "C# backend fixture snapshots match"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "CLI build stops before emission on non-exhaustive match"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "CLI build compiles nominal union match lowering"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [DiagnosticDescriptors.cs](../lang/TypeSharp.Compiler/Diagnostics/DiagnosticDescriptors.cs)
- [union-match-discard](../test/fixtures/diagnostics/type-checker/positive/union-match-discard)
- [0018-nominal-union-match-lowering](../test/fixtures/backend/csharp/positive/0018-nominal-union-match-lowering)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [tasks.md](tasks.md)

## Task 0300 Structural Discriminant Narrowing Slice

Completed type-checker work established:

- Added bounded branch-scope narrowing for `if value.Tag == "literal"` and `if value.Tag != "literal"` when `value` has a local type-level union type and every union member resolves to a shape with a required literal discriminant member.
- Narrowed the original variable to the matching structural member type when a branch leaves exactly one union member, enabling member access such as `shape.radius` or `shape.side` inside the corresponding branch.
- Added a deterministic `TS2201` diagnostic when the checked discriminant literal is impossible for a fully known discriminated structural/type-level union.
- Kept the feature local-only; public structural and type-level union boundary diagnostics are unchanged.
- Added positive and negative type-checker fixtures for structural discriminant narrowing and impossible discriminant checks.
- Updated Feature Status and Type System docs for the supported equality/inequality boundary and the remaining boolean-algebra backlog.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "type checker fixture diagnostics match"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [structural-discriminant-narrowing](../test/fixtures/diagnostics/type-checker/positive/structural-discriminant-narrowing)
- [structural-discriminant-impossible](../test/fixtures/diagnostics/type-checker/negative/structural-discriminant-impossible)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [tasks.md](tasks.md)

## Task 0301 Source Module Alias And Project Reference Policy

Completed docs/policy work established:

- Refreshed official TypeScript `paths`, TypeScript project references, and MSBuild `ProjectReference` reference signals for the module/configuration boundary.
- Documented reserved `[modules.aliases]` policy: aliases must be manifest-owned, deterministic, source graph affecting, and pre-emission diagnostic producing instead of TypeScript-style type-check-only paths.
- Documented reserved `[projectReferences]` policy: each path names another `TypeSharp.toml`; referenced projects build first and are consumed through generated `net48` assemblies plus explicit export metadata.
- Kept current preview behavior clear: relative imports remain the active source module feature, `[projectReferences]` is not implemented, and current external CLR metadata still uses `references.paths`.
- Queued bounded follow-up tasks for manifest source alias diagnostics (0303) and TypeSharp project reference build graph implementation (0304).

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [Modules And Imports](../docs/src/content/docs/modules.md)
- [Project Configuration](../docs/src/content/docs/project-configuration.md)
- [Runtime Artifacts](../docs/src/content/docs/runtime-artifacts.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0303 Manifest Source Alias Diagnostics

Completed implementation work established:

- Added manifest project options for `[modules.aliases]` with source alias records, TOML string-map loading, and invalid/duplicate manifest value diagnostics.
- Implemented current-project source alias validation in the source module graph: bare specifier keys, project-relative targets, source-root containment, one-wildcard shape, case-insensitive collision checks, longest-prefix precedence, ambiguous match diagnostics, and unresolved expanded-target diagnostics.
- Routed source aliases through checker, builder, binder, interop validation, type checking, and C# backend import classification so alias imports and re-exports resolve through existing source module dependency targets instead of external CLR imports.
- Extended generated C# target collection for alias-backed function, value, type, module, and star re-export barrels.
- Updated canonical docs and ledgers to mark current-project manifest aliases implemented while keeping `[projectReferences]` as the remaining source-module gap.

Verification:

```powershell
dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --no-build --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- "source alias"
dotnet run --no-build --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Primary evidence:

- [ModuleOptions.cs](../lang/TypeSharp.Compiler/Projects/ModuleOptions.cs)
- [TypeSharpManifestLoader.cs](../lang/TypeSharp.Compiler/Projects/TypeSharpManifestLoader.cs)
- [MinimalTomlDocument.cs](../lang/TypeSharp.Compiler/Projects/MinimalTomlDocument.cs)
- [SourceModuleGraph.cs](../lang/TypeSharp.Compiler/Projects/SourceModuleGraph.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [Program.cs](../test/TypeSharp.Compiler.Tests/Program.cs)
- [Modules And Imports](../docs/src/content/docs/modules.md)
- [Project Configuration](../docs/src/content/docs/project-configuration.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [tasks.md](tasks.md)

## Task 0304 TypeSharp Project Reference Build Graph

Completed implementation work established:

- Added manifest options and TOML loading for `[projectReferences] paths = [...]`, including stable source locations for diagnostics.
- Implemented direct TypeSharp project-reference graph loading with missing manifest, invalid referenced manifest, cycle, duplicate direct project name, and target-framework compatibility diagnostics.
- Derived direct referenced source-module export metadata from referenced manifests and source graphs, including nested referenced-project imports inside referenced projects.
- Routed direct project source specifiers such as `"Shared/Api"` through checker, source graph validation, interop validation, type checking, generated C# source imports, and generated project reference resolution.
- Implemented `typesharp build` ordering so direct referenced projects build before dependents and dependent generated C# projects consume referenced generated assemblies through explicit local `<Reference>` hint paths.
- Preserved direct-reference visibility: hidden transitive source imports are rejected, and cross-project `export ... from` forwarding remains unsupported until richer project-reference re-export metadata is designed.
- Updated CLI, module, configuration, runtime artifact, diagnostics, feature status, API, and work-ledger docs for the implemented boundary and remaining gaps.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "project reference"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "transitive"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj -- "source alias"
dotnet run --no-build --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build
git diff --check
```

Primary evidence:

- [ProjectReferenceOptions.cs](../lang/TypeSharp.Compiler/Projects/ProjectReferenceOptions.cs)
- [TypeSharpProjectReferenceResolver.cs](../lang/TypeSharp.Compiler/Projects/TypeSharpProjectReferenceResolver.cs)
- [ProjectReferenceResolutionResult.cs](../lang/TypeSharp.Compiler/Projects/ProjectReferenceResolutionResult.cs)
- [SourceModuleGraph.cs](../lang/TypeSharp.Compiler/Projects/SourceModuleGraph.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- [TypeSharpChecker.cs](../lang/TypeSharp.Compiler/Checking/TypeSharpChecker.cs)
- [TypeSharpReferenceResolver.cs](../lang/TypeSharp.Compiler/Interop/TypeSharpReferenceResolver.cs)
- [Program.cs](../test/TypeSharp.Compiler.Tests/Program.cs)
- [Modules And Imports](../docs/src/content/docs/modules.md)
- [Project Configuration](../docs/src/content/docs/project-configuration.md)
- [Runtime Artifacts](../docs/src/content/docs/runtime-artifacts.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [tasks.md](tasks.md)

## Task 0302 Advanced Type Operator Evaluator Budget

Completed design work established:

- Documented the future advanced type-operator evaluator budget for mapped, conditional, template-literal, and utility type computation.
- Set first-implementation limits: 16 alias instantiation depth, 512 evaluator reductions per root alias, 64 normalized union members, 64 mapped keys, 64 conditional distribution branches, 128 template-literal products, and 8 direct evaluator diagnostics per root alias.
- Restricted evaluator inputs to TypeSharp-owned type facts and banned user-code execution, package restore, TypeScript declaration-file compatibility, runtime-value inspection, and C# overload-set inference during type computation.
- Defined utility type admission rules for future TypeSharp-owned versions of `Pick`, `Omit`, `Readonly`, `Mutable`, `Partial`, `Required`, `Record`, `Extract`, `Exclude`, `NonNullable`, `ReturnType`, and `Parameters`.
- Kept public ABI safe: advanced computed types may appear in public signatures only after normalizing to CLR-visible metadata; structural, union, template-generated, or unresolved computed results remain local-only and must diagnose before emission.
- Added planned advanced type-operator diagnostic classes without allocating unimplemented descriptor codes.

Verification:

```powershell
npm run build
git diff --check
```

Primary evidence:

- [Type System](../docs/src/content/docs/type-system.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Advanced Topics](../docs/src/content/docs/advanced.md)
- [tasks.md](tasks.md)

## Task 0297 .NET Ecosystem Tooling Roadmap

Completed roadmap work established:

- Refreshed official Microsoft Learn and VS Code sources for .NET Framework target framework monikers, `.NET Framework 4.8.1` deployment/support state, NuGet `PackageReference` lock files, `dotnet restore` auditing, package source mapping, trusted signers, `dotnet new` custom templates, VS Code language server extensions, and VSIX/Marketplace publishing.
- Added a .NET ecosystem tooling roadmap to Project Policy that keeps `references.packages` reserved until TypeSharp has deterministic package metadata, checked-in lock files, locked-mode CI, explicit package sources, source mapping, vulnerability audit policy, license inventory, checksum/signature policy, transitive dependency policy, offline-cache behavior, and a no-user-code boundary for `typesharp check`.
- Clarified target profile rules: `net48` remains the broad stable generated target, while `net481` is a future qualified profile requiring explicit manifest/CLI admission, target-pack/deployment assumptions, Core/Runtime builds, generated project smokes, C# consumer smokes, and host compatibility evidence.
- Documented template and tooling gates: built-in `typesharp new` templates remain the current path, future `dotnet new` packs require release packaging policy, and VS Code/LSP behavior must stay aligned with CLI diagnostics, formatting, semantic hover, definition, completion, versioning, runtime ABI, and VSIX smoke tests.
- Clarified release/adoption gates for CLI zip, runtime/core `net48` zip, VSIX, release notes, SHA-256 checksums, future NuGet/template/Marketplace publication, and rollback paths.
- Updated Project Configuration, Requirements, .NET Interop, Runtime Artifacts, CLI, VS Code And LSP, Feature Status, Work Ledger, task state, and traceability to point at the new policy boundary.
- Queued `0305 Roadmap refresh after ecosystem plan` as the next Q1 roadmap-refresh item because the visible queue would otherwise be empty.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands passed on 2026-05-22.

Primary evidence:

- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Project Configuration](../docs/src/content/docs/project-configuration.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Runtime Artifacts](../docs/src/content/docs/runtime-artifacts.md)
- [CLI](../docs/src/content/docs/cli.md)
- [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0305 Roadmap Refresh After Ecosystem Plan

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after the .NET ecosystem tooling roadmap.
- Confirmed that TypeSharp's baseline remains unchanged: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 and TypeScript 7.0 remain preview/tooling-direction signals, and package/Marketplace/template publication remains gated by Project Policy.
- Updated Feature Status with TypeScript 6.0 and TypeScript 7.0 Beta source links, clarified TypeScript native-port boundaries, recorded F# 10 diagnostic/async/parallel-compiler signals, and restated that C# 15 collection expression arguments and union types are directional only.
- Compared the refreshed signals against Work Ledger remaining future areas and selected `match` arm `when` guard support as the next bounded language implementation slice.
- Queued task `0306 Match guard implementation slice` for parser, checker, and lowering work over nominal and local type-level union matches where guarded arms do not prove exhaustiveness without an unguarded cover.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0306 Match Guard Implementation Slice

Completed match guard implementation work established:

- Added `when` as a reserved parser keyword for match arms and kept single-identifier guards from being parsed as lambdas before the arm `=>`.
- Type-checked match guard expressions as `bool` inside the narrowed arm scope for nominal union payload bindings and local type-level union pattern variables.
- Preserved exhaustiveness behavior by treating guarded nominal and type-level union arms as non-covering unless a later unguarded arm or `_` discard covers the remaining closed set.
- Lowered guarded nominal union arms, guarded type-level union arms, and guarded `_` arms to C# 7.3-compatible conditional code in source order.
- Added parser, type-checker positive/negative, backend snapshot, and CLI build smoke coverage for guarded matches, non-boolean guards, and guarded-only non-exhaustive matches.
- Updated canonical grammar, feature status, type-system, lowering, reference, diagnostics, and work-ledger docs for the implemented boundary.

Verification:

```powershell
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [Grammar](../docs/src/content/docs/grammar.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- `test/fixtures/parser/positive/0003-unions-patterns`
- `test/fixtures/diagnostics/type-checker/positive/match-guards`
- `test/fixtures/diagnostics/type-checker/negative/match-guard-non-bool`
- `test/fixtures/diagnostics/type-checker/negative/guarded-only-non-exhaustive-match`
- `test/fixtures/backend/csharp/positive/0018-nominal-union-match-lowering`
- `test/fixtures/backend/csharp/positive/0019-type-level-union-narrowing`
- `test/TypeSharp.Compiler.Tests/Program.cs`

Remaining:

- Enum exhaustiveness and richer pattern algebra remain future match work.

## Task 0307 Roadmap Refresh After Match Guards

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework lifecycle, NuGet, and VS Code sources on 2026-05-21 after match guard implementation.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 and TypeScript 7.0 remain preview/tooling-direction signals, and package/Marketplace/template publication remains gated by Project Policy.
- Recorded the refresh in Feature Status and Work Ledger, including the continued separation between stable TypeSharp semantics and preview external language/runtime features.
- Selected the next bounded implementation slice: literal pattern parsing plus bool and local literal-union match exhaustiveness/lowering, with enum exhaustiveness and richer pattern algebra remaining separate follow-ups.
- Created active task `0308 Literal match exhaustiveness slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0308 Literal Match Exhaustiveness Slice

Completed language/compiler work established:

- Added parser support for `true`, `false`, string, and numeric literal patterns in `match` arms.
- Added `bool` match exhaustiveness diagnostics that track unguarded `true`/`false` coverage, allow unguarded `_` fallback coverage, and keep guarded literal arms non-covering.
- Added local literal type-level union match exhaustiveness diagnostics for string, numeric, and bool literal members, including deterministic missing-member messages and incompatible literal-pattern diagnostics.
- Lowered supported literal matches to C# 7.3-compatible immediately invoked `System.Func<T>` delegates with ordered `object.Equals` comparisons and runtime `TypeSharpPattern.NoMatch` fallback.
- Preserved compile-time-only public ABI boundaries for local literal unions; exported wrappers continue to expose CLR-visible signatures.
- Added parser, type-checker positive/negative, backend snapshot, and CLI `net48` build/consumer smoke coverage for literal pattern parsing, bool/local literal-union coverage, guarded-only non-coverage, and C# lowering.
- Updated canonical grammar, feature status, type-system, lowering, reference, diagnostics, and work-ledger docs for the implemented boundary.

Verification:

```powershell
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0034-literal-match-patterns`
- `test/fixtures/diagnostics/type-checker/positive/literal-match-exhaustiveness`
- `test/fixtures/diagnostics/type-checker/negative/literal-match-non-exhaustive`
- `test/fixtures/backend/csharp/positive/0038-literal-match-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Enum exhaustiveness and richer pattern algebra remain future match work.

## Task 0309 Roadmap Refresh After Literal Match Exhaustiveness

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework lifecycle, NuGet, and VS Code sources on 2026-05-21 after literal match exhaustiveness landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 and TypeScript 7.0 remain preview/tooling-direction signals, and package/Marketplace/template publication remains gated by Project Policy.
- Confirmed the latest source signals still match Feature Status: C# language versioning maps .NET Framework targets to C# 7.3, F# 10 remains a refinement/tooling signal, TypeScript 7.0 Beta stays side-by-side through `@typescript/native-preview`, NuGet lock/audit/source-mapping constraints still apply, and VS Code extension publishing still uses `vsce`/Marketplace gates.
- Compared Feature Status, Grammar, and Work Ledger against the compiler state and selected the next bounded implementation slice: simple TypeSharp enum declarations and C# 7.3-compatible lowering as prerequisite groundwork for enum match exhaustiveness.
- Created active task `0310 Enum declaration implementation slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework)
- [dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0310 Enum Declaration Implementation Slice

Completed language/compiler work established:

- Added parser and lexer support for simple TypeSharp-owned enum declarations with `EnumDeclaration` and `EnumMember` syntax nodes.
- Bound enum declarations as type-space symbols and reported duplicate enum members deterministically with `TS2002`.
- Added enum symbol facts to type checking so same-enum member access such as `Color.Green` has the enum type, unrelated enum values report `TS2201`, and missing enum members report `TS2201`.
- Lowered supported enum declarations and enum member references to C# 7.3-compatible generated source using ordinary CLR enum declarations.
- Included enum declarations in local type export/source graph/build surfaces so generated assemblies can expose enum-backed public APIs.
- Added parser, binder, type-checker positive/negative, backend snapshot, and CLI `net48` build/consumer smoke coverage.
- Updated canonical grammar, reference, type-system, lowering, feature-status, diagnostics, and work-ledger docs for the implemented enum boundary.

Verification:

```powershell
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
git diff --check
```

Primary evidence:

- Full runner completed 516/516 PASS after the explicit test project build above.
- [SyntaxKind.cs](../lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs)
- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpBinder.cs](../lang/TypeSharp.Compiler/Binding/TypeSharpBinder.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- [SourceModuleGraph.cs](../lang/TypeSharp.Compiler/Projects/SourceModuleGraph.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/binder/negative/duplicate-enum-member`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-declaration-mismatch`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Enum match exhaustiveness, flags, explicit underlying types, explicit numeric enum values, enum member attributes, imported C# enum exhaustiveness, and richer pattern algebra remain future work.

## Task 0311 Roadmap Refresh After Enum Declarations

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework lifecycle, NuGet, and VS Code sources on 2026-05-21 after simple enum declarations landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Compared Feature Status and Work Ledger against the compiler state and selected the next bounded implementation slice: enum match exhaustiveness for TypeSharp-owned enums.
- Created active task `0312 Enum match exhaustiveness slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework)
- [dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

## Task 0312 Enum Match Exhaustiveness Slice

Completed language/compiler work established:

- Added TypeSharp-owned enum match exhaustiveness diagnostics for known enum input types.
- Treated unguarded enum member arms as coverage and unguarded `_` as covering the remaining enum members.
- Kept guarded enum member arms non-covering unless a later unguarded member arm or discard covers the same member space.
- Reported unknown enum member patterns with `TS2201` and non-exhaustive enum matches with `TS2203`.
- Lowered supported enum matches to C# 7.3-compatible immediately invoked `System.Func<T>` delegates with ordered enum member comparisons and runtime `TypeSharpPattern.NoMatch` fallback.
- Added type-checker positive/negative fixtures, backend snapshot coverage, and a CLI `net48` build/consumer smoke for generated enum match code.
- Updated canonical grammar, reference, type-system, lowering, feature-status, and diagnostics docs for the implemented enum match boundary.

Verification:

```powershell
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/diagnostics/type-checker/positive/enum-match-exhaustiveness`
- `test/fixtures/diagnostics/type-checker/negative/enum-match-non-exhaustive`
- `test/fixtures/backend/csharp/positive/0040-enum-match-exhaustiveness-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Imported C# enum exhaustiveness, flag semantics, explicit enum underlying types, explicit numeric enum values, enum member attributes, and richer pattern algebra remain future work.

## Task 0313 Roadmap Refresh After Enum Match Exhaustiveness

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework lifecycle, NuGet, and VS Code sources on 2026-05-21 after TypeSharp-owned enum match exhaustiveness landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, and compiler metadata paths against the current code state.
- Selected the next bounded implementation slice: imported C# enum match exhaustiveness using finite enum members already present in local/framework metadata.
- Created active task `0314 Imported C# enum exhaustiveness slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework)
- [dotnet restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# enum exhaustiveness was completed in task 0314. Flag semantics, explicit enum underlying types, explicit numeric enum values, enum member attributes, and richer pattern algebra remain future work.

## Task 0314 Imported C# Enum Exhaustiveness Slice

Completed compiler and interop work established:

- Added enum metadata shape to imported C# type symbols, including deterministic public enum member names from literal static fields.
- Registered named imported C# enums, including import aliases, in the type-checker enum scope.
- Reused the existing finite enum match logic so missing imported enum members report `TS2203`, guarded arms remain non-covering, and `_` covers the remaining known member space.
- Passed imported enum shapes into C# source emission so matches over imported enums lower to C# 7.3-compatible `object.Equals` comparisons against imported enum members.
- Added local `net48` C# DLL coverage for metadata enum members, `typesharp check` imported enum exhaustiveness diagnostics, and `typesharp build` generated assembly plus C# consumer compilation.
- Updated canonical type-system, reference, lowering, .NET interop, diagnostics, feature-status, and work-ledger docs for the implemented boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [MetadataAssemblySymbol.cs](../lang/TypeSharp.Compiler/Interop/MetadataAssemblySymbol.cs)
- [TypeSharpMetadataReader.cs](../lang/TypeSharp.Compiler/Interop/TypeSharpMetadataReader.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Flag semantics, explicit enum underlying types, explicit numeric enum values, enum member attributes, enum aliases, and richer pattern algebra remain future work.

## Task 0315 Roadmap Refresh After Imported C# Enum Exhaustiveness

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, .NET support, NuGet, and VS Code sources on 2026-05-21 after imported C# enum exhaustiveness landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, and current enum parser/backend shapes.
- Selected the next bounded implementation slice: explicit numeric member values for TypeSharp-owned enums, without taking on flags, aliases, explicit underlying types, computed values, or imported enum numeric metadata.
- Created active task `0316 Explicit enum numeric values slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Explicit TypeSharp enum numeric values are active in task 0316. Flag semantics, explicit enum underlying types, enum member attributes, enum aliases, and richer pattern algebra remain future work.

## Task 0316 Explicit Enum Numeric Values Slice

Completed language/compiler work established:

- Added parser support for optional explicit numeric enum member initializers such as `Red = 1`.
- Preserved duplicate enum member binding and existing enum type-checking/exhaustiveness semantics by keeping enum reasoning name/member based.
- Lowered explicit TypeSharp-owned enum member values to ordinary C# enum assignments while keeping generated source C# 7.3-compatible.
- Kept flags, explicit underlying types, computed enum values, enum aliases, imported C# enum numeric metadata, and enum member attributes out of scope.
- Added parser, type-checker, backend snapshot, and CLI generated `net48` assembly plus C# consumer coverage.
- Updated canonical grammar, reference, type-system, lowering, feature-status, and work-ledger docs for the implemented enum value boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

Remaining:

- Flag semantics, explicit enum underlying types, enum member attributes, enum aliases, imported C# enum numeric metadata, and richer pattern algebra remain future work.

## Task 0317 Roadmap Refresh After Explicit Enum Numeric Values

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after explicit TypeSharp enum numeric values landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, and current enum parser/backend shapes after numeric member value support.
- Selected the next bounded implementation slice: explicit underlying types for TypeSharp-owned enums, without taking on flags, aliases, computed values, enum member attributes, imported enum underlying-type metadata, or numeric range validation.
- Created active task `0318 Explicit enum underlying types slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Explicit TypeSharp enum underlying types are active in task 0318. Flag semantics, enum member attributes, enum aliases, imported C# enum numeric/underlying metadata, numeric range validation, and richer pattern algebra remain future work.

## Task 0318 Explicit Enum Underlying Types Slice

Completed language/compiler work established:

- Added parser support for optional explicit enum underlying type clauses such as `enum Color : byte`.
- Preserved duplicate enum member binding and existing enum type-checking/exhaustiveness semantics by keeping enum reasoning name/member based.
- Lowered explicit TypeSharp-owned enum underlying types to ordinary C# enum base-type clauses while keeping generated source C# 7.3-compatible.
- Kept explicit numeric member values working with explicit underlying types.
- Added deterministic `TS2201` diagnostics for unsupported enum underlying types before generated C# emission.
- Kept flags, computed enum values, enum aliases, imported C# enum numeric/underlying metadata, numeric range validation, and enum member attributes out of scope.
- Added parser, type-checker positive/negative, backend snapshot, and CLI generated `net48` assembly plus C# consumer coverage.
- Updated canonical grammar, reference, type-system, lowering, feature-status, diagnostics, and work-ledger docs for the implemented enum underlying-type boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-underlying-type-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Flag semantics, enum member attributes, enum aliases, imported C# enum numeric/underlying metadata, numeric range validation, and richer pattern algebra remain future work.

## Task 0319 Roadmap Refresh After Explicit Enum Underlying Types

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after explicit TypeSharp enum underlying types landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, and current enum parser/type-checker/backend shapes after explicit underlying type support.
- Selected the next bounded implementation slice: explicit enum numeric range validation for TypeSharp-owned enums, without taking on flags, aliases, computed values, enum member attributes, or imported enum numeric/underlying metadata.
- Created active task `0320 Explicit enum numeric range validation slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Explicit enum numeric range validation is active in task 0320. Flag semantics, enum member attributes, enum aliases, imported C# enum numeric/underlying metadata, and richer pattern algebra remain future work.

## Task 0320 Explicit Enum Numeric Range Validation Slice

Completed language/compiler work established:

- Added TypeSharp-owned enum numeric initializer validation against the selected enum underlying type range.
- Used `int` as the default range for enums without explicit underlying type declarations.
- Reported deterministic `TS2201` diagnostics for explicit enum member values outside `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, or `ulong` ranges.
- Reported deterministic `TS2201` diagnostics for non-integral numeric enum member tokens such as decimal or decimal-suffix literals.
- Preserved existing enum name/member reasoning, same-enum member type checking, match exhaustiveness, and C# enum lowering shape.
- Kept computed enum expressions, flags, aliases, enum member attributes, and imported C# enum numeric/underlying metadata out of scope.
- Added negative type-checker fixture coverage for default `int` overflow, explicit `byte` range failures, and non-integral member values.
- Updated canonical grammar, reference, type-system, lowering, diagnostics, feature-status, and work-ledger docs for the implemented boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/enum-numeric-range-invalid`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Flag semantics, enum member attributes, enum aliases, imported C# enum numeric/underlying metadata, and richer pattern algebra remain future work.

## Task 0321 Roadmap Refresh After Enum Numeric Range Validation

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after explicit enum numeric range validation landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, and current enum parser/type-checker/backend shapes after numeric range validation.
- Selected the next bounded implementation slice: explicit enum member aliases for TypeSharp-owned enums, without taking on computed enum expressions, flags, enum member attributes, or imported enum numeric/underlying metadata.
- Created active task `0322 Explicit enum member aliases slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Explicit enum member aliases are active in task 0322. Flag semantics, enum member attributes, imported C# enum numeric/underlying metadata, and richer pattern algebra remain future work.

## Task 0322 Explicit Enum Member Aliases Slice

Completed language/compiler work established:

- Added parser support for TypeSharp-owned enum member aliases such as `Crimson = Red`.
- Kept enum alias initializers token-shaped like numeric enum initializers so general expression binding does not reinterpret alias targets.
- Added deterministic `TS2201` diagnostics for aliases that do not target a previously declared member of the same enum, covering missing, forward, and self aliases.
- Lowered valid aliases to ordinary C# enum member assignments while preserving explicit underlying type and numeric member value lowering.
- Preserved existing numeric range validation, same-enum value type checking, enum match exhaustiveness, and imported C# enum policy.
- Kept arbitrary computed enum expressions, flags, enum member attributes, and imported C# enum numeric/underlying metadata out of scope.
- Updated parser, type-checker positive/negative, backend snapshot, canonical enum docs, diagnostics docs, feature status, and work-ledger state.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-alias-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Flag semantics, enum member attributes, imported C# enum numeric/underlying metadata, arbitrary computed enum member expressions, and richer pattern algebra remain future work.

## Task 0323 Roadmap Refresh After Enum Member Aliases

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after explicit enum member aliases landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current metadata reader enum shape, and the remaining enum backlog after alias support.
- Selected the next bounded implementation slice: imported C# enum numeric metadata capture, without taking on flags, numeric pattern algebra, enum member attributes, or TypeSharp-owned computed enum expressions.
- Created active task `0324 Imported enum numeric metadata slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# enum numeric metadata capture is complete in task 0324. Flag semantics, enum member attributes, arbitrary TypeSharp-owned computed enum expressions, numeric pattern algebra, and richer pattern algebra remain future work.

## Task 0324 Imported Enum Numeric Metadata Slice

Completed implementation work:

- Added imported C# enum underlying type metadata to `MetadataTypeSymbol`.
- Added field literal value metadata and populated deterministic imported enum member numeric values from ECMA-335 constant blobs.
- Preserved existing imported enum match exhaustiveness and generated C# lowering behavior as name/member based.
- Extended local `net48` metadata reader coverage with a `byte`-backed imported enum and deterministic `Name=Value` assertions.
- Updated .NET interop, type-system, feature-status, lowering, reference, work-ledger, tasks, and traceability docs for the metadata-only boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "metadata reader indexes local public symbols"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker reports imported C# enum match exhaustiveness"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles imported C# enum match exhaustiveness"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [MetadataAssemblySymbol.cs](../lang/TypeSharp.Compiler/Interop/MetadataAssemblySymbol.cs)
- [TypeSharpMetadataReader.cs](../lang/TypeSharp.Compiler/Interop/TypeSharpMetadataReader.cs)
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)

Remaining:

- Enum declaration/member attribute lowering is complete in task 0326. Flag semantics, broad attribute target validation, arbitrary TypeSharp-owned computed enum expressions, numeric pattern algebra, flag-style reasoning over imported numeric enum metadata, and richer pattern algebra remain future work.

## Task 0325 Roadmap Refresh After Imported Enum Numeric Metadata

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after imported enum numeric metadata landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, parser attribute support, backend enum lowering, and the remaining enum backlog after imported enum numeric metadata.
- Selected the next bounded implementation slice: TypeSharp-owned enum declaration/member attribute lowering, without taking on flag algebra, broad attribute target validation, numeric pattern algebra, or arbitrary TypeSharp-owned computed enum expressions.
- Created active task `0326 Enum attribute lowering slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Enum declaration/member attribute lowering is complete in task 0326. Flag semantics, broad attribute target validation, arbitrary TypeSharp-owned computed enum expressions, numeric pattern algebra, flag-style reasoning over imported numeric enum metadata, and richer pattern algebra remain future work.

## Task 0326 Enum Attribute Lowering Slice

Completed implementation work:

- Added parser support for attribute lists before TypeSharp-owned enum members.
- Lowered TypeSharp-owned enum declaration attributes before generated C# enum declarations.
- Lowered enum member attributes before generated C# enum members while preserving explicit numeric values and aliases.
- Added parser and C# backend fixture coverage using `[FlagsAttribute]` and `[ObsoleteAttribute]`.
- Extended the generated `net48` enum API build smoke to assert emitted enum/member attributes.
- Updated grammar, reference, type-system, lowering, feature-status, work-ledger, tasks, and traceability docs for the metadata-only boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

Remaining:

- Flag semantics, broad attribute target validation, arbitrary TypeSharp-owned computed enum expressions, numeric pattern algebra, flag-style reasoning over imported numeric enum metadata, and richer pattern algebra remain future work.

## Task 0327 Roadmap Refresh After Enum Attribute Lowering

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after enum attribute lowering landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, parser enum initializer shape, type-checker enum initializer validation, C# backend enum member lowering, and the remaining enum backlog after declaration/member attribute lowering.
- Selected the next bounded implementation slice: TypeSharp-owned enum composite member expressions using enum initializer-local `|` over previously declared same-enum members and integer literals.
- Kept general bitwise expressions, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, broad attribute target validation, and arbitrary computed enum expressions out of the slice.
- Created active task `0328 Enum composite member expressions slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Enum composite member expressions are complete in task 0328. Flag semantics, broad attribute target validation, arbitrary/general computed enum member expressions, numeric pattern algebra, flag-style reasoning over imported numeric enum metadata, and richer pattern algebra remain future work.

## Task 0328 Enum Composite Member Expressions Slice

Completed language/compiler work established:

- Added parser support for TypeSharp-owned enum initializer-local `|` composite member expressions over integer literals, signed integer literals, and identifier operands.
- Validated every composite identifier operand against previously declared members of the same enum while preserving the existing single-alias diagnostic wording for `Alias = Member`.
- Extended enum numeric range/integrality validation to numeric operands inside composite initializers.
- Lowered accepted composites to ordinary C# enum member assignments such as `Purple = Red | Blue`.
- Kept general expression bitwise operators, `&`, `^`, `~`, shifts, parentheses, arbitrary/general computed enum expressions, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and broad attribute target validation out of scope.
- Added parser, type-checker positive/negative, backend snapshot, generated `net48` CLI build, fixture README, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-composite-invalid`
- `test/fixtures/diagnostics/type-checker/negative/enum-numeric-range-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Flag semantics, general expression bitwise operators, broad attribute target validation, arbitrary/general computed enum member expressions beyond enum initializer-local `|`, numeric pattern algebra, flag-style reasoning over imported numeric enum metadata, and richer pattern algebra remain future work.

## Task 0329 Roadmap Refresh After Enum Composite Member Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after enum composite member expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, parser binary precedence, enum initializer-local composite support, type-checker enum validation, C# backend expression lowering, and the remaining enum backlog.
- Selected the next bounded implementation slice: expression-level same-enum value `|` expressions such as `Permission.Read | Permission.Write`.
- Kept numeric/general bitwise operators, `&`, `^`, `~`, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations out of the slice.
- Created active task `0330 Enum value bitwise OR expression slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Same-enum value `|` expressions are active in task 0330. Numeric/general bitwise operators, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0330 Enum Value Bitwise OR Expression Slice

Completed language/compiler work established:

- Added parser support for expression-level `|` over enum values without changing type-union, pattern-or, pipeline, or enum initializer-local parsing.
- Added type-checker validation for same-enum value `|` expressions and deterministic `TS2201` diagnostics for mixed enum or non-enum operands.
- Preserved the narrow boundary: numeric/general bitwise operators, `&`, `^`, `~`, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations remain out of scope.
- Reused existing C# binary expression lowering so accepted expressions emit ordinary C# 7.3-compatible `|`.
- Added parser, type-checker positive/negative, backend snapshot, generated `net48` CLI build, fixture README, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-or-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Numeric/general bitwise operators, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0331 Roadmap Refresh After Enum Value OR Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after enum value OR expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, parser `|` precedence, same-enum value OR type checking, C# backend binary expression lowering, and the remaining enum flag backlog.
- Selected the next bounded implementation slice: expression-level same-enum value `&` expressions such as `permission & Permission.Read`.
- Kept numeric/general bitwise operators, `^`, `~`, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations out of the slice.
- Created active task `0332 Enum value bitwise AND expression slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Same-enum value `&` expressions are active in task 0332. Numeric/general bitwise operators, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0332 Enum Value Bitwise AND Expression Slice

Completed language/compiler work established:

- Added parser support for expression-level `&` over enum values without changing type-intersection, pattern-and, or enum initializer-local parsing.
- Generalized enum value bitwise type checking so same-enum value `|` and `&` expressions infer the enum type and use operator-specific deterministic `TS2201` diagnostics for mixed enum or non-enum operands.
- Preserved the narrow boundary: numeric/general bitwise operators, `^`, `~`, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations remain out of scope.
- Reused existing C# binary expression lowering so accepted expressions emit ordinary C# 7.3-compatible `&`.
- Added parser, type-checker positive/negative, backend snapshot, generated `net48` CLI build, fixture README, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-and-invalid`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-or-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Numeric/general bitwise operators, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0333 Roadmap Refresh After Enum Value AND Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after enum value AND expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, parser unary/binary precedence, same-enum value `|`/`&` type checking, C# backend unary/binary expression lowering, and the remaining enum flag operator backlog.
- Selected the next bounded implementation slice: expression-level same-enum value `^` and unary `~` expressions such as `permission ^ Permission.Write` and `~permission`.
- Kept numeric/general bitwise operators, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations out of the slice.
- Created active task `0334 Enum value XOR and complement expression slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Same-enum value `^` and unary `~` expressions are complete in task 0334. Numeric/general bitwise operators, shifts, compound assignment, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0334 Enum Value XOR And Complement Expression Slice

Completed language/compiler work established:

- Added lexer/parser support for expression-level `^` and unary `~` over enum values without changing type-union, type-intersection, pattern operators, pipeline/composition, or enum initializer-local parsing.
- Generalized enum value bitwise type checking so same-enum value `|`, `&`, `^`, and unary `~` expressions infer the enum type and use deterministic `TS2201` diagnostics for mixed enum or non-enum operands.
- Preserved the narrow boundary: numeric/general bitwise operators, shifts, compound assignment, flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and arbitrary/general computed enum member declarations remain out of scope.
- Reused existing C# unary/binary expression lowering so accepted expressions emit ordinary C# 7.3-compatible `^` and `~`.
- Added parser, type-checker positive/negative, backend snapshot, generated `net48` CLI build, fixture README, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles enum declaration API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [SyntaxKind.cs](../lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs)
- [TypeSharpLexer.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs)
- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0035-enum-declaration`
- `test/fixtures/diagnostics/type-checker/positive/enum-declaration`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-xor-complement-invalid`
- `test/fixtures/backend/csharp/positive/0039-enum-declaration-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Numeric/general bitwise operators, shifts, compound assignment, flag-aware match exhaustiveness, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0335 Roadmap Refresh After Enum XOR And Complement Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after enum XOR/complement expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, existing parser bitwise precedence, same-enum value `|`/`&`/`^`/`~` type checking, C# backend unary/binary expression lowering, and the remaining operator backlog.
- Selected the next bounded implementation slice: expression-level integral numeric `|`, `&`, `^`, and unary `~` over known non-null primitive integral operands.
- Kept shifts, compound assignment, boolean bitwise expressions, flag-aware match algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of the slice.
- Created active task `0336 Integral numeric bitwise expression slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Integral numeric `|`/`&`/`^`/`~` expressions are active in task 0336. Shifts, compound assignment, boolean bitwise expressions, flag-aware match algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0336 Integral Numeric Bitwise Expression Slice

Completed language/compiler work established:

- Generalized the expression bitwise checker so same-enum value `|`/`&`/`^`/`~` behavior remains intact while known non-null primitive integral operands are accepted for `|`, `&`, `^`, and unary `~`.
- Added C#-style integral promotion for supported bitwise expressions: small integral operands promote to `int`, `uint`/`long`/`ulong` combinations follow the bounded C# integral promotion rules, and nullable, boolean, decimal, string, and unsupported operands report deterministic `TS2201` diagnostics.
- Preserved the narrow boundary: shifts, compound assignment, boolean bitwise expressions, flag-aware match algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra remain out of scope.
- Reused existing C# unary/binary expression emission so accepted expressions lower to ordinary C# 7.3-compatible `|`, `&`, `^`, and `~`.
- Added type-checker positive/negative fixtures, backend snapshot, generated `net48` CLI build and C# consumer smoke, fixture README coverage, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles integral bitwise expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [TypeSharpInferenceEngine.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- `test/fixtures/diagnostics/type-checker/positive/integral-bitwise-expression`
- `test/fixtures/diagnostics/type-checker/negative/integral-bitwise-invalid`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-or-invalid`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-and-invalid`
- `test/fixtures/diagnostics/type-checker/negative/enum-value-bitwise-xor-complement-invalid`
- `test/fixtures/backend/csharp/positive/0041-integral-bitwise-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Shifts, compound assignment, boolean bitwise expressions, flag-aware match algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0337 Roadmap Refresh After Integral Numeric Bitwise Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after integral numeric bitwise expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current bitwise checker behavior, C# backend unary/binary expression lowering, and the remaining operator backlog.
- Selected the next bounded implementation slice: expression-level boolean `|`, `&`, and `^` over known non-null `bool` operands.
- Kept unary boolean complement, shifts, compound assignment, user-defined operators, flag-aware enum algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of the slice.
- Kept shifts separate because `>>`/`<<` are already TypeSharp function-composition syntax and need a dedicated grammar/design pass.
- Created active task `0338 Boolean bitwise expression slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta announcement](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Boolean `|`/`&`/`^` expressions are active in task 0338. Shifts, compound assignment, user-defined operators, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0338 Boolean Bitwise Expression Slice

Completed active implementation work for:

- Start Time: 2026-05-21 17:29:25 +09:00
- End Time: 2026-05-21 17:40:23 +09:00

Completed work:

- Added expression-level boolean `|`, `&`, and `^` support for known non-null `bool` operands in the type checker.
- Extended expression inference, generated-source type inference, and project builder type inference so boolean bitwise expressions produce `bool`.
- Preserved enum value bitwise and integral numeric bitwise behavior while adding deterministic diagnostics for bool/non-bool mixes and nullable bool operands.
- Kept unary boolean complement, shifts, compound assignment, user-defined operators, flag-aware enum algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of scope.
- Added focused positive/negative type-checker fixtures, backend C# snapshot coverage, a generated `net48` CLI build and C# consumer smoke, fixture README coverage, canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles boolean bitwise expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [TypeSharpInferenceEngine.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [TypeSharpBuilder.cs](../lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs)
- `test/fixtures/diagnostics/type-checker/positive/boolean-bitwise-expression`
- `test/fixtures/diagnostics/type-checker/negative/boolean-bitwise-invalid`
- `test/fixtures/diagnostics/type-checker/negative/integral-bitwise-invalid`
- `test/fixtures/backend/csharp/positive/0042-boolean-bitwise-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Unary boolean complement, shifts, compound assignment, user-defined operators, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0339 Roadmap Refresh After Boolean Bitwise Expressions

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code sources on 2026-05-21 after boolean bitwise expressions landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current assignment parsing/lowering behavior, and the remaining operator backlog.
- Selected the next bounded implementation slice: bitwise compound assignment `|=`, `&=`, and `^=` over the already supported assignment surface.
- Kept shifts and shift assignment separate because `>>` and `<<` are already TypeSharp function-composition syntax and need a dedicated grammar/design pass.
- Kept user-defined operators, broader assignment target analysis, flag-aware enum algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of the slice.
- Created active task `0340 Bitwise compound assignment slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript official blog](https://devblogs.microsoft.com/typescript/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Bitwise compound assignment `|=`/`&=`/`^=` is active in task 0340. Shifts, shift assignment, unary boolean complement, user-defined operators, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0340 Bitwise Compound Assignment Slice

Completed active implementation work for:

- Start Time: 2026-05-21 17:55:52 +09:00
- End Time: 2026-05-21 18:06:31 +09:00

Completed work:

- Added `|=`, `&=`, and `^=` lexer tokens and assignment parsing.
- Preserved existing `=`, `+=`, and `-=` assignment behavior.
- Reused the existing assignment emitter so bitwise compound assignment lowers to ordinary C# 7.3-compatible assignment operators.
- Added parser snapshot coverage, type-checker acceptance coverage, backend snapshot coverage, generated `net48` CLI build and C# consumer smoke, fixture README coverage, canonical docs, work-ledger, tasks, and traceability coverage.
- Kept shifts, shift assignment, user-defined operators, broader assignment target analysis, flag-aware enum algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of scope.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "generated C# compiles in net48 project"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles bitwise compound assignment API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [SyntaxKind.cs](../lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs)
- [TypeSharpLexer.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs)
- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- `test/fixtures/parser/positive/0036-bitwise-compound-assignment`
- `test/fixtures/diagnostics/type-checker/positive/bitwise-compound-assignment`
- `test/fixtures/backend/csharp/positive/0043-bitwise-compound-assignment-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)

Remaining:

- Shifts, shift assignment, unary boolean complement, user-defined operators, broader assignment target analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0341 Roadmap Refresh After Bitwise Compound Assignments

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after bitwise compound assignments landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current assignment parsing/lowering behavior, existing interop assignment diagnostics, and the remaining operator/assignment backlog.
- Selected the next bounded implementation slice: local assignment target analysis for TypeSharp identifier assignments, tracking `let mut` and focused assignment compatibility diagnostics.
- Kept imported C# member/indexer/static/event assignment on the existing metadata-backed interop validator path.
- Kept shifts and shift assignment separate because `>>` and `<<` are already TypeSharp function-composition syntax and need a dedicated grammar/design pass.
- Kept user-defined operators, broad class-member body analysis, flag-aware enum algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, broad attribute target validation, numeric pattern algebra, and richer pattern algebra out of the slice.
- Created active task `0342 Local assignment target analysis slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript official blog](https://devblogs.microsoft.com/typescript/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Local assignment target analysis is active in task 0342. Shifts, shift assignment, user-defined operators, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0342 Local Assignment Target Analysis Slice

Completed active implementation work for:

- Start Time: 2026-05-21 18:21:53 +09:00
- End Time: 2026-05-21 18:31:37 +09:00

Completed work:

- Added mutability tracking to the type-checker value scope while preserving existing value and function inference behavior.
- Routed `AssignmentExpression` through a dedicated checker path for local identifier assignments.
- Required `let mut` for local identifier assignment targets and reported deterministic `TS2201` diagnostics for immutable local bindings and function parameters.
- Added known simple-assignment compatibility checks for local identifiers, including nullability, structural, and ordinary assignment compatibility.
- Added known local `|=`, `&=`, and `^=` operand checks for same-enum, primitive integral, and bool targets.
- Diagnosed obvious non-assignable local expression targets such as literals while leaving imported C# member, indexer, static member, and event assignment on the existing metadata-backed interop validator path.
- Added positive and negative type-checker fixtures plus canonical docs, work-ledger, tasks, and traceability coverage.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "assignment"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/local-assignment-target-analysis`
- `test/fixtures/diagnostics/type-checker/negative/local-assignment-target-invalid`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)

Remaining:

- Shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0343 Roadmap Refresh After Local Assignment Target Analysis

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after local assignment target analysis landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current composition parsing/lowering behavior, and the remaining shift/operator backlog.
- Selected the next bounded implementation slice: composition/shift ambiguity diagnostics for known value-shaped `>>` and `<<` expressions.
- Kept `>>` and `<<` as TypeSharp function composition syntax and kept numeric shifts plus shift assignment out of the slice.
- Kept user-defined operators, broader composition type inference, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0344 Composition shift ambiguity diagnostics slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript official blog](https://devblogs.microsoft.com/typescript/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Composition/shift ambiguity diagnostics are active in task 0344. Numeric shifts, shift assignment, user-defined operators, broader composition type inference, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0344 Composition Shift Ambiguity Diagnostics Slice

Completed implementation work established:

- Preserved `>>` and `<<` as TypeSharp function-composition operators and kept the existing parser/backend composition fixtures unchanged.
- Added a type-checker composition path that checks direct operands before generic inference and reports `TS2201` when a known value-shaped operand, including numeric, `bool`, `string`, nullable primitive, `null`, or enum-shaped values, would otherwise lower as invalid composition.
- Added the negative fixture `test/fixtures/diagnostics/type-checker/negative/composition-shift-ambiguity` covering numeric literal `>>`, numeric literal `<<`, local known values, enum member values, and nullable primitive values.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs to spell out the ambiguity boundary.
- Kept numeric shifts, shift assignment, user-defined operators, broader composition type inference, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/composition-shift-ambiguity`
- `test/fixtures/parser/positive/0022-composition-expression`
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Numeric shifts, shift assignment, user-defined operators, broader composition type inference, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0345 Roadmap Refresh After Composition Shift Ambiguity Diagnostics

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after composition/shift ambiguity diagnostics landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current composition parsing/lowering behavior, and the remaining function composition/operator backlog.
- Selected the next bounded implementation slice: direct named-function composition compatibility diagnostics for unary TypeSharp-declared function pairs.
- Kept higher-order function values, currying, partial application, generic/imported composition inference, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0346 Composition function compatibility diagnostics slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [F# task expressions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/task-expressions)
- [TypeScript official blog](https://devblogs.microsoft.com/typescript/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct named-function composition compatibility diagnostics are active in task 0346. Higher-order function values, currying, partial application, generic/imported composition inference, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0346 Composition Function Compatibility Diagnostics Slice

Completed implementation work established:

- Extended TypeSharp function symbol metadata with optional known parameter types for TypeSharp-declared functions while preserving existing return-type and capability checks.
- Added direct named-function composition validation for unary `f >> g` and `g << f` pairs when both signatures are known.
- Reported deterministic `TS2201` diagnostics when the first-applied function's return type cannot flow into the next function's parameter type.
- Preserved value-shaped `>>`/`<<` ambiguity diagnostics, positive composition parser/backend fixtures, and existing C# 7.3-compatible composition lowering.
- Added `test/fixtures/diagnostics/type-checker/negative/composition-function-compatibility` covering valid direct named-function composition, invalid `>>`, invalid `<<`, and multi-parameter function out-of-scope behavior.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded direct named-function compatibility boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/composition-function-compatibility`
- `test/fixtures/parser/positive/0022-composition-expression`
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Higher-order function values, currying, partial application, generic/imported composition inference, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0347 Roadmap Refresh After Composition Function Compatibility Diagnostics

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after direct named-function composition compatibility diagnostics landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current pipeline parsing/lowering behavior, and the remaining function composition/pipeline/operator backlog.
- Selected the next bounded implementation slice: direct pipeline input compatibility diagnostics for `value |> f` and `value |> f(args...)` targets with known TypeSharp-declared first parameter signatures.
- Kept higher-order pipeline targets, imported functions, generic inference, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0348 Pipeline function input compatibility diagnostics slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [F# functions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/functions/)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct pipeline input compatibility diagnostics are active in task 0348. Higher-order pipeline targets, imported functions, generic inference, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0348 Pipeline Function Input Compatibility Diagnostics Slice

Completed implementation work established:

- Added a type-checker pipeline path that preserves existing target capability checks, checks direct call target arguments, and validates the piped input against known TypeSharp-declared first parameter types.
- Reported deterministic `TS2201` diagnostics for incompatible `value |> f` and `value |> f(args...)` targets when the target signature is known.
- Preserved existing pipeline parsing, backend fixture output, and C# 7.3-compatible call lowering.
- Added `test/fixtures/diagnostics/type-checker/negative/pipeline-function-input-compatibility` covering compatible direct targets, chained pipeline targets, call targets, incompatible identifier targets, incompatible call targets, and zero-parameter out-of-scope behavior.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded direct pipeline compatibility boundary.
- Kept higher-order pipeline targets, imported functions, generic inference, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles pipeline lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/pipeline-function-input-compatibility`
- `test/fixtures/backend/csharp/positive/0023-pipeline-lowering`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Higher-order pipeline targets, imported functions, generic inference, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0349 Roadmap Refresh After Pipeline Input Compatibility Diagnostics

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after direct pipeline input compatibility diagnostics landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current pipeline parsing/lowering behavior, and the remaining function pipeline/operator backlog.
- Selected the next bounded implementation slice: direct pipeline target arity and non-piped call argument diagnostics for known TypeSharp-declared function targets.
- Kept imported C# pipeline targets, generic inference, optional/default/params TypeSharp function parameter policy, higher-order pipeline targets, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0350 Pipeline target arity and argument diagnostics slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [F# functions](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/functions/)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct pipeline target arity and non-piped call argument diagnostics are active in task 0350. Imported C# pipeline targets, generic inference, optional/default/params TypeSharp function parameter policy, higher-order pipeline targets, currying, partial application, pipeline overload ranking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0350 Pipeline Target Arity And Argument Diagnostics Slice

Completed implementation work established:

- Extended the direct pipeline type-checking path to validate known TypeSharp-declared target arity after first-argument lowering.
- Reported deterministic `TS2201` diagnostics for zero-parameter pipeline targets, missing lowered arguments, extra lowered arguments, and incompatible non-piped call arguments.
- Kept imported C# pipeline targets, generic inference, optional/default/params TypeSharp function parameter policy, higher-order pipeline targets, currying, partial application, pipeline overload ranking, and general direct TypeSharp function call argument checking outside pipeline targets out of the slice.
- Preserved existing pipeline parsing, backend fixture output, valid pipeline CLI build behavior, and C# 7.3-compatible call lowering.
- Added `test/fixtures/diagnostics/type-checker/negative/pipeline-target-arity-and-argument` covering valid direct/call pipeline targets plus zero-parameter, missing-argument, extra-argument, and incompatible non-piped argument diagnostics.
- Kept `test/fixtures/diagnostics/type-checker/negative/pipeline-function-input-compatibility` focused on first-parameter input compatibility.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded direct pipeline arity and argument boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles pipeline lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/pipeline-target-arity-and-argument`
- `test/fixtures/diagnostics/type-checker/negative/pipeline-function-input-compatibility`
- `test/fixtures/backend/csharp/positive/0023-pipeline-lowering`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# pipeline targets, generic inference, optional/default/params TypeSharp function parameter policy, higher-order pipeline targets, currying, partial application, pipeline overload ranking, general direct TypeSharp function call argument checking, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0351 Roadmap Refresh After Pipeline Target Diagnostics

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# bitwise/shift operator sources on 2026-05-21 after direct pipeline target arity and argument diagnostics landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current TypeSharp-declared function call behavior, and the remaining function call/pipeline backlog.
- Selected the next bounded implementation slice: direct TypeSharp-declared function call arity and argument diagnostics.
- Kept imported C# call validation, generic TypeSharp function argument inference, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0352 Direct function call arity and argument diagnostics slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct TypeSharp-declared function call arity and argument diagnostics are active in task 0352. Imported C# call validation, generic TypeSharp function argument inference, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0352 Direct Function Call Arity And Argument Diagnostics Slice

Completed implementation work established:

- Added a direct call type-checking path for known TypeSharp-declared functions with known parameter lists.
- Reported deterministic `TS2201` diagnostics for direct calls with too few arguments, too many arguments, or incompatible known argument types.
- Reused existing assignment compatibility, nullability, and structural mismatch helpers for argument checks.
- Preserved imported C# call validation, type constructor calls, generated C# call lowering, and existing TypeSharp call inference for unknown signatures.
- Added `test/fixtures/diagnostics/type-checker/negative/direct-function-call-arity-and-argument` covering compatible zero/one/two-argument and nested calls plus missing-argument, extra-argument, and argument-type diagnostics.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded direct function call diagnostics boundary.
- Kept imported C# call validation, generic TypeSharp function argument inference, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles basic semantics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/negative/direct-function-call-arity-and-argument`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# call validation, generic TypeSharp function argument inference, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0353 Roadmap Refresh After Direct Function Call Diagnostics

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and C# generic/function-call-related source signals on 2026-05-21 after direct TypeSharp-declared function call diagnostics landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current TypeSharp-declared generic function call behavior, and the remaining function call/pipeline backlog.
- Selected the next bounded implementation slice: direct generic TypeSharp function call inference for simple TypeSharp-declared generic functions.
- Kept imported C# generic call validation, generic pipeline/composition inference, constructed generic parameter inference, generic constraints beyond the existing backend-compatible checks, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0354 Direct generic function call inference slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct generic TypeSharp function call inference is active in task 0354. Imported C# generic call validation, generic pipeline/composition inference, constructed generic parameter inference, generic constraints beyond the existing backend-compatible checks, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0354 Direct Generic Function Call Inference Slice

Completed implementation work established:

- Tracked TypeSharp-declared function type parameter names in the type checker function table.
- Added a direct generic call path for TypeSharp-declared functions that accepts explicit generic type arguments and infers simple type parameters from known direct call arguments.
- Substituted simple direct type-parameter return types so inferred or explicit generic calls participate in assignment and nested-call checks.
- Reported deterministic `TS2201` diagnostics for explicit generic arity mismatches, explicit argument mismatches, inconsistent repeated type-parameter inference, and substituted return type mismatches.
- Kept imported C# generic call validation, generic pipeline/composition inference, constructed generic parameter inference such as `T[]` or `List<T>`, broader generic constraints, optional/default/params TypeSharp parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, and type constructor policy out of the slice.
- Preserved existing generated C# call lowering and strengthened the generic function CLI build smoke with inferred and explicit TypeSharp generic calls.
- Added `test/fixtures/diagnostics/type-checker/positive/direct-generic-function-call-inference`, `test/fixtures/diagnostics/type-checker/negative/direct-generic-function-call-inference`, and `test/fixtures/backend/csharp/positive/0044-direct-generic-function-call-lowering`.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded direct generic function call inference boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles generic function API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-function-call-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-function-call-inference`
- `test/fixtures/backend/csharp/positive/0044-direct-generic-function-call-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# generic call validation, generic pipeline/composition inference, constructed generic parameter inference, generic constraints beyond the existing backend-compatible checks, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0355 Roadmap Refresh After Direct Generic Function Call Inference

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after direct TypeSharp-declared generic function call inference landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current direct generic TypeSharp function call behavior, and the remaining function call/pipeline backlog.
- Selected the next bounded implementation slice: constructed generic function call inference for direct TypeSharp-declared generic functions.
- Kept imported C# generic call validation, generic pipeline/composition inference, broader generic constraints, optional/default/params TypeSharp parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0356 Constructed generic function call inference slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Constructed generic TypeSharp function call inference is active in task 0356. Imported C# generic call validation, generic pipeline/composition inference, broader generic constraints, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0356 Constructed Generic Function Call Inference Slice

Completed implementation work established:

- Extended direct TypeSharp-declared generic call inference from simple `T` positions to bounded constructed parameter shapes.
- Added recursive direct inference for arrays such as `T[]` and matching single-argument generic wrappers such as `List<T>`.
- Substituted supported constructed return shapes after explicit or inferred type arguments so assignments and nested direct checks see concrete array/List results.
- Preserved existing simple generic inference, explicit type-argument checks, imported C# generic call validation, pipeline/composition behavior, and generated C# call lowering.
- Reported deterministic `TS2201` diagnostics for repeated constructed inference conflicts, explicit constructed argument mismatches, and constructed return assignment mismatches.
- Added constructed array/List coverage to `test/fixtures/diagnostics/type-checker/positive/direct-generic-function-call-inference`, `test/fixtures/diagnostics/type-checker/negative/direct-generic-function-call-inference`, and `test/fixtures/backend/csharp/positive/0044-direct-generic-function-call-lowering`.
- Strengthened the generic function CLI build smoke with public array and `List<T>` generic function APIs plus a C# `net48` consumer.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the bounded constructed generic inference boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles generic function API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-function-call-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-function-call-inference`
- `test/fixtures/backend/csharp/positive/0044-direct-generic-function-call-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# generic call validation, generic pipeline/composition inference, broader generic constraints, optional/default/params TypeSharp function parameter policy, function-typed values, higher-order calls, currying, partial application, TypeSharp function overload ranking, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0357 Roadmap Refresh After Constructed Generic Function Call Inference

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after constructed generic TypeSharp function call inference landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current direct generic call behavior, and the remaining pipeline/composition backlog.
- Selected the next bounded implementation slice: direct generic pipeline inference for known TypeSharp-declared generic function targets.
- Kept imported C# pipeline targets, generic composition inference, higher-order pipeline targets, function values, currying, partial application, optional/default/params TypeSharp parameter policy, pipeline overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0358 Direct generic pipeline inference slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct generic pipeline inference is active in task 0358. Imported C# pipeline targets, generic composition inference, higher-order pipeline targets, function values, currying, partial application, optional/default/params TypeSharp parameter policy, pipeline overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0358 Direct Generic Pipeline Inference Slice

Completed implementation work established:

- Extended direct `value |> f` and `value |> f(args...)` checks to known TypeSharp-declared generic function targets.
- Reused bounded direct generic inference/substitution for simple type-parameter positions, arrays such as `T[]`, and matching single-argument generic wrappers such as `List<T>`.
- Inferred generic parameters from the piped input plus non-piped pipeline call arguments, then returned substituted pipeline result types so downstream assignment checks see concrete types.
- Reported deterministic `TS2201` diagnostics for inconsistent repeated generic inference, substituted return mismatches, and lowered pipeline arity mistakes.
- Preserved existing non-generic pipeline checks, direct generic function call behavior, imported C# pipeline backlog boundaries, and generated C# first-argument call lowering.
- Added `test/fixtures/diagnostics/type-checker/positive/direct-generic-pipeline-inference` and `test/fixtures/diagnostics/type-checker/negative/direct-generic-pipeline-inference`.
- Strengthened `test/fixtures/backend/csharp/positive/0023-pipeline-lowering` and the `CLI build compiles pipeline lowering` smoke with generic TypeSharp pipeline targets and a `net48` C# consumer.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the direct generic pipeline inference boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles pipeline lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-pipeline-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-pipeline-inference`
- `test/fixtures/backend/csharp/positive/0023-pipeline-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# pipeline targets, generic composition inference, higher-order pipeline targets, function values, currying, partial application, optional/default/params TypeSharp parameter policy, pipeline overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0359 Roadmap Refresh After Direct Generic Pipeline Inference

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after direct generic TypeSharp pipeline inference landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current direct generic pipeline behavior, and the remaining pipeline/composition backlog.
- Selected the next bounded implementation slice: direct generic named-function composition inference for known TypeSharp-declared unary function targets.
- Kept imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0360 Direct generic composition inference slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [VS Code Language Server Extension Guide](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct generic composition inference is active in task 0360. Imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0360 Direct Generic Composition Inference Slice

Completed implementation work established:

- Extended direct identifier-only `f >> g` and `g << f` compatibility checks to known TypeSharp-declared generic unary function targets.
- Added bounded composition-edge inference/substitution for simple `T`, arrays such as `T[]`, and matching single-argument generic wrappers such as `List<T>`.
- Preserved existing direct named-function composition checks and skipped broader unresolved generic composition cases until a future full composition type-inference slice.
- Reported deterministic `TS2201` diagnostics for incompatible substituted generic composition edges.
- Strengthened the C# backend composition fixture and CLI composition build smoke with generic TypeSharp unary functions while keeping the existing C# 7.3-compatible delegate-lambda lowering shape.
- Added positive and negative type-checker fixtures for direct generic composition inference.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the direct generic composition inference boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles composition lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/direct-generic-composition-inference`
- `test/fixtures/diagnostics/type-checker/negative/direct-generic-composition-inference`
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition expression function-type inference, public ABI inference for composition expressions, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0361 Roadmap Refresh After Direct Generic Composition Inference

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after direct generic TypeSharp composition inference landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current direct generic composition behavior, and the remaining composition/function-type backlog.
- Selected the next bounded implementation slice: explicit function-type annotation compatibility for direct named-function composition values.
- Kept unannotated composition expression function-type inference, public ABI inference for composition expressions, imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0362 Composition function-type annotation compatibility slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Explicit composition function-type annotation compatibility is active in task 0362. Unannotated composition expression function-type inference, public ABI inference for composition expressions, imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0362 Composition Function-Type Annotation Compatibility Slice

Completed implementation work established:

- Added type-checker validation for explicitly annotated direct composition value declarations shaped like `let composed: A -> B = f >> g` and `let composed: A -> B = g << f`.
- Validated the annotation input against the first composed TypeSharp-declared unary function parameter.
- Validated the final composed return against the annotation return type.
- Reused the existing bounded generic composition edge substitution for direct TypeSharp generic unary targets when the edge provides enough concrete type information.
- Reported deterministic `TS2201` diagnostics for annotation input/result incompatibilities before generated C# assignment.
- Preserved existing direct composition compatibility diagnostics, unannotated composition behavior, imported composition backlog boundaries, and C# 7.3-compatible delegate-lambda lowering.
- Added positive and negative type-checker fixtures for composition function-type annotation compatibility.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the completed behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles composition lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/diagnostics/type-checker/positive/composition-function-type-annotation-compatibility`
- `test/fixtures/diagnostics/type-checker/negative/composition-function-type-annotation-compatibility`
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Unannotated composition expression function-type inference, public ABI inference for composition expressions, imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0364 Direct Composition Value Inference Slice

Completed implementation work established:

- Added backend-local direct TypeSharp function signature tracking for top-level functions before value emission.
- Inferred concrete generated delegate types for unannotated non-exported top-level direct composition values shaped like `let composed = f >> g` and `let composed = g << f` when both operands are direct TypeSharp-declared unary functions and the bounded generic edge substitution yields a fully known input/result signature.
- Emitted representable private/internal composed values as `System.Func<TInput, TResult>` or `System.Action<TInput>` instead of `object`, while leaving unresolved, imported, nested, higher-order, and public ABI cases conservative.
- Reused the bounded direct generic composition edge inference shapes for simple type parameters, arrays, and matching single-argument generic wrappers.
- Added `TS2201` validation for public/exported direct composition values that omit an explicit function type annotation, keeping public ABI inference closed.
- Extended the C# backend composition fixture and CLI composition smoke to cover unannotated non-exported concrete delegate lowering, including forward, backward, and bounded generic identity-edge cases.
- Added a negative type-checker fixture for public direct composition values that require explicit function type annotations.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the completed behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles composition lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- `test/fixtures/backend/csharp/positive/0029-composition-expression-lowering`
- `test/fixtures/diagnostics/type-checker/negative/composition-public-annotation-required`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Roadmap refresh after private direct composition value inference is active in task 0365. Exported public ABI inference for composition expressions, imported C# composition targets, nested/higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0363 Roadmap Refresh After Composition Annotation Compatibility

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after explicit composition function-type annotation compatibility landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current direct composition annotation behavior, and the remaining composition value inference backlog.
- Selected the next bounded implementation slice: private direct composition value inference for unannotated non-exported values whose direct TypeSharp-declared unary composition signature is fully known.
- Kept exported unannotated composition values, public ABI inference for composition expressions, imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0364 Direct composition value inference slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Direct composition value inference is active in task 0364. Exported unannotated composition values, public ABI inference for composition expressions, imported C# composition targets, higher-order function values, currying, partial application, optional/default/params TypeSharp parameter policy, composition overload ranking, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0365 Roadmap Refresh After Direct Composition Value Inference

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after private direct composition value inference landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, current private direct composition value inference behavior, and the remaining direct call/parameter ergonomics backlog.
- Selected the next bounded implementation slice: TypeSharp-owned `params` parameter declarations for final array parameters on direct TypeSharp-declared function calls, with C# 7.3-compatible `params T[]` generated signatures.
- Kept optional/default parameter declarations, named argument binding for TypeSharp-declared functions, TypeSharp overload ranking, imported C# `params` behavior changes, `params` in function-type syntax, higher-order function values, currying, partial application, public composition ABI inference, imported composition targets, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra out of the slice.
- Created active task `0366 Params parameter declaration slice`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# method parameters](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/method-parameters)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript rest parameters](https://www.typescriptlang.org/docs/handbook/2/functions.html)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Params parameter declaration support is active in task 0366. Optional/default parameter declarations, named argument binding for TypeSharp-declared functions, TypeSharp overload ranking, imported C# `params` behavior changes, `params` in function-type syntax, higher-order function values, currying, partial application, public composition ABI inference, imported composition targets, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0366 Params Parameter Declaration Slice

Completed implementation work established:

- Added lexer/parser support for preserving a `params` keyword before parameter names in TypeSharp parameter lists.
- Added checker validation for TypeSharp-owned `params` declarations: only one `params` parameter is allowed, it must be final, and it must be array-typed.
- Extended direct TypeSharp-declared function calls so final-array `params` parameters accept one exact array argument or expanded trailing element arguments, including zero trailing arguments after fixed parameters.
- Extended direct generic function call inference/substitution through the implemented `params` tail for exact array and expanded element argument shapes.
- Extended first-argument pipeline validation so known TypeSharp-declared targets account for final-array `params` tails after pipeline lowering.
- Lowered accepted final-array `params` declarations to C# 7.3-compatible `params T[]` signatures while keeping generated calls ordinary C# invocations.
- Added parser, type-checker positive/negative, backend snapshot, and CLI generated `net48` compile smoke coverage.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, and Traceability docs with the implemented boundary.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles TypeSharp params parameter lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "diagnostic fixture polarity is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
npm run build          # in docs
git diff --check
```

Primary evidence:

- [SyntaxKind.cs](../lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs)
- [TypeSharpLexer.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs)
- [TypeSharpParser.cs](../lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs)
- [TypeSharpTypeChecker.cs](../lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs)
- [CSharpSourceBackend.cs](../lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs)
- `test/fixtures/parser/positive/0037-params-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/positive/params-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/negative/params-parameter-declaration`
- `test/fixtures/backend/csharp/positive/0045-params-parameter-lowering`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [traceability.md](traceability.md)
- [tasks.md](tasks.md)

Remaining:

- Roadmap refresh after params parameter declarations is active in task 0367. Optional/default parameter declarations, named argument binding for TypeSharp-declared functions, TypeSharp overload ranking, imported C# `params` behavior changes, constructor/delegate/union-case broadening, `params` in function-type syntax, higher-order function values, currying, partial application, public composition ABI inference, imported composition targets, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra remain future work.

## Task 0367 Roadmap Refresh After Params Parameter Declaration

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, and VS Code source signals on 2026-05-21 after TypeSharp-owned final-array `params` parameter declarations landed.
- Confirmed no baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 remains Beta/native-preview tooling direction, F# 10 remains a refinement/tooling signal, and package/Marketplace/template publication remains gated by Project Policy.
- Reviewed Feature Status, Work Ledger, completed `params` parameter declaration behavior, and the remaining parameter/function ergonomics backlog.
- Kept optional/default parameter declarations, named argument binding for TypeSharp-declared functions, TypeSharp overload ranking, imported C# `params` behavior changes, constructor/delegate/union-case broadening, `params` in function-type syntax, higher-order function values, currying, partial application, public composition ABI inference, imported composition targets, broader generic constraints, type constructor policy, numeric shifts, shift assignment, user-defined operators, TypeSharp member assignment policy, broad class-member body analysis, flag-aware enum algebra, broad attribute target validation, numeric pattern algebra, imported enum flag reasoning, arbitrary/general computed enum member declarations, and richer pattern algebra as future language work.
- Promoted the pending User Task Inbox request as the next active bounded task because active work completed before user-owned queued work should continue: `0368 Test suite runtime reduction plan and refactor`.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [`dotnet restore` auditing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet trusted signers](https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-trusted-signers)
- [VS Code Language Server Extension Guide](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Test suite runtime reduction planning/refactor is active in task 0368. The user-owned inbox item remains unchecked until the refactor is actually completed.

## Task 0368 Test Suite Runtime Reduction Plan And Refactor

Completed test-runtime work established:

- Profiled the current custom `TypeSharp.Compiler.Tests` runner and found 212 `BuildLegacyReferenceDll(` call sites, mostly in C# interop checker and generated compile smokes.
- Confirmed the measured bottleneck was repeated `dotnet build Legacy.Tools.csproj`/legacy `net48` reference assembly setup, not assertion framework dispatch.
- Added a content-hash keyed cache for generated legacy reference assemblies under ignored `test/tmp/legacy-reference-cache`; each test still copies the cached DLL into its own workspace-local `lib` folder.
- Added a named mutex around cache population so parallel test processes cannot race the same generated project output.
- Added `--shard <zero-based-index>/<count>` runner selection and four shard projects that link the same runner with stable `index % 4` test slices.
- Kept the original main runner as the full release-confidence path and documented the faster parallel shard path in `test/README.md`.
- Evaluated NuGet test framework adoption: MSTest SDK/Microsoft Testing Platform and xUnit.net v3 are viable future migration paths, but this slice avoided adding a package before extracting the custom `(name, Action)` catalog into discoverable test cases because that would not address the measured repeated-build bottleneck.
- Updated Project Policy with the shard-runner invariant: shared generated inputs need process-safe caches and each test keeps a unique `test/tmp` workspace.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard1\TypeSharp.Compiler.Tests.Shard1.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard2\TypeSharp.Compiler.Tests.Shard2.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard3\TypeSharp.Compiler.Tests.Shard3.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "test runner shard selection"
dotnet run --project test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard1\TypeSharp.Compiler.Tests.Shard1.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard2\TypeSharp.Compiler.Tests.Shard2.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard3\TypeSharp.Compiler.Tests.Shard3.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
git diff --check
```

Primary evidence:

- `test/TypeSharp.Compiler.Tests/Program.cs`
- `test/TypeSharp.Compiler.Tests/TestShardDefaults.cs`
- `test/TypeSharp.Compiler.Tests.Shard0`
- `test/TypeSharp.Compiler.Tests.Shard1`
- `test/TypeSharp.Compiler.Tests.Shard2`
- `test/TypeSharp.Compiler.Tests.Shard3`
- `test/README.md`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [MSTest SDK guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [xUnit.net v3 packages](https://xunit.net/docs/nuget-packages-v3)

Measured result:

- Main full runner: 517 PASS, about 209.0s.
- Four parallel shard projects: 517 total PASS, about 68.0s wall-clock.

Remaining:

- Roadmap refresh after test-suite runtime reduction is active in task 0369. A future MSTest SDK/Microsoft Testing Platform or xUnit.net v3 migration remains possible after extracting the custom test catalog into discoverable test cases.

## Task 0369 Roadmap Refresh After Test Suite Runtime Reduction

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, and .NET testing source signals on 2026-05-22 after test-suite runtime reduction landed.
- Confirmed no generated-artifact baseline change: TypeSharp generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 14 remains the current stable .NET 10 C# signal, C# 15 remains preview on .NET 11 preview, TypeScript 7.0 Beta remains a native-compiler/tooling signal, and F# 10 remains a refinement/tooling signal.
- Confirmed NuGet policy still separates generated runtime dependencies from modern host/test tooling. MSTest SDK/Microsoft Testing Platform and xUnit.net v3 are viable package-based test-host targets for the `net10.0` test projects, but they do not replace the measured repeated legacy-reference build bottleneck already handled by task 0368.
- Reviewed the completed shard/cache behavior against Project Policy and Work Ledger: the main custom runner remains the full release-confidence gate, shard projects execute disjoint catalog slices in parallel, shared generated inputs use a process-safe cache, and tests keep unique `test/tmp` workspaces.
- Selected the next bounded implementation slice: `0370 Test catalog extraction for framework migration prerequisite`.
- Kept full MSTest SDK/Microsoft Testing Platform or xUnit.net v3 migration, custom runner removal, test semantics changes, and new test NuGet package references out of this roadmap-refresh slice.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [F# strategy](https://learn.microsoft.com/en-us/dotnet/fsharp/strategy)
- [TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [`dotnet restore` auditing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet trusted signers](https://learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-trusted-signers)
- [VS Code Language Server Extension Guide](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [MSTest SDK guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [xUnit.net v3 packages](https://xunit.net/docs/nuget-packages-v3)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0370: the reusable catalog now exists and the pinned MSTest SDK/Microsoft Testing Platform bridge reuses it. Future xUnit.net v3 or CI adoption should keep using the same catalog.

## Task 0370 Test Catalog Extraction And MSTest Bridge

Completed test-harness work established:

- Split the former monolithic custom runner into reusable internal harness files: `TypeSharpCompilerTestCase`, `TypeSharpCompilerTestCases`, `TypeSharpCompilerTestCatalog`, `TypeSharpCompilerTestRunner`, and `TestRunnerSettings`.
- Reduced `Program.cs` to the console entry point that runs `TypeSharpCompilerTestRunner.Run(TypeSharpCompilerTestCases.All, args)`.
- Updated all four shard projects to link the same extracted catalog and runner harness, preserving stable test names, filtering, shard selection, pass/fail output, and exit-code behavior.
- Added catalog stability coverage to the runner self-test: expected count, first/last names, and distinct names.
- Added a `net10.0` MSTest SDK/Microsoft Testing Platform bridge in `test/TypeSharp.Compiler.Tests.MSTest`, pinned to `MSTest.Sdk/4.2.3`, with root `global.json` opting .NET 10 `dotnet test` into MTP mode.
- Kept generated `net48` artifacts, `TypeSharp.Core`, `TypeSharp.Runtime`, and the package-free main/shard runners free of test-framework package references.
- Confirmed the NuGet package bridge is useful for `dotnet test` discovery and ecosystem integration, but not the fastest release-confidence path: full MSTest catalog execution passed in about 3m27s while the four-shard custom runner path passed in about 67.5s wall-clock.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard1\TypeSharp.Compiler.Tests.Shard1.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard2\TypeSharp.Compiler.Tests.Shard2.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard3\TypeSharp.Compiler.Tests.Shard3.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "test runner shard selection"
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogCase" --minimum-expected-tests 517
# four shard projects via Start-Job
npm run build          # in docs
git diff --check
```

Primary evidence:

- `global.json`
- `test/TypeSharp.Compiler.Tests/Program.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCase.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestRunner.cs`
- `test/TypeSharp.Compiler.Tests/TestRunnerSettings.cs`
- `test/TypeSharp.Compiler.Tests.Shard0`
- `test/TypeSharp.Compiler.Tests.Shard1`
- `test/TypeSharp.Compiler.Tests.Shard2`
- `test/TypeSharp.Compiler.Tests.Shard3`
- `test/TypeSharp.Compiler.Tests.MSTest`
- `test/README.md`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [.NET `dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)

Remaining:

- Completed in task 0371: the roadmap refresh after the MSTest catalog bridge selected test-host NuGet package selection plus restore hardening before broader CI adoption.

## Task 0371 Roadmap Refresh After MSTest Catalog Bridge

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, VS Code, .NET testing, MSTest SDK, and xUnit.net v3 source signals on 2026-05-22 after the pinned MSTest SDK/Microsoft Testing Platform bridge landed.
- Confirmed no generated-artifact baseline change: TypeSharp generated artifacts stay `net48`, generated source stays C# 7.3-compatible, C# 14 remains the current stable .NET 10 C# signal, C# 15 remains preview on .NET 11 preview, TypeScript 6.0 is the transition release toward TypeScript 7.0, TypeScript 7.0 Beta remains native-preview tooling, and F# 10 remains a refinement/tooling signal.
- Confirmed TypeSharp is already using a `net10.0` NuGet test-host bridge: `MSTest.Sdk/4.2.3` remains the pinned package, .NET 10 `dotnet test` uses MTP mode through `global.json`, xUnit.net v3 remains a viable comparison point, and the package-free custom shard runner remains the faster release-confidence path.
- Selected the next bounded task: `0372 Test-host NuGet package selection and restore hardening`, because introducing the first test-host package SDK should be followed by an explicit MSTest SDK versus xUnit.net v3 comparison plus lock/source-mapping/audit review before broader CI adoption.
- Kept generated-artifact target changes, test framework package changes, xUnit.net v3 implementation, CI migration, and language feature work out of this roadmap-refresh slice.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [TypeScript 6.0 release notes](https://www.typescriptlang.org/docs/handbook/release-notes/typescript-6-0.html)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework installation on Windows](https://learn.microsoft.com/en-us/dotnet/framework/install/on-windows-and-server)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
- [`dotnet test` command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0372: MSTest SDK/MTP remains the selected package bridge, xUnit.net v3 remains a future candidate, and the selected test-host package path now has lock/source-mapping/audit controls.

## Task 0372 Test-host NuGet Package Selection And Restore Hardening

Completed test-host package hardening work established:

- Confirmed TypeSharp was already using a `net10.0` NuGet package-based test bridge through `Project Sdk="MSTest.Sdk/4.2.3"` in `test/TypeSharp.Compiler.Tests.MSTest`.
- Compared MSTest SDK/Microsoft Testing Platform and xUnit.net v3 against TypeSharp's needs. MSTest remains selected for the first package bridge because it is Microsoft-supported, defaults to MTP, works with the root `.NET 10` MTP `global.json`, and exposes `TypeSharpCompilerTestCases.All` with minimal bridge code. xUnit.net v3 remains a future bridge candidate over the same catalog if it provides distinct ecosystem value.
- Added root `NuGet.config` that clears inherited package sources, uses `nuget.org` as the package and audit source, maps the current MSTest bridge package graph to `nuget.org`, and sends restored packages to ignored repo-local `.nuget/packages`.
- Added `.nuget/` to `.gitignore`.
- Enabled `RestorePackagesWithLockFile`, `RestoreLockedMode` under `ContinuousIntegrationBuild=true`, `NuGetAudit`, transitive audit mode, and high-severity audit threshold in `test/TypeSharp.Compiler.Tests.MSTest`.
- Checked in `test/TypeSharp.Compiler.Tests.MSTest/packages.lock.json` for the generated MSTest bridge graph: direct `MSTest.TestAdapter` and `MSTest.TestFramework` plus Microsoft Testing Platform transitives.
- Documented the MSBuild SDK restore exception: `MSTest.Sdk` itself is restored before the normal project package graph and is not listed as a normal lock-file dependency. The compensating controls are the exact `Project Sdk` pin, source mapping for `MSTest.*`, repo-local package cache, and locked restore for the emitted graph.
- Kept generated `net48` artifacts, `TypeSharp.Core`, `TypeSharp.Runtime`, package-free main/shard runners, CI migration, compiler NuGet restore, and generated target frameworks unchanged.

Verification:

```powershell
dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
dotnet build-server shutdown
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
rg -n "PackageReference|packages\.config" lang\TypeSharp.Core lang\TypeSharp.Runtime
npm run build          # in docs
git diff --check
```

Primary evidence:

- `NuGet.config`
- `.gitignore`
- `global.json`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharp.Compiler.Tests.MSTest.csproj`
- `test/TypeSharp.Compiler.Tests.MSTest/packages.lock.json`
- `test/README.md`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet audit](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
- [`dotnet restore` command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)

Remaining:

- Completed in task 0373: the roadmap refresh after test-host NuGet hardening selected CI regression gating over the shard runner and focused MSTest bridge smoke.

## Task 0373 Roadmap Refresh After Test-host NuGet Hardening

Completed roadmap refresh work established:

- Rechecked official source signals on 2026-05-22 after test-host NuGet hardening. The same-day source set still supports the current baseline: `.NET Framework` target frameworks map to C# 7.3, modern C# 14/.NET 10 and C# 15/.NET 11 signals do not change generated artifact requirements, TypeScript 7.0 remains a native-preview transition signal, .NET 10 `dotnet test` MTP mode is the right package-based discovery path, and xUnit.net v3 remains a future comparison candidate.
- Confirmed no generated-artifact baseline change: generated artifacts stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and the package-free custom shard runner remains the release-confidence path.
- Inspected current workflows and confirmed only docs deployment and release artifact packaging exist; there is no push/PR compiler regression CI gate yet.
- Selected the next bounded task: `0374 CI regression gate for shard runner and MSTest smoke`. The gate should run on Windows, exercise the four package-free shard projects, restore the MSTest bridge in locked mode, and run a focused MSTest bridge smoke for package-based discovery evidence.
- Kept actual CI workflow implementation, xUnit.net v3 adoption, compiler NuGet restore, generated target changes, and test semantics changes out of this roadmap-refresh slice.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- `.github/workflows/docs.yml`
- `.github/workflows/release-artifacts.yml`
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0374: the Windows regression gate now exercises the package-free shard runner and focused MSTest smoke.

## Task 0374 CI Regression Gate For Shard Runner And MSTest Smoke

Completed CI regression gate work established:

- Added `.github/workflows/regression.yml` for Windows push, pull request, and manual regression runs over compiler, language, test, examples, VS Code, NuGet/global SDK, and workflow changes.
- Pinned .NET `10.0.x` and Node `24` in the workflow to match the existing release/test host assumptions.
- Restores `test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj` in locked mode so the root `NuGet.config`, package source mapping, audit source, repo-local package cache, and `packages.lock.json` controls are exercised in CI.
- Builds the package-free main runner, builds all four package-free shard runner projects, and runs the four shards in parallel with CI failure propagation.
- Builds the MSTest SDK/Microsoft Testing Platform bridge without restore and runs the focused `CatalogIsExposedForPackageRunners` smoke as package-based discovery evidence.
- Added regression workflow assertions to the existing workflow contract test coverage without changing the 517-case catalog membership.
- Documented the gate in `test/README.md`, Project Policy, Feature Status, Work Ledger, and traceability.
- Kept generated artifacts on `net48`, generated C# on C# 7.3-compatible output, `TypeSharp.Core`/`TypeSharp.Runtime` package-free, and full MSTest catalog execution optional because the shard runner remains the release-confidence path.

Verification:

```powershell
dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "release and regression workflow contracts"
$shards = 0..3 | ForEach-Object { "test\TypeSharp.Compiler.Tests.Shard$_\TypeSharp.Compiler.Tests.Shard$_.csproj" }; foreach ($project in $shards) { dotnet build $project --nologo --verbosity quiet }
# Ran the workflow-equivalent PowerShell Start-Job shard command; all four shard runners passed in about 70 seconds.
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-restore --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
npm run build          # in docs
```

Primary evidence:

- `.github/workflows/regression.yml`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/README.md`
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0375: roadmap refresh after the CI regression gate selected optional/default parameter declarations.

## Task 0375 Roadmap Refresh After CI Regression Gate

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET test platform, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after the Windows regression CI gate landed.
- Confirmed no generated-artifact baseline change: generated TypeSharp assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and compiler/CLI/LSP/test hosts may continue using modern .NET and Node tooling.
- Confirmed the CI posture now has a Windows push/PR/manual regression gate over the package-free shard runner plus focused MSTest/MTP discovery smoke. Full MSTest catalog execution and xUnit.net v3 remain optional future evidence, not replacements for the shard runner.
- Recorded GitHub Actions as a watch item: `windows-latest`, setup-dotnet, and setup-node remain valid for the regression workflow, while the June 2026 Windows Server 2025 + Visual Studio 2026 hosted-runner image migration should be monitored by CI rather than changing TypeSharp's baseline now.
- Selected the next bounded implementation slice: `0376 Optional/default parameter declarations`, focused on TypeSharp-owned direct function parameters with deterministic diagnostics and C# 7.3-compatible generated output.
- Kept generated target changes, compiler NuGet restore, xUnit.net v3 adoption, VS Code Marketplace publication, `dotnet new` template packs, and host packaging automation out of this roadmap-refresh slice.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0376: optional/default parameter declarations for TypeSharp-owned direct functions.

## Task 0376 Optional Default Parameter Declaration Slice

Completed implementation work established:

- Added parser support for defaulted parameters on direct top-level/module/exported `fun` declarations as `name: Type = literal`, while keeping interface/class/extension signatures outside the initial parser surface.
- Added type-checker support for trailing defaulted suffixes on non-generic TypeSharp-owned direct functions, including omitted direct-call and first-argument pipeline arguments.
- Added deterministic `TS2201` diagnostics for non-trailing defaults, missing explicit parameter types, unsupported default expressions, default/type and nullability mismatches, `params` interaction, generic functions, and ambient/`extern` signatures.
- Added C# source lowering for optional parameters with C# 7.3-compatible literal defaults and nullable value-type mapping for generated signatures.
- Added parser, type-checker, and backend fixtures for accepted and rejected optional/default parameter declarations.
- Added a generated `net48` library smoke that asserts emitted C# optional parameter signatures, public ABI optional metadata, and C# 7.3 `net48` consumer calls with omitted arguments.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, .NET Interop, Feature Status, Project Policy, and test/MSTest catalog documentation for the 518-case catalog.
- Kept imported C# optional/default behavior, TypeSharp named arguments, generic defaults, overload ranking, constructors, delegates, union cases, function types, lambdas, and higher-order values out of this slice.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "optional/default parameter"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "fixture scenario README coverage is stable"
dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-restore --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
# Built all four shard projects and ran the package-free shard runners in parallel with PowerShell Start-Job; all 518 catalog cases passed.
npm run build          # in docs
git diff --check
```

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/fixtures/parser/positive/0038-optional-default-parameter-declaration`
- `test/fixtures/parser/negative/0002-missing-default-parameter-expression`
- `test/fixtures/diagnostics/type-checker/positive/optional-default-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/negative/optional-default-parameter-declaration`
- `test/fixtures/backend/csharp/positive/0046-optional-default-parameter-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

Remaining:

- Completed in task 0377: roadmap refresh after optional/default parameter declarations selected direct TypeSharp named argument binding next.

## Task 0377 Roadmap Refresh After Optional Default Parameters

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after the optional/default parameter implementation slice.
- Confirmed no generated-artifact baseline change: generated TypeSharp assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and compiler/CLI/LSP/test hosts may continue using modern .NET and Node tooling.
- Kept C# 14 stable and C# 15 preview as language-design inputs only. C# language versioning still maps `.NET Framework` targets to C# 7.3, while C# 15 union and collection-expression argument signals remain preview/watch items rather than generated-source requirements.
- Kept F# 10 and TypeScript 7.0 Beta as tooling, diagnostics, and compiler-engineering signals only. They do not introduce an F# runtime, TypeScript runtime, Go toolchain, or preview compiler dependency into TypeSharp.
- Confirmed NuGet lock/source-mapping/audit guidance, .NET 10 MTP mode, MSTest SDK/MTP, and xUnit.net v3 package guidance do not require replacing the current package-free shard runner or pinned MSTest bridge.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or CI baselines. The Windows Server 2025 + Visual Studio 2026 hosted image remains a CI watch item, not a TypeSharp generated-artifact baseline change.
- Selected the next bounded implementation slice: `0378 Direct TypeSharp named argument binding`, focused on known TypeSharp-owned direct function calls and first-argument pipeline targets. The slice should reuse existing parser/backend named-argument machinery, lower accepted TypeSharp-owned named calls to positional C# calls, and keep TypeSharp overload ranking/imported C# behavior changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Active in task 0378: direct TypeSharp named argument binding for known TypeSharp-owned direct functions.

## Task 0378 Direct TypeSharp Named Argument Binding

Completed bounded TypeSharp-owned named argument binding:

- Added declared parameter names to TypeSharp function metadata and used them for known non-generic direct `fun` calls.
- Added checker binding for named arguments after any positional prefix, including `TS2201` diagnostics for unknown names, duplicate names, positional arguments after named arguments, missing required parameters, argument type mismatches, generic functions, and `params` combinations.
- Added first-argument pipeline support where the target is a known TypeSharp-owned function and non-piped arguments use names, while keeping the piped input bound to parameter 1.
- Updated C# lowering so accepted TypeSharp-owned named calls emit ordinary positional C# calls, preserving generated `net48` and C# 7.3 compatibility. Imported C# named calls remain on the metadata-backed interop path.
- Added type-checker positive/negative fixtures, a backend C# snapshot, and a generated `net48` CLI/C# consumer smoke. The shared catalog is now 519 cases.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, test README, and traceability.

Verification:

```powershell
dotnet build lang/TypeSharp.Compiler/TypeSharp.Compiler.csproj
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- --filter "TypeSharp named argument lowering"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- --filter "test runner shard selection"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- --filter "C# backend fixture snapshots match"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj -- --filter "type checker fixture diagnostics match"
dotnet test test/TypeSharp.Compiler.Tests.MSTest/TypeSharp.Compiler.Tests.MSTest.csproj --filter "CatalogIsExposedForPackageRunners" --no-restore --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: all listed commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/fixtures/diagnostics/type-checker/positive/direct-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/negative/direct-named-argument-binding`
- `test/fixtures/backend/csharp/positive/0047-direct-named-argument-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

Remaining:

- Completed in task 0379: roadmap refresh after direct named argument binding selected generic TypeSharp named argument binding next.

## Task 0379 Roadmap Refresh After Direct Named Arguments

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after the direct TypeSharp named argument binding implementation slice.
- Confirmed no generated-artifact baseline change: generated TypeSharp assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and compiler/CLI/LSP/test hosts may continue using modern .NET and Node tooling.
- Kept C# 14 stable and C# 15 preview as language-design inputs only. C# language versioning still maps `.NET Framework` targets to C# 7.3, while C# 15 union and collection-expression argument signals remain preview/watch items rather than generated-source requirements.
- Kept F# 10 and TypeScript 7.0 Beta as tooling, diagnostics, and compiler-engineering signals only. They do not introduce an F# runtime, TypeScript runtime, Go toolchain, or preview compiler dependency into TypeSharp.
- Confirmed .NET Framework 4.8.1 remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a separate qualified-profile backlog item.
- Confirmed NuGet lock/source-mapping/audit guidance, .NET 10 MTP mode, MSTest SDK/MTP, and xUnit.net v3 package guidance do not require replacing the current package-free shard runner or pinned MSTest bridge.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or CI baselines. The Windows Server 2025 + Visual Studio 2026 hosted image migration scheduled for June 2026 remains a CI monitoring item, not a TypeSharp generated-artifact baseline change.
- Selected the next bounded implementation slice: `0380 Generic TypeSharp named argument binding`, focused on known TypeSharp-owned generic direct calls and first-argument pipeline targets. The slice should reuse the existing parameter-name binding, existing bounded generic inference shapes, and positional C# lowering, while keeping overload ranking, `params` broadening, and imported C# behavior changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0380: generic TypeSharp named argument binding for known TypeSharp-owned generic functions.

## Task 0380 Generic TypeSharp Named Argument Binding

Completed bounded generic TypeSharp-owned named argument binding:

- Extended the checker so known TypeSharp-owned generic direct calls can bind named arguments through the existing parameter-name map and bounded generic inference/substitution shapes.
- Extended first-argument pipeline validation so generic TypeSharp targets can bind named non-piped arguments while keeping the piped input bound to parameter 1.
- Kept generic `params` named direct and pipeline combinations out of scope with deterministic `TS2201` diagnostics before C# emission.
- Updated C# lowering so accepted generic TypeSharp-owned named calls emit ordinary positional C# calls, including explicit generic calls such as `choose<string>("c", "d")` and inferred pipeline calls such as `choose("h", "i")`.
- Preserved imported C# named arguments on the metadata-backed interop path.
- Added generic named-argument positive/negative type-checker fixtures, backend snapshot `0048-generic-named-argument-lowering`, and a generated `net48` CLI/C# consumer smoke. The shared catalog is now 520 cases.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, test README, and traceability.

Verification:

```powershell
dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build -- --filter "type checker fixture diagnostics match"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build -- --filter "C# backend fixture snapshots match"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build -- --filter "generic named argument"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build -- --filter "test runner shard selection"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build -- --filter "diagnostic fixture polarity"
dotnet test test/TypeSharp.Compiler.Tests.MSTest/TypeSharp.Compiler.Tests.MSTest.csproj --filter "CatalogIsExposedForPackageRunners" --no-restore --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: all listed commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/fixtures/diagnostics/type-checker/positive/generic-named-argument-binding`
- `test/fixtures/diagnostics/type-checker/negative/generic-named-argument-binding`
- `test/fixtures/backend/csharp/positive/0048-generic-named-argument-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)

Remaining:

- Completed in task 0381: roadmap refresh after generic TypeSharp named arguments selected the MSTest package shard bridge next.

## Task 0381 Roadmap Refresh After Generic TypeSharp Named Arguments

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after generic TypeSharp named argument binding.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Kept C# 14 stable on .NET 10 and C# 15 preview on .NET 11 as language-design inputs only. Microsoft C# language versioning still maps `.NET Framework` targets to C# 7.3.
- Kept F# 10 and TypeScript 7.0 Beta as tooling, diagnostics, and compiler-engineering signals only. They do not introduce an F# runtime, TypeScript runtime, Go toolchain, or preview compiler dependency into TypeSharp.
- Confirmed .NET Framework 4.8.1 remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a separate qualified-profile backlog item.
- Confirmed TypeSharp already uses a `net10.0` NuGet package bridge through `MSTest.Sdk/4.2.3`, `.NET 10` `dotnet test` MTP mode, NuGet lock files, package source mapping, repo-local package cache, and restore audit controls.
- Answered the test-package optimization concern by selecting the next bounded slice: `0382 MSTest package shard bridge`. The slice should make the package-based MSTest/MTP path shardable over the existing shared catalog, while keeping the package-free four-shard runner as the fastest release-confidence path and keeping xUnit.net v3 as a future comparison candidate only.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or generated-artifact baselines. The `windows-latest`/`windows-2025` migration to Visual Studio 2026 beginning 2026-06-08 remains a CI watch item.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [TypeScript 6.0 release notes](https://www.typescriptlang.org/docs/handbook/release-notes/typescript-6-0.html)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet restore audit](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [.NET testing overview](https://learn.microsoft.com/en-us/dotnet/core/testing/)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test)
- [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0382: MSTest package shard bridge.

## Task 0382 MSTest Package Shard Bridge

Completed package-based test sharding work:

- Added four `net10.0` `MSTest.Sdk/4.2.3` shard bridge projects under `test/TypeSharp.Compiler.Tests.MSTest.Shard0` through `Shard3`.
- Reused the existing `TypeSharpCompilerTestCases.All` catalog and the existing `TestShardDefaults` ordinal partition model, so package-free and package-based shard runners share the same test list and stable shard math.
- Updated the MSTest dynamic-data bridge so each shard project exposes only its `index % 4` catalog slice, while the base `TypeSharp.Compiler.Tests.MSTest` project still exposes the full catalog for package-runner discovery smoke.
- Checked in package lock files for each MSTest shard project. The restore graph remains the existing MSTest SDK/Microsoft Testing Platform graph mapped to `nuget.org` through the root `NuGet.config`.
- Updated the Windows regression workflow to restore all MSTest bridge projects in locked mode, build the package shard bridge projects, run the existing MSTest package discovery smoke, and run the four MSTest package shard bridges in parallel with minimum expected catalog counts.
- Updated the test README with package-based shard commands and added regression tests for the project/lock/workflow contracts. The shared catalog is now 521 cases.

Verification:

```powershell
dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode --verbosity minimal
dotnet restore test\TypeSharp.Compiler.Tests.MSTest.Shard0\TypeSharp.Compiler.Tests.MSTest.Shard0.csproj --locked-mode --verbosity minimal
dotnet restore test\TypeSharp.Compiler.Tests.MSTest.Shard1\TypeSharp.Compiler.Tests.MSTest.Shard1.csproj --locked-mode --verbosity minimal
dotnet restore test\TypeSharp.Compiler.Tests.MSTest.Shard2\TypeSharp.Compiler.Tests.MSTest.Shard2.csproj --locked-mode --verbosity minimal
dotnet restore test\TypeSharp.Compiler.Tests.MSTest.Shard3\TypeSharp.Compiler.Tests.MSTest.Shard3.csproj --locked-mode --verbosity minimal
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-restore --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.MSTest.Shard0\TypeSharp.Compiler.Tests.MSTest.Shard0.csproj --no-restore --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.MSTest.Shard1\TypeSharp.Compiler.Tests.MSTest.Shard1.csproj --no-restore --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.MSTest.Shard2\TypeSharp.Compiler.Tests.MSTest.Shard2.csproj --no-restore --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.MSTest.Shard3\TypeSharp.Compiler.Tests.MSTest.Shard3.csproj --no-restore --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "MSTest package shard bridge"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "release and regression workflow contracts"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "test runner shard selection"
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet
# Ran the four package-free shard projects in parallel.
# Ran the four MSTest package shard projects in parallel with minimum expected counts 131, 130, 130, and 130.
npm run build          # in docs
git diff --check
```

Result: all listed commands succeeded on 2026-05-22; the package-free shard runner still passed the full catalog, the parallel MSTest package shards passed 131/130/130/130 catalog cases, the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `.github/workflows/regression.yml`
- `test/README.md`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests.MSTest.Shard0`
- `test/TypeSharp.Compiler.Tests.MSTest.Shard1`
- `test/TypeSharp.Compiler.Tests.MSTest.Shard2`
- `test/TypeSharp.Compiler.Tests.MSTest.Shard3`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0383: roadmap refresh after MSTest package shards selected generic optional/default parameters next.

## Task 0383 Roadmap Refresh After MSTest Package Shards

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after package-based MSTest shard execution landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Kept C# 14 stable on .NET 10 and C# 15 preview on .NET 11 as language-design inputs only. Microsoft C# language versioning still maps `.NET Framework` targets to C# 7.3, and C# 15 union/runtime helper churn remains preview-only.
- Kept F# 10 and TypeScript 7.0 Beta as tooling, diagnostics, and compiler-engineering signals only. TypeScript 7.0's fixed worker/checker partitioning and side-by-side native-preview strategy reinforce TypeSharp's deterministic parallel test/compiler policy, not a runtime or toolchain dependency.
- Confirmed .NET Framework 4.8.1 remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a separate qualified-profile backlog item.
- Confirmed TypeSharp's now-sharded `net10.0` MSTest SDK/MTP package bridge remains aligned with .NET 10 `dotnet test` MTP mode, `MSTest.Sdk/4.2.3`, NuGet lock files, package source mapping, repo-local package cache, restore audit controls, and GitHub Actions setup-dotnet/setup-node pins.
- Kept xUnit.net v3 as a future comparison candidate only. The new package-based MSTest shards already cover the shared catalog in parallel, so adding another runner package now would duplicate the same evidence without advancing TypeSharp's language surface.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or generated-artifact baselines. The `windows-latest`/`windows-2025` migration to Visual Studio 2026 beginning 2026-06-08 remains a CI watch item.
- Selected the next bounded implementation slice: `0384 Generic TypeSharp optional/default parameters`. The slice should lift the current generic-function exclusion only for concrete trailing literal defaults on TypeSharp-owned generic direct `fun` declarations, reuse existing bounded generic inference/substitution paths for direct and first-argument pipeline calls, keep `params`, type-parameter-typed defaults, ambient/extern, overload ranking, and imported C# behavior out of scope, and preserve generated `net48` C# 7.3 optional metadata.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet restore audit](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0384: generic TypeSharp optional/default parameters.

## Task 0384 Generic TypeSharp Optional Default Parameters

Completed generic optional/default parameter work:

- Removed the blanket `TS2201` rejection for TypeSharp-owned generic direct `fun` declarations with defaulted parameters.
- Added a bounded generic-default rule: every defaulted parameter in a generic `fun` must have an explicit concrete TypeSharp type that does not reference a declared type parameter. Defaulted parameter types such as `T`, `T[]`, or matching wrappers that reference `T` report deterministic `TS2201` before C# emission.
- Reused the existing direct generic explicit/inferred call, named generic call, and first-argument generic pipeline inference/substitution paths so accepted calls can omit the supported trailing concrete optional/default suffix.
- Kept `params` combinations, ambient/`extern` signatures, constructors, delegates, union cases, function types, lambdas, higher-order values, overload ranking, imported C# behavior changes, and broader generic type-constructor unification out of scope.
- Reused the existing C# backend optional-parameter emission so generated generic methods keep C# 7.3-compatible optional metadata in `net48` assemblies.
- Added positive and negative type-checker fixtures, a C# backend snapshot, and a generated `net48` C# consumer smoke. The shared catalog is now 522 cases.
- Updated Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, task control, and traceability records for the new bounded behavior.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles TypeSharp optional/default parameter lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles TypeSharp generic named argument lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles TypeSharp generic optional/default parameter lowering"
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: all listed commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/diagnostics/type-checker/positive/generic-optional-default-parameter-declaration`
- `test/fixtures/diagnostics/type-checker/negative/generic-optional-default-parameter-declaration`
- `test/fixtures/backend/csharp/positive/0049-generic-optional-default-parameter-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Active in task 0385: roadmap refresh after generic optional/default parameters.

## Task 0385 Roadmap Refresh After Generic Optional Default Parameters

Completed roadmap refresh work established:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals on 2026-05-22 after generic optional/default parameters landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed the current test-package answer: TypeSharp already uses a `net10.0` NuGet package bridge through pinned `MSTest.Sdk/4.2.3`, .NET 10 Microsoft Testing Platform mode, checked-in package lock files, package source mapping, audit controls, and four package-based MSTest shard projects over the shared catalog. Adding another runner package now would duplicate the same evidence instead of improving generated artifact compatibility.
- Kept C# 14 stable on .NET 10 and C# 15 preview on .NET 11 as language-design inputs only. Microsoft C# language versioning still maps `.NET Framework` targets to C# 7.3, and C# 15 union/runtime helper churn remains preview-only.
- Kept F# 10 and TypeScript 7.0 Beta as tooling, diagnostics, and compiler-engineering signals only. TypeScript 7.0's side-by-side native-preview strategy reinforces TypeSharp's deterministic migration and parallel-test policy, not a runtime or generated-artifact dependency.
- Confirmed .NET Framework 4.8.1 remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a separate qualified-profile backlog item.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or generated-artifact baselines. The Windows Server 2025 + Visual Studio 2026 hosted-runner migration beginning June 2026 remains a CI watch item.
- Selected the next bounded implementation slice: `0386 Integral numeric shift expressions`. The slice should accept `left << right` and `left >> right` only for known non-null primitive integral operands, preserve `>>`/`<<` composition for function-shaped operands, keep ambiguous or non-integral value-shaped operands diagnostic-first, lower accepted shifts to ordinary C# 7.3-compatible operators, and keep shift assignment, logical unsigned shifts, user-defined operators, enum flag algebra, imported operator overloads, and higher-order composition changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet restore audit](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#mtp-mode-of-dotnet-test)
- [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Active in task 0386: integral numeric shift expressions.

## Task 0386 Integral Numeric Shift Expressions

Completed on 2026-05-22.

Summary:

- Accepted `left << right` and `left >> right` only when both operands type-check as known non-null primitive integral values and the right count is a C# 7.3-compatible non-null `byte`, `sbyte`, `short`, `ushort`, or `int`.
- Preserved existing `>>` and `<<` composition behavior for function-shaped operands, including the existing composition lowering smoke.
- Reused C# shift result rules for generated `net48` source: small left operands promote to `int`, while `int`, `uint`, `long`, and `ulong` keep the left operand result shape.
- Changed invalid value-shaped shift-looking operands such as `bool`, `string`, `decimal`, enum values, nullable integral values, unsupported `uint` counts, and records to deterministic `TS2201` diagnostics before backend emission.
- Lowered accepted primitive integral shifts to ordinary C# `<<` and `>>` expressions in C# 7.3-compatible generated source.
- Added backend and generated `net48` C# consumer evidence for the new shift surface and updated the shared catalog to 523 cases with four-shard expected counts of `131`, `131`, `131`, and `130`.
- Kept shift assignment, logical unsigned shifts, user-defined operators, enum flag algebra beyond existing same-enum operators, imported operator overload resolution, parser token reshaping, and higher-order/imported composition changes out of scope.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "CLI build compiles integral shift expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "CLI build compiles composition lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "fixture scenario README coverage is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "test runner shard selection"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "MSTest package shard bridge"
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: compiler/MSTest builds and targeted harness checks succeeded on 2026-05-22; docs build and diff checks succeeded after ledger updates.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `test/fixtures/diagnostics/type-checker/positive/integral-shift-expression`
- `test/fixtures/diagnostics/type-checker/negative/composition-shift-ambiguity`
- `test/fixtures/backend/csharp/positive/0050-integral-shift-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/README.md`
- `.github/workflows/regression.yml`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0387: roadmap refresh after integral shift expressions.
- Completed in task 0388: shift assignment expressions.

## Task 0387 Roadmap Refresh After Integral Shift Expressions

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after integral numeric shifts landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed C# language versioning still maps `.NET Framework` targets to C# 7.3. C# 14 remains the current stable .NET 10 C# signal; C# 15 remains preview on .NET 11 preview with collection expression arguments and union types as directional signals only.
- Confirmed TypeScript 6.0 remains the transition release toward the TypeScript 7.0 native compiler, while TypeScript 7.0 Beta remains a side-by-side `@typescript/native-preview`/`tsgo` tooling signal rather than a TypeSharp runtime or syntax dependency.
- Confirmed F# 10 remains a refinement/tooling signal for diagnostics, task workflows, and deterministic compiler work without adding an F# compiler/runtime dependency.
- Confirmed `.NET Framework 4.8.1` remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a qualified-profile backlog item.
- Confirmed the current test-package answer still holds: TypeSharp already uses pinned `MSTest.Sdk/4.2.3` with .NET 10 MTP mode and package-based shards; xUnit.net v3 remains a future bridge candidate but would duplicate current package-host coverage now.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or generated-artifact baselines. The Windows Server 2025 + Visual Studio 2026 `windows-latest` migration beginning June 2026 remains a CI watch item.
- Selected the next bounded implementation slice: `0388 Shift assignment expressions`. The slice should implement `<<=` and `>>=` over the existing assignment surface, reuse the primitive integral shift count/result policy, preserve generated C# 7.3 `net48` lowering, and keep logical unsigned `>>>=`, user-defined operators, enum flag algebra, imported operator overload resolution, and broad assignment target analysis out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0388: shift assignment expressions.

## Task 0388 Shift Assignment Expressions

Completed on 2026-05-22.

Summary:

- Added lexer/parser support for `<<=` and `>>=` as assignment operators while keeping plain `<<`/`>>` composition and shift parsing on the existing two-token path.
- Added type-checker validation for mutable local primitive integral shift assignments, reusing the non-null primitive integral target and int-compatible count policy from task 0386.
- Preserved immutable local diagnostics, invalid assignment target diagnostics, and existing imported C# member/indexer/event assignment validation behavior.
- Lowered accepted shift assignments to ordinary generated C# `<<=` and `>>=` operators and added backend snapshot coverage.
- Added parser, positive/negative type-checker, backend, generated `net48` C# consumer, catalog-count, MSTest bridge, docs, and traceability evidence.
- Updated the shared catalog to 524 cases with four-shard expected counts of `131`, `131`, `131`, and `131`.
- Kept logical unsigned `>>>=`, logical unsigned shift assignment, user-defined operators, enum flag algebra, imported operator overload resolution, and broad assignment target analysis out of scope.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "CLI build compiles shift assignment expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "MSTest package shard bridge projects are stable"
dotnet build test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard1\TypeSharp.Compiler.Tests.Shard1.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard2\TypeSharp.Compiler.Tests.Shard2.csproj --nologo --verbosity quiet
dotnet build test\TypeSharp.Compiler.Tests.Shard3\TypeSharp.Compiler.Tests.Shard3.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests.Shard0\TypeSharp.Compiler.Tests.Shard0.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard1\TypeSharp.Compiler.Tests.Shard1.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard2\TypeSharp.Compiler.Tests.Shard2.csproj --no-build
dotnet run --project test\TypeSharp.Compiler.Tests.Shard3\TypeSharp.Compiler.Tests.Shard3.csproj --no-build
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: compiler build, focused harness filters, all four package-free shard runners, MSTest bridge build/smoke, docs build, and diff check succeeded on 2026-05-22. The docs build emitted the existing Vite chunk-size warning, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/parser/positive/0039-shift-assignment-expression`
- `test/fixtures/diagnostics/type-checker/positive/shift-assignment-expression`
- `test/fixtures/diagnostics/type-checker/negative/shift-assignment-invalid`
- `test/fixtures/backend/csharp/positive/0051-shift-assignment-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/README.md`
- `.github/workflows/regression.yml`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0389: roadmap refresh after shift assignment expressions.

## Task 0389 Roadmap Refresh After Shift Assignment Expressions

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after bounded `<<=`/`>>=` shift assignments landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed C# language versioning still maps `.NET Framework` targets to C# 7.3. C# 14 remains the current stable .NET 10 C# signal; C# 15 remains preview on .NET 11 preview with collection expression arguments and union types as directional signals only.
- Confirmed C# documents `>>>` and `>>>=`, but TypeSharp cannot emit C# `>>>` in generated source while preserving the C# 7.3 backend baseline.
- Confirmed TypeScript 6.0 remains the bridge toward the TypeScript 7.0 native compiler, while TypeScript 7.0 Beta remains a side-by-side `@typescript/native-preview`/`tsgo` tooling signal rather than a TypeSharp runtime or syntax dependency.
- Confirmed F# 10 remains a refinement/tooling signal for diagnostics, task workflows, and deterministic compiler work without adding an F# compiler/runtime dependency.
- Confirmed `.NET Framework 4.8.1` remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a qualified-profile backlog item.
- Confirmed the current test-package answer still holds: TypeSharp already uses pinned `MSTest.Sdk/4.2.3` with .NET 10 MTP mode and package-based shards; xUnit.net v3 remains a future bridge candidate but would duplicate current package-host coverage now.
- Confirmed VS Code LSP guidance and GitHub Actions runner-image signals do not change current editor or generated-artifact baselines. The Windows Server 2025 + Visual Studio 2026 `windows-latest` migration beginning June 2026 remains a CI watch item.
- Selected the next bounded implementation slice: `0390 Logical unsigned shift expressions`. The slice should implement expression-only `>>>` over known non-null primitive integral operands, reuse the existing shift count/result policy, lower signed operands with explicit unsigned casts in C# 7.3-compatible generated source, and keep `>>>=`, user-defined operators, enum flag algebra, imported operator overload resolution, broad assignment target analysis, and composition semantics changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0390: logical unsigned shift expressions.

## Task 0390 Logical Unsigned Shift Expressions

Completed on 2026-05-22.

Summary:

- Added parser support for expression-level `>>>` by grouping three adjacent `GreaterToken` nodes before the existing two-token `>>` composition/shift parsing path, preserving `>>`, `<<`, `<<=`, and `>>=` behavior.
- Added type-checker and inference support for known non-null primitive integral `left >>> count` operands using the existing shift count/result policy: small left operands promote to `int`, and `int`, `uint`, `long`, and `ulong` keep the left shape.
- Added deterministic `TS2201` diagnostics for nullable, non-integral, enum, record, unsupported-count, and composition-shaped `>>>` operands before backend emission.
- Added C# 7.3-compatible lowering for signed logical unsigned shifts through explicit unchecked unsigned casts plus ordinary `>>`, while unsigned `uint`/`ulong` operands continue to emit ordinary `>>`.
- Added parser, type-checker, backend, and generated `net48` C# consumer evidence, bringing the shared catalog to 525 cases with four-shard expected counts of `132`, `131`, `131`, and `131`.
- Kept `>>>=`, user-defined operators, enum flag algebra, imported operator overload resolution, broad assignment target analysis, and any change to existing function composition semantics out of scope.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles logical unsigned shift expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: all commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpInferenceEngine.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `test/fixtures/parser/positive/0040-logical-unsigned-shift-expression`
- `test/fixtures/diagnostics/type-checker/positive/logical-unsigned-shift-expression`
- `test/fixtures/diagnostics/type-checker/negative/logical-unsigned-shift-invalid`
- `test/fixtures/backend/csharp/positive/0052-logical-unsigned-shift-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/README.md`
- `.github/workflows/regression.yml`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0391: roadmap refresh after logical unsigned shift expressions.

## Task 0391 Roadmap Refresh After Logical Unsigned Shift Expressions

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after bounded expression-level `>>>` landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed C# language versioning still maps `.NET Framework` targets to C# 7.3. C# 14 remains the current stable .NET 10 C# signal, and C# 15 remains preview on .NET 11 preview with collection expression arguments and union types as directional signals only.
- Confirmed C# documents stable `>>>` and `>>>=` bitwise/shift forms, including compound assignment, but TypeSharp must keep generated C# free of those tokens under the C# 7.3 backend baseline.
- Confirmed TypeScript 6.0 remains the latest stable TypeScript release and TypeScript 7.0 Beta remains a native/compiler tooling transition signal; its parallel type-checking and deterministic type-ordering work inform TypeSharp's deterministic diagnostics and parallel-work policy without changing TypeSharp syntax or runtime targets.
- Confirmed F# 10 remains a .NET 10 refinement/tooling signal for diagnostics, task workflows, functional consistency, and performance without adding an F# compiler/runtime dependency.
- Confirmed `.NET Framework 4.8.1` remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a qualified-profile backlog item.
- Confirmed the test-host policy still holds: pinned `MSTest.Sdk/4.2.3` with .NET 10 MTP mode and package shards remains the selected package bridge, while xUnit.net v3 remains a future bridge candidate over the same catalog.
- Confirmed VS Code LSP/Marketplace guidance and GitHub Actions runner-image/setup-action signals do not change current editor, CI, or generated-artifact baselines. The June 2026 Windows Server 2025 + Visual Studio 2026 image migration remains a CI watch item.
- Selected the next bounded implementation slice: `0392 Logical unsigned shift assignment expressions`. The slice should implement `>>>=` over supported primitive integral mutation surfaces, lower through C# 7.3-compatible explicit assignment/cast forms instead of emitted `>>>`/`>>>=`, and keep user-defined operators, enum flag algebra, imported operator overload resolution, and broad assignment target analysis out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [`dotnet restore` audit sources](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code extension publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0392: logical unsigned shift assignment expressions.

## Task 0392 Logical Unsigned Shift Assignment Expressions

Completed on 2026-05-22.

Summary:

- Added lexer/parser support for `>>>=` as a single assignment operator without perturbing existing `>>>`, `>>=`, `>>`, `<<`, or function-composition parsing.
- Added type-checker support for mutable local primitive integral `target >>>= count` with the existing non-null shift count policy, accepting `byte`, `sbyte`, `short`, `ushort`, and `int` counts.
- Added deterministic `TS2201` diagnostics for immutable targets, invalid targets, nullable, non-integral, enum, record, unsupported-count, and imported/member/indexer/event `>>>=` targets that still need a single-evaluation lowering policy.
- Added C# 7.3-compatible lowering for accepted local `>>>=` forms through explicit assignment/cast shapes, preserving generated `net48` source with no emitted `>>>` or `>>>=`.
- Taught backend local type tracking to infer unannotated local declaration types from known initializers so assignment lowering can preserve `long`, small integral, and unsigned target shapes.
- Added parser, type-checker, backend, and generated `net48` C# consumer evidence, bringing the shared catalog to 526 cases with four-shard expected counts of `132`, `132`, `131`, and `131`.
- Kept user-defined operators, enum flag algebra, imported operator overload resolution, non-local `>>>=` single-evaluation lowering, and broad assignment target analysis out of scope.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles logical unsigned shift assignment expression API"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
npm run build          # in docs
git diff --check
```

Result: all commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/fixtures/parser/positive/0041-logical-unsigned-shift-assignment-expression`
- `test/fixtures/diagnostics/type-checker/positive/logical-unsigned-shift-assignment-expression`
- `test/fixtures/diagnostics/type-checker/negative/logical-unsigned-shift-assignment-invalid`
- `test/fixtures/backend/csharp/positive/0053-logical-unsigned-shift-assignment-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/README.md`
- `.github/workflows/regression.yml`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0393: roadmap refresh after logical unsigned shift assignment expressions.

## Task 0393 Roadmap Refresh After Logical Unsigned Shift Assignment Expressions

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after bounded local `>>>=` landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed C# language versioning still maps `.NET Framework` targets to C# 7.3. C# 14 remains the current stable .NET 10 C# signal, and C# 15 remains preview on .NET 11 preview with collection expression arguments and union types as directional signals only.
- Confirmed C# documents `>>>` and `>>>=` in the bitwise/shift operator set, and documents compound assignment as `x = x op y` except that the left operand is evaluated once. The next `>>>=` expansion therefore needs an explicit single-evaluation receiver lowering policy instead of merely re-emitting the receiver twice.
- Confirmed TypeScript 6.0 remains the latest stable TypeScript release and TypeScript 7.0 Beta remains a native/compiler tooling transition signal; its parallelization and deterministic behavior work remain compiler-engineering input only.
- Confirmed F# 10 remains a .NET 10 refinement/tooling signal for diagnostics, task workflows, functional consistency, and performance without adding an F# compiler/runtime dependency.
- Confirmed `.NET Framework 4.8.1` remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a qualified-profile backlog item.
- Confirmed the test-package answer still holds: TypeSharp already uses pinned `MSTest.Sdk/4.2.3`, .NET 10 Microsoft Testing Platform mode, locked restore/source mapping/audit controls, and four package-based MSTest shard projects over the shared catalog. Adding xUnit.net v3 now would duplicate test-host evidence instead of improving generated `net48` compatibility.
- Confirmed VS Code LSP/Marketplace guidance and GitHub Actions runner-image/setup-action signals do not change current editor, CI, or generated-artifact baselines. The June 2026 Windows Server 2025 + Visual Studio 2026 image migration remains a CI watch item.
- Selected the next bounded implementation slice: `0394 Logical unsigned shift assignment imported member targets`. The slice should implement supported metadata-backed imported C# member access `>>>=` with C# 7.3-compatible explicit assignment/cast lowering and single-evaluation receiver behavior, while keeping indexers, events, user-defined operators, enum flag algebra, imported operator overload resolution, broad assignment target analysis, and test-framework package changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [C# assignment operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/assignment-operator)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [`dotnet restore` audit sources](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [VS Code language server extensions](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code extension publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)

Remaining:

- Completed in task 0394: logical unsigned shift assignment imported member targets.

## Task 0394 Logical Unsigned Shift Assignment Imported Member Targets

Completed on 2026-05-22.

Summary:

- Added bounded checker support for imported C# `target.member >>>= count` when the member resolves to a readable/writable metadata-backed instance/static field or property, the member type is a known non-null primitive integral value, and the count is `byte`, `sbyte`, `short`, `ushort`, or `int`.
- Kept indexer, event, unresolved member, unsupported count, nullable, non-integral, enum, record, user-defined operator, imported operator overload, TypeSharp member assignment, and broad class-member assignment cases as deterministic diagnostics before emission.
- Extended metadata-backed member access inference so imported field/property reads expose normalized primitive member types to the checker and C# backend.
- Added C# 7.3-compatible imported member `>>>=` lowering with explicit unchecked unsigned casts for signed/small integral targets, ordinary `>>` for unsigned targets, no emitted `>>>` or `>>>=`, and single-evaluation lowering for non-trivial receivers through a generated `System.Func<TReceiver,TMember>` expression.
- Threaded metadata assemblies into the C# source backend during CLI build emission so generated-source lowering can choose imported member target/receiver types without changing the generated `net48` baseline.
- Expanded the legacy metadata fixture and added shared catalog coverage for imported instance property, instance field, static unsigned property, static byte field, non-trivial receiver, unsupported count, indexer, and event cases, bringing the shared catalog to 528 cases with four-shard expected counts of `132`, `132`, `132`, and `132`.
- Updated Feature Status, Type System, Lowering, Work Ledger, queue, and traceability docs to record the implemented boundary and remaining follow-ups.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "imported logical unsigned shift assignment"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet test test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress
npm run build          # in docs
git diff --check
```

Result: all commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackendAdapter.cs`
- `lang/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- `test/README.md`
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0395: roadmap refresh after logical unsigned shift assignment imported member targets.

## Task 0395 Roadmap Refresh After Logical Unsigned Shift Assignment Imported Member Targets

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after imported C# field/property `>>>=` landed.
- Confirmed no generated-artifact baseline change: TypeSharp-generated assemblies stay `net48`, generated source stays C# 7.3-compatible, `TypeSharp.Core` and `TypeSharp.Runtime` stay package-free, and modern .NET/Node/package dependencies remain isolated to compiler, test, docs, CI, or editor tooling.
- Confirmed C# language versioning still maps all `.NET Framework` targets to C# 7.3. C# 14 remains the current stable .NET 10 C# signal, and C# 15 remains preview on .NET 11 preview.
- Confirmed C# documents compound assignment as equivalent to `x = x op y` except the left side is evaluated once, and documents bitwise/shift operators over integral operands. Imported indexer `>>>=` therefore needs an explicit single-evaluation receiver/index-argument lowering policy.
- Confirmed TypeScript 6.0 remains the stable TypeScript release and TypeScript 7.0 Beta remains a native/compiler tooling transition signal; F# 10 remains a .NET 10 refinement/tooling signal and adds no runtime dependency.
- Confirmed `.NET Framework 4.8.1` remains the latest Framework, while `net48` remains TypeSharp's broad generated target and `net481` stays a qualified-profile backlog item.
- Confirmed the NuGet test-package answer still holds: TypeSharp already uses pinned `MSTest.Sdk/4.2.3`, .NET 10 Microsoft Testing Platform mode, locked restore/source mapping/audit controls, and four package-based MSTest shard projects over the shared catalog. Adding xUnit.net v3 now would duplicate test-host evidence instead of improving generated `net48` compatibility.
- Confirmed VS Code publishing/LSP guidance and GitHub Actions runner-image/setup-action signals do not change current editor, CI, or generated-artifact baselines. The June 2026 Windows Server 2025 + Visual Studio 2026 image migration remains a CI watch item.
- Selected the next bounded implementation slice: `0396 Logical unsigned shift assignment imported indexer targets`. The slice should implement supported metadata-backed imported C# indexer `>>>=` with C# 7.3-compatible explicit assignment/cast lowering and single-evaluation receiver/index-argument behavior, while keeping events, user-defined operators, enum flag algebra, imported operator overload resolution, TypeSharp member assignment policy, broad assignment target analysis, and test-framework package changes out of scope.

Verification:

```powershell
npm run build          # in docs
git diff --check
```

Result: both commands succeeded on 2026-05-22; the docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted line-ending warnings only.

Primary evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)
- [C# assignment operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/assignment-operator)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [Target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [.NET `dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [Run tests with MSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests)
- [NuGet MSTest 4.2.3](https://www.nuget.org/packages/MSTest/4.2.3)
- [xUnit.net v3 package guidance](https://xunit.net/docs/nuget-packages-v3)
- [xUnit.net v3 MTP guidance](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [VS Code extension publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [GitHub Actions runner images](https://github.com/actions/runner-images)
- [GitHub Actions image migrations](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0396: logical unsigned shift assignment imported indexer targets.

## Task 0396 Logical Unsigned Shift Assignment Imported Indexer Targets

Completed on 2026-05-22.

Summary:

- Added checker support for metadata-backed imported C# `target[index] >>>= count` when overload resolution selects a public instance indexer with getter/setter, the indexer value type is a known non-null primitive integral value, and the count is `byte`, `sbyte`, `short`, `ushort`, or `int`.
- Reused imported indexer validation/ranking boundaries for mismatched and ambiguous argument diagnostics while keeping missing setter, unsupported count, nullable, non-integral, enum, record, event, and unresolved targets deterministic before emission.
- Added C# 7.3-compatible backend lowering with no generated `>>>` or `>>>=`; simple receiver/index arguments lower as ordinary indexer assignments, and non-trivial receiver or index arguments lower through generated `System.Func<...>` expressions so the receiver and index arguments are evaluated once.
- Added the `LegacyMutableIndexer` fixture, metadata assertions, positive/negative catalog cases, generated `net48` C# consumer evidence, shared catalog count 530, and shard counts 133/133/132/132.
- Updated Type System, Lowering, Reference, Diagnostics, Feature Status, Work Ledger, task queue, and traceability docs.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "imported logical unsigned shift assignment indexer"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "imported logical unsigned shift assignment"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet test test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress
npm run build
git diff --check
```

Result: commands succeeded on 2026-05-22. The docs build emitted the existing Vite chunk-size warning only, and `git diff --check` emitted Git line-ending warnings only.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Reference](../docs/src/content/docs/reference.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Completed in task 0397: roadmap refresh after logical unsigned shift assignment imported indexer targets.

## Task 0397 Roadmap Refresh After Logical Unsigned Shift Assignment Imported Indexer Targets

Completed on 2026-05-22.

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions source signals after metadata-backed imported C# indexer `>>>=` landed.
- Reaffirmed the generated artifact baseline: generated assemblies, `TypeSharp.Core`, and `TypeSharp.Runtime` remain package-free `net48`; generated C# remains C# 7.3-compatible because Microsoft C# language versioning still maps all .NET Framework targets to C# 7.3.
- Reaffirmed platform signals: C# 14 is the current stable .NET 10 C# signal, C# 15 remains .NET 11 preview-only, F# 10 is a tooling/design signal only, TypeScript 6.0/7.0 are editor/compiler architecture signals only, and .NET Framework 4.8.1 being latest does not replace the broad `net48` profile.
- Answered the `net10.0` NuGet test-package question explicitly: TypeSharp already uses NuGet packages at the test-host boundary through pinned `MSTest.Sdk/4.2.3`, Microsoft Testing Platform, package lock files, source mapping, audit controls, repo-local package cache, and package-based shard projects over `TypeSharpCompilerTestCases.All`. It avoids NuGet only for generated `net48` artifacts, where package-free compatibility is a core project constraint.
- Kept xUnit.net v3 as a future bridge candidate. Its native Microsoft Testing Platform support is valid, but adding it now would duplicate the existing `net10.0` MSTest SDK/MTP shard evidence without improving generated artifact compatibility or the current release-confidence path.
- Selected task 0398, null-conditional assignment imported member targets, because C# 14 null-conditional assignment is stable and the current TypeSharp docs reserve `?.` while the lexer/parser do not yet implement it.

Official source evidence:

- [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning)
- [What's new in C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14)
- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [What's new in F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10)
- [Announcing TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/)
- [Announcing TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/)
- [.NET target frameworks](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
- [.NET Framework versions and dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies)
- [.NET Framework support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-framework)
- [NuGet PackageReference lock files](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)
- [NuGet package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping)
- [NuGet restore audit](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore#audit-for-security-vulnerabilities)
- [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview)
- [`dotnet test` MTP mode](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk)
- [NuGet MSTest.Sdk](https://www.nuget.org/packages/MSTest.Sdk)
- [xUnit.net v3 Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
- [VS Code language server extension guide](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide)
- [VS Code extension publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension)
- [GitHub Actions image migration changelog](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [actions/setup-node](https://github.com/actions/setup-node)

Verification:

```powershell
npm run build
git diff --check
```

Result: docs build succeeded on 2026-05-22 with the existing Vite chunk-size warning only, and `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Active in task 0398: null-conditional assignment imported member targets.

## Task 0398 Null-conditional assignment imported member targets

Status: Done
Queue: Q2
Completed: 2026-05-22

Summary:

- Added `?.` token/syntax support with a parser fixture for the bounded null-conditional member assignment target shape.
- Implemented checker support for simple `receiver?.Member = value` when `receiver` is a nullable or reference-like metadata-backed imported C# instance receiver and `Member` resolves to a writable public imported C# field or property.
- Rejected null-conditional compound assignment, static targets, events, readonly/unwritable members, indexers, local binding targets, TypeSharp-owned members, and null-conditional reads with deterministic diagnostics before backend emission.
- Lowered accepted assignments to C# 7.3-compatible `System.Func<TReceiver,TMember>` null guards, preserving single receiver evaluation and skipping right-side evaluation when the receiver is null, without emitted `?.` syntax or generated package dependencies.
- Added generated `net48` C# consumer evidence, updated the shared custom/MSTest catalog count to 532, and set package-based shard expectations to `133, 133, 133, 133`.
- Updated Grammar, Type System, Lowering, Diagnostics, Feature Status, .NET Interop, Work Ledger, test README, regression workflow counts, and traceability.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles null-conditional assignment imported member targets"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "checker rejects unsupported null-conditional assignment imported member targets"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress
dotnet restore/build/test over test\TypeSharp.Compiler.Tests.MSTest.Shard0-3 with --locked-mode restore, --no-restore build, --no-build CatalogCase runs, and minimum expected tests 133 per shard
npm run build
git diff --check
```

Result: compiler test host build passed; focused parser/generated-consumer/negative-checker/shard-count tests passed; full 532-case package-free catalog passed; MSTest package bridge smoke passed; all four MSTest package shard bridge `dotnet test` runs passed with 133 tests per shard; docs build succeeded with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpLexer.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/fixtures/parser/positive/0042-null-conditional-assignment-expression`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Next ready task 0399 should recheck official language/platform/package/test/editor/CI signals after null-conditional assignment landed and select the next bounded TypeSharp implementation slice.

## Task 0399 Roadmap Refresh After Null-conditional Assignment Imported Member Targets

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official C#, F#, TypeScript, .NET Framework, NuGet, .NET testing, MSTest SDK, xUnit.net v3, VS Code, and GitHub Actions sources after Task 0398 landed `receiver?.Member = value`.
- Confirmed no generated-artifact baseline change: TypeSharp generated artifacts remain package-free `net48`, generated C# remains C# 7.3-compatible, and .NET Framework targets still default to C# 7.3.
- Confirmed no test-host package change: `MSTest.Sdk/4.2.3` remains current on NuGet, is still the selected `net10.0` Microsoft Testing Platform bridge, and xUnit.net v3 remains a future bridge candidate rather than a necessary duplicate now.
- Confirmed external preview boundaries: C# 15 collection expression arguments and union types stay Preview Watch, TypeScript 7.0 Beta remains a native/compiler transition signal, and F# 10 remains a refinement/tooling signal without an FSharp.Core dependency for TypeSharp generated artifacts.
- Selected Task 0400 as the next bounded implementation slice: metadata-backed imported C# instance indexer null-conditional assignment `receiver?[index] = value`, preserving skipped right-side evaluation on null receivers, single receiver/index evaluation, C# 7.3-compatible generated `net48` source, and no generated package/runtime baseline change.
- Updated Feature Status, Project Policy, Work Ledger, tasks.md, and traceability.

Official sources reviewed:

- Microsoft Learn C# language versioning, C# 14, and C# 15 pages.
- TypeScript 6.0 release notes, TypeScript 6.0 announcement, TypeScript 7.0 Beta announcement, and TypeScript modules reference.
- Microsoft Learn F# 10, F# strategy, computation expressions, and task expressions pages.
- Microsoft Learn target-framework monikers, .NET Framework versions/dependencies, and .NET Framework support policy.
- Microsoft Learn .NET test platforms and `dotnet test` MTP mode pages.
- NuGet `MSTest.Sdk/4.2.3`, NuGet `MSTest/4.2.3`, and xUnit.net v3 package guidance.
- VS Code LSP and extension publishing docs.
- GitHub Actions runner images and the 2026-05-14 image migration changelog.

Verification:

```powershell
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity minimal
cd docs
npm run build
git diff --check
```

Result: MSTest SDK/MTP bridge smoke passed with one discovered test; docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0400 should implement imported C# instance indexer null-conditional assignment `receiver?[index] = value`; compound assignment, null-conditional reads, events, static targets, TypeSharp-owned targets, and package/runtime baseline changes remain out of scope.

## Task 0400 Null-conditional Assignment Imported Indexer Targets

Status: Done
Queue: Q2
Completed: 2026-05-22

Summary:

- Added parser support for postfix null-conditional indexer syntax `receiver?[index]`, represented as `NullConditionalIndexerExpression`, plus parser fixture coverage for `receiver?[index] = value`.
- Added checker support for simple `=` assignments to writable metadata-backed imported C# instance indexer targets whose selected public indexer has getter/setter metadata and supported argument types.
- Rejected null-conditional indexer reads, compound assignment, missing-setter/mismatched indexers, static/type targets, TypeSharp-owned targets, and other unsupported forms before backend emission.
- Lowered accepted assignments to C# 7.3-compatible nested `System.Func` guards: the receiver is evaluated once, index arguments and RHS are evaluated only after the receiver is non-null, and generated `net48` C# emits no `?[]`.
- Added generated `net48` C# consumer evidence, updated the shared custom/MSTest catalog count to 534, and set package-based shard expectations to `134, 134, 133, 133`.
- Updated Grammar, Type System, Lowering, Diagnostics, Feature Status, .NET Interop, Work Ledger, test README, regression workflow counts, and traceability.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "null-conditional assignment imported indexer"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress
dotnet restore/build/test over test\TypeSharp.Compiler.Tests.MSTest.Shard0-3 with --locked-mode restore, --no-restore build, --no-build CatalogCase runs, and minimum expected tests 134, 134, 133, 133
npm run build
git diff --check
```

Result: compiler test host build passed; focused parser/generated-consumer/negative-checker/shard-count tests passed; full 534-case package-free catalog passed; MSTest package bridge smoke passed; all four MSTest package shard bridge `dotnet test` runs passed with 134, 134, 133, and 133 tests; docs build succeeded with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/SyntaxKind.cs`
- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `lang/TypeSharp.Compiler/Interop/TypeSharpInteropValidator.cs`
- `test/fixtures/parser/positive/0043-null-conditional-indexer-assignment-expression`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Next ready task 0401 should address the user-requested GitHub Actions regression where the VS Code live smoke cannot start `npm`; the post-0400 roadmap refresh remains queued after that as Task 0402.

## Task 0402 Roadmap Refresh After Null-conditional Assignment Imported Indexer Targets

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official language, platform, package, test-platform, editor, and CI sources after Task 0400 completed imported C# instance indexer null-conditional assignment.
- Confirmed no baseline change: generated artifacts remain package-free `net48`, generated C# remains C# 7.3-compatible, C# 14/.NET 10 is the stable signal, and C# 15/.NET 11 preview remains Preview Watch input only.
- Reconfirmed the NuGet test-host answer: TypeSharp already uses the current broad `net10.0` package path through pinned `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge projects and package shards; adding xUnit.net v3 now would duplicate package-host evidence rather than improve generated-artifact compatibility.
- Rechecked the GitHub Actions regression from run 26260793703: `Setup Node` succeeds, but the VS Code live smoke cannot start `npm` from C# with `UseShellExecute=false`. Because the `gh-fix-ci` workflow requires explicit user approval before implementation, Task 0401 remains Blocked pending approval for the likely Windows `cmd.exe /d /s /c npm ...` helper fix.
- Selected Task 0403 as the next ready bounded slice: define and implement C# 14 extension member policy around TypeSharp extension properties/static members, C# 7.3-compatible lowering, diagnostics, and fixtures over the existing explicit-receiver extension method MVP.

Official sources reviewed:

- Microsoft Learn C# language versioning, C# 14, and C# 15 pages.
- Microsoft Learn target-framework monikers, .NET Framework versions/dependencies, and .NET Framework support policy.
- TypeScript 6.0 and TypeScript 7.0 Beta announcements.
- Microsoft Learn .NET test platforms, `dotnet test` MTP mode, MSTest runner guidance, and MSTest SDK configuration pages.
- NuGet `MSTest.Sdk/4.2.3`, NuGet `MSTest/4.2.3`, and xUnit.net v3 Microsoft Testing Platform guidance.
- VS Code LSP and extension publishing docs.
- GitHub Actions runner images, `actions/setup-dotnet`, `actions/setup-node`, and the 2026-05-14 image migration changelog.

Verification:

```powershell
gh run view 26260793703 --json status,conclusion,url,jobs
gh run view 26260793703 --log-failed
cd docs
npm run build
git diff --check
```

Result: GitHub Actions failure context confirmed the `npm` process-launch failure rather than a missing setup-node step; docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0403 completed the extension member policy/implementation slice; Task 0404 should perform the post-implementation roadmap refresh next.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0403 C# 14 Extension Member Policy And Bounded Implementation Slice

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Added declaration receiver names for explicit-receiver `extension` declarations and bound that receiver name for extension property initializers.
- Added getter-only TypeSharp-authored extension property support in the parser, binder, checker, and C# backend for `extension ReceiverType receiverName { public let Name: Type = expr }`.
- Enforced the bounded policy deterministically: extension properties require a receiver name, cannot be `let mut`, require an explicit type annotation, require an initializer expression, and reject accessor blocks in this slice.
- Added exact known non-null receiver member access for TypeSharp-authored extension properties after ordinary imported/instance members and structural shape members.
- Lowered accepted properties to C# 7.3-compatible helper methods such as `GetWordCount(this string text)` and lowered `value.WordCount` to `StringExtensions.GetWordCount(value)` instead of emitting C# 14 extension blocks.
- Added parser, negative type-checker, backend, generated `net48` C# consumer, catalog-count, MSTest bridge, docs, and traceability evidence.
- Updated the shared catalog to 535 cases with four-shard expected counts of `134`, `134`, `134`, and `133`.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "parser fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "C# backend fixture snapshots match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "CLI build compiles extension property lowering"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --no-progress
dotnet build/test over test\TypeSharp.Compiler.Tests.MSTest.Shard0-3 with --no-build CatalogCase runs and minimum expected tests 134, 134, 134, 133
cd docs
npm run build
git diff --check
```

Result: compiler test host build passed; focused parser/type-checker/backend/generated-consumer/shard-count tests passed; full 535-case package-free catalog passed; MSTest package bridge build/smoke passed; all four MSTest package shard bridge `dotnet test` runs passed with 134, 134, 134, and 133 tests; docs build succeeded with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/Parsing/TypeSharpParser.cs`
- `lang/TypeSharp.Compiler/Binding/TypeSharpBinder.cs`
- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `lang/TypeSharp.Compiler/Backend/CSharpSourceBackend.cs`
- `test/fixtures/parser/positive/0044-extension-property-declaration`
- `test/fixtures/diagnostics/type-checker/negative/extension-property-policy`
- `test/fixtures/backend/csharp/positive/0054-extension-property-lowering`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0404 should perform the post-implementation roadmap refresh and select the next bounded slice.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0404 Roadmap Refresh After C# 14-inspired Extension Property Slice

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official language, platform, package, test-platform, editor, and CI sources after Task 0403 landed getter-only TypeSharp-authored extension properties.
- Confirmed no generated-artifact baseline change: generated artifacts remain package-free `net48`, generated C# remains C# 7.3-compatible, C# 14 remains stable on .NET 10, and C# 15/.NET 11 preview stays Preview Watch input only.
- Reaffirmed the NuGet test-host answer requested by the user: TypeSharp already uses the current broad `net10.0` package route through pinned `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge projects, four package-based shards, lock files, source mapping, audit controls, and repo-local package cache. Generated `net48` artifacts, `TypeSharp.Core`, and `TypeSharp.Runtime` remain package-free.
- Confirmed xUnit.net v3 remains a viable future bridge candidate, but adding it now would duplicate the existing `MSTest.Sdk`/MTP package-host evidence unless it provides distinct ecosystem value over the same extracted catalog.
- Left Task 0401 blocked pending explicit approval for the GitHub Actions `npm` process-launch implementation fix; the failure remains a C# process-launch issue rather than a missing setup-node/setup-dotnet step or missing NuGet test package.
- Selected Task 0405 as the next bounded implementation slice: extension-property duplicate/conflict diagnostics over the current exact receiver matching and instance/structural precedence, before static extension members, setters, operators, nullable receiver lifting, or imported extension property metadata are added.

Official sources reviewed:

- Microsoft Learn C# language versioning, C# 14, and C# 15 pages.
- TypeScript 6.0 announcement and TypeScript 7.0 Beta announcement.
- Microsoft Learn F# 10 page.
- Microsoft Learn target-framework monikers, .NET Framework system requirements, and .NET Framework lifecycle pages.
- Microsoft Learn .NET test platforms, `dotnet test` MTP mode, MSTest runner guidance, and MSTest SDK configuration pages.
- NuGet `MSTest.Sdk/4.2.3` and xUnit.net v3 package guidance.
- VS Code LSP and extension publishing docs.
- GitHub Actions runner images, `actions/setup-dotnet`, and `actions/setup-node` docs.

Verification:

```powershell
cd docs
npm run build
git diff --check
```

Result: docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0405 should implement extension-property duplicate/conflict diagnostics.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0405 Extension Property Duplicate And Conflict Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Added deterministic type-checker collection diagnostics for duplicate TypeSharp-authored extension properties with the same exact non-null receiver type and property name.
- Added `TS2201` diagnostics when a TypeSharp-authored extension property would be hidden by currently implemented ordinary imported instance field/property lookup or structural/record shape member precedence.
- Tightened extension property registration and resolution to exact non-null receiver matches so declaration collection mirrors member-access behavior.
- Added focused negative fixture coverage for record-shape conflicts, structural-alias conflicts, and duplicate exact receiver/name declarations.
- Updated grammar, type-system, C# member/overload, diagnostics, feature-status, work-ledger, task, and traceability docs. The shared catalog count stayed 535 because the new fixture is covered by the existing type-checker fixture catalog case.
- Selected Task 0406 as the next ready bounded slice: roadmap refresh after extension-property duplicate/conflict diagnostics.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build --filter "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
cd docs
npm run build
git diff --check
```

Result: compiler build, focused type-checker fixture diagnostics, and the full 535-case compiler catalog passed. Docs build passed with the existing Vite chunk-size warning. `git diff --check` reported only Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/diagnostics/type-checker/negative/extension-property-conflicts`
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0406 should perform the post-implementation roadmap refresh and select the next bounded slice.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0406 Roadmap Refresh After Extension Property Duplicate/Conflict Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official language, platform, package, test-platform, editor, and CI sources after Task 0405 landed extension-property duplicate/conflict diagnostics.
- Confirmed no generated-artifact baseline change: generated artifacts remain package-free `net48`, generated C# remains C# 7.3-compatible, C# 14 remains stable on .NET 10, and C# 15/.NET 11 remains Preview Watch input only.
- Reaffirmed the NuGet test-host answer requested by the user: TypeSharp already uses the current broad `net10.0` package route through pinned `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge projects, four package-based shards, lock files, source mapping, audit controls, and repo-local package cache. Generated `net48` artifacts, `TypeSharp.Core`, and `TypeSharp.Runtime` remain package-free by policy.
- Confirmed xUnit.net v3 remains a viable future bridge candidate, but adding it now would duplicate the existing `MSTest.Sdk`/MTP package-host evidence unless it provides distinct ecosystem value over the same extracted catalog.
- Left Task 0401 blocked pending explicit approval for the GitHub Actions `npm` process-launch implementation fix; the failure remains a C# process-launch issue rather than a missing setup-node/setup-dotnet step or missing NuGet test package.
- Selected Task 0407 as the next bounded implementation slice: extension property helper-name collision diagnostics for generated helpers such as `GetName(this T receiver)` before static extension members, setters, operators, nullable receiver lifting, or imported extension property metadata expand the surface.

Official sources reviewed:

- Microsoft Learn C# language versioning, C# 14, and C# 15 pages.
- TypeScript 6.0 announcement and TypeScript 7.0 Beta announcement.
- Microsoft Learn F# 10 page.
- Microsoft Learn target-framework monikers and .NET Framework versions/dependencies pages.
- Microsoft Learn .NET test platforms, `dotnet test` MTP mode, MSTest runner guidance, and NuGet `MSTest.Sdk/4.2.3`.
- xUnit.net v3 package guidance and Microsoft Testing Platform guidance.
- VS Code language server and extension publishing docs.
- GitHub Actions runner images, `actions/setup-dotnet`, `actions/setup-node`, and the 2026-05-14 image migration changelog.

Verification:

```powershell
cd docs
npm run build
git diff --check
```

Result: docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0407 should implement extension property helper-name collision diagnostics.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0407 Extension Property Helper Name Collision Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Added deterministic type-checker diagnostics for getter-only TypeSharp-authored extension property helper names such as `GetWordCount` when they collide with TypeSharp-authored extension methods in the same emitted extension container.
- Added a defensive same-container generated-helper collision check for extension property helpers; the normal binder still reports duplicate property symbols first in ordinary source pipelines, and a direct checker regression locks the backend-prevention diagnostic.
- Added focused negative fixture coverage for extension-method/helper collisions and a catalog regression for generated-helper collisions.
- Updated the shared compiler/MSTest catalog count to 536 and package shard expectations to `134, 134, 134, 134`.
- Updated type-system, lowering, C# member/overload, diagnostics, feature-status, work-ledger, task, traceability, test README, and regression workflow docs without changing the generated `net48`/C# 7.3 baseline.
- Selected Task 0408 as the next ready bounded slice: roadmap refresh after extension-property helper-name collision diagnostics.

Verification:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj "checker reports extension property generated helper collision diagnostics"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj "type checker fixture diagnostics match"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj "test runner shard selection is stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj "MSTest package shard bridge projects are stable"
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
cd docs
npm run build
git diff --check
```

Result: compiler build passed; focused helper-collision, type-checker fixture, shard-count, and MSTest shard-bridge tests passed; full 536-case compiler catalog passed; MSTest bridge catalog exposure smoke passed; docs build passed with the existing Vite chunk-size warning; `git diff --check` reported only Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/diagnostics/type-checker/negative/extension-property-helper-name-collisions`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCases.cs`
- `test/TypeSharp.Compiler.Tests/TypeSharpCompilerTestCatalog.cs`
- `test/TypeSharp.Compiler.Tests.MSTest/TypeSharpCompilerMSTestCatalog.cs`
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0408 should perform the post-implementation roadmap refresh and select the next bounded slice.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0408 Roadmap Refresh After Extension Property Helper Name Collision Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official language, platform, package, test-platform, editor, and CI sources after Task 0407 landed extension-property helper-name collision diagnostics.
- Confirmed no generated-artifact baseline change: generated artifacts remain package-free `net48`, generated C# remains C# 7.3-compatible, C# 14 remains stable on .NET 10, and C# 15/.NET 11 remains Preview Watch input only.
- Reaffirmed the NuGet test-host answer requested by the user: TypeSharp already uses the current broad `net10.0` package route through pinned `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge projects, four package-based shards, lock files, source mapping, audit controls, and repo-local package cache. Generated `net48` artifacts, `TypeSharp.Core`, and `TypeSharp.Runtime` remain package-free by policy.
- Confirmed xUnit.net v3 remains a viable future bridge candidate, but adding it now would duplicate the existing `MSTest.Sdk`/MTP package-host evidence unless it provides distinct ecosystem value over the same extracted catalog.
- Left Task 0401 blocked pending explicit approval for the GitHub Actions `npm` process-launch implementation fix; the failure remains a C# process-launch issue rather than a missing setup-node/setup-dotnet step or missing NuGet test package.
- Selected Task 0409 as the next bounded implementation slice: nullable receiver diagnostics for getter-only TypeSharp-authored extension properties before nullable receiver lifting, static extension members, setters, operators, or imported extension property metadata expand the surface.

Official sources reviewed:

- Microsoft Learn C# language versioning, C# 14, and C# 15 pages.
- TypeScript 6.0 release notes and TypeScript 7.0 Beta announcement.
- Microsoft Learn F# 10 page and F# strategy.
- Microsoft Learn target-framework monikers, .NET Framework versions/dependencies, and .NET Framework support policy pages.
- Microsoft Learn .NET test platforms, `dotnet test` MTP mode, MSTest runner guidance, MSTest SDK configuration, and NuGet `MSTest.Sdk/4.2.3`.
- xUnit.net v3 package guidance and NuGet `xunit.v3`.
- VS Code language server and extension publishing docs.
- GitHub Actions runner images, `actions/setup-dotnet`, `actions/setup-node`, and the 2026-05-14 image migration changelog.

Verification:

```powershell
cd docs
npm run build
git diff --check
```

Result: docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0409 should implement nullable receiver diagnostics for getter-only TypeSharp-authored extension properties.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0409 Extension Property Nullable Receiver Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Added deterministic `TS2201` diagnostics for getter-only TypeSharp-authored extension properties declared on nullable receivers such as `string?` or `int?`.
- Preserved the current exact known non-null receiver policy: nullable receiver declarations now fail before backend emission, while accepted non-null extension properties still collect and lower through C# 7.3-compatible `GetName(this T receiver)` helper methods.
- Added focused negative fixture coverage under `test/fixtures/diagnostics/type-checker/negative/extension-property-nullable-receiver`.
- Kept existing extension-property helper-name collision diagnostics and generated `net48` extension-property lowering behavior passing.
- Updated canonical docs and ledgers to describe the nullable receiver boundary.
- Selected Task 0410 as the next ready bounded slice: roadmap refresh after extension-property nullable receiver diagnostics.

Verification:

```powershell
dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj "type checker fixture diagnostics match"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj "checker reports extension property generated helper collision diagnostics"
dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj "CLI build compiles extension property lowering"
cd docs
npm run build
git diff --check
```

Result: compiler build and focused tests passed; docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- `lang/TypeSharp.Compiler/TypeChecking/TypeSharpTypeChecker.cs`
- `test/fixtures/diagnostics/type-checker/negative/extension-property-nullable-receiver`
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0410 completed the post-implementation roadmap refresh; Task 0411 is the active bounded implementation slice.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

## Task 0410 Roadmap Refresh After Extension Property Nullable Receiver Diagnostics

Status: Done
Queue: Q1
Completed: 2026-05-22

Summary:

- Rechecked official language, platform, package, test-platform, editor, and CI signals after nullable receiver diagnostics.
- Confirmed no TypeSharp baseline drift: generated artifacts stay package-free `net48`, generated C# stays C# 7.3-compatible, and C# 14 remains the stable .NET 10 C# signal while C# 15 remains a .NET 11 preview signal.
- Reaffirmed the existing `net10.0` `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge and four package shard projects as the current NuGet test-host path. TypeSharp is using NuGet packages at the test-host boundary; generated `net48` artifacts intentionally remain package-free.
- Kept xUnit.net v3 as a future bridge candidate because adding it now would duplicate package-host evidence over the same extracted catalog instead of improving generated artifact compatibility.
- Kept Task 0401 blocked pending explicit approval for the GitHub Actions `npm` process-launch fix.
- Selected Task 0411 as the next bounded implementation slice: deterministic diagnostics when getter-only TypeSharp-authored extension properties are used as assignment targets, before extension setters expand the surface.

Official sources reviewed:

- Microsoft Learn [C# language versioning](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-versioning), [C# 14](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14), [C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15), and [extension declarations](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extension).
- TypeScript team [TypeScript 6.0](https://devblogs.microsoft.com/typescript/announcing-typescript-6-0/) and [TypeScript 7.0 Beta](https://devblogs.microsoft.com/typescript/announcing-typescript-7-0-beta/) announcements.
- Microsoft Learn [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview), [.NET 11 preview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview), [F# 10](https://learn.microsoft.com/en-us/dotnet/fsharp/whats-new/fsharp-10), [.NET Framework versions/dependencies](https://learn.microsoft.com/en-us/dotnet/framework/install/versions-and-dependencies), and [target framework monikers](https://learn.microsoft.com/en-us/dotnet/standard/frameworks).
- Microsoft Learn [.NET test platforms overview](https://learn.microsoft.com/en-us/dotnet/core/testing/test-platforms-overview), [`dotnet test`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test), [MSTest runner guidance](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-running-tests), [MSTest SDK configuration](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-sdk), NuGet [`MSTest.Sdk`](https://www.nuget.org/packages/MSTest.Sdk), and xUnit.net [v3 package](https://xunit.net/docs/nuget-packages-v3) and [MTP](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform) guidance.
- VS Code [language server extension](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide) and [extension publishing](https://code.visualstudio.com/api/working-with-extensions/publishing-extension) docs.
- GitHub Actions [`actions/runner-images`](https://github.com/actions/runner-images), [`actions/setup-dotnet`](https://github.com/actions/setup-dotnet), [`actions/setup-node`](https://github.com/actions/setup-node), and the [2026-05-14 image migration changelog](https://github.blog/changelog/2026-05-14-github-actions-upcoming-image-migrations/).

Verification:

```powershell
cd docs
npm run build
git diff --check
```

Result: docs build passed with the existing Vite chunk-size warning; `git diff --check` reported no whitespace errors beyond Git line-ending warnings.

Primary evidence:

- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [C# Members And Overloads](../docs/src/content/docs/csharp-members-overloads.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks.md](tasks.md)
- [traceability.md](traceability.md)

Remaining:

- Task 0411 should add deterministic diagnostics for assignment targets that resolve to getter-only TypeSharp-authored extension properties.
- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.

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

- Completed historical work through task 0400 and tasks 0402-0409 is compressed here.
- `agent/tasks.md` is the active task pointer.
- `agent/tasks-rollup.md` is the only completed task rollup file.

Remaining:

- Continue the next ready task from [tasks.md](tasks.md) when work resumes.
- Fold each future completed active task back into this file and remove its completed packet.

Blocked:

- Task 0401 GitHub Actions regression npm missing in VS Code live smoke is pending explicit user approval for the CI implementation fix.
