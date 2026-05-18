# Task: Async Task Interop Lowering

Status: Done
Queue: Q2-Q3
Start Time: 2026-05-19 04:30:48 +09:00
End Time: 2026-05-19 04:35:03 +09:00

## Objective

Connect the existing `async fun` and `await` grammar to generated C# `async`/`Task<T>` output so TypeSharp async APIs can compile into `net48` assemblies and be consumed from C# as normal `Task<T>` members.

## Source Of Truth

- [../goal.md](../goal.md)
- [../grammar/interop.md](../grammar/interop.md)
- [../grammar/expressions.md](../grammar/expressions.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- `async fun` C# source backend emission
- `await` expression C# source backend emission
- minimal `Task<T>` return checking for async functions
- backend golden fixture
- generated `net48` assembly and C# consumer smoke

Out:
- async `main` support
- non-generic `Task`/`unit` inference
- async block/task computation expression
- cancellation, synchronization context, exception-to-`Result` conversion helpers
- byref crossing async boundary diagnostics

## Acceptance Criteria

- [x] `async fun greeting(): Task<string>` emits `public static async Task<string> greeting()`.
- [x] `await Task.FromResult("value")` emits C# `await Task.FromResult("value")`.
- [x] Generated async TypeSharp assembly builds as `net48`.
- [x] C# `net48` consumer can await or synchronously consume the generated `Task<string>` API.
- [x] `docs/checklist.md` and `docs/traceability.md` reflect the implemented async Task interop evidence.

## Verification

Command:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
git diff --check
```

Expected:
- backend golden fixture includes async method and await emission.
- CLI build smoke compiles generated `net48` async assembly and a C# `net48` consumer.
- existing parser, binder, type checker, backend, CLI, LSP, runtime, and net48 smokes continue to pass.

Result:
- Pass. `dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
- Pass. `git diff --check`

## Handoff

Done:
- `CSharpSourceBackend` emits `async` function modifiers and C# `await` expressions.
- `TypeSharpTypeChecker` unwraps explicit async `Task<T>` return annotations for minimal body result checking.
- Added backend fixture `0020-async-task-interop`.
- Added CLI generated `net48` assembly and C# `net48` consumer smoke for generated `Task<string>` APIs.
- Marked `Task`/`Task<T>` async interop complete in the checklist and traceability docs.

Remaining:
- Async `main`, non-generic `Task`/`unit` inference, richer await type inference for external metadata, exception-to-`Result` helpers, and byref async-boundary diagnostics remain out of scope.

Blocked:
- None.
