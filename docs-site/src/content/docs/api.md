---
title: API And CLI Reference
description: TypeSharp CLI, manifest, runtime, generated assembly, and tooling reference.
---

## CLI Commands

Implemented commands:

```text
typesharp version
typesharp new <console|library> <name> [--target net48] [--output <path>]
typesharp check [project]
typesharp build [project]
typesharp run [project] [-- args...]
typesharp format [project-or-path] [--check]
typesharp explain <diagnostic-code>
```

See [CLI](../cli/) for command details and canonical behavior.

## Manifest Reference

`TypeSharp.toml` defines project identity and build shape.

```toml
[project]
name = "Sample"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Samples"
generatedOutputRoot = "generated"

[references]
paths = ["lib/Legacy.Tools.dll"]
```

Canonical doc: [`cli.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/cli.md)

## Runtime And Core Libraries

The generated code can depend on:

- `TypeSharp.Core`: `Option<T>`, `Result<T, E>`, and user-facing core helpers.
- `TypeSharp.Runtime`: helpers for generated unions, pattern matching, equality, hash composition, and async.

Canonical docs:

- [`standard-library.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/standard-library.md)
- [`src/TypeSharp.Core/README.md`](https://github.com/naramdash/TypeSharp/blob/main/src/TypeSharp.Core/README.md)
- [`src/TypeSharp.Runtime/README.md`](https://github.com/naramdash/TypeSharp/blob/main/src/TypeSharp.Runtime/README.md)

## Generated Assembly Layout

`typesharp build` emits generated source and a generated C# project under the configured generated output root. Build output follows:

```text
generated/
  src/*.g.cs
  <ProjectName>.Generated.csproj
  bin/<Configuration>/net48/<ProjectName>.dll
```

Executable projects produce `.exe` instead of `.dll`.

## VS Code Extension

The VS Code extension lives in `vscode/typesharp` and starts the language server over stdio. It currently covers diagnostics, hover, go-to-definition, completion, and formatting smokes.

See [VS Code And LSP](../vscode-lsp/) for commands and package-shape checks.
