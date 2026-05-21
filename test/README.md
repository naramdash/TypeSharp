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

The full runner remains the release-confidence gate. For faster local or CI wall-clock time, the same runner is also linked into four shard projects that execute disjoint `index % 4` slices:

```powershell
$shards = 0..3 | ForEach-Object { "test\TypeSharp.Compiler.Tests.Shard$_\TypeSharp.Compiler.Tests.Shard$_.csproj" }
foreach ($project in $shards) { dotnet build $project --nologo --verbosity quiet }
$jobs = foreach ($project in $shards) { Start-Job -ScriptBlock { param($p) dotnet run --project $p --no-build } -ArgumentList $project }
$jobs | Wait-Job | Receive-Job
```

MSTest SDK/Microsoft Testing Platform and xUnit.net v3 are reasonable future migration targets for `dotnet test` integration. The current shard path keeps the package-free runner while preserving all existing regression evidence; a framework migration should first extract the custom test catalog into discoverable test cases.
