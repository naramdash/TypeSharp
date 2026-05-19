const childProcess = require("child_process");
const fs = require("fs");
const path = require("path");
const vscode = require("vscode");

let activeClient;

function activate(context) {
  const output = vscode.window.createOutputChannel("TypeSharp");
  const diagnostics = vscode.languages.createDiagnosticCollection("typesharp");
  const client = new TypeSharpLanguageClient(context, output, diagnostics);
  activeClient = client;

  context.subscriptions.push(output, diagnostics, client);
  client.start();

  const selector = { language: "typesharp", scheme: "file" };
  context.subscriptions.push(
    vscode.workspace.onDidOpenTextDocument((document) => client.didOpen(document)),
    vscode.workspace.onDidChangeTextDocument((event) => client.didChange(event.document)),
    vscode.workspace.onDidCloseTextDocument((document) => client.didClose(document)),
    vscode.languages.registerHoverProvider(selector, {
      provideHover: (document, position, token) => client.provideHover(document, position, token),
    }),
    vscode.languages.registerDefinitionProvider(selector, {
      provideDefinition: (document, position, token) => client.provideDefinition(document, position, token),
    }),
    vscode.languages.registerCompletionItemProvider(
      selector,
      {
        provideCompletionItems: (document, position, token) => client.provideCompletionItems(document, position, token),
      },
      ".",
      ":"
    ),
    vscode.languages.registerDocumentFormattingEditProvider(selector, {
      provideDocumentFormattingEdits: (document) => client.provideDocumentFormattingEdits(document),
    })
  );

  for (const document of vscode.workspace.textDocuments) {
    client.didOpen(document);
  }
}

async function deactivate() {
  if (activeClient) {
    await activeClient.stop();
    activeClient = undefined;
  }
}

class TypeSharpLanguageClient {
  constructor(context, output, diagnostics) {
    this.context = context;
    this.output = output;
    this.diagnostics = diagnostics;
    this.nextId = 1;
    this.pending = new Map();
    this.openDocuments = new Set();
    this.buffer = Buffer.alloc(0);
    this.process = undefined;
    this.ready = Promise.resolve(false);
  }

  start() {
    const launch = resolveServerLaunch(this.context);
    if (!launch) {
      this.output.appendLine("TypeSharp language server was not found. Build src/TypeSharp.LanguageServer or configure typesharp.languageServer.command.");
      return;
    }

    this.output.appendLine(`Starting TypeSharp language server: ${launch.command} ${launch.args.join(" ")}`);
    this.process = childProcess.spawn(launch.command, launch.args, {
      cwd: launch.cwd,
      stdio: ["pipe", "pipe", "pipe"],
      windowsHide: true,
    });

    this.process.stdout.on("data", (chunk) => this.handleData(chunk));
    this.process.stderr.on("data", (chunk) => this.output.append(chunk.toString()));
    this.process.on("exit", (code, signal) => {
      this.output.appendLine(`TypeSharp language server exited with code ${code ?? "null"} and signal ${signal ?? "null"}.`);
      this.rejectPending(new Error("TypeSharp language server exited."));
      this.process = undefined;
    });

    const workspaceFolder = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders.length > 0
      ? vscode.workspace.workspaceFolders[0]
      : undefined;
    const rootUri = workspaceFolder ? workspaceFolder.uri.toString() : vscode.Uri.file(launch.cwd).toString();
    this.ready = this.request("initialize", {
      processId: process.pid,
      rootUri,
      workspaceFolders: workspaceFolder ? [{ uri: rootUri, name: workspaceFolder.name }] : [],
      capabilities: {},
    })
      .then(() => {
        this.notify("initialized", {});
        return true;
      })
      .catch((error) => {
        this.output.appendLine(`TypeSharp language server initialization failed: ${error.message}`);
        return false;
      });
  }

  async stop() {
    if (!this.process) {
      return;
    }

    try {
      await this.request("shutdown", null);
      this.notify("exit", null);
    } catch (error) {
      this.output.appendLine(`TypeSharp language server shutdown failed: ${error.message}`);
    }

    if (this.process && !this.process.killed) {
      this.process.kill();
    }
  }

  dispose() {
    void this.stop();
  }

  async didOpen(document) {
    if (!isTypeSharpDocument(document) || !(await this.ensureReady())) {
      return;
    }

    const uri = document.uri.toString();
    this.openDocuments.add(uri);
    this.notify("textDocument/didOpen", {
      textDocument: {
        uri,
        languageId: "typesharp",
        version: document.version,
        text: document.getText(),
      },
    });
  }

  async didChange(document) {
    if (!isTypeSharpDocument(document) || !(await this.ensureReady())) {
      return;
    }

    const uri = document.uri.toString();
    if (!this.openDocuments.has(uri)) {
      await this.didOpen(document);
      return;
    }

    this.notify("textDocument/didChange", {
      textDocument: {
        uri,
        version: document.version,
      },
      contentChanges: [
        {
          text: document.getText(),
        },
      ],
    });
  }

  async didClose(document) {
    if (!isTypeSharpDocument(document) || !(await this.ensureReady())) {
      return;
    }

    const uri = document.uri.toString();
    this.openDocuments.delete(uri);
    this.diagnostics.delete(document.uri);
    this.notify("textDocument/didClose", {
      textDocument: {
        uri,
      },
    });
  }

  async provideHover(document, position, token) {
    if (!(await this.ensureDocumentReady(document, token))) {
      return undefined;
    }

    const response = await this.request("textDocument/hover", documentPositionParams(document, position));
    if (!response || token.isCancellationRequested) {
      return undefined;
    }

    const value = response.contents && response.contents.value ? response.contents.value : "";
    if (!value) {
      return undefined;
    }

    const markdown = new vscode.MarkdownString(value);
    markdown.isTrusted = false;
    return new vscode.Hover(markdown, toVsCodeRange(response.range));
  }

  async provideDefinition(document, position, token) {
    if (!(await this.ensureDocumentReady(document, token))) {
      return undefined;
    }

    const response = await this.request("textDocument/definition", documentPositionParams(document, position));
    if (!response || token.isCancellationRequested) {
      return undefined;
    }

    return new vscode.Location(vscode.Uri.parse(response.uri), toVsCodeRange(response.range));
  }

  async provideCompletionItems(document, position, token) {
    if (!(await this.ensureDocumentReady(document, token))) {
      return undefined;
    }

    const response = await this.request("textDocument/completion", documentPositionParams(document, position));
    if (!Array.isArray(response) || token.isCancellationRequested) {
      return undefined;
    }

    return response.map((item) => {
      const completion = new vscode.CompletionItem(item.label, toVsCodeCompletionKind(item.kind));
      completion.detail = item.detail;
      return completion;
    });
  }

  provideDocumentFormattingEdits(document) {
    if (!isTypeSharpDocument(document)) {
      return [];
    }

    const text = document.getText();
    const formatted = formatSourceText(text);
    if (text.replace(/\r\n/g, "\n") === formatted) {
      return [];
    }

    return [
      vscode.TextEdit.replace(fullDocumentRange(document), formatted),
    ];
  }

  async ensureDocumentReady(document, token) {
    if (token.isCancellationRequested || !isTypeSharpDocument(document) || !(await this.ensureReady())) {
      return false;
    }

    if (!this.openDocuments.has(document.uri.toString())) {
      await this.didOpen(document);
    }

    return !token.isCancellationRequested;
  }

  async ensureReady() {
    if (!this.process) {
      return false;
    }

    return this.ready;
  }

  request(method, params) {
    if (!this.process || !this.process.stdin.writable) {
      return Promise.reject(new Error("TypeSharp language server is not running."));
    }

    const id = this.nextId++;
    const message = {
      jsonrpc: "2.0",
      id,
      method,
      params,
    };

    const promise = new Promise((resolve, reject) => {
      this.pending.set(id, { resolve, reject });
    });
    this.send(message);
    return promise;
  }

  notify(method, params) {
    if (!this.process || !this.process.stdin.writable) {
      return;
    }

    this.send({
      jsonrpc: "2.0",
      method,
      params,
    });
  }

  send(message) {
    const payload = Buffer.from(JSON.stringify(message), "utf8");
    const header = Buffer.from(`Content-Length: ${payload.length}\r\n\r\n`, "ascii");
    this.process.stdin.write(Buffer.concat([header, payload]));
  }

  handleData(chunk) {
    this.buffer = Buffer.concat([this.buffer, chunk]);

    while (true) {
      const headerEnd = this.buffer.indexOf("\r\n\r\n");
      if (headerEnd < 0) {
        return;
      }

      const header = this.buffer.slice(0, headerEnd).toString("ascii");
      const length = parseContentLength(header);
      if (!length) {
        this.output.appendLine("Received LSP message without Content-Length.");
        this.buffer = this.buffer.slice(headerEnd + 4);
        continue;
      }

      const messageEnd = headerEnd + 4 + length;
      if (this.buffer.length < messageEnd) {
        return;
      }

      const payload = this.buffer.slice(headerEnd + 4, messageEnd).toString("utf8");
      this.buffer = this.buffer.slice(messageEnd);

      try {
        this.handleMessage(JSON.parse(payload));
      } catch (error) {
        this.output.appendLine(`Could not parse LSP message: ${error.message}`);
      }
    }
  }

  handleMessage(message) {
    if (Object.prototype.hasOwnProperty.call(message, "id")) {
      const pending = this.pending.get(message.id);
      if (!pending) {
        return;
      }

      this.pending.delete(message.id);
      if (message.error) {
        pending.reject(new Error(message.error.message || "LSP request failed."));
      } else {
        pending.resolve(message.result);
      }
      return;
    }

    if (message.method === "textDocument/publishDiagnostics") {
      this.publishDiagnostics(message.params);
    }
  }

  publishDiagnostics(params) {
    if (!params || !params.uri || !Array.isArray(params.diagnostics)) {
      return;
    }

    const uri = vscode.Uri.parse(params.uri);
    const mapped = params.diagnostics.map((diagnostic) => {
      const item = new vscode.Diagnostic(
        toVsCodeRange(diagnostic.range),
        diagnostic.message || "",
        toVsCodeDiagnosticSeverity(diagnostic.severity)
      );
      item.code = diagnostic.code;
      item.source = diagnostic.source || "typesharp";
      return item;
    });
    this.diagnostics.set(uri, mapped);
  }

  rejectPending(error) {
    for (const pending of this.pending.values()) {
      pending.reject(error);
    }

    this.pending.clear();
  }
}

function resolveServerLaunch(context) {
  const config = vscode.workspace.getConfiguration("typesharp.languageServer");
  const configuredCommand = config.get("command");
  const configuredArgs = config.get("args") || [];
  const configuredCwd = config.get("cwd");
  const workspaceFolder = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders.length > 0
    ? vscode.workspace.workspaceFolders[0].uri.fsPath
    : undefined;
  const cwd = configuredCwd || workspaceFolder || context.extensionPath;

  if (configuredCommand) {
    return {
      command: configuredCommand,
      args: configuredArgs,
      cwd,
    };
  }

  const bundledDll = path.join(context.extensionPath, "server", "TypeSharp.LanguageServer.dll");
  if (fs.existsSync(bundledDll)) {
    return {
      command: "dotnet",
      args: [bundledDll],
      cwd,
    };
  }

  const repositoryRoot = path.resolve(context.extensionPath, "..", "..");
  const developmentDll = path.join(repositoryRoot, "src", "TypeSharp.LanguageServer", "bin", "Debug", "net10.0", "TypeSharp.LanguageServer.dll");
  if (fs.existsSync(developmentDll)) {
    return {
      command: "dotnet",
      args: [developmentDll],
      cwd,
    };
  }

  const developmentProject = path.join(repositoryRoot, "src", "TypeSharp.LanguageServer", "TypeSharp.LanguageServer.csproj");
  if (fs.existsSync(developmentProject)) {
    return {
      command: "dotnet",
      args: ["run", "--project", developmentProject, "--no-build", "--no-restore"],
      cwd,
    };
  }

  return undefined;
}

function isTypeSharpDocument(document) {
  return document.languageId === "typesharp" && document.uri.scheme === "file";
}

function formatSourceText(text) {
  const normalized = text.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
  const lines = normalized.split("\n");
  const formattedLines = [];
  let blankLineCount = 0;

  for (let index = 0; index < lines.length; index += 1) {
    if (index === lines.length - 1 && lines[index].length === 0) {
      continue;
    }

    const line = lines[index].replace(/[ \t]+$/u, "");
    if (line.length === 0) {
      blankLineCount += 1;
      if (blankLineCount <= 1 && formattedLines.length > 0) {
        formattedLines.push("");
      }

      continue;
    }

    blankLineCount = 0;
    formattedLines.push(line);
  }

  while (formattedLines.length > 0 && formattedLines[formattedLines.length - 1].length === 0) {
    formattedLines.pop();
  }

  return `${formattedLines.join("\n")}\n`;
}

function fullDocumentRange(document) {
  const text = document.getText();
  const normalized = text.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
  const lines = normalized.split("\n");
  const lastLine = Math.max(lines.length - 1, 0);
  const lastCharacter = lines[lastLine] ? lines[lastLine].length : 0;
  return new vscode.Range(new vscode.Position(0, 0), new vscode.Position(lastLine, lastCharacter));
}

function documentPositionParams(document, position) {
  return {
    textDocument: {
      uri: document.uri.toString(),
    },
    position: {
      line: position.line,
      character: position.character,
    },
  };
}

function toVsCodeRange(range) {
  if (!range) {
    return undefined;
  }

  return new vscode.Range(
    new vscode.Position(range.start.line, range.start.character),
    new vscode.Position(range.end.line, range.end.character)
  );
}

function toVsCodeDiagnosticSeverity(severity) {
  switch (severity) {
    case 1:
      return vscode.DiagnosticSeverity.Error;
    case 2:
      return vscode.DiagnosticSeverity.Warning;
    case 3:
      return vscode.DiagnosticSeverity.Information;
    case 4:
      return vscode.DiagnosticSeverity.Hint;
    default:
      return vscode.DiagnosticSeverity.Error;
  }
}

function toVsCodeCompletionKind(kind) {
  switch (kind) {
    case 3:
      return vscode.CompletionItemKind.Function;
    case 6:
      return vscode.CompletionItemKind.Variable;
    case 7:
      return vscode.CompletionItemKind.Class;
    case 9:
      return vscode.CompletionItemKind.Module;
    case 12:
      return vscode.CompletionItemKind.Value;
    case 14:
      return vscode.CompletionItemKind.Keyword;
    case 18:
      return vscode.CompletionItemKind.Reference;
    default:
      return vscode.CompletionItemKind.Text;
  }
}

function parseContentLength(header) {
  for (const line of header.split("\r\n")) {
    const separator = line.indexOf(":");
    if (separator < 0) {
      continue;
    }

    const name = line.slice(0, separator).trim().toLowerCase();
    if (name !== "content-length") {
      continue;
    }

    const length = Number.parseInt(line.slice(separator + 1).trim(), 10);
    return Number.isFinite(length) ? length : 0;
  }

  return 0;
}

module.exports = {
  activate,
  deactivate,
  formatSourceText,
};
