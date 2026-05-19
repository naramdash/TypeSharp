# Task 0167: Official Docs Deep Benchmark And Docs Overhaul

Status: Planned
Queue: Q5
Start Time: TBD
End Time: TBD

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

- [ ] The benchmark report inventories all reachable official docs pages for Vue.js, Nuxt.js, TypeScript, C#, and F# or records explicit limitations for pages that cannot be automatically enumerated.
- [ ] The report includes dated source URLs, version labels, navigation trees, page counts, page archetypes, and benchmark observations.
- [ ] The report compares beginner, intermediate, advanced, migrator, and reference/API user paths across all five sources.
- [ ] The report maps TypeSharp docs gaps to concrete docs-site additions or rewrites.
- [ ] GitHub Pages docs include clear human-facing tutorials, guides, cookbook, fundamentals, grammar, API/reference, tooling, interop, diagnostics, migration, troubleshooting, examples, and advanced sections.
- [ ] TypeSharp language features influenced by TypeScript, F#, and C# are documented for both beginners and advanced users.
- [ ] New docs pages avoid copied official prose and explain TypeSharp-specific behavior, examples, limitations, and links.
- [ ] Docs navigation and cross-links make the first-time path and reference path easy to follow.
- [ ] Docs contract checks verify key new pages, sidebar entries, and headings.
- [ ] The docs site build passes.
- [ ] The task packet records verification commands and results before being marked Done.
- [ ] The completed task is committed and pushed.

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
