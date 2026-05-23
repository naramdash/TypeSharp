---
title: Tutorials
description: Sequential TypeSharp learning paths tied to runnable examples and smoke tests.
---

These tutorials point at runnable examples that are checked by the repository smoke suite. Copy the commands from each example README when you want the exact verified flow.

Start by installing the CLI with the release zip and checksum flow on [Install](../install/). When a tag does not have a published release asset yet, use the source-built fallback on [Start Here](../start-here/) and replace `typesharp` with the built CLI DLL command.

## 1. Hello Project

Goal: create and build a `net48` console program with nominal invoice records, a small calculation, and framework rendering.

Use: [`examples/runnable/console-hello`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/console-hello)

Core commands:

```powershell
typesharp new console HelloTypeSharp --target net48 --output .\HelloTypeSharp
cd .\HelloTypeSharp
typesharp check
typesharp build
typesharp run
```

Status: implemented and smoke-tested. Antivirus can block generated `.exe` launch on some machines; the build path is still valid when executable creation succeeds.

## 2. Library Public API

Goal: build a TypeSharp library whose generated assembly can be consumed from C#.

Use: [`examples/runnable/library-public-api`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/library-public-api)

You will see public account, quote, decision, and calculator types lowered to C#-friendly metadata and consumed from a C# host smoke project.

Status: implemented and smoke-tested.

## 3. C# Interop

Goal: build a local `net48` C# class library, reference its DLL from TypeSharp, and call realistic billing APIs.

Use: [`examples/runnable/csharp-interop`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/csharp-interop)

Core flow:

```powershell
dotnet build legacy-src
typesharp check
typesharp build
```

Status: implemented for local DLL references, constructors, static/instance members, properties, fields, indexers, delegates, events, attributes, generic types, generic method calls through generated C# inference, and byref/params/optional/named call smokes.

## 4. Diagnostics Workflow

Goal: read TypeSharp diagnostics as text or JSON and fix nullability errors at a customer profile boundary.

Use: [`examples/runnable/diagnostics-null-safety`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/diagnostics-null-safety)

Core command:

```powershell
typesharp check --diagnostic-format json
```

Status: implemented and smoke-tested.

## 5. VS Code And LSP Workflow

Goal: use diagnostics, hover, go-to-definition, completion, and formatting through the VS Code extension and language server.

Start with [VS Code And LSP](../vscode-lsp/).

Status: extension activation and language-server smokes are implemented.

## 6. Host Compatibility Overview

Goal: see how generated TypeSharp assemblies can be referenced by ASP.NET/WCF-style and worker-style `net48` host projects.

Use:

- [`examples/runnable/host-aspnet-wcf`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/host-aspnet-wcf)
- [`examples/runnable/host-worker`](https://github.com/naramdash/TypeSharp/tree/main/examples/runnable/host-worker)

Status: host reference shape is smoke-tested. Project template generation and packaging automation remain backlog.
