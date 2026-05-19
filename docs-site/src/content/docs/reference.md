---
title: Grammar And Language Reference
description: Scannable links to the current TypeSharp grammar and language reference material.
---

This page is a human-facing index. The canonical grammar documents live in the repository under `docs/grammar`.

## Lexical Basics

Source files use `.tysh`. The lexical grammar covers identifiers, keywords, literals, comments, and operators.

Canonical doc: [`grammar/lexical.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/lexical.md)

## Declarations

Implemented and planned declaration forms include functions, values, literals, records, classes, interfaces, delegates, unions, type aliases, modules, namespaces, imports, attributes, and `partial` type/module declarations.

`partial` is implemented for declarations that currently lower to generated C# types: modules, records, unions, classes, and interfaces. Partial methods, partial constructors, partial events, and source augmentation hooks remain future work.

Source files without an explicit file-scoped `namespace` still lower under the manifest `rootNamespace`; they are not emitted into a global namespace.

Each discovered `.tysh` file has a source-root-relative module path. If two source roots produce the same module path, `typesharp check` and `typesharp build` report `TS0111` before generated C# emission.

Single-source builds keep top-level functions on generated C# `Module`. Multi-source builds use deterministic module-path containers such as `ModuleMain` and `ModuleFeature_Helper`, so two files in the same C# namespace can compile into the same assembly.

Relative source module specifiers such as `./Helper` are resolved against that module path. Missing relative modules report `TS0112`. Existing relative source imports are recorded in the source module graph but currently report `TS0113` until project-wide import binding and C# lowering are implemented.

Ambient function signatures are explicit declarations of external host APIs. The current compiler parses and checks those signatures but omits them from generated C# output.

Root-level `open Namespace.Name` declarations lower to generated C# `using Namespace.Name;` directives. Ambiguity diagnostics and module-local `open` semantics remain future work.

Named import aliases such as `import { StringBuilder as Builder } from "System.Text"` lower to generated C# `using Builder = System.Text.StringBuilder;` directives. Namespace import aliases such as `import * as Text from "System.Text"` lower to `using Text = System.Text;`. If an import alias reuses a name from the same file scope, `typesharp check` reports `TS2002`.

Export specifier syntax such as `export { Name as Alias } from "Module"`, `export type { Name } from "Module"`, and `export * from "Module"` is parser-visible. Source module graph resolution and C# re-export lowering remain future work, so `typesharp check` and `typesharp build` currently report `TS2003` for those declarations instead of silently emitting incomplete C#.

Canonical doc: [`grammar/declarations.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/declarations.md)

Human guide: [Modules And Imports](../modules/)

## Expressions

Expressions include literals, identifiers, calls, member access, indexer access, blocks, `if`, `match`, lambdas, collection expressions, pipeline expressions, async/await, and record expressions for the implemented subset.

Canonical doc: [`grammar/expressions.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/expressions.md)

## Types

Type syntax includes primitive names, nullable types, arrays, generic types, function types, structural shapes, type-level unions, and nominal declarations.

Canonical doc: [`grammar/types.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/types.md)

Human guide: [Type System](../type-system/)

## Patterns

Pattern syntax supports nominal union and type-level union narrowing paths in the current smoke-tested scope.

Canonical doc: [`grammar/patterns.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/patterns.md)

## Interop Syntax

Interop syntax covers imports from C# namespaces, attributes, `ref`/`out`/`in`, named arguments, and capability markers.

Canonical doc: [`grammar/interop.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/interop.md)

## Public ABI Rules

Public API must lower to C#-understandable CLR metadata. Structural shapes and type-level unions are compile-time-only and must not leak directly through public boundaries.

Canonical docs:

- [`csharp-interop.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/csharp-interop.md)
- [`runtime-abi.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/runtime-abi.md)

## Diagnostic Code Index

Use [Diagnostics](../diagnostics/) for the current stable code list and `typesharp explain <CODE>` for command-line explanations.
