# TypeSharp CLI

문서 기준일: 2026-05-20

이 파일은 task `0251-docs-site-canonical-language-ledger` 이후 남아 있는 short bridge stub다.

Canonical CLI command, manifest, source discovery, diagnostics output, exit code, and formatting contract는 [docs-site CLI](../docs-site/src/content/docs/cli.md)다. Cross-reference index는 [docs-site API And CLI Reference](../docs-site/src/content/docs/api.md)다.

## Bridge Scope

- `typesharp version`, `new`, `check`, `build`, `run`, `explain`, `format`, and stable-backlog `lsp`/`test` command policy는 docs-site CLI로 접혔다.
- Common options, exit codes, manifest shape, source discovery, diagnostics text/JSON shape, and implementation order는 docs-site CLI로 접혔다.
- Diagnostic code taxonomy and descriptor metadata는 [docs-site Diagnostics](../docs-site/src/content/docs/diagnostics.md)를 따른다.
- 이 bridge는 active agent가 예전 경로를 열었을 때 target canonical pages를 찾을 수 있게 하기 위해서만 남아 있다.
