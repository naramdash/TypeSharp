# Task Rollup: CSharp Member And Byref Interop Smokes

Status: Done
Queue: Q3
Start Time: 2026-05-18 22:29:37 +09:00
End Time: 2026-05-18 22:48:51 +09:00
Rollup Time: 2026-05-18 22:49:43 +09:00

## Objective

Referenced `net481` C# local DLL APIs를 TypeSharp source에서 호출하고 generated `net481` C# project가 property, `params`, `out`, `in`, `ref` call shapes를 컴파일하는 smoke coverage를 묶는다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../checklist.md](../checklist.md)
- [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md)
- `src/TypeSharp.Compiler`
- `tests/TypeSharp.Compiler.Tests`

## Completed Work

- `0038-csharp-property-access-smoke`: imported `LegacyFormatter.Prefix` property read compiles through generated `net481` project.
- `0039-csharp-params-interop-smoke`: imported `LegacyParams.Join(string, params string[])` positional call compiles through generated `net481` project.
- `0040-csharp-out-interop-smoke`: parsed `out` argument emits `out value` and imported `LegacyByRef.TryParseCount(string, out int)` compiles.
- `0041-csharp-in-interop-smoke`: parsed `in` argument emits `in value` and imported `LegacyByRef.AddOne(in int)` compiles.
- `0042-csharp-ref-interop-smoke`: `ref` is lexed as a keyword, parsed as `RefArgument`, emits `ref value`, and imported `LegacyByRef.Increment(ref int)` compiles.

## Acceptance Criteria

- [x] local `net481` DLL exposes public property, `params`, `out`, `in`, and `ref` APIs used by smoke tests.
- [x] TypeSharp source imports the C# types and calls each API shape.
- [x] generated C# source preserves property access and byref/params call-site syntax.
- [x] generated `net481` projects compile successfully.
- [x] existing tests still pass.
- [x] checklist, traceability, and agent next-work docs are updated.

## Verification

Command:

```text
dotnet run --project tests/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj
dotnet build src/TypeSharp.Core/TypeSharp.Core.csproj
dotnet build src/TypeSharp.Runtime/TypeSharp.Runtime.csproj
dotnet build src/TypeSharp.Cli/TypeSharp.Cli.csproj
dotnet run --project src/TypeSharp.Cli/TypeSharp.Cli.csproj -- check docs/examples/cli-console/TypeSharp.toml --diagnostic-format json
```

Result:
- Passed on 2026-05-18 22:48:51 +09:00 after `0042`.

## Handoff

Done:
- Generated C# compile smoke now covers imported C# property access, `params`, `out`, `in`, and `ref` call-site shapes from a referenced local `net481` DLL.
- Related task packets `0038` through `0042` are compressed into this rollup.

Remaining:
- Metadata-backed C# interop validation remains open: overload selection, nullable metadata, invalid byref diagnostics, public ABI fixtures, and richer metadata symbol use.

Blocked:
- None.
