# Task: roadmap-refresh-after-imported-csharp-null-conditional-bitwise-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 15:59:18 +09:00
Source: Task 0425 imported C# null-conditional bitwise compound assignment member targets

## Objective

Refresh the roadmap after the imported C# null-conditional bitwise compound assignment member-target slice, then select the next bounded implementation task.

## Context

Task 0425 implemented `receiver?.Member |= value`, `receiver?.Member &= value`, and `receiver?.Member ^= value` for readable/writable metadata-backed imported C# instance fields/properties. It reused the existing primitive integral, enum, and bool bitwise compound target/value policy, preserved single receiver evaluation, skipped right-side evaluation on null receivers, lowered through C# 7.3-compatible `System.Func` null guards plus ordinary compound assignment operators, and added generated `net48` consumer plus negative checker coverage.

The project must continue to preserve generated package-free `net48` artifacts and C# 7.3 source output while using the existing `net10.0` `MSTest.Sdk/4.2.3` Microsoft Testing Platform bridge and package shards for test-host NuGet coverage.

## Scope

In scope:

- Recheck current official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals.
- Confirm whether the existing `net10.0` MSTest.Sdk/MTP shard setup remains the most general useful test NuGet path for TypeSharp, or whether a distinct package-host change is now justified.
- Keep generated user assemblies targeting `net48` with C# 7.3-compatible generated source unless the canonical docs explicitly change.
- Keep Task 0401 blocked unless explicit approval arrives for the GitHub Actions `npm` process-launch fix.
- Update docs, task ledger, and traceability with the refresh result.
- Select and packet the next bounded implementation slice.

Out of scope:

- Implementing Task 0401 without explicit approval.
- Replacing the current test-host package bridge unless the refresh creates a separate approved implementation task.
- Adding generated-project NuGet restore.
- Moving generated artifacts to `net10.0`, `net481`, or newer C# syntax.
- Broad feature implementation beyond selecting the next slice.

## Acceptance Criteria

- Official source review is summarized in canonical docs.
- Existing generated artifact and test-host package boundaries are either reaffirmed or explicitly changed with rationale.
- Work ledger, feature status, task queue, and traceability are updated.
- A next active implementation packet is created or the queue explicitly records why none is ready.
- Verification commands are recorded in the rollup.
