# Expression Grammar

문서 기준일: 2026-05-18

TypeSharp는 expression-oriented 언어를 목표로 한다. C#식 statement 문법은 interop와 명령형 코드 편의를 위해 제공하지만, 가능한 많은 control form이 값을 만들 수 있어야 한다.

Operator precedence와 associativity는 [precedence.md](precedence.md)를 기준으로 한다.

## Expression

```ebnf
expression ::= assignment_expression

assignment_expression ::= conditional_expression
                        | unary_expression assignment_operator expression

conditional_expression ::= is_expression
                         | if_expression
                         | match_expression
                         | try_expression
                         | using_expression

is_expression ::= null_coalescing_expression ("is" pattern)?
null_coalescing_expression ::= pipeline_expression ("??" pipeline_expression)*
pipeline_expression ::= with_expression (("|>" | "<|") with_expression)*
with_expression     ::= binary_expression ("with" record_expression)*
binary_expression   ::= unary_expression (binary_operator unary_expression)*
unary_expression    ::= postfix_expression
                      | unary_operator unary_expression

assignment_operator ::= "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "??="
binary_operator     ::= "==" | "!=" | "<" | "<=" | ">" | ">="
                      | "+" | "-" | "*" | "/" | "%"
                      | "&&" | "||"
unary_operator      ::= "!" | "-" | "+"
```

## Primary Expression

```ebnf
primary_expression ::= literal
                     | identifier
                     | "this"
                     | "base"
                     | parenthesized_expression
                     | tuple_expression
                     | record_expression
                     | object_expression
                     | collection_expression
                     | lambda_expression
                     | block_expression
                     | await_expression
                     | throw_expression

throw_expression   ::= "throw" expression
```

## Block Expression

```ebnf
block_expression ::= "{" block_item* expression? "}"
block_item       ::= declaration | statement | expression_statement
```

규칙:
- block의 마지막 expression이 block value가 된다.
- 마지막 expression이 없으면 `unit`이다.

예:

```typesharp
let total = {
  let tax = price * 0.1m
  price + tax
}
```

## Lambda

```ebnf
lambda_expression ::= parameter_list "=>" expression
                    | identifier "=>" expression
```

의미:
- lambda는 expression이며 `let` initializer로 바인딩할 수 있다.
- `let name = params => expr`는 이름 있는 함수 선언이 아니라 함수값 binding이다.
- 함수값 타입은 `A -> B` 또는 `(x: A) -> B` 형태로 명시할 수 있다.
- parameter type은 contextual typing 또는 `let`의 function type annotation에서 추론할 수 있다.
- public callable API는 기본적으로 `fun`을 사용하고, `let` lambda는 local/module helper와 higher-order value에 우선 사용한다.

예:

```typesharp
let inc = x => x + 1
let add = (x: int, y: int) => x + y
let normalize: string -> string = text => text.Trim().ToLower()
```

## If Expression

```ebnf
if_expression ::= "if" expression block_expression
                  ("elif" expression block_expression)*
                  ("else" block_expression)?
```

규칙:
- `else`가 없는 `if`의 타입은 `unit` 또는 optional control type으로 제한한다.
- expression context에서는 모든 branch가 compatible type이어야 한다.

## Match Expression

```ebnf
match_expression ::= "match" expression "{" match_arm* "}"
match_arm        ::= pattern guard_clause? "=>" expression ","?
guard_clause     ::= "when" expression
```

예:

```typesharp
let label = match result {
  Ok(value) => $"ok: {value}"
  Error(err) => $"error: {err.Message}"
}
```

## Pipeline and Composition

```ebnf
pipeline_expression ::= with_expression (("|>" | "<|") with_expression)*
composition_expression ::= expression ">>" expression
                         | expression "<<" expression
```

예:

```typesharp
let normalized =
  input
  |> trim
  |> toLower
  |> validate
```

정책:
- `|>`는 Stable Grammar 목표다.
- `value |> f`는 `f(value)`로 낮춘다.
- `value |> f(arg1, arg2)`는 `f(value, arg1, arg2)`로 낮춘다.
- 이 규칙은 .NET의 receiver-first API와 TypeSharp collection helper를 자연스럽게 연결하기 위한 MVP 규칙이다.
- pipeline 오른쪽에 복잡한 expression이 오면 명시 lambda 또는 local `let` binding을 권장한다.
- placeholder partial application은 Planned Grammar다.

예:

```typesharp
let totals =
  lines
  |> map(toLineTotal)
```

위 코드는 `map(lines, toLineTotal)`로 낮춘다.

## Member Access and Call

```ebnf
postfix_expression ::= primary_expression postfix_part*
postfix_part       ::= "." identifier
                     | "?." identifier
                     | argument_list
                     | type_argument_list
                     | indexer_argument_list
                     | "!" 
argument_list      ::= "(" argument_list_items? ")"
argument_list_items ::= argument ("," argument)* ","?
argument           ::= identifier ":" expression | expression
```

지원:
- optional chaining `?.`
- null-forgiving `!`
- named argument
- generic type argument
- indexer

## Object and Record Expression

```ebnf
record_expression ::= "{" record_field_list? "}"
record_field_list ::= record_field ("," record_field)* ","?
record_field      ::= identifier ":" expression
                    | identifier

planned_record_field ::= "..." expression
```

예:

```typesharp
let p = { name: "Ada", age: 36 }
let older = p with { age: p.age + 1 }
```

의미:
- TypeScript object literal 편의성과 F# record update를 결합한다.
- target type이 nominal record면 해당 record construction/update로 lowering한다.
- target type이 structural shape면 compile-time shape proof를 만든다.
- field shorthand와 explicit field는 Stable Grammar다.
- `...` spread field는 Planned Grammar다.

## Collection Expression

```ebnf
collection_expression ::= "[" collection_element_list? "]"
collection_element_list ::= collection_element ("," collection_element)* ","?
collection_element ::= expression

planned_collection_element ::= "..." expression
                             | "with" argument_list
```

예:

```typesharp
let xs: int[] = [1, 2, 3]
```

Planned 예:

```typesharp
let ys = [0, ...xs, 4]
let set = [with(StringComparer.OrdinalIgnoreCase), "a", "A"]
```

정책:
- 기본 array/list literal은 Stable Grammar다.
- MVP에서 target type이 없는 homogeneous collection literal은 `T[]`로 추론한다.
- 빈 collection literal `[]`은 contextual type 또는 명시 타입 주석이 필요하다.
- target type이 `List<T>` 또는 다른 collection type이면 해당 target에 맞는 construction으로 lowering한다.
- `...` spread element는 Planned Grammar다.
- C# 15식 `with(...)` collection argument는 Preview Watch다.

## Async and Await

```ebnf
await_expression ::= "await" expression
async_block      ::= "async" block_expression
task_block       ::= "task" block_expression
```

규칙:
- public `async fun`은 `Task` 또는 `Task<T>` 반환 타입을 명시한다.
- local/private `async fun`은 반환 타입 생략 시 `Task` 또는 `Task<T>`로 추론할 수 있다.
- F#식 computation expression 전체는 Planned Grammar다.
- `and!` 스타일 concurrent binding은 Planned Grammar다.

## Try/Using

```ebnf
try_expression ::= "try" block_expression catch_clause* finally_clause?
catch_clause   ::= "catch" pattern guard_clause? block_expression
finally_clause ::= "finally" block_expression

using_expression ::= "using" pattern "=" expression block_expression
```

의미:
- C# exception interop를 유지한다.
- `Result<T, E>` 중심 오류 모델과 함께 사용할 수 있다.

## Imperative Statements

```ebnf
statement ::= while_statement
            | for_statement
            | return_statement
            | break_statement
            | continue_statement
            | throw_statement

while_statement    ::= "while" expression block_expression
for_statement      ::= "for" pattern "in" expression block_expression
return_statement   ::= "return" expression?
break_statement    ::= "break"
continue_statement ::= "continue"
throw_statement    ::= throw_expression
expression_statement ::= expression
```

정책:
- 명령형 statement는 C# 개발자가 필요한 workflow를 작성할 수 있게 제공한다.
- expression-oriented style을 기본 권장한다.

## Planned Intrinsics and Checked Context

Planned Grammar:

```ebnf
nameof_expression   ::= "nameof" "(" name_reference ")"
checked_expression  ::= ("checked" | "unchecked") block_expression
lock_statement      ::= "lock" expression block_expression
yield_statement     ::= "yield" expression
```

정책:
- `nameof`는 C# interop와 refactoring 안전성을 위해 compiler intrinsic으로 둔다.
- `checked`/`unchecked`는 numeric overflow 정책을 명시한다.
- `lock`은 .NET monitor 기반 synchronization으로 낮출 수 있지만 expression-oriented style에서는 제한적으로 사용한다.
- `yield`는 `IEnumerable<T>` lowering 전략이 정해진 뒤 Planned Grammar로 확정한다.

## Expression Coverage

| 외부 기능 | TypeSharp expression |
| --- | --- |
| TypeScript object/array literal | record/collection expression |
| TypeScript optional chaining/nullish coalescing | `?.`, `??` |
| TypeScript arrow function | lambda expression |
| F# pipeline | `|>` |
| F# match | `match` expression |
| F# record update | `with` expression |
| C# async/await | `async fun`, `await` |
| C# object/collection initializer | object/record/collection expression |
| C# using/try/catch | `using`, `try` expression |

