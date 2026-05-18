# TypeSharp Grammar

문서 기준일: 2026-05-18

이 폴더는 TypeSharp가 가져야 할 문법을 정의한다. 목표는 TypeScript, F#, C#의 모든 실용 기능을 TypeSharp 문법으로 직접 포괄하거나, TypeSharp의 더 일관된 기능으로 대체할 수 있을 때까지 문법 사양을 확장하는 것이다.

## 문법 목표

TypeSharp 문법은 다음을 동시에 만족해야 한다.

- TypeScript처럼 파일이 module graph의 일부로 동작한다.
- TypeScript처럼 암묵적 타입 추론, 구조적 타입, type-level union, narrowing을 제공한다.
- F#처럼 expression-oriented syntax, immutable binding, nominal closed union, pattern matching, pipeline/composition을 중심에 둔다.
- C#처럼 class/interface/property/event/delegate/attribute/async/using/extension/partial 같은 .NET 친화 편의 기능을 제공한다.
- VS Code와 CLI가 같은 grammar와 semantic model을 공유할 수 있어야 한다.
- .NET Framework 4.8로 lowering 가능한 문법만 stable grammar로 들어간다.

## 문법 문서 구성

- [lexical.md](lexical.md): 문자, 토큰, 식별자, 키워드, 주석, literal.
- [consistency.md](consistency.md): 공통 기호, 선언 형태, namespace/import 순서, mutability, optional/nullable 규칙.
- [modules.md](modules.md): 파일, module graph, import/export, namespace, ambient declaration.
- [declarations.md](declarations.md): type/function/member/module declaration.
- [types.md](types.md): nominal type, structural type, generic, nullable, type-level union/intersection, literal type.
- [expressions.md](expressions.md): expression, block, lambda, pipeline, object/collection expression, control expression.
- [patterns.md](patterns.md): pattern matching, narrowing, exhaustiveness.
- [interop.md](interop.md): .NET interop 문법, C# library import/call, attribute, capability marker, unsafe/dynamic/reflect/interop boundary.
- [resolution.md](resolution.md): 이름 해석, import/open 우선순위, overload candidate, public boundary resolution.
- [ambiguity.md](ambiguity.md): stable grammar의 parser ambiguity와 recovery 결정을 추적한다.
- [precedence.md](precedence.md): expression/type/pattern operator precedence와 associativity를 고정한다.
- [coverage.md](coverage.md): TypeScript/F#/C# 기능을 TypeSharp 문법이 어떻게 포괄하거나 대체하는지 추적한다.

## 안정성 등급

| 등급 | 의미 |
| --- | --- |
| Stable Grammar | MVP 또는 안정 버전에서 구현해야 하는 문법 |
| Planned Grammar | 목표에는 포함되지만 MVP 이후 구현 가능한 문법 |
| Experimental Grammar | feature gate가 필요한 실험 문법 |
| Replacement | 외부 언어 기능을 그대로 복제하지 않고 TypeSharp식 기능으로 대체 |
| Rejected Syntax | 목표와 맞지 않아 문법으로 채택하지 않는 기능 |

## 문법 설계 원칙

1. 문법은 parser-friendly해야 한다.
   - 가능한 한 deterministic parsing이 가능해야 한다.
   - formatter와 syntax highlighting이 문맥 없이도 많은 정보를 얻을 수 있어야 한다.

2. expression-oriented를 기본으로 한다.
   - `if`, `match`, `try`, block은 값을 만들 수 있어야 한다.
   - statement-only 기능은 C# interop와 imperative code에 필요한 경우로 제한한다.

3. public ABI와 compile-time type을 분리한다.
   - `union` declaration은 F#식 nominal closed union이다.
   - `A | B`는 TypeScript식 type-level union이며 기본적으로 compile-time type이다.
   - public API에는 type-level union이 직접 노출되지 않는다.

4. module이 기본이다.
   - 모든 source file은 module graph에 속한다.
   - global script는 기본값이 아니다.
   - ambient declaration은 명시해야 한다.

5. C# interop를 문법 차원에서 숨기지 않는다.
   - attribute, namespace, class, interface, delegate, event, property, async/task, `using`/resource lifetime 문법을 제공한다.
   - unsafe/dynamic/reflect/interop escape는 명시 marker를 요구한다.

6. TypeScript, F#, C# 기능은 추적 가능해야 한다.
   - 모든 주요 외부 기능은 [coverage.md](coverage.md)에서 Direct, Equivalent, Replacement, Planned, Experimental, Rejected 중 하나로 분류한다.
   - 분류되지 않은 기능은 아직 문법 설계가 끝난 것이 아니다.

7. 선언 문법의 기호를 최소화한다.
   - 타입 주석은 parameter, property, return type 모두 `:`를 사용한다.
   - 함수 선언의 expression body는 `=`를 사용한다.
   - 함수 선언의 block body는 `{ ... }`를 사용한다.
   - `->`는 함수 타입에만 사용한다.
   - `=>`는 lambda와 match arm에만 사용한다.

8. 별칭 문법보다 하나의 공식 문법을 선호한다.
   - immutable binding은 `let`, mutable binding은 `let mut`를 사용한다.
   - compile-time constant가 필요하면 `const`가 아니라 `literal` declaration을 사용한다.
   - core grammar에는 `var`/`val`/`const` alias를 두지 않는다.
   - file-scoped namespace를 기본으로 하고 block namespace는 필요할 때만 사용한다.

## 우선 구현 순서

1. Consistency rules와 lexical grammar.
2. File/module grammar.
3. Type grammar: primitive, generic, nullable, structural shape, type-level union.
4. Declaration grammar: function, record, nominal closed union, class/interface.
5. Expression grammar: block expression, call/member access, lambda, match, pipeline.
6. Pattern grammar: literal/type/union/record/tuple pattern과 narrowing.
7. Interop grammar: attribute, namespace, delegate, event, async/task, capability marker.
8. Name resolution grammar/semantics: module import, open, member lookup, overload candidate.
9. VS Code TextMate grammar와 LSP parser integration.

