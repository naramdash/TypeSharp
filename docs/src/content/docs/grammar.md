---
title: Grammar
description: Stable TypeSharp syntax documents and language coverage tracking.
---

This page is the docs canonical grammar ledger. The former repository files under `docs/grammar/` have been removed; grammar updates belong here and in the related canonical reference pages.

For a developer-friendly learning path through the same syntax, use [TypeSharp Syntax Guide](../syntax-guide/). This page stays more ledger-like so it can track parser-visible, bounded, stable, backlog, and rejected syntax precisely.

## Coverage Answer

The docs do not yet explain every TypeSharp grammar form in a beginner-friendly way. They do contain the canonical grammar ledger, reference index, feature status, type-system rules, module rules, lowering rules, and interop rules, but some syntax is still easier to find by reading implementation-ledger prose than by scanning a normal language reference.

This page now uses two layers:

- **Syntax Quick Map** gives a general developer a compact inventory of the current syntax surface.
- **Detailed Rule Summaries** keeps the precise implementation and `net48` lowering boundaries.

When a feature is parser-visible but not stable or not fully lowered, the docs must say so explicitly. Stable syntax should link to examples, fixtures, smoke tests, or a canonical rule page before it is treated as user-facing language behavior.

## Grammar Coverage Plan

Use this plan when improving grammar docs:

1. Keep [Language Tour](../language-tour/) small and example-led.
2. Keep this page as the complete syntax ledger.
3. Keep [Grammar And Language Reference](../reference/) as the scannable index into this page, [Modules And Imports](../modules/), [Type System](../type-system/), [Lowering](../lowering/), and [.NET Interop](../dotnet-interop/).
4. For every syntax form, document whether it is stable, bounded, parser-visible only, backlog, experimental, or rejected.
5. For every public API syntax form, state whether it can lower to C# 7.3-compatible `net48` metadata.
6. For every TypeScript/F#/C# inspired form, document the TypeSharp rule instead of implying source-language compatibility.
7. Add or update parser fixtures, checker fixtures, backend snapshots, C# consumer smokes, or docs contract tests when a syntax form graduates from planned to stable.

## Grammar Goals

TypeSharp grammar has to serve language design, compiler implementation, formatter/LSP tooling, and `.NET Framework 4.8` lowering at the same time.

Required grammar properties:

- Source files are modules by default and participate in an explicit module graph.
- Local inference, structural types, type-level unions, literal types, `unknown` narrowing, and limited compile-time type operators are expressible without making public CLR metadata ambiguous.
- Expression-oriented constructs, immutable binding, nominal closed unions, pattern matching, pipeline, and composition stay central to ordinary TypeSharp code.
- C#/.NET interop constructs such as namespace, class, interface, property, event, delegate, attribute, async, `Task`, partial declarations, extension members, and local DLL references are explicit enough to lower predictably.
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
| Declarations | `let`, `let mut`, `literal`, `fun`, `record`, `class`, `interface`, `enum`, `delegate`, `event`, `union`, `type`, `partial`, explicit-receiver `extension` methods and getter-only extension properties. |
| Types | Predefined types, nullable `T?`, arrays `T[]`, generic types, function types `A -> B`, tuple types, structural shapes, intersections, type-level unions, literal types, limited `keyof`, and limited indexed access `T[K]`. |
| Expressions | Literals, identifiers, parenthesized/grouping expressions, unary logical-not and numeric sign expressions, same-enum value `|`, `&`, `^`, and unary `~` expressions, calls, member/indexer access, blocks, `if`, `match`, lambdas, collection expressions, spread for known arrays/List targets, pipeline, composition, `satisfies`, `nameof`, `checked`, `unchecked`, async/await, nominal record expressions, block-level `yield`, and block-level `lock`. |
| Patterns | Wildcard, literal, type, union-case, tuple, record/list pattern direction, `not`, pattern `&`, pattern `\|`, and match guards in the stable design surface. |
| Interop | C# namespace imports, framework/local DLL references through manifest, attributes, named/optional/params/byref argument shapes, capability markers, and public ABI diagnostics. |

## Syntax Quick Map

This section is the general developer inventory. It answers "what syntax exists?" before the detailed rules explain the exact limits.

### File And Module Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Source file | `Main.tysh` | Files are modules by default. |
| File namespace | `namespace Company.Billing` | Controls generated C# namespace identity. |
| Module declaration | `module Billing.Rules` | Parser-visible module syntax; generated identity must stay deterministic. |
| Named import | `import { Customer } from "./Models"` | Relative source imports and .NET namespace imports are supported in the implemented subset. |
| Type-only import | `import type { Customer } from "./Models"` | Binds type-space names only. |
| Namespace import | `import * as Models from "./Models"` | Creates a module or namespace alias. |
| Import alias | `import { StringBuilder as Builder } from "System.Text"` | Lowers to a C# alias for metadata imports. |
| Static import | `import static System.Math` | Adds static member candidates. |
| Open namespace | `open System.Text` | Lowers to generated C# `using` directives for the supported root-level shape. |
| Local export | `export { helper as publicHelper }` | Exports same-file declarations through explicit names. |
| Re-export | `export { helper } from "./Helper"` | Relative source re-exports are supported for lowerable function/value/type/module surfaces. |
| Star re-export | `export * from "./Helper"` | Forwards the currently lowerable source surface. |
| Ambient declaration | `ambient fun hostValue(): string` | Parsed and checked as external host API shape, omitted from generated C# output. |

### Declaration Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Immutable value | `let name = "Ada"` | Local immutable binding. |
| Mutable value | `let mut count = 0` | Required for assignment to local identifiers. |
| Literal constant | `literal Version = "1.0"` | Compile-time constant spelling; no `const` alias. |
| Function | `fun label(id: int): string = id.ToString()` | Expression or block body. Exported/public APIs should use explicit types. |
| Async function | `async fun load(): Task<string> { ... }` | Lowers to .NET `Task` or `Task<T>`. |
| `params` parameter | `fun join(params parts: string[]): string` | Final-array parameter shape for the implemented direct-call and pipeline subset. |
| Default parameter | `fun page(size: int = 20): int` | Trailing literal defaults only, with documented generic and `params` limits. |
| Record | `public record Customer(Name: string)` | Nominal immutable public data shape. |
| Class | `public class Service { ... }` | MVP public class/member subset only. |
| Interface | `public interface IRule { fun validate(value: string): bool }` | MVP public interface/member subset only. |
| Delegate | `public delegate Transform(value: string): string` | Named CLR delegate shape. |
| Event | `public event Changed: ChangedHandler` | Supported on class/interface MVP member surfaces with named delegate types. |
| Enum | `public enum Permission : byte { Read = 1 }` | Bounded enum declarations, values, aliases, bitwise enum expressions, and match exhaustiveness. |
| Nominal union | `public union Result { Ok(string) Error(string) }` | Closed public domain alternatives that lower to CLR-visible metadata. |
| Type alias | `type Id = string` | Source-level alias; public ABI rules depend on the target type. |
| Structural type alias | `type Named = { Name: string }` | Compile-time-only unless wrapped in a nominal public type. |
| Partial type | `public partial record Customer(Name: string)` | Implemented for generated C# type declarations: modules, records, unions, classes, and interfaces. |
| Extension method | `public extension string { public fun HasPrefix(text: string, prefix: string): bool = text.StartsWith(prefix) }` | Explicit receiver methods lower to C# extension methods for the supported shape. |
| Extension property | `public extension string text { public let WordCount: int = text.Length }` | Getter-only helper-method lowering for the bounded shape. |
| Attribute | `[Obsolete("message")] public record Old()` | Supported on documented declaration/member surfaces; broad target validation is still bounded. |
| Capability marker | `dynamic fun call(value: dynamic): dynamic` | `dynamic`, `reflect`, `interop`, and `unsafe` boundaries must be explicit. |

### Type Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Primitive type | `int`, `string`, `bool`, `decimal` | Maps to CLR-friendly types. |
| Nullable type | `string?`, `int?` | Reference-like nullability contract or `Nullable<T>` for value types. |
| Array | `string[]` | CLR array shape. |
| Generic type | `List<string>` | Supported for named CLR-visible generic shapes and bounded inference. |
| Function type | `string -> int` | Public use must lower to `Func`/`Action` or a named delegate. |
| Tuple type | `(string, int)` | Parser-visible type shape; public ABI support remains bounded by lowering policy. |
| Structural shape | `{ Name: string }` | Local compile-time proof shape. |
| Intersection | `Named & Aged` | Local structural composition only. |
| Type-level union | `string | int` | Local narrowing type; not a direct public CLR signature. |
| Literal type | `"draft"`, `42`, `true` | Used for local narrowing and finite unions. |
| `unknown` | `unknown` | Safe boundary; access requires narrowing or proof. |
| `dynamic` | `dynamic` | Explicit compatibility escape behind capability rules. |
| `keyof` | `keyof Customer` | Limited local key derivation for known records and shapes. |
| Indexed access | `Customer["Name"]` | Limited local member type lookup. |

### Expression Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Literal | `"Ada"`, `1`, `true`, `null` | Literal checking and lowering for supported primitive values. |
| Call | `format(value)` | Direct TypeSharp calls and imported C# calls have separate validation paths. |
| Named argument | `format(value: "Ada")` | Supported for TypeSharp-owned direct calls in the bounded subset and imported C# metadata calls. |
| Member access | `customer.Name` | Nominal metadata/member lookup first, structural proof and extension properties later. |
| Indexer access | `items[0]` | Arrays and imported C# indexers in the supported subset. |
| Null-conditional access | `legacy?.Name`, `legacy?[0]` | Parser-visible; stable semantic support is intentionally bounded to imported C# targets. |
| Block | `{ let x = 1; x }` | Final expression can produce the block value. |
| If expression | `if ok { value } else { fallback }` | Value-producing when branch values are compatible. |
| Match expression | `match state { Draft => "Draft" }` | Exhaustiveness for known bounded domains. |
| Lambda | `text => text.Length` | Target delegate or annotated function type should make public metadata clear. |
| Collection | `[1, 2, 3]` | Stable for known array and `List<T>` targets. |
| Collection spread | `[...left, ...right]` | Known arrays and `List<T>` targets only. |
| Record expression | `{ Name: "Ada" }` | Requires an expected nominal record type. |
| Record spread/update | `{ ...customer, Age: 42 }`, `customer with { Age: 42 }` | Nominal record spread/update in the implemented subset. |
| Pipeline | `value |> normalize` | First-argument pipeline for known TypeSharp-declared targets. |
| Composition | `parse >> format` | Bounded unary function composition. |
| `satisfies` | `customer satisfies Named` | Compile-time proof that erases to the left expression. |
| `nameof` | `nameof(Customer.Name)` | C#-style name reference; unbound generic type names lower to constants. |
| `checked`/`unchecked` | `checked(value + 1)` | C# overflow-context lowering. |
| `await` | `await load()` | Async function body expression shape. |
| `yield` | `yield "Ada"` | Block-level iterator yield with explicit CLR enumerable return. |
| `lock` | `lock gate { ... }` | Block-level monitor lock over a known non-null reference gate. |
| Assignment | `count = count + 1` | Local identifiers require `let mut`; imported C# assignment is metadata-backed. |
| Compound assignment | `count += 1`, `count *= 2`, `count >>>= 1` | Supported only for documented local/imported target and operand subsets. |
| Bitwise/logical unsigned operators | `a | b`, `a << 1`, `a >>> 1` | Bounded enum, primitive integral, and bool rules; no C# 11+ syntax emitted. |

### Pattern Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Wildcard | `_ => value` | Covers remaining known space when allowed. |
| Literal pattern | `"draft" => value` | Stable for local literal unions and supported literal domains. |
| Bool pattern | `true => value` | Exhaustiveness over `bool`. |
| Union case pattern | `Found(customer) => customer.Name` | Nominal union case matching with bounded payload capture. |
| Enum member pattern | `Permission.Read => value` | TypeSharp-owned and named imported C# enum members. |
| Type pattern | `value: string => value` | Local type-level union narrowing. |
| Guard | `Found(c) when c.Age > 0 => c.Name` | Guard checked in narrowed scope; does not prove exhaustiveness by itself. |
| Record/list/tuple pattern direction | `{ Name }`, `[head, ...tail]`, `(a, b)` | Parser/design surface; executable support remains narrower than the grammar direction. |
| Pattern algebra | `not p`, `p & q`, `p | q` | Design surface; richer executable algebra remains backlog. |

### Interop Syntax

| Form | Example | Current Rule |
| --- | --- | --- |
| Manifest assembly reference | `assemblies = ["System"]` | Lives in `TypeSharp.toml`, not source syntax. |
| Manifest DLL path | `paths = ["lib/Legacy.dll"]` | Explicit `net48` compatible local DLL reference. |
| C# namespace import | `import { Regex } from "System.Text.RegularExpressions"` | Metadata namespace lookup through string-literal specifier. |
| Byref argument | `ref value`, `out value`, `in value` | Supported through metadata-backed C# interop checks. |
| Attribute target | `[method: SomeAttribute]` | .NET-style attribute target names are recognized in attribute syntax. |
| Public ABI boundary | `export fun f(value: Customer): Result` | Must lower to C#-understandable CLR metadata. |

## Known Grammar Documentation Gaps

These are documentation gaps, not necessarily implementation gaps:

| Gap | Required Follow-Up |
| --- | --- |
| Some planned parser-visible forms are listed beside stable forms. | Keep "parser/design surface" wording next to forms that are not fully checked/lowered. |
| Class and interface member examples are sparse. | Add more examples when the member-body analysis and public ABI docs settle. |
| Pattern grammar is broader than executable pattern support. | Keep stable match examples focused on union, enum, bool, and local type-level union cases. |
| Advanced type operators are directional only. | Do not document mapped, conditional, template-literal, or utility types as stable until evaluator-budget implementation lands. |
| Null-conditional assignment support is interop-heavy. | Keep examples tied to imported C# metadata targets instead of TypeSharp-owned members. |
| Generated C# shape is sometimes explained away from syntax pages. | Link back to [Lowering](../lowering/) whenever a syntax form's meaning depends on generated `net48` output. |

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

Reserved punctuation and operators include delimiters `()[]{}<>`, member/list separators `. , : ;`, nullable/optional tokens `? ?. ?? ??=`, lambda and function type tokens `=> ->`, pipeline, composition, and logical unsigned shift `|> >> << >>>`, spreads/range-reserved forms `... ..`, equality/relational/logical operators, assignment operators, and attribute/interpolation sigils. Stable parsing is context-sensitive for shared tokens such as `|`, `&`, `?`, `<`, `>`, and `{ ... }`.

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

- `let` is immutable binding; `let mut` is mutable binding. Local identifier assignment requires a mutable binding, and the checker validates known simple assignment, multiplicative compound assignment, bitwise compound assignment, shift assignment, and logical unsigned shift assignment compatibility before generated C# emission.
- `literal` is the compile-time constant spelling. Core grammar does not add `var`, `val`, or `const` aliases.
- `fun` is the public callable declaration form; expression-bodied functions use `=`, and block-bodied functions use `{ ... }`. Direct TypeSharp-owned `fun` declarations can give trailing parameters literal defaults with `name: Type = literal`; supported default literals are string, numeric, `true`, `false`, and `null`.
- `extension ReceiverType { ... }` declares explicit-receiver extension methods. `extension ReceiverType receiverName { public let Name: Type = expr }` additionally supports the bounded getter-only extension property shape; the receiver name is available in the property initializer. Duplicate exact receiver/name extension properties and conflicts with the currently implemented ordinary/structural member precedence report diagnostics before C# emission. Static extension members, setters, operators, imported C# extension property metadata, and C# 14 `extension(...)` block emission remain backlog.
- Simple enum declarations use `enum Name { Member, Other }`; declarations and members can carry .NET attribute lists, members can include explicit integer numeric values such as `Member = 1`, aliases to previously declared members such as `Alias = Member`, or enum initializer-local composite-or forms such as `ReadWrite = Read | Write`, and declarations can use explicit integral underlying types such as `enum Name : byte { Member = 1 }`. Explicit numeric member values and numeric operands must fit the selected underlying type, or `int` when no underlying type is declared. Expression-level same-enum value `|` forms such as `Permission.Read | Permission.Write`, `&` forms such as `permission & Permission.Read`, `^` forms such as `permission ^ Permission.Write`, and unary `~` forms such as `~permission` are supported. Expression-level integral numeric `|`, `&`, `^`, unary `~`, `<<`, `>>`, and `>>>` are supported for known non-null primitive integral operands; shifts promote small left operands to `int` and require a non-null `byte`, `sbyte`, `short`, `ushort`, or `int` count so generated C# remains C# 7.3-compatible. Logical unsigned `>>>` lowers through explicit unsigned casts instead of emitting C# `>>>`. Expression-level boolean `|`, `&`, and `^` are supported for known non-null `bool` operands. Multiplicative compound assignments `*=`, `/=`, and `%=` are supported for mutable local identifier targets, readable/writable imported C# member/indexer targets, and bounded imported null-conditional member/indexer targets whose known non-null primitive integral, floating-point, or decimal operands produce a result assignable back to the target. Local `checked(...)` and `unchecked(...)` wrappers over mutable local multiplicative compound assignments reuse that policy; checked/unchecked imported member/indexer and null-conditional multiplicative assignment targets plus user-defined multiplicative assignment targets remain backlog. Bitwise compound assignments `|=`, `&=`, and `^=` are supported for mutable local identifier targets and existing imported C# assignment targets; shift assignments `<<=` and `>>=` are supported over the same assignment surface when mutable local operands satisfy the primitive integral shift policy. Logical unsigned shift assignment `>>>=` is supported for mutable local primitive integral targets and readable/writable metadata-backed imported C# field/property/indexer targets with explicit assignment/cast lowering instead of emitted C# `>>>` or `>>>=`. Null-conditional assignment is supported for bounded imported C# instance member and indexer targets shaped `receiver?.Member = value` and `receiver?[index] = value`; bounded null-conditional additive compound assignment is supported for imported C# instance member and indexer targets shaped `receiver?.Member += value`, `receiver?.Member -= value`, `receiver?[index] += value`, and `receiver?[index] -= value`. Function-shaped `>>` and `<<` operands remain composition, while invalid value-shaped `>>`, `<<`, or `>>>` operands such as nullable, boolean, string, enum, record, or unsupported count expressions report `TS2214`. Unary boolean complement, parentheses in enum member initializers, arbitrary computed member values, flag-aware match policy, and broad attribute target validation remain planned.
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
- Same-enum value `left | right`, `left & right`, `left ^ right`, and unary `~value` expressions are stable when the operands type-check as enum values and binary operands have the same enum type. Integral numeric `left | right`, `left & right`, `left ^ right`, unary `~value`, `left << count`, `left >> count`, and logical unsigned `left >>> count` expressions are stable for known non-null primitive integral operands. Small left operands promote to `int`; mixed `|`/`&`/`^` operands follow the supported C# numeric promotion rules; shift results follow the left operand's C# 7.3 shift result type and require a non-null `byte`, `sbyte`, `short`, `ushort`, or `int` count. Boolean `left | right`, `left & right`, and `left ^ right` expressions are stable for known non-null `bool` operands and lower to non-short-circuit C# boolean operators. These forms lower to C# `|`, `&`, `^`, `~`, `<<`, ordinary `>>`, or explicit unsigned casts plus `>>` for `>>>`. Multiplicative compound assignments `target *= value`, `target /= value`, and `target %= value` parse and lower for mutable local identifier targets when both operands are known non-null primitive integral numeric values and the promoted result can be assigned back to the target. Bitwise compound assignments `target |= value`, `target &= value`, and `target ^= value` parse and lower to ordinary C# compound assignments. Shift assignments `target <<= count` and `target >>= count` parse and lower to ordinary C# compound assignments when mutable local operands satisfy the primitive integral shift policy. Logical unsigned shift assignment `target >>>= count` parses as one assignment operator and lowers for mutable local primitive integral targets, readable/writable metadata-backed imported C# field/property member targets, and metadata-backed imported C# instance indexer targets with explicit C# 7.3-compatible assignment/cast forms. Null-conditional assignment `receiver?.Member = value` and `receiver?[index] = value` parses through postfix `?.`/`?[]` and lowers only for writable metadata-backed imported C# instance field/property or indexer targets. Bounded null-conditional additive compound assignment `receiver?.Member += value`, `receiver?.Member -= value`, `receiver?[index] += value`, and `receiver?[index] -= value` lowers for readable/writable metadata-backed imported C# instance field/property or indexer targets with supported primitive integral operands. Mutable local identifier targets are checked for `let mut`, known primitive-integral operand/result compatibility for multiplicative compound assignment, known enum/integral/bool operand compatibility for bitwise compound assignment, and known primitive-integral target/count compatibility for shift assignment and logical unsigned shift assignment. Invalid value-shaped `left >> right`, `left << right`, and `left >>> right` operands report `TS2214` instead of lowering as composition or invalid C#. These forms do not enable unary boolean complement, user-defined operators, or flag-aware match reasoning.
- Direct `f(args...)` calls to known TypeSharp-declared functions validate arity and known argument types against the declared parameter list, or report `TS2215`. A final `params name: T[]` parameter accepts either one exact array argument or expanded trailing `T` arguments, including zero trailing arguments after fixed parameters. Trailing defaulted parameters can be omitted from direct calls when every omitted parameter has a supported literal default; generic direct `fun` declarations may use the same defaulted suffix only when the defaulted parameter types are explicit concrete TypeSharp types that do not reference declared type parameters. TypeSharp-owned direct calls may use named arguments after any positional prefix; accepted calls lower as ordinary positional C# calls, and unknown names, duplicates, positional-after-named ordering, missing required parameters, wrong types, or `params` combinations report `TS2215`. Generic named direct calls reuse the direct generic inference/substitution slice when type parameters appear directly as parameter/return types or inside bounded constructed shapes such as arrays and matching single-argument generic wrappers; generic named `params` calls remain unsupported, and concrete optional/default suffixes can still be omitted. Defaulted parameters must form a suffix, cannot combine with `params`, and are not supported when their parameter type references a generic type parameter, or on ambient/`extern` signatures, constructors, delegates, union cases, function types, lambdas, or higher-order values. Direct generic TypeSharp-declared calls support explicit type arguments plus inference/substitution when declared type parameters appear directly as parameter/return types or inside bounded constructed shapes such as arrays and matching single-argument generic wrappers, including the implemented final-array `params` tail when named arguments are not used. Imported C# calls remain on the metadata-backed overload validation path. Function-typed values, higher-order calls, currying, partial application, broader type-constructor unification, and TypeSharp function overload ranking remain backlog.
- Collection expressions can be lambda bodies. Known array or `List<T>` delegate return targets provide the element target for checking and lowering, and homogeneous collection expression arguments can filter imported C# array overloads.
- Lambdas use `=>` and can be assigned to local or module `let` bindings. Lambda bodies can be blocks; when the target delegate expects a value, the final block expression is returned.
- `match` arms use `Pattern => expr` or `Pattern when boolExpr => expr`. Guard expressions are checked in the narrowed arm scope.
- Pipeline `value |> f` and `value |> f(args...)` lower as direct function calls. When the target is a known TypeSharp-declared function, the piped input must be assignable to the target's first parameter type, and the lowered argument count plus non-piped call arguments must fit known target parameters, final `params` tails, trailing defaulted parameters, or supported named non-piped arguments, or the checker reports `TS2215`. A final `params T[]` parameter participates in the lowered call as either an exact array argument or expanded trailing element arguments. Generic TypeSharp pipeline targets use the same bounded direct-call inference/substitution for simple, array, and matching single-argument generic wrapper positions with named non-piped arguments, and also for the implemented `params` tail when named arguments are not used. Higher-order pipeline targets, imported functions, currying, partial application, generic named `params` pipeline calls, and pipeline overload ranking remain backlog.
- Composition `f >> g` and `g << f` lower to unary delegate lambdas when the operands are function-shaped. Known primitive integral value operands use the bounded numeric shift rule above; other value-shaped operands such as `bool`, `string`, nullable primitives, enum values, records, or unsupported shift counts report `TS2214` instead of lowering as composition. Direct named unary function pairs with known TypeSharp signatures are compatibility-checked so the first function's return type must flow into the next function's parameter type; direct TypeSharp generic unary targets use bounded inference/substitution for simple, array, and matching single-argument generic wrapper edges. Explicit unary function-type annotations on direct composition value declarations also validate the annotation input and result against the composed signature before C# emission. Unannotated non-exported top-level direct composition values infer a concrete `System.Func<TInput,TResult>` or `System.Action<TInput>` only when both direct TypeSharp-declared unary targets produce a fully known composed signature; public/exported direct composition values require an explicit function type annotation. Higher-order function values, currying, partial application, broader unannotated composition expression inference, public ABI inference, imported composition inference, broader type-constructor unification, and multi-parameter function composition remain backlog.
- `satisfies` checks a named type or named structural shape proof and erases to the left expression in generated C#.
- Nominal record expressions can use `...` spread from a known nominal record value.
- Collection spread is stable for known arrays and `List<T>` targets.
- Block-level `yield` requires an explicit CLR enumerable return type.
- Block-level `lock` requires a known non-null reference gate and lowers to C# `lock`.
- `nameof`, `checked`, and `unchecked` are compiler intrinsics with C# 7.3-compatible lowering. `nameof(Generic<>)` and higher-arity forms such as `nameof(Pair<,>)` are allowed only as `nameof` targets; they lower to string constants for the simple type name.

### Patterns

Pattern matching borrows from F# union patterns, C# pattern tests, and TypeScript-style narrowing, but the 1.0 executable boundary is smaller than the parser surface.

Stable pattern rules:

- `_` discards, including match arms that cover the remaining known union space.
- Nominal union case names can match union cases, with an optional single identifier payload capture.
- TypeSharp-owned enum members and named imported C# enum members can match enum inputs.
- `true` and `false` can match `bool`; literal members can match literal-only local type-level unions.
- Typed member patterns such as `value: string` can match non-literal local type-level unions.
- Guard syntax uses `when`; guarded arms do not prove nominal, bool, or local type-level union exhaustiveness without a later unguarded arm or discard.
- Nominal closed unions, TypeSharp-owned enums, `bool`, and known local type-level unions, including literal unions, report missing cases/members when the checker can prove non-exhaustiveness.
- Richer pattern algebra, including record/structural patterns, nested payload destructuring, extractor-style patterns, standalone identifier binding patterns, ordinary numeric patterns, numeric enum patterns, flag-style enum reasoning, or-patterns, and pattern conjunction/negation, remains planned beyond the current nominal, TypeSharp-owned enum, named imported C# enum, bool, and local type-level union guard rules.

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

1. Instance or imported metadata members.
2. Structural shape member proof.
3. TypeSharp-authored extension properties with an exact non-null receiver type.
4. Dynamic member access only inside an explicit `dynamic` boundary.

An extension property declaration that would duplicate another TypeSharp-authored exact receiver/name pair or be hidden by the implemented ordinary or structural member steps reports `TS2213` during type checking.

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
| 2 | Postfix: `.`, `?.`, `?[]`, calls, explicit type arguments, indexers, null-forgiving `!`. |
| 3 | Unary: `!`, unary `-`, unary `+`, enum value `~`. |
| 4-9 | Multiplicative, additive, relational, equality, same-enum value `|`/`&`/`^`, logical `&&`, logical `||`. |
| 10 | Record update `with { ... }`. |
| 11 | Pipeline, composition, and logical unsigned shift: `|>`, `>>`, `<<`, `>>>`. |
| 12 | Null coalescing `??`. |
| 13 | `is` pattern. |
| 14 | `satisfies` type proof. |
| 15 | Lambda. |
| 16 | Assignment and compound assignment. |

The implemented null-conditional grammar slice parses postfix member and indexer access so `receiver?.Member = value`, `receiver?[index] = value`, bounded imported C# member/indexer additive, bitwise, and logical unsigned shift compound assignments, and bounded imported C# member/indexer reads can be recognized. Semantic support is intentionally narrower than parsing: increment/decrement, invocation, chains, events, static targets, local binding targets, TypeSharp-owned member/indexer targets, and null-conditional compound assignment beyond the implemented imported member/indexer additive, bitwise, and logical unsigned shift slices remain follow-ups.

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

Control forms such as `if`, `match`, `try`, `using`, async blocks, `yield`, and `lock` use dedicated parse forms instead of infix precedence. Planned operators such as `**`, ranges, `===`, and `!==` are reserved or diagnostic-producing until stabilized; enum member initializer-local `|`, same-enum value expression `|`, `&`, `^`, unary `~`, integral numeric expression `|`, `&`, `^`, unary `~`, bounded integral `<<`/`>>`/`>>>`, boolean expression `|`, `&`, `^`, bitwise compound assignment `|=`, `&=`, `^=`, shift assignment `<<=`, `>>=`, logical unsigned shift assignment `>>>=`, and invalid value-shaped `>>`/`<<`/`>>>` diagnostics are the implemented bitwise-shaped exceptions.

Recovery notes for precedence parsing live in [Project Policy](../project-policy/) parser fixture policy and [Diagnostics](../diagnostics/).

## External Language Coverage

Coverage status answers whether TypeSharp directly supports a feature, provides an equivalent, replaces it with a TypeSharp-native construct, plans it, experiments with it, or rejects it.

| Source | Direct Or Equivalent Coverage Examples | Replacement Or Boundary Examples |
| --- | --- | --- |
| TypeScript | ES module import/export, type-only import/export, ambient declarations, type aliases, interfaces, structural object types, type-level unions, intersections, literal types, narrowing, `unknown`, generics, object/array literals, spread, optional chaining, nullish coalescing, lambdas, `keyof`, indexed access types, `satisfies`, classes, enums, async/await. | Unbounded `any` becomes an explicit compatibility escape, decorators defer to .NET attributes, JSX is rejected as core grammar, advanced mapped/conditional/template-literal and utility types remain planned behind a finite evaluator budget. |
| F# | `let`, `let mut`, `literal`, expression/block-bodied functions, modules, `open`, records, nominal closed unions, option/result modeling, pattern matching, guards, pipeline, composition, iterator-style `yield`, and direct `Task` async interop. | General currying/partial application, computation expressions, active patterns, units of measure, type providers, recursive union ergonomics, and richer member constraints remain backlog or experimental until they have deterministic `net48` lowering and diagnostics. |
| C# | `namespace`, imports, classes, interfaces, structs, enums, delegates, events, properties, attributes, async/await, nullable syntax, literals, named/optional/params/byref parameters, pattern direction, records, collection expressions, extension methods, bounded extension properties, partial declarations, unsafe/dynamic markers, constructors, tuples, `required`/`init`, `lock`, iterator `yield`, local functions, `nameof`, checked/unchecked. | LINQ query syntax maps to pipeline/comprehension direction, source generators remain external build/generator work, C# 14 extension blocks lower through TypeSharp helper methods instead of emitted C# 14 syntax, and preview union types map to TypeSharp nominal unions plus local type-level unions. |

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

