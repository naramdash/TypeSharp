---
title: Core Goal
description: TypeSharp's target runtime, language identity, and required success conditions.
---

TypeSharp is a new static language that emits `.NET Framework 4.8`-compatible artifacts and combines practical ideas from modern C#, F#, and TypeScript.

This page is the docs canonical core goal established by task `0251-docs-canonical-language-ledger`. Standard goal text now belongs here; Codex goal runs use `agent.md` plus the temporary agent work records under `agent/`.

## Mission

TypeSharp must let long-lived .NET Framework teams write new code with a modern static language while still producing assemblies that load, run, and interoperate like ordinary `net48` libraries.

The project is successful only when the language, compiler, runtime helpers, CLI, VS Code extension, docs, and examples work together as a usable development loop.

## Compatibility Baseline

- `.NET Framework 4.8` as the generated artifact and runtime compatibility target.
- VS Code and CLI as first-class adoption tools.
- nominal closed unions, type-level unions, null safety, structural checks, and async `Task` interop as required language capabilities.
- C#/.NET Framework interop and host compatibility for ASP.NET, WCF, Windows Service, scheduled job, and worker-style projects.

The compiler, CLI, and language server host may run on a modern .NET LTS runtime, but generated code and TypeSharp runtime/core libraries must stay compatible with `.NET Framework 4.8`.

## Language Identity

TypeSharp combines four design pressures without treating any source language as a copy target.

| Axis | TypeSharp Direction |
| --- | --- |
| TypeScript-style flexible typing | Local inference, contextual typing, structural shape checks, type-level unions, literal types, `unknown` narrowing, and limited compile-time type operators. |
| TypeScript-style modules | Every source file participates in an explicit module graph; imports, exports, source roots, and ambient declarations must be reproducible. |
| F#-style functional consistency | Immutable defaults, expression-oriented code, option/result modeling, nominal closed unions, pattern matching, pipeline, and composition are central language features. |
| C#/.NET practicality | Generated public API shape, attributes, properties, constructors, delegates, events, async `Task`, generics, and local C# DLL/framework assembly interop must feel predictable to .NET users. |

## Required Proof

The goal is not complete until the repository proves:

- generated `net48` execution;
- TypeSharp public APIs consumed from C# .NET Framework projects;
- C# framework and local `net48` assemblies consumed from TypeSharp;
- diagnostics, hover, definition, completion, and formatting through CLI and VS Code/LSP;
- null safety, nominal closed unions, type-level unions, structural checks, async `Task`, and public ABI boundaries with tests or fixture evidence;
- runnable console/library/interop/host-style examples with reproducible commands;
- a buildable Astro Starlight docs site that connects goal, grammar, CLI, diagnostics, VS Code/LSP, migration, examples, and project ledger pages.

## Boundaries

TypeSharp does not replace the CLR, does not promise JavaScript runtime compatibility, and does not copy every C#, F#, or TypeScript feature one-to-one. Features are adopted only when their TypeSharp semantics, .NET Framework lowering, public ABI rules, diagnostics, and tooling impact are clear.

Preview features from external languages stay out of the stable contract until they are explicitly classified and gated.

## Canonical Records

Use [Project Requirements](../requirements/) for the detailed requirement ledger, [Feature Status](../feature-status/) for the feature bucket map, [Project Ledger](../project-ledger/) for the web index of canonical project records, [Document Ownership](../document-ownership/) for the docs migration matrix, [Work Ledger](../work-ledger/) for current task state, and [Agentic Workflow](../agentic-workflow/) for Codex CLI goal and long-running task execution rules.

