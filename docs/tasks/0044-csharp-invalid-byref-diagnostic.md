# Task: CSharp Invalid Byref Diagnostic

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:56:12 +09:00
End Time: 2026-05-18 22:59:15 +09:00

## Objective

Local C# metadata index를 사용해 TypeSharp source의 `ref`/`out`/`in` call-site modifier가 imported C# method metadata와 맞지 않을 때 `TS2403` diagnostic을 내는 최소 validation path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [0043-csharp-local-metadata-symbol-index.md](0043-csharp-local-metadata-symbol-index.md)
- `src/TypeSharp.Compiler/Interop`
- `src/TypeSharp.Compiler/Checking`
- `src/TypeSharp.Compiler/Building`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TS2403` descriptor registration
- metadata-backed validation for direct imported type static member calls with a single same-arity metadata candidate
- mismatch diagnostics for `ref`/`out`/`in` versus method parameter byref metadata
- `typesharp check` and `typesharp build` diagnostic pipeline integration
- smoke tests proving invalid byref is diagnosed and build stops before emission
- checklist/traceability updates

Out:
- full overload resolution
- instance method receiver type inference
- nullable metadata diagnostics
- addressability and byref escape analysis
- generic candidate matching

## Acceptance Criteria

- [x] `TS2403` is present in `DiagnosticDescriptors.All`.
- [x] check reports `TS2403` when call-site byref modifier mismatches local C# method metadata.
- [x] build stops before generated C# emission when `TS2403` is present.
- [x] valid existing byref compile smokes remain accepted.
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
- existing tests and new invalid byref diagnostic smoke tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:59:15 +09:00.

## Handoff

Done:
- Added `TS2403` descriptor registration for invalid byref interop use.
- Added `TypeSharpInteropValidator` to compare parsed `ref`/`out`/`in` call-site modifiers with local C# method metadata.
- Integrated validator diagnostics into `typesharp check` and `typesharp build`; build stops before generated C# emission on `TS2403`.
- Added smoke tests for checker diagnostics and CLI build stop.
- Updated checklist and traceability.

Remaining:
- Continue with overload, nullable metadata, and richer metadata-backed interop validation.

Blocked:
- None.
