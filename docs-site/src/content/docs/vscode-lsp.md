---
title: VS Code And LSP
description: TypeSharp VS Code extension and language server workflow.
---

The VS Code extension lives in `vscode/typesharp`, and the language server host lives in `src/TypeSharp.LanguageServer`.

Current editor features:

- `.tysh` language registration and TextMate syntax highlighting,
- extension activation on `typesharp` documents,
- stdio LSP client launch,
- diagnostics from compiler parser, binder, and type checker,
- hover,
- go-to-definition,
- basic completion,
- document formatting for the CLI formatter MVP whitespace rules.

Formatting behavior follows the canonical [CLI](../cli/) formatting convention. Diagnostic codes, descriptor metadata, and source spans follow [Diagnostics](../diagnostics/).

The extension can use a bundled `server/TypeSharp.LanguageServer.dll`, a repository development build, or explicit settings under `typesharp.languageServer.*`.

Smoke-tested commands:

```text
cd vscode/typesharp
npm run check
npm run check:live
npm run prepare:server
npm run test:live
npm run test:host
```

`npm run test:live` starts the bundled language server over stdio and verifies diagnostics, hover, go-to-definition, completion, and formatting through the extension client. `npm run test:host` launches a real VS Code Extension Host and exercises the same editor-facing commands against `.tysh` documents in a temporary workspace.
