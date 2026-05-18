# Task: Progress And ADR Policy

Status: Done
Queue: Q1
Start Time: 2026-05-19 04:43:43 +09:00
End Time: 2026-05-19 04:45:44 +09:00

## Objective

Define how long-running TypeSharp work records progress and architecture decisions so future `/goal` iterations do not rely on chat history alone.

## Source Of Truth

- [../goal.md](../goal.md)
- [../agentic-execution.md](../agentic-execution.md)
- [README.md](README.md)
- [../checklist.md](../checklist.md)

## Scope

In:
- progress log policy
- ADR directory and format
- links from documentation index and agentic execution contract
- checklist and traceability updates

Out:
- writing retroactive ADRs for every historical decision
- changing existing task rollups
- release governance policy

## Acceptance Criteria

- [x] A progress policy defines when to use task packets, rollups, and commits.
- [x] The policy preserves current-computer start/end timestamps for new task packets.
- [x] ADR format defines decision status, context, options, decision, consequences, validation, and supersession.
- [x] Documentation index, checklist, and traceability link the new policies.

## Verification

Command:

```text
git diff --check
```

Expected:
- documentation-only policy update has no whitespace errors.

Result:
- Pass. `git diff --check`

## Handoff

Done:
- Added [../progress.md](../progress.md) for task packet, rollup, commit, and handoff logging policy.
- Added [../adr/README.md](../adr/README.md) for architecture decision record criteria and template.
- Linked progress and ADR policy from [../README.md](../README.md), [../agentic-execution.md](../agentic-execution.md), [README.md](README.md), and [../traceability.md](../traceability.md).
- Marked the progress log and ADR checklist items complete.

Remaining:
- No retroactive ADRs were written for historical decisions.

Blocked:
- None.
