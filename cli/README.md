# TypeSharp CLI

This folder owns the command-line host for TypeSharp.

## Contents

- `TypeSharp.Cli`: the `typesharp` executable project. It wires user-facing commands such as `new`, `check`, `build`, `run`, `format`, `version`, `explain`, and `lsp` to the language implementation in `../lang`.

## Common Commands

```powershell
dotnet build cli\TypeSharp.Cli\TypeSharp.Cli.csproj
dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
```

The CLI depends on `lang/TypeSharp.Compiler` and `lang/TypeSharp.LanguageServer`; it should not own compiler semantics directly.
