---
title: VS Code And LSP
description: TypeSharp VS Code extension and language server workflow.
---

The VS Code extension lives in `vscode/typesharp`, and the language server host lives in `lang/TypeSharp.LanguageServer`.

Use [Install](../install/) first for the release CLI, checksum manifest, and release notes that define the matching VSIX asset.

Current editor features:

- `.tysh` language registration and TextMate syntax highlighting,
- extension activation on `typesharp` documents,
- stdio LSP client launch,
- open/change/close document synchronization,
- diagnostics from compiler parser, binder, and type checker,
- stale diagnostic clearing when a `.tysh` document closes,
- hover,
- go-to-definition,
- basic completion,
- document formatting for the CLI formatter MVP whitespace rules.

Formatting behavior follows the canonical [CLI](../cli/) formatting convention. Diagnostic codes, descriptor metadata, and source spans follow [Diagnostics](../diagnostics/).

The extension can use a bundled `server/TypeSharp.LanguageServer.dll`, a repository development build, the public `typesharp lsp` CLI entry point, or explicit settings under `typesharp.languageServer.*`.

The implementation follows the official [VS Code Language Server Extension Guide](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide) shape: the editor client owns document notifications and UI wiring, while TypeSharp owns diagnostics and semantic responses.

## Syntax Highlighting Extension

The syntax highlighting extension is the package under `vscode/typesharp`. It contributes:

- the `typesharp` language id for `.tysh` files,
- `language-configuration.json` for comments, brackets, pairs, and word matching,
- `syntaxes/typesharp.tmLanguage.json` for TextMate highlighting based on the stable grammar surface.

Release VSIX install:

```powershell
$version = "v0.1.0-preview.2"
$repo = "naramdash/TypeSharp"
$release = "https://github.com/$repo/releases/download/$version"
$downloadRoot = Join-Path $env:TEMP "typesharp-$version"

New-Item -ItemType Directory -Force $downloadRoot | Out-Null
Invoke-WebRequest "$release/typesharp-vscode-$version.vsix" -OutFile "$downloadRoot/typesharp-vscode-$version.vsix"
Invoke-WebRequest "$release/SHA256SUMS.txt" -OutFile "$downloadRoot/SHA256SUMS.txt"

# Use the Assert-ReleaseAssetHash helper from Install.
Assert-ReleaseAssetHash "typesharp-vscode-$version.vsix"

code --install-extension "$downloadRoot/typesharp-vscode-$version.vsix"
```

Open the tag-specific GitHub Release notes and confirm the exact asset names before download, including the VSIX asset. The release VSIX name is `typesharp-vscode-<tag>.vsix`, and it is covered by the same `SHA256SUMS.txt` manifest as the CLI and runtime archives. Use the same tag as the CLI release so the bundled language server and Runtime ABI expectation match the installed compiler.

Local development package:

```text
cd vscode/typesharp
npm run prepare:server
npm run check
npm run package:vsix
code --install-extension .\typesharp-vscode-<local-version>.vsix
```

VS Code can also install the release VSIX through Extensions: Install from VSIX.

Smoke-tested commands:

```text
cd vscode/typesharp
npm run check
npm run check:smoke
npm run check:live
npm run test:smoke
npm run prepare:server
npm run test:live
npm run test:host
```

`npm run test:smoke` verifies the extension client forwards document lifecycle notifications, maps diagnostics, clears closed-document diagnostics, and forwards hover, go-to-definition, completion, and formatting requests. `npm run test:live` starts the bundled language server over stdio and verifies diagnostics, hover, go-to-definition, completion, and formatting through the extension client. `npm run test:host` launches a real VS Code Extension Host and exercises the same editor-facing commands against `.tysh` documents in a temporary workspace.

## Workflow Parity Roadmap

The VS Code extension is release-ready only when it stays behaviorally aligned with the CLI:

- diagnostics share compiler codes, severity, source ordering, and source spans with `typesharp check`;
- formatting matches `typesharp format --check` for every supported formatter rule;
- hover, go-to-definition, and completion read from the same semantic model used by CLI checks;
- packaged VSIX builds include the same language server/runtime ABI expected by the matching CLI release;
- release verification covers mocked extension-client tests, live stdio language-server tests, and a real Extension Host smoke test.

## Temporary Marketplace Publishing Guide

Publishing requires user-owned Microsoft/Azure DevOps credentials and a Visual Studio Marketplace publisher id. The repository guide lives at `vscode/typesharp/MARKETPLACE.md` and follows the official Visual Studio Code extension publishing flow:

- create an Azure DevOps Personal Access Token with Marketplace Manage scope,
- create or select a Marketplace publisher,
- verify locally with `npx --yes @vscode/vsce login <publisher-id>`,
- package with `npm run package:vsix`,
- publish with `npx --yes @vscode/vsce publish` or upload the generated VSIX manually.

Marketplace publication is an adoption gate, not a normal build step. It requires a release-owner credential path, versioned VSIX artifact, release notes, checksum coverage, and a rollback path through manual VSIX installation.

Official reference: [Publishing Extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension).
