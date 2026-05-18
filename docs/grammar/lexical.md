# Lexical Grammar

문서 기준일: 2026-05-18

이 문서는 TypeSharp의 lexical grammar를 정의한다. 목표는 TypeScript의 친숙한 표기, F#의 간결한 함수형 스타일, C#의 .NET 친화 문법을 동시에 지원할 수 있는 토큰 체계를 만드는 것이다.

## Source File

```ebnf
source_file        ::= bom? shebang? source_text eof
source_text        ::= input_element*
input_element      ::= whitespace | newline | comment | token
```

권장:
- UTF-8을 기본 인코딩으로 한다.
- BOM은 허용하지만 생성 파일에서는 생략한다.
- shebang은 CLI script mode에서만 허용한다.

## Whitespace와 Newline

```ebnf
whitespace         ::= " " | "\t" | vertical_tab | form_feed
newline            ::= "\r\n" | "\n" | "\r"
```

규칙:
- 기본 문법은 indentation-sensitive가 아니다.
- newline은 automatic semicolon insertion을 만들지 않는다.
- formatter는 newline을 의미 없는 trivia로 보존할 수 있어야 한다.
- F#식 가독성을 위해 block expression과 pipeline에서 newline-friendly layout을 권장한다.

## Comments

```ebnf
line_comment       ::= "//" chars_until_newline
block_comment      ::= "/*" nested_comment_text "*/"
doc_comment        ::= "///" chars_until_newline
doc_block_comment  ::= "/**" nested_comment_text "*/"
```

규칙:
- block comment는 중첩을 허용하는 방향을 선호한다.
- doc comment는 C# XML doc과 Markdown doc comment 중 하나를 사양에서 확정해야 한다.
- compiler는 doc comment를 public API metadata 또는 sidecar documentation에 연결할 수 있어야 한다.

## Identifiers

```ebnf
identifier         ::= regular_identifier | escaped_identifier
regular_identifier ::= identifier_start identifier_part*
escaped_identifier ::= "`" escaped_identifier_text "`"
```

권장:
- Unicode identifier를 허용한다.
- escaped identifier는 keyword나 공백 포함 이름을 interop할 때만 권장한다.
- public API에는 escaped identifier 사용을 warning으로 둘 수 있다.

예:

```typesharp
let customerName = "Ada"
let `type` = "keyword as name"
```

## Keywords

### Reserved Keywords

```text
as async await base break case catch class continue delegate do elif else enum
export extension false finally for from fun if import in interface is let literal
match module mut namespace new not null open out override params private
protected public ref return static struct this throw true try type union unsafe
using when where while with yield
```

### Contextual Keywords

```text
abstract ambient checked default dynamic event explicit extern field get global
const implicit init inline internal nameof operator partial readonly record reflect
required sealed set throws unknown unchecked virtual
```

규칙:
- reserved keyword는 일반 identifier로 사용할 수 없다.
- contextual keyword는 해당 문맥에서만 keyword다.
- C# interop keyword와 TypeScript/F# 계열 keyword 충돌은 escaped identifier로 해결한다.
- core grammar는 `var`/`val`/`const` alias를 사용하지 않고 `let`/`let mut`/`literal`로 binding을 통일한다.
- TypeScript식 `as const`는 literal preservation annotation 후보로 남아 있지만, standalone `const` binding keyword는 두지 않는다.
- `const`는 `as const` 같은 제한된 문맥에서만 contextual keyword다.

## Literals

```ebnf
literal            ::= null_literal
                     | bool_literal
                     | numeric_literal
                     | char_literal
                     | string_literal
                     | interpolated_string
                     | raw_string_literal

null_literal       ::= "null"
bool_literal       ::= "true" | "false"
```

### Numeric Literals

```ebnf
numeric_literal    ::= integer_literal | floating_literal | decimal_literal
integer_literal    ::= decimal_int | hex_int | binary_int
decimal_int        ::= digit ("_"? digit)* integer_suffix?
hex_int            ::= "0x" hex_digit ("_"? hex_digit)* integer_suffix?
binary_int         ::= "0b" binary_digit ("_"? binary_digit)* integer_suffix?
```

권장 suffix:

```text
i8 i16 i32 i64 u8 u16 u32 u64 n un f32 f64 m
```

기본:
- 정수 literal의 기본 타입은 `int`.
- 부동소수 literal의 기본 타입은 `float`.
- decimal literal은 `m` suffix를 사용한다.

### String Literals

```ebnf
string_literal      ::= '"' string_char* '"'
raw_string_literal  ::= '"""' raw_string_char* '"""'
interpolated_string ::= "$" string_literal | "$" raw_string_literal
```

목표:
- C#의 interpolation 편의성을 제공한다.
- TypeScript template literal type과 유사한 타입 레벨 문자열 처리는 Planned Grammar로 둔다.

## Operators와 Punctuation

```text
( ) [ ] { } < > . , : ; ? ! @ # $ _
+ - * / % ** & | ^ ~ && || ?. ?? =>
= == != === !== < <= > >=
-> <- |> <| >> << ... .. ::
+= -= *= /= %= &= |= ^= ??=
```

규칙:
- `|`는 type-level union, pattern alternative, bitwise OR 문맥에서 사용한다.
- `|>`는 pipeline operator다.
- `=>`는 lambda와 match arm에 사용한다.
- `->`는 function type 표기에만 사용한다.
- 함수 선언의 반환 타입은 다른 타입 주석과 동일하게 `:`로 표기한다.
- `?`는 타입 뒤에서는 nullable, structural member 이름 뒤에서는 optional member를 뜻한다.
- formatter는 semicolon을 출력하지 않는다. parser는 interop와 error recovery를 위해 semicolon을 허용할 수 있다.

## Trivia와 Tooling

parser는 다음 정보를 보존해야 한다.

- leading/trailing trivia
- doc comment association
- source span
- raw literal text
- newline presence

이 정보는 formatter, VS Code syntax highlighting, LSP diagnostics, go-to-definition에 필요하다.

