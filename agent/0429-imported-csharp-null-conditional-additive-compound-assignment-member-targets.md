# Task: imported-csharp-null-conditional-additive-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 17:20:00 +09:00
Source: Task 0428 roadmap refresh after imported C# null-conditional bitwise compound assignment indexer targets

## Objective

Implement the next bounded C# 14 null-conditional compound-assignment slice for imported C# member targets.

## Context

Tasks 0398 through 0427 established the imported C# null-conditional member/indexer path for reads, simple assignment, logical unsigned shift assignment, and bitwise compound assignment while preserving generated package-free `net48` and C# 7.3-compatible lowering.

C# 14 documents `?.` and `?[]` as valid left-hand sides for assignment and compound assignment, while increment/decrement remain disallowed. TypeSharp currently rejects null-conditional `+=` and `-=` so it does not emit invalid generated assignment targets through expression-form null-conditional reads.

Task 0428 rechecked official signals and kept the current test-host package answer: TypeSharp already uses the broad current `net10.0` NuGet path through pinned `MSTest.Sdk/4.2.3`, Microsoft Testing Platform, package lock files, source mapping, audit controls, repo-local package cache, and four package-based shard projects. Task 0428 also switched the package shard CI command to native MTP module-level parallelism.

## Scope

In scope:

- Support `receiver?.Member += value` and `receiver?.Member -= value` for readable/writable metadata-backed imported C# instance field/property targets with supported primitive numeric operands.
- Add deterministic checker diagnostics for unsupported null-conditional additive compound member targets, including readonly fields, events, static targets, indexers, TypeSharp-owned targets, nullable/unknown unsupported operands, and non-numeric values.
- Lower accepted targets through C# 7.3-compatible `System.Func<TReceiver,TValue>` null guards with ordinary C# `+=` or `-=` in the non-null branch.
- Preserve single receiver evaluation and evaluate the right side only when the receiver is non-null.
- Add generated `net48` C# consumer coverage plus focused negative checker coverage.
- Update the shared catalog count, MSTest bridge count, shard expectations, docs, ledgers, and traceability.

Out of scope:

- Null-conditional additive compound indexer targets; reserve them for the paired follow-up slice.
- `*=`, `/=`, and `%=` parsing/lowering; the current lexer/parser only model `+=` and `-=`.
- String concatenation, decimal/floating policy expansion beyond the selected primitive numeric set unless the implementation can reuse existing TypeSharp type rules without broadening diagnostics.
- User-defined operators, event add/remove, increment/decrement, invocation, chains, static targets, TypeSharp-owned assignment targets, TypeSharp member assignment policy, and broad class-member body analysis.
- Changing generated artifact target frameworks or adding NuGet dependencies to generated `net48` artifacts.

## Acceptance Criteria

- Positive coverage proves accepted `receiver?.Member += value` and `receiver?.Member -= value` behavior over metadata-backed imported C# instance field/property targets.
- Negative coverage proves unsupported null-conditional additive compound member targets report deterministic diagnostics before backend emission.
- Generated C# remains C# 7.3-compatible, emits no `?.`, and generated artifacts remain package-free `net48`.
- Right-side evaluation is skipped when the receiver is null, and non-trivial receivers are evaluated once.
- Package-free shard and MSTest package shard expectations are updated consistently.
- Required docs and agent ledgers are updated.
- Verification commands pass before completion.
