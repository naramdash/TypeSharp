# Interop Grammar

문서 기준일: 2026-05-18

TypeSharp는 .NET Framework 4.8과 C#/.NET 생태계 위에서 실행된다. 따라서 interop 문법은 부가 기능이 아니라 언어의 핵심이다. 기존 C# 라이브러리 참조와 public ABI 설계의 세부 계약은 [../csharp-interop.md](../csharp-interop.md)를 기준으로 한다.

## Managed Assembly Reference

C#/.NET 라이브러리 참조는 source file 문법이 아니라 project manifest와 compiler host가 담당한다.

```toml
[references]
assemblies = [
  "System",
  "System.Core"
]
paths = [
  "lib/Legacy.Billing.dll"
]
```

규칙:
- `assemblies`는 framework/GAC/reference assembly 이름이다.
- `paths`는 명시적인 local DLL reference다.
- NuGet package reference는 manifest 표면을 예약하되, restore/lock/dependency inventory 정책이 필요한 Stable Backlog다.
- 참조된 assembly의 metadata는 source symbol과 같은 symbol model로 들어간다.
- `net48`에서 로드할 수 없는 assembly는 build 전에 diagnostic을 낸다.

## Attribute

```ebnf
attribute_list       ::= "[" attribute_target? attribute ("," attribute)* "]"
attribute_target     ::= ("assembly" | "module" | "type" | "method" | "property" | "field" | "event" | "param" | "return") ":"
attribute            ::= type_name attribute_arguments?
```

예:

```typesharp
[Serializable]
public record Customer(id: string)

[assembly: AssemblyVersion("1.0.0.0")]
```

## Namespace and Metadata

```ebnf
namespace_declaration ::= file_scoped_namespace | block_namespace_declaration
```

규칙:
- namespace는 .NET metadata namespace로 lowering한다.
- module은 generated static class 또는 container type으로 lowering한다.
- source file은 file-scoped namespace를 기본으로 사용한다.

## C# Library Import

```typesharp
import { Console, DateTime } from "System"
import { File } from "System.IO"
import { Regex } from "System.Text.RegularExpressions"
import type { Task } from "System.Threading.Tasks"
import static System.Math
```

규칙:
- string literal module specifier가 .NET namespace와 일치하면 metadata namespace lookup을 수행한다.
- `import type`은 type-space만 바인딩한다.
- `import static`은 C# `using static`처럼 static member 후보를 추가한다.
- `open`은 namespace/module 후보를 넓히지만 ambiguity diagnostic 대상이다.
- import가 source module인지 metadata namespace인지는 manifest와 reference resolver가 결정한다.

## Access Modifier

```ebnf
accessibility ::= "public" | "private" | "protected" | "internal"
                | "protected" "internal"
                | "private" "protected"
```

목표:
- C# accessibility와 호환한다.
- module export와 .NET accessibility가 충돌하지 않게 별도 규칙을 둔다.

## Delegate and Event

```ebnf
delegate_declaration ::= attribute_list* modifier* "delegate" identifier type_parameters?
                         parameter_list return_type?
event_declaration    ::= attribute_list* modifier* "event" identifier ":" type
```

예:

```typesharp
public delegate ChangedHandler(sender: object, args: EventArgs): unit
public event Changed: ChangedHandler
```

Function type interop:
- explicit function type `A -> B`는 public parameter/return position에서 `Func<A, B>`로 낮출 수 있다.
- explicit function type `A -> unit`은 public parameter/return position에서 `Action<A>`로 낮출 수 있다.
- `Func`/`Action`으로 표현할 수 없는 경우에는 명시 `delegate` declaration을 요구한다.
- inferred anonymous function type은 public ABI에 직접 노출하지 않는다.

## Async Task Interop

```ebnf
async_function ::= function_declaration
await_expression ::= "await" expression
```

규칙:
- `async_function`은 modifier list에 `async`가 있는 `function_declaration`이다.
- public async function의 반환 타입은 `Task` 또는 `Task<T>`를 명시한다.
- local/private async function은 반환 타입을 생략할 수 있으며 compiler가 `Task` 또는 `Task<T>`를 추론할 수 있다.
- `ValueTask`는 .NET Framework dependency와 interop를 검토한 뒤 Planned Grammar로 둔다.

## Managed Member Interop

C# metadata에서 온 type/member는 TypeSharp member access와 call 문법으로 호출한다.

```typesharp
import { Int32 } from "System"
import { StringBuilder } from "System.Text"
import { Option, Some, None } from "TypeSharp.Core"

fun buildMessage(name: string): string {
  let builder = StringBuilder()
  builder.Append("Hello, ")
  builder.Append(name)
  builder.ToString()
}

fun tryParseInt(text: string): Option<int> {
  let mut value = 0

  if Int32.TryParse(text, out value) {
    Some(value)
  }
  else {
    None
  }
}
```

규칙:
- `TypeName(args)`는 constructor call이다.
- `TypeName.Member`는 static member, `value.Member`는 instance member lookup이다.
- property get/set, field access, indexer call은 .NET metadata member로 바인딩한다.
- C# optional parameter, named argument, `params` parameter는 overload candidate ranking에 들어간다.
- `out` 인자는 addressable `let mut` binding이어야 한다.
- `ref` 인자는 초기화된 mutable binding이어야 한다.
- `in` 인자는 readonly byref로 취급한다.
- byref 값은 async/lambda/iterator boundary를 넘을 수 없다.
- C# extension method의 instance-call sugar는 ranking 규칙이 안정화될 때까지 Stable Backlog로 둔다.

## Type Mapping

| TypeSharp | .NET metadata | 규칙 |
| --- | --- | --- |
| `unit` return | `void` | 반환 위치에서만 `void`로 lowering한다. |
| `unit` value | `TypeSharp.Core.Unit` | generic/value position에서 사용한다. |
| `T?` value type | `Nullable<T>` | `T: struct`일 때 적용한다. |
| `T?` reference type | nullable metadata + guard | nullable metadata가 없으면 unknown nullability로 본다. |
| `A -> B` | `Func<A, B>` 또는 `Action<A>` | public ABI에서 표현 가능한 경우에만 사용한다. |
| `delegate` | CLR delegate | event/callback public ABI에 권장한다. |
| `Task<T>` | `System.Threading.Tasks.Task<T>` | async interop 기본 타입이다. |
| `literal` | metadata literal or constant field | public constant interop에 사용한다. |
| nominal `union` | generated .NET type hierarchy | MVP는 reference-type class hierarchy다. |
| type-level union/shape | metadata 없음 | public boundary에서 diagnostic이다. |

## Overload and Nullability Interop

규칙:
- overload resolution은 nominal match를 structural proof보다 우선한다.
- nullable compatibility와 generic constraint 만족 여부는 overload ranking 입력이다.
- C# nullable annotation이 없는 imported reference type은 unknown nullability로 처리한다.
- unknown nullability 값을 strict non-null 위치에 넣으면 warning 또는 guard 요구 diagnostic을 낸다.
- `dynamic` overload resolution은 명시 `dynamic` boundary에서만 허용한다.

## Capability Marker

```ebnf
capability_modifier ::= "unsafe" | "dynamic" | "reflect" | "interop"
```

의미:
- `unsafe`: pointer, fixed buffer, unmanaged interop.
- `dynamic`: .NET dynamic binder 또는 late binding.
- `reflect`: reflection 기반 access.
- `interop`: COM, P/Invoke, native boundary.

예:

```typesharp
interop dynamic fun callLegacy(name: string): dynamic
unsafe fun copy(src: nativeptr<byte>, dst: nativeptr<byte>, len: int): unit
```

규칙:
- capability marker는 호출자에게 warning 또는 effect로 전파될 수 있다.
- strict mode에서는 marker 없는 escape를 금지한다.
- `dynamic` type annotation은 containing function에 `dynamic` modifier가 없으면 `TS2206`으로 보고한다.
- `dynamic fun`을 non-`dynamic` function에서 직접 호출하거나 pipeline target으로 사용하면 `TS2207`로 보고한다.

## Extern and Native

Extern function은 `extern` modifier가 붙은 `function_signature`로 표현한다. body를 가지지 않는다.

예:

```typesharp
[DllImport("kernel32.dll")]
interop extern fun GetTickCount(): uint
```

규칙:
- `extern`은 function signature modifier로 취급한다.
- native boundary에는 `interop extern fun`을 권장한다.
- managed extern 또는 generated extern 같은 특수 사례는 별도 capability 없이 허용할 수 있지만 문서화해야 한다.

## Partial and Generated Code

```ebnf
partial_declaration ::= "partial" type_declaration
```

목표:
- C# partial type과 source generation interop를 지원한다.
- generated file과 user file의 ownership을 분리한다.

## Extension Interop

```ebnf
extension_declaration ::= "extension" type extension_body
```

lowering:
- static helper method
- extension method metadata attribute
- extension property는 getter/setter method로 lowering

## Public ABI Boundary

Public API에서 금지 또는 변환이 필요한 타입:
- type-level union
- structural shape type
- anonymous record/object type
- inferred anonymous function type
- `dynamic` without marker

대체:
- nominal closed union
- nominal interface
- explicit wrapper
- delegate
- exported record/class

MVP 규칙:
- compiler는 위 타입이 public boundary에 직접 나타나면 diagnostic을 낸다.
- generated wrapper/interface 자동 생성은 Stable Backlog로 둔다.

## Interop Coverage

| C#/.NET 기능 | TypeSharp 문법 |
| --- | --- |
| Attribute | attribute list |
| Namespace | `namespace` |
| Assembly reference | manifest + import |
| Local DLL reference | manifest `paths` |
| NuGet reference | manifest `packages` 후보, Stable Backlog |
| Class/interface/struct/enum | declaration grammar |
| Delegate/event | `delegate`, `event` |
| Property/indexer | property/member grammar |
| Async/Task | `async fun`, `await` |
| Constructor/member call | `Type(args)`, `value.Member`, `Type.Member` |
| Optional/named/params argument | argument list + metadata parameter info |
| `ref`/`out`/`in` | parameter modifier + call-site modifier |
| const field | `literal` declaration |
| P/Invoke | `extern` + attribute + `interop` |
| unsafe | `unsafe` capability marker |
| dynamic | `dynamic` type + marker |
| reflection | `reflect` capability marker |
| partial | `partial` modifier |

