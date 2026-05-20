# TypeSharp VS Code Workspace

This folder owns editor integration.

## Contents

- `typesharp`: VS Code extension package, TextMate grammar, language configuration, formatter client, LSP client, smoke tests, and marketplace notes.

The extension bundles the language server built from `../lang/TypeSharp.LanguageServer`.

## Common Commands

```powershell
cd vscode\typesharp
npm run check
npm run prepare:server
```
