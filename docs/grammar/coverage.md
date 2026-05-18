# Grammar Coverage Matrix

문서 기준일: 2026-05-18

이 문서는 TypeScript, F#, C# 기능을 TypeSharp 문법이 어떻게 포괄하거나 대체하는지 추적한다. TypeSharp가 세 언어의 실용 기능을 직접 지원하거나 TypeSharp식 대체 기능으로 설명할 수 있을 때까지 이 문서는 계속 갱신해야 한다.

## 커버리지 원칙

- "모든 기능"은 이름과 표면 문법을 1:1로 복제한다는 뜻이 아니다.
- 사용자가 세 언어에서 기대하는 실용 사용 사례를 TypeSharp에서 직접 작성할 수 있거나, 더 일관된 TypeSharp 기능으로 대체할 수 있어야 한다.
- .NET Framework 4.8로 안정적으로 낮출 수 없거나 TypeSharp 목표와 충돌하는 기능은 Rejected로 명시한다.
- 새로 발견한 C#, F#, TypeScript 기능이 이 표에 없으면 문법 설계가 완료된 것이 아니다.

## 상태

| 상태 | 의미 |
| --- | --- |
| Direct | TypeSharp 문법으로 직접 제공 |
| Equivalent | 다른 문법이 같은 사용 사례를 제공 |
| Replacement | 외부 기능을 더 일관된 TypeSharp 기능으로 대체 |
| Planned | 목표에는 있으나 아직 세부 문법 미완성 |
| Experimental | feature gate 필요 |
| Rejected | 의도적으로 문법에 넣지 않음 |

## TypeScript Coverage

| TypeScript 기능 | TypeSharp 대응 | 상태 | 문서 |
| --- | --- | --- | --- |
| ES module import/export | `import`/`export` | Direct | [modules.md](modules.md) |
| type-only import/export | `import type`/`export type` | Direct | [modules.md](modules.md) |
| ambient declaration | `ambient` | Direct | [modules.md](modules.md) |
| type alias | `type` | Direct | [declarations.md](declarations.md) |
| interface | `interface` + structural type | Direct | [declarations.md](declarations.md), [types.md](types.md) |
| structural object type | record shape type | Direct | [types.md](types.md) |
| union type | type-level union `A | B` | Direct | [types.md](types.md) |
| intersection type | `A & B` parser/type representation first | Planned | [types.md](types.md) |
| literal type | literal type | Direct | [types.md](types.md) |
| narrowing | `is`, `match`, shape pattern | Direct | [patterns.md](patterns.md) |
| `unknown` | `unknown` | Direct | [types.md](types.md) |
| `any` | compatibility-only escape | Replacement | [types.md](types.md) |
| generics | generic type/function | Direct | [types.md](types.md), [declarations.md](declarations.md) |
| mapped type | limited mapped type | Planned | [types.md](types.md) |
| conditional type | limited conditional type | Planned | [types.md](types.md) |
| template literal type | limited type-level string | Experimental | [types.md](types.md) |
| object literal | record/object expression | Direct | [expressions.md](expressions.md) |
| array literal | collection expression | Direct | [expressions.md](expressions.md) |
| spread in array/object | `...` spread element/field | Planned | [expressions.md](expressions.md) |
| optional chaining | `?.` | Direct | [expressions.md](expressions.md) |
| nullish coalescing | `??` | Direct | [expressions.md](expressions.md) |
| arrow function | lambda | Direct | [expressions.md](expressions.md) |
| decorators | .NET attribute first | Replacement | [interop.md](interop.md) |
| namespace legacy syntax | `namespace`, `module`, `ambient` | Replacement | [modules.md](modules.md) |
| optional property | `?` property marker | Direct | [types.md](types.md) |
| readonly property | immutable `let` member, `readonly` modifier | Direct | [declarations.md](declarations.md) |
| index signature | `[key: T]: U` shape member | Direct | [types.md](types.md) |
| indexed access type | `T[K]` limited type operator | Planned | [types.md](types.md) |
| `keyof` | limited `keyof` type operator | Planned | [types.md](types.md) |
| `typeof` type query | limited `typeof` type query | Planned | [types.md](types.md) |
| `satisfies` | shape proof expression/operator | Planned | [types.md](types.md), [patterns.md](patterns.md) |
| `as const` | literal preservation annotation | Planned | [types.md](types.md) |
| class/private fields | class + access modifiers | Direct | [declarations.md](declarations.md) |
| enum | `enum` | Direct | [declarations.md](declarations.md) |
| async/await | `async fun`, `await` with `Task` | Direct | [expressions.md](expressions.md) |
| generator/iterator | `yield` with enumerable lowering | Planned | [expressions.md](expressions.md) |
| JSX | external DSL/generator, not core grammar | Rejected | [interop.md](interop.md) |

## F# Coverage

| F# 기능 | TypeSharp 대응 | 상태 | 문서 |
| --- | --- | --- | --- |
| immutable binding | `let` | Direct | [declarations.md](declarations.md) |
| mutable binding | `let mut` | Direct | [declarations.md](declarations.md) |
| compile-time constant | `literal` declaration | Direct | [declarations.md](declarations.md), [interop.md](interop.md) |
| function | `fun` declaration and `let`-bound `=>` lambda | Direct | [declarations.md](declarations.md), [expressions.md](expressions.md) |
| module | `module` | Direct | [modules.md](modules.md) |
| open | `open` | Direct | [modules.md](modules.md) |
| record | `record` | Direct | [declarations.md](declarations.md) |
| discriminated union | nominal closed `union` | Direct | [declarations.md](declarations.md) |
| option | `Option<T>` | Direct | [types.md](types.md) |
| result | `Result<T, E>` | Direct | [types.md](types.md) |
| pattern matching | `match` | Direct | [patterns.md](patterns.md) |
| guards | `when` | Direct | [patterns.md](patterns.md) |
| pipeline | `|>` | Direct | [expressions.md](expressions.md) |
| composition | `>>`, `<<` | Planned | [expressions.md](expressions.md) |
| computation expression | `async`/`task` block first | Planned | [expressions.md](expressions.md) |
| `and!` concurrent binding | task concurrent binding | Planned | [expressions.md](expressions.md) |
| type provider | sandboxed schema import/generator only | Experimental | [interop.md](interop.md) |
| units of measure | type-level annotation | Planned | [types.md](types.md) |
| active patterns | pattern helper or extractor pattern | Planned | [patterns.md](patterns.md) |
| sequence expression | collection/iterator expression | Planned | [expressions.md](expressions.md) |
| object expression | anonymous object/implementation expression | Planned | [expressions.md](expressions.md) |
| computation expression builder | limited task/result workflow | Planned | [expressions.md](expressions.md) |
| statically resolved type parameters | generic constraints + shape proof | Replacement | [types.md](types.md), [resolution.md](resolution.md) |
| discriminated union struct representation | union representation option | Planned | [declarations.md](declarations.md), [interop.md](interop.md) |
| member constraints | generic constraints + structural shape | Replacement | [types.md](types.md), [resolution.md](resolution.md) |

## C# Coverage

| C# 기능 | TypeSharp 대응 | 상태 | 문서 |
| --- | --- | --- | --- |
| namespace | `namespace` | Direct | [modules.md](modules.md) |
| using/import | `import`, `open`, `import static` | Direct | [modules.md](modules.md) |
| assembly/library reference | manifest `[references]` + metadata import | Direct | [../csharp-interop.md](../csharp-interop.md), [interop.md](interop.md) |
| class | `class` | Direct | [declarations.md](declarations.md) |
| interface | `interface` | Direct | [declarations.md](declarations.md) |
| struct | `struct` | Direct | [declarations.md](declarations.md) |
| enum | `enum` | Direct | [declarations.md](declarations.md) |
| delegate | `delegate` | Direct | [declarations.md](declarations.md), [interop.md](interop.md) |
| event | `event` | Direct | [declarations.md](declarations.md), [interop.md](interop.md) |
| property | property declaration | Direct | [declarations.md](declarations.md) |
| attribute | attribute list | Direct | [interop.md](interop.md) |
| async/await | `async fun`, `await` | Direct | [expressions.md](expressions.md), [interop.md](interop.md) |
| nullable | `T?`, `Option<T>` | Direct | [types.md](types.md) |
| const field | `literal` declaration | Direct | [declarations.md](declarations.md), [interop.md](interop.md) |
| named/optional/params/ref/out/in parameters | argument list + parameter modifier | Direct | [declarations.md](declarations.md), [interop.md](interop.md), [resolution.md](resolution.md) |
| pattern matching | `match`, `is` pattern | Direct | [patterns.md](patterns.md) |
| records | `record` | Direct | [declarations.md](declarations.md) |
| basic array/list literal | collection expression | Direct | [expressions.md](expressions.md) |
| advanced collection expression | spread/builder argument | Planned | [expressions.md](expressions.md) |
| extension method/member | `extension` | Planned | [declarations.md](declarations.md) |
| partial | `partial` | Direct | [declarations.md](declarations.md), [interop.md](interop.md) |
| unsafe | restricted `unsafe` marker | Direct | [interop.md](interop.md) |
| dynamic | restricted `dynamic` marker/type | Direct | [types.md](types.md), [interop.md](interop.md) |
| LINQ query syntax | pipeline/comprehension | Replacement | [expressions.md](expressions.md) |
| source generator | external build/generator | Replacement | [interop.md](interop.md) |
| union types preview | nominal closed union + type-level union | Replacement | [declarations.md](declarations.md), [types.md](types.md) |
| constructor | primary/regular constructor | Direct | [declarations.md](declarations.md) |
| indexer | indexer member/call syntax | Planned | [declarations.md](declarations.md), [expressions.md](expressions.md) |
| operator overload | `operator` declaration | Planned | [declarations.md](declarations.md) |
| implicit/explicit conversion | conversion operator or named conversion | Planned | [declarations.md](declarations.md) |
| tuple/deconstruction | tuple type and pattern | Direct | [types.md](types.md), [patterns.md](patterns.md) |
| `required`/`init` | `required`, `init` accessor | Direct | [declarations.md](declarations.md) |
| lock | `lock` statement/expression candidate | Planned | [expressions.md](expressions.md) |
| iterator method/yield | `yield` lowering to enumerable | Planned | [expressions.md](expressions.md) |
| local function | block-scoped `fun` | Direct | [declarations.md](declarations.md), [expressions.md](expressions.md) |
| anonymous type | record/object expression with inferred structural type | Replacement | [expressions.md](expressions.md), [types.md](types.md) |
| `nameof` | `nameof` intrinsic | Planned | [expressions.md](expressions.md) |
| checked/unchecked | `checked`/`unchecked` block or marker | Planned | [expressions.md](expressions.md) |
| preprocessor directives | manifest/feature flags first | Replacement | [modules.md](modules.md), [interop.md](interop.md) |

## 미분류 기능 처리 규칙

- C#, F#, TypeScript에서 발견한 기능이 이 표에 없으면 문법 설계는 완료된 것이 아니다.
- 새 기능은 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 분류해야 한다.
- Replacement로 분류한 기능은 왜 원 문법을 그대로 채택하지 않는지 설명해야 한다.
- Rejected로 분류한 기능은 goal.md의 비목표와 연결해야 한다.

