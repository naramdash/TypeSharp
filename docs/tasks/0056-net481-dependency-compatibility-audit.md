# Task: Net481 Dependency Compatibility Audit

Status: Done
Queue: Q3
Start Time: 2026-05-19 00:00:29 +09:00
End Time: 2026-05-19 00:04:01 +09:00

## Objective

Generated TypeSharp artifact와 `TypeSharp.Core`/`TypeSharp.Runtime`가 package-free `net481` surface를 유지하는지 감사하고, 현재 dependency inventory와 future dependency gate를 문서화한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../feasibility.md](../feasibility.md)
- [../checklist.md](../checklist.md)
- [../dependencies.md](../dependencies.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Core`
- `src/TypeSharp.Runtime`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TypeSharp.Core` and `TypeSharp.Runtime` `net481` artifact audit
- package-free project audit for runtime/core artifacts
- small static denylist scan for obvious .NET 5+ or package-backed runtime API drift
- dependency inventory documentation for generated artifact, Core, Runtime, compiler, CLI, test host
- checklist and traceability updates

Out:
- full binary API compatibility analyzer
- NuGet restore implementation
- package signing or checksum implementation
- transitive dependency lock file support
- compiler/CLI host downgrade to `net481`

## Acceptance Criteria

- [x] `TypeSharp.Core` and `TypeSharp.Runtime` are audited as `net481` and package-free.
- [x] Core/Runtime source is scanned for obvious .NET 5+ or package-backed runtime API drift.
- [x] dependency inventory records current external NuGet dependency state and license state.
- [x] future dependency gate records required `net481`, license, checksum/lock, deployment criteria.
- [x] checklist and traceability reflect the audit.
- [x] existing tests and standard build checks still pass.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
Get-ChildItem -Recurse -File -Filter *.csproj | Select-String -Pattern '<PackageReference'
```

Expected:
- dependency compatibility audit smoke passes.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.
- PackageReference search returns no project matches.

Result:
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.
- Pass. PackageReference project search returned no matches.

## Handoff

Done:
- Added `Net481RuntimeArtifactsAvoidExternalPackageDependencies` smoke test.
- Added [../dependencies.md](../dependencies.md) inventory and future dependency gate.
- Updated checklist and traceability for .NET 5+ API drift audit and dependency inventory.

Remaining:
- Add full binary compatibility analyzer or package lock/checksum policy when external package support is implemented.
- Revisit dependency inventory whenever a package reference, generated runtime helper, or host-specific dependency is proposed.

Blocked:
- None.
