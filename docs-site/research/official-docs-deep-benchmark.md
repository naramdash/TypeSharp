# Official Documentation Deep Benchmark

Benchmark date: 2026-05-19

Task rollup: [Documentation Process Release And Adoption](../../docs/0001-0255-task-ledger-rollup.md#documentation-process-release-and-adoption)

This benchmark records the official documentation information architecture that TypeSharp should match as it becomes a real language, compiler, CLI, and VS Code tooling project. It updates the first benchmark in [official-docs-benchmark.md](official-docs-benchmark.md) with a broader inventory snapshot and a stronger docs-site gap plan.

The benchmark summarizes structure and page archetypes. It does not copy official documentation prose.

## Inventory Method

The inventory was generated with a temporary Node.js script under `tests/tmp/` and recorded in [official-docs-deep-benchmark-inventory.json](official-docs-deep-benchmark-inventory.json). The script was not committed because it was only a one-off crawler for this task.

| Source | Official index used | Inventory scope | Entries | Unique hrefs | Sections |
| --- | --- | --- | ---: | ---: | ---: |
| Vue.js | <https://vuejs.org/llms.txt> | Official docs LLM index covering guide, API, style guide, examples, tutorial, and related docs families. | 94 | 94 | 17 |
| Nuxt | <https://nuxt.com/llms.txt> | Official Nuxt v4 docs, deployment guides, blog entries exposed by the official LLM index, and documentation set links. | 317 | 317 | 4 |
| TypeScript | <https://www.typescriptlang.org/docs/> | Official docs navigation links reachable from the rendered docs home. | 72 | 72 | 2 |
| C# | <https://raw.githubusercontent.com/dotnet/docs/main/docs/csharp/toc.yml> | Microsoft Learn C# TOC source from the official dotnet/docs repository. | 270 | 270 | 51 |
| F# | <https://raw.githubusercontent.com/dotnet/docs/main/docs/fsharp/toc.yml> | Microsoft Learn F# TOC source from the official dotnet/docs repository. | 148 | 148 | 34 |

Inventory limitations:

- Vue and Nuxt provide official `llms.txt` indexes, so those sources are the strongest page inventory for this benchmark.
- TypeScript does not expose the same `llms.txt` surface from the docs home in this run, so the snapshot records the official docs home navigation links rather than a search-engine crawl.
- C# and F# are represented through the official Microsoft Learn TOC source files, which are a better structural source than scraping rendered pages.

## Page Archetype Counts

The crawler classified every entry into one or more archetypes based on section, title, and href. Counts are directional, not a claim about content length.

| Source | Tutorial | Fundamentals | Guide | Cookbook/Examples | Grammar | API/Reference | Tooling/Config | Interop | Migration/Release | Troubleshooting | Advanced |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Vue.js | 3 | 31 | 58 | 0 | 2 | 38 | 0 | 3 | 0 | 5 | 14 |
| Nuxt | 3 | 34 | 39 | 20 | 0 | 120 | 15 | 2 | 16 | 33 | 25 |
| TypeScript | 2 | 71 | 0 | 1 | 0 | 5 | 5 | 26 | 5 | 0 | 1 |
| C# | 14 | 85 | 109 | 1 | 27 | 13 | 18 | 18 | 18 | 6 | 64 |
| F# | 0 | 35 | 111 | 0 | 26 | 108 | 8 | 9 | 9 | 1 | 10 |

## Navigation Patterns

### Vue.js

Vue separates a short getting-started path from a deeper guide, then gives a large API surface its own reference structure. The strongest pattern for TypeSharp is the separation between a learning guide and a non-linear reference. Vue also exposes style guide, TypeScript, performance, accessibility, security, and production concerns as discoverable topics instead of hiding them in an appendix.

TypeSharp action: keep [Language Tour](../src/content/docs/language-tour.md), [Fundamentals](../src/content/docs/fundamentals.md), and formal reference pages separate. Surface stability, diagnostics, and generated artifact concerns from the first-level navigation.

### Nuxt

Nuxt is the best benchmark for project structure, configuration, generated folders, command behavior, deployment, recipes, and operational workflow. Its documentation treats directories, configuration, commands, runtime behavior, deployment, testing, and upgrade paths as first-class information architecture.

TypeSharp action: add a dedicated project configuration page for `TypeSharp.toml`, source roots, generated output, references, target framework, configuration, and generated artifact ownership.

### TypeScript

TypeScript is the strongest benchmark for background-based onboarding and for separating a sequential handbook from advanced reference topics. Its docs distinguish "new programmer", "already knows JavaScript", "Java/C# background", "functional background", tooling, handbook, declaration files, modules, project configuration, and release notes.

TypeSharp action: keep background-based [Learning Paths](../src/content/docs/learning-paths.md), then add deeper module and type-system pages so TypeScript users can find TypeSharp equivalents for modules, narrowing, structural types, and public boundary rules.

### C#

C# is the strongest benchmark for language maturity, public metadata, compiler behavior, app domains, advanced topics, reference sections, and interop with the platform. It gives beginners runnable lessons while preserving deep pages for language strategy, program structure, async, LINQ, native interop, reflection, compiler options, and feature specifications.

TypeSharp action: document the generated `.NET Framework 4.8` artifact model with the language. Users need to know whether a feature is compile-time-only, generated C# source, runtime helper, public CLR metadata, or interop validation.

### F#

F# is the strongest benchmark for expression-oriented language documentation. Its TOC makes values, functions, inference, pattern matching, records, discriminated unions, computation, collections, object programming, async, type providers, reflection, modules, and `open` easy to find.

TypeSharp action: make the TypeSharp type system and module system independently discoverable. Functional features should not live only in a general "fundamentals" page.

## User Journey Comparison

| User | Official-docs pattern | TypeSharp requirement |
| --- | --- | --- |
| Programming beginner | Short start page, glossary-like fundamentals, and runnable hello-world path before reference. | `Start Here`, `Learning Paths`, `Language Tour`, and `Tutorials` must explain the first project and basic syntax without assuming compiler knowledge. |
| Existing .NET Framework maintainer | Platform compatibility, app models, generated artifacts, deployment, and host interop are visible early. | `.NET Interop`, `Project Configuration`, `CLI`, `Examples`, and `Troubleshooting` must explain `net48`, generated output, host references, and antivirus/build environment issues. |
| TypeScript user | Modules, structural types, narrowing, config, and compile-time vs runtime boundaries are explicit. | `Modules And Imports` and `Type System` must explain module graph, relative source paths, structural shapes, type-level unions, `unknown`, and public CLR boundary diagnostics. |
| F# user | Functional concepts and formal language reference both exist. | `Fundamentals`, `Type System`, `Grammar`, and `Language Reference` must expose immutability, functions, pipeline, option/result, unions, match, and async. |
| C# user | Public API shape, metadata, generics, delegates, attributes, overloads, async, and compiler options are indexed. | `.NET Interop`, `API`, `Project Configuration`, and `Advanced Topics` must connect TypeSharp syntax to generated C# and C# consumption rules. |
| Advanced implementer | Language specification, compiler behavior, versioning, and design notes are reachable after user docs. | `Advanced Topics`, docs-site canonical pages, temporary task packets, traceability, and tests must remain linked from the docs site. |

## TypeSharp Gap Matrix

| Gap found in current docs-site | Benchmark source | Improvement in this task |
| --- | --- | --- |
| Project manifest and generated output details were split across guides, API, CLI, and interop pages. | Nuxt configuration and directory docs, TypeScript project configuration, C# compiler options | Add `project-configuration.md` and sidebar entry. |
| Source module graph, relative module paths, imports, exports, and generated containers were buried in a reference paragraph. | TypeScript modules reference, F# namespaces/modules/open, Vue guide/reference split | Add `modules.md` and link it from fundamentals/reference. |
| Type inference, `unknown`, `dynamic`, nullability, structural shapes, unions, generics, and public ABI boundary were not grouped as a type-system topic. | TypeScript handbook/reference, F# types and inference, C# type system | Add `type-system.md` and link it from learning/reference pages. |
| Benchmark evidence existed, but not a durable full inventory snapshot. | All benchmark sources | Add `official-docs-deep-benchmark-inventory.json` and this report. |
| Docs contract tests verified older benchmark pages only. | All benchmark sources | Update docs-site contract checks for new pages, sidebar entries, and headings. |

## Overhaul Plan Applied

The docs-site now follows a benchmark-derived structure:

- Learn: overview, start here, learning paths, language tour, tutorials, fundamentals.
- Use TypeSharp: guides, project configuration, .NET interop, cookbook, examples, migration.
- Reference: modules and imports, type system, grammar, language reference, API/CLI reference, diagnostics, advanced topics.
- Tools and project: CLI, VS Code/LSP, troubleshooting, core goal.

This keeps the first-time user path short while giving intermediate and advanced users enough direct entry points to find implementation and reference material.
