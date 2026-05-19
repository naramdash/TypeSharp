---
title: Modules And Imports
description: Source files, module paths, imports, exports, namespaces, and generated C# containers.
---

TypeSharp follows the TypeScript-style idea that files belong to a module graph. The graph is explicit: source roots define module identity, and import/export declarations define dependencies and public surface intent.

## Files Are Modules

Every `.tysh` source file receives a source-root-relative module path.

```text
src/Main.tysh           -> Main.tysh
src/Feature/Helper.tysh -> Feature/Helper.tysh
```

The compiler uses that path to detect duplicate modules and resolve relative specifiers such as `./Feature/Helper`.

## Namespaces

A file-scoped `namespace` gives generated C# a stable namespace.

```text
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

```text
import { StringBuilder } from "System.Text"
import { StringBuilder as Builder } from "System.Text"
import * as Text from "System.Text"
```

Named aliases lower to generated C# alias `using` directives. Namespace aliases lower to namespace alias `using` directives. Alias conflicts in the same file scope report `TS2002`.

## Relative Source Imports

Relative specifiers are resolved against the source module graph.

```text
import { helper } from "./Feature/Helper"
```

Missing relative modules report `TS0112`. Resolved relative imports are tracked by the source module graph. Some source import lowering remains active implementation work, so unsupported forms report `TS0113` rather than producing partial generated C#.

Unaliased named source imports lower through a generated C# `using static` directive for the target source module container:

```text
import { helper } from "./Feature/Helper"
```

Namespace source imports lower to a generated C# alias for the target source module container:

```text
import * as Helper from "./Feature/Helper"
```

Named source import aliases such as `import { helper as h } from "./Feature/Helper"` remain unsupported and report `TS0113`.

## Export Surface

`export` marks public TypeSharp declarations for generated API shape where the declaration is implemented by the backend.

```text
export record Customer(Name: string)
export fun greet(name: string): string = "Hello, " + name
```

Forwarding exports are parser-visible:

```text
export { Customer } from "./Models"
export type { CustomerShape } from "./Models"
export * from "./Models"
```

The current compiler reports `TS2003` for those forwarding forms until module graph public-surface lowering is implemented.

## Related Pages

- [Project Configuration](../project-configuration/)
- [Type System](../type-system/)
- [Grammar And Language Reference](../reference/)
- [Diagnostics](../diagnostics/)
