# Task: roadmap-refresh-after-imported-csharp-null-conditional-bitwise-compound-assignment-indexer-targets

Status: In Progress
Queue: Q1
Start Time: 2026-05-22 17:30:00 +09:00
Source: Task 0427 imported C# null-conditional bitwise compound assignment indexer targets

## Objective

Recheck the TypeSharp roadmap and external signals after imported C# null-conditional bitwise compound assignment indexer targets landed.

## Context

Task 0427 implemented the paired indexer bitwise compound assignment slice:

- `receiver?[index] |= value`
- `receiver?[index] &= value`
- `receiver?[index] ^= value`

The implementation accepts readable/writable metadata-backed imported C# instance indexers with supported arguments, reuses the existing primitive integral, enum, and bool bitwise compound target/value policy, preserves single receiver/index evaluation, evaluates index arguments and the right side only when the receiver is non-null, and lowers through C# 7.3-compatible `System.Func` guard/capture forms plus ordinary C# compound assignment operators.

The shared test catalog now has 548 cases with four package shard expectations of 137 each. Generated user artifacts remain package-free `net48` assemblies with C# 7.3-compatible source, while test-host NuGet usage remains isolated in the pinned `net10.0` `MSTest.Sdk/4.2.3`/Microsoft Testing Platform bridge and package shard projects.

## Scope

In scope:

- Recheck official C#, F#, TypeScript, .NET Framework, .NET, NuGet, .NET testing, MSTest SDK, xUnit.net, NUnit, VS Code, and GitHub Actions signals.
- Confirm whether generated `net48`/C# 7.3 and test-host `net10.0` MSTest.Sdk/MTP package-shard baselines still hold.
- Re-evaluate whether another test NuGet package adds distinct evidence beyond the existing MSTest package bridge and shards.
- Keep Task 0401 blocked unless the user explicitly approves the GitHub Actions `npm` process-launch fix.
- Select the next bounded implementation slice and update docs, ledgers, and traceability.

Out of scope:

- Implementing Task 0401 without explicit approval.
- Adding generated-project NuGet restore, changing generated artifact target frameworks, or switching test frameworks without source-backed need.
- Starting a broad assignment-target, user-defined operator, invocation/chaining, event, or TypeSharp-owned null-conditional assignment implementation without a bounded next slice.

## Acceptance Criteria

- Official source review is summarized with links in the rollup or canonical docs.
- Docs and agent ledgers record whether baseline/package decisions changed.
- A concrete next task packet is created if a bounded implementation slice is selected.
- `npm run build` from `docs` and `git diff --check` pass before completion.
