---
title: Modules And Imports
description: Source files, module paths, imports, exports, namespaces, and generated C# containers.
---

TypeSharp follows the TypeScript-style idea that files belong to a module graph. The graph is explicit: source roots define module identity, and import/export declarations define dependencies and public surface intent.

The TypeSharp module roadmap deliberately stops short of TypeScript's full Node, bundler, CommonJS, package `exports`, and `paths` behavior. A source import must either resolve through the TypeSharp source graph or lower to a C# namespace/type import. Manifest-owned source aliases can be added later, but they must either rewrite to deterministic generated C# references or report diagnostics before emission; they must not create TypeScript-style type-check-only paths that fail at runtime.

## Files Are Modules

Every `.tysh` source file receives a source-root-relative module path.

```text
src/Main.tysh           -> Main.tysh
src/Feature/Helper.tysh -> Feature/Helper.tysh
```

The compiler uses that path to detect duplicate modules and resolve relative specifiers such as `./Feature/Helper`.

## Namespaces

A file-scoped `namespace` gives generated C# a stable namespace.

```tysh
namespace Samples.Billing

export fun total(): int = 42
```

If a file has no explicit namespace, the compiler uses the manifest `rootNamespace` instead of emitting into the global namespace.

## Generated Containers

Single-source builds keep top-level functions in generated C# `Module`.

Multi-source builds use module-path-based containers so multiple files can share one C# namespace without colliding.

```text
Main.tysh           -> ModuleMain
Feature/Helper.tysh -> ModuleFeature_Helper
```

This naming is deterministic and is part of the current generated source backend contract.

## Importing C# Namespaces

Imports from non-relative module specifiers are treated as C# namespace/type imports.

```tysh
import { StringBuilder } from "System.Text"
import { StringBuilder as Builder } from "System.Text"
import * as Text from "System.Text"
```

Named aliases lower to generated C# alias `using` directives. Namespace aliases lower to namespace alias `using` directives. Alias conflicts in the same file scope report `TS2002`.

## Relative Source Imports

Relative specifiers are resolved against the source module graph.

```tysh
import { helper } from "./Feature/Helper"
```

Missing relative modules report `TS0112`. Resolved relative imports are tracked by the source module graph. Future unsupported source import forms report `TS0113` rather than producing partial generated C#.

Unaliased named source imports lower through a generated C# `using static` directive for the target source module container:

```tysh
import { helper } from "./Feature/Helper"
```

Relative named function import aliases lower through a generated private forwarding method in the importing module container:

```tysh
import { helper as runHelper } from "./Feature/Helper"
```

Relative top-level value import aliases lower through a generated private property in the importing module container:

```tysh
import { PublicName as ImportedName } from "./Feature/Helper"
```

Relative type import aliases, including regular named aliases for exported source types, lower through generated C# `using` aliases:

```tysh
import type { Customer as Model } from "./Feature/Models"
import { Customer as NamedModel } from "./Feature/Models"
```

Relative named module import aliases lower through generated C# type aliases:

```tysh
import { Tools as HelperTools } from "./Feature/Helper"
```

Namespace source imports lower to a generated C# alias for the target source module container:

```tysh
import * as Helper from "./Feature/Helper"
```

Relative named and type imports must refer to names exported by the target source module. Namespace alias member access, such as `Helper.hidden()`, must also reference exported target members. Missing exports report `TS0114` before generated C# emission.

## Manifest-Owned Source Aliases

Source aliases are manifest-owned source graph mappings. They are not the same feature as TypeScript `paths`: TypeSharp aliases affect the source graph and generated C# identity, or they stop with diagnostics before emission.

Manifest shape:

```toml
[modules.aliases]
"@app/*" = "src/*"
"@shared/*" = "../Shared/src/*"
```

Current rules:

- A key may contain at most one `*`, and a value that uses `*` must use the same captured segment.
- A resolved target must normalize under a configured current-project source root and match a discovered `.tysh` source module.
- Alias resolution must be deterministic across platforms: normalized slash form, case-insensitive collision checks on Windows-compatible paths, and longest matching alias prefix before shorter wildcard matches.
- Resolved aliases reuse the normal source module graph. Missing target modules report the same unresolved source module class as `TS0112`; missing exported names still report `TS0114`.
- Unsupported alias syntax, ambiguous matches, or targets outside declared source roots report manifest or project diagnostics before generated C# is written.

Project-reference alias targets remain future work. Current aliases resolve only inside the current project's discovered source graph.

## TypeSharp Project References

TypeSharp project references are also a manifest-owned source graph feature. They are not JavaScript package references, npm workspace links, or TypeScript declaration-only project references. A referenced TypeSharp project must produce a generated `net48` assembly and explicit module/export metadata before a dependent project can compile against it.

Reserved manifest shape:

```toml
[projectReferences]
paths = ["../Shared/TypeSharp.toml"]
```

Project reference rules for the implementation slice:

- Each path names another `TypeSharp.toml`, not a generated DLL. DLLs stay under `references.paths`.
- The referenced project is checked and built first, using its own `generatedOutputRoot`, target framework, source roots, and diagnostics policy.
- The dependent project consumes the referenced generated assembly and TypeSharp export metadata through explicit generated artifact paths. No source import may cross into another project by walking parent directories.
- Project reference imports must preserve CLR-visible identity. Public records, classes, interfaces, delegates, enums, and nominal unions cross the boundary through generated C# metadata; compile-time-only structural shapes and type-level unions do not.
- Direct references are required for source-level project imports. TypeSharp must not assume hidden transitive project-reference visibility for generated `net48` artifacts.
- Cycles, missing manifests, incompatible target frameworks, stale or missing referenced artifacts, and missing exported module members must report deterministic diagnostics before emission.

## Export Surface

`export` marks public TypeSharp declarations for generated API shape where the declaration is implemented by the backend.

```tysh
export record Customer(Name: string)
export fun greet(name: string): string = "Hello, " + name
```

Local export lists can mark declarations in the same file as public surface:

```tysh
record Customer(Name: string)
fun greet(name: string): string = "Hello, " + name

export type { Customer }
export { greet }
```

Local named function export aliases also contribute public surface and emit forwarding methods:

```tysh
fun helper(): string = "helper"
export { helper as publicHelper }
```

Local literal export aliases contribute public surface and emit public constant or static readonly fields:

```tysh
literal InternalVersion: string = "1.0"
export { InternalVersion as PublicVersion }
```

Local top-level value export aliases contribute public surface and emit a public property backed by the generated field:

```tysh
let InternalName: string = "Ada"
export { InternalName as PublicName }
```

Explicitly annotated function-valued top-level `let` declarations lower to generated C# delegate values, and local aliases for those declarations emit forwarding properties:

```tysh
export let Transform: string -> string = text => text

let internalTransform: string -> string = text => text
export { internalTransform as PublicTransform }
```

Unannotated lambda-valued top-level `let` exports are also lowerable. The backend uses conservative delegate inference: the parameter type is `object`, and simple literal/name/comparison bodies infer the return side when possible. Add an explicit function type annotation when the public delegate metadata needs precise parameter types:

```tysh
export let Transform = text => text
export let NameFactory = text => "Ada"

let internalTransform = text => text
export { internalTransform as PublicTransform }
```

Local type export aliases contribute source module public surface and relative type imports lower to the original generated C# type:

```tysh
record Customer(Name: string)
export type { Customer as Model }
```

Duplicate names in local export lists report `TS2004`. Relative named/type source imports and namespace alias member access use this export surface; importing or accessing a non-exported target name reports `TS0114`.

Forwarding exports are parser-visible:

```tysh
export { Customer } from "./Models"
export type { CustomerShape } from "./Models"
export * from "./Models"
```

The current compiler lowers relative named function re-exports and top-level value re-exports, including aliases, for example:

```tysh
export { helper } from "./Feature/Helper"
export { helper as publicHelper } from "./Feature/Helper"
export { PublicName } from "./Feature/Helper"
```

The re-exporting module contributes `helper`, `publicHelper`, or `PublicName` to its export surface and emits a generated C# forwarding method or property in its module container. The target must already export the function or top-level value; missing targets report `TS0114`.

Relative type-only re-exports also contribute to the source module type surface:

```tysh
export type { VisibleModel as PublicModel } from "./Feature/Models"
```

Downstream `import type { PublicModel as Model } from "./Barrel"` lowers to a generated C# `using` alias for the original target type. The target must already export the type; missing targets report `TS0114`.

Relative module re-export aliases also contribute to the source module surface:

```tysh
export { Tools as PublicTools } from "./Feature/Helper"
```

Downstream `import { PublicTools as HelperTools } from "./Barrel"` lowers to a generated C# type alias for the original module type.

Relative star re-exports forward the currently lowerable function, top-level value, lambda-valued top-level value, and type surface from the target module:

```tysh
export * from "./Feature/Helper"
```

Non-relative forwarding and non-lowerable forwarding forms still report `TS2003`.

## Roadmap Boundary

Near-term module work should focus on:

- manifest-owned source path aliases that preserve generated C# identity and cannot silently diverge from runtime references;
- stricter diagnostics for unsupported non-relative source imports, non-lowerable re-exports, and side-effect-only imports;
- project-graph references between TypeSharp projects, modeled through generated assemblies and source-module metadata rather than JavaScript package resolution;
- editor navigation metadata for generated source and public exports, analogous to TypeScript declaration maps but grounded in TypeSharp source spans and C# artifacts.

## Related Pages

- [Project Configuration](../project-configuration/)
- [Type System](../type-system/)
- [Grammar And Language Reference](../reference/)
- [Diagnostics](../diagnostics/)
