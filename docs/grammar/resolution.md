# Name Resolution And Overload Rules

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical name/member/overload resolution policy is in [docs-site Grammar](../../docs-site/src/content/docs/grammar.md), [Modules And Imports](../../docs-site/src/content/docs/modules.md), [Grammar And Language Reference](../../docs-site/src/content/docs/reference.md), and [.NET Interop](../../docs-site/src/content/docs/dotnet-interop.md).

## Bridge Scope

- Namespace/type/value/member/module spaces, lookup order, explicit import/open precedence, member lookup order, overload candidate checks, nominal-first ranking, ambiguity diagnostics, and interop overload boundaries are folded into docs-site canonical pages.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
