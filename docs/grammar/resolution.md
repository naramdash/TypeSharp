# Name Resolution and Overload Grammar

문서 기준일: 2026-05-18

이 문서는 TypeSharp의 이름 해석과 overload 후보 선택 규칙을 문법 사양 관점에서 정리한다. 실제 overload ranking과 type inference는 타입 체커 사양으로 확장해야 하지만, parser와 binder가 어떤 이름 공간을 만들고 어떤 순서로 탐색하는지는 초기에 고정해야 한다.

## Resolution 목표

- TypeScript처럼 파일별 module graph와 명시 import를 기본으로 한다.
- F#처럼 `open`을 제공하되 이름 오염을 진단 가능하게 한다.
- C#처럼 namespace, type, static member, extension member, overload set을 .NET metadata와 함께 다룬다.
- public boundary에서는 TypeScript식 structural/type-level 표현이 .NET ABI로 직접 새지 않게 한다.

## 이름 공간

```text
namespace-space
type-space
value-space
member-space
label-space
type-parameter-space
module-space
```

규칙:
- `namespace`, `module`, `type`, `class`, `interface`, `record`, `union`, `enum`, `delegate`는 type/module/namespace 이름 공간에 들어간다.
- `let`, `let mut`, `literal`, `fun`은 value 이름 공간에 들어간다.
- class/record/interface member는 member 이름 공간에 들어간다.
- type parameter는 가장 가까운 generic declaration scope에 묶인다.
- 같은 scope의 동일 이름 충돌은 명시 overload가 가능한 declaration을 제외하고 diagnostic이다.

## Scope

```ebnf
scope ::= project_scope
        | source_file_scope
        | namespace_scope
        | module_scope
        | type_scope
        | member_scope
        | block_scope
        | pattern_scope
```

탐색 순서:

1. local/block binding
2. pattern binding
3. parameter
4. member 또는 `this` scope
5. enclosing type/module
6. explicit import
7. `open` namespace/module
8. project references와 .NET metadata
9. ambient declarations

규칙:
- explicit import는 `open`보다 우선한다.
- 동일 우선순위에서 두 symbol이 충돌하면 명시 qualification을 요구한다.
- ambient symbol은 실제 source symbol보다 우선하지 않는다.

## Import와 Open Resolution

```typesharp
import { Customer as DomainCustomer } from "./domain/customer"
import static System.Math
open Company.Product
```

규칙:
- `import { A }`는 file/module export surface에서만 이름을 가져온다.
- `import type`은 type-space만 바인딩하며 runtime dependency를 만들지 않는다.
- `import static T`는 static member lookup 후보를 추가한다.
- `open`은 namespace/module의 member를 후보로 추가하지만 ambiguity diagnostic을 만들 수 있다.

## Member Lookup

```ebnf
member_access ::= expression "." identifier
                | expression "?." identifier
```

후보 순서:

1. instance member
2. applicable extension member
3. structural shape member proof
4. dynamic member access, 단 `dynamic` 또는 capability marker가 있을 때만

규칙:
- 실제 instance member가 extension member보다 우선한다.
- structural shape member는 compile-time proof이며 reflection 기반 호출로 낮추지 않는다.
- `dynamic` member access는 strict mode에서 명시 marker 없이는 diagnostic이다.

## Overload Candidate

```ebnf
call_resolution ::= callee argument_list type_argument_list?
```

초기 ranking 입력:
- arity
- named argument match
- required/optional/default parameter match
- `params` expansion compatibility
- `ref`/`out`/`in` parameter compatibility
- type argument count
- generic constraint satisfiability
- receiver type compatibility
- nullability compatibility
- type-level union narrowing 가능성
- structural shape proof 가능성

정책:
- MVP에서는 C# interop와 예측 가능성을 위해 nominal match를 structural match보다 우선한다.
- overload 후보가 type inference만으로 안정적으로 선택되지 않으면 명시 type annotation을 요구한다.
- `dynamic` overload resolution은 명시 dynamic boundary에서만 허용한다.
- C# extension method instance-call sugar는 feature gate 또는 Stable Backlog로 두고, MVP에서는 static call이 가장 예측 가능한 fallback이다.

## Public Boundary Resolution

exported declaration의 parameter, return type, property type, field type, event type은 public ABI 검사를 통과해야 한다.

진단 대상:
- type-level union이 public metadata에 직접 나타나는 경우
- structural shape type이 public metadata에 직접 나타나는 경우
- anonymous record/object type이 exported API에 나타나는 경우
- `dynamic`이 capability marker 없이 public API에 나타나는 경우
- nullable contract가 .NET metadata로 설명되지 않는 경우

권장 해결:
- nominal closed `union`
- `interface`
- `record`
- explicit wrapper
- explicit `dynamic`/`interop` marker

## Coverage

| 외부 기능 | TypeSharp resolution |
| --- | --- |
| TypeScript module resolution | explicit import/export + manifest resolution |
| TypeScript structural member access | compile-time shape proof |
| TypeScript overload signature | overload candidate + type-level union/type narrowing |
| F# open | `open` 후보 추가, ambiguity diagnostic |
| F# pattern binding | pattern scope |
| C# overload resolution | nominal-first overload ranking |
| C# extension methods | extension member 후보 |
| C# using static | `import static` |
| C# dynamic dispatch | explicit `dynamic` boundary |

