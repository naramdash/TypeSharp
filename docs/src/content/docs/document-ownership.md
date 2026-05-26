---
title: Document Ownership
description: Target ownership model for TypeSharp documentation after the docs canonical ledger migration.
---

The target model is simple: `docs/` is the canonical standard language and project ledger surface. `agent/` keeps short repository-local notes outside the public docs site.

The public site source is `docs/`. Repository-local notes stay under `agent/`.

## Ownership Rule

| Surface | Target Role |
| --- | --- |
| `docs/src/content/docs/` | Canonical human-readable language reference, feature status, lowering guide, interop contract, CLI/LSP guide, examples guide, migration guide, and project ledger pages. |
| `agent/` | Short repository-local notes, ADR guidance, and the language 1.0 gap tracker. |
| `.codex/skills/` | Project-local Codex skill packages selected for TypeSharp work. |
| `agent.md` | Minimal local agent notes pointing to canonical docs and the language 1.0 tracker. |

## Canonical Standard Pages

Former operational bridge files below are no longer present. The target owner is the docs page to update first.

| Former Source | Canonical Owner |
| --- | --- |
| `docs/goal.md` | [Core Goal](../goal/) |
| `docs/requirements.md` | [Project Requirements](../requirements/) |
| `docs/feature-map.md`, `docs/feasibility.md` | [Feature Status](../feature-status/), [Advanced Topics](../advanced/) |
| `docs/grammar/**` | [Grammar](../grammar/), [Grammar And Language Reference](../reference/), [Modules And Imports](../modules/), [Type System](../type-system/) |
| `docs/standard-library.md`, `docs/runtime-abi.md` | [API And CLI Reference](../api/), [.NET Interop](../dotnet-interop/) |
| `docs/csharp-interop.md` | [.NET Interop](../dotnet-interop/) |
| `docs/lowering.md` | [Lowering](../lowering/) |
| `docs/cli.md`, `docs/formatting.md` | [CLI](../cli/), [API And CLI Reference](../api/), [VS Code And LSP](../vscode-lsp/) |
| `docs/diagnostics.md` | [Diagnostics](../diagnostics/), [Troubleshooting](../troubleshooting/) |
| `docs/migration-guide.md` | [Migration](../migration/) |
| `docs/architecture.md`, `docs/dependencies.md`, `docs/framework-targeting.md`, `docs/release.md`, `docs/regression-testing.md`, `docs/feature-review.md`, `docs/parser-fixtures.md`, `docs/references.md` | [Project Policy](../project-policy/), [Project Configuration](../project-configuration/), [Advanced Topics](../advanced/) |

Audience translation guides live directly in this docs collection:

| Audience | Canonical Owner |
| --- | --- |
| General developer grammar learning | [TypeSharp Syntax Guide](../syntax-guide/) |
| TypeScript developers | [From TypeScript](../from-typescript/) |
| C# and .NET Framework developers | [From C#](../from-csharp/) |
| F# and functional programming developers | [From F#](../from-fsharp/) |

Raw benchmark artifacts live under `docs/research/`. Source and runnable samples live under root `examples/`.

## Repository-Local Notes

| File | Target Role |
| --- | --- |
| `agent/README.md` | Short index for repository-local notes. |
| `agent/adr.md` | ADR authoring template for large decisions. |
| `agent/lang-1.0-tasks.md` | Language 1.0 gap tracker. |

## Maintenance Rule

1. Put new standard language/project reference material in docs canonical pages.
2. Do not add new `docs/` bridge stubs for standard docs.
3. Keep `agent/` concise and limited to repository-local notes.
4. Run docs build and stale-path scans after docs ownership changes.
