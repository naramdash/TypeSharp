# Task: imported-csharp-null-conditional-bitwise-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 16:28:00 +09:00
Source: Task 0426 roadmap refresh after imported C# null-conditional bitwise compound assignment member targets

## Objective

Implement the next bounded C# 14-inspired null-conditional assignment slice for imported C# indexer bitwise compound targets.

## Context

Task 0425 implemented imported C# null-conditional bitwise compound assignment for metadata-backed instance field/property member targets:

- `receiver?.Member |= value`
- `receiver?.Member &= value`
- `receiver?.Member ^= value`

Task 0426 rechecked official language/platform/package/test/editor/CI signals and confirmed no baseline change. TypeSharp-generated artifacts remain package-free `net48` assemblies with C# 7.3-compatible generated source. The repository already uses the broad current `net10.0` NuGet test-host path through pinned `MSTest.Sdk/4.2.3`, Microsoft Testing Platform, checked-in lock files, source mapping, audit controls, repo-local package cache, and four package shard projects. Adding xUnit.net v3 or NUnit now would duplicate the same extracted-catalog evidence without improving generated `net48` compatibility or the measured release-confidence path.

The remaining natural paired slice is imported C# null-conditional indexer bitwise compound assignment:

- `receiver?[index] |= value`
- `receiver?[index] &= value`
- `receiver?[index] ^= value`

## Scope

In scope:

- Accept metadata-backed imported C# instance indexer targets when overload resolution selects a public getter/setter pair and index arguments are supported.
- Reuse the existing primitive integral, enum, and bool bitwise compound assignment target/value policy.
- Preserve single evaluation of the receiver and each index argument.
- Evaluate index arguments and the right side only when the receiver is non-null.
- Lower through C# 7.3-compatible `System.Func` guard/capture forms and ordinary C# `|=`, `&=`, and `^=` operators.
- Preserve existing null-conditional simple assignment, read, member bitwise compound assignment, and member/indexer `>>>=` behavior.
- Add focused positive generated `net48` C# consumer coverage and deterministic negative checker coverage.
- Update catalog counts, shard expectations, docs, task ledger, and traceability.

Out of scope:

- User-defined operators or imported operator overload resolution.
- Event targets, static targets, local/TypeSharp-owned targets, nullable value-type receivers, invocation, chains, increment/decrement, and broader assignment target analysis.
- Adding generated-project NuGet restore or changing generated artifact target frameworks.
- Implementing Task 0401 without explicit approval for the GitHub Actions `npm` process-launch fix.

## Acceptance Criteria

- Positive CLI build coverage compiles `receiver?[index] |=`, `&=`, and `^=` against a local imported C# DLL and a generated `net48` C# consumer.
- Generated C# stays C# 7.3-compatible, emits no `?[]`, and shows receiver/index/RHS evaluation only inside the non-null branch.
- Negative checker coverage rejects unsupported indexer bitwise targets and operands with deterministic diagnostics.
- Existing null-conditional member bitwise, member/indexer `>>>=`, simple assignment, reads, and shard-count tests continue to pass.
- Docs and agent ledgers record the implemented boundary and remaining backlog.
