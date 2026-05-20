# TypeSharp Language Projects

This folder owns the language implementation and runtime surface.

## Contents

- `TypeSharp.Compiler`: parser, binder, checker, diagnostics, lowering, backend, project loading, and fixture conventions.
- `TypeSharp.LanguageServer`: LSP server used by the CLI and VS Code extension.
- `TypeSharp.Core`: package-free `net48` core helpers exposed to generated projects.
- `TypeSharp.Runtime`: package-free `net48` runtime helpers used by generated code.

Compiler and tooling projects can target the modern .NET SDK. User-facing generated artifacts and runtime libraries remain `net48`.
