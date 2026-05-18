# Syntax Consistency Rules

문서 기준일: 2026-05-18

이 문서는 TypeSharp 문법 표면을 최소화하기 위한 공통 규칙을 정의한다. 새 기능은 먼저 이 규칙으로 표현할 수 있는지 확인하고, 꼭 필요한 경우에만 새 기호나 별도 문법을 추가한다.

## 핵심 원칙

| 의미 | 표기 | 적용 범위 |
| --- | --- | --- |
| 타입 주석 | `:` | parameter, return type, property, event, shape member, tuple label |
| 값 또는 alias 정의 | `=` | `let`, `literal`, `type`, expression-bodied `fun` |
| 블록 body | `{ ... }` | function, class, record body, module, namespace, control expression |
| 함수 타입 | `->` | `T -> U`, `(sender: object, args: EventArgs) -> unit` |
| expression mapping | `=>` | lambda, `match` arm |
| nullable value | `T?` | 값이 `null`일 수 있음을 표시 |
| optional member | `name?: T` | structural shape/object member 존재가 선택적임을 표시 |
| immutable binding | `let` | local, module, property |
| mutable binding | `let mut` | local, module, property |
| compile-time constant | `literal` | constant expression, C# metadata literal interop |

## 선언 규칙

함수 선언은 두 형태만 사용한다.

```typesharp
fun name(params): ReturnType = expression
fun name(params): ReturnType {
  expression
}
```

규칙:
- 반환 타입을 생략할 수 있더라도 표기할 때는 `:`를 사용한다.
- expression body는 `=`를 사용한다.
- block body는 `{ ... }`를 사용한다.
- `->`는 함수 타입에만 사용한다.
- `=>`는 함수 선언에 사용하지 않는다.
- `let name = params => expr`는 함수 선언이 아니라 lambda를 값으로 바인딩하는 문법이다.
- 이름 있는 public callable API는 `fun`을 기본으로 하고, 전달/조합/고차함수용 함수값은 `let` lambda를 기본으로 한다.

## 이름 공간과 import

파일은 다음 순서를 따른다.

```typesharp
namespace Company.Product

import { Thing } from "./thing"
import type { Shape } from "./shape"
open Company.Shared

export fun run(): int {
  0
}
```

규칙:
- source file은 기본적으로 file-scoped namespace를 선호한다.
- `namespace X` 뒤의 선언은 해당 namespace에 속한다.
- `namespace X { ... }` block form은 generated code, interop, nested namespace가 필요할 때만 사용한다.
- import/open은 file-scoped namespace 다음, 실제 선언 전으로 모은다.

## 구분자

규칙:
- declaration, statement, accessor는 newline 또는 block 구조로 구분한다.
- comma는 parameter, argument, tuple, object/record field, collection element처럼 list인 곳에만 사용한다.
- formatter는 semicolon을 출력하지 않는다.
- parser는 interop와 error recovery를 위해 semicolon을 허용할 수 있지만 핵심 문법 예제에는 쓰지 않는다.

## Optional과 Nullable

```typesharp
type HasEmail = { email?: string }
type NullableEmail = { email: string? }
type OptionalNullableEmail = { email?: string? }
```

규칙:
- `email?: string`은 member가 없어도 된다는 뜻이다.
- `email: string?`은 member는 있지만 값이 null일 수 있다는 뜻이다.
- public .NET boundary에서는 optional structural member를 직접 노출하지 않는다.

## Mutability

```typesharp
let name = "Ada"
let mut count = 0
literal MaxRetryCount = 3
```

규칙:
- `let`은 immutable이 기본이다.
- `let mut`는 mutable state를 명시한다.
- `const`는 core grammar에 두지 않는다.
- compile-time constant가 필요한 경우에는 `literal Name = constantExpression`을 사용한다.
- `literal`은 값을 재계산하지 않고 .NET metadata literal 또는 generated compile-time constant로 lowering할 수 있는 expression만 허용한다.
- 별도 `var`/`val` alias는 core grammar에 두지 않는다.

## Interop 예외

Interop 기능도 가능한 한 공통 규칙을 따른다.

```typesharp
[DllImport("kernel32.dll")]
interop extern fun GetTickCount(): uint
```

규칙:
- attribute는 .NET metadata 연결에만 사용하고 TypeScript decorator 역할을 하지 않는다.
- capability는 `interop`, `dynamic`, `reflect`, `unsafe` modifier로 드러낸다.
- public API에는 structural shape와 type-level union을 직접 노출하지 않는다.
