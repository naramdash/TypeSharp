# TypeSharp VS Code Extension

This directory contains the VS Code extension for TypeSharp.

Current scope:
- registers the `typesharp` language id
- associates `.tysh` files with TypeSharp
- contributes language configuration for comments, brackets, and pairs
- contributes a TextMate grammar based on `docs/src/content/docs/grammar.md`
- activates on `onLanguage:typesharp`
- starts the TypeSharp language server over stdio
- publishes LSP diagnostics into VS Code diagnostics
- forwards hover, go-to-definition, and basic completion requests to the language server
- provides a document formatter that mirrors the CLI formatter MVP whitespace normalization
- defines package metadata for the extension JS, TextMate grammar, language configuration, and bundled server folder

Development launch contract:
- build `src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj`
- open this folder as a VS Code extension development host
- open a `.tysh` file

Packaging contract:
- run `npm run prepare:server` to publish `TypeSharp.LanguageServer` into `vscode/typesharp/server`
- run `npm run check`, `npm run check:smoke`, `npm run check:live`, `npm run check:host`, `npm run test:smoke`, `npm run test:live`, and `npm run test:host`
- package the extension folder with the files listed in `package.json`

Extension Host smoke:
- `npm run test:host` launches the installed VS Code executable with `--extensionDevelopmentPath` and `--extensionTestsPath`
- the smoke opens `.tysh` files in a temporary workspace and verifies diagnostics, hover, go-to-definition, completion, and document formatting through VS Code commands
- set `VSCODE_BIN` to a VS Code executable path if `Code.exe` or `code` is not discoverable

docs contract:
- `docs/src/content/docs/vscode-lsp.md` lists the same smoke-tested commands so the public VS Code/LSP documentation stays reproducible by the repository checks.

The `typesharp.languageServer.command`, `typesharp.languageServer.args`, and `typesharp.languageServer.cwd` settings can override the default server discovery for local or packaged builds.
