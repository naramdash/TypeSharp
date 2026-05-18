# Task Group: Foundation Bootstrap

Status: Done
Queue: Q1-Q2
Start Time: 2026-05-18 22:21:04 +09:00
End Time: 2026-05-18 22:23:28 +09:00

## Objective

TypeSharp 장기 실행과 초기 compiler 작업을 가능하게 하는 fixture 정책, parser 결정, compiler/CLI project skeleton, manifest/source discovery 기반을 고정한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../agentic-execution.md](../agentic-execution.md)
- [../parser-fixtures.md](../parser-fixtures.md)
- [../grammar/ambiguity.md](../grammar/ambiguity.md)
- [../grammar/precedence.md](../grammar/precedence.md)
- [../checklist.md](../checklist.md)
- `src/TypeSharp.Compiler`
- `src/TypeSharp.Cli`
- `tests/TypeSharp.Compiler.Tests`

## Compressed Tasks

- 0001: parser fixture format and golden diagnostics layout.
- 0002: grammar ambiguity review before parser implementation.
- 0003: compiler core, CLI, and smoke test project skeleton.
- 0004: `TypeSharp.toml` manifest parser and source discovery.
- 0005: parser precedence table.

Timing note:
- Exact original task start/end times were not captured before the timing convention was introduced.
- The recorded start/end times describe this rollup compaction on the current computer clock.

## Scope

In:
- task packet and fixture policy
- parser ambiguity and precedence documentation
- compiler/CLI/test skeleton projects
- manifest loading, manifest locating, source discovery
- initial checklist/traceability wiring

Out:
- full parser implementation
- semantic model
- generated assembly build
- runtime library implementation

## Acceptance Criteria

- [x] parser fixture format and snapshot locations are documented and tested.
- [x] ambiguity and precedence decisions are documented before parser implementation.
- [x] compiler, CLI, and package-free smoke test skeletons exist.
- [x] manifest loading/location and deterministic source discovery are implemented.
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
- fixture convention, manifest/source discovery, and CLI skeleton tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:23:28 +09:00.

## Handoff

Done:
- Foundational docs and skeleton code are compressed into this rollup.

Remaining:
- Parser implementation and coverage are tracked by [0006-0017-parser-implementation-and-coverage.md](0006-0017-parser-implementation-and-coverage.md).

Blocked:
- None.
