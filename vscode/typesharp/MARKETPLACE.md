# TypeSharp VS Code Marketplace Guide

This is a temporary publishing guide for the TypeSharp VS Code extension. It follows the official Visual Studio Code extension publishing flow.

Authoritative reference:

- https://code.visualstudio.com/api/working-with-extensions/publishing-extension

## User-Owned Prerequisites

Publishing needs credentials that an agent should not create or handle:

- a Microsoft account that can access Azure DevOps,
- an Azure DevOps Personal Access Token with Marketplace Manage scope,
- a Visual Studio Marketplace publisher id,
- agreement on whether the package publisher remains `typesharp` or changes to another publisher id.

If the Marketplace publisher id is not `typesharp`, update `publisher` in `package.json` before packaging or publishing.

## Preflight

Run these commands from `vscode/typesharp`:

```powershell
npm run prepare:server
npm run check
npm run check:smoke
npm run check:live
npm run check:host
npm run test:smoke
npm run test:live
npm run test:host
```

`npm run test:host` requires an installed VS Code executable. Set `VSCODE_BIN` if the runner cannot find `Code.exe` or `code`.

## Package A VSIX

```powershell
cd vscode/typesharp
npm run package:vsix
```

This creates `typesharp-vscode-0.1.0.vsix` in the extension directory.

Local install:

```powershell
code --install-extension .\typesharp-vscode-0.1.0.vsix
```

The same file can be installed through VS Code Extensions: Install from VSIX.

## Create Publisher Credentials

These steps require the user's browser session:

1. Create or choose an Azure DevOps organization.
2. Create a Personal Access Token with Marketplace Manage scope.
3. Open the Visual Studio Marketplace publisher management page.
4. Create or select the publisher id that should own the TypeSharp extension.
5. Verify the publisher locally:

```powershell
npx --yes @vscode/vsce login <publisher-id>
```

Paste the PAT only into the local `vsce` prompt. Do not commit it, paste it into docs, or expose it in terminal logs.

## Publish

Automatic publish:

```powershell
cd vscode/typesharp
npx --yes @vscode/vsce publish
```

Manual publish:

1. Run `npm run package:vsix`.
2. Upload the generated VSIX in the Visual Studio Marketplace publisher management page.

After publishing, install the Marketplace extension into a clean VS Code profile and open a `.tysh` file to confirm language id, highlighting, diagnostics, hover, definition, completion, and formatting.
