# Task: CSharp Event Interop Smoke

Status: Done
Queue: Q3
Start Time: 2026-05-18 23:54:31 +09:00
End Time: 2026-05-18 23:57:43 +09:00

## Objective

C# event add/remove call-site를 TypeSharp source에서 표현하고 generated C# source가 `+=`/`-=` event assignment와 lambda handler를 보존해 `net481` assembly로 컴파일되는지 검증한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- `src/TypeSharp.Compiler/Parsing`
- `src/TypeSharp.Compiler/Backend`
- `tests/TypeSharp.Compiler.Tests`
- `tests/fixtures/backend/csharp/positive`

## Scope

In:
- lexer tokens for `+=` and `-=`
- parser assignment expressions using `+=` and `-=`
- C# backend assignment expression emission
- backend golden fixture for event add/remove assignments
- local `net481` C# DLL fixture with an event
- CLI build smoke verifying generated `net481` assembly output
- checklist and traceability updates

Out:
- event metadata symbol indexing
- event accessor validation diagnostics
- typed or multi-parameter lambda emission
- event handler identity analysis
- actual event runtime behavior assertions

## Acceptance Criteria

- [x] TypeSharp source can parse `source.Event += handler` and `source.Event -= handler` assignment expressions.
- [x] generated C# preserves `+=` and `-=` assignment operators.
- [x] backend snapshot covers event assignment emission.
- [x] `typesharp build` compiles an imported C# event add/remove lambda call into a generated `net481` assembly.
- [x] checklist and traceability reflect event interop smoke coverage.
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
- event add/remove interop smoke passes.
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
- Added lexer/parser support for `+=` and `-=`.
- Added C# backend assignment expression emission.
- Added a backend golden fixture for event add/remove assignment emission.
- Added a local C# event fixture and generated build smoke.
- Updated checklist and traceability for event interop smoke coverage.

Remaining:
- Index event metadata symbols and validate event add/remove target types.
- Add multi-parameter and typed lambda emission before conventional `EventHandler`-style events are fully covered.

Blocked:
- None.
