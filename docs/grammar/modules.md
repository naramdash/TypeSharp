# Module Grammar

문서 기준일: 2026-05-18

TypeSharp의 파일 구조는 TypeScript처럼 module graph를 기본으로 한다. C#의 namespace와 F#의 module 개념은 .NET metadata와 함수형 코드 구성을 위해 함께 제공한다.

## Source Unit

```ebnf
source_unit        ::= file_header? file_namespace? import_or_open_declaration*
                       top_level_declaration*
file_header        ::= file_attribute*
file_attribute     ::= "#!" attribute_target? attribute_list
file_namespace     ::= "namespace" qualified_name
import_or_open_declaration ::= import_declaration | open_declaration
```

규칙:
- TypeSharp source file의 기본 확장자는 `.tysh`다.
- 모든 source file은 기본적으로 module이다.
- source file은 file-scoped namespace를 기본으로 사용한다.
- file-scoped namespace가 없으면 generated C# namespace는 manifest `rootNamespace`로 fallback한다.
- import/open은 file-scoped namespace 다음, 실제 선언 전으로 모은다.
- import/export가 없더라도 global script가 되지 않는다.
- global scope 오염은 `ambient` 선언 또는 manifest opt-in으로만 허용한다.

## Import

```ebnf
import_declaration ::= import_named
                     | import_namespace
                     | import_type
                     | import_static

import_named       ::= "import" import_clause "from" string_literal
import_namespace   ::= "import" "*" "as" identifier "from" string_literal
import_type        ::= "import" "type" import_clause "from" string_literal
import_static      ::= "import" "static" type_name

import_clause      ::= identifier
                     | "{" import_specifier_list? "}"
import_specifier_list ::= import_specifier ("," import_specifier)* ","?
import_specifier   ::= identifier ("as" identifier)?
```

예:

```typesharp
import { readFile, writeFile as write } from "System.IO"
import * as Json from "./json"
import type { JsonValue } from "./json-types"
import static System.Math
```

결정:
- module specifier는 TypeScript처럼 string literal을 사용한다.
- .NET assembly reference와 source module path resolution은 project manifest와 compiler host가 담당한다.
- `import type`은 compile-time type만 가져오며 runtime dependency를 만들지 않는다.
- 현재 구현된 named import alias slice는 `import { Name as Alias } from "Namespace"`를 generated C# `using Alias = Namespace.Name;` directive로 낮춘다.
- 현재 구현된 namespace import slice는 `import * as Alias from "Namespace"`를 generated C# `using Alias = Namespace;` directive로 낮춘다.
- named/namespace import alias가 같은 file scope의 기존 선언이나 다른 import alias와 같은 local name을 쓰면 binder가 `TS2002` duplicate symbol diagnostic을 보고한다. Cross-file source module graph conflict analysis는 별도 후속 작업이다.

## Export

```ebnf
export_declaration ::= "export" top_level_declaration
                     | "export" "{" export_specifier_list? "}"
                     | "export" "*" "from" string_literal
                     | "export" "type" "{" export_specifier_list? "}"

export_specifier_list ::= export_specifier ("," export_specifier)* ","?
export_specifier      ::= identifier ("as" identifier)?
```

규칙:
- public surface는 export로 결정한다.
- .NET public accessibility와 module export는 별도이므로 mapping 사양이 필요하다.
- library project에서는 export된 declaration만 public API 후보가 된다.

## Namespace

```ebnf
namespace_declaration       ::= file_scoped_namespace
                              | block_namespace_declaration
file_scoped_namespace       ::= "namespace" qualified_name
block_namespace_declaration ::= accessibility? "namespace" qualified_name block
qualified_name              ::= identifier ("." identifier)*
```

예:

```typesharp
namespace Company.Product

export class CustomerService { }
```

규칙:
- namespace는 .NET metadata namespace와 대응한다.
- namespace는 파일 경로와 반드시 같을 필요는 없지만 analyzer가 mismatch를 경고할 수 있다.
- `namespace Company.Product`는 파일의 남은 선언에 적용된다.
- `namespace Company.Product { ... }`는 generated code, interop, nested namespace가 필요할 때만 사용한다.

## Module Declaration

```ebnf
module_declaration ::= accessibility? "module" identifier module_body
module_body        ::= block
```

의미:
- module은 함수, 값, helper type을 묶는 logical container다.
- .NET lowering은 static class 또는 generated container type으로 한다.

예:

```typesharp
module MathEx {
  let squareValue: int -> int = x => x * x

  export fun square(x: int): int =
    squareValue(x)
}
```

## Open

F# 친화성을 위해 `open`을 지원할 수 있다.

```ebnf
open_declaration ::= "open" qualified_name
```

규칙:
- `open`은 module/namespace member lookup을 편하게 하는 문법이다.
- 현재 구현된 slice는 root-level `open Namespace`를 parser에서 보존하고 generated C# `using Namespace;` directive로 낮춘다.
- public API surface를 만들지 않는다.
- wildcard import와 같은 과도한 이름 오염은 warning 대상이다.

## Ambient Declaration

```ebnf
ambient_declaration ::= "ambient" (block_namespace_declaration
                                  | module_declaration
                                  | type_declaration
                                  | function_signature
                                  | value_declaration)
```

의미:
- ambient declaration은 외부 assembly, generated symbol, native interop, global host API를 설명한다.
- implementation을 emit하지 않는다.
- ambient function은 body 없는 signature로 선언한다.

예:

```typesharp
namespace Legacy

ambient public fun Invoke(name: string): dynamic
```

규칙:
- ambient는 명시해야 한다.
- 현재 구현된 slice는 body 없는 ambient function signature를 parser/binder/type-checker에서 보존하고 generated C# emission에서는 제외한다.
- ambient 파일은 manifest에서 분리할 수 있어야 한다.
- ambient declaration이 runtime symbol과 맞지 않으면 interop diagnostics를 제공한다.

## Project Manifest 관련 문법

문법 자체는 manifest 형식을 강제하지 않지만 module resolution은 manifest와 연결된다.

필수 manifest 정보:
- source roots
- output root
- target framework
- assembly references
- local DLL references
- package references
- language version
- strict/preview feature flags

## C#/F#/TypeScript 대응

| 외부 기능 | TypeSharp 문법 |
| --- | --- |
| TypeScript ES module | `import` / `export` |
| TypeScript type-only import | `import type` |
| C# namespace | `namespace` |
| C# using static | `import static` |
| F# module | `module` |
| F# open | `open` |
| TypeScript ambient declaration | `ambient` |
| C# global namespace | 명시 `ambient` 또는 manifest opt-in |

