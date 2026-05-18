# Parser Precedence Table

문서 기준일: 2026-05-18

이 문서는 TypeSharp parser가 expression, type, pattern의 operator precedence와 associativity를 고정하기 위한 기준이다. 문법 예제와 parser fixture는 이 표를 따른다.

## 공통 규칙

- 높음에서 낮음 순서로 적용한다.
- Parser는 syntax context를 먼저 고른다. Type context, expression context, pattern context의 같은 token은 서로 다른 precedence table을 사용한다.
- 같은 precedence의 left-associative operator는 왼쪽부터 묶는다.
- 같은 precedence의 right-associative operator는 오른쪽부터 묶는다.
- Non-associative operator는 같은 operator를 괄호 없이 연쇄하면 diagnostic을 낸다.
- Future/Planned operator는 lexer token으로 남길 수 있지만 stable parser에서는 diagnostic 또는 feature-gate diagnostic을 낸다.

## Expression Precedence

높음에서 낮음 순서:

| Level | Form | Associativity | Status | Parser decision |
| --- | --- | --- | --- | --- |
| 1 | primary: literal, identifier, `this`, `base`, tuple, record/object, collection, block, `await`, `throw` | n/a | Stable | Primary expression starts the precedence parser. |
| 2 | postfix: `.`, `?.`, call `(...)`, type arguments `<...>`, indexer `[...]`, null-forgiving `!` | Left | Stable | Repeats while the next token is a postfix start. |
| 3 | unary: `!`, unary `-`, unary `+` | Right | Stable | Unary operand parses at level 3. |
| 4 | multiplicative: `*`, `/`, `%` | Left | Stable | Numeric or overloaded operator candidate. |
| 5 | additive: `+`, `-` | Left | Stable | String concatenation is semantic, not parser-specific. |
| 6 | relational: `<`, `<=`, `>`, `>=` | Non-assoc | Stable | Chained relational forms require explicit `&&` or parentheses. |
| 7 | equality: `==`, `!=` | Non-assoc | Stable | `===` and `!==` are reserved tokens, not Stable Grammar operators. |
| 8 | logical AND: `&&` | Left | Stable | Boolean expression operator. |
| 9 | logical OR: `||` | Left | Stable | Boolean expression operator. |
| 10 | record update: `with { ... }` | Left | Stable | Applies after binary operators and before pipeline. |
| 11 | pipeline: `|>`, `<|` | Left | Stable | `value |> f(args)` lowers as `f(value, args)`. |
| 12 | null coalescing: `??` | Right | Stable | Matches C#-style fallback chaining. |
| 13 | `is` pattern | Non-assoc | Stable | Parses one full pattern after `is`. |
| 14 | lambda: `identifier => expr`, `(params) => expr` | Right | Stable | Lambda body is a full expression. |
| 15 | assignment: `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `??=` | Right | Stable | Left side must be assignable; parser only checks shape. |

Control forms:
- `if`, `match`, `try`, `using`, `async` block, and `task` block start dedicated expression forms and do not participate as infix operators.
- `return`, `break`, `continue`, `while`, `for`, and `yield` are statement forms inside block parsing.

Planned or reserved expression operators:

| Operator | Status | Parser decision |
| --- | --- | --- |
| `>>`, `<<` | Planned composition or shift conflict | Stable parser treats as feature-gated or diagnostic outside explicitly enabled composition grammar. |
| `**` | Planned numeric operator | Token reserved; no Stable Grammar parse as binary expression yet. |
| `&`, `|`, `^`, `~` in expression context | Planned bitwise family | Lexer accepts tokens, stable parser reports unsupported operator diagnostic. |
| `..` | Planned range | Token reserved; no Stable Grammar parse yet. |
| `===`, `!==` | Reserved compatibility tokens | Stable parser reports unsupported operator diagnostic. |

## Type Precedence

높음에서 낮음 순서:

| Level | Form | Associativity | Status | Parser decision |
| --- | --- | --- | --- | --- |
| 1 | primary type: predefined, type name, tuple, record shape, literal type, parenthesized type, `unknown`, `dynamic`, `unit`, `never` | n/a | Stable | Primary type starts the type parser. |
| 2 | type arguments: `Name<T>` | Left | Stable | Only after a type name or qualified name. |
| 3 | postfix: nullable `T?`, array `T[]` | Left | Stable | Repeats after primary or generic type. |
| 4 | intersection: `A & B` | Left | Planned representation | Stable parser may build syntax, type checker can feature-gate semantics. |
| 5 | union: `A | B` | Left, flattened | Stable | Consecutive unions produce one flattened type-level union node. |
| 6 | function type: `A -> B`, `(A, B) -> C` | Right | Stable | `A -> B -> C` parses as `A -> (B -> C)`. |

Rules:
- `T?[]` means array of nullable `T`.
- `T[]?` means nullable array of `T`.
- `A & B | C` parses as `(A & B) | C`.
- `A | B -> C` parses as `(A | B) -> C` only when the left side is explicitly in function parameter position; otherwise use parentheses to avoid ambiguity.
- Public ABI legality is checked after parsing, not by the precedence parser.

Planned type operators:

| Operator/Form | Status | Parser decision |
| --- | --- | --- |
| `keyof T` | Planned | Feature-gated type primary. |
| `typeof expr` | Planned | Feature-gated type primary. |
| `T[K]` indexed access | Planned | Ambiguous with array syntax; requires parser feature gate and lookahead. |
| conditional type `A extends B ? C : D` | Planned | Not part of Stable Grammar precedence table. |
| mapped type `{ for K in T: U }` | Planned | Parsed as feature-gated record-shape variant later. |

## Pattern Precedence

높음에서 낮음 순서:

| Level | Form | Associativity | Status | Parser decision |
| --- | --- | --- | --- | --- |
| 1 | primary pattern: `_`, literal, identifier, type pattern, union case, tuple, record, list, parenthesized pattern | n/a | Stable | Primary pattern starts the pattern parser. |
| 2 | `not` pattern | Right | Stable | `not not p` is allowed and parsed right-associatively. |
| 3 | pattern AND: `p & q` | Left | Stable | Combines narrowing constraints. |
| 4 | pattern OR: `p | q` | Left, flattened | Stable | Consecutive alternatives produce one flattened or-pattern node. |

Rules:
- Guard `when expr` is not part of pattern precedence. It attaches to the completed match arm pattern.
- `Ok(value) | Error(value)` parses as an or-pattern of two union-case patterns.
- `not A | B` parses as `(not A) | B`.
- `A & B | C` parses as `(A & B) | C`.

## Recovery Notes

- Missing right operand after a binary expression operator creates a missing expression node at the current token.
- Missing type after `->`, `|`, `&`, `?`, or `[]` creates a missing type node and resumes at comma, newline declaration boundary, `)`, `]`, `}`, or EOF.
- Missing pattern after `|`, `&`, or `not` creates a missing pattern node and resumes at `=>`, `when`, comma, `}`, or EOF.
- Unsupported reserved operators produce parse diagnostics in stable mode, but parser should continue with a skipped-token node so later declarations are still parsed.
- Incomplete generic type argument lists recover at `>`, `(`, `{`, newline declaration boundary, or EOF.

## Implementation Notes

- Expression parsing should use Pratt or precedence-climbing style so future operators can be inserted without rewriting the grammar.
- Type and pattern parsers can use smaller precedence-climbing loops because their operator sets are narrow.
- Parser fixtures should include at least one expression, type, and pattern fixture for associativity once the parser runner exists.
