---
title: Project Ledger
description: Canonical TypeSharp source-of-truth documents grouped for web navigation.
---

Task `0251-docs-site-canonical-language-ledger` changed the ownership model: docs-site is the canonical standard language and project ledger surface, while `docs/` remains the temporary operating area for agentic goal work and handoff state.

Use this page for integrated web navigation during the transition. The detailed migration matrix lives in [Document Ownership](../document-ownership/).

## Operating Rule

| Surface | Role | Update Rule |
| --- | --- | --- |
| `docs-site/` | Target canonical standard language, project ledger, reference, guide, and user-facing documentation | Update first for standard language and project reference decisions after the migration row is folded in. |
| `docs/` | Temporary agentic goal work, active task packets, rollups, handoff state, and execution-control bridge files | Keep operational and short; do not grow long-lived language reference here after task 0251. |
| `agent.md` | Codex CLI goal bootstrap and long-running agent instructions | Keep concise and operational. Do not replace it with a docs-site page. |

## Project Record Pages

- [Core Goal](../goal/) shows the mission in public docs form.
- [Project Ledger](../project-ledger/) is this source-of-truth map.
- [Work Ledger](../work-ledger/) shows the active task state and compressed completed-work rollup.
- [Document Ownership](../document-ownership/) tracks the docs-site canonical migration matrix.
- [Project Policy](../project-policy/) records architecture, dependency, target framework, regression, feature review, parser fixture, and release policy.
- [Agentic Workflow](../agentic-workflow/) explains how Codex CLI goal and long-running agents use the canonical records.

## Goal And Requirements

Canonical web records:

- [Core Goal](../goal/): project mission, required proof, non-goals, and adoption boundaries.
- [Project Requirements](../requirements/): required platform, language, compiler, interop, tooling, runtime, quality, security, and release constraints.
- [Feature Status](../feature-status/): MVP, stable backlog, preview watch, experimental, and rejected feature classification.
- [Project Policy](../project-policy/): architecture, dependency, target framework, regression, feature review, parser fixture, release, and official-reference tracking policy.
- [Start Here](../start-here/): role-based entry point for users and evaluators.
- [Learning Paths](../learning-paths/): background-specific reading order.
- [Document Ownership](../document-ownership/): target owner for each legacy `docs/` source.

Transition bridge files still used by agents and raw audits:

- [`docs/goal.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/goal.md): project mission bridge until the Core Goal migration is fully folded.
- [`docs/requirements.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/requirements.md): transition bridge for the requirements ledger now folded into [Project Requirements](../requirements/).
- [`docs/feasibility.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/feasibility.md): transition bridge for feasibility boundaries now folded into [Advanced Topics](../advanced/) and [Feature Status](../feature-status/).
- [`docs/feature-map.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/feature-map.md): transition bridge for feature classification now folded into [Feature Status](../feature-status/).

Related web pages:

- [Core Goal](../goal/)
- [Project Requirements](../requirements/)
- [Feature Status](../feature-status/)
- [Project Policy](../project-policy/)
- [Start Here](../start-here/)
- [Learning Paths](../learning-paths/)

## Language And Compiler Contracts

After task `0251`, the files below remain bridge sources. Their target canonical pages are listed in [Document Ownership](../document-ownership/).

- [`docs/grammar/README.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/README.md): transition bridge to [Grammar](../grammar/) and [Grammar And Language Reference](../reference/).
- [`docs/grammar/coverage.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/coverage.md): transition bridge for coverage decisions now folded into [Grammar](../grammar/), [Feature Status](../feature-status/), and [Grammar And Language Reference](../reference/).
- [`docs/feature-specs.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/feature-specs.md): transition bridge for the [Grammar And Language Reference](../reference/) feature specification index.
- [`docs/standard-library.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/standard-library.md): transition bridge for [API And CLI Reference](../api/), the canonical standard library namespace, core type, runtime helper, and interop helper policy.
- [`docs/csharp-interop.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/csharp-interop.md): transition bridge for [.NET Interop](../dotnet-interop/), the canonical C#/.NET Framework interop, public ABI, and host compatibility policy.
- [`docs/lowering.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/lowering.md): transition bridge for [Lowering](../lowering/), the canonical TypeSharp-to-C# 7.3 lowering contract and fixture evidence.
- [`docs/runtime-abi.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/runtime-abi.md): transition bridge for the [.NET Interop](../dotnet-interop/) runtime ABI policy.

Related web pages:

- [Grammar](../grammar/)
- [Grammar And Language Reference](../reference/)
- [Type System](../type-system/)
- [Lowering](../lowering/)
- [.NET Interop](../dotnet-interop/)
- [Advanced Topics](../advanced/)

## Tooling And User Adoption

After task `0251`, the files below remain bridge sources. The durable user-facing pages are [CLI](../cli/), [VS Code And LSP](../vscode-lsp/), [Diagnostics](../diagnostics/), [Migration](../migration/), and [Examples](../examples/). Raw example artifacts live outside `docs/` under root `examples/`.

- [`docs/cli.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/cli.md): transition bridge for [CLI](../cli/), the canonical command, manifest, diagnostics output, exit code, source discovery, and formatting contract.
- [`docs/formatting.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/formatting.md): transition bridge for the [CLI](../cli/) formatting convention and [VS Code And LSP](../vscode-lsp/) formatter integration.
- [`docs/diagnostics.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/diagnostics.md): transition bridge for [Diagnostics](../diagnostics/), the canonical diagnostic code, descriptor, explanation, output shape, and fixture policy.
- [`docs/migration-guide.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/migration-guide.md): transition bridge for [Migration](../migration/), the canonical incremental .NET Framework/C# adoption guide.
- [`examples/README.md`](https://github.com/naramdash/TypeSharp/blob/main/examples/README.md): root artifact index for examples now cataloged through [Examples](../examples/), [Tutorials](../tutorials/), and [Cookbook](../cookbook/).

Related web pages:

- [CLI](../cli/)
- [VS Code And LSP](../vscode-lsp/)
- [Diagnostics](../diagnostics/)
- [Migration](../migration/)
- [Examples](../examples/)

## Governance, Traceability, And Work Records

- [`docs/architecture.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/architecture.md): transition bridge for [Project Policy](../project-policy/) architecture and backend policy.
- [`docs/dependencies.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/dependencies.md): transition bridge for [Project Policy](../project-policy/) dependency inventory and future dependency gate.
- [`docs/framework-targeting.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/framework-targeting.md): transition bridge for [Project Policy](../project-policy/) target framework profile rules and [Project Configuration](../project-configuration/).
- [`docs/release.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/release.md): transition bridge for [Project Policy](../project-policy/) release, breaking-change, preview, checksum, security, and compatibility matrix policy.
- [`docs/regression-testing.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/regression-testing.md): transition bridge for [Project Policy](../project-policy/) regression evidence and snapshot policy.
- [`docs/feature-review.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/feature-review.md): transition bridge for [Project Policy](../project-policy/) feature review gate.
- [`docs/parser-fixtures.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/parser-fixtures.md): transition bridge for [Project Policy](../project-policy/) parser fixture layout and snapshot policy.
- [`docs/references.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/references.md): transition bridge for [Project Policy](../project-policy/) official reference tracking and refresh rules.
- [`agent.md`](https://github.com/naramdash/TypeSharp/blob/main/agent.md): Codex CLI goal bootstrap instructions.
- [`docs/agentic-execution.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/agentic-execution.md): long-running agent execution contract.
- [`docs/progress.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/progress.md): task packet, rollup, commit, and handoff policy.
- [`docs/checklist.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/checklist.md): executable project checklist and remaining work source.
- [`docs/tasks/README.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks/README.md): current task packet index and rollups.
- [`docs/traceability.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/traceability.md): goal, requirements, features, checklist, and evidence connections.
- [`docs/adr/README.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/adr/README.md): architecture decision record format.

Related web pages:

- [Agentic Workflow](../agentic-workflow/)
- [Work Ledger](../work-ledger/)
- [Advanced Topics](../advanced/)
- [Project Policy](../project-policy/)

## Change Discipline

When a decision changes:

1. For standard language or project ledger decisions, update the docs-site canonical page listed in [Document Ownership](../document-ownership/).
2. If a `docs/` bridge file still exists for that row, update it in the same change or reduce it toward a bridge stub.
3. Update implementation, tests, examples, and task packets if the rule is already implemented or being implemented.
4. Run the docs-site build when the change affects web pages or navigation.
