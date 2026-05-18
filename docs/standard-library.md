# TypeSharp Standard Library

문서 기준일: 2026-05-18

이 문서는 TypeSharp MVP가 의존하는 표준 라이브러리의 초기 namespace 정책과 핵심 타입을 정의한다. 목표는 예제, compiler-generated code, public API가 같은 런타임 이름을 사용하게 만드는 것이다.

## Namespace 정책

| Namespace | 역할 |
| --- | --- |
| `TypeSharp.Core` | `Option<T>`, `Result<T, E>`, `Unit`, 기본 함수형 helper |
| `TypeSharp.Collections` | `map`, collection helper, sequence helper, immutable/read-only collection adapter |
| `TypeSharp.Runtime` | compiler-generated code가 의존하는 낮은 수준 helper |
| `TypeSharp.Interop` | .NET reflection, dynamic, COM/PInvoke helper |

규칙:
- 사용자 코드 예제는 표준 라이브러리 타입과 값을 명시적으로 import한다.
- compiler-generated code는 `TypeSharp.Runtime` helper를 사용할 수 있지만, 사용자 public API에는 Runtime helper가 직접 노출되지 않아야 한다.
- `TypeSharp.Core`는 작고 안정적으로 유지한다.
- 표준 라이브러리는 `net48`에서 로드되어야 한다.

## Core Types

```typesharp
namespace TypeSharp.Core

public union Option<T> {
  Some(value: T)
  None
}

public union Result<T, E> {
  Ok(value: T)
  Error(error: E)
}

public struct Unit { }
```

규칙:
- `Option<T>`는 값이 없을 수 있음을 타입으로 표현한다.
- `Result<T, E>`는 성공/실패를 타입으로 표현한다.
- `Ok`, `Error`, `Some`, `None`은 union case constructor다.
- `unit` 문법 타입의 lowering은 `void`, `TypeSharp.Core.Unit`, 또는 backend-specific representation 중 하나로 확정해야 한다.
- MVP union lowering은 abstract base class + sealed case class 계열 reference representation을 사용한다.

## Import 원칙

숨은 대형 prelude를 두지 않는다. 예제와 일반 source file은 필요한 core symbol을 명시적으로 가져온다.

```typesharp
import { Result, Ok, Error } from "TypeSharp.Core"
import { Option, Some, None } from "TypeSharp.Core"
```

장기적으로 project manifest에서 `TypeSharp.Core` 자동 참조는 제공할 수 있지만, 이름 import는 명시하는 방향을 기본으로 한다.

## Public ABI

- public API에서 `Option<T>`와 `Result<T, E>`는 `TypeSharp.Core`의 nominal union으로 노출한다.
- type-level union, structural shape, anonymous object는 public API에 직접 노출하지 않는다.
- C# 소비자를 위해 union lowering representation과 helper API를 안정화해야 한다.
- generated nominal union case class는 `TypeSharp.Runtime.ITypeSharpUnionCase`를 구현해 tag, case name, payload metadata를 노출할 수 있다.
- generated code와 C# interop helper는 `TypeSharp.Runtime.TypeSharpUnion`으로 case tag 검사, payload 접근, equality/hash helper를 공유한다.
- generated pattern matching lowering은 `TypeSharp.Runtime.TypeSharpPattern`으로 case predicate와 payload extraction helper를 공유할 수 있다.
- generated record와 union case equality/hash lowering은 `TypeSharp.Runtime.TypeSharpEquality`로 값 비교, sequence 비교, hash composition helper를 공유할 수 있다.

## Collections

초기 public API signature:

```typesharp
namespace TypeSharp.Collections

public fun map<T, U>(items: T[], transform: T -> U): U[]
```

위 코드는 표준 라이브러리 public contract를 설명하는 signature이며, 단독 `.tysh` source file로 컴파일되는 예제는 아니다. 실제 구현은 `TypeSharp.Collections` assembly 안에 둔다.

규칙:
- 기본 array/list literal은 언어 문법으로 제공한다.
- collection helper는 `TypeSharp.Collections`에서 명시 import한다.
- `map`의 `T -> U` parameter는 public ABI에서 `Func<T, U>` 계열로 낮출 수 있어야 한다.
- immutable collection type은 `net48` dependency와 라이선스를 검토한 뒤 확정한다.

## Interop Helpers

`TypeSharp.Interop`는 기존 C#/.NET Framework API와 TypeSharp 타입 안전성 사이의 경계를 명시적으로 다루는 namespace다.

초기 후보:
- nullable/unknown nullability 값을 `Option<T>`로 변환하는 helper
- CLR exception을 `Result<T, E>`로 감싸는 helper
- `Result<T, E>`를 C# 호출자에게 exception 또는 success value로 변환하는 helper
- reflection/dynamic/COM/P/Invoke boundary를 표시하는 낮은 수준 helper
- nominal union representation을 C# 소비자가 검사할 때 쓰는 generated helper

규칙:
- `TypeSharp.Interop` helper는 explicit import 없이는 사용자 코드에 들어오지 않는다.
- strict mode에서 `dynamic`, `reflect`, `interop`, `unsafe` marker가 필요한 작업은 helper 호출도 같은 capability 진단을 따라야 한다.
- public API 안정성이 필요한 helper와 compiler-generated 내부 helper는 분리한다. 내부 helper는 `TypeSharp.Runtime`에 둔다.
- C# library interop의 자세한 계약은 [csharp-interop.md](csharp-interop.md)를 기준으로 한다.
