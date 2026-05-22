# Task: roadmap-refresh-after-imported-csharp-null-conditional-additive-compound-assignment-member-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 17:45:00 +09:00
Source: Task 0429 imported C# null-conditional additive compound assignment member targets

## Objective

Recheck the official language, platform, package, test-host, editor, and CI signals after completing imported C# null-conditional additive compound assignment member targets, then select the next bounded implementation slice.

## Context

Task 0429 added `receiver?.Member += value` and `receiver?.Member -= value` for readable/writable metadata-backed imported C# instance field/property targets with supported primitive integral operands. The implementation preserves single receiver evaluation, skips right-side evaluation when the receiver is null, emits C# 7.3-compatible `System.Func<TReceiver,TValue>` guards with ordinary `+=`/`-=`, keeps generated artifacts package-free `net48`, and updates the shared catalog to 550 cases with package-shard MTP minimum 554.

Task 0428 already answered the NuGet package question: TypeSharp uses the broad current `net10.0` package test-host path through pinned `MSTest.Sdk/4.2.3`, Microsoft Testing Platform, package lock files, source mapping, audit controls, repo-local package cache, four package-based shard projects, and MTP `--test-modules` module parallelism. Generated `net48` artifacts intentionally remain package-free.

## Scope

In scope:

- Recheck current official sources for C#, F#, TypeScript, .NET Framework/.NET, NuGet/package status, .NET testing/MTP/MSTest SDK, xUnit.net/NUnit comparison points, VS Code, and GitHub Actions runner/setup actions.
- Confirm whether any source changes TypeSharp's generated `net48`/C# 7.3 artifact boundary or the existing `net10.0` MSTest.Sdk/MTP package bridge.
- Confirm Task 0401 remains blocked unless the user explicitly approves the `gh-fix-ci` implementation path.
- Pick one next bounded implementation task with clear in-scope/out-of-scope boundaries, acceptance criteria, expected tests, docs, and ledger updates.

Out of scope:

- Implementing a language feature in this refresh task.
- Changing generated artifact target frameworks or adding NuGet dependencies to generated `net48` artifacts.
- Replacing the existing MSTest.Sdk/MTP bridge with another package runner unless the source review shows new evidence that changes the release-confidence tradeoff.
- Implementing Task 0401 without explicit approval.

## Acceptance Criteria

- Official source review is summarized with dates and links where relevant.
- The generated artifact and test-host package policy is either reaffirmed or updated with a concrete rationale.
- The next implementation slice is selected and gets its own task packet.
- `agent/tasks.md`, `agent/tasks-rollup.md`, `agent/traceability.md`, Feature Status, Work Ledger, Project Policy if needed, and test docs if needed are updated.
- Verification commands appropriate to changed files pass before completion.
