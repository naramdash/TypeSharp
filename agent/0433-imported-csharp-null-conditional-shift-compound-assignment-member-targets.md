# Task: imported-csharp-null-conditional-shift-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 22:39:28 +09:00
Source: Task 0432 roadmap refresh after imported C# null-conditional additive compound assignment indexer targets

## Objective

Implement the bounded imported C# null-conditional shift compound assignment member-target slice: `receiver?.Member <<= count` and `receiver?.Member >>= count` for readable/writable metadata-backed imported C# instance field/property targets, while preserving generated package-free `net48` artifacts and C# 7.3-compatible source output.

## Source Of Truth

- [Core Goal](../docs/src/content/docs/goal.md)
- [Project Requirements](../docs/src/content/docs/requirements.md)
- [Feature Status](../docs/src/content/docs/feature-status.md)
- [Type System](../docs/src/content/docs/type-system.md)
- [Lowering](../docs/src/content/docs/lowering.md)
- [.NET Interop](../docs/src/content/docs/dotnet-interop.md)
- [Project Policy](../docs/src/content/docs/project-policy.md)
- [Work Ledger](../docs/src/content/docs/work-ledger.md)
- [Task 0432 rollup](tasks-rollup.md#task-0432-roadmap-refresh-after-imported-csharp-null-conditional-additive-compound-assignment-indexer-targets)

## Scope

In:

- Accept `receiver?.Member <<= count` and `receiver?.Member >>= count` when `Member` resolves to a readable and writable metadata-backed imported C# public instance field/property.
- Reuse the existing shift assignment operand policy: the target must be a known non-null primitive integral value and the count must be int-compatible.
- Lower accepted targets through explicit C# 7.3-compatible null guards and ordinary `<<=`/`>>=` operators with single receiver evaluation.
- Ensure the count expression is evaluated only when the receiver is non-null.
- Add generated `net48` C# consumer evidence and deterministic negative checker coverage.
- Preserve existing simple assignment, read, additive, bitwise, and `>>>=` null-conditional behavior.
- Keep Task 0401 blocked unless the user explicitly approves implementing the GitHub Actions `npm` process-launch fix.

Out:

- Indexer shift compound assignment `receiver?[index] <<= count` / `receiver?[index] >>= count`.
- Multiplicative compound assignment operators; `*=`, `/=`, and `%=` do not exist in the current lexer/parser assignment surface.
- Event `>>>=`, events under ordinary shift assignment, static targets, local binding targets, TypeSharp-owned member/indexer targets, null-conditional invocation/chains, increment/decrement, and user-defined operators.
- Adding generated-artifact package dependencies, changing generated target frameworks, or replacing the existing `MSTest.Sdk/4.2.3` MTP test-host bridge.

## Acceptance Criteria

- [ ] Checker accepts only the bounded imported C# instance field/property member-target shapes for `<<=` and `>>=`.
- [ ] Checker reports deterministic diagnostics for readonly, event, static, missing/unresolved, invalid operand/count, local, TypeSharp-owned, indexer, and unsupported null-conditional targets.
- [ ] Backend emits C# 7.3-compatible guarded lowering with no emitted C# `?.`.
- [ ] Generated consumer coverage proves receiver single evaluation and skipped count evaluation on null receivers.
- [ ] Existing null-conditional additive, bitwise, logical unsigned shift, simple assignment, and read tests remain covered.
- [ ] Shared catalog, package shard expectations, docs, Work Ledger, tasks, and traceability are updated as needed.

## Verification

Command: TBD
Expected: focused tests for null-conditional member shift compound assignment, related null-conditional regression smokes, full package-free catalog, MSTest package shard smoke if counts change, docs build, and `git diff --check` pass.
Result: TBD

## Handoff

Done:

- Task 0432 rechecked official sources and selected this bounded slice because C# 14 null-conditional assignment supports compound assignment broadly while TypeSharp already has base `<<=`/`>>=` syntax/checking/lowering but still excludes the null-conditional imported member target shape.

Remaining:

- Implement the checker/backend/test/docs updates for imported C# null-conditional member shift compound assignment.

Blocked:

- Task 0401 remains blocked until the user explicitly approves the GitHub Actions CI implementation fix.
