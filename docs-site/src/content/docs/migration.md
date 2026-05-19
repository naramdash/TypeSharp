---
title: Migration
description: Adopting TypeSharp from existing .NET Framework and C# projects.
---

The migration guide is maintained in `docs/migration-guide.md`.

The current adoption path is incremental:

- keep existing `.NET Framework 4.8` projects and C# assemblies,
- introduce TypeSharp as generated `net48` libraries,
- reference explicit framework assemblies or local DLLs,
- expose C#-friendly nominal public APIs,
- use null safety, `Option<T>`, `Result<T,E>`, and diagnostics to improve new code first.

Automatic conversion, NuGet restore integration, MSBuild-first project authoring, and source generators remain future work.

