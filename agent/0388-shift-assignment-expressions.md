# Task: shift-assignment-expressions

Status: In Progress
Queue: Q2
Start Time: 2026-05-22 04:39:50 +09:00
End Time: TBD

## Objective

Implement bounded `<<=` and `>>=` shift assignment expressions over the existing TypeSharp assignment surface while preserving generated `net48` C# 7.3-compatible lowering.

## Source Of Truth

- [agent.md](../agent.md)
- [agentic-execution.md](agentic-execution.md)
- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0387-roadmap-refresh-after-integral-shift-expressions](tasks-rollup.md#task-0387-roadmap-refresh-after-integral-shift-expressions)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [C# bitwise and shift operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/bitwise-and-shift-operators)

## Scope

In:
- Parse `<<=` and `>>=` as assignment operators without changing existing `<<`/`>>` expression parsing for shifts or composition.
- Accept shift assignments on the already supported assignment surface when the target is assignable and the operands satisfy the primitive integral shift policy from task 0386.
- Preserve `let mut` local assignment target diagnostics and existing imported C# assignment target validation behavior.
- Lower accepted shift assignments to ordinary C# `<<=` and `>>=` in generated C# 7.3-compatible source.
- Add parser, type-checker positive/negative, backend snapshot, generated `net48` smoke, docs, traceability, and catalog/MSTest evidence.

Out:
- Logical unsigned `>>>=`.
- User-defined shift assignment operators or imported C# operator overload resolution.
- Enum flag algebra beyond the existing same-enum `|`/`&`/`^`/`~` surface.
- Broad assignment target analysis beyond the existing assignment surface.
- Changes to function composition, pipeline inference, or shift expression precedence.

## Acceptance Criteria

- [ ] `<<=` and `>>=` parse deterministically and do not perturb existing shift/composition parsing.
- [ ] Mutable local primitive integral shift assignments type-check with the same count policy as shift expressions.
- [ ] Invalid targets, nullable operands, non-integral operands, unsupported counts, and immutable locals report stable diagnostics before backend emission.
- [ ] Accepted shift assignments lower to C# 7.3-compatible source and compile into generated `net48` evidence.
- [ ] Canonical docs, feature status, work ledger, task ledger, and traceability are updated.

## Verification

Command: `dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: test harness builds.
Result: TBD

Command: targeted `dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build ...`
Expected: relevant parser/type-checker/backend/generated-smoke/catalog scenarios pass.
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
- Task 0387 rechecked official language, platform, package, testing, editor, and CI signals after integral shifts and found no generated-artifact baseline change.
- Task 0387 selected `<<=`/`>>=` shift assignment as the next bounded implementation slice.

Remaining:
- Implement and verify bounded shift assignment expressions.

Blocked:
- None.
