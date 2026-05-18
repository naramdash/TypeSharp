# Task: CSharp Named Argument Overload Validation

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:30:29 +09:00
End Time: 2026-05-18 23:33:23 +09:00

## Objective

Parsed TypeSharp named arguments를 generated C# named arguments로 낮추고, local C# metadata validator가 parameter name을 사용해 overload 후보를 필터링하게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../checklist.md](../checklist.md)
- [0050-csharp-optional-parameter-overload-validation.md](0050-csharp-optional-parameter-overload-validation.md)
- `src/TypeSharp.Compiler/Backend`
- `src/TypeSharp.Compiler/Interop`
- `tests/TypeSharp.Compiler.Tests`

## Scope

In:
- C# backend emits `NamedArgument` as `name: value`
- interop validator matches named arguments to metadata parameter names
- named argument parameter matching participates in byref validation and exact type narrowing
- smoke test proves a named optional overload call compiles through generated `net481` project
- checklist/traceability updates

Out:
- diagnostics for unknown named parameter names
- duplicate named argument diagnostics
- named `ref`/`out`/`in` syntax
- full C# overload tie-break ordering

## Acceptance Criteria

- [x] generated C# preserves TypeSharp named arguments as C# named arguments.
- [x] metadata validator rejects candidates that do not contain the named parameter.
- [x] named argument expressions participate in exact match type inference.
- [x] named optional overload call compiles through generated `net481` project.
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
- named optional overload call compiles through generated `net481` project.
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
- Emitted TypeSharp `NamedArgument` nodes as C# `name: value`.
- Matched named arguments to metadata parameter names in interop validation.
- Reused named argument inner expressions for exact type inference and byref checks.
- Added generated `net481` smoke for named optional overload disambiguation.
- Updated checklist and traceability.

Remaining:
- Continue with duplicate/unknown named argument diagnostics and nullable metadata validation.

Blocked:
- None.
