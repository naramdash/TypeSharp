# Task: test-suite-runtime-reduction

Status: In Progress
Queue: Q4
Start Time: 2026-05-21 23:48:04 +09:00
End Time: TBD

## Objective

Plan and refactor `test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj` runtime so routine verification gets faster without weakening the regression evidence that TypeSharp needs.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- `test/TypeSharp.Compiler.Tests/Program.cs`
- `test/fixtures`
- `vscode/typesharp/test`
- `examples/runnable`

## Scope

In:
- Measure the current `TypeSharp.Compiler.Tests` runtime shape enough to identify the slowest categories and repeated setup work.
- Decide which runtime-reduction plan fits TypeSharp's purpose: fast local confidence, deterministic diagnostics, generated `net48` proof, C# interop proof, VS Code/LSP proof, docs proof, and full release confidence.
- Refactor test runner configuration, filtering, fixture grouping, or shared setup only where the same evidence remains covered by an appropriate fast or full gate.
- Keep a full-suite path for release/regression confidence and make the faster path explicit in docs or task ledgers if behavior changes.

Out:
- Removing assertions, fixture scenarios, generated `net48` compile/consumer proof, or interop coverage just to reduce time.
- Changing TypeSharp language/compiler behavior except for test harness code required by the refactor.
- Adding new global tooling or runtime requirements beyond the existing `dotnet` and `node` baseline.
- Rewriting unrelated test areas that do not materially affect runtime or maintainability.

## Acceptance Criteria

- [ ] Current test runtime profile and bottleneck categories are recorded.
- [ ] The selected plan explains why the faster configuration still serves TypeSharp's project goals and regression policy.
- [ ] Test configuration or runner changes are implemented with focused coverage for the changed behavior.
- [ ] Full confidence verification remains available and documented.
- [ ] `agent/tasks.md`, `agent/tasks-rollup.md`, traceability, and docs Work Ledger are updated when the task completes.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: test project builds.
Result: TBD

Command: targeted and full `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj` variants selected after the runtime plan.
Expected: changed runner/configuration behavior passes and full confidence remains available.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0367 completed the official-source roadmap refresh and promoted the pending user-owned test-suite runtime request.

Remaining:
- Profile, plan, implement, and verify test-suite runtime reduction.

Blocked:
- None.
