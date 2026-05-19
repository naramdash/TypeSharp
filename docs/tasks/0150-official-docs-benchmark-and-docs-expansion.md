# Task 0150: Official Docs Benchmark And Docs Expansion

Status: Planned
Queue: Q5
Start Time: TBD
End Time: TBD

## Objective

Analyze the official documentation websites for Vue.js, Nuxt.js, TypeScript, C#, and F# across their full page/navigation structures, record a benchmark report, and use that benchmark to expand the TypeSharp GitHub Pages docs into a detailed learning and reference experience for beginners through advanced users.

## Official Benchmark Sources

Use only official documentation sources unless a task handoff explicitly expands the scope.

- Vue.js: https://vuejs.org/
- Nuxt.js: https://nuxt.com/docs
- TypeScript: https://www.typescriptlang.org/docs/
- C#: https://learn.microsoft.com/en-us/dotnet/csharp/
- F#: https://learn.microsoft.com/en-us/dotnet/fsharp/

## Scope

In:
- Crawl or inventory every public documentation page reachable from the official docs navigation, sitemap, or equivalent docs index for each benchmark source.
- Record the current benchmark date, source URLs, navigation hierarchy, page categories, and page counts.
- Classify page archetypes such as tutorial, getting started, fundamentals, guide, concept, language reference, API reference, cookbook, migration, tooling, examples, troubleshooting, release/version notes, and advanced design guidance.
- Compare how each official docs site serves beginner, intermediate, and advanced users.
- Build a benchmark matrix that maps TypeSharp docs gaps against the strongest patterns from Vue, Nuxt, TypeScript, C#, and F#.
- Expand the Astro Starlight GitHub Pages docs based on the benchmark, not by copying source text.
- Cover TypeSharp language features, CLI, VS Code/LSP, project structure, .NET Framework hosting, C# interop, diagnostics, grammar, API surface, examples, migration, and advanced design/reference topics.
- Make the docs navigable for real human users: a beginner path, topic guides, cookbook recipes, full reference pages, troubleshooting, and advanced implementation notes.
- Add verification that the docs site builds and that key benchmark-derived pages/sections exist.
- Commit and push when this task is completed.

Out:
- Copying official docs prose or large verbatim excerpts.
- Claiming feature parity with Vue, Nuxt, TypeScript, C#, or F#.
- Implementing language/compiler features unless a benchmark gap directly requires a separate implementation task.
- Publishing the GitHub Pages site from CI beyond verifying the existing workflow/build contract.

## Benchmark Deliverables

- A benchmark report under `docs/` or `docs-site/` that includes:
  - source inventory method,
  - official URL list or generated sitemap/index snapshot,
  - navigation tree summary per source,
  - page archetype taxonomy,
  - beginner-to-advanced learning path comparison,
  - TypeSharp documentation gap matrix,
  - prioritized docs-site improvements.
- Updated GitHub Pages content under `docs-site/src/content/docs/`.
- Updated docs-site sidebar/navigation if new pages are added.
- Regression checks in `tests/TypeSharp.Compiler.Tests` or equivalent docs contract tests for important new pages/sections.
- Updated `docs/tasks/README.md`, `docs/checklist.md`, and `docs/traceability.md` as appropriate.

## Acceptance Criteria

- [ ] The benchmark report lists the crawl/inventory method and dated official source URLs for Vue.js, Nuxt.js, TypeScript, C#, and F#.
- [ ] The benchmark report records each source's docs navigation structure and page archetype distribution.
- [ ] The benchmark compares beginner, intermediate, and advanced user paths across the five official docs sites.
- [ ] The benchmark maps each TypeSharp docs gap to a concrete docs-site improvement.
- [ ] GitHub Pages docs add or expand pages for beginner onboarding, tutorials, fundamentals, guides, cookbook, grammar/reference, API, diagnostics, tooling, C#/.NET Framework interop, examples, migration, troubleshooting, and advanced topics.
- [ ] The docs explain TypeSharp-related language features in enough detail for programming beginners and advanced users to find what they need.
- [ ] The docs site build passes.
- [ ] Tests or docs contract checks verify the important new pages/sections.
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

- This task requires web access or a checked-in benchmark source inventory. If web access is restricted, request approval before crawling official docs.
- Keep benchmark text as analysis and summary. Do not copy official documentation content wholesale.
- Prefer adding more focused child task packets if the benchmark produces implementation-sized compiler or language work.
