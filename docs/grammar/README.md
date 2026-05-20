# TypeSharp Grammar Bridge

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 긴 표준 문법 원장이 아니라 short bridge stub이다. TypeSharp 문법의 target canonical surface는 [docs-site Grammar](../../docs-site/src/content/docs/grammar.md)와 [Grammar And Language Reference](../../docs-site/src/content/docs/reference.md)다.

## 현재 소유권

| 항목 | 위치 |
| --- | --- |
| Canonical grammar ledger | [docs-site Grammar](../../docs-site/src/content/docs/grammar.md) |
| Canonical language reference index | [docs-site Grammar And Language Reference](../../docs-site/src/content/docs/reference.md) |
| 문서 ownership matrix | [docs-site Document Ownership](../../docs-site/src/content/docs/document-ownership.md) |
| Completed migration task | [../tasks/0001-0254-task-ledger-rollup.md](../tasks/0001-0254-task-ledger-rollup.md) |

## 남아 있는 Bridge Files

아래 파일들은 아직 detailed syntax, coverage, parser recovery, or name-resolution bridge로 남아 있다. 각 row가 docs-site로 접히면 해당 파일도 이 bridge처럼 줄이거나 references를 갱신한 뒤 제거한다.

- [ambiguity.md](ambiguity.md)
- [consistency.md](consistency.md)
- [coverage.md](coverage.md)
- [declarations.md](declarations.md)
- [expressions.md](expressions.md)
- [interop.md](interop.md)
- [lexical.md](lexical.md)
- [modules.md](modules.md)
- [patterns.md](patterns.md)
- [precedence.md](precedence.md)
- [resolution.md](resolution.md)
- [types.md](types.md)

문법 원장 내용을 바꾸는 새 작업은 먼저 docs-site canonical page를 갱신하고, 필요한 경우 같은 변경에서 이 bridge 파일들을 맞춘다.
