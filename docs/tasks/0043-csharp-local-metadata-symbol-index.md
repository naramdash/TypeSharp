# Task: CSharp Local Metadata Symbol Index

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:51:23 +09:00
End Time: 2026-05-18 22:55:26 +09:00

## Objective

Referenced local `net481` C# DLL에서 public type/member metadata를 읽어 이후 overload, nullable, invalid byref validation이 실제 symbol data를 기준으로 동작할 수 있는 최소 metadata index를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0038-0042-csharp-member-byref-interop-smokes.md](0038-0042-csharp-member-byref-interop-smokes.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- local assembly metadata reader reads public top-level types
- public method and property names are indexed per type
- public method parameter names and byref modifiers are represented enough for follow-up validation
- smoke test builds a temporary `net481` DLL and verifies the metadata index
- checklist/traceability updates

Out:
- full overload resolution
- nullable attribute interpretation
- generic type/member signature decoding beyond simple display strings
- framework assembly metadata resolution
- invalid byref diagnostics

## Acceptance Criteria

- [x] local `net481` DLL public types are represented in `MetadataAssemblySymbol`.
- [x] public methods, properties, and parameters are represented in metadata symbols.
- [x] byref parameter modifiers can be observed from the metadata index.
- [x] existing placeholder behavior for framework assemblies remains intact.
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
- existing tests and new local metadata index smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:55:26 +09:00.

## Handoff

Done:
- Added local PE metadata reading for public top-level types, methods, properties, parameters, and byref modifiers.
- Added a smoke test that indexes a temporary `Legacy.Tools` `net481` DLL and verifies `params`/`out`/`in`/`ref` metadata shape.
- Updated checklist and traceability for the local public symbol index.

Remaining:
- Continue with metadata-backed overload/byref validation after this index exists.

Blocked:
- None.
