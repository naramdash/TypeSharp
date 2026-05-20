# Dependency Inventory

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical dependency inventory and dependency gate policy는 [docs-site Project Policy](../docs-site/src/content/docs/project-policy.md)의 `Dependency And Target Policy` 섹션이다.

## Bridge Scope

- Generated project, `TypeSharp.Core`, `TypeSharp.Runtime`, compiler/CLI/test host, and docs-site dependency inventory는 docs-site Project Policy로 접혔다.
- `net48` compatibility audit and future dependency gate policy는 docs-site Project Policy로 접혔다.
- Runtime/public ABI policy는 [docs-site .NET Interop](../docs-site/src/content/docs/dotnet-interop.md)를 따른다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical page를 찾을 수 있게 하기 위해서만 남아 있다.
