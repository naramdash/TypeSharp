# Task 0167: Official Docs Deep Benchmark And Docs Overhaul

Status: Done
Queue: Q5
Start Time: 2026-05-19 22:08:34 +09:00
End Time: 2026-05-19 22:14:41 +09:00

## Objective

Analyze every reachable official documentation page and information architecture for Vue.js, Nuxt.js, TypeScript, C#, and F#, record a detailed benchmark result, then use that benchmark to overhaul the TypeSharp GitHub Pages docs so real human users from programming beginners through advanced users can find tutorials, guides, cookbook recipes, fundamentals, grammar, API, tooling, interop, diagnostics, and advanced language material.

This is a deeper follow-up to [Task 0150](0150-official-docs-benchmark-and-docs-expansion.md). Task 0150 established the first benchmark-driven docs expansion; this task requires a full inventory, stronger gap analysis, and substantially more complete human-facing docs coverage.

## Official Benchmark Sources

Use official documentation sources only unless the task handoff explicitly expands the scope.

- Vue.js: https://vuejs.org/guide/introduction.html
- Nuxt.js: https://nuxt.com/docs and current versioned docs such as https://nuxt.com/docs/4.x/getting-started/introduction
- TypeScript: https://www.typescriptlang.org/docs/
- C#: https://learn.microsoft.com/en-us/dotnet/csharp/
- F#: https://learn.microsoft.com/en-us/dotnet/fsharp/

## Scope

In:
- Inventory every public documentation page reachable from each official docs navigation tree, docs index, sitemap, or official docs source repository when necessary.
- Record benchmark date, version labels, source URLs, crawl or inventory method, navigation hierarchy, page titles, page categories, and page counts.
- Benchmark official docs structure, not just prose: top navigation, sidebar taxonomy, learning paths, page templates, examples, reference depth, search affordances, versioning, API/reference separation, troubleshooting, migration paths, and advanced material.
- Classify pages into tutorial, quickstart, fundamentals, guide, concept, cookbook, example, grammar, language reference, API reference, tooling, configuration, migration, troubleshooting, diagnostics, release notes, design rationale, and advanced implementation categories.
- Compare how the five official docs sites serve absolute beginners, experienced developers, framework/language migrators, and advanced users.
- Produce a TypeSharp docs gap matrix that maps each benchmark finding to concrete GitHub Pages improvements.
- Expand `docs-site` into a richer human-facing structure covering tutorials, guides, cookbook, fundamentals, grammar, API, diagnostics, CLI, VS Code/LSP, project structure, modules, type system, interop, .NET Framework hosting, examples, migration, troubleshooting, and advanced design/reference topics.
- Ensure TypeSharp language features related to TypeScript, F#, and C# influences are discoverable from beginner and advanced entry points.
- Add or update docs contract tests that verify important new pages, sidebars, headings, and cross-links exist.
- Commit and push when this task is completed.

Out:
- Copying official documentation prose or large verbatim excerpts.
- Claiming TypeSharp has feature parity with Vue, Nuxt, TypeScript, C#, or F#.
- Implementing compiler/runtime/language features discovered as gaps; create separate implementation tasks for those.
- Publishing GitHub Pages beyond verifying the existing docs build and workflow contract.

## Benchmark Deliverables

- A dated benchmark report, likely `docs/official-docs-deep-benchmark.md`, containing:
  - inventory/crawl method and limitations,
  - source URLs and version labels,
  - per-site navigation tree summaries,
  - page count and page archetype tables,
  - beginner/intermediate/advanced user journey comparison,
  - benchmarked docs patterns worth adopting,
  - TypeSharp docs gap matrix,
  - prioritized docs overhaul plan.
- Updated GitHub Pages content under `docs-site/src/content/docs/`.
- Updated Starlight sidebar/navigation for the expanded docs IA.
- Updated root docs cross-links when needed.
- Docs contract tests or equivalent checks for key pages and navigation.
- Updated `docs/tasks/README.md`, `docs/checklist.md`, and `docs/traceability.md` as appropriate.

## Acceptance Criteria

- [x] The benchmark report inventories all reachable official docs pages for Vue.js, Nuxt.js, TypeScript, C#, and F# or records explicit limitations for pages that cannot be automatically enumerated.
- [x] The report includes dated source URLs, version labels, navigation trees, page counts, page archetypes, and benchmark observations.
- [x] The report compares beginner, intermediate, advanced, migrator, and reference/API user paths across all five sources.
- [x] The report maps TypeSharp docs gaps to concrete docs-site additions or rewrites.
- [x] GitHub Pages docs include clear human-facing tutorials, guides, cookbook, fundamentals, grammar, API/reference, tooling, interop, diagnostics, migration, troubleshooting, examples, and advanced sections.
- [x] TypeSharp language features influenced by TypeScript, F#, and C# are documented for both beginners and advanced users.
- [x] New docs pages avoid copied official prose and explain TypeSharp-specific behavior, examples, limitations, and links.
- [x] Docs navigation and cross-links make the first-time path and reference path easy to follow.
- [x] Docs contract checks verify key new pages, sidebar entries, and headings.
- [x] The docs site build passes.
- [x] The task packet records verification commands and results before being marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site contract is stable"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Handoff Notes

- This task requires web access for a fresh official docs inventory. If web access is unavailable, request approval or use a checked-in inventory snapshot and record the limitation.
- Respect official site rate limits and prefer sitemap/source navigation files when available.
- Keep benchmark content as analysis and summary. Do not paste large official docs excerpts into the repository.
- If the benchmark identifies missing compiler/language behavior, add separate implementation tasks instead of mixing implementation with the docs overhaul.

## Progress

- 2026-05-19 22:08:34 +09:00: Replaced long inline PowerShell inventory attempts with a temporary Node.js inventory script under ignored `tests/tmp/`.
- 2026-05-19 22:09:26 +09:00: Generated `docs/official-docs-deep-benchmark-inventory.json` from official Vue, Nuxt, TypeScript, C#, and F# documentation indexes/TOCs.
- 2026-05-19 22:13:41 +09:00: Added `docs/official-docs-deep-benchmark.md`, new GitHub Pages pages for project configuration, modules/imports, and type system, sidebar entries, cross-links, and docs contract checks.
- 2026-05-19 22:14:41 +09:00: Ran final whitespace and tracked-binary checks.

## Verification Results

- `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`: passed with 0 warnings and 0 errors.
- `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site contract is stable"`: passed.
- `npm run build` from `docs-site`: passed and generated 24 pages.
- `git diff --check`: passed. Git reported expected LF-to-CRLF working-copy warnings only.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: passed with no tracked binaries listed.
