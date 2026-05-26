---
title: Project Ledger
description: Canonical TypeSharp source-of-truth documents grouped for web navigation.
---

`docs/` is the canonical standard language and project ledger surface. `agent/` keeps short repository-local notes outside the public docs site.

Use this page for integrated web navigation. The detailed ownership matrix lives in [Document Ownership](../document-ownership/).

## Operating Rule

| Surface | Role | Update Rule |
| --- | --- | --- |
| `docs/` | Canonical standard language, project ledger, reference, guide, and user-facing documentation. | Update first for standard language and project reference decisions. |
| `agent/` | Short repository-local notes, ADR guidance, and the language 1.0 gap tracker. | Keep concise; do not add long-lived language reference here. |
| `.codex/skills/` | Project-local Codex skill packages selected for TypeSharp work. | Add, update, or remove skill folders only when the project needs them. |
| `agent.md` | Minimal local agent notes. | Keep concise. |

## Repository Layout

| Path | Role |
| --- | --- |
| `cli/` | TypeSharp command-line host and user-facing tool entrypoint. |
| `lang/` | Compiler, language server, package-free `net48` Core library, and package-free `net48` Runtime library. |
| `test/` | Regression runner, compiler/CLI/runtime/docs/VS Code smokes, and parser/type-checker/backend fixtures. |
| `docs/` | Canonical public documentation site and web-facing project ledger. |
| `agent/` | Short repository-local notes and language 1.0 gap tracker. |
| `examples/` | Single-file and runnable TypeSharp adoption artifacts used by docs and smoke tests. |
| `vscode/` | VS Code extension workspace, syntax assets, formatter client, LSP client, and package smoke tests. |

## Project Record Pages

- [Core Goal](../goal/): project mission, required proof, non-goals, and adoption boundaries.
- [Project Requirements](../requirements/): required platform, language, compiler, interop, tooling, runtime, quality, security, and release constraints.
- [Feature Status](../feature-status/): MVP, stable backlog, preview watch, experimental, and rejected feature classification.
- [Project Policy](../project-policy/): architecture, dependency, target framework, regression, feature review, parser fixture, release, and official-reference tracking policy.
- [Document Ownership](../document-ownership/): target owner for each canonical docs source.
- [Writing Guide](../writing-guide/): docs authoring style, `tysh` example project rules, emoji policy, and review checklist.

## Language And Compiler Contracts

- [Grammar](../grammar/) and [Grammar And Language Reference](../reference/): stable syntax, grammar summaries, feature spec index, and parser evidence.
- [Modules And Imports](../modules/): source module paths, imports, exports, and generated containers.
- [Type System](../type-system/): inference, null safety, `unknown`, `dynamic`, structural shapes, intersections, unions, and public ABI boundaries.
- [C# And CLR Type Model](../csharp-type-model/) and [C# Members And Overloads](../csharp-members-overloads/): Microsoft Learn C#-style detailed reference pages for TypeSharp's CLR type mapping, member lookup, overloads, delegates, events, extension methods, and diagnostics.
- [Lowering](../lowering/): TypeSharp-to-C# 7.3 lowering contract and fixture evidence.
- [Runtime Artifacts](../runtime-artifacts/): generated C# project shape, `net48` assembly layout, Core/Runtime references, and deployment set.
- [.NET Interop](../dotnet-interop/): C#/.NET Framework interop, runtime ABI, public ABI, and host compatibility.
- [API And CLI Reference](../api/): manifest, runtime/core libraries, generated assembly layout, and API reference entry points.

## Tooling And Adoption

- [CLI](../cli/): command surface, project workflow, formatting convention, diagnostics output, exit codes, and source discovery.
- [VS Code And LSP](../vscode-lsp/): extension, language server, formatting, diagnostics, hover, definition, and completion workflow.
- [Diagnostics](../diagnostics/): diagnostic code ranges, descriptor metadata, JSON/text shape, and explanation surface.
- [TypeSharp Syntax Guide](../syntax-guide/): developer-friendly grammar learning guide that complements the canonical grammar ledger.
- [From TypeScript](../from-typescript/), [From C#](../from-csharp/), and [From F#](../from-fsharp/): background-specific translation guides for developers adopting TypeSharp from existing language ecosystems.
- [Migration](../migration/): incremental .NET Framework/C# adoption guide.
- [Examples](../examples/), [Tutorials](../tutorials/), [Cookbook](../cookbook/), and [Writing Guide](../writing-guide/): runnable examples, learning paths, recipes, and authoring rules for future docs changes.

## Change Discipline

When a decision changes:

1. For standard language or project ledger decisions, update the docs canonical page listed in [Document Ownership](../document-ownership/).
2. Do not recreate standard-document bridge stubs under `docs/` or store durable standard docs under `agent/`.
3. Update implementation, tests, and examples if the rule is already implemented or being implemented.
4. Run the docs build when the change affects web pages or navigation.
