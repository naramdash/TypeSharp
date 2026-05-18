# Task: CSharp Unknown Nullability Diagnostic

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:37:17 +09:00
End Time: 2026-05-18 23:41:00 +09:00

## Objective

Local C# metadata에서 nullable annotation이 없는 imported reference return을 strict nullable mode에서 unknown nullability warning으로 보고해 nullable metadata validation의 첫 diagnostic path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [0043-0048-csharp-metadata-backed-interop-validation.md](0043-0048-csharp-metadata-backed-interop-validation.md)
- `src/TypeSharp.Compiler/Interop`
- `src/TypeSharp.Compiler/Diagnostics`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- diagnostic descriptor for unknown C# nullability
- local metadata reader marks reference returns without nullable metadata as unknown nullability
- interop validator reports a strict-mode warning for selected imported static method calls returning unknown-nullability reference types
- smoke test verifies warning diagnostic on a nullable-oblivious legacy C# return
- checklist/traceability updates

Out:
- full `NullableAttribute` byte payload decoding
- parameter nullability compatibility
- generated nullable metadata emit
- guard insertion or flow analysis
- warning-as-error policy

## Acceptance Criteria

- [x] `TS2404` is present in `DiagnosticDescriptors.All`.
- [x] metadata method symbols expose return nullability state.
- [x] nullable-oblivious reference returns are marked as unknown.
- [x] strict mode reports `TS2404` warning for an imported C# call returning unknown nullability.
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
- unknown nullability warning smoke passes.
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
- Added `TS2404` warning descriptor.
- Added `MetadataNullabilityKind` and method return nullability state.
- Marked nullable-oblivious reference returns as unknown when no nullable metadata is present.
- Passed manifest nullable mode into interop validation.
- Reported `TS2404` in strict nullable mode for selected imported C# calls returning unknown-nullability reference types.
- Updated diagnostics docs, checklist, and traceability.

Remaining:
- Decode nullable attribute payloads, validate parameters, and add guard/non-null assignment diagnostics.

Blocked:
- None.
