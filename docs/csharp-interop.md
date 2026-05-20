# C# Library Interop

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical interop/public ABI/runtime ABI 원장은 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)이다.

## Bridge Scope

- C#/.NET Framework reference model, import model, type mapping, overload/member validation, capability boundary, host compatibility, public API exposure, diagnostics, interop smoke policy는 docs-site `.NET Interop`로 접혔다.
- Runtime ABI versioning policy는 docs-site `.NET Interop`의 `Runtime ABI Policy` 섹션으로 접혔다.
- 문법 표면은 docs-site [Grammar](../docs-site/src/content/docs/grammar.md)와 [Grammar And Language Reference](../docs-site/src/content/docs/reference.md)를 따른다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical page를 찾을 수 있게 하기 위해서만 남아 있다.
