---
title: Core Goal
description: TypeSharp's target runtime, language identity, and required success conditions.
---

TypeSharp is a new static language that emits `.NET Framework 4.8`-compatible artifacts and combines practical ideas from modern C#, F#, and TypeScript.

The canonical goal document is `docs/goal.md`. It defines:

- `.NET Framework 4.8` as the generated artifact and runtime compatibility target.
- VS Code and CLI as first-class adoption tools.
- nominal closed unions, type-level unions, null safety, structural checks, and async `Task` interop as required language capabilities.
- C#/.NET Framework interop and host compatibility for ASP.NET, WCF, Windows Service, scheduled job, and worker-style projects.

## Required Proof

The goal is not complete until the repository can prove generated `net48` execution, C# interop in both directions, CLI and VS Code diagnostics/navigation, runnable examples, and documentation build/deployment.

