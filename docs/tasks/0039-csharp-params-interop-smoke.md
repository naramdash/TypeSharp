# Task: CSharp Params Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:37:24 +09:00
End Time: 2026-05-18 22:39:26 +09:00

## Objective

TypeSharp source가 referenced `net481` C# local DLL의 `params` API를 일반 positional call shape로 호출하고 generated `net481` C# project가 해당 호출을 컴파일하는 smoke path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md)
- [0038-csharp-property-access-smoke.md](0038-csharp-property-access-smoke.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- temporary local `net481` C# DLL with a public `params` method
- TypeSharp source that imports the C# type and calls the `params` method with multiple positional arguments
- generated C# source assertion for the emitted call
- generated project build smoke proving the `params` call compiles
- checklist/traceability updates

Out:
- `ref`, `out`, or `in` call-site lowering
- overload resolution for competing `params` and non-`params` candidates
- array literal lowering for explicit `params` arrays
- nullable metadata diagnostics

## Acceptance Criteria

- [x] local `net481` DLL exposes a public `params` API used by the smoke.
- [x] TypeSharp source imports the C# type and calls the `params` API.
- [x] generated C# source emits the positional call accepted by C# `params`.
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
- existing tests and new `params` interop smoke test pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:39:26 +09:00.

## Handoff

Done:
- Added a CLI build smoke test that compiles an imported C# `params` call from a referenced local `net481` DLL.
- Updated checklist, traceability, and next agent priority so `out`/`ref`/`in` lowering remains as follow-up.

Remaining:
- Continue with `out`/`ref`/`in` interop lowering.

Blocked:
- None.
