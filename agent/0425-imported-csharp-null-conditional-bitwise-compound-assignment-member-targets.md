# Task: imported-csharp-null-conditional-bitwise-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:44:30 +09:00
Source: Task 0424 roadmap refresh after imported C# null-conditional logical unsigned shift assignment indexer targets

## Objective

Implement a bounded imported C# null-conditional bitwise compound assignment member-target slice.

## Context

Tasks 0398 and 0400 added imported C# null-conditional simple assignment for metadata-backed member and indexer targets. Tasks 0417 and 0419 added null-conditional reads. Tasks 0421 and 0423 added the bounded logical unsigned shift compound slices `receiver?.Member >>>= count` and `receiver?[index] >>>= count`.

C# 14 supports null-conditional assignment and compound assignment, but TypeSharp must keep generated artifacts package-free `net48` and generated C# C# 7.3-compatible. This slice expands the supported compound surface only for imported C# instance member targets that already fit the existing bitwise compound assignment target/value policy.

## Scope

In scope:

- Accept `receiver?.Member |= value`, `receiver?.Member &= value`, and `receiver?.Member ^= value` when `Member` resolves to a readable/writable metadata-backed imported C# instance field or property.
- Reuse the existing bitwise compound assignment target/value policy for primitive integral, enum, and bool shapes already supported by non-null-conditional member assignments.
- Preserve single receiver evaluation.
- Evaluate the right-hand side only when the receiver is non-null.
- Lower accepted forms to C# 7.3-compatible explicit null guards and ordinary compound assignment/operator forms; never emit C# `?.`.
- Add generated `net48` C# consumer coverage for accepted field/property targets.
- Add deterministic checker coverage for unsupported null-conditional bitwise member targets.
- Update catalog counts, package shard expectations, docs, Work Ledger, tasks, and traceability if behavior or counts change.

Out of scope:

- Indexer targets such as `receiver?[index] |= value`.
- Logical unsigned shift assignment `>>>=`, already covered by Tasks 0421 and 0423.
- Shift assignment `<<=`/`>>=` and arithmetic/null-coalescing compound operators.
- Increment/decrement.
- Event, static, local binding, TypeSharp-owned, invocation, chained conditional-access, nullable receiver lifting, user-defined operator, and broader assignment-target policy.
- Task 0401 GitHub Actions `npm` process-launch fix without explicit approval.
- Replacing the current `net10.0` MSTest.Sdk/MTP test-host package bridge.
- Generated-project NuGet restore or moving generated artifacts away from package-free `net48`/C# 7.3-compatible output.

## Acceptance Criteria

- Parser/binder/checker/backend behavior accepts the three bounded imported C# null-conditional bitwise compound member forms.
- Accepted forms preserve single receiver evaluation and skip right-hand-side evaluation when the receiver is null.
- Generated C# stays C# 7.3-compatible and emits no null-conditional C# syntax.
- Unsupported shapes report deterministic diagnostics before backend emission.
- Focused generated `net48` C# consumer and negative checker coverage pass.
- Shared catalog count and MSTest package shard expectations are updated if tests are added.
- Documentation and operational ledgers reflect the implemented slice and remaining backlog.
