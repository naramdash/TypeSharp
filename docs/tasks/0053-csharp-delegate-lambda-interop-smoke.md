# Task: CSharp Delegate Lambda Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:44:52 +09:00
End Time: 2026-05-18 23:47:39 +09:00

## Objective

C# delegate parameter를 받는 local `net481` DLL method에 TypeSharp lambda expression을 전달하고, generated C# source와 generated `net481` assembly build가 모두 통과하는 delegate interop smoke를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Backend`
- `tests/TypeSharp.Compiler.Tests`
- `tests/fixtures/backend/csharp/positive`

## Scope

In:
- single-parameter `LambdaExpression` C# backend emission
- backend golden fixture for lambda expression emission
- temporary `LegacyDelegates.Apply(string, Func<string,string>)` local DLL metadata/build smoke
- CLI build smoke verifying generated `net481` assembly output
- checklist and traceability updates

Out:
- parenthesized or typed lambda parameter list emission
- event add/remove lowering with `+=` and `-=`
- contextual lambda overload ranking
- delegate variance, capture analysis, async delegate interop

## Acceptance Criteria

- [x] TypeSharp `text => text` emits C# `text => text`.
- [x] backend snapshot covers lambda expression emission.
- [x] local C# metadata reader indexes the delegate-parameter method.
- [x] `typesharp build` compiles a call to `LegacyDelegates.Apply("value", text => text)` into a generated `net481` assembly.
- [x] checklist keeps delegate parameter smoke complete while event interop remains open.
- [x] existing tests and standard build checks still pass.

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
- delegate lambda interop smoke passes.
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
- Added C# backend emission for single-parameter lambda expressions.
- Added a backend golden fixture for delegate-style lambda emission.
- Added a `LegacyDelegates.Apply` local DLL fixture and generated build smoke.
- Updated checklist and traceability for delegate parameter interop.

Remaining:
- Implement event add/remove lowering and validation.
- Add contextual lambda overload ranking once overload resolution grows beyond arity/exact primitive matching.

Blocked:
- None.
