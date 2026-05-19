---
title: CLI
description: TypeSharp CLI command surface and project workflow.
---

The CLI contract is defined in `docs/cli.md`.

Implemented command surface:

```text
typesharp version
typesharp new <console|library> <name>
typesharp check <project>
typesharp build <project>
typesharp run <project>
typesharp explain <diagnostic-code>
typesharp format <project-or-path> [--check]
```

The CLI creates starter console/library projects, reads `TypeSharp.toml`, discovers `.tysh` sources, reports diagnostics in text or JSON, emits C# 7.3-compatible generated projects, builds `net48` assemblies, and runs executable projects when the local environment permits generated executable launch.

`typesharp build` and `typesharp run` accept `--configuration Debug|Release`; the selected value is passed to the generated SDK-style C# project build and reflected in the reported `bin/<Configuration>/net48` assembly or executable path.

Project build/run commands also accept `--target net48`. The override is validated by the CLI and takes precedence over the manifest `targetFramework` when writing the generated C# project and output path.

`typesharp build --verbosity quiet|minimal|normal|diagnostic` controls success logging: quiet suppresses artifact logs, minimal reports only the final assembly, normal reports generated source/project/assembly paths, and diagnostic adds option summary lines.

Project commands reject unknown options with usage exit code 2. `--preview` is accepted as the reserved switch for future preview-feature gates.

`typesharp format` currently normalizes parser-clean `.tysh` files by enforcing LF line endings, trimming trailing whitespace, collapsing repeated blank lines, and preserving a final newline. `--check` reports formatting drift without rewriting files.

`--warnings-as-errors` and manifest `tooling.treatWarningsAsErrors = true` make warnings fail CLI check/build gates without changing the diagnostic severity payload.

Runnable CLI examples are listed in `docs/examples/runnable/`.
