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

`TypeSharp.Compiler.Tests.MSTest` is a `net10.0` MSTest SDK/Microsoft Testing Platform bridge over the same extracted catalog:

```powershell
dotnet restore test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --locked-mode
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners"
dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --filter "FullyQualifiedName~CatalogCase" --minimum-expected-tests 550
```

The same MSTest bridge is also split into four package-based shard projects for `dotnet test`/MTP discovery coverage:

```powershell
$mstestShards = 0..3 | ForEach-Object { "test\TypeSharp.Compiler.Tests.MSTest.Shard$_\TypeSharp.Compiler.Tests.MSTest.Shard$_.csproj" }
foreach ($project in $mstestShards) { dotnet restore $project --locked-mode --verbosity minimal }
foreach ($project in $mstestShards) { dotnet build $project --no-restore --nologo --verbosity quiet }
dotnet test `
  --test-modules "test\TypeSharp.Compiler.Tests.MSTest.Shard*\bin\Debug\net10.0\TypeSharp.Compiler.Tests.MSTest.Shard*.dll" `
  --root-directory . `
  --max-parallel-test-modules 4 `
  --minimum-expected-tests 554 `
  --no-progress
```

The `554` package-shard expectation is the 550 shared catalog cases plus one `CatalogIsExposedForPackageRunners` smoke per shard.

The root `global.json` opts `dotnet test` into Microsoft Testing Platform mode for .NET 10 SDKs, so use MTP-supported options such as `--test-modules` and `--max-parallel-test-modules`. The root `NuGet.config` clears inherited package sources, maps the current MSTest bridge package graph to `nuget.org`, uses `nuget.org` for audit data, and stores restored packages under ignored `.nuget/packages`. The MSTest bridge exists for package-based discovery and ecosystem integration; the package-free main runner and four shard projects remain the faster release-confidence path.

## CI Regression Gate

`.github/workflows/regression.yml` runs on Windows for compiler, test, example, VS Code, and workflow changes. It pins .NET 10 and Node 24, restores the MSTest bridge projects in locked mode, builds the package-free main and shard runners, runs all four package-free shard runners in parallel, then builds and runs the focused MSTest bridge discovery smoke plus the four MSTest package shards through MTP module-level parallelism.
