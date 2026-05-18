# Task Group: Diagnostics And Semantics Skeleton

Status: Done
Queue: Q1-Q2
Start Time: 2026-05-18 22:21:04 +09:00
End Time: 2026-05-18 22:23:28 +09:00

## Objective

Parser 이후의 diagnostic descriptor registry, binder/name resolution skeleton, basic type checker skeleton, and semantic golden fixtures를 만든다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../diagnostics.md](../diagnostics.md)
- [../checklist.md](../checklist.md)
- [0006-0017-parser-implementation-and-coverage.md](0006-0017-parser-implementation-and-coverage.md)
- `src/TypeSharp.Compiler/Diagnostics`
- `src/TypeSharp.Compiler/Binding`
- `src/TypeSharp.Compiler/TypeChecking`
- `tests/fixtures/diagnostics`
- `tests/TypeSharp.Compiler.Tests`

## Compressed Tasks

- 0018: diagnostic code taxonomy and descriptor metadata.
- 0019: binder/name resolution skeleton and unresolved-name diagnostic.
- 0020: basic type mismatch type checker skeleton.
- 0021: type checker positive/negative fixtures.
- 0022: binder positive/negative fixtures.

Timing note:
- Exact original task start/end times were not captured before the timing convention was introduced.
- The recorded start/end times describe this rollup compaction on the current computer clock.

## Scope

In:
- diagnostic descriptor registry and metadata
- binder/name resolution skeleton
- basic type mismatch checking
- binder and type checker fixture conventions
- CLI/check/build diagnostic preservation

Out:
- full semantic model
- overload resolution
- nullable analysis
- public ABI checking
- exhaustive pattern analysis

## Acceptance Criteria

- [x] diagnostic descriptor codes and metadata are stable and tested.
- [x] binder reports unresolved names through golden fixtures.
- [x] type checker reports basic mismatch diagnostics through golden fixtures.
- [x] checker/build paths preserve parser, binder, type checker diagnostics.
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
- diagnostic registry, binder fixture, and type checker fixture tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:23:28 +09:00.

## Handoff

Done:
- Diagnostics and early semantic skeleton task packets are compressed into this rollup.

Remaining:
- Backend/runtime/interop skeleton work is tracked by [0023-0032-runtime-cli-interop-backend-skeleton.md](0023-0032-runtime-cli-interop-backend-skeleton.md).

Blocked:
- None.
