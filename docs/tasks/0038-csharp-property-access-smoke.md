# Task: CSharp Property Access Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:29:37 +09:00
End Time: 2026-05-18 22:30:53 +09:00

## Objective

TypeSharp source가 referenced `net481` C# local DLL의 public property를 읽고 generated `net481` C# project가 해당 property access를 컴파일하는 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- temporary local `net481` C# DLL with a public property
- TypeSharp source that imports the C# type, constructs it, and reads the property
- generated C# source assertion for property access
- generated project build smoke proving the property access compiles
- checklist/traceability updates

Out:
- property setter/assignment
- indexer property
- nullable metadata diagnostics
- overload/property resolution semantics beyond compile smoke

## Acceptance Criteria

- [x] local `net481` DLL exposes a public property used by the smoke.
- [x] TypeSharp source imports the property-owning C# type.
- [x] generated C# source emits constructor and property access.
- [x] generated `net481` project compiles successfully.
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
- existing tests and new property access smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:30:53 +09:00.

## Handoff

Done:
- Added a CLI build smoke test that compiles imported C# property access from a referenced local `net481` DLL.

Remaining:
- Continue with indexed/member interop or byref/params interop smoke according to checklist priority.

Blocked:
- None.
