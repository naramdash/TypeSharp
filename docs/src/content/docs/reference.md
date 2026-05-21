---
title: Grammar And Language Reference
description: Scannable links to the current TypeSharp grammar and language reference material.
---

This page is the docs canonical language-reference index established by task `0251-docs-canonical-language-ledger`. It links the human-facing reference pages and the implemented feature evidence.

## Lexical Basics

Source files use `.tysh`. The lexical grammar covers identifiers, keywords, literals, comments, and operators.

Canonical page: [Grammar](../grammar/)

## Declarations

Implemented and planned declaration forms include functions, values, literals, records, classes, interfaces, simple enums, delegates, unions, type aliases, modules, namespaces, imports, attributes, and `partial` type/module declarations.

`partial` is implemented for declarations that currently lower to generated C# types: modules, records, unions, classes, and interfaces. Partial methods, partial constructors, partial events, and source augmentation hooks remain future work.

Simple TypeSharp-owned enum declarations parse, bind duplicate members, support optional explicit integral underlying types, integer numeric member values, aliases to previously declared members, enum initializer-local composite member expressions such as `ReadWrite = Read | Write`, and declaration/member attribute lists. The checker validates explicit numeric values and numeric operands against the selected underlying type range, validates identifier operands against previously declared same-enum members, type-checks same-enum member access and expression-level same-enum value `|`, `&`, `^`, and unary `~` forms such as `Permission.Read | Permission.Write`, `permission & Permission.Read`, `permission ^ Permission.Write`, and `~permission`, lowers to ordinary generated C# enum declarations and C# `|`/`&`/`^`/`~` expressions, and participates in match exhaustiveness. Named imported C# enums also participate in match exhaustiveness when metadata exposes finite public enum members, and imported enum metadata captures underlying type names plus literal numeric member values for future interop decisions. Numeric/general expression bitwise operators, flag algebra beyond same-enum value `|`/`&`/`^`/`~`, arbitrary/general computed enum member values, broad attribute target validation, numeric pattern algebra, and flag-style reasoning over imported numeric metadata remain future work.

Source files without an explicit file-scoped `namespace` still lower under the manifest `rootNamespace`; they are not emitted into a global namespace.

Each discovered `.tysh` file has a source-root-relative module path. If two source roots produce the same module path, `typesharp check` and `typesharp build` report `TS0111` before generated C# emission.

Single-source builds keep top-level functions on generated C# `Module`. Multi-source builds use deterministic module-path containers such as `ModuleMain` and `ModuleFeature_Helper`, so two files in the same C# namespace can compile into the same assembly.

Relative source module specifiers such as `./Helper` are resolved against that module path. Manifest-owned aliases under `[modules.aliases]`, such as `@app/* = "src/*"`, resolve current-project imports/re-exports into the same source graph before lowering. Missing relative modules or alias targets report `TS0112`. Unaliased relative or alias named imports lower to generated C# `using static` directives for target module containers, named function import aliases lower to generated C# private forwarding methods, named top-level value import aliases lower to generated C# private forwarding properties, type import aliases including regular named exported type aliases lower to generated C# `using` aliases, named module import aliases lower to generated C# type aliases, namespace imports lower to generated C# container aliases, named function re-exports, including `as` aliases, lower to generated C# forwarding methods, named top-level value re-exports lower to generated C# forwarding properties, relative type-only re-exports remap downstream `import type` aliases to the original generated C# type, and relative star re-exports forward the currently lowerable function/value/type source surface. Future unsupported source import forms report `TS0113`. Named/type imports, named function/value/type re-exports, and namespace alias member access that reference a target module name outside its export surface report `TS0114`.

Ambient function signatures are explicit declarations of external host APIs. The current compiler parses and checks those signatures but omits them from generated C# output.

Root-level `open Namespace.Name` declarations lower to generated C# `using Namespace.Name;` directives. Ambiguity diagnostics and module-local `open` semantics remain future work.

Named import aliases such as `import { StringBuilder as Builder } from "System.Text"` lower to generated C# `using Builder = System.Text.StringBuilder;` directives. Namespace import aliases such as `import * as Text from "System.Text"` lower to `using Text = System.Text;`. If an import alias reuses a name from the same file scope, `typesharp check` reports `TS2002`.

Local export specifier syntax such as `export { Name }` and `export type { TypeName }` marks same-file declarations as generated public surface. Local named function export aliases such as `export { helper as publicHelper }` also contribute public surface and emit generated forwarding methods. Local literal export aliases such as `export { InternalVersion as PublicVersion }` contribute public surface and emit generated public constant or static readonly fields. Local top-level value export aliases such as `export { InternalName as PublicName }` contribute public surface and emit a generated public property backed by the top-level `let` field. Function-valued top-level `let` declarations such as `export let Transform: string -> string = text => text` lower to generated C# `System.Func`/`System.Action` values, and local export aliases for those declarations emit forwarding properties. Unannotated lambda-valued top-level exports such as `export let Transform = text => text` lower with conservative `System.Func<object, TResult>` delegate inference, including supported block lambda and collection expression return bodies; use an explicit function type annotation for precise public parameter metadata. Local type export aliases such as `export type { Customer as Model }` contribute source module public surface and relative type imports lower to the original generated C# type through a `using` alias. Duplicate local export names report `TS2004`. Relative named function re-export syntax, including aliases such as `export { helper as publicHelper } from "./Helper"`, contributes to the exporting module surface and emits a forwarding method. Relative named top-level value re-export syntax, including aliases such as `export { InternalName as PublicName } from "./Helper"`, emits a forwarding property. Relative module re-export aliases such as `export { Tools as PublicTools } from "./Helper"` remap downstream named module import aliases to the original generated C# module type. Relative type-only re-export syntax, including aliases such as `export type { Customer as Model } from "./Models"`, contributes to the exporting module type surface and remaps downstream `import type` aliases. Relative star re-exports forward the currently lowerable function/value/type source surface. Non-relative forwarding and non-lowerable forwarding forms remain parser-visible but unsupported; `typesharp check` and `typesharp build` report `TS2003` for those forms instead of silently emitting incomplete C#.

Canonical pages: [Grammar](../grammar/), [Modules And Imports](../modules/), [Feature Status](../feature-status/)

Human guide: [Modules And Imports](../modules/)

## Expressions

Expressions include literals, identifiers, parenthesized grouping expressions, unary logical-not and numeric sign expressions, same-enum value `|`, `&`, `^`, and unary `~` expressions, calls, member access, indexer access, blocks, value-producing `if`, `match`, lambdas including block-bodied lambdas, collection expressions with array/List spread elements and lambda return contextual array/List targets, iterator `yield` statements and `lock` statements in block bodies, pipeline and composition expressions, `satisfies` proof expressions, `nameof` including unbound generic type-name targets in `nameof` only, `checked(...)`, `unchecked(...)`, async/await, and nominal record expressions with spread fields for the implemented subset. Declarations include explicit-receiver `extension` methods that lower to C# extension methods.

Canonical pages: [Grammar](../grammar/), [Language Tour](../language-tour/), [Type System](../type-system/)

## Types

Type syntax includes primitive names, nullable types, arrays, generic types, function types, structural shapes, structural intersection aliases, limited `keyof`, limited indexed access types, type-level unions, and nominal declarations. Mapped, conditional, template-literal, and utility type syntax remains planned until the evaluator budget, diagnostics, formatter/LSP behavior, and public ABI rules are implemented together.

Canonical pages: [Grammar](../grammar/), [Type System](../type-system/), [C# And CLR Type Model](../csharp-type-model/), [Feature Status](../feature-status/)

Human guide: [Type System](../type-system/)

Detailed C#/.NET reference: [C# And CLR Type Model](../csharp-type-model/)

## Patterns

Pattern syntax supports nominal union, TypeSharp-owned enum, named imported C# enum, `bool`, and local type-level union narrowing paths in the current smoke-tested scope, including local literal-union match arms. Match arms can use `when` guards; the guard is checked in the narrowed arm scope and does not prove exhaustiveness without a later unguarded cover.

Canonical pages: [Grammar](../grammar/), [Type System](../type-system/)

## Interop Syntax

Interop syntax covers imports from C# namespaces, attributes, `ref`/`out`/`in`, named arguments, and capability markers.

Canonical pages: [Grammar](../grammar/), [.NET Interop](../dotnet-interop/), [Project Requirements](../requirements/)

## Name Resolution And Overloads

Name resolution keeps separate namespace, type, value, member, label, type-parameter, and module spaces. Local and pattern bindings outrank parameters, members, enclosing declarations, explicit imports, `open` candidates, project references, metadata symbols, and ambient declarations.

Member lookup prefers instance members, then applicable extension members, then structural proof members, and only then dynamic access inside an explicit `dynamic` boundary.

Overload resolution is nominal-first. Candidate checks include arity, named/optional/default parameters, `params`, `ref`/`out`/`in`, generic arity, generic constraints, receiver compatibility, nullability, type-level union narrowing, and structural proof availability.

Canonical pages: [Grammar](../grammar/), [Modules And Imports](../modules/), [.NET Interop](../dotnet-interop/), [Diagnostics](../diagnostics/)

Detailed C#/.NET reference: [C# Members And Overloads](../csharp-members-overloads/)

## Public ABI Rules

Public API must lower to C#-understandable CLR metadata. Structural shapes, intersection aliases, and type-level unions are compile-time-only and must not leak directly through public boundaries.

Canonical pages:

- [.NET Interop](../dotnet-interop/)
- [C# And CLR Type Model](../csharp-type-model/)
- [C# Members And Overloads](../csharp-members-overloads/)
- [Project Requirements](../requirements/)
- [Advanced Topics](../advanced/)

## Lowering Reference

Implemented TypeSharp features lower to deterministic C# 7.3-compatible source before the generated `net48` project is built. The lowering contract covers generated module containers, records, nominal unions, pattern matching, structural proof erasure, async `Task`, collection expressions, indexers, intrinsics, extension methods, and public ABI rejection for compile-time-only types.

Canonical page: [Lowering](../lowering/)

## Feature Specification Index

This index covers implemented features, stable grammar, MVP features, and features with smoke or fixture evidence. Planned, Preview Watch, Experimental, and Stable Backlog features are tracked in [Feature Status](../feature-status/) and are not treated as completed implementation specs.

Core language feature specs:

| Feature | Canonical Spec | Evidence |
| --- | --- | --- |
| Lexical grammar and tokens | [Grammar](../grammar/), this page | Parser fixtures, TextMate syntax scaffold. |
| Parser precedence and ambiguity | [Grammar](../grammar/), [Project Policy](../project-policy/) | Parser snapshots and recovery fixtures. |
| Module graph, namespace, import/export, `open`, ambient signatures | [Modules And Imports](../modules/), [Grammar](../grammar/) | Parser fixtures, binder/source module diagnostics, backend import/open/alias snapshots, build/run smokes. |
| Functions, values, literals | [Grammar](../grammar/), [Lowering](../lowering/) | Parser fixtures, binder/type checker smokes, generated C# snapshots. |
| Records, classes, interfaces, enums, delegates, nominal unions | [Grammar](../grammar/), [Type System](../type-system/), [Lowering](../lowering/) | Backend snapshots, diagnostics fixtures, generated `net48` build, C# consumer smokes. |
| Type-level unions, structural shapes, intersections, `keyof`, indexed access, `satisfies` | [Type System](../type-system/), [Grammar](../grammar/), [.NET Interop](../dotnet-interop/) | Parser snapshots, type checker fixtures, backend snapshots, generated build smokes. |
| Advanced mapped, conditional, template-literal, and utility type operators | [Type System](../type-system/), [Feature Status](../feature-status/) | Documented evaluator budget only; implementation evidence is required before promotion. |
| `unknown`, nullability, capability boundaries | [Type System](../type-system/), [Diagnostics](../diagnostics/), [.NET Interop](../dotnet-interop/) | `TS2202`, `TS2206`, `TS2207`, `TS2208`, `TS2209`, `TS2404` fixtures and CLI smokes. |
| Pattern matching, pipeline, composition, async, `yield`, `lock`, extension methods, collections, `nameof`, checked/unchecked | [Grammar](../grammar/), [Feature Status](../feature-status/), [Lowering](../lowering/) | Parser snapshots, type checker fixtures, backend snapshots, generated build and C# consumer smokes. |

Interop and tooling feature specs:

| Feature | Canonical Spec | Evidence |
| --- | --- | --- |
| Framework assembly and local DLL references | [CLI](../cli/), [.NET Interop](../dotnet-interop/), [Grammar](../grammar/) | Reference resolver and metadata reader smokes. |
| C# metadata reader, calls, overloads, generics, byref, optional/named arguments | [.NET Interop](../dotnet-interop/), [Diagnostics](../diagnostics/), [Lowering](../lowering/) | Metadata-backed diagnostics and generated `net48` build smokes. |
| Public ABI checker and runtime ABI versioning | [.NET Interop](../dotnet-interop/), [Project Policy](../project-policy/) | Public ABI snapshot smoke and runtime ABI policy review. |
| ASP.NET/WCF/worker-style host compatibility | [Core Goal](../goal/), [Project Requirements](../requirements/), [.NET Interop](../dotnet-interop/) | Host compatibility smoke tests. |
| Project manifest and source discovery | [CLI](../cli/), [Project Configuration](../project-configuration/), [Project Policy](../project-policy/) | Manifest loader, locator, and source discovery smokes. |
| CLI, diagnostics, generated C# emission, VS Code/LSP | [CLI](../cli/), [Diagnostics](../diagnostics/), [Lowering](../lowering/), [VS Code And LSP](../vscode-lsp/) | CLI, backend, generated build, LSP, and extension smokes. |

When adding or promoting a feature, update the relevant canonical spec, [Feature Status](../feature-status/) if status changes, [Project Policy](../project-policy/) review/coverage evidence, and the active task packet.

## Diagnostic Code Index

Use [Diagnostics](../diagnostics/) for the current stable code list and `typesharp explain <CODE>` for command-line explanations.
