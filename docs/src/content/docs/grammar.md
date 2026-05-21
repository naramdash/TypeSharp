---
title: Grammar
description: Stable TypeSharp syntax documents and language coverage tracking.
---

This page is the docs canonical grammar ledger established by task `0251-docs-canonical-language-ledger`. The former repository files under `docs/grammar/` have been removed; grammar updates belong here and in the related canonical reference pages.

## Grammar Goals

TypeSharp grammar has to serve language design, compiler implementation, formatter/LSP tooling, and `.NET Framework 4.8` lowering at the same time.

Required grammar properties:

- Source files are modules by default and participate in an explicit module graph.
- Local inference, structural types, type-level unions, literal types, `unknown` narrowing, and limited compile-time type operators are expressible without making public CLR metadata ambiguous.
- Expression-oriented constructs, immutable binding, nominal closed unions, pattern matching, pipeline, and composition stay central to ordinary TypeSharp code.
- C#/.NET interop constructs such as namespace, class, interface, property, event, delegate, attribute, async, `Task`, partial declarations, extension methods, and local DLL references are explicit enough to lower predictably.
- Stable grammar must be parseable by the compiler, useful to TextMate/LSP tooling, and explainable through fixtures or tests.

## Stability Levels

| Level | Meaning |
| --- | --- |
| Stable Grammar | MVP or stable syntax that the compiler should parse and, where implemented, test through fixtures or smoke tests. |
| Planned Grammar | Goal-aligned syntax that still needs implementation detail or sequencing. |
| Experimental Grammar | Syntax that needs a feature gate, security review, or complexity budget. |
| Replacement | External language feature covered by a more consistent TypeSharp form. |
| Rejected Syntax | Syntax intentionally kept out of TypeSharp. |

Stable grammar is not just a design note. It should have syntax examples and parser-readable coverage before it is treated as implemented behavior.

## Design Rules

| Rule | Decision |
| --- | --- |
| Expression-oriented default | `if`, `match`, `try`, and blocks should be able to produce values where semantics allow it. |
| Compile-time and public ABI separation | `A \| B`, structural shapes, intersections, `keyof`, indexed access types, and future computed type-operator results are compile-time tools unless they normalize to CLR-visible metadata or are represented through nominal public declarations. |
| Modules by default | Source files do not become implicit global scripts. Ambient declarations must be explicit. |
| Minimal declaration symbols | Type annotations use `:`, expression-bodied functions use `=`, block bodies use `{ ... }`, function types use `->`, and lambdas/match arms use `=>`. |
| Official spelling over aliases | Immutable binding is `let`, mutable binding is `let mut`, and compile-time constants use `literal` instead of `const`. |
| C# interop is visible | Attribute, namespace, delegate, event, async/Task, and capability boundaries remain visible in syntax and diagnostics. |

## Stable Syntax Surface

| Area | Stable Or Implemented Forms |
| --- | --- |
| Lexical | `.tysh` source files, identifiers, keywords, comments, numeric/string/bool/null literals, and reserved operators. |
| Modules | `namespace`, `module`, `import`, `import type`, namespace imports, named import aliases, `open`, ambient function signatures, local and relative exports. |
| Declarations | `let`, `let mut`, `literal`, `fun`, `record`, `class`, `interface`, `enum`, `delegate`, `event`, `union`, `type`, `partial`, explicit-receiver `extension` methods. |
| Types | Predefined types, nullable `T?`, arrays `T[]`, generic types, function types `A -> B`, tuple types, structural shapes, intersections, type-level unions, literal types, limited `keyof`, and limited indexed access `T[K]`. |
| Expressions | Literals, identifiers, parenthesized/grouping expressions, unary logical-not and numeric sign expressions, same-enum value `|`, `&`, `^`, and unary `~` expressions, calls, member/indexer access, blocks, `if`, `match`, lambdas, collection expressions, spread for known arrays/List targets, pipeline, composition, `satisfies`, `nameof`, `checked`, `unchecked`, async/await, nominal record expressions, block-level `yield`, and block-level `lock`. |
| Patterns | Wildcard, literal, type, union-case, tuple, record/list pattern direction, `not`, pattern `&`, pattern `\|`, and match guards in the stable design surface. |
| Interop | C# namespace imports, framework/local DLL references through manifest, attributes, named/optional/params/byref argument shapes, capability markers, and public ABI diagnostics. |

## Lexical Reference

Source files use UTF-8 by default. A BOM is accepted but generated files should omit it. A shebang is reserved for CLI script mode.

Whitespace and newlines are trivia, not layout-sensitive syntax. Newlines do not create automatic semicolon insertion. The formatter may preserve newline shape for readability, especially in blocks and pipelines.

Comment forms:

- `//` line comment,
- `/* ... */` block comment, with nested block comments preferred by policy,
- `///` line doc comment,
- `/** ... */` doc block comment.

Identifiers may be regular identifiers or backtick-escaped identifiers. Unicode identifiers are allowed by policy, but escaped identifiers should be rare and mainly used for interop with keywords or awkward external names. Public API escaped identifiers are warning candidates.

Reserved keywords include core language, module, declaration, control-flow, type, pattern, interop, and visibility words such as `as`, `async`, `await`, `class`, `delegate`, `export`, `extension`, `fun`, `import`, `interface`, `keyof`, `let`, `literal`, `lock`, `match`, `module`, `namespace`, `open`, `out`, `params`, `public`, `ref`, `return`, `union`, `unsafe`, `using`, `when`, `where`, `with`, and `yield`.

Contextual keywords include words such as `ambient`, `checked`, `dynamic`, `event`, `extern`, `field`, `nameof`, `partial`, `readonly`, `record`, `reflect`, `required`, `throws`, `unknown`, and `unchecked`.

Literal forms include `null`, `true`, `false`, integer literals, floating literals, decimal literals with `m`, char literals, string literals, interpolated strings, and raw string literals. TypeScript-style template literal types remain planned grammar behind the advanced type-operator evaluator budget; they are not stable lexical behavior.

Reserved punctuation and operators include delimiters `()[]{}<>`, member/list separators `. , : ;`, nullable/optional tokens `? ?. ?? ??=`, lambda and function type tokens `=> ->`, pipeline and composition `|> >> <<`, spreads/range-reserved forms `... ..`, equality/relational/logical operators, assignment operators, and attribute/interpolation sigils. Stable parsing is context-sensitive for shared tokens such as `|`, `&`, `?`, `<`, `>`, and `{ ... }`.

The parser preserves leading/trailing trivia, doc comments, source spans, raw literal text, and newline presence for formatter, VS Code highlighting, LSP diagnostics, and go-to-definition.

## Parser Ambiguity And Recovery

Deterministic parser decisions:

- lexer uses longest-token matching, so `|>`, `??=`, `??`, and `?.` win over shorter prefixes,
- parser context chooses whether `|` means type union, pattern alternative, enum member initializer-local composite-or, same-enum value expression-or, integral numeric bitwise-or, or boolean bitwise-or,
- parser context chooses whether `&` means type intersection, pattern-and, same-enum value expression-and, integral numeric bitwise-and, or boolean bitwise-and,
- parser context treats `^` as same-enum value, integral numeric, or boolean bitwise-xor and unary `~` as same-enum value or integral numeric complement,
- `=>` separates lambda bodies in expression context and match arms inside match blocks,
- `->` is only a function type operator; function declarations use `:` for return types,
- `{ ... }` is interpreted by context first, then by contents in expression context; empty braces may stay neutral until semantic binding,
- `[` before declaration headers is attribute syntax; `[` in expression context is collection syntax,
- generic `<...>` in expression context is tentative and must be followed by call/member/indexing/postfix evidence,
- import forms are selected by the first token after `import`: `type`, `static`, `*`, `{`, or identifier,
- `is` is the stable pattern-narrowing operator rather than a general binary expression family,
- attribute targets are recognized only inside attribute syntax.

Recovery rules:

- failed match-arm patterns recover at `=>` or `}`,
- malformed imports recover at newline or declaration boundaries,
- missing operands/types/patterns create missing nodes and resume at a stable delimiter,
- unsupported reserved operators produce diagnostics while keeping skipped-token nodes so later declarations still parse.

## Detailed Rule Summaries

### Modules And Source Units

Every `.tysh` file is a module. Its source-root-relative path is the module identity used for duplicate-module diagnostics, relative imports, relative exports, generated C# container naming, and source graph edges.

Rules:

- Import/open declarations appear after the optional file-scoped namespace and before top-level declarations.
- A file without an explicit namespace lowers under the manifest `rootNamespace`, not the global namespace.
- Relative imports resolve against source module paths. Missing target modules report `TS0112`.
- Unsupported source import or export forms report `TS0113` instead of producing partial generated C#.
- Relative named/type imports, re-exports, and namespace alias member access must refer to exported target names; missing names report `TS0114`.

Use [Modules And Imports](../modules/) for examples and generated container behavior.

### Declarations

Top-level declarations include namespaces, modules, types, functions, values, extension declarations, and ambient declarations.

Stable declaration rules:

- `let` is immutable binding; `let mut` is mutable binding. Local identifier assignment requires a mutable binding, and the checker validates known simple assignment and bitwise compound assignment compatibility before generated C# emission.
- `literal` is the compile-time constant spelling. Core grammar does not add `var`, `val`, or `const` aliases.
- `fun` is the public callable declaration form; expression-bodied functions use `=`, and block-bodied functions use `{ ... }`.
- Simple enum declarations use `enum Name { Member, Other }`; declarations and members can carry .NET attribute lists, members can include explicit integer numeric values such as `Member = 1`, aliases to previously declared members such as `Alias = Member`, or enum initializer-local composite-or forms such as `ReadWrite = Read | Write`, and declarations can use explicit integral underlying types such as `enum Name : byte { Member = 1 }`. Explicit numeric member values and numeric operands must fit the selected underlying type, or `int` when no underlying type is declared. Expression-level same-enum value `|` forms such as `Permission.Read | Permission.Write`, `&` forms such as `permission & Permission.Read`, `^` forms such as `permission ^ Permission.Write`, and unary `~` forms such as `~permission` are supported. Expression-level integral numeric `|`, `&`, `^`, and unary `~` are supported for known non-null primitive integral operands. Expression-level boolean `|`, `&`, and `^` are supported for known non-null `bool` operands. Bitwise compound assignments `|=`, `&=`, and `^=` are supported for mutable local identifier targets and existing imported C# assignment targets. Known value-shaped `>>` and `<<` expressions report a composition/shift ambiguity diagnostic because those tokens are TypeSharp composition syntax. Numeric shifts, shift assignment, unary boolean complement, parentheses in enum member initializers, arbitrary computed member values, flag-aware match policy, and broad attribute target validation remain planned.
- `partial` currently lowers for generated C# type declarations: modules, records, unions, classes, and interfaces.
- `async` belongs on function declarations and lowers through `Task`/`Task<T>`.
- `unsafe`, `dynamic`, `reflect`, and `interop` are capability markers, not ordinary type-system escapes.
- Exported function-valued `let` declarations may use explicit function type annotations for precise generated delegate metadata. Unannotated lambda-valued top-level exports lower with conservative `System.Func<object, TResult>` inference for currently supported lambda bodies, including block final-expression and collection expression returns.

Nominal declarations carry the public ABI surface. Compile-time-only aliases such as type-level unions or structural shapes must not leak through exported declarations.

### Expressions

TypeSharp is expression-oriented, but still exposes imperative statement forms needed for .NET interop and generated C# readability.

Stable expression rules:

- Blocks can return the final expression value; if there is no final expression, the block value is `unit`.
- `if` is a value-producing expression when branch blocks have a compatible final expression; generated C# uses a C# 7.3-compatible helper delegate shape where an expression position needs a block.
- Parenthesized expressions group an inner expression and preserve the inner expression type for checking and lowering.
- Unary logical-not `!expr` is stable for `bool` operands and lowers to C# `!expr`.
- Unary numeric sign expressions `+expr` and `-expr` are stable for supported numeric operands and lower to C# unary sign operators.
- Same-enum value `left | right`, `left & right`, `left ^ right`, and unary `~value` expressions are stable when the operands type-check as enum values and binary operands have the same enum type. Integral numeric `left | right`, `left & right`, `left ^ right`, and unary `~value` expressions are stable for known non-null primitive integral operands, with small integral operands promoting to `int` and mixed integral operands following the supported C# numeric promotion rules. Boolean `left | right`, `left & right`, and `left ^ right` expressions are stable for known non-null `bool` operands and lower to non-short-circuit C# boolean operators. They lower to C# `|`, `&`, `^`, or `~`. Bitwise compound assignments `target |= value`, `target &= value`, and `target ^= value` parse and lower to ordinary C# compound assignments. Mutable local identifier targets are checked for `let mut` and known enum/integral/bool operand compatibility; imported C# member, indexer, static member, and event assignment stays on the metadata-backed interop validation path. Known value-shaped `left >> right` and `left << right` expressions report `TS2201` instead of lowering as composition. They do not enable numeric shifts, shift assignment, unary boolean complement, user-defined operators, or flag-aware match reasoning.
- Direct `f(args...)` calls to known TypeSharp-declared functions validate arity and known argument types against the declared parameter list, or report `TS2201`. Direct generic TypeSharp-declared calls support explicit type arguments and simple inference/substitution when declared type parameters appear directly as parameter or return types. Imported C# calls remain on the metadata-backed overload validation path. Constructed generic parameter inference, generic pipeline/composition inference, optional/default/params TypeSharp parameter policy, function-typed values, higher-order calls, currying, partial application, and TypeSharp function overload ranking remain backlog.
- Collection expressions can be lambda bodies. Known array or `List<T>` delegate return targets provide the element target for checking and lowering, and homogeneous collection expression arguments can filter imported C# array overloads.
- Lambdas use `=>` and can be assigned to local or module `let` bindings. Lambda bodies can be blocks; when the target delegate expects a value, the final block expression is returned.
- `match` arms use `Pattern => expr` or `Pattern when boolExpr => expr`. Guard expressions are checked in the narrowed arm scope.
- Pipeline `value |> f` and `value |> f(args...)` lower as direct function calls. When the target is a known TypeSharp-declared function, the piped input must be assignable to the target's first parameter type, and the lowered argument count plus non-piped call arguments must match known target parameters, or the checker reports `TS2201`. Higher-order pipeline targets, imported functions, generic inference, optional/default/params TypeSharp parameter policy, currying, partial application, and pipeline overload ranking remain backlog.
- Composition `f >> g` and `g << f` lower to unary delegate lambdas. If either side is a known ordinary value such as a numeric, `bool`, `string`, nullable primitive, or enum value, the checker reports `TS2201` because `>>` and `<<` are not numeric shift operators in TypeSharp. Direct named unary function pairs with known TypeSharp signatures are compatibility-checked so the first function's return type must flow into the next function's parameter type. Higher-order function values, currying, partial application, generic/imported composition inference, and multi-parameter function composition remain backlog.
- `satisfies` checks a named type or named structural shape proof and erases to the left expression in generated C#.
- Nominal record expressions can use `...` spread from a known nominal record value.
- Collection spread is stable for known arrays and `List<T>` targets.
- Block-level `yield` requires an explicit CLR enumerable return type.
- Block-level `lock` requires a known non-null reference gate and lowers to C# `lock`.
- `nameof`, `checked`, and `unchecked` are compiler intrinsics with C# 7.3-compatible lowering. `nameof(Generic<>)` and higher-arity forms such as `nameof(Pair<,>)` are allowed only as `nameof` targets; they lower to string constants for the simple type name.

### Patterns

Pattern matching unifies F# union patterns, C# pattern tests, and TypeScript-style narrowing.

Stable pattern rules:

- `_` discards, including match arms that cover the remaining known union space.
- Literal, identifier, type, union-case, tuple, record, and list patterns form the stable design surface.
- Pattern `|` is an or-pattern; pattern `&` combines constraints.
- `not` negates a pattern.
- Guard syntax uses `when`; guarded arms do not prove nominal, bool, or local type-level union exhaustiveness without a later unguarded arm or discard.
- `expr is pattern` introduces narrowing and pattern bindings with scoped lifetime.
- Nominal closed unions, TypeSharp-owned enums, `bool`, and known local type-level unions, including literal unions, report missing cases/members when the checker can prove non-exhaustiveness.
- Richer pattern algebra remains planned beyond the current nominal, TypeSharp-owned enum, named imported C# enum, bool, and local type-level union guard rules.

### Name Resolution

The binder maintains distinct namespace, type, value, member, label, type-parameter, and module spaces.

Lookup order:

1. Local and block bindings.
2. Pattern bindings.
3. Parameters.
4. Member or `this` scope.
5. Enclosing type/module.
6. Explicit imports.
7. `open` namespace/module candidates.
8. Project references and .NET metadata.
9. Ambient declarations.

Explicit imports outrank `open`. Equal-priority conflicts require qualification or report diagnostics. Ambient declarations do not outrank real source symbols.

### Member And Overload Resolution

Member lookup favors ordinary .NET-shaped resolution before dynamic or structural fallbacks:

1. Instance members.
2. Applicable extension members.
3. Structural shape member proof.
4. Dynamic member access only inside an explicit `dynamic` boundary.

Overload candidates consider arity, named arguments, optional/default parameters, `params`, byref modifiers, type argument count, generic constraints, receiver compatibility, nullability, union narrowing, and structural proofs. Nominal metadata matches outrank structural proofs for predictability.

### Interop Syntax

Interop syntax is core grammar because TypeSharp emits and consumes `.NET Framework 4.8` artifacts.

Rules:

- Assembly and local DLL references live in `TypeSharp.toml`, not source syntax.
- Metadata namespace imports use string-literal module specifiers, for example `import { StringBuilder } from "System.Text"`.
- `import type` binds only type-space names.
- `import static` adds C# static member candidates.
- Attribute targets follow .NET target names such as `assembly`, `module`, `type`, `method`, `property`, `field`, `event`, `param`, and `return`.
- Function types in public positions lower only when they can be represented as `Func` or `Action`; otherwise use `delegate`.
- `extern` signatures with `interop` and attributes describe native or host boundaries.
- Public ABI rejects type-level unions, structural shapes, anonymous object shapes, inferred anonymous function types, and marker-free `dynamic`.

## Operator Precedence

Expression precedence, high to low:

| Level | Forms |
| --- | --- |
| 1 | Primary: literals, identifiers, parenthesized grouping, `this`, `base`, tuples, record/object expressions, collections, blocks, `await`, `throw`. |
| 2 | Postfix: `.`, `?.`, calls, explicit type arguments, indexers, null-forgiving `!`. |
| 3 | Unary: `!`, unary `-`, unary `+`, enum value `~`. |
| 4-9 | Multiplicative, additive, relational, equality, same-enum value `|`/`&`/`^`, logical `&&`, logical `||`. |
| 10 | Record update `with { ... }`. |
| 11 | Pipeline and composition: `|>`, `>>`, `<<`. |
| 12 | Null coalescing `??`. |
| 13 | `is` pattern. |
| 14 | `satisfies` type proof. |
| 15 | Lambda. |
| 16 | Assignment and compound assignment. |

Type precedence, high to low:

| Level | Forms |
| --- | --- |
| 1 | Primary types: predefined names, type names, tuples, record shapes, literal types, parenthesized types, `unknown`, `dynamic`, `unit`, `never`. |
| 2 | Generic type arguments. |
| 3 | Postfix nullable `T?`, arrays `T[]`, and indexed access `T[K]`. |
| 4 | Intersection `A & B`. |
| 5 | Type-level union `A | B`. |
| 6 | Right-associative function type `A -> B`. |

Planned advanced type operators such as mapped types, conditional types, template-literal types, and utility-type aliases must stay behind the evaluator budget in [Type System](../type-system/). They should not become stable grammar until the parser, checker, diagnostics, formatter/LSP surface, and public ABI rules land together.

Pattern precedence, high to low:

| Level | Forms |
| --- | --- |
| 1 | Primary patterns: `_`, literals, identifiers, type patterns, union cases, tuples, records, lists, parenthesized patterns. |
| 2 | `not` pattern. |
| 3 | Pattern-and `p & q`. |
| 4 | Pattern-or `p | q`. |

Control forms such as `if`, `match`, `try`, `using`, async blocks, `yield`, and `lock` use dedicated parse forms instead of infix precedence. Planned operators such as `**`, numeric shifts, shift assignment, ranges, `===`, and `!==` are reserved or diagnostic-producing until stabilized; enum member initializer-local `|`, same-enum value expression `|`, `&`, `^`, unary `~`, integral numeric expression `|`, `&`, `^`, unary `~`, boolean expression `|`, `&`, `^`, bitwise compound assignment `|=`, `&=`, `^=`, and value-shaped `>>`/`<<` ambiguity diagnostics are the implemented bitwise-shaped exceptions.

Recovery notes for precedence parsing live in [Project Policy](../project-policy/) parser fixture policy and [Diagnostics](../diagnostics/).

## External Language Coverage

Coverage status answers whether TypeSharp directly supports a feature, provides an equivalent, replaces it with a TypeSharp-native construct, plans it, experiments with it, or rejects it.

| Source | Direct Or Equivalent Coverage Examples | Replacement Or Boundary Examples |
| --- | --- | --- |
| TypeScript | ES module import/export, type-only import/export, ambient declarations, type aliases, interfaces, structural object types, type-level unions, intersections, literal types, narrowing, `unknown`, generics, object/array literals, spread, optional chaining, nullish coalescing, lambdas, `keyof`, indexed access types, `satisfies`, classes, enums, async/await. | Unbounded `any` becomes an explicit compatibility escape, decorators defer to .NET attributes, JSX is rejected as core grammar, advanced mapped/conditional/template-literal and utility types remain planned behind a finite evaluator budget. |
| F# | `let`, `let mut`, `literal`, expression/block-bodied functions, modules, `open`, records, nominal closed unions, option/result modeling, pattern matching, guards, pipeline, composition, iterator-style `yield`, and direct `Task` async interop. | General currying/partial application, computation expressions, active patterns, units of measure, type providers, recursive union ergonomics, and richer member constraints remain backlog or experimental until they have deterministic `net48` lowering and diagnostics. |
| C# | `namespace`, imports, classes, interfaces, structs, enums, delegates, events, properties, attributes, async/await, nullable syntax, literals, named/optional/params/byref parameters, pattern direction, records, collection expressions, extension methods, partial declarations, unsafe/dynamic markers, constructors, tuples, `required`/`init`, `lock`, iterator `yield`, local functions, `nameof`, checked/unchecked. | LINQ query syntax maps to pipeline/comprehension direction, source generators remain external build/generator work, preview union types map to TypeSharp nominal unions plus local type-level unions. |

## Implementation Order

1. Consistency rules and lexical grammar.
2. File/module grammar.
3. Type grammar for primitive, generic, nullable, structural, intersection, and union types.
4. Declaration grammar for functions, records, nominal unions, classes, and interfaces.
5. Expression grammar for blocks, calls, member access, lambdas, match, pipeline, and proof expressions.
6. Pattern grammar for literal, type, union, record, tuple, and narrowing.
7. Interop grammar for attributes, namespaces, delegates, events, async/Task, and capability markers.
8. Name-resolution semantics for imports, `open`, member lookup, overload candidates, and public boundaries.
9. VS Code TextMate grammar and LSP parser integration.

## Parser Evidence

 Stable syntax should be reflected in parser fixtures under `test/fixtures/parser/`. Parser fixture layout, snapshot rules, and current parser coverage policy are canonical in [Project Policy](../project-policy/).

## Former Sources

The old `docs/grammar/**` bridge files were removed after their durable content was folded into docs. Do not recreate them; update [Grammar And Language Reference](../reference/), [Modules And Imports](../modules/), [Type System](../type-system/), [Lowering](../lowering/), or [.NET Interop](../dotnet-interop/) as appropriate.

