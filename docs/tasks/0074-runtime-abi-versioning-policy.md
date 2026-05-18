# Task: Runtime ABI Versioning Policy

Status: Done
Queue: Q3
Start Time: 2026-05-19 01:54:20 +09:00
End Time: 2026-05-19 02:00:11 +09:00

## Objective

`TypeSharp.Core`, `TypeSharp.Runtime`, generated `net48` assembly가 공유하는 public ABI versioning 정책을 문서화하고 compiler/runtime ABI constant alignment를 smoke test로 고정한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../standard-library.md](../standard-library.md)
- [../architecture.md](../architecture.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/TypeSharpCompilerInfo.cs`
- `src/TypeSharp.Runtime/TypeSharpRuntimeInfo.cs`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- runtime ABI policy document
- compiler/runtime `RuntimeAbiVersion` alignment rule
- ABI change rule and compatibility gate definitions
- release policy for ABI 0 and 1.0+
- smoke coverage for compiler/runtime ABI constant alignment
- checklist, traceability, architecture, standard library, and task queue updates

Out:
- full binary compatibility analyzer
- public ABI snapshot tests
- package versioning and signing policy
- migration guide
- release notes template

## Acceptance Criteria

- [x] `docs/runtime-abi.md` defines runtime/public ABI versioning policy.
- [x] docs link runtime ABI policy from standard library, architecture, and docs README.
- [x] tests verify compiler/runtime `RuntimeAbiVersion` constants stay aligned.
- [x] `public ABI versioning 정책` checklist item is marked complete while broader release policy items remain open.
- [x] standard build/test smoke still passes.

## Verification

Command:

```text
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj
dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj
dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
rg -n "net481|Net481|4\.8\.1" docs src tests agent.md -S
git diff --check
```

Expected:
- runtime project builds.
- test project builds.
- `runtime ABI constants are aligned` passes.
- support library builds, CLI build, and example CLI check pass.
- whitespace check has no errors.

Result:
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build src/TypeSharp.Compiler/TypeSharp.Compiler.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build src/TypeSharp.LanguageServer/TypeSharp.LanguageServer.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj` completed, including `runtime ABI constants are aligned`.
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj` completed with 0 warnings and 0 errors.
- Pass. `dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json` returned no diagnostics.
- Pass. `rg -n "net481|Net481|4\.8\.1" docs src tests agent.md -S` returns only intentional latest Framework profile comparison, official .NET Framework 4.8.1 reference text, and migration history/task documentation.
- Pass. `git diff --check` reported no whitespace errors.

## Handoff

Done:
- Added [../runtime-abi.md](../runtime-abi.md) for runtime/core/generated assembly public ABI policy.
- Linked runtime ABI policy from docs README, architecture, standard library, checklist, and traceability.
- Added smoke coverage for compiler/runtime `RuntimeAbiVersion` constant alignment.
- Kept the ABI policy anchored to generated `net48` assemblies and .NET Framework host compatibility.

Remaining:
- Full binary compatibility analyzer.
- Public ABI snapshot tests.
- Package versioning and signing policy.
- Migration guide.
- Release notes template.

Blocked:
- None.
