# Task: composition-function-type-annotation-compatibility-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 22:05:18 +09:00
End Time: TBD

## Objective

Validate explicit function-type annotations on direct named-function composition values before C# emission, without enabling unannotated composition type inference or changing C# 7.3-compatible lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0361-roadmap-refresh-after-direct-generic-composition-inference)

## Scope

In:
- Explicitly annotated value declarations shaped like `let composed: A -> B = f >> g` and `let composed: A -> B = g << f`.
- Direct identifier-only `f >> g` and `g << f` pairs where both sides resolve to TypeSharp-declared unary functions.
- Reuse existing direct generic composition edge inference for simple `T`, arrays such as `T[]`, and matching single-argument generic wrappers such as `List<T>`.
- `TS2201` diagnostics when the annotation input cannot feed the first composed function parameter or the final composed return cannot flow to the annotation return.
- Positive/negative type-checker fixtures plus backend/CLI smoke coverage showing generated C# lowering remains ordinary delegate-lambda assignment.
- Docs and agent ledger updates for the completed behavior.

Out:
- Unannotated composition expression function-type inference or public ABI inference.
- Function-valued variables, lambda composition targets, imported C# composition targets, and non-identifier composition targets.
- Higher-order values, currying, partial application, optional/default/params TypeSharp parameter policy, and overload ranking.
- Generic composition cases that require full bidirectional type-constructor unification beyond the bounded direct-call shapes.
- Numeric shifts, shift assignment, user-defined operators, and broader composition type inference.

## Acceptance Criteria

- [ ] Existing direct composition compatibility diagnostics remain unchanged.
- [ ] Explicit function-type annotations on direct composition values validate the composed input and output edge.
- [ ] Incompatible annotation/composition pairs report deterministic `TS2201` diagnostics before generated C# emission.
- [ ] Compatible generic direct composition annotations use the existing bounded substitution behavior.
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
- Roadmap refresh selected this bounded implementation slice after task 0360.

Remaining:
- Implement checker validation, fixtures, backend/CLI smoke reinforcement if needed, and docs updates.

Blocked:
- None.
