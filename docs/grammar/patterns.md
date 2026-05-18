# Pattern Grammar

문서 기준일: 2026-05-18

Pattern matching은 TypeSharp의 중심 문법이다. F#의 exhaustive match, C#의 pattern matching, TypeScript의 discriminated union narrowing을 하나의 narrowing 모델로 통합한다.

Pattern operator precedence와 associativity는 [precedence.md](precedence.md)를 기준으로 한다.

## Pattern

```ebnf
pattern ::= or_pattern

or_pattern       ::= and_pattern ("|" and_pattern)*
and_pattern      ::= unary_pattern ("&" unary_pattern)*
unary_pattern    ::= discard_pattern
                   | literal_pattern
                   | identifier_pattern
                   | type_pattern
                   | union_case_pattern
                   | tuple_pattern
                   | record_pattern
                   | list_pattern
                   | parenthesized_pattern
                   | "not" unary_pattern
```

## Basic Patterns

```ebnf
discard_pattern    ::= "_"
literal_pattern    ::= literal
identifier_pattern ::= identifier
type_pattern       ::= identifier ":" type
```

예:

```typesharp
match value {
  _ => "anything"
}
```

## Union Case Pattern

```ebnf
union_case_pattern ::= qualified_name pattern_arguments?
pattern_arguments  ::= "(" pattern_list? ")"
pattern_list       ::= pattern ("," pattern)* ","?
```

예:

```typesharp
match result {
  Ok(value) => value
  Error(err) => throw err
}
```

규칙:
- nominal closed union에 대해 exhaustiveness를 진단한다.
- unknown case는 compile-time error다.

## Record and Object Shape Pattern

```ebnf
record_pattern ::= "{" record_pattern_field_list? "}"
record_pattern_field_list ::= record_pattern_field ("," record_pattern_field)* ","?
record_pattern_field ::= identifier ":" pattern
                       | identifier
                       | "..." discard_pattern
```

예:

```typesharp
match customer {
  { name, age: 18 } => name
  { name } => name
}
```

의미:
- nominal record와 structural shape 모두에 사용할 수 있다.
- structural pattern은 TypeScript식 narrowing을 만든다.

## Tuple Pattern

```ebnf
tuple_pattern ::= "(" pattern "," pattern ("," pattern)* ","? ")"
```

## List Pattern

```ebnf
list_pattern ::= "[" list_pattern_items? "]"
list_pattern_items ::= pattern ("," pattern)* ("," spread_pattern)? ","?
spread_pattern ::= "..." pattern
```

Planned Grammar:
- array/list destructuring
- head/tail pattern

## Guard

```ebnf
guard_clause ::= "when" expression
```

예:

```typesharp
match score {
  x when x >= 90 => "A"
  x when x >= 80 => "B"
  _ => "C"
}
```

## `is` Narrowing

```ebnf
is_expression ::= expression "is" pattern
```

예:

```typesharp
if value is string s {
  s.Length
}
```

목표:
- C# `is` pattern과 TypeScript narrowing의 장점을 결합한다.
- pattern binding scope를 명확히 정의해야 한다.

## Exhaustiveness

필수:
- nominal closed union은 exhaustive match를 진단해야 한다.
- bool, enum, literal union은 가능한 범위에서 exhaustive match를 진단한다.
- type-level union은 narrowing 대상이 모두 closed/literal/known structural case일 때만 exhaustive를 보장한다.

진단:
- missing union case
- unreachable arm
- redundant pattern
- impossible type test
- non-exhaustive expression match

## Pattern Coverage

| 외부 기능 | TypeSharp pattern |
| --- | --- |
| F# union case pattern | union case pattern |
| F# guard | `when` |
| F# tuple/list pattern | tuple/list pattern |
| C# type/property pattern | type/record pattern |
| C# `is` pattern | `is` expression |
| TypeScript discriminated union narrowing | record/shape pattern + type-level union narrowing |
| TypeScript `in` narrowing | Planned shape/member existence pattern |

