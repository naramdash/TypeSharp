# TypeSharp Formatting Convention

문서 기준일: 2026-05-19

이 문서는 `typesharp format`, VS Code formatter, parser fixtures, examples가 공유할 공식 formatting convention을 정의한다. 현재 CLI formatter MVP는 parser-clean `.tysh` 파일의 줄끝, 후행 공백, 연속 blank line, 최종 newline을 정규화하고, 아래 전체 convention은 AST 기반 formatter가 확장해 나갈 기준이다.

## 목표

- TypeSharp source가 formatter, LSP, analyzer가 다루기 쉬운 안정적인 layout을 갖게 한다.
- [grammar/consistency.md](grammar/consistency.md)의 공통 문법 규칙을 실제 파일 layout으로 고정한다.
- `typesharp format --check`가 canonical whitespace output과 실제 파일을 비교한다.

비목표:
- formatter 구현 세부 알고리즘을 정하지 않는다.
- linter naming policy 전체를 다루지 않는다.
- parser가 허용할 수 있는 모든 recovery syntax를 canonical output으로 만들지 않는다.

## 기본 규칙

- 들여쓰기는 space 2칸이다. tab은 출력하지 않는다.
- formatter는 semicolon을 출력하지 않는다.
- 한 줄에는 하나의 선언, statement, match arm, pipeline stage를 둔다.
- list는 짧으면 한 줄에 두고, 여러 줄이면 요소 하나를 한 줄에 둔다.
- 여러 줄 list의 마지막 요소 뒤에는 trailing comma를 출력하지 않는다.
- 연속 blank line은 하나로 줄인다.
- comment는 의미 있는 다음 node에 붙인다. formatter는 comment를 임의로 멀리 이동하지 않는다.
- parse error가 있는 파일은 rewrite하지 않고 diagnostic을 보고한다.
- formatting은 idempotent해야 한다. 이미 format된 파일을 다시 format해도 diff가 없어야 한다.

## 파일 Layout

파일은 아래 순서를 따른다.

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
- file-scoped `namespace`가 있으면 파일 첫 선언으로 둔다.
- `import`와 `open`은 namespace 다음, 실제 declaration 전에 둔다.
- formatter MVP는 import/open의 상대 순서를 보존한다. 의미가 확정되기 전에는 자동 정렬로 side effect나 shadowing을 만들지 않는다.
- namespace와 import/open group 사이에는 blank line 하나를 둔다.
- import/open group과 첫 declaration 사이에는 blank line 하나를 둔다.
- top-level declaration 사이에는 blank line 하나를 둔다.

## Modifier와 Attribute

Attribute는 대상 declaration 바로 위에 붙인다.

```typesharp
[Serializable]
public record Customer(id: CustomerId, name: string)
```

Modifier는 아래 순서를 canonical order로 둔다.

```text
visibility -> partial -> static/abstract/sealed/virtual/override -> readonly/required -> async/unsafe/dynamic/reflect/interop/extern -> declaration keyword
```

예:

```typesharp
public async fun load(path: string): Task<string> {
  await readAll(path)
}

interop extern fun GetTickCount(): uint
```

규칙:
- `export`는 module public surface marker로 declaration 앞에 둔다.
- capability marker(`unsafe`, `dynamic`, `reflect`, `interop`)는 숨기지 않는다.
- attribute와 대상 declaration 사이에는 blank line을 두지 않는다.

## Declaration Layout

짧은 expression-bodied function은 한 줄 또는 다음 줄 expression으로 둔다.

```typesharp
fun add(x: int, y: int): int = x + y

fun lineTotal(line: OrderLine): decimal =
  line.price * line.quantity
```

Block body는 여는 brace를 declaration line 끝에 둔다.

```typesharp
fun sum(values: decimal[]): decimal {
  let mut total = 0m

  for value in values {
    total = total + value
  }

  total
}
```

Record와 union은 짧으면 한 줄, member가 있으면 block으로 둔다.

```typesharp
export record Receipt(id: string, paidAt: DateTime)

export union PaymentStatus {
  Pending
  Paid(at: DateTime)
  Failed(reason: string)
}
```

규칙:
- final expression은 `return` 없이 쓰는 것을 기본 layout으로 둔다.
- early return이나 C# interop에 필요한 imperative flow에서는 `return`을 보존한다.
- public callable API는 `fun` declaration으로 둔다. function value는 `let name = x => ...`로 둔다.

## Parameter와 Argument

짧은 parameter/argument list는 한 줄에 둔다.

```typesharp
fun rename(id: CustomerId, name: string): Customer
```

줄이 길거나 named argument가 여러 개면 한 줄에 하나씩 둔다.

```typesharp
OrderTotal(
  subtotal: subtotal,
  tax: tax,
  total: subtotal + tax
)
```

규칙:
- comma는 list separator로만 사용한다.
- multi-line list의 closing delimiter는 list 시작 indentation에 맞춘다.
- named argument의 `:` 뒤에는 space 하나를 둔다.

## Expression Layout

### Pipeline

Pipeline은 data flow가 보이도록 subject와 각 stage를 별도 줄에 둔다.

```typesharp
let normalized =
  input
  |> trim
  |> toLower
  |> validate
```

규칙:
- 두 stage 이상이면 multi-line을 기본으로 한다.
- stage expression은 pipeline subject보다 한 level 들여쓴다.
- 오른쪽 stage가 복잡하면 lambda 또는 local `let` binding으로 분리한다.

### Match

`match`는 multi-line block으로 둔다.

```typesharp
match status {
  Pending => "Waiting for payment"
  Paid(at) => $"Paid at {at}"
  Failed(reason) => $"Failed: {reason}"
}
```

규칙:
- arm 하나당 한 줄을 기본으로 한다.
- arm expression이 block이면 다음 줄 block으로 둔다.
- formatter는 match arm 뒤에 comma를 출력하지 않는다.

### If, Try, Using

Control expression은 keyword line 끝에 block을 연다.

```typesharp
if value is string {
  value.Trim()
}
else {
  ""
}
```

`catch`, `finally`, `else`, `elif`는 직전 block의 닫는 brace 다음 줄에 같은 indentation으로 둔다.

## Type과 Structural Shape

짧은 type annotation은 inline으로 둔다.

```typesharp
let normalize: string -> string = text => text.Trim()
```

Structural shape가 길면 member를 한 줄에 하나씩 둔다.

```typesharp
type CustomerShape = {
  id: string
  name: string
  email?: string
}
```

규칙:
- nullable `T?`와 optional member `name?: T`는 붙여 쓴다.
- type-level union이 길면 `|`를 각 줄 앞에 둔다.

```typesharp
type Json =
  string
  | decimal
  | bool
  | null
```

## Collection과 Record Expression

짧은 collection은 inline으로 둔다.

```typesharp
let xs = [1, 2, 3]
```

여러 요소 또는 복잡한 요소는 multi-line으로 둔다.

```typesharp
export let sampleLines: OrderLine[] = [
  OrderLine(sku: "BOOK-001", quantity: 2, price: 12.5m),
  OrderLine(sku: "PEN-002", quantity: 5, price: 1.2m)
]
```

Record update는 짧으면 inline, 여러 field면 multi-line으로 둔다.

```typesharp
let older = customer with {
  age: customer.age + 1
}
```

## Interop와 Public ABI

- C#에서 온 type/member 이름은 원래 casing을 보존한다.
- public .NET API 이름은 C# 소비자가 이해할 수 있는 이름을 허용한다.
- formatter는 public ABI 의미를 바꾸는 rename, reorder, visibility 변경을 하지 않는다.
- attribute, `ref`/`out`/`in`, named argument, capability marker는 원래 의미가 드러나게 보존한다.

## `typesharp format` 계약

초기 formatter는 현재 아래 동작을 제공한다.

- project 또는 path 인자를 받아 `.tysh` 파일만 대상으로 삼는다.
- `--check`는 파일을 쓰지 않고 format diff가 있으면 non-zero exit code를 반환한다.
- parse diagnostics가 있으면 파일을 rewrite하지 않는다.
- source discovery는 [cli.md](cli.md)의 manifest/source discovery 규칙을 따른다.
- diagnostics code와 source span은 CLI와 LSP가 공유하는 compiler model을 사용한다.
- formatter output은 parser가 다시 읽을 수 있어야 한다.
- 현재 rewrite 범위는 LF line ending, trailing whitespace 제거, 연속 blank line 하나로 축소, 최종 newline 보장이다. 선언 재정렬, indentation 재계산, AST 재출력은 후속 formatter 확장이다.
