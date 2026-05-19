# Type Grammar

문서 기준일: 2026-05-18

TypeSharp의 타입 문법은 TypeScript식 유연성과 F#식 안정성, C#/.NET metadata 호환성을 함께 만족해야 한다.

Type operator precedence와 associativity는 [precedence.md](precedence.md)를 기준으로 한다.

## Type

```ebnf
type                 ::= function_type
                       | union_type
                       | intersection_type
                       | nullable_type
                       | primary_type

primary_type         ::= predefined_type
                       | type_name
                       | tuple_type
                       | record_shape_type
                       | array_type
                       | literal_type
                       | parenthesized_type
                       | "unknown"
                       | "dynamic"
                       | "unit"
                       | "never"
```

## Predefined Types

```text
bool byte sbyte short ushort int uint long ulong nativeint unativeint
float double decimal char string object
unit never unknown dynamic
```

의미:
- `unit`은 값 없는 성공을 의미하며 .NET `void`/`Unit` lowering 정책이 필요하다.
- `never`는 도달 불가능 또는 반환하지 않는 expression type이다.
- `unknown`은 안전한 top type이다.
- `dynamic`은 명시적 interop escape hatch다.

## Nominal Type

```ebnf
type_name        ::= qualified_name type_argument_list?
type_argument_list ::= "<" type_argument ("," type_argument)* ","? ">"
type_argument    ::= type
```

예:

```typesharp
List<string>
Dictionary<string, int>
Result<Customer, Error>
```

## Nullable Type

```ebnf
nullable_type ::= primary_type "?"
```

규칙:
- `T?`는 nullable value 또는 nullable reference를 의미한다.
- `T?`와 `Option<T>`의 관계는 lowering/interop 사양에서 정한다.
- public C# interop에서는 nullable metadata가 없는 assembly를 unknown nullability로 본다.

## Type-Level Union

```ebnf
union_type ::= intersection_type ("|" intersection_type)+
```

의미:
- TypeScript식 compile-time type expression이다.
- local inference, literal union, overload 후보, structural narrowing에 사용한다.
- public .NET ABI에 직접 노출하지 않는다.

예:

```typesharp
type Primitive = string | int | bool
type Direction = "left" | "right" | "up" | "down"
```

Public boundary 규칙:
- exported function parameter/return type에 type-level union이 있으면 diagnostic을 낸다.
- 해결책은 `union` declaration, nominal interface, wrapper 중 하나를 사용자가 명시적으로 작성하는 것이다.

## Intersection Type

```ebnf
intersection_type ::= nullable_type ("&" nullable_type)+
```

의미:
- TypeScript식 structural composition을 표현한다.
- MVP에서는 parser와 type representation 후보로만 유지하고, 실제 type checking은 Stable Backlog로 둔다.
- .NET public ABI에 직접 노출하지 않는다.

## Structural Shape Type

```ebnf
record_shape_type ::= "{" shape_member_list? "}"
shape_member_list ::= shape_member ("," shape_member)* ","?
shape_member      ::= property_signature
                    | method_signature
                    | index_signature

property_signature ::= identifier optional_marker? ":" type
method_signature   ::= identifier type_parameters? parameter_list return_type?
index_signature    ::= "[" identifier ":" type "]" ":" type
optional_marker    ::= "?"
```

예:

```typesharp
type Named = { name: string }
type DictionaryLike<T> = { [key: string]: T }
```

규칙:
- structural type은 compile-time shape constraint다.
- MVP public boundary에서는 diagnostic을 내고 interface나 wrapper로 명시적으로 닫게 한다.
- interface/wrapper 자동 생성은 Stable Backlog다.
- row-polymorphic record는 Planned Grammar다.
- `name?: T`는 optional member를 뜻한다.
- `name: T?`는 required member가 nullable value를 가진다는 뜻이다.
- `name?: T?`는 optional member가 존재할 경우 nullable value를 가질 수 있다는 뜻이다.

## Tuple Type

```ebnf
tuple_type ::= "(" tuple_element ("," tuple_element)+ ","? ")"
tuple_element ::= identifier? ":"? type
```

예:

```typesharp
type Point = (x: int, y: int)
type Pair<T> = (T, T)
```

## Array and Collection Types

```ebnf
array_type ::= primary_type "[" "]"
             | "Array" "<" type ">"
             | "List" "<" type ">"
             | "ReadonlyList" "<" type ">"
```

결정:
- `T[]`는 CLR array에 대응한다.
- immutable/read-only collection은 standard library 또는 dependency 정책이 필요하다.

## Function Type

```ebnf
function_type ::= type_parameters? parameter_type_list "->" type
parameter_type_list ::= type | "(" parameter_type_items? ")"
parameter_type_items ::= parameter_type ("," parameter_type)* ","?
parameter_type ::= identifier? ":"? type
```

예:

```typesharp
type Mapper<T, U> = T -> U
type Handler = (sender: object, args: EventArgs) -> unit
```

Public ABI 규칙:
- public parameter/return type에 명시적인 function type이 나타나면 `System.Func<...>` 또는 `System.Action<...>`으로 낮춘다.
- 반환 타입이 `unit`이면 `Action` 계열로 낮추고, 값 반환이면 `Func` 계열로 낮춘다.
- `Func`/`Action`으로 표현할 수 없는 arity, byref, capability가 필요한 함수 타입은 명시 `delegate` 선언을 요구한다.
- inferred anonymous function type은 public boundary에 직접 노출하지 않는다.

## Literal Types

```ebnf
literal_type ::= string_literal | numeric_literal | bool_literal | null_literal
```

예:

```typesharp
type HttpMethod = "GET" | "POST" | "PUT" | "DELETE"
```

## Generic Constraints

```ebnf
where_clause ::= "where" constraint_list
constraint_list ::= constraint ("," constraint)*
constraint ::= type_parameter ":" constraint_item ("+" constraint_item)*
constraint_item ::= type | "class" | "struct" | "notnull" | "new" "(" ")"
```

예:

```typesharp
fun create<T>(): T where T: new()
```

현재 C# 7.3 source backend lowering 범위:
- `class`, `struct`, `new()`, nominal/interface type constraint는 C# `where` clause로 보존한다.
- `notnull`은 TypeSharp 설계 문법에 남겨 두지만 C# 7.3 backend에서는 `TS2205`로 막는다.
- constraint satisfiability, ordering normalization, generic method inference는 후속 작업이다.

## Type Query and Type Operators

Planned Grammar:

```ebnf
type_query      ::= "typeof" expression
keyof_type      ::= "keyof" type
indexed_type    ::= type "[" type "]"
literal_preserve ::= expression "as" "const"
shape_proof     ::= expression "satisfies" type
conditional_type ::= type "extends" type "?" type ":" type
mapped_type     ::= "{" "for" identifier "in" type ":" type "}"
```

정책:
- TypeScript의 타입 레벨 프로그래밍은 강력하지만 complexity budget을 둔다.
- MVP는 literal type, literal union, local structural shape proof에 집중한다.
- `typeof`, `keyof`, indexed access는 Stable Backlog로 둔다.
- `satisfies`는 값을 변환하지 않고 structural proof만 남기는 방향으로 설계한다.
- `as const`는 literal type 보존 annotation으로 설계하되 public ABI에 직접 새지 않게 한다.
- conditional/mapped/template literal type은 Planned 또는 Experimental Grammar다.

## Type Coverage

| 외부 기능 | TypeSharp 타입 문법 |
| --- | --- |
| TypeScript `unknown` | `unknown` |
| TypeScript `any` | 기본 미채택, compatibility mode 후보 |
| TypeScript union | type-level union `A | B` |
| TypeScript intersection | `A & B` planned |
| TypeScript structural object type | record shape type |
| TypeScript literal type | literal type |
| F# option | `Option<T>` 및 nullable/option interop |
| F# tuple | tuple type |
| F# function type | `A -> B` |
| C# nullable | `T?` |
| C# generics/constraints | generic type + `where` |
| C# delegate | function type + delegate declaration |

