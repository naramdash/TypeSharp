const assert = require("assert");
const childProcess = require("child_process");
const fs = require("fs");
const { EventEmitter } = require("events");
const Module = require("module");
const path = require("path");

const sentChunks = [];
const outputLines = [];
const providers = {};
const registrations = [];
const workspaceEvents = {};
const diagnostics = {
  calls: [],
  deleted: [],
  set(uri, items) {
    this.calls.push({ uri, items });
  },
  delete(uri) {
    this.deleted.push(uri);
  },
  dispose() {},
};
let spawnedProcess;
let mockedExtensionPath = "";

const originalExistsSync = fs.existsSync;
fs.existsSync = function existsSync(candidate) {
  const bundledServer = path.join(mockedExtensionPath, "server", "TypeSharp.LanguageServer.dll");
  if (candidate === bundledServer) {
    return true;
  }

  return originalExistsSync.call(this, candidate);
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

const documentUri = new FakeUri("file:///workspace/src/Main.tysh", "/workspace/src/Main.tysh");
const document = {
  languageId: "typesharp",
  uri: documentUri,
  version: 7,
  getText() {
    return 'namespace Samples\n\nexport fun greeting(): string = "Hello"';
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
      return disposable("hover");
    },
    registerDefinitionProvider(selector, provider) {
      providers.definition = { selector, provider };
      return disposable("definition");
    },
    registerCompletionItemProvider(selector, provider, ...triggers) {
      providers.completion = { selector, provider, triggers };
      return disposable("completion");
    },
    registerDocumentFormattingEditProvider(selector, provider) {
      providers.formatting = { selector, provider };
      return disposable("formatting");
    },
  },
  workspace: {
    workspaceFolders: [
      {
        name: "workspace",
        uri: new FakeUri("file:///workspace", "/workspace"),
      },
    ],
    textDocuments: [document],
    getConfiguration(section) {
      assert.strictEqual(section, "typesharp.languageServer");
      const values = {
        command: "",
        args: [],
        cwd: "/workspace",
      };
      return {
        get(key) {
          return values[key];
        },
      };
    },
    onDidOpenTextDocument(listener) {
      workspaceEvents.open = listener;
      return disposable("open");
    },
    onDidChangeTextDocument(listener) {
      workspaceEvents.change = listener;
      return disposable("change");
    },
    onDidCloseTextDocument(listener) {
      workspaceEvents.close = listener;
      return disposable("close");
    },
  },
  Uri: {
    parse(value) {
      return new FakeUri(value);
    },
    file(value) {
      return new FakeUri(`file://${value}`, value);
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

function disposable(name) {
  const item = { name, disposed: false, dispose() { this.disposed = true; } };
  registrations.push(item);
  return item;
}

childProcess.spawn = function spawn(command, args, options) {
  const process = new EventEmitter();
  process.stdout = new EventEmitter();
  process.stderr = new EventEmitter();
  process.stdin = {
    writable: true,
    write(chunk) {
      sentChunks.push(Buffer.from(chunk));
    },
  };
  process.killed = false;
  process.kill = function kill() {
    this.killed = true;
  };
  spawnedProcess = { command, args, options, process };
  return process;
};

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
    extensionPath: path.resolve(__dirname, ".."),
    subscriptions: [],
  };
  mockedExtensionPath = context.extensionPath;

  extension.activate(context);

  assert.strictEqual(spawnedProcess.command, "dotnet");
  assert.deepStrictEqual(spawnedProcess.args, [path.join(context.extensionPath, "server", "TypeSharp.LanguageServer.dll")]);
  assert.strictEqual(spawnedProcess.options.cwd, "/workspace");
  assert.strictEqual(spawnedProcess.options.windowsHide, true);
  assert.ok(providers.hover, "hover provider should be registered");
  assert.ok(providers.definition, "definition provider should be registered");
  assert.ok(providers.completion, "completion provider should be registered");
  assert.ok(providers.formatting, "formatting provider should be registered");
  assert.deepStrictEqual(providers.completion.triggers, [".", ":"]);

  const initialize = findLastRequest("initialize");
  assert.ok(initialize, "initialize request should be sent");
  emitMessage({ jsonrpc: "2.0", id: initialize.id, result: { capabilities: {} } });

  await waitFor(() => findNotification("initialized"), "initialized notification");
  await waitFor(() => findNotification("textDocument/didOpen"), "didOpen notification");
  assert.strictEqual(findNotification("textDocument/didOpen").params.textDocument.text, document.getText());

  emitMessage({
    jsonrpc: "2.0",
    method: "textDocument/publishDiagnostics",
    params: {
      uri: documentUri.toString(),
      diagnostics: [
        {
          range: {
            start: { line: 2, character: 11 },
            end: { line: 2, character: 19 },
          },
          severity: 1,
          code: "TS2201",
          source: "typesharp",
          message: "Cannot assign expression of type 'int' to 'string'.",
        },
      ],
    },
  });
  assert.strictEqual(diagnostics.calls.length, 1);
  assert.strictEqual(diagnostics.calls[0].items[0].code, "TS2201");

  const token = { isCancellationRequested: false };
  const position = new Position(2, 11);

  const hoverPromise = providers.hover.provider.provideHover(document, position, token);
  const hoverRequest = await waitFor(() => findLastRequest("textDocument/hover"), "hover request");
  emitMessage({
    jsonrpc: "2.0",
    id: hoverRequest.id,
    result: {
      contents: { kind: "markdown", value: "`greeting`: string" },
      range: lspRange(),
    },
  });
  const hover = await hoverPromise;
  assert.strictEqual(hover.contents.value, "`greeting`: string");
  assert.strictEqual(hover.contents.isTrusted, false);

  const definitionPromise = providers.definition.provider.provideDefinition(document, position, token);
  const definitionRequest = await waitFor(() => findLastRequest("textDocument/definition"), "definition request");
  emitMessage({
    jsonrpc: "2.0",
    id: definitionRequest.id,
    result: {
      uri: documentUri.toString(),
      range: lspRange(),
    },
  });
  const definition = await definitionPromise;
  assert.strictEqual(definition.uri.toString(), documentUri.toString());

  const completionPromise = providers.completion.provider.provideCompletionItems(document, position, token);
  const completionRequest = await waitFor(() => findLastRequest("textDocument/completion"), "completion request");
  emitMessage({
    jsonrpc: "2.0",
    id: completionRequest.id,
    result: [
      {
        label: "greeting",
        kind: 3,
        detail: "function",
      },
    ],
  });
  const completions = await completionPromise;
  assert.strictEqual(completions.length, 1);
  assert.strictEqual(completions[0].label, "greeting");
  assert.strictEqual(completions[0].detail, "function");

  const unformattedDocument = {
    languageId: "typesharp",
    uri: documentUri,
    version: 8,
    getText() {
      return "namespace Samples  \r\n\r\n\r\nexport fun greeting(): string = \"Hello\"  ";
    },
  };
  const formattingEdits = providers.formatting.provider.provideDocumentFormattingEdits(unformattedDocument);
  assert.strictEqual(formattingEdits.length, 1);
  assert.strictEqual(formattingEdits[0].newText, "namespace Samples\n\nexport fun greeting(): string = \"Hello\"\n");

  assert.ok(workspaceEvents.close, "close listener should be registered");
  await workspaceEvents.close(document);
  const didClose = await waitFor(() => findNotification("textDocument/didClose"), "didClose notification");
  assert.strictEqual(didClose.params.textDocument.uri, documentUri.toString());
  assert.strictEqual(diagnostics.deleted.at(-1).toString(), documentUri.toString());

  const deactivatePromise = extension.deactivate();
  const shutdown = await waitFor(() => findLastRequest("shutdown"), "shutdown request");
  emitMessage({ jsonrpc: "2.0", id: shutdown.id, result: null });
  await deactivatePromise;
  assert.ok(findNotification("exit"), "exit notification should be sent");
  assert.strictEqual(spawnedProcess.process.killed, true);
}

function lspRange() {
  return {
    start: { line: 2, character: 11 },
    end: { line: 2, character: 19 },
  };
}

function emitMessage(message) {
  const payload = Buffer.from(JSON.stringify(message), "utf8");
  const header = Buffer.from(`Content-Length: ${payload.length}\r\n\r\n`, "ascii");
  spawnedProcess.process.stdout.emit("data", Buffer.concat([header, payload]));
}

function parseSentMessages() {
  const buffer = Buffer.concat(sentChunks);
  const messages = [];
  let offset = 0;

  while (offset < buffer.length) {
    const headerEnd = buffer.indexOf("\r\n\r\n", offset);
    if (headerEnd < 0) {
      break;
    }

    const header = buffer.slice(offset, headerEnd).toString("ascii");
    const match = /Content-Length:\s*(\d+)/i.exec(header);
    assert.ok(match, `missing Content-Length in ${header}`);
    const length = Number.parseInt(match[1], 10);
    const payloadStart = headerEnd + 4;
    const payloadEnd = payloadStart + length;
    if (payloadEnd > buffer.length) {
      break;
    }

    messages.push(JSON.parse(buffer.slice(payloadStart, payloadEnd).toString("utf8")));
    offset = payloadEnd;
  }

  return messages;
}

function findLastRequest(method) {
  return parseSentMessages()
    .filter((message) => message.method === method && Object.prototype.hasOwnProperty.call(message, "id"))
    .at(-1);
}

function findNotification(method) {
  return parseSentMessages()
    .find((message) => message.method === method && !Object.prototype.hasOwnProperty.call(message, "id"));
}

async function waitFor(callback, label) {
  for (let attempt = 0; attempt < 50; attempt += 1) {
    const value = callback();
    if (value) {
      return value;
    }

    await new Promise((resolve) => setTimeout(resolve, 10));
  }

  throw new Error(`Timed out waiting for ${label}. Sent messages: ${JSON.stringify(parseSentMessages())}`);
}

main().catch((error) => {
  console.error(error && error.stack ? error.stack : error);
  if (outputLines.length > 0) {
    console.error(outputLines.join(""));
  }
  process.exitCode = 1;
});
