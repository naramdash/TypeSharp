---
title: Project Configuration
description: TypeSharp.toml, source roots, generated output, references, and build shape.
---

TypeSharp projects are described by `TypeSharp.toml`. The manifest is the stable contract between the CLI, compiler, generated C# backend, examples, and host projects.

TypeScript's TSConfig is useful input for project ergonomics, but TypeSharp does not copy JavaScript runtime options. `module`, `moduleResolution`, `target`, `jsx`, `allowJs`, `checkJs`, package `exports`, and `paths` only become TypeSharp features when they have a deterministic `TypeSharp.toml` meaning and a generated `net48` artifact shape.

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

## Planned Source Aliases

Manifest-owned source aliases are reserved for a future implementation slice. They are project configuration, not source syntax and not TypeScript `paths` compatibility.

```toml
[modules.aliases]
"@app/*" = "src/*"
"@shared/*" = "../Shared/src/*"
```

The compiler must treat an alias import as a source graph lookup that either resolves to the same module identity as a relative `.tysh` import or stops before emission. An alias target must normalize under a declared `sourceRoots` entry or through an explicit TypeSharp project reference. Targets that only help type checking, depend on a JavaScript runtime resolver, or point at npm/package `exports` behavior are outside the TypeSharp contract.

Until alias implementation lands, use relative imports such as `./Feature/Rules`.

## Planned Project References

TypeSharp project references are reserved for multi-project build graphs where each project has its own manifest and generated `net48` assembly.

```toml
[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

The planned behavior is:

- load each referenced `TypeSharp.toml`;
- check and build referenced projects before the dependent project;
- consume referenced generated assemblies and TypeSharp export metadata through explicit artifact paths;
- require direct project references for source-level imports across project boundaries;
- reject cycles, missing manifests, missing generated metadata, incompatible target frameworks, and missing exports before generated C# is emitted.

The current preview compiler does not implement `[projectReferences]`. Reference an already-built local `net48` DLL through `references.paths` when a project needs external CLR metadata today.

## Configuration And Target Overrides

The CLI supports build configuration and target framework overrides for generated build/run paths.

```powershell
typesharp build TypeSharp.toml --configuration Release --target net48
typesharp run TypeSharp.toml --configuration Debug --target net48
```

Keep `net48` as the default TypeSharp compatibility target unless a documented compatibility profile says otherwise.

Use [Project Policy](../project-policy/) for the canonical dependency inventory, future dependency gate, target-framework profile rules, and release baseline policy.
Use [Runtime Artifacts](../runtime-artifacts/) for the generated C# project shape, Core/Runtime reference model, and deployable `net48` artifact set.

## TypeScript-Inspired Configuration Roadmap

Accepted direction:

- source include/exclude patterns that still produce deterministic source-root-relative module paths;
- manifest-owned source aliases that lower or diagnose rather than behaving like TypeScript `paths` entries that do not affect emit;
- TypeSharp project references that build referenced projects first and consume generated assemblies/metadata through explicit paths;
- declaration/navigation metadata for editors when it can point back to `.tysh` source spans and generated C# artifacts;
- stricter checking profiles inspired by TSConfig strictness flags, expressed as TypeSharp diagnostic policy rather than JavaScript compiler switches.

Rejected or deferred:

- `tsconfig.json` as the TypeSharp project file;
- JavaScript/JSX source inclusion through `allowJs`, `checkJs`, or JSX options;
- Node/bundler module-resolution modes as language semantics;
- npm package restore through TypeScript declaration lookup before TypeSharp has a package lock, license, checksum, and metadata security policy.

## Language And Tooling Options

`language.version` currently supports `preview`. `language.nullable` supports `strict` and `loose`; strict mode reports unknown C# nullability warnings for interop calls that return reference types from metadata without nullable annotations.

`tooling.diagnosticFormat` supports `text` and `json`. `tooling.treatWarningsAsErrors = true` makes warning diagnostics fail `typesharp check` and `typesharp build` gates without changing the diagnostic payload severity.

## Related Pages

- [Modules And Imports](../modules/)
- [CLI](../cli/)
- [Runtime Artifacts](../runtime-artifacts/)
- [.NET Interop](../dotnet-interop/)
- [Project Policy](../project-policy/)
- [Troubleshooting](../troubleshooting/)
