# TypeSharp Test Suite

This folder owns repository verification.

## Contents

- `TypeSharp.Compiler.Tests`: the custom smoke and regression test runner for compiler, CLI, LSP, runtime, docs, workflows, examples, and VS Code package contracts.
- `fixtures`: parser, binder, type-checker, and C# backend fixture inputs and snapshots.
- `tmp`: ignored scratch space used by tests.

## Common Commands

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```
