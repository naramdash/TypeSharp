# Grammar Ambiguity Review

문서 기준일: 2026-05-18

이 문서는 parser 구현 전에 stable grammar ambiguity를 검토하고, deterministic parsing을 위한 결정을 기록한다. 각 행의 parser decision은 syntax tree, formatter, TextMate grammar, LSP가 서로 다른 언어처럼 갈라지지 않게 하는 기준이다.

## Parser Decision 원칙

- Lexer는 longest-token rule을 사용한다.
- Parser는 현재 syntactic context를 먼저 사용한다.
- Parser는 semantic type information으로 문법을 고르지 않는다.
- 모호성이 남는 brace form처럼 syntax만으로 완전히 고르기 어려운 경우에는 neutral syntax node를 허용하고 semantic 단계가 의미를 닫는다.
- Recovery는 가능한 다음 declaration/import/member boundary까지 진행한다.
- Formatter는 parser가 고른 concrete syntax kind를 기준으로 출력한다.

## 검토표

| 영역 | 모호성 | Parser decision | Recovery 또는 후속 조치 | 상태 |
| --- | --- | --- | --- | --- |
| `|` | type-level union, pattern alternative, future bitwise OR, `|>` pipeline prefix가 겹친다. | Lexer가 `|>`를 먼저 토큰화한다. Type context에서는 union type, pattern context에서는 or-pattern, expression context에서는 pipeline 또는 binary operator로만 해석한다. | Expression bitwise OR를 안정화할 때 precedence table에 별도 행을 추가한다. | 결정됨 |
| `&` | intersection type, pattern and, future bitwise AND가 겹친다. | Type context에서는 intersection type, pattern context에서는 and-pattern, expression context에서는 binary operator 후보로 둔다. | Bitwise AND를 MVP에 넣지 않으면 expression parser는 `&` 사용에 parse diagnostic을 낸다. | 결정됨 |
| `?` | nullable `T?`, optional member `name?: T`, optional chaining `?.`, nullish `??`, assignment `??=`가 겹친다. | Lexer가 `??=`, `??`, `?.`를 `?`보다 먼저 토큰화한다. Type postfix에서는 nullable, shape member identifier 뒤 `?:`에서는 optional member다. | `T ?`처럼 whitespace가 있어도 nullable postfix로 허용할지 formatter 정책에서 확정한다. | 결정됨 |
| `=>` | lambda와 `match` arm이 같은 토큰을 쓴다. | `match` body 안에서는 pattern parser가 arm head를 읽고 `=>`를 match arm separator로 소비한다. 일반 expression context에서는 parameter list 또는 identifier 뒤 `=>`를 lambda로 본다. | `match` arm의 pattern parse가 실패하면 다음 `=>` 또는 `}`까지 skipped token으로 복구한다. | 결정됨 |
| `->` | function type과 함수 선언 반환 표기가 혼동될 수 있다. | `->`는 type parser에서만 function type으로 허용한다. Function declaration return type은 항상 `:`다. | `fun f() -> T`는 parse diagnostic으로 `:`를 제안한다. | 결정됨 |
| `with` | record update expression과 identifier가 충돌할 수 있다. | `with`는 reserved keyword다. Expression parser에서 `expr with { ... }`만 update form으로 허용한다. | `with` 뒤에 brace form이 없으면 missing record update body diagnostic을 낸다. | 결정됨 |
| `{ ... }` | block expression, record/object expression, record shape type, record pattern, declaration body가 같은 brace를 쓴다. | Type context는 record shape, pattern context는 record pattern, declaration context는 body로 읽는다. Expression context는 brace content를 본다. Field list 형태면 record/object expression, declaration/statement가 있으면 block expression, 빈 brace는 `EmptyBraceExpression` neutral node로 둔다. | Semantic 단계가 `EmptyBraceExpression`을 target/context에 따라 empty record/object 또는 unit block으로 닫는다. | 결정됨 |
| `{ name }` | record shorthand와 block의 마지막 identifier expression이 겹친다. | Expression context에서 comma-separated field grammar를 만족하면 record/object expression으로 읽는다. 단일 identifier block expression이 필요하면 block 안에 명시 statement/declaration을 포함해야 한다. | Formatter는 shorthand record expression을 한 줄로 유지할 수 있다. | 결정됨 |
| `[ ... ]` | attribute list와 collection expression이 겹친다. | Declaration/member/parameter header context에서 declaration keyword 또는 modifier 앞의 `[`는 attribute로 tentative parse한다. Expression context에서는 collection expression이다. | Attribute parse가 닫히지 않으면 다음 declaration boundary까지 복구한다. | 결정됨 |
| modifier vs identifier | `record`, `dynamic`, `reflect`, `extern`, `event` 같은 contextual keyword가 일반 이름과 겹칠 수 있다. | Modifier list는 declaration keyword 앞에서만 modifier를 소비한다. Contextual keyword는 해당 문맥 밖에서는 identifier로 허용한다. | Public API에서 escaped/contextual identifier 사용은 analyzer warning 후보로 둔다. | 결정됨 |
| generics `<...>` | type argument list와 `<`, `>` comparison이 겹친다. | Type/declaration context에서는 generic argument/parameter list로 읽는다. Expression context에서는 callee/member 뒤 `<...>`가 닫히고 곧바로 `(`, `.`, `?.`, `[` 중 하나가 오면 type argument list로 tentative parse한다. 그 외에는 comparison expression이다. | Tentative parse 실패 시 원래 위치로 되돌리고 binary expression으로 파싱한다. | 결정됨 |
| import forms | `import type`, `import static`, `import * as`, `import { ... }`, bare import clause가 겹친다. | `import` 다음 첫 토큰으로 form을 선택한다. `type`은 type-only import, `static`은 static import, `*`는 namespace import, `{` 또는 identifier는 named import다. | 잘못된 import clause는 다음 newline 또는 declaration boundary까지 복구한다. | 결정됨 |
| `is` | C#식 `is` pattern과 일반 binary expression precedence가 겹친다. | Expression grammar는 null-coalescing expression 뒤 optional `is pattern`을 둔다. `is`는 pattern narrowing operator로만 안정화한다. | Chained `x is A is B`는 parse diagnostic을 낸다. | 결정됨 |
| `using` | resource lifetime expression이 block item인지 expression인지 문서 간 위치가 모호했다. | `using`은 expression grammar의 conditional/control expression 계층에 포함한다. | `using pattern = expr { ... }`에서 block이 없으면 missing using body diagnostic을 낸다. | 결정됨 |
| attributes target | `[assembly: ...]`와 일반 attribute, collection element label이 비슷해 보일 수 있다. | Attribute parser 안에서만 `attribute_target ":"`를 허용한다. Expression collection에는 target label 문법이 없다. | Unknown target은 parse는 성공시키고 semantic diagnostic으로 넘긴다. | 결정됨 |

## 발견한 문서 불일치

이번 검토에서 아래 불일치를 함께 정리했다.

- [lexical.md](lexical.md)의 reserved keyword 목록에 다른 grammar 문서가 사용하는 `base`, `elif`, `not`, `params`, `ref`가 빠져 있었다.
- [lexical.md](lexical.md)의 operator 목록에 [expressions.md](expressions.md)가 사용하는 `?.`가 빠져 있었다.
- [expressions.md](expressions.md)의 최상위 expression production에 `using_expression`과 `is_expression` 연결이 빠져 있었다.

## 후속 작업

- Operator precedence와 associativity는 [precedence.md](precedence.md)를 기준으로 유지한다.
- `EmptyBraceExpression`을 syntax tree에 둘지, parser AST와 bound AST 사이의 green/red tree 변환에서만 둘지 compiler skeleton에서 결정한다.
- Formatter convention 문서를 만들 때 `{ name }`, `{}` 출력 정책을 다시 확인한다.
