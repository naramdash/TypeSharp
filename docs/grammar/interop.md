# Interop Grammar

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical interop syntax is in [docs-site Grammar](../../docs-site/src/content/docs/grammar.md), [Grammar And Language Reference](../../docs-site/src/content/docs/reference.md), and [.NET Interop](../../docs-site/src/content/docs/dotnet-interop.md).

## Bridge Scope

- C# namespace imports, `import type`, `import static`, attributes, `ref`/`out`/`in`, named/optional/params argument shapes, capability markers, extern declarations, and public ABI rejection rules are folded into docs-site canonical pages.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
