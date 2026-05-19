const assert = require("assert");
const childProcess = require("child_process");
const fs = require("fs");
const os = require("os");
const path = require("path");

const extensionRoot = path.resolve(__dirname, "..");
const serverDll = path.join(extensionRoot, "server", "TypeSharp.LanguageServer.dll");
assert.ok(fs.existsSync(serverDll), "Run npm run prepare:server before npm run test:host.");

const workspaceRoot = fs.mkdtempSync(path.join(os.tmpdir(), "typesharp-vscode-host-"));
const userDataDir = path.join(workspaceRoot, ".vscode-user-data");
const extensionsDir = path.join(workspaceRoot, ".vscode-extensions");
const sourceRoot = path.join(workspaceRoot, "src");
fs.mkdirSync(sourceRoot, { recursive: true });

fs.writeFileSync(
  path.join(workspaceRoot, "TypeSharp.toml"),
  [
    "[project]",
    'name = "VsCodeHostSmoke"',
    'targetFramework = "net48"',
    'outputType = "library"',
    'rootNamespace = "Samples.VsCodeHostSmoke"',
    'sourceRoots = ["src"]',
    'generatedOutputRoot = "generated"',
    "",
    "[language]",
    'version = "preview"',
    "strict = true",
    'nullable = "strict"',
    "previewFeatures = []",
    "",
    "[references]",
    'assemblies = ["System", "System.Core"]',
    "paths = []",
    "packages = []",
    "",
  ].join("\n"),
  "utf8"
);

fs.writeFileSync(
  path.join(sourceRoot, "Main.tysh"),
  [
    "namespace Samples.VsCodeHostSmoke",
    "",
    "export fun greeting(name: string): string = name",
    "",
    'export fun main(): string = greeting("TypeSharp")',
    "",
    "export fun unsafeReturn(input: string?): string = input",
    "",
  ].join("\n"),
  "utf8"
);

fs.writeFileSync(
  path.join(sourceRoot, "Format.tysh"),
  "namespace Samples.Format  \r\n\r\n\r\nexport fun greeting(): string = \"ok\"  ",
  "utf8"
);

const codeExecutable = resolveCodeExecutable();
const args = [
  "--new-window",
  "--skip-welcome",
  "--skip-release-notes",
  "--disable-workspace-trust",
  "--disable-extensions",
  "--user-data-dir",
  userDataDir,
  "--extensions-dir",
  extensionsDir,
  "--extensionDevelopmentPath",
  extensionRoot,
  "--extensionTestsPath",
  path.join(extensionRoot, "test", "extension-host-smoke.js"),
  workspaceRoot,
];

try {
  const result = childProcess.spawnSync(codeExecutable, args, {
    env: {
      ...process.env,
      TYPESHARP_EXTENSION_HOST_SMOKE_WORKSPACE: workspaceRoot,
    },
    stdio: "inherit",
    timeout: 180000,
  });

  if (result.error) {
    throw result.error;
  }

  process.exitCode = result.status || 0;
} finally {
  fs.rmSync(workspaceRoot, { recursive: true, force: true });
}

function resolveCodeExecutable() {
  if (process.env.VSCODE_BIN) {
    return process.env.VSCODE_BIN;
  }

  if (process.platform === "win32") {
    const candidates = [
      process.env.LOCALAPPDATA && path.join(process.env.LOCALAPPDATA, "Programs", "Microsoft VS Code", "Code.exe"),
      process.env.ProgramFiles && path.join(process.env.ProgramFiles, "Microsoft VS Code", "Code.exe"),
      process.env["ProgramFiles(x86)"] && path.join(process.env["ProgramFiles(x86)"], "Microsoft VS Code", "Code.exe"),
    ].filter(Boolean);

    const candidate = candidates.find((item) => fs.existsSync(item));
    if (candidate) {
      return candidate;
    }
  }

  return "code";
}
