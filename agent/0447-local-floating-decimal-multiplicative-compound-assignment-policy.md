# Task: local-floating-decimal-multiplicative-compound-assignment-policy

Status: In Progress
Queue: Q1
Start Time: 2026-05-23 03:48:00 +09:00
Source: Task 0446 roadmap refresh after imported C# null-conditional indexer multiplicative compound assignment targets

## Objective

Extend local mutable `*=`, `/=`, and `%=` compound assignment from the current primitive integral-only policy to a bounded floating-point and decimal policy that still preserves generated package-free `net48`/C# 7.3 output and deterministic diagnostics.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [Diagnostics](../docs/src/content/docs/diagnostics.md)
- [Task 0446 rollup](tasks-rollup.md#task-0446-roadmap-refresh-after-imported-csharp-null-conditional-indexer-multiplicative-compound-assignment-targets)

## Scope

In:

- Local `let mut` identifier targets for `*=`, `/=`, and `%=` over supported known non-null `float`, `double`, and `decimal` operands.
- Conservative built-in numeric compatibility for same-family floating/decimal operands and integral right operands that can be assigned under the existing TypeSharp numeric conversion policy.
- Parser/type-checker/backend/generated `net48` C# consumer coverage plus deterministic negative diagnostics for bool/string/enum/nullable/mixed decimal-floating/narrowing/unsupported targets.
- Docs and test catalog count updates.

Out:

- Imported C# member/indexer or null-conditional target expansion for floating-point/decimal multiplicative assignment.
- Checked-overflow policy changes.
- User-defined operators or imported operator overload resolution.
- Increment/decrement or broader assignment target analysis.
- Task 0401 GitHub Actions CI fix without explicit user approval.

## Acceptance Criteria

- [ ] Local mutable `float`, `double`, and `decimal` `*=`, `/=`, and `%=` targets type-check under a bounded policy.
- [ ] Unsupported operands and targets report deterministic diagnostics before backend emission.
- [ ] Accepted targets lower to C# 7.3-compatible generated source and build as package-free `net48` output.
- [ ] Focused positive/negative tests and generated C# consumer evidence are added.
- [ ] Catalog counts, shard expectations, package-shard MTP minimum, docs, tasks, and traceability are updated.

## Verification

Command: TBD
Expected: focused multiplicative filters, full package-free catalog, MSTest shard bridge checks, docs build, stale-reference scan, and `git diff --check`.
Result: TBD

## Handoff

Done:

- Task 0446 rechecked official C#/F#/TypeScript/.NET Framework/.NET/NuGet/.NET testing/MSTest SDK/xUnit.net/NUnit/VS Code/GitHub Actions signals after Task 0445 and kept the generated package-free `net48`/C# 7.3 baseline plus current `net10.0` MSTest.Sdk/MTP package bridge unchanged.

Remaining:

- Implement the bounded local floating-point/decimal multiplicative compound assignment policy.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
