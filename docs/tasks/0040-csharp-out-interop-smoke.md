# Task: CSharp Out Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:40:35 +09:00
End Time: 2026-05-18 22:42:28 +09:00

## Objective

TypeSharp source가 referenced `net481` C# local DLL의 `out` API를 호출하고 generated `net481` C# project가 해당 call-site를 컴파일하는 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md)
- [0039-csharp-params-interop-smoke.md](0039-csharp-params-interop-smoke.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- C# backend emission for parsed `out` call arguments
- temporary local `net481` C# DLL with a public `out` method
- TypeSharp source that imports the C# type, declares a mutable local, calls the `out` method, and returns the assigned local
- generated C# source assertion for `out value`
- generated project build smoke proving the `out` call compiles
- checklist/traceability updates

Out:
- `ref` or `in` call-site lowering
- definite assignment and flow analysis around `out`
- overload resolution by byref modifiers
- invalid byref diagnostics

## Acceptance Criteria

- [x] backend emits `out` call arguments.
- [x] local `net481` DLL exposes a public `out` API used by the smoke.
- [x] TypeSharp source imports the C# type and calls the `out` API.
- [x] generated C# source emits the `out value` call-site.
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
- existing tests and new `out` interop smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:42:28 +09:00.

## Handoff

Done:
- Added C# backend emission for parsed `out` arguments.
- Added a CLI build smoke test that compiles an imported C# `out` call from a referenced local `net481` DLL.
- Updated checklist, traceability, and next agent priority so `ref`/`in` lowering remains as follow-up.

Remaining:
- Continue with `ref`/`in` interop lowering.

Blocked:
- None.
