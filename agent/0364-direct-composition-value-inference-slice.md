# Task: direct-composition-value-inference-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 22:24:30 +09:00
End Time: TBD

## Objective

Infer concrete delegate types for unannotated non-exported direct named-function composition values when the composed TypeSharp-declared unary signature is fully known, without enabling public ABI inference or imported/higher-order composition.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [tasks-rollup.md](tasks-rollup.md#task-0363-roadmap-refresh-after-composition-annotation-compatibility)

## Scope

In:
- Unannotated non-exported top-level value declarations shaped like `let composed = f >> g` and `let composed = g << f`.
- Direct identifier-only composition pairs where both sides resolve to TypeSharp-declared unary functions.
- Concrete composed signatures after existing direct composition compatibility and bounded generic edge inference have enough information.
- Generated private/internal C# value type inference to `System.Func<TInput, TResult>` or `System.Action<TInput>` when representable by current function-type lowering.
- Positive backend and CLI smoke coverage proving generated C# remains C# 7.3-compatible.
- Docs and agent ledger updates for the completed behavior.

Out:
- Exported unannotated composition values, local export aliases, source re-export metadata, or public ABI inference for composition expressions.
- Function-valued variables, lambda composition targets, imported C# composition targets, non-identifier composition targets, and nested composition type inference beyond existing lowering.
- Higher-order values, currying, partial application, optional/default/params TypeSharp parameter policy, and overload ranking.
- Generic composition cases that require full bidirectional type-constructor unification beyond the bounded direct-call shapes.
- Numeric shifts, shift assignment, user-defined operators, and broader composition type inference.

## Acceptance Criteria

- [ ] Existing annotated composition behavior and diagnostics remain unchanged.
- [ ] Unannotated non-exported direct composition values with fully known signatures lower with concrete delegate types instead of `object`.
- [ ] Cases whose signature is not fully known remain conservative rather than inventing a public or unstable delegate shape.
- [ ] Exported unannotated composition values remain out of scope unless they already have an explicit function-type annotation.
- [ ] Existing C# composition lowering remains C# 7.3-compatible and compiles in the existing `net48` smoke.
- [ ] Grammar, Reference, Type System, Lowering, Feature Status, Work Ledger, Traceability, and task rollup docs reflect the new bounded behavior.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: build succeeds with zero warnings/errors.
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
- Roadmap refresh selected this bounded implementation slice after task 0362.

Remaining:
- Implement backend/builder inference, focused fixture and smoke coverage, and docs updates.

Blocked:
- None.
