# Task Group: Runtime CLI Interop Backend Skeleton

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-18 22:21:04 +09:00
End Time: 2026-05-18 22:23:28 +09:00

## Objective

Runtime/Core `net481` libraries, first C# source backend, `typesharp build` generated source emission, C# reference/metadata skeleton, and backend import/call/block lowering skeletons를 연결한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../architecture.md](../architecture.md)
- [../cli.md](../cli.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../standard-library.md](../standard-library.md)
- [../checklist.md](../checklist.md)
- [0018-0022-diagnostics-and-semantics-skeleton.md](0018-0022-diagnostics-and-semantics-skeleton.md)
- `src/TypeSharp.Compiler`
- `src/TypeSharp.Core`
- `src/TypeSharp.Runtime`
- `src/TypeSharp.Cli`
- `tests/fixtures/backend/csharp`
- `tests/TypeSharp.Compiler.Tests`

## Compressed Tasks

- 0023: first C# 7.3 source backend golden output.
- 0024: `TypeSharp.Runtime` `net481` skeleton.
- 0025: CLI build generated C# source emission.
- 0026: `TypeSharp.Core` `Option<T>`, `Result<T,E>`, and `Unit` skeleton.
- 0027: C# framework/local reference resolver skeleton.
- 0028: metadata reader skeleton.
- 0029: check/build metadata diagnostics pipeline integration.
- 0030: C# import directive backend skeleton.
- 0031: identifier/member/call expression backend skeleton.
- 0032: block body and local `let` backend skeleton.

Timing note:
- Exact original task start/end times were not captured before the timing convention was introduced.
- The recorded start/end times describe this rollup compaction on the current computer clock.

## Scope

In:
- C# source backend golden fixtures
- runtime/core `net481` project skeletons
- CLI generated source emission
- C# reference resolver and metadata placeholder pipeline
- generated C# import, call expression, block, and local lowering skeletons
- diagnostics stopping build emission on errors

Out:
- generated project build invocation
- generated assembly consumption from C#
- manifest reference propagation into generated project
- full metadata reflection
- overload resolution
- nullable metadata analysis

## Acceptance Criteria

- [x] first C# source backend golden fixture is deterministic.
- [x] runtime/core `net481` projects build.
- [x] `typesharp build` emits generated C# source and stops on diagnostics.
- [x] reference resolver and metadata reader skeletons preserve diagnostics.
- [x] generated C# backend emits import directives, simple calls, block bodies, local `let`, and final returns.
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
- backend fixture, runtime/core, CLI build emission, reference resolver, metadata reader, and metadata pipeline tests pass.
- support libraries and CLI build without errors.
- CLI check reports no diagnostics for the example project.

Result:
- Passed on 2026-05-18 22:23:28 +09:00.

## Handoff

Done:
- Runtime, CLI, interop, and C# backend skeleton task packets are compressed into this rollup.

Remaining:
- Generated assembly build pipeline work is tracked by [0033-0037-generated-net481-build-pipeline.md](0033-0037-generated-net481-build-pipeline.md).

Blocked:
- None.
