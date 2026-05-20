# TypeSharp Formatting Convention

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical formatting convention은 [docs-site CLI](../docs-site/src/content/docs/cli.md)의 `Formatting Convention` 섹션이다. VS Code formatter 연결은 [docs-site VS Code And LSP](../docs-site/src/content/docs/vscode-lsp.md)를 따른다.

## Bridge Scope

- indentation, semicolon, blank-line, file-layout, modifier-order, declaration-layout, expression-layout, type-layout, collection/record-expression, interop/public ABI formatting, and `typesharp format --check` policy는 docs-site CLI로 접혔다.
- Formatter and LSP integration policy는 docs-site CLI and VS Code/LSP pages로 접혔다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
