const assert = require("assert");
const path = require("path");
const vscode = require("vscode");

async function run() {
  const workspaceRoot = process.env.TYPESHARP_EXTENSION_HOST_SMOKE_WORKSPACE;
  assert.ok(workspaceRoot, "TYPESHARP_EXTENSION_HOST_SMOKE_WORKSPACE must point at the smoke workspace.");

  const sourceUri = vscode.Uri.file(path.join(workspaceRoot, "src", "Main.tysh"));
  const document = await vscode.workspace.openTextDocument(sourceUri);
  await vscode.window.showTextDocument(document);

  assert.strictEqual(document.languageId, "typesharp");

  const extension = vscode.extensions.getExtension("typesharp.typesharp-vscode");
  assert.ok(extension, "TypeSharp extension should be registered in the Extension Host.");
  await waitFor(() => extension.isActive, "TypeSharp extension activation");

  const diagnostics = await waitFor(() => {
    const items = vscode.languages.getDiagnostics(sourceUri);
    return items.some((item) => String(item.code) === "TS2202") ? items : undefined;
  }, "TS2202 diagnostics");
  assert.ok(diagnostics.some((item) => item.message.includes("Cannot return nullable expression")));

  const referencePosition = new vscode.Position(4, 30);
  const hoverItems = await waitFor(async () => {
    const items = await vscode.commands.executeCommand("vscode.executeHoverProvider", sourceUri, referencePosition);
    return Array.isArray(items) && items.length > 0 ? items : undefined;
  }, "hover result");
  assert.ok(hoverText(hoverItems).includes("greeting"), "Hover result should describe greeting.");

  const definitions = await waitFor(async () => {
    const items = await vscode.commands.executeCommand("vscode.executeDefinitionProvider", sourceUri, referencePosition);
    return Array.isArray(items) && items.length > 0 ? items : undefined;
  }, "definition result");
  const definition = definitions[0];
  const definitionUri = definition.uri || definition.targetUri;
  const definitionRange = definition.range || definition.targetRange;
  assert.strictEqual(definitionUri.toString(), sourceUri.toString());
  assert.strictEqual(definitionRange.start.line, 2);
  assert.strictEqual(definitionRange.start.character, 11);

  const completionItems = await waitFor(async () => {
    const list = await vscode.commands.executeCommand(
      "vscode.executeCompletionItemProvider",
      sourceUri,
      new vscode.Position(4, 28)
    );
    const items = Array.isArray(list) ? list : list && list.items;
    return items && items.some((item) => item.label === "greeting") ? items : undefined;
  }, "completion result");
  assert.ok(completionItems.some((item) => item.label === "greeting"));

  const formatUri = vscode.Uri.file(path.join(workspaceRoot, "src", "Format.tysh"));
  const formatDocument = await vscode.workspace.openTextDocument(formatUri);
  assert.strictEqual(formatDocument.languageId, "typesharp");
  const formattingEdits = await waitFor(async () => {
    const items = await vscode.commands.executeCommand("vscode.executeFormatDocumentProvider", formatUri);
    return Array.isArray(items) && items.length > 0 ? items : undefined;
  }, "formatting result");
  const workspaceEdit = new vscode.WorkspaceEdit();
  for (const edit of formattingEdits) {
    workspaceEdit.replace(formatUri, edit.range, edit.newText);
  }

  assert.strictEqual(await vscode.workspace.applyEdit(workspaceEdit), true);
  assert.strictEqual(
    formatDocument.getText().replace(/\r\n/g, "\n"),
    "namespace Samples.Format\n\nexport fun greeting(): string = \"ok\"\n"
  );
}

async function waitFor(callback, label) {
  for (let attempt = 0; attempt < 100; attempt += 1) {
    const value = await callback();
    if (value) {
      return value;
    }

    await new Promise((resolve) => setTimeout(resolve, 100));
  }

  throw new Error(`Timed out waiting for ${label}.`);
}

function hoverText(items) {
  return items
    .flatMap((item) => Array.isArray(item.contents) ? item.contents : [item.contents])
    .map((content) => {
      if (typeof content === "string") {
        return content;
      }

      if (content && typeof content.value === "string") {
        return content.value;
      }

      return "";
    })
    .join("\n");
}

if (require.main === module) {
  run().catch((error) => {
    console.error(error && error.stack ? error.stack : error);
    process.exitCode = 1;
  });
}

module.exports = {
  run,
};
