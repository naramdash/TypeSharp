---
title: Examples
description: Runnable TypeSharp project catalog and feature examples.
---

Examples live in `docs/examples/`.

The smoke-tested runnable catalog is `docs/examples/runnable/`:

- `console-hello`: `typesharp check`, `build`, and `run` workflow.
- `library-public-api`: generated `net48` library with public record/class API.
- `csharp-interop`: TypeSharp consuming a local `net48` C# DLL.
- `host-aspnet-wcf`: ASP.NET Web Forms-style, WCF service, and WCF client/proxy-shaped `net48` host code referencing a generated TypeSharp library plus Core/Runtime DLLs.
- `host-worker`: worker-style `net48` host referencing a generated TypeSharp library plus Core/Runtime DLLs.
- `diagnostics-null-safety`: expected `TS2202` JSON diagnostics workflow.

Feature-oriented single-file examples remain in `docs/examples/*.tysh` and are connected to grammar and lowering docs.
