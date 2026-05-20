# Official Documentation Benchmark

Benchmark date: 2026-05-19

Task rollup: [Documentation Process Release And Adoption](../../docs/0001-0255-task-ledger-rollup.md#documentation-process-release-and-adoption)

This benchmark records the documentation information architecture patterns TypeSharp should match for real users. It uses official documentation indexes and navigation surfaces only; it summarizes structure and page archetypes without copying source prose.

## Sources And Inventory Method

The inventory uses official navigation, official LLM/index files where available, and official rendered table-of-contents pages. The goal is to capture every page family exposed by the official docs navigation, then translate those patterns into TypeSharp documentation requirements.

| Source | Official URLs | Inventory basis | Navigation coverage captured |
| --- | --- | --- | --- |
| Vue.js | <https://vuejs.org/> and <https://vuejs.org/llms.txt> | Official `llms.txt` documentation index | Guide, API Reference, Examples, Tutorial, About page families; at least 80 indexed documentation entries. |
| Nuxt | <https://nuxt.com/docs> and <https://nuxt.com/llms.txt> | Official `llms.txt` documentation index | Getting Started, Guide, Directory Structure, Recipes, API, Commands, Configuration, Advanced page families; 100+ indexed documentation entries. |
| TypeScript | <https://www.typescriptlang.org/docs/> and <https://www.typescriptlang.org/docs/handbook/intro.html> | Official docs home and Handbook navigation | Get Started, Handbook, Reference, Modules Reference, Tutorials, What's New, Declaration Files, JavaScript, Project Configuration, Cheat Sheets; 124 navigation entries in the Handbook docs tree. |
| C# | <https://learn.microsoft.com/en-us/dotnet/csharp/> and <https://learn.microsoft.com/en-us/dotnet/csharp/tour-of-csharp/> | Microsoft Learn C# landing, tour, and language reference pages | Beginner and experienced paths, fundamentals, key concepts, tutorials, what's new, language reference, specification, app-domain links, API reference. |
| F# | <https://learn.microsoft.com/en-us/dotnet/fsharp/> and <https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/> | Microsoft Learn F# landing and language reference pages | Learn to program, language guide, fundamentals, in-practice guides, tools, new features, language-reference concept tables; 40+ landing entries and 70+ language-reference entries. |

## Benchmark Navigation Summary

### Vue.js

Vue splits content by user intent:

- Getting started path: quick start, app creation, and ways of using Vue.
- Learning path: essentials first, then components in depth, reusability, built-in components, scaling up, and best practices.
- Reference path: global API, Composition API, Options API, built-ins, single-file components, and advanced APIs.
- Practice path: examples and a step-by-step tutorial.
- Trust path: performance, accessibility, security, production deployment, releases, team, community, and contribution pages.

TypeSharp takeaway: keep tutorial content separate from API reference, and make reliability topics visible instead of burying them under implementation notes.

### Nuxt

Nuxt is strong at operational documentation:

- Getting Started covers installation, configuration, views, assets, routing, SEO, transitions, data fetching, state, errors, server, layers, prerendering, deployment, testing, and upgrades.
- Guide content explains auto-imports, rendering modes, server engine, runtime config, custom routing, lifecycle hooks, modules, and directory structure.
- API content is grouped by components, composables, utilities, commands, configuration, and advanced framework APIs.
- Recipes are short and task-oriented, especially for fetch customization and authentication/session concerns.

TypeSharp takeaway: document project structure, command behavior, configuration files, generated output, and host deployment as first-class topics.

### TypeScript

TypeScript has the clearest language-doc split:

- Get Started pages are segmented by reader background: new programmer, JavaScript, Java/C#, functional programming, and tooling.
- The Handbook is a sequential conceptual path: basics, everyday types, narrowing, functions, object types, type manipulation, classes, and modules.
- Reference pages are non-linear deep dives: utility types, decorators, declaration merging, enums, iterators, JSX, mixins, namespaces, symbols, type compatibility, type inference, and declarations.
- Supporting areas cover modules, tutorials, declaration files, JavaScript projects, project configuration, cheat sheets, and release notes.

TypeSharp takeaway: preserve separate paths for background-based onboarding, sequential fundamentals, and deep reference. TypeSharp should also document compile-time-only features differently from public CLR metadata features.

### C#

C# uses Microsoft Learn's audience and maturity model:

- Beginner path: in-browser hello world, beginner tutorials, videos, and certification/training links.
- Experienced developer path: tours and transition material from Java, JavaScript, and Python.
- Fundamentals: type system, object-oriented programming, functional techniques, exceptions, coding style, and language strategy.
- Key concepts: LINQ, async, reflection, attributes, interface implementation, expression trees, native interop, performance, and Roslyn.
- Reference: language reference, keywords, operators, statements, special characters, documentation comments, unsafe code, preprocessor directives, compiler options, standards, and feature specifications.
- App domains: web, cloud, desktop, mobile, IoT, games, AI, microservices, and API reference.

TypeSharp takeaway: document the language and the generated .NET artifact model together. Users need to know what they can build, what is stable, and what C# or .NET concept backs each feature.

### F#

F# is the best benchmark for expression-oriented and functional language coverage:

- Beginner path: what F# is, strategy, first steps, install, IDE setup, videos, and further learning.
- Fundamentals: tour, values, types and inference, functional concepts, type providers, functions, pattern matching, object programming, and async.
- Practice path: web, machine learning, Azure, style guide, formatting, and coding conventions.
- Tools path: F# Interactive, development tools, notebooks, and JavaScript.
- Reference path: namespaces, modules, `open`, signatures, access control, XML docs, literals, strings, values, functions, loops, conditionals, pattern matching, exceptions, type inference, collections, records, discriminated unions, object programming, async/tasks/lazy, computation expressions, queries, attributes, reflection, quotations, type providers, FSharp.Core, keyword tables, symbol/operator tables, compiler options, and directives.

TypeSharp takeaway: TypeSharp docs need both a functional-language tour and a formal reference index. Pattern matching, unions, option/result, type inference, async, and interop should be easy to find from multiple paths.

## Page Archetype Taxonomy

| Archetype | Vue | Nuxt | TypeScript | C# | F# | TypeSharp action |
| --- | --- | --- | --- | --- | --- | --- |
| Overview | Strong | Strong | Strong | Strong | Strong | Keep `index.md` concise and user-facing. |
| Background-based onboarding | Moderate | Moderate | Strong | Strong | Moderate | Expand `start-here.md` and add `learning-paths.md`. |
| Sequential tutorial | Strong | Strong | Moderate | Strong | Strong | Keep runnable tutorials tied to smoke-tested examples. |
| Fundamentals / handbook | Strong | Strong | Strong | Strong | Strong | Expand TypeSharp fundamentals and add a one-page language tour. |
| Practical guides | Strong | Strong | Moderate | Strong | Strong | Keep project, CLI, and interop guides separate from reference. |
| Cookbook / recipes | Moderate | Strong | Low | Moderate | Low | Keep focused recipes for common .NET Framework tasks. |
| Language reference | Strong | Moderate | Strong | Strong | Strong | Keep grammar/reference pages authoritative and non-linear. |
| API reference | Strong | Strong | Strong | Strong | Strong | Separate CLI/API/config/runtime reference from prose guides. |
| Configuration and commands | Moderate | Strong | Strong | Strong | Moderate | Document `TypeSharp.toml`, CLI commands, generated output, and LSP commands. |
| Tooling | Strong | Strong | Strong | Strong | Strong | Keep VS Code/LSP and diagnostics workflows visible. |
| Interop / platform | Moderate | Strong | Moderate | Strong | Strong | Add a dedicated .NET Framework and C# interop docs-site page. |
| Advanced implementation | Strong | Strong | Strong | Strong | Strong | Add an advanced topics page for lowering, ABI, metadata, and compiler pipeline. |
| What's new / release notes | Strong | Strong | Strong | Strong | Strong | Track as follow-up until TypeSharp has versioned releases. |

## Beginner-To-Advanced Path Comparison

| User level | Official-docs pattern | TypeSharp documentation requirement |
| --- | --- | --- |
| Beginner | Start with a path by background, then use a small runnable tutorial. Avoid forcing reference material first. | `Start Here`, `Learning Paths`, `Language Tour`, and `Tutorials` must explain what to do before listing internal architecture. |
| Intermediate | Provide task guides, recipes, examples, config, commands, and troubleshooting. | `Guides`, `.NET Interop`, `Cookbook`, `Examples`, `CLI`, `Diagnostics`, and `Troubleshooting` must answer day-to-day project questions. |
| Advanced | Provide formal reference, API details, compiler behavior, versioning, and platform contracts. | `Grammar`, `Language Reference`, `API`, `Advanced Topics`, `Goal`, and repository design docs must make stability boundaries explicit. |

## TypeSharp Gap Matrix

| Gap before Task 0150 | Benchmark source | Docs-site improvement |
| --- | --- | --- |
| Background-based entry existed but did not give a full curriculum by skill level. | TypeScript Get Started, C# Choose your path, F# Learn to program | Added `learning-paths.md` and linked it from `index.md` and `start-here.md`. |
| Fundamentals page was compact and assumed too much prior language knowledge. | TypeScript Handbook, Vue Guide, F# fundamentals | Added `language-tour.md` as a one-page guided explanation before deep reference. |
| .NET Framework interop was spread across guides, API, examples, and design docs. | C# app/API docs, Nuxt deployment/config docs | Added `dotnet-interop.md` for `net48`, local DLLs, generated assemblies, hosts, runtime dependencies, and public ABI rules. |
| Advanced/compiler material was available only in repository design docs. | TypeScript Reference, C# specification/feature specs, F# language reference | Added `advanced.md` to connect lowering, metadata, diagnostics, LSP, performance, and feature maturity. |
| Docs contract tests only checked the previous page set. | All benchmark sources use stable navigation groups | Updated docs-site contract checks for new pages and benchmark-derived anchors. |
| No durable benchmark record linked the official docs analysis to TypeSharp docs changes. | All sources | Added this benchmark report, now stored under `docs-site/research/`. |

## Prioritized Docs-Site Improvements

Completed in Task 0150:

1. Add background and skill-level learning paths.
2. Add a human-readable language tour that sits before the formal grammar/reference pages.
3. Add a dedicated .NET Framework/C# interop page.
4. Add advanced topics for implementation-aware users.
5. Update the Starlight sidebar and docs contract test for the new IA.

Follow-up candidates:

1. Add versioned release notes when TypeSharp begins tagged preview releases.
2. Add generated API tables once public runtime/core packages stabilize.
3. Add shorter "try in 10 minutes" tutorials after CLI installation packaging is stable.
4. Add a public sitemap snapshot generated from the built docs site.
