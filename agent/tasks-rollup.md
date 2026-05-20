# Task Rollup: Project Work Ledger

Status: Done
Queue: Q0-Q5
Start Time: 2026-05-20 02:17:44 +09:00
End Time: 2026-05-21 00:06:00 +09:00

## Objective

Keep one compact completed-work ledger for agent handoff without preserving every historical task packet as a separate file.

## Compression Rule

This rollup replaces individual completed task packet files for work 0001 through 0262. Future completed active packets should be folded into this file, then removed from `agent/`.

## State At Compression

| Area | State |
| --- | --- |
| Completed work covered | 0001-0262 |
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

- `tests/fixtures/parser`
- `tests/fixtures/diagnostics`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Runtime Build Backend And Language Lowering

Completed backend/runtime work established:

- `TypeSharp.Core` and `TypeSharp.Runtime` `net48` libraries with `Option<T>`, `Result<T,E>`, `Unit`, async/equality/pattern/union helpers.
- C# 7.3 source backend, generated project scaffold, runtime import lowering, generated `net48` compile smoke, generated project reference propagation.
- Lowering and fixture coverage for functions, blocks, imports, modules/namespaces, public API, classes/interfaces/records, generic types/functions/constraints, immutable records, nominal unions, pattern matching, type-level union narrowing, async `Task`, pipeline/composition, collection expressions and spread, indexer, `nameof`, checked/unchecked, `satisfies`, `yield`, `lock`, extension methods, record construction/spread, limited `keyof`, and limited indexed access.

Primary evidence:

- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests/Program.cs`
- `src/TypeSharp.Core`
- `src/TypeSharp.Runtime`

## CSharp Interop And Metadata Diagnostics

Completed interop work established:

- Framework and local DLL reference resolution.
- Metadata reader indexes for public types, fields, properties, indexers, generic methods, constraints, extension methods, params/byref/property accessors, static members, and nullability markers.
- C# constructor, member, field, property, indexer, delegate, event, attribute, generic type/method, nullable, overload, params/optional/named/byref, extension method, local/framework missing symbol, and generic constraint diagnostics.
- Imported class-to-interface/base assignment validation and metadata relationship ranking.
- Unsupported NuGet/package reference diagnostics and capability markers for `dynamic`, `reflect`, `interop`, and `unsafe`.

Primary evidence:

- C# interop tests in `tests/TypeSharp.Compiler.Tests/Program.cs`
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

- `tests/TypeSharp.Compiler.Tests/Program.cs`
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

Primary evidence:

- `docs/src/content/docs`
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

- Source module tests in `tests/TypeSharp.Compiler.Tests/Program.cs`
- Parser/backend/diagnostic fixtures under `tests/fixtures`
- [Modules And Imports](../docs/src/content/docs/modules.md), [Type System](../docs/src/content/docs/type-system.md)

## Task 0256 Test Suite Quality Audit

Completed test quality audit work established:

- Parser, binder, type-checker, and C# backend fixture directories now have enforced scenario README coverage for every `input.tysh` fixture.
- Missing scenario README files were added for backend indexer, record expression, partial declaration, `nameof`, and extension method lowering fixtures; binder unresolved `nameof`; type-checker collection/extension/record mismatch diagnostics; parser partial declaration and extension declaration fixtures.
- Parser, binder, and type-checker diagnostic fixtures now enforce polarity: positive fixtures must keep empty expected diagnostics, and negative fixtures must keep at least one structured diagnostic with code, message, file, and start/end locations.
- Runnable example CLI smoke helper now requires successful `check` and `build` commands to leave stderr empty.
- VS Code Extension Host smoke runner now fails on signal termination or null/nonzero exit status instead of converting a signal-terminated process result to success.
- `tests/tmp` was confirmed ignored and untracked, so generated residue is not part of the regression fixture set.

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
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
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

- `tests/TypeSharp.Compiler.Tests/Program.cs`
- `tests/fixtures`
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
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
git diff --check
```

Primary evidence:

- `docs`
- `agent`
- `.github/workflows/docs.yml`
- `tests/TypeSharp.Compiler.Tests/Program.cs`
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
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "parallel diagnostics"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "compiler check performance"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build uses module path containers"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build          # in docs
git diff --check
```

Primary evidence:

- `src/TypeSharp.Compiler/Checking/TypeSharpChecker.cs`
- `src/TypeSharp.Compiler/Building/TypeSharpBuilder.cs`
- `tests/TypeSharp.Compiler.Tests/Program.cs`
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
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "GitHub Pages workflow contract"
npm outdated --json    # in docs
```

Primary evidence:

- `docs/package.json`
- `docs/package-lock.json`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

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
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "GitHub Pages workflow contract"
```

Primary evidence:

- `docs/astro.config.ts`
- `tests/TypeSharp.Compiler.Tests/Program.cs`

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
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code extension package shape"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "VS Code syntax grammar"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "docs site contract"
npm run build          # in docs
git diff --check
```

Primary evidence:

- `vscode/typesharp/package.json`
- `vscode/typesharp/syntaxes/typesharp.tmLanguage.json`
- `vscode/typesharp/README.md`
- `vscode/typesharp/MARKETPLACE.md`
- [VS Code And LSP](../docs/src/content/docs/vscode-lsp.md)
- `tests/TypeSharp.Compiler.Tests/Program.cs`

## Verification Summary

Representative commands used across the completed range:

```powershell
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
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

- Completed historical work through task 0262 is compressed here.
- `agent/tasks.md` is the active task pointer.
- `agent/tasks-rollup.md` is the only completed task rollup file.

Remaining:

- Continue the active task in [tasks.md](tasks.md).
- Fold each completed active task back into this file and remove the completed packet.

Blocked:

- None.
