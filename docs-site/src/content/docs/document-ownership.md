---
title: Document Ownership
description: Target ownership model for TypeSharp documentation after the docs-site canonical ledger migration.
---

The target model is simple: docs-site is the canonical standard language and project ledger surface, while `docs/` remains the temporary operating area for agentic goal work, handoff packets, and execution control.

This page is the ownership matrix established by task `0251-docs-site-canonical-language-ledger`. Some rows still point to existing `docs/` bridge files because those files are short operational pointers, while moved raw artifacts record their new non-`docs/` location.

## Ownership Rule

| Surface | Target Role |
| --- | --- |
| `docs-site/src/content/docs/` | Canonical human-readable language reference, feature status, lowering guide, interop contract, CLI/LSP guide, examples guide, migration guide, and project ledger pages. |
| `docs/` | Temporary agentic workspace for active goal runs, task packets, rollups, checklist/traceability bridge files, and handoff state. |
| `agent.md` | Minimal bootstrap file for Codex goal runs. It points to docs-site canonical pages and the remaining `docs/` temporary work surface. |

## Target Canonical Docs-Site Pages

Every current or recently migrated standard language/project ledger document under `docs/` must have one of three target owners:

- `docs-site canonical`: the durable user/project reference belongs in docs-site.
- `docs temporary agentic`: the file remains in `docs/` because it controls active agent work, handoff, or execution state.
- `archive/remove`: the file is a raw audit artifact or source sample that should leave `docs/` after links, tests, and task records are updated.

### Goal, Requirements, And Project Policy

| Source Or Former Source | Target Owner | Migration Status |
| --- | --- | --- |
| `docs/goal.md` | `docs-site/src/content/docs/goal.md`, `start-here.md`, `project-ledger.md` | Core Goal is now the target canonical page; `docs/goal.md` remains an agent bootstrap bridge. |
| `docs/requirements.md` | `requirements.md`, `project-ledger.md`, `start-here.md`, `advanced.md` | Canonical requirement ledger folded into `requirements.md`; `docs/requirements.md` is now a short bridge stub. |
| `docs/feasibility.md` | `advanced.md`, `feature-status.md`, `project-ledger.md`, `project-policy.md` | Feasibility boundaries folded into `advanced.md`, `feature-status.md`, and `project-policy.md`; `docs/feasibility.md` is now a short bridge stub. |
| `docs/feature-map.md` | `feature-status.md`, `language-tour.md`, `type-system.md`, `reference.md` | Canonical feature status and external-language mapping ledger folded into `feature-status.md` and related reference pages; `docs/feature-map.md` is now a short bridge stub. |
| `docs/feature-specs.md` | `reference.md`, `project-ledger.md`, `project-policy.md` | Implemented/stable feature specification index, canonical spec links, evidence links, and promotion update rules folded into `reference.md`; `docs/feature-specs.md` is now a short bridge stub. |
| `docs/architecture.md` | `project-policy.md`, `advanced.md`, `project-ledger.md` | Compiler pipeline, host target policy, project split, backend strategy, runtime/interop/tooling architecture, and risk controls folded into `project-policy.md`; `docs/architecture.md` is now a short bridge stub. |
| `docs/dependencies.md` | `project-policy.md`, `project-configuration.md`, `advanced.md` | Dependency inventory, compatibility audit, and future dependency gate folded into `project-policy.md`; `docs/dependencies.md` is now a short bridge stub. |
| `docs/framework-targeting.md` | `project-policy.md`, `dotnet-interop.md`, `project-configuration.md` | Target framework profile policy and `net48`/`net481` boundary folded into `project-policy.md` and `project-configuration.md`; `docs/framework-targeting.md` is now a short bridge stub. |
| `docs/release.md` | `project-policy.md`, `project-ledger.md`, `advanced.md` | Release versioning, breaking-change gates, preview gate, checksum/signing, security, compatibility matrix, release notes template, and release checklist folded into `project-policy.md`; `docs/release.md` is now a short bridge stub. |
| `docs/regression-testing.md` | `project-policy.md`, `advanced.md`, `project-ledger.md` | Regression evidence matrix, checklist closure, snapshot update policy, and failure triage folded into `project-policy.md`; `docs/regression-testing.md` is now a short bridge stub. |
| `docs/feature-review.md` | `project-policy.md`, `project-ledger.md`, `advanced.md` | Feature review gate and required evidence questions folded into `project-policy.md`; `docs/feature-review.md` is now a short bridge stub. |
| `docs/parser-fixtures.md` | `project-policy.md`, `advanced.md`, `project-ledger.md` | Parser fixture layout, positive/negative rules, diagnostic JSON shape, syntax tree snapshot policy, and current parser coverage summary folded into `project-policy.md`; `docs/parser-fixtures.md` is now a short bridge stub. |
| `docs/references.md` | `project-policy.md`, page footnotes as needed | Official .NET Framework, C#, F#, TypeScript, Windows lifecycle reference set and refresh rules folded into `project-policy.md`; `docs/references.md` is now a short bridge stub. |
| `docs/official-docs-benchmark.md` | `docs-site/research/official-docs-benchmark.md`, `project-ledger.md`, `learning-paths.md` | Raw benchmark summary moved out of `docs/` into docs-site research artifacts. |
| `docs/official-docs-deep-benchmark.md` | `docs-site/research/official-docs-deep-benchmark.md`, `project-ledger.md`, `learning-paths.md` | Raw benchmark summary moved out of `docs/` into docs-site research artifacts. |
| `docs/official-docs-deep-benchmark-inventory.json` | `docs-site/research/official-docs-deep-benchmark-inventory.json` | Raw inventory artifact moved out of `docs/` into docs-site research artifacts. |

### Grammar And Language Reference

| Current Source | Target Owner | Migration Status |
| --- | --- | --- |
| `docs/grammar/README.md` | `grammar.md`, `reference.md` | Converted to a short bridge stub after grammar goals, stability levels, design rules, implementation order, and bridge inventory were folded into `grammar.md`. |
| `docs/grammar/ambiguity.md` | `grammar.md`, `reference.md`, `project-policy.md` | Parser ambiguity and recovery decisions folded into `grammar.md`; file is now a short bridge stub. |
| `docs/grammar/consistency.md` | `grammar.md`, `cli.md`, `advanced.md` | Syntax consistency and formatting policy folded into `grammar.md` and `cli.md`; file is now a short bridge stub. |
| `docs/grammar/coverage.md` | `grammar.md`, `feature-status.md`, `reference.md` | Coverage model and external-language examples folded into `grammar.md`, `feature-status.md`, and `reference.md`; file is now a short bridge stub. |
| `docs/grammar/declarations.md` | `grammar.md`, `reference.md`, `lowering.md` | Declaration grammar folded into `grammar.md`, `reference.md`, and `lowering.md`; file is now a short bridge stub. |
| `docs/grammar/expressions.md` | `grammar.md`, `reference.md`, `type-system.md`, `lowering.md` | Expression grammar folded into `grammar.md`, `reference.md`, `type-system.md`, and `lowering.md`; file is now a short bridge stub. |
| `docs/grammar/interop.md` | `grammar.md`, `dotnet-interop.md`, `reference.md` | Interop grammar folded into `grammar.md`, `.NET Interop`, and `reference.md`; file is now a short bridge stub. |
| `docs/grammar/lexical.md` | `grammar.md`, `reference.md` | Lexical grammar folded into `grammar.md`; file is now a short bridge stub. |
| `docs/grammar/modules.md` | `grammar.md`, `modules.md`, `reference.md` | Module grammar folded into `modules.md`, `grammar.md`, and `reference.md`; file is now a short bridge stub. |
| `docs/grammar/patterns.md` | `grammar.md`, `type-system.md`, `reference.md` | Pattern grammar folded into `grammar.md`, `type-system.md`, and `reference.md`; file is now a short bridge stub. |
| `docs/grammar/precedence.md` | `grammar.md`, `reference.md` | Expression/type/pattern precedence table folded into `grammar.md`; file is now a short bridge stub. |
| `docs/grammar/resolution.md` | `grammar.md`, `modules.md`, `advanced.md`, `reference.md`, `dotnet-interop.md` | Name/member/overload resolution folded into `grammar.md`, `reference.md`, modules, and `.NET Interop`; file is now a short bridge stub. |
| `docs/grammar/types.md` | `grammar.md`, `type-system.md`, `reference.md` | Type grammar folded into `grammar.md`, `type-system.md`, and `reference.md`; file is now a short bridge stub. |

### Runtime, Interop, Tooling, And User Guides

| Source Or Former Source | Target Owner | Migration Status |
| --- | --- | --- |
| `docs/csharp-interop.md` | `dotnet-interop.md`, `advanced.md` | C#/.NET reference model, import model, type mapping, overload/member validation, capability boundary, host compatibility, public API exposure, diagnostics, and interop smoke policy folded into `dotnet-interop.md`; `docs/csharp-interop.md` is now a short bridge stub. |
| `docs/runtime-abi.md` | `dotnet-interop.md`, `advanced.md` | Runtime ABI version fields, covered surfaces, change rules, compatibility gates, and pre-1.0 ABI policy folded into `dotnet-interop.md`; `docs/runtime-abi.md` is now a short bridge stub. |
| `docs/standard-library.md` | `api.md`, `type-system.md`, `dotnet-interop.md`, `lowering.md` | Standard library namespace policy, core type policy, runtime helper policy, and interop helper boundaries folded into `api.md`, `type-system.md`, `.NET Interop`, and `lowering.md`; `docs/standard-library.md` is now a short bridge stub. |
| `docs/lowering.md` | `lowering.md`, `advanced.md`, `reference.md` | Canonical lowering contract and evidence map folded into `lowering.md`; `docs/lowering.md` is now a short bridge stub. |
| `docs/cli.md` | `cli.md`, `api.md` | CLI commands, options, exit codes, manifest/source discovery, diagnostics output shape, and implementation order folded into `cli.md`; `docs/cli.md` is now a short bridge stub. |
| `docs/formatting.md` | `cli.md`, `vscode-lsp.md` | Formatting convention and `typesharp format --check` policy folded into `cli.md`, with VS Code formatter linkage in `vscode-lsp.md`; `docs/formatting.md` is now a short bridge stub. |
| `docs/diagnostics.md` | `diagnostics.md`, `troubleshooting.md` | Diagnostic code ranges, descriptor table, metadata requirements, output shape, and golden fixture policy folded into `diagnostics.md`; `docs/diagnostics.md` is now a short bridge stub. |
| `docs/migration-guide.md` | `migration.md` | Incremental adoption strategy, minimal project shape, public API rules, C# pattern mapping, interop calls, generated DLL consumption, nullability/error migration, host compatibility, unsupported automation, and adoption checklist folded into `migration.md`; `docs/migration-guide.md` is now a short bridge stub. |
| `docs/examples/README.md` | `examples.md`, `tutorials.md`, `cookbook.md`, `examples/README.md` | Example narrative folded into docs-site `examples.md`; the artifact index moved out of `docs/` to root `examples/README.md`. |
| `docs/examples/*.tysh` | `examples/*.tysh`, `examples.md` narrative links | Source samples moved out of `docs/` to root `examples/`. |
| `docs/examples/cli-console/**` | `examples/cli-console/**`, `examples.md` narrative links | Runnable CLI starter project moved out of `docs/` to root `examples/cli-console/`. |
| `docs/examples/runnable/**` | `examples/runnable/**`, `examples.md` narrative links | Runnable smoke projects moved out of `docs/` to root `examples/runnable/`. |

## Temporary Agentic Work Surface

| File Or Folder | Target Role |
| --- | --- |
| `docs/tasks/README.md` | Active task packet index while agentic goal work is running. |
| `docs/tasks/*-task-ledger-rollup.md` | Temporary compressed work history until folded into docs-site work ledger. |
| `docs/tasks/0001-0253-task-ledger-rollup.md` | Compressed completed-work ledger including tasks `0251-docs-site-canonical-language-ledger`, `0252-agent-bootstrap-docs-site-canonical-followup`, and `0253-cli-manifest-semantic-validation`. |
| `docs/agentic-execution.md` | Temporary agent execution contract for goal runs; it may later become a short bridge to docs-site Agentic Workflow. |
| `docs/progress.md` | Temporary policy bridge for task packets, rollups, and handoff mechanics. |
| `docs/checklist.md` | Temporary checklist bridge until incomplete work is represented in docs-site ledger pages. |
| `docs/traceability.md` | Temporary evidence bridge until traceability is represented in docs-site project ledger pages. |
| `docs/adr/README.md` | Temporary ADR authoring template until architecture decision records are represented through docs-site or moved under an agentic work area. |
| `docs/README.md` | Temporary bridge index for the agentic work surface. |

## Post-0251 Maintenance Rule

1. Put new standard language/project reference material in docs-site canonical pages.
2. Keep remaining `docs/` standard pages as short bridge stubs, or remove them after references are updated.
3. Keep active task, handoff, checklist, traceability, and goal execution-control documents in `docs/`.
4. Update `agent.md`, `docs/agentic-execution.md`, Work Ledger, Project Ledger, and Agentic Workflow when agent work selection or ownership rules change.
5. Run docs-site build and stale-path scans before marking the task done.
