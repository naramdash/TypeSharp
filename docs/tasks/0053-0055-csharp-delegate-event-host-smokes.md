# Task Rollup: CSharp Delegate Event Host Smokes

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:44:52 +09:00
End Time: 2026-05-18 23:57:43 +09:00

## Objective

C#/.NET Framework interop smoke coverage를 delegate parameter, event add/remove, and ASP.NET/WCF/worker-style host reference까지 확장해 generated TypeSharp `net481` assembly가 기존 .NET Framework ecosystem에서 일반 C# library처럼 소비되는지 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../feasibility.md](../feasibility.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Parsing`
- `src/TypeSharp.Compiler/Backend`
- `tests/TypeSharp.Compiler.Tests`
- `tests/fixtures/backend/csharp/positive`

## Completed Packets

- 0053: C# delegate parameter interop smoke
  - emitted single-parameter lambda expressions as C# `parameter => expression`.
  - added a backend golden fixture for delegate-style lambda emission.
  - verified `LegacyDelegates.Apply(string, Func<string,string>)` generated `net481` build.

- 0054: .NET Framework application model host smokes
  - built a generated TypeSharp `net481` library.
  - built `TypeSharp.Core` and `TypeSharp.Runtime` as `net481` DLLs.
  - verified ASP.NET Web Forms-style `System.Web.UI.Page`, WCF `ServiceContract`/`OperationContract`, and Windows Service-style `ServiceBase` consumer projects can reference generated/Core/Runtime assemblies.

- 0055: C# event interop smoke
  - added lexer/parser support for `+=` and `-=`.
  - emitted assignment expressions through the C# backend.
  - verified imported event add/remove with lambda handlers compiles into a generated `net481` assembly.

## Scope

In:
- delegate parameter lambda emission and generated build smoke
- event add/remove assignment tokenization, parsing, emission, and generated build smoke
- ASP.NET/WCF/worker-style `net481` host reference smoke
- checklist and traceability updates

Out:
- typed, parenthesized, or multi-parameter lambda emission
- event metadata symbol indexing and event accessor diagnostics
- actual IIS, WCF endpoint configuration, or Windows Service installation/runtime execution
- contextual lambda overload ranking

## Acceptance Criteria

- [x] TypeSharp `text => text` emits C# `text => text`.
- [x] delegate parameter smoke compiles through `typesharp build`.
- [x] generated TypeSharp assembly, `TypeSharp.Core`, and `TypeSharp.Runtime` are referenced by ASP.NET/WCF/worker-style `net481` host projects.
- [x] TypeSharp source parses and emits event `+=` and `-=` assignments.
- [x] event add/remove smoke compiles through `typesharp build`.
- [x] checklist and traceability reflect the completed smoke coverage.

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
- delegate, host compatibility, and event smokes pass.
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
- Added delegate parameter lambda build smoke.
- Added ASP.NET/WCF/worker-style host reference smokes.
- Added event add/remove assignment support and build smoke.
- Updated checklist and traceability.

Remaining:
- Add typed and multi-parameter lambda emission before conventional `EventHandler`-style events are fully covered.
- Index event metadata symbols and validate event add/remove target types.
- Add real host runtime/package tests later if IIS packaging, WCF configuration generation, or Windows Service scaffolding enters scope.

Blocked:
- None.
