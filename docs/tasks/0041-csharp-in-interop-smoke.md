# Task: CSharp In Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:43:27 +09:00
End Time: 2026-05-18 22:46:03 +09:00

## Objective

TypeSharp source가 referenced `net481` C# local DLL의 `in` readonly byref API를 호출하고 generated `net481` C# project가 해당 call-site를 컴파일하는 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0040-csharp-out-interop-smoke.md](0040-csharp-out-interop-smoke.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- parser/binder/backend support for `in` call arguments
- temporary local `net481` C# DLL with a public `in` method
- TypeSharp source that imports the C# type, declares a local, and calls the `in` method
- generated C# source assertion for `in value`
- generated project build smoke proving the `in` call compiles
- checklist/traceability updates

Out:
- `ref` call-site lowering
- readonly byref escape analysis
- overload resolution by byref modifiers
- invalid byref diagnostics

## Acceptance Criteria

- [x] parser produces `in` call arguments.
- [x] binder and backend handle `in` call arguments.
- [x] local `net481` DLL exposes a public `in` API used by the smoke.
- [x] TypeSharp source imports the C# type and calls the `in` API.
- [x] generated C# source emits the `in value` call-site.
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
- existing tests and new `in` interop smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:46:03 +09:00.

## Handoff

Done:
- Added parser/binder/backend handling for `in` call arguments.
- Added a CLI build smoke test that compiles an imported C# `in` call from a referenced local `net481` DLL.
- Updated checklist, traceability, and next agent priority so `ref` lowering remains as follow-up.

Remaining:
- Continue with `ref` interop lowering.

Blocked:
- None.
