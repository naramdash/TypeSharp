# Task: optional-default-parameter-declarations

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 01:41:24 +09:00
End Time: TBD

## Objective

Implement a bounded TypeSharp-owned optional/default parameter declaration slice for direct TypeSharp functions, preserving deterministic diagnostics, C# 7.3-compatible lowering, and the generated `net48` artifact baseline.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [tasks-rollup.md](tasks-rollup.md#task-0375-roadmap-refresh-after-ci-regression-gate)
- `lang/TypeSharp.Compiler`
- `test/fixtures`
- `test/TypeSharp.Compiler.Tests`

## Scope

In:
- Define the initial syntax and semantics for TypeSharp-owned defaulted parameters on direct function declarations.
- Keep the first implementation slice conservative: direct TypeSharp-declared functions, deterministic arity behavior, and C# 7.3-compatible lowering.
- Reject or defer ambiguous/default-after-required/interaction-with-`params` shapes with clear diagnostics unless the implementation can prove a safe rule.
- Add parser, binder/type-checker, backend, CLI/build, and docs coverage that proves both accepted and rejected cases.
- Document public ABI behavior for C# consumers and generated `net48` artifacts.

Out:
- Imported C# optional/default parameter behavior changes.
- TypeSharp function overload ranking.
- Named argument binding for TypeSharp-declared functions unless needed for the default-parameter slice.
- Optional/default parameters on constructors, delegates, union cases, function types, lambdas, or higher-order values.
- General default expressions that cannot be lowered predictably to C# 7.3-compatible generated code.

## Acceptance Criteria

- [ ] Grammar/reference docs describe the accepted optional/default parameter syntax and explicit exclusions.
- [ ] Parser fixtures cover accepted declarations and rejected syntax/recovery cases.
- [ ] Type-checker or binder coverage validates omitted direct-call arguments and rejected unsafe shapes.
- [ ] Backend snapshots and generated `net48` build smoke prove C# 7.3-compatible lowering.
- [ ] Public ABI or C# consumer behavior is documented and tested or explicitly rejected.
- [ ] Feature Status, Work Ledger, task rollup, and traceability are updated.
- [ ] Verification includes targeted tests, relevant build/smoke commands, docs build, and `git diff --check`.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: compiler test runner builds.
Result: TBD

Command: targeted `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- --filter "<optional/default parameter filter>"`
Expected: targeted optional/default parameter coverage passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds if docs are changed.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0375 selected this slice after confirming the CI regression gate does not change TypeSharp's generated-artifact baseline.

Remaining:
- Design, implement, document, and verify optional/default parameter declarations.

Blocked:
- None.
