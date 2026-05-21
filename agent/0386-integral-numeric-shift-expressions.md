# Task: integral-numeric-shift-expressions

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 04:07:42 +09:00
End Time: TBD

## Objective

Accept known non-null primitive integral `<<` and `>>` expressions while preserving function composition behavior and generated `net48` C# 7.3-compatible lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0385-roadmap-refresh-after-generic-optional-default-parameters](tasks-rollup.md#task-0385-roadmap-refresh-after-generic-optional-default-parameters)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Grammar And Language Reference](../docs/src/content/docs/reference.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)

## Scope

In:
- Treat `left << right` and `left >> right` as numeric shifts only when both operands type-check as supported known non-null primitive integral values and the right operand is a supported integral shift count.
- Preserve existing `>>` and `<<` composition behavior for function-shaped operands.
- Keep ambiguous, unknown, nullable, bool, string, enum, structural, and non-integral value-shaped operands on deterministic diagnostics before C# emission.
- Lower accepted shifts to ordinary C# `left << right` and `left >> right` expressions in generated C# 7.3 source.
- Add positive/negative type-checker fixtures, backend snapshot coverage, generated `net48` smoke evidence, docs, traceability, and catalog/MSTest evidence.

Out:
- `<<=` and `>>=` shift assignment.
- Unsigned/logical right shift syntax or C# 11-style `>>>`.
- User-defined operators and imported C# operator overload resolution.
- Enum flag algebra or flag-style match reasoning.
- Parser token reshaping that breaks existing function composition.
- Higher-order/imported composition inference, currying, partial application, or pipeline overload ranking.

## Acceptance Criteria

- [ ] Known non-null primitive integral `<<`/`>>` expressions type-check and infer the expected promoted integral result.
- [ ] Existing function composition fixtures still pass for function-shaped `>>`/`<<` operands.
- [ ] Invalid shift operands report stable diagnostics before backend emission.
- [ ] Accepted shifts lower to C# 7.3-compatible source and compile into generated `net48` evidence.
- [ ] Canonical docs, feature status, work ledger, task ledger, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: test harness builds.
Result: TBD

Command: targeted `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build ...`
Expected: relevant type-checker/backend/generated-smoke/catalog scenarios pass.
Result: TBD

Command: `dotnet build test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --nologo --verbosity quiet`
Expected: MSTest bridge builds.
Result: TBD

Command: `dotnet test --project test\TypeSharp.Compiler.Tests.MSTest\TypeSharp.Compiler.Tests.MSTest.csproj --no-build --filter "FullyQualifiedName~CatalogIsExposedForPackageRunners" --verbosity quiet`
Expected: package-based discovery smoke passes.
Result: TBD

Command: `npm run build` from `docs`
Expected: docs build succeeds after docs updates.
Result: TBD

Command: `git diff --check`
Expected: no whitespace errors.
Result: TBD

## Handoff

Done:
- Task 0385 confirmed the existing `net10.0` MSTest.Sdk/MTP NuGet bridge already covers package-based parallel tests without changing generated `net48` artifacts.
- Task 0385 selected primitive integral shifts as the next bounded language slice.

Remaining:
- Implement and verify the bounded numeric shift behavior.

Blocked:
- None.
