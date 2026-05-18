# Task: NetFx Application Model Host Smokes

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:50:09 +09:00
End Time: 2026-05-18 23:53:14 +09:00

## Objective

Generated TypeSharp `net481` assembly와 `TypeSharp.Core`/`TypeSharp.Runtime` DLL이 ASP.NET Web Forms, WCF, worker-style .NET Framework host project에서 일반 class library처럼 참조되는지 smoke test로 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../feasibility.md](../feasibility.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- generated TypeSharp `net481` library build
- `TypeSharp.Core` and `TypeSharp.Runtime` `net481` DLL builds
- ASP.NET Web Forms-style consumer project referencing `System.Web` and TypeSharp assemblies
- WCF contract/service-style consumer project referencing `System.ServiceModel` and TypeSharp assemblies
- Windows Service/worker-style consumer project referencing `System.ServiceProcess` and TypeSharp assemblies
- checklist and traceability updates

Out:
- ASP.NET project template generation
- IIS packaging, `web.config`, or AppDomain lifecycle runtime tests
- WCF endpoint/binding/config generation
- actual Windows Service installation or Service Control Manager execution

## Acceptance Criteria

- [x] `typesharp build` produces a generated `net481` library assembly for the host smoke.
- [x] `TypeSharp.Core` and `TypeSharp.Runtime` compile to `net481` DLLs.
- [x] ASP.NET Web Forms-style host project compiles with references to generated assembly, Core, Runtime, and `System.Web`.
- [x] WCF contract/service-style host project compiles with references to generated assembly, Core, Runtime, and `System.ServiceModel`.
- [x] Windows Service/worker-style host project compiles with references to generated assembly, Core, Runtime, and `System.ServiceProcess`.
- [x] checklist and traceability reflect the smoke coverage.

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
- host compatibility smoke passes.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Pass. `dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj`
- Pass. `dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj`
- Pass. `dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj`
- Pass. CLI check returned `{ "diagnostics": [] }`.

## Handoff

Done:
- Added one integrated host compatibility smoke covering ASP.NET Web Forms-style, WCF contract/service-style, and Windows Service/worker-style `net481` consumer projects.
- Verified generated TypeSharp assembly plus Core/Runtime DLL references compile in those projects.
- Updated checklist and traceability for the three application model smoke items.

Remaining:
- Add real host runtime/package tests later if IIS packaging, WCF configuration generation, or Windows Service scaffolding enters scope.
- Keep host-specific templates in Stable Backlog until generated assembly ABI and public metadata policies are more complete.

Blocked:
- None.
