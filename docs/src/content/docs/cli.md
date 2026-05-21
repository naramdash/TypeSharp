---
title: CLI
description: TypeSharp CLI command surface and project workflow.
---

This is the canonical docs ledger for the TypeSharp CLI command surface, project workflow, formatting contract, exit codes, manifest shape, and source discovery behavior.

## Goals

- Inspect, build, and run `.tysh` projects from the CLI alone.
- Provide deterministic diagnostics and exit codes for CI.
- Share parser, binder, type checker, diagnostics, and formatting behavior with the language server.
- Standardize a project unit that emits `.NET Framework 4.8` artifacts.
- Let users run `typesharp explain` when they need diagnostic guidance.

## Command Surface

```text
typesharp version
typesharp new <console|library> <name> [--target net48] [--output <path>]
typesharp check [project] [options]
typesharp build [project] [options]
typesharp run [project] [-- args...]
typesharp explain <diagnostic-code> [--json]
typesharp format <project-or-path> [--check]
typesharp lsp
```

Stable backlog command surface:

```text
typesharp test [project]
```

`typesharp lsp` is the public stdio LSP entry point for editors and tooling hosts. The VS Code extension can still use its bundled language server DLL directly, but `typesharp lsp` is the stable CLI launch path.

## Command Contracts

### `typesharp version`

Prints CLI, compiler, language, runtime ABI, and default target information. It must run without loading a project and supports JSON output for scripts and CI.

Recommended text shape:

```text
TypeSharp CLI 0.1.0-preview
Compiler 0.1.0-preview
Language preview
Runtime ABI 0
Target default net48
```

### `typesharp new`

Creates starter `console` or `library` projects. The default target framework is `net48`, templates must not enable preview features by default, and generated `.tysh` source should follow the formatting convention on this page.

Generated starter files:

- `TypeSharp.toml`
- `src/Main.tysh` or `src/Library.tysh`
- `.gitignore`

### `typesharp check`

Loads the manifest, discovers source files, parses, binds, type checks, validates public ABI/interop rules, and reports diagnostics without writing generated output.

Required behavior:

- source discovery,
- manifest validation,
- parse diagnostics,
- name resolution diagnostics,
- type diagnostics,
- public ABI diagnostics,
- feature gate diagnostics.

For `[projectReferences]`, `check` loads direct referenced manifests, validates cycles and target compatibility, derives source-module export metadata for direct referenced imports, and still stops before writing generated output.

`check` uses the same diagnostic codes as VS Code and LSP. Use [Diagnostics](../diagnostics/) for the canonical code taxonomy and `typesharp explain` metadata.

### `typesharp build`

Runs the same diagnostics path as `check`, then emits C# 7.3-compatible generated source and builds a generated `net48` project.

The MVP backend is `--emit csharp`; future direct IL emission is Stable Backlog. If `--emit` is omitted, `csharp` is the default.

`typesharp build` and `typesharp run` accept `--configuration Debug|Release`; the selected value is passed to the generated SDK-style C# project build and reflected in the reported `bin/<Configuration>/net48` assembly or executable path.

Project build/run commands also accept `--target net48`. The override is validated by the CLI and takes precedence over the manifest `targetFramework` when writing the generated C# project and output path.

When a manifest contains `[projectReferences]`, `build` builds direct referenced projects before the dependent project and writes explicit local references to the referenced generated assemblies into the dependent generated C# project.

`typesharp build --verbosity quiet|minimal|normal|diagnostic` controls success logging: quiet suppresses artifact logs, minimal reports only the final assembly, normal reports generated source/project/assembly paths, and diagnostic adds option summary lines.

Project commands reject unknown options with usage exit code 2. `--preview` is accepted as the reserved switch for future preview-feature gates.

### `typesharp run`

Builds and executes an executable project. It only runs when `outputType = "exe"` and build succeeds.

Rules:

- values after `--` are forwarded to `main(args: string[])`,
- supported executable entry points are currently `main()` and `main(args: string[])`,
- invalid executable entry points report `TS3500`,
- generated executable launch can be blocked by local security tools; use `typesharp check` and `typesharp build` first when troubleshooting.

Current smoke behavior wraps `main(): string`, `main(): int`, `main(args: string[]): string`, and `main(args: string[]): int` into a generated C# entry point. `int` returns become process exit codes; non-null non-`int` returns are printed to stdout.

### `typesharp explain`

Looks up a diagnostic descriptor by code and prints:

- code,
- title,
- severity,
- category,
- message template,
- explanation,
- suggested action.

Lookup is case-insensitive. Unknown codes return exit code `1`; missing codes or invalid options return exit code `2`. `--json` and `--diagnostic-format json` emit the same JSON descriptor payload.

### `typesharp format`

`typesharp format` currently normalizes parser-clean `.tysh` files by enforcing LF line endings, trimming trailing whitespace, collapsing repeated blank lines, and preserving a final newline. `--check` reports formatting drift without rewriting files.

`--warnings-as-errors` and manifest `tooling.treatWarningsAsErrors = true` make warnings fail CLI check/build gates without changing the diagnostic severity payload.

The formatter does not rewrite files with parse diagnostics. The current MVP does not reorder declarations, reflow pipeline/match expressions, or perform full AST printing.

### `typesharp lsp`

Starts the TypeSharp language server over standard input/output using the current working directory as the workspace root. This command is for editor and tooling hosts that speak LSP framing directly; it does not write normal CLI progress output to stdout.

Rules:

- protocol messages use standard LSP `Content-Length` framing,
- `--no-color` is accepted as a no-op common option,
- other command options are usage errors with exit code `2`,
- the server exits cleanly after the LSP `exit` notification or stdin EOF.

## Common Options

| Option | Meaning |
| --- | --- |
| `--project <path>` | Project manifest path. |
| `--configuration Debug|Release` | Build configuration. |
| `--target net48` | Target framework override. |
| `--emit csharp|il` | Backend selection; `csharp` is MVP default and `il` is Stable Backlog. |
| `--diagnostic-format text|json` | Diagnostic output format. |
| `--warnings-as-errors` | Promote warnings to failing diagnostics. |
| `--no-color` | Disable ANSI color. |
| `--verbosity quiet|minimal|normal|diagnostic` | Build success log detail. |
| `--preview` | Reserved preview feature gate switch. |

CLI options may override manifest options. When they do, the override should be visible in diagnostics or logs. Unknown project command options are usage errors.

## Exit Codes

| Code | Meaning |
| --- | --- |
| `0` | Success. |
| `1` | User code or project diagnostics failed. |
| `2` | CLI usage error. |
| `3` | Compiler internal error. |
| `4` | Environment error, missing target framework, or missing toolchain. |
| `5` | Unsupported target/backend/feature combination. |

Warnings keep exit code `0` unless `--warnings-as-errors` or `tooling.treatWarningsAsErrors = true` is enabled.

## Project Manifest

The project manifest is `TypeSharp.toml`.

```toml
[project]
name = "HelloApp"
targetFramework = "net48"
outputType = "exe"
rootNamespace = "HelloApp"
sourceRoots = ["src"]
generatedOutputRoot = "obj/generated"
main = "HelloApp.main"

[language]
version = "preview"
strict = true
nullable = "strict"
previewFeatures = []

[references]
assemblies = [
  "System",
  "System.Core"
]
paths = []
packages = []

[projectReferences]
paths = []

[tooling]
diagnosticFormat = "text"
treatWarningsAsErrors = false
```

Manifest rules:

- `sourceRoots` controls `.tysh` discovery; if omitted, `src` is the default source root.
- `project.outputType` must be `library` or `exe`.
- `language.version` must be `preview`.
- `language.nullable` must be `strict` or `loose`; `strict` reports unknown C# nullability warnings at interop boundaries.
- `tooling.diagnosticFormat` must be `text` or `json`.
- source file extension is `.tysh`.
- source-root-relative module paths are case-insensitive for collision checks.
- duplicate module paths report `TS0111`.
- unresolved relative source modules report `TS0112`.
- unsupported source import forms report `TS0113`.
- missing named/type import, re-export, or namespace alias targets report `TS0114`.
- `bin/`, `obj/`, `.git`, and the generated output root are excluded from default discovery.
- default target framework is `net48`.
- default generated output root is `obj/generated`.
- `main` is required only for executable projects.
- `references.assemblies` names framework/GAC/reference assemblies.
- `references.paths` names explicit local DLL references.
- `references.packages` is reserved; the current compiler reports `TS2405` instead of restoring NuGet packages.
- `projectReferences.paths` names direct TypeSharp manifests. Direct project source imports use `ProjectName/ModulePath`, referenced projects build before dependents, and hidden transitive project source imports are not visible.
- MSBuild first-class integration is Stable Backlog and must not conflict with manifest semantics.

Use [Modules And Imports](../modules/) and [Grammar And Language Reference](../reference/) for source module import/export lowering details.

## Diagnostics Format

Text diagnostics:

```text
src/Main.tysh(8,15): error TS2204: Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.
```

JSON diagnostics:

```json
{
  "diagnostics": [
    {
      "code": "TS2204",
      "severity": "error",
      "message": "Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.",
      "file": "src/Main.tysh",
      "start": { "line": 8, "column": 15 },
      "end": { "line": 8, "column": 33 }
    }
  ]
}
```

Diagnostic codes are stable strings, source spans are 1-based line/column, and JSON diagnostics must convert to VS Code/LSP diagnostics without loss.

## Formatting Convention

Canonical `.tysh` layout:

- indentation is two spaces; tabs are not emitted,
- semicolons are not emitted,
- one declaration, statement, match arm, or pipeline stage per line,
- short lists may stay inline; multi-line lists put one element per line,
- multi-line lists do not use trailing commas,
- repeated blank lines collapse to one blank line,
- comments stay attached to the meaningful next node,
- formatting is idempotent.

File layout:

```tysh
namespace Company.Product

import { Thing } from "./thing"
import type { Shape } from "./shape"
open Company.Shared

export fun run(): int {
  0
}
```

Rules:

- file-scoped `namespace` comes first,
- `import` and `open` declarations follow namespace and precede declarations,
- formatter MVP preserves import/open order,
- namespace, import/open group, and first declaration are separated by one blank line,
- top-level declarations are separated by one blank line.

Modifier order:

```text
visibility -> partial -> static/abstract/sealed/virtual/override -> readonly/required -> async/unsafe/dynamic/reflect/interop/extern -> declaration keyword
```

Declaration and expression layout:

- attributes attach directly to the following declaration,
- block bodies open braces on the declaration line,
- expression-bodied functions may be inline or put the expression on the next line,
- final expressions prefer no `return`; early returns are preserved,
- short parameter/argument lists stay inline,
- multi-line parameter/argument lists put one item per line and align the closing delimiter with the opener,
- two or more pipeline stages use multi-line layout,
- `match` uses one arm per line and no trailing commas,
- `else`, `catch`, and `finally` align with the preceding block,
- long structural shapes put one member per line,
- long type-level unions put `|` at the start of each continuation line,
- C# imported names preserve their original casing.

The formatter must not rename, reorder, or change visibility in a way that changes public ABI.

## Implementation Order

1. `typesharp version`
2. manifest parser and source discovery
3. `typesharp check`
4. diagnostics text/JSON output
5. `typesharp build --emit csharp`
6. `typesharp run`
7. `typesharp new`
8. `typesharp explain`
9. `typesharp format`
10. `typesharp lsp`

## Related Pages

- [API And CLI Reference](../api/)
- [Project Configuration](../project-configuration/)
- [Diagnostics](../diagnostics/)
- [VS Code And LSP](../vscode-lsp/)
- [Examples](../examples/)
