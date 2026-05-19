# C# Library Interop

문서 기준일: 2026-05-19

이 문서는 TypeSharp가 기존 C#/.NET Framework 라이브러리를 어떻게 참조하고 호출하며, TypeSharp public API를 C# 소비자에게 어떻게 노출할지 정의한다. 문법 표면은 [grammar/interop.md](grammar/interop.md)를 따르고, 구현 구조는 [architecture.md](architecture.md)의 .NET Interop Layer를 따른다.

## 목표

- 기존 `net48` C# assembly를 TypeSharp 프로젝트에서 참조하고 사용할 수 있어야 한다.
- TypeSharp가 만든 library assembly를 C# .NET Framework 프로젝트가 참조하고 호출할 수 있어야 한다.
- TypeSharp generated assembly와 runtime library는 ASP.NET Web Forms/MVC/Web API, WCF, Windows Service, scheduled job, worker-style host에서 기존 C# class library처럼 참조, 배포, 로드될 수 있어야 한다.
- C# class, interface, struct, enum, delegate, event, attribute, generic type, async `Task` API를 TypeSharp 타입 시스템 안에서 다룰 수 있어야 한다.
- TypeScript식 structural/type-level 기능은 local type checking에 쓰되, C# public metadata boundary에는 그대로 새지 않게 한다.
- unsafe, dynamic, reflection, COM, P/Invoke 같은 불안정한 경계는 명시 capability marker를 요구한다.

## 범위 결정

| 영역 | 상태 | 결정 |
| --- | --- | --- |
| Framework assembly reference | MVP | `System`, `System.Core` 같은 .NET Framework assembly를 manifest에서 참조한다. |
| Local DLL reference | MVP | `paths`로 명시한 `net48` 호환 DLL을 읽는다. |
| NuGet package reference | Stable Backlog | `net48` compatible asset, restore, lock file, license inventory가 필요하다. |
| C# member call | MVP | constructor, method, property, field, indexer, event를 metadata symbol로 바인딩한다. |
| C# overload resolution | MVP | nominal-first ranking을 사용하고 ambiguous call에는 annotation을 요구한다. |
| `ref`/`out`/`in`/`params` | MVP | declaration parameter modifier를 C# metadata와 연결한다. |
| C# extension method instance syntax | Stable Backlog | MVP에서는 static call 또는 명시 helper를 우선한다. |
| `dynamic`/reflection/COM/P/Invoke | MVP opt-in | `dynamic`, `reflect`, `interop`, `unsafe` marker 없이는 strict mode에서 diagnostic이다. `dynamic` type annotation without a `dynamic fun` boundary is reported as `TS2206`; calling a `dynamic fun` from a non-`dynamic` function is reported as `TS2207`; calling `reflect`, `interop`, or `unsafe` functions without the matching marker is reported as `TS2208`. |
| F# specific ABI | Stable Backlog | F# option/record/union compatibility layer는 별도 검토한다. |
| ASP.NET/WCF/worker host compatibility | MVP contract and smoke coverage, Stable Backlog templates/packaging | generated `net48` library와 runtime은 host-specific migration 없이 기존 .NET Framework application model에서 로드 가능해야 한다. ASP.NET Web Forms-style/WCF/worker host smoke는 검증 범위에 포함하고, generated templates와 packaging automation은 별도 단계로 둔다. |

## Project Reference Model

`TypeSharp.toml`의 `[references]`는 source graph가 아니라 외부 .NET metadata graph를 정의한다.

```toml
[references]
assemblies = [
  "System",
  "System.Core",
  "System.Xml"
]

paths = [
  "lib/Legacy.Billing.dll"
]

packages = [
  "Newtonsoft.Json:13.0.3"
]
```

규칙:
- `assemblies`는 target framework reference assembly 또는 GAC/framework assembly 이름이다.
- `paths`는 repository 또는 build environment에 존재하는 명시 DLL 경로다.
- `packages`는 장기 manifest 표면으로 예약하되, 현재 compiler는 NuGet restore를 직접 수행하지 않고 `TS2405`로 build/check를 중단한다.
- package를 허용할 때는 `net48` compatible asset 선택, transitive dependency, license, checksum 또는 lock file 정책을 함께 구현해야 한다.
- 같은 simple name의 assembly가 여러 경로에서 발견되면 manifest가 우선순위를 명시하거나 diagnostic을 낸다.

## .NET Framework Application Model Compatibility

TypeSharp는 기존 .NET Framework host를 대체하지 않고, 그 안에 들어가는 managed library 산출물을 만든다.

규칙:
- ASP.NET Web Forms, ASP.NET MVC/Web API, WCF, Windows Service, scheduled job, queue/background worker 프로젝트는 TypeSharp generated assembly를 일반 `net48` C# class library처럼 참조할 수 있어야 한다.
- TypeSharp runtime library는 ASP.NET `web.config`, `bin` deployment, IIS/AppDomain lifecycle, shadow copy, MSBuild packaging 관례를 깨는 host-specific loader나 startup hook을 기본으로 요구하지 않는다.
- WCF interop는 CLR-visible interface/class/attribute metadata를 통해 service contract, data contract, message contract, proxy/client consumption을 표현하는 방향으로 둔다.
- worker/service interop는 long-running process lifecycle, configuration file, logging/diagnostics integration을 고려하되, host-specific framework package를 TypeSharp runtime의 필수 dependency로 추가하지 않는다.
- Runnable and internal host smokes cover ASP.NET Web Forms-style page references, WCF service/client contract references, worker-style host references, and generated/Core/Runtime DLL reference shape.
- Host-specific project templates, WCF config generation beyond the hand-authored runnable example, IIS packaging, and Windows Service installer scaffolding remain Stable Backlog.

## Import Model

TypeSharp의 `import`는 source module과 .NET namespace/type import를 같은 표면으로 다루되, resolution 단계에서 구분한다.

```typesharp
namespace Samples.CSharpInterop

import { Console, DateTime, StringComparer } from "System"
import { File } from "System.IO"
import { Regex } from "System.Text.RegularExpressions"
import { StringBuilder } from "System.Text"
import type { Task } from "System.Threading.Tasks"
import static System.Math
```

규칙:
- string literal module specifier가 .NET namespace와 일치하면 metadata namespace lookup을 수행한다.
- `import type`은 type-space만 바인딩하며 generated code에 runtime import를 만들지 않는다.
- `import static T`는 C# `using static`에 해당하며 static member 후보를 추가한다.
- `open System`은 허용할 수 있지만 ambiguity diagnostic이 쉬운 explicit import를 권장한다.
- 외부 assembly에 없는 namespace/type을 import하면 reference 누락 또는 오타 diagnostic을 낸다.

## Type Mapping

| TypeSharp | C#/.NET metadata | 규칙 |
| --- | --- | --- |
| `bool` | `System.Boolean` | primitive alias |
| `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | 해당 CLR integral type | overflow 정책은 `checked`/`unchecked` 사양에서 결정 |
| `float`, `double`, `decimal` | `System.Single`, `System.Double`, `System.Decimal` | numeric overload ranking에 사용 |
| `char`, `string`, `object` | `System.Char`, `System.String`, `System.Object` | `string`은 reference-like non-null 기본 |
| `unit` return | `void` | 반환 위치에서는 C# `void`로 낮춘다. |
| `unit` value/generic position | `TypeSharp.Core.Unit` | 값으로 필요한 경우 runtime unit type을 사용한다. |
| `T?` value type | `System.Nullable<T>` | `T: struct`일 때 적용한다. |
| `T?` reference type | nullable reference contract + runtime guard | nullable metadata가 없으면 unknown nullability warning을 낸다. |
| `T[]` | CLR array | C# array와 직접 상호 운용한다. |
| `List<T>`, `Dictionary<K,V>` | BCL generic collection | 명시 import 또는 full name resolution 필요 |
| `A -> B` public type | `System.Func<A,B>` 또는 `System.Action<A>` | 표현 불가능하면 명시 `delegate`를 요구한다. |
| `delegate` | CLR delegate type | event, callback, C# API boundary에 권장 |
| `Task<T>` | `System.Threading.Tasks.Task<T>` | `async fun` public return에 권장 |
| `Option<T>`, `Result<T,E>` | `TypeSharp.Core` nominal union | C# 소비자를 위한 helper API를 안정화해야 한다. |
| type-level union, structural shape | compile-time only | public ABI에 직접 노출하지 않는다. |

## Calling C# APIs

### Constructor, Static Member, Instance Member

```typesharp
import { StringBuilder } from "System.Text"
import { Regex } from "System.Text.RegularExpressions"

fun renderLine(name: string, amount: decimal): string {
  let builder = StringBuilder()
  builder.Append(name)
  builder.Append(": ")
  builder.Append(amount)
  builder.ToString()
}

fun isSlug(value: string): bool =
  Regex.IsMatch(value, "^[a-z0-9-]+$")
```

규칙:
- `TypeName(args)`는 C# constructor call로 해석한다.
- `TypeName.Member`는 static member, `value.Member`는 instance member lookup으로 해석한다.
- property get은 member access로, property set은 assignment로 낮춘다.
- field read는 member access로 낮추고 static/instance field shape은 C# metadata로 index한다.
- indexer는 `value[index]` 형태로 다루고 generated C#의 일반 indexer/array access로 낮춘다.
- generic method call은 명시 type argument 없이 generated C# call site를 보존할 수 있는 경우 C# compiler의 method type inference에 맡긴다. TypeSharp-side generic method inference는 별도 확장이다.
- imported C# interface type은 TypeSharp parameter/return annotation과 public signature에 보존할 수 있다. Imported class-to-interface implementation relation 검증은 metadata relationship indexing 이후로 둔다.

### Named, Optional, Params Argument

```typesharp
fun splitCsv(line: string): string[] =
  line.Split(separator: [","], options: StringSplitOptions.None)
```

규칙:
- C# metadata의 parameter name을 named argument 후보로 사용한다.
- optional parameter는 C# default value metadata가 있을 때만 생략할 수 있다.
- `params` parameter는 배열 전달과 vararg-like 호출을 모두 허용할 수 있다.
- named/optional/params가 overload ambiguity를 만들면 명시 type annotation 또는 full qualification을 요구한다.

### Ref, Out, In Parameter

```typesharp
import { Int32 } from "System"
import { Option, Some, None } from "TypeSharp.Core"

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
- `out` 인자는 addressable `let mut` binding이어야 한다.
- `ref` 인자는 초기화된 mutable binding이어야 한다.
- `in` 인자는 readonly byref로 취급하며 callee가 값을 바꾸지 않는다는 metadata 계약을 따른다.
- lambda capture, async boundary, iterator boundary를 넘는 byref 사용은 diagnostic이다.

### Delegate, Lambda, Event

```typesharp
import { EventHandler } from "System"
import { Button } from "System.Windows.Forms"

fun subscribe(button: Button): unit {
  let handler: EventHandler = (sender, args) => button.Refresh()
  button.Click += handler
}
```

규칙:
- C# delegate parameter에는 compatible lambda 또는 명시 delegate 값을 전달할 수 있다.
- event add/remove는 `+=`와 `-=`로 lowering한다.
- public event type은 명시 `delegate` 또는 외부 CLR delegate type이어야 한다.
- function type이 `Func`/`Action`으로 자연스럽게 낮아지지 않으면 명시 delegate 선언을 요구한다.

## Overload Resolution Policy

MVP ranking은 C# 소비자가 예측하기 쉬운 순서로 둔다.

1. arity와 named argument 일치
2. required/optional/params parameter 일치
3. exact nominal type match
4. nullable compatibility
5. numeric conversion
6. generic inference와 constraint 만족
7. delegate/lambda contextual typing
8. explicit conversion
9. structural proof 또는 type-level union narrowing
10. `dynamic` boundary

규칙:
- nominal match는 structural match보다 우선한다.
- type inference만으로 후보가 안정적으로 하나로 줄지 않으면 diagnostic을 낸다.
- `dynamic` 값이 포함된 type annotation은 명시 `dynamic` marker 또는 compatibility option 없이는 strict mode에서 `TS2206` diagnostic이다.
- `dynamic fun` 호출은 호출 함수에도 `dynamic` marker를 요구하며 marker가 없으면 `TS2207` diagnostic이다.
- `reflect`, `interop`, `unsafe` function 호출은 호출 함수에도 같은 marker를 요구하며 marker가 없으면 `TS2208` diagnostic이다.
- C# extension method instance-call sugar는 ranking 안정화 전까지 Stable Backlog다.

## Nullability

규칙:
- TypeSharp source의 reference-like type은 기본 non-null이다.
- C# nullable annotation이 있는 assembly는 metadata를 읽어 `T`와 `T?`를 구분한다.
- nullable annotation이 없는 C# assembly는 imported reference type을 unknown nullability로 본다.
- unknown nullability 값을 non-null TypeSharp 위치에 넣으면 warning 또는 guard 요구 diagnostic을 낸다.
- public TypeSharp API가 C#으로 나갈 때 nullable metadata emit을 목표로 하되, `net48` compiler/toolchain 제약을 compatibility note에 남긴다.

## Exception Interop

규칙:
- C# exception은 TypeSharp `try`/`catch`에서 CLR exception type으로 잡는다.
- TypeSharp는 `Result<T,E>` 모델링을 권장하지만, 기존 C# API 호출은 exception interop를 유지한다.
- `throws`/`raises` annotation은 문서화와 diagnostics를 위한 실험 기능이며 CLR checked exception을 만들지 않는다.
- C# 호출자를 위한 `Result` to exception, exception to `Result` helper는 `TypeSharp.Interop` 또는 `TypeSharp.Core` extension helper 후보로 둔다.

## Attribute and Metadata

규칙:
- TypeSharp attribute list는 .NET attribute metadata로 emit한다.
- TypeScript decorator처럼 runtime wrapper를 암묵 생성하지 않는다.
- attribute constructor argument는 C# attribute argument로 표현 가능한 constant, enum, `typeof`, array 범위로 제한한다.
- TypeSharp compile-time constant는 `literal` declaration으로 표현하고, public `literal`은 C# 소비자가 상수처럼 볼 수 있는 metadata로 낮춘다.
- assembly/module/type/member/param/return target을 지원한다.
- externally imported C# attribute는 TypeSharp declaration에 그대로 사용할 수 있다.

## Public API Design for C# Consumers

C# 소비자가 호출할 수 있는 TypeSharp library는 다음 규칙을 따른다.

- exported declaration 중 public metadata로 나갈 선언은 explicit type을 우선한다.
- public parameter, return, property, field, event는 nominal .NET type이어야 한다.
- structural shape, anonymous object, type-level union, inferred anonymous function type은 public API에 직접 노출하지 않는다.
- nominal `union`은 reference-type class hierarchy representation으로 시작한다.
- C# 소비자가 union case를 검사하고 생성할 수 있는 generated member와 helper API를 안정화해야 한다.
- public naming은 C# 소비자를 고려해 PascalCase member를 허용하고, TypeSharp formatter/linter가 프로젝트 convention을 검사한다.

## Diagnostics

필수 diagnostic:
- 참조한 assembly 또는 namespace/type을 찾을 수 없음
- 같은 type 이름이 여러 reference에서 발견됨
- C# overload 후보가 모호함
- `out`/`ref` 인자가 addressable mutable value가 아님
- byref 값이 async/lambda/iterator boundary를 넘음
- nullable annotation이 없는 C# API를 strict non-null 위치에 사용함
- `dynamic`, reflection, COM, P/Invoke를 marker 없이 사용함
- public API에 structural shape, type-level union, anonymous object가 노출됨
- `net48`에서 로드할 수 없는 assembly/package를 참조함

## Interop Smoke Tests

MVP 테스트 fixture는 최소 다음 네 가지를 가져야 한다.

1. TypeSharp가 `System`, `System.Core`, `System.IO`, `System.Text.RegularExpressions` API를 호출한다.
2. TypeSharp가 local `net48` C# class library DLL을 참조하고 interface type을 public signature에 보존하며 generic method, event, delegate를 호출한다.
3. C# `net48` console 또는 test project가 TypeSharp library assembly를 참조하고 public API를 호출한다.
4. public boundary diagnostic이 structural shape, type-level union, nullable unknown, dynamic escape를 잡는다.
5. ASP.NET Web Forms-style, WCF service/client, and worker-style `net48` host projects reference generated TypeSharp/Core/Runtime DLLs without requiring TypeSharp-specific host startup hooks.
