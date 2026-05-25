---
title: Project Configuration
description: TypeSharp.toml, source roots, generated output, references, and build shape.
---

TypeSharp projects are described by `TypeSharp.toml`. The manifest is the stable contract between the CLI, compiler, generated C# backend, examples, and host projects.

Start from [Install](../install/) for the release CLI, checksum, and matching runtime archive route before editing project references.

TypeScript's TSConfig is useful input for project ergonomics, but TypeSharp does not copy JavaScript runtime options. `module`, `moduleResolution`, `target`, `jsx`, `allowJs`, `checkJs`, package `exports`, `paths`, and project references only become TypeSharp features when they have a deterministic `TypeSharp.toml` meaning and a generated `net48` artifact shape.

## Minimal Manifest

```toml
[project]
name = "BillingRules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing"
generatedOutputRoot = "generated"
```

`targetFramework` should be `net48` for the default compatibility profile. The compiler and CLI can run on a modern .NET SDK, but generated artifacts are built for .NET Framework 4.8 unless a documented compatibility profile says otherwise. `net481` is reserved for a future explicit profile and must not be treated as an automatic upgrade from `net48`.

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

Framework assemblies are listed under `references.assemblies`. Local C# DLL references and TypeSharp Core/Runtime DLLs from the release runtime archive are listed under `references.paths`.

```toml
[references]
assemblies = ["System", "System.Core"]
paths = [
  "lib/Legacy.Tools.dll",
  "../typesharp-runtime/lib/net48/TypeSharp.Core.dll",
  "../typesharp-runtime/lib/net48/TypeSharp.Runtime.dll"
]
packages = []
```

Open the tag-specific GitHub Release notes, confirm the exact asset names, then download `typesharp-runtime-net48-<tag>.zip` from the same release as the CLI and verify it with `SHA256SUMS.txt` before referencing `TypeSharp.Core.dll` or `TypeSharp.Runtime.dll`. Keep those paths explicit for 1.0; the CLI does not auto-discover repository build folders or add hidden runtime references.

The 1.0 dependency acquisition scope is framework assemblies, explicit local `net48` DLLs, direct TypeSharp project references, and matching TypeSharp Core/Runtime DLLs from the release runtime archive. The current compiler validates local DLL metadata directly. NuGet package restore through `references.packages` is reserved and currently reports `TS2405`; package restore cannot become stable until the [Project Policy](../project-policy/) defines and implements lock files, package source mapping, audit severity, license inventory, checksum/signature, transitive dependency, and offline-cache behavior.

## Source Aliases

Manifest-owned source aliases are project configuration, not source syntax and not TypeScript `paths` compatibility.

```toml
[modules.aliases]
"@app/*" = "src/*"
"@features/*" = "src/Feature/*"
```

The compiler treats an alias import as a source graph lookup that either resolves to the same module identity as a relative `.tysh` import or stops before emission. An alias target must normalize under a declared `sourceRoots` entry in the current project. Targets that only help type checking, depend on a JavaScript runtime resolver, point at npm/package `exports` behavior, or cross project boundaries are outside the current alias contract. Cross-project source imports use direct TypeSharp project references instead.

Examples:

```tysh
import { rule } from "@app/Feature/Rules"
export { rule as featureRule } from "@features/Rules"
```

## Project References

TypeSharp project references describe direct multi-project build graph edges. Each referenced project has its own manifest, source roots, generated output root, and generated `net48` assembly.

```toml
[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

Implemented rules:

- `typesharp check` loads each referenced `TypeSharp.toml`, validates the graph, and derives direct referenced source-module export metadata without writing generated output.
- `typesharp build` builds direct referenced projects before the dependent project and adds their generated assemblies to the dependent generated C# project as explicit local `<Reference>` items with deterministic hint paths.
- Source imports across project boundaries use the direct referenced project name plus source-root-relative module path, for example `import { helper } from "Shared/Api"` when `../Shared/TypeSharp.toml` has `project.name = "Shared"` and `src/Api.tysh`.
- Cross-project `export ... from` forwarding is not part of the current slice; use direct imports from referenced modules instead.
- Direct references are required for source-level project imports. A project cannot import modules that are visible only through a referenced project's own transitive references.
- Missing manifests, invalid referenced manifests, cycles, duplicate direct project names, incompatible target frameworks, unresolved project module specifiers, and missing exported module members report diagnostics before dependent emission.

Use `references.paths` for already-built local DLL metadata that is not a TypeSharp project manifest.

## Configuration And Target Overrides

The CLI supports build configuration and target framework overrides for generated build/run paths.

```powershell
typesharp build TypeSharp.toml --configuration Release --target net48
typesharp run TypeSharp.toml --configuration Debug --target net48
```

Keep `net48` as the default TypeSharp compatibility target unless a documented compatibility profile says otherwise. A future `net481` override needs explicit manifest and CLI admission, Core/Runtime compatibility, generated project smoke coverage, and deployment assumptions before it can be accepted.

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
- npm package restore through TypeScript declaration lookup, or any NuGet restore path without TypeSharp-owned lock, source mapping, audit, license, checksum/signature, and metadata security policy.

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
