# Task: CSharp Ref Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:46:48 +09:00
End Time: 2026-05-18 22:48:51 +09:00

## Objective

TypeSharp source가 referenced `net481` C# local DLL의 `ref` API를 호출하고 generated `net481` C# project가 해당 call-site를 컴파일하는 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0041-csharp-in-interop-smoke.md](0041-csharp-in-interop-smoke.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- lexer/parser/binder/backend support for `ref` call arguments
- temporary local `net481` C# DLL with a public `ref` method
- TypeSharp source that imports the C# type, declares a mutable local, calls the `ref` method, and returns the mutated local
- generated C# source assertion for `ref value`
- generated project build smoke proving the `ref` call compiles
- checklist/traceability updates

Out:
- addressability validation for `ref`
- byref escape analysis
- overload resolution by byref modifiers
- invalid byref diagnostics

## Acceptance Criteria

- [x] lexer and parser produce `ref` call arguments.
- [x] binder and backend handle `ref` call arguments.
- [x] local `net481` DLL exposes a public `ref` API used by the smoke.
- [x] TypeSharp source imports the C# type and calls the `ref` API.
- [x] generated C# source emits the `ref value` call-site.
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
- existing tests and new `ref` interop smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:48:51 +09:00.

## Handoff

Done:
- Added lexer/parser/binder/backend handling for `ref` call arguments.
- Added a CLI build smoke test that compiles an imported C# `ref` call from a referenced local `net481` DLL.
- Updated checklist, traceability, and next agent priority so metadata-backed C# interop validation remains as follow-up.

Remaining:
- Continue with metadata-backed byref validation after the compile smoke.

Blocked:
- None.
