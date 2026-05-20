---
title: Work Ledger
description: Current long-running TypeSharp task state and completed work rollup.
---

This page is the web-facing view of the long-running task ledger. The compact source record lives in `docs/tasks.md`, including user and agent task sections, the active task packet, and `docs/tasks-rollup.md`.

Codex CLI goal and other long-running agents should still read the flat canonical files under `docs/` when selecting work. This page exists so humans can inspect the same state through the docs site.

## Current State

| Item | State |
| --- | --- |
| Active task packet | None |
| Next top-priority task | Not selected |
| Task queue owner | [`docs/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks.md) |
| Current tooling slice | Task `0256-test-suite-quality-audit` is complete and folded into the compressed task rollup. No active task is selected. |
| Completed work covered | 0001-0256 |
| Canonical task index | [`docs/tasks.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks.md) |
| Compressed work ledger | [`docs/tasks-rollup.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/tasks-rollup.md) |

## Completed Work Themes

| Theme | Covered Work |
| --- | --- |
| Foundation, parser, semantics | Fixture policy, grammar precedence/ambiguity, compiler/CLI skeleton, parser coverage, diagnostics, binder, type checker, duplicate symbol diagnostics. |
| Runtime, backend, lowering | Runtime/core helpers, generated `net48` C# build pipeline, public API lowering, records, unions, null safety, structural checks, structural intersection aliases, limited `keyof`, limited indexed access types, and `satisfies` proof erasure, async `Task`, iterator `yield`, block-level `lock`, explicit-receiver extension methods, collection expressions including array/List spread lowering, pipeline/composition/indexer/`nameof`/checked-unchecked/record expressions including nominal record spread, partial declarations, explicit and inferred generic C# calls. |
| C# interop | Framework/local DLL references, framework/local metadata reader, constructor parameter validation including generic constructor type substitution, optional/named/params constructor calls and ambiguous constructor diagnostics, overload/byref/params/optional/named validation including known literal/null/imported metadata argument type, delegate lambda arity, known delegate lambda return type, identity lambda parameter return filtering, known lambda return conversion ranking, metadata-backed lambda member-chain return filtering, metadata-backed lambda method-call return filtering, metadata-backed lambda extension method-call return filtering, metadata-backed lambda static method-call return filtering, and metadata-backed lambda binary predicate return filtering, null metadata-specificity ranking, method/indexer/extension receiver metadata relationship distance, extension receiver `object` fallback, and numeric literal conversion filtering, explicit and inferred generic method constraint validation including constructed generic arguments, framework constraints, and transitive base/interface metadata checks, imported C# class-to-interface/base assignment validation, imported indexer parameter type, numeric literal conversion, exact/metadata-relationship ranking, and ambiguous candidate validation, extension method metadata markers, imported receiver instance-call syntax and extension no-matching overload diagnostics, nullable diagnostics, delegates/events, fields, generic methods, generic type constructor calls, interfaces, attributes, unsupported package references, framework-backed missing type/method/member diagnostics, metadata-backed `TS2402`/`TS2406`-`TS2417` diagnostics, and imported C# parameter/local alias/assignment receiver tracking. |
| CLI and VS Code | `check`, `build`, `run`, `format`, `new`, `lsp`, common options, warning gates, target/configuration/verbosity handling, strict option parsing, semantic manifest value validation, VS Code activation, LSP diagnostics, hover, definition, completion, and formatting. |
| Docs and adoption | Astro Starlight docs site, GitHub Pages workflow, runnable examples, migration guide, docs benchmarks, host compatibility notes, progress/ADR/regression policy, release readiness, and traceability. |
| Agentic bootstrap | Post-0251 bootstrap docs now direct agents to docs-site canonical standard ledger pages, keep `docs/` as the temporary work surface, and require task-end commit/push handoff in `agent.md`. |
| Source modules and safety gates | Capability diagnostics, `unknown` narrowing, root namespace fallback, ambient signatures, `open`, import aliases, source module path identity, relative source graph/lowering, multi-source containers, export parsing, local export-list public surface, local/relative named function import/export/re-export alias forwarding, local literal export alias forwarding, local top-level value export alias lowering, explicitly annotated function-valued top-level `let` export and alias lowering, relative top-level value import/re-export alias lowering, local/relative type import/export alias lowering including regular named exported type aliases, relative named module import alias lowering, relative named module re-export alias remapping, relative type-only re-export remapping, relative star re-exports over the currently lowerable function/value/type surface, and missing source module export diagnostics for named/type imports, re-exports, and namespace alias member access. |
| Regression quality | Task 0256 audited parser/backend/diagnostic fixtures, CLI/run/example smokes, metadata/C# interop, runtime/core, VS Code mocked/live/host tests, docs-site build smoke, and generated residue handling; it hardened fixture README coverage, diagnostic polarity, runnable CLI stderr checks, and Extension Host exit semantics. |

## Remaining Known Future Areas

- Non-relative re-export, unannotated lambda-valued `export let`, and non-lowerable source re-export forms beyond the current function/literal/value/type/module alias forwarding and remapping slice.
- Fuller indexer conversion/ranking beyond exact/object/known numeric/metadata relationship checks, broader contextual generic inference beyond direct and explicit constructed generic argument positions, fuller C# overload conversion/contextual ranking beyond current literal/null metadata-specificity/imported metadata relationship/delegate arity, known return checks, identity lambda parameter return checks, known lambda return conversion ranking, metadata member-chain return inference, metadata instance method-call return inference, imported extension method-call return inference, imported static method-call return inference, and comparison/logical binary predicate return inference, richer lambda body contextual typing beyond those known/literal, identity, member-chain, instance/extension/static method-call, and binary predicate bodies, and richer extension conversion/conflict diagnostics.
- Future IL backend work after the C# 7.3 source backend remains stable.

## Update Rule

When a long-running task completes:

1. Update `docs/tasks-rollup.md`.
2. Update this page if the current state, theme summary, or remaining future areas changed.
3. Update [Agentic Workflow](../agentic-workflow/) only if the way agents choose or record work changed.
4. Run `npm run build` from `docs-site`.
