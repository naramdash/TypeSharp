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

The extension can use a bundled `server/TypeSharp.LanguageServer.dll`, a repository development build, the public `typesharp lsp` CLI entry point, or explicit settings under `typesharp.languageServer.*`.

## Syntax Highlighting Extension

The syntax highlighting extension is the package under `vscode/typesharp`. It contributes:

- the `typesharp` language id for `.tysh` files,
- `language-configuration.json` for comments, brackets, pairs, and word matching,
- `syntaxes/typesharp.tmLanguage.json` for TextMate highlighting based on the stable grammar surface.

Local VSIX install:

```text
cd vscode/typesharp
npm run prepare:server
npm run check
npm run package:vsix
code --install-extension .\typesharp-vscode-0.1.0.vsix
```

VS Code can also install the generated VSIX through Extensions: Install from VSIX.

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

## Temporary Marketplace Publishing Guide

Publishing requires user-owned Microsoft/Azure DevOps credentials and a Visual Studio Marketplace publisher id. The repository guide lives at `vscode/typesharp/MARKETPLACE.md` and follows the official Visual Studio Code extension publishing flow:

- create an Azure DevOps Personal Access Token with Marketplace Manage scope,
- create or select a Marketplace publisher,
- verify locally with `npx --yes @vscode/vsce login <publisher-id>`,
- package with `npm run package:vsix`,
- publish with `npx --yes @vscode/vsce publish` or upload the generated VSIX manually.

Official reference: [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension).
