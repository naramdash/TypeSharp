# Task: CSharp Params Overload Validation

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:13:11 +09:00
End Time: 2026-05-18 23:16:27 +09:00

## Objective

Local C# metadata validator가 `params` parameter를 확장 arity 후보로 다뤄 `params` overload ambiguity와 exact match narrowing을 source emission 전에 검증하게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0046-csharp-exact-overload-ranking.md](0046-csharp-exact-overload-ranking.md)
- [0047-csharp-params-metadata-flag.md](0047-csharp-params-metadata-flag.md)
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- `TypeSharpInteropValidator` treats a trailing `params` parameter as applicable for expanded arguments
- exact match ranking compares expanded `params` arguments against the array element type
- byref validation maps expanded `params` arguments back to the trailing metadata parameter
- smoke tests cover exact expanded `params` overload narrowing and ambiguous expanded `params` overload diagnostics
- checklist/traceability updates

Out:
- optional parameter default value decoding
- named argument overload ranking
- generated C# lowering changes
- nullable metadata compatibility

## Acceptance Criteria

- [x] `params` methods are applicable when argument count is greater than fixed parameter count.
- [x] expanded `params` arguments participate in exact match narrowing.
- [x] ambiguous expanded `params` overloads report `TS2402`.
- [x] existing `ref`/`out`/`in` validation still works.
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
- exact expanded `params` overload call compiles through generated `net481` project.
- ambiguous expanded `params` overload call reports `TS2402` before emission.
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
- Added expanded `params` arity applicability to `TypeSharpInteropValidator`.
- Mapped expanded `params` arguments to the trailing metadata parameter for byref validation.
- Compared expanded `params` arguments against the array element type for exact match narrowing.
- Added exact and ambiguous expanded `params` overload smoke tests.
- Updated checklist and traceability.

Remaining:
- Continue with optional/named overload ranking and nullable metadata validation.

Blocked:
- None.
