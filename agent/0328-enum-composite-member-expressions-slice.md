# Task: enum-composite-member-expressions-slice

Status: In Progress
Queue: Q2
Start Time: 2026-05-21 15:34:00 +09:00
End Time: TBD

## Objective

Add bounded TypeSharp-owned enum member composite initializers such as `ReadWrite = Read | Write` while preserving the current C# 7.3 `net48` lowering baseline.

## Source Of Truth

- [tasks.md](tasks.md)
- [tasks-rollup.md#task-0327-roadmap-refresh-after-enum-attribute-lowering](tasks-rollup.md#task-0327-roadmap-refresh-after-enum-attribute-lowering)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Grammar](../docs/src/content/docs/grammar.md)
- [Lowering](../docs/src/content/docs/lowering.md)

## Scope

In:
- Parse enum initializer-local `|` composites over integer literals, optional signed integer literals, and identifier operands.
- Validate identifier operands reference previously declared members of the same enum.
- Preserve integer literal range validation for numeric operands under the enum underlying type.
- Lower accepted composites to C# enum member assignments.
- Add parser, type-checker, backend, generated `net48`, and docs coverage.

Out:
- General expression-level bitwise operators outside enum member initializers.
- `&`, `^`, `~`, shifts, arithmetic, parentheses, or arbitrary computed enum expressions.
- Flag-aware match exhaustiveness, numeric pattern algebra, imported enum flag reasoning, and broad attribute target validation.

## Acceptance Criteria

- [ ] `ReadWrite = Read | Write` parses and lowers for TypeSharp-owned enums.
- [ ] Composite initializer identifiers must target earlier same-enum members.
- [ ] Numeric operands keep existing enum underlying range diagnostics.
- [ ] Non-enum expression parsing remains unchanged for `|` and `&`.
- [ ] Canonical docs and operational ledgers describe the bounded surface.

## Verification

Command: `dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --nologo --verbosity quiet`
Expected: Succeeds.
Result: TBD

Command: `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build "parser fixture snapshots match"`
Expected: Succeeds.
Result: TBD

Command: `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build "type checker fixture diagnostics match"`
Expected: Succeeds.
Result: TBD

Command: `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj --no-build "C# backend fixture snapshots match"`
Expected: Succeeds.
Result: TBD

Command: `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
Expected: Succeeds.
Result: TBD

Command: `npm run build` from `docs`
Expected: Succeeds.
Result: TBD

Command: `git diff --check`
Expected: Succeeds.
Result: TBD

## Handoff

Done:
- Roadmap selected this implementation slice after enum attribute lowering.

Remaining:
- Implement parser/type-checker/backend support, fixtures, docs, and verification.

Blocked:
- None.
