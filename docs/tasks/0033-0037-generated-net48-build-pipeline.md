# Task Group: Generated Net48 Build Pipeline

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:16:26 +09:00
End Time: 2026-05-18 22:28:44 +09:00

## Objective

Generated C# backend snapshots를 `typesharp build`의 CLI-visible `net48` assembly 산출물, C# consumer smoke, manifest reference propagation까지 연결한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../cli.md](../cli.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0023-0032-runtime-cli-interop-backend-skeleton.md](0023-0032-runtime-cli-interop-backend-skeleton.md)
- `src/TypeSharp.Compiler`
- `src/TypeSharp.Cli`
- `tests/TypeSharp.Compiler.Tests`

## Compressed Tasks

- 0033: generated C# source compiles inside a temporary SDK-style `net48` C# project.
- 0034: `typesharp build` writes deterministic generated `.g.cs` source and `<ProjectName>.Generated.csproj`.
- 0035: `typesharp build` invokes offline-friendly `dotnet build` for the generated project and reports the generated DLL path.
- 0036: a C# `net48` consumer project references the generated TypeSharp DLL and compiles a call to the generated public API.
- 0037: manifest `references.assemblies` and `references.paths` are propagated as generated C# project `<Reference>` items, including relative local DLL `HintPath` values.

Timing note:
- Exact start/end times for 0033-0036 were not captured before the task timing convention was requested.
- The recorded start time is when timing began during 0037 on the current computer clock.

## Scope

In:
- generated C# `net48` compile smoke
- generated C# project scaffold emission
- generated project build invocation from `typesharp build`
- generated assembly path reporting
- generated TypeSharp assembly consumption from C#
- generated project reference item propagation for framework assemblies and local DLLs
- framework and local DLL static member call compile smokes
- imported constructor and instance member call compile smoke
- diagnostic-stop behavior before emission/build when references or source diagnostics fail
- checklist/traceability updates

Out:
- running generated executables
- full MSBuild integration
- incremental build cache
- NuGet package restore/reference policy
- overload resolution
- nullable metadata diagnostics
- delegate/event/byref interop
- broad public ABI snapshot coverage

## Acceptance Criteria

- [x] generated C# source compiles in a temporary `net48` C# project.
- [x] `typesharp build` writes generated C# source and SDK-style `net48` generated project scaffold.
- [x] `typesharp build` invokes generated project build for a clean project.
- [x] generated assembly path is reported by CLI output.
- [x] generated `net48` DLL exists after build.
- [x] C# `net48` consumer project references the generated TypeSharp DLL and calls a generated public API.
- [x] generated C# project includes deterministic reference items from `references.assemblies`.
- [x] generated C# project includes deterministic local DLL reference items from `references.paths`.
- [x] local DLL `HintPath` values are relative to the generated output root when possible.
- [x] reference diagnostics still stop before emission/build when references are missing.
- [x] generated project compiles a framework static member call from TypeSharp source.
- [x] generated project compiles a local DLL static member call from TypeSharp source.
- [x] generated project compiles imported constructor and instance member calls from TypeSharp source.
- [x] existing tests still pass.
- [x] checklist and traceability are updated.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Expected:
- existing tests and generated net48 build pipeline smoke tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:28:44 +09:00.

## Handoff

Done:
- Generated source, project scaffold, generated project build, assembly path reporting, C# consumer reference, generated project manifest reference propagation, framework static member calls, local DLL static member calls, and imported constructor/instance member calls are implemented and covered by smoke tests.
- Related task packets 0033-0037 were compacted into this rollup.

Remaining:
- Next implementation should focus on property interop smoke, starting with imported C# property access in generated `net48` projects.

Blocked:
- None.
