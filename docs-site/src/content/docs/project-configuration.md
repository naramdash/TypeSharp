---
title: Project Configuration
description: TypeSharp.toml, source roots, generated output, references, and build shape.
---

TypeSharp projects are described by `TypeSharp.toml`. The manifest is the stable contract between the CLI, compiler, generated C# backend, examples, and host projects.

## Minimal Manifest

```toml
[project]
name = "BillingRules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing"
generatedOutputRoot = "generated"
```

`targetFramework` should be `net48` for the compatibility profile. The compiler and CLI can run on a modern .NET SDK, but generated artifacts are built for .NET Framework 4.8 unless a task explicitly says otherwise.

## Source Roots

By default, source files live under `src` and use the `.tysh` extension. The compiler records a source-root-relative module path for every file. That path drives source module identity, duplicate module diagnostics, relative module resolution, and multi-source generated container names.

Example:

```text
src/
  Main.tysh
  Feature/Rules.tysh
```

`Feature/Rules.tysh` becomes a module path distinct from `Main.tysh`. If two source roots produce the same module path, `typesharp check` reports `TS0111`.

## Generated Output

`generatedOutputRoot` tells the CLI where generated C# source, the generated C# project, and build outputs belong.

```text
generated/
  src/Main.g.cs
  BillingRules.Generated.csproj
  bin/Release/net48/BillingRules.dll
```

Generated output is rebuildable and should not be treated as source of truth. The repository ignores generated folders in runnable examples.

## Output Type

Use `library` for DLLs consumed by C# or host projects. Use `exe` when the project has a supported `main` entry point.
Other `outputType` values are rejected as manifest diagnostics.

Executable projects can be run with:

```powershell
typesharp run TypeSharp.toml -- alpha beta
```

If local antivirus blocks a generated `.exe`, run `typesharp check` and `typesharp build` first to separate compiler errors from local execution policy.

## References

Local C# DLL references are listed under `references.paths`.

```toml
[references]
paths = ["lib/Legacy.Tools.dll"]
packages = []
```

The current compiler validates local DLL metadata directly. NuGet package restore through `references.packages` is reserved and currently reports `TS2405`.

## Configuration And Target Overrides

The CLI supports build configuration and target framework overrides for generated build/run paths.

```powershell
typesharp build TypeSharp.toml --configuration Release --target net48
typesharp run TypeSharp.toml --configuration Debug --target net48
```

Keep `net48` as the default TypeSharp compatibility target unless a documented compatibility profile says otherwise.

Use [Project Policy](../project-policy/) for the canonical dependency inventory, future dependency gate, target-framework profile rules, and release baseline policy.

## Language And Tooling Options

`language.version` currently supports `preview`. `language.nullable` supports `strict` and `loose`; strict mode reports unknown C# nullability warnings for interop calls that return reference types from metadata without nullable annotations.

`tooling.diagnosticFormat` supports `text` and `json`. `tooling.treatWarningsAsErrors = true` makes warning diagnostics fail `typesharp check` and `typesharp build` gates without changing the diagnostic payload severity.

## Related Pages

- [Modules And Imports](../modules/)
- [CLI](../cli/)
- [.NET Interop](../dotnet-interop/)
- [Project Policy](../project-policy/)
- [Troubleshooting](../troubleshooting/)
