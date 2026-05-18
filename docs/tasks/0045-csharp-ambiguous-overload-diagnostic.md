# Task: CSharp Ambiguous Overload Diagnostic

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:00:00 +09:00
End Time: 2026-05-18 23:02:31 +09:00

## Objective

Local C# metadata index를 사용해 TypeSharp source의 imported C# static method call이 같은 arity의 여러 overload 후보와 매칭될 때 `TS2402` diagnostic을 내는 최소 overload ambiguity validation path를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [0043-csharp-local-metadata-symbol-index.md](0043-csharp-local-metadata-symbol-index.md)
- [0044-csharp-invalid-byref-diagnostic.md](0044-csharp-invalid-byref-diagnostic.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TS2402` descriptor registration
- metadata-backed ambiguity diagnostic for direct imported type static member calls with multiple same-arity candidates
- `typesharp check` and `typesharp build` coverage through the existing interop validator path
- smoke tests proving ambiguous overload is diagnosed and build stops before emission
- diagnostics/checklist/traceability updates

Out:
- full nominal overload ranking
- numeric conversion ranking
- generic inference
- optional/named/params overload ranking

## Acceptance Criteria

- [x] `TS2402` is present in `DiagnosticDescriptors.All`.
- [x] check reports `TS2402` when multiple same-arity C# metadata candidates exist.
- [x] build stops before generated C# emission when `TS2402` is present.
- [x] existing interop smokes still pass.
- [x] diagnostics docs reflect implemented `TS2402` and `TS2403` descriptors.
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
- existing tests and new ambiguous overload diagnostic smoke tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 23:02:31 +09:00.

## Handoff

Done:
- Added `TS2402` descriptor registration for ambiguous C# overloads.
- Extended `TypeSharpInteropValidator` to report same-arity imported static method overload ambiguity.
- Added checker and CLI build-stop smoke tests for ambiguous overload diagnostics.
- Updated diagnostics docs, checklist, and traceability.

Remaining:
- Continue with nominal overload ranking and nullable metadata validation.

Blocked:
- None.
