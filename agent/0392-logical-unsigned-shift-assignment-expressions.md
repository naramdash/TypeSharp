# Task: logical-unsigned-shift-assignment-expressions

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 05:52:15 +09:00
End Time: TBD

## Objective

Implement bounded logical unsigned shift assignment `>>>=` for supported primitive integral mutation surfaces while preserving generated `net48` artifacts and C# 7.3-compatible lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0391-roadmap-refresh-after-logical-unsigned-shift-expressions](tasks-rollup.md#task-0391-roadmap-refresh-after-logical-unsigned-shift-expressions)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)

## Scope

In:
- Parse `>>>=` as an assignment expression without perturbing existing `>>>`, `>>=`, `>>`, `<<`, or function-composition parsing.
- Validate `>>>=` over mutable local primitive integral targets and non-null `byte`, `sbyte`, `short`, `ushort`, or `int` shift counts, reusing the existing logical unsigned shift result policy.
- Lower accepted `>>>=` forms to C# 7.3-compatible explicit assignment using the same unchecked unsigned-cast strategy as expression-level `>>>`; generated C# must not contain `>>>` or `>>>=`.
- Add parser, type-checker, backend, and generated `net48` C# consumer evidence, then update the shared catalog counts and task/docs ledgers.

Out:
- Changing expression-level `>>>`, ordinary `<<`/`>>`, `<<=`/`>>=`, or function composition semantics.
- User-defined operators, enum flag algebra, imported operator overload resolution, or broad assignment target analysis.
- Non-local `>>>=` lowering unless a single-evaluation C# 7.3 lowering policy is implemented in this slice.

## Acceptance Criteria

- [ ] Parser accepts `target >>>= count` as a single assignment operator.
- [ ] Type checker accepts valid primitive integral mutable local cases and reports deterministic `TS2201` diagnostics for nullable, non-integral, enum, record, immutable, unsupported-count, or unsupported target cases.
- [ ] C# backend emits C# 7.3-compatible assignment/cast lowering with no emitted `>>>` or `>>>=`.
- [ ] Parser/type-checker/backend fixtures plus generated `net48` C# consumer smoke cover the new behavior.
- [ ] Feature Status, Work Ledger, task ledger, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test project builds.
Result: TBD

Command: targeted parser/type-checker/backend/CLI smokes through `test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj`
Expected: new fixtures and generated `net48` consumer evidence pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build`
Expected: full shared catalog passes.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet`
Expected: MSTest bridge builds against the updated catalog count.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after roadmap updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0391 rechecked official sources on 2026-05-22, preserved the generated `net48`/C# 7.3 baseline, and selected `>>>=` as the next bounded implementation slice.

Remaining:
- Implement parser/checker/backend/docs/test coverage for logical unsigned shift assignment.

Blocked:
- None.
