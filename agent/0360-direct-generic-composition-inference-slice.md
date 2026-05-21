# Task: direct-generic-composition-inference-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 21:41:50 +09:00
End Time: TBD

## Objective

Extend direct named-function composition checks to bounded TypeSharp-declared generic unary function targets without changing C# 7.3-compatible composition lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0359-roadmap-refresh-after-direct-generic-pipeline-inference)

## Scope

In:
- Direct identifier-only `f >> g` and `g << f` pairs where both sides resolve to TypeSharp-declared unary functions.
- Generic target signatures whose type parameters can be inferred from the first function return flowing into the second function parameter.
- Bounded inference/substitution for simple `T`, arrays such as `T[]`, and matching single-argument generic wrappers such as `List<T>`.
- `TS2201` diagnostics for incompatible substituted composition edges that currently slip through because either side is generic.
- Positive/negative type-checker fixtures and composition backend/CLI smoke coverage that demonstrates generated C# lowering remains ordinary delegate-lambda calls.
- Docs and agent ledger updates for the completed behavior.

Out:
- Imported C# composition targets.
- Higher-order function values beyond direct named functions.
- Currying, partial application, optional/default/params TypeSharp parameter policy, and overload ranking.
- Composition expression function-type inference or public ABI inference.
- Generic composition cases that require full bidirectional type-constructor unification beyond the bounded direct-call shapes.
- Numeric shifts, shift assignment, user-defined operators, and broader composition type inference.

## Acceptance Criteria

- [ ] Non-generic direct named-function composition diagnostics remain unchanged.
- [ ] Generic direct named unary function pairs infer bounded substitutions from the composition edge before compatibility checking.
- [ ] Incompatible generic composition pairs report deterministic `TS2201` diagnostics.
- [ ] Existing C# composition lowering remains C# 7.3-compatible and compiles in the existing `net48` smoke.
- [ ] Grammar, Reference, Type System, Lowering, Diagnostics, Feature Status, Work Ledger, Traceability, and task rollup docs reflect the new bounded behavior.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: build succeeds with zero warnings/errors.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"`
Expected: type-checker fixture snapshots pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"`
Expected: backend fixture snapshots pass.
Result: TBD

Command: `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build "CLI build compiles composition lowering"`
Expected: composition CLI smoke passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Roadmap refresh selected this bounded implementation slice after task 0358.

Remaining:
- Implement the checker, fixtures, backend/CLI smoke reinforcement, and docs updates.

Blocked:
- None.
