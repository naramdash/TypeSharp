const assert = require("assert");
const fs = require("fs");
const Module = require("module");
const os = require("os");
const path = require("path");
const { pathToFileURL } = require("url");

const extensionRoot = path.resolve(__dirname, "..");
const serverDll = path.join(extensionRoot, "server", "TypeSharp.LanguageServer.dll");
assert.ok(fs.existsSync(serverDll), "Run npm run prepare:server before npm run test:live.");

const workspaceRoot = fs.mkdtempSync(path.join(os.tmpdir(), "typesharp-vscode-live-"));
const sourceRoot = path.join(workspaceRoot, "src");
const sourcePath = path.join(sourceRoot, "Main.tysh");
fs.mkdirSync(sourceRoot, { recursive: true });
fs.writeFileSync(
  path.join(workspaceRoot, "TypeSharp.toml"),
  '[project]\nname = "VsCodeLiveSmoke"\ntargetFramework = "net48"\noutputType = "library"\n',
  "utf8"
);

const sourceText = [
  "namespace Samples.Lsp",
  "",
  "export fun greeting(name: string): string = name",
  "",
  'export fun main(): string = greeting("TypeSharp")',
].join("\n");
fs.writeFileSync(sourcePath, sourceText, "utf8");

const providers = {};
const outputLines = [];
const diagnostics = {
  calls: [],
  set(uri, items) {
    this.calls.push({ uri, items });
  },
  delete() {},
  dispose() {},
};

class FakeUri {
  constructor(value, fsPath) {
    this.value = value;
    this.fsPath = fsPath || value.replace(/^file:\/\//, "");
    this.scheme = value.split(":")[0];
  }

  toString() {
    return this.value;
  }
}

class Position {
  constructor(line, character) {
    this.line = line;
    this.character = character;
  }
}

class Range {
  constructor(start, end) {
    this.start = start;
    this.end = end;
  }
}

class Diagnostic {
  constructor(range, message, severity) {
    this.range = range;
    this.message = message;
    this.severity = severity;
  }
}

class MarkdownString {
  constructor(value) {
    this.value = value;
    this.isTrusted = undefined;
  }
}

class Hover {
  constructor(contents, range) {
    this.contents = contents;
    this.range = range;
  }
}

class Location {
  constructor(uri, range) {
    this.uri = uri;
    this.range = range;
  }
}

class CompletionItem {
  constructor(label, kind) {
    this.label = label;
    this.kind = kind;
    this.detail = undefined;
  }
}

const documentUri = uriFromPath(sourcePath);
const workspaceUri = uriFromPath(workspaceRoot);
const document = {
  languageId: "typesharp",
  uri: documentUri,
  version: 1,
  getText() {
    return sourceText;
  },
};

const vscodeStub = {
  window: {
    createOutputChannel() {
      return {
        append(value) {
          outputLines.push(value);
        },
        appendLine(value) {
          outputLines.push(`${value}\n`);
        },
        dispose() {},
      };
    },
  },
  languages: {
    createDiagnosticCollection() {
      return diagnostics;
    },
    registerHoverProvider(selector, provider) {
      providers.hover = { selector, provider };
      return disposable();
    },
    registerDefinitionProvider(selector, provider) {
      providers.definition = { selector, provider };
      return disposable();
    },
    registerCompletionItemProvider(selector, provider, ...triggers) {
      providers.completion = { selector, provider, triggers };
      return disposable();
    },
    registerDocumentFormattingEditProvider(selector, provider) {
      providers.formatting = { selector, provider };
      return disposable();
    },
  },
  workspace: {
    workspaceFolders: [
      {
        name: "workspace",
        uri: workspaceUri,
      },
    ],
    textDocuments: [document],
    getConfiguration(section) {
      assert.strictEqual(section, "typesharp.languageServer");
      return {
        get(key) {
          if (key === "command") {
            return "";
          }

          if (key === "args") {
            return [];
          }

          if (key === "cwd") {
            return workspaceRoot;
          }

          return undefined;
        },
      };
    },
    onDidOpenTextDocument() {
      return disposable();
    },
    onDidChangeTextDocument() {
      return disposable();
    },
    onDidCloseTextDocument() {
      return disposable();
    },
  },
  Uri: {
    parse(value) {
      return new FakeUri(value);
    },
    file(value) {
      return uriFromPath(value);
    },
  },
  Position,
  Range,
  Diagnostic,
  MarkdownString,
  Hover,
  Location,
  CompletionItem,
  DiagnosticSeverity: {
    Error: 0,
    Warning: 1,
    Information: 2,
    Hint: 3,
  },
  CompletionItemKind: {
    Text: 1,
    Function: 3,
    Variable: 6,
    Class: 7,
    Module: 9,
    Value: 12,
    Keyword: 14,
    Reference: 18,
  },
  TextEdit: {
    replace(range, newText) {
      return { range, newText };
    },
  },
};

function disposable() {
  return { dispose() {} };
}

const originalLoad = Module._load;
Module._load = function load(request, parent, isMain) {
  if (request === "vscode") {
    return vscodeStub;
  }

  return originalLoad.call(this, request, parent, isMain);
};

const extension = require("../extension.js");

async function main() {
  const context = {
    extensionPath: extensionRoot,
    subscriptions: [],
  };

  try {
    extension.activate(context);

    await waitFor(() => diagnostics.calls.length > 0, "initial diagnostics");
    assert.strictEqual(diagnostics.calls.at(-1).uri.toString(), documentUri.toString());
    assert.deepStrictEqual(diagnostics.calls.at(-1).items, []);

    assert.ok(providers.hover, "hover provider should be registered");
    assert.ok(providers.definition, "definition provider should be registered");
    assert.ok(providers.completion, "completion provider should be registered");
    assert.ok(providers.formatting, "formatting provider should be registered");

    const token = { isCancellationRequested: false };
    const position = new Position(4, 30);

    const hover = await providers.hover.provider.provideHover(document, position, token);
    assert.ok(hover, "hover should be returned from the live language server");
    assert.match(hover.contents.value, /greeting/);
    assert.match(hover.contents.value, /function/);

    const definition = await providers.definition.provider.provideDefinition(document, position, token);
    assert.ok(definition, "definition should be returned from the live language server");
    assert.strictEqual(definition.uri.toString(), documentUri.toString());
    assert.strictEqual(definition.range.start.line, 2);
    assert.strictEqual(definition.range.start.character, 11);

    const completions = await providers.completion.provider.provideCompletionItems(document, new Position(4, 28), token);
    assert.ok(Array.isArray(completions), "completion provider should return an array");
    assert.ok(completions.some((item) => item.label === "greeting" && item.detail === "function"));

    const unformattedDocument = {
      languageId: "typesharp",
      uri: documentUri,
      version: 2,
      getText() {
        return `${sourceText}  \r\n\r\n`;
      },
    };
    const formattingEdits = providers.formatting.provider.provideDocumentFormattingEdits(unformattedDocument);
    assert.strictEqual(formattingEdits.length, 1);
    assert.strictEqual(formattingEdits[0].newText, `${sourceText}\n`);

    await extension.deactivate();
  } finally {
    await extension.deactivate();
    await removeWorkspace(workspaceRoot);
  }
}

function uriFromPath(filePath) {
  return new FakeUri(pathToFileURL(filePath).href, filePath);
}

async function waitFor(callback, label) {
  for (let attempt = 0; attempt < 100; attempt += 1) {
    if (callback()) {
      return;
    }

    await new Promise((resolve) => setTimeout(resolve, 25));
  }

  throw new Error(`Timed out waiting for ${label}.\n${outputLines.join("")}`);
}

async function removeWorkspace(root) {
  for (let attempt = 0; attempt < 20; attempt += 1) {
    try {
      fs.rmSync(root, { recursive: true, force: true });
      return;
    } catch (error) {
      if (attempt === 19) {
        throw error;
      }

      await new Promise((resolve) => setTimeout(resolve, 100));
    }
  }
}

main().catch((error) => {
  console.error(error && error.stack ? error.stack : error);
  if (outputLines.length > 0) {
    console.error(outputLines.join(""));
  }
  process.exitCode = 1;
});
