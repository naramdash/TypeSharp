# Declaration Grammar

문서 기준일: 2026-05-18

이 문서는 TypeSharp의 선언 문법을 정의한다. 목표는 TypeScript의 type alias/interface, F#의 function/record/union, C#의 class/interface/property/event/delegate/attribute/partial/extension 편의 기능을 하나의 일관된 선언 체계로 통합하는 것이다.

## Top-Level Declaration

```ebnf
top_level_declaration ::= block_namespace_declaration
                        | module_declaration
                        | type_declaration
                        | function_declaration
                        | value_declaration
                        | extension_declaration
                        | ambient_declaration
```

## Modifiers

```ebnf
modifier ::= "public" | "private" | "protected" | "internal"
           | "static" | "abstract" | "sealed" | "virtual" | "override"
           | "partial" | "readonly" | "required"
           | "async" | "unsafe" | "dynamic" | "reflect" | "interop" | "extern"
```

규칙:
- modifier 순서는 formatter가 정규화한다.
- `public`은 module export와 .NET public metadata를 모두 고려해야 한다.
- `partial`은 C# interop와 generated code를 위해 지원한다.
- `async`는 `fun` declaration에 붙어 `Task`/`Task<T>` lowering을 요청한다.
- `unsafe`, `dynamic`, `reflect`, `interop`는 capability marker이며 일반 modifier 위치에 둔다.

## Attributes

```ebnf
attribute_list       ::= "[" attribute_target? attribute ("," attribute)* "]"
attribute_target     ::= identifier ":"
attribute            ::= type_name attribute_arguments?
attribute_arguments  ::= "(" argument_list? ")"
```

예:

```typesharp
[Serializable]
public record Customer(id: CustomerId, name: string)

[method: Obsolete("Use NewApi")]
fun OldApi(): unit
```

## Value Declaration

```ebnf
value_declaration ::= attribute_list* modifier* binding_kind binding_list
binding_kind      ::= "let" mutability_marker? | "literal"
mutability_marker ::= "mut"
binding_list      ::= binding ("," binding)* ","?
binding           ::= pattern type_annotation? initializer?
type_annotation   ::= ":" type
initializer       ::= "=" expression
```

의미:
- `let`은 immutable binding이다.
- `let mut`는 mutable binding이다.
- `literal`은 compile-time constant declaration이다.
- `literal` binding은 simple identifier pattern과 initializer를 반드시 가져야 한다.
- lambda expression도 일반 expression이므로 `let` initializer에 사용할 수 있다.
- `let`으로 바인딩한 lambda는 value 이름 공간의 함수값이다.
- module/local helper처럼 값으로 전달되는 함수는 `let name = params => expr` 형태를 사용할 수 있다.
- public .NET API의 callable member는 기본적으로 `fun` declaration을 권장한다. `export let name: A -> B = ...`는 function value/delegate lowering 정책이 정해진 뒤 안정화한다.
- core grammar에는 별도 `var`/`val`/`const` alias를 두지 않는다.
- C# `const` field처럼 metadata literal constant가 필요한 경우도 `literal` declaration으로 표현한다.
- `literal` initializer는 primitive, string, enum, null, 또는 compiler가 constant expression으로 증명할 수 있는 값으로 제한한다.
- public `literal`은 C# 소비자가 상수로 볼 수 있게 metadata literal 또는 equivalent field로 lowering한다.

예:

```typesharp
let answer = 42
let name: string = "Ada"
let mut count = 0
literal MaxRetryCount = 3
public literal ApiVersion = "1.0"
let inc = x => x + 1
let parse: string -> int = text => Int32.Parse(text)
```

## Function Declaration and Signature

```ebnf
function_declaration ::= function_header function_body
function_signature   ::= function_header
function_header      ::= attribute_list* modifier* "fun" identifier type_parameters?
                         parameter_list return_type? effect_clause?

parameter_list       ::= "(" parameter_list_items? ")"
parameter_list_items ::= parameter ("," parameter)* ","?
parameter            ::= attribute_list* parameter_modifier? pattern type_annotation? default_value?
parameter_modifier   ::= "this" | "ref" | "out" | "in" | "params"
default_value        ::= "=" expression
return_type          ::= ":" type
effect_clause        ::= ("throws" | "raises") type
function_body        ::= block | "=" expression
```

규칙:
- 함수 선언의 반환 타입은 다른 타입 주석과 같이 `:`로 표기한다.
- `=`는 expression body를 시작한다.
- `{ ... }`는 block body를 시작한다.
- body 없는 `function_signature`는 interface member, delegate-like declaration, ambient declaration에서만 사용한다.
- `->`는 함수 타입에서만 사용하고 함수 선언에는 사용하지 않는다.
- `=>`는 lambda와 match arm 같은 expression 내부 문법에만 사용한다.
- 일반 effect system은 MVP 문법이 아니며, exception interop는 우선 `throws`/`raises`로만 표기한다.

예:

```typesharp
public fun add(x: int, y: int): int = x + y

public async fun load(path: string): Task<string> {
  using reader = StreamReader(path) {
    return await reader.ReadToEndAsync()
  }
}
```

## Type Declaration

```ebnf
type_declaration ::= class_declaration
                   | interface_declaration
                   | record_declaration
                   | union_declaration
                   | enum_declaration
                   | struct_declaration
                   | delegate_declaration
                   | type_alias_declaration
```

## Type Alias

```ebnf
type_alias_declaration ::= attribute_list* modifier* "type" identifier type_parameters?
                           "=" type
```

예:

```typesharp
type UserId = string
type Primitive = string | int | bool
type JsonObject = { [key: string]: JsonValue }
```

규칙:
- `A | B` alias는 TypeScript식 type-level union이다.
- public API에 type-level union alias가 직접 노출되면 diagnostic을 낸다.

## Nominal Closed Union

```ebnf
union_declaration ::= attribute_list* modifier* "union" identifier type_parameters?
                      union_body

union_body        ::= "{" union_case* "}"
union_case        ::= attribute_list* identifier union_case_payload?
union_case_payload ::= "(" parameter_list_items? ")"
```

예:

```typesharp
public union Validation<T, E> {
  Valid(value: T)
  Invalid(error: E)
}

public union PaymentStatus {
  Pending
  Paid(at: DateTime)
  Failed(reason: string)
}
```

의미:
- F#식 discriminated union을 TypeSharp의 공식 runtime union으로 삼는다.
- exhaustiveness는 `match`에서 진단한다.
- .NET lowering representation은 별도 사양에서 결정한다.

## Record

```ebnf
record_declaration ::= attribute_list* modifier* "record" identifier type_parameters?
                       record_parameters? record_body?
record_parameters ::= "(" parameter_list_items? ")"
record_body       ::= "{" record_member* "}"
```

예:

```typesharp
public record Customer(id: CustomerId, name: string)

record Money {
  amount: decimal
  currency: string
}
```

기본:
- immutable property
- value equality
- copy/update expression

## Class

```ebnf
class_declaration ::= attribute_list* modifier* "class" identifier type_parameters?
                      primary_constructor? base_clause? class_body
primary_constructor ::= "(" parameter_list_items? ")"
base_clause          ::= ":" type_list
class_body           ::= "{" class_member* "}"
```

예:

```typesharp
public class CustomerService(repository: ICustomerRepository) : IDisposable {
  public fun Dispose(): unit { }
}
```

## Interface

```ebnf
interface_declaration ::= attribute_list* modifier* "interface" identifier type_parameters?
                          base_clause? interface_body
interface_body        ::= "{" interface_member* "}"
interface_member      ::= function_signature
                        | interface_property_signature
                        | event_declaration
                        | nested_type_declaration
interface_property_signature ::= attribute_list* modifier* property_kind identifier
                                 type_annotation accessor_signature_block?
accessor_signature_block     ::= "{" accessor_signature* "}"
accessor_signature           ::= attribute_list* modifier* ("get" | "set" | "init")
```

지원:
- function signature
- property signature
- event declaration
- static abstract member는 .NET Framework lowering 제약 때문에 experimental 또는 rejected로 분류한다.

## Struct

```ebnf
struct_declaration ::= attribute_list* modifier* "struct" identifier type_parameters?
                       base_clause? struct_body
struct_body        ::= "{" struct_member* "}"
struct_member      ::= constructor_declaration
                     | method_declaration
                     | property_declaration
                     | field_declaration
                     | event_declaration
                     | nested_type_declaration
```

규칙:
- value type을 표현한다.
- ref struct, readonly ref struct 같은 최신 C# 기능은 .NET Framework/IL 제약을 검토한 뒤 결정한다.

## Enum

```ebnf
enum_declaration ::= attribute_list* modifier* "enum" identifier enum_base? enum_body
enum_base        ::= ":" type
enum_body        ::= "{" enum_member_list? "}"
enum_member_list ::= enum_member ("," enum_member)* ","?
enum_member      ::= attribute_list* identifier ("=" expression)?
```

## Delegate

```ebnf
delegate_declaration ::= attribute_list* modifier* "delegate" identifier type_parameters?
                         parameter_list return_type?
```

예:

```typesharp
public delegate Predicate<T>(value: T): bool
```

## Members

```ebnf
class_member ::= constructor_declaration
               | method_declaration
               | property_declaration
               | event_declaration
               | field_declaration
               | nested_type_declaration

constructor_declaration ::= attribute_list* modifier* "new" parameter_list
                            constructor_initializer? block
constructor_initializer ::= ":" ("base" | "this") argument_list
method_declaration   ::= function_declaration
property_declaration ::= attribute_list* modifier* property_kind identifier type_annotation
                         accessor_block?
property_kind        ::= "let" mutability_marker?
accessor_block       ::= "{" accessor* "}"
accessor             ::= attribute_list* modifier* ("get" | "set" | "init") function_body?
event_declaration    ::= attribute_list* modifier* "event" identifier ":" type
field_declaration    ::= value_declaration
nested_type_declaration ::= type_declaration
```

예:

```typesharp
class Person {
  public let Name: string { get }
  public let mut Age: int {
    get
    private set
  }
  public event Changed: EventHandler
}
```

## Extension Declaration

```ebnf
extension_declaration ::= attribute_list* modifier* "extension" type extension_body
extension_body        ::= "{" extension_member* "}"
extension_member      ::= function_declaration
                        | property_declaration
                        | event_declaration
```

예:

```typesharp
extension string {
  public fun IsBlank(): bool = this.Trim().Length == 0
}
```

의미:
- C# extension member 계열을 TypeSharp식으로 제공한다.
- lowering은 static helper method와 metadata attribute를 사용한다.

## Declaration Coverage

| 외부 기능 | TypeSharp 선언 |
| --- | --- |
| TypeScript type alias | `type` |
| TypeScript interface | `interface`와 structural type |
| F# discriminated union | `union` |
| F# record | `record` |
| F# module value/function | `module` + `let` + `fun` declaration |
| C# class/interface/struct/enum | 동일 declaration |
| C# delegate/event/property | `delegate`/`event`/property declaration |
| C# partial | `partial` modifier |
| C# extension members | `extension` declaration |
| C# attributes | attribute list |

