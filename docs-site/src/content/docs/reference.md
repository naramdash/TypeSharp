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

Ambient function signatures are explicit declarations of external host APIs. The current compiler parses and checks those signatures but omits them from generated C# output.

Root-level `open Namespace.Name` declarations lower to generated C# `using Namespace.Name;` directives. Ambiguity diagnostics and module-local `open` semantics remain future work.

Named import aliases such as `import { StringBuilder as Builder } from "System.Text"` lower to generated C# `using Builder = System.Text.StringBuilder;` directives.

Canonical doc: [`grammar/declarations.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/declarations.md)

## Expressions

Expressions include literals, identifiers, calls, member access, indexer access, blocks, `if`, `match`, lambdas, collection expressions, pipeline expressions, async/await, and record expressions for the implemented subset.

Canonical doc: [`grammar/expressions.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/expressions.md)

## Types

Type syntax includes primitive names, nullable types, arrays, generic types, function types, structural shapes, type-level unions, and nominal declarations.

Canonical doc: [`grammar/types.md`](https://github.com/naramdash/TypeSharp/blob/main/docs/grammar/types.md)

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
