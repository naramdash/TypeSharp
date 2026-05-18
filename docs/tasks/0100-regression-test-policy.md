# Task: Regression Test Policy

Status: Done
Queue: Q1-Q3
Start Time: 2026-05-19 05:02:29 +09:00
End Time: 2026-05-19 05:03:44 +09:00

## Objective

Define the regression test policy that tells future TypeSharp changes which fixture, smoke, or metadata check must be added before a checklist item can be closed.

## Source Of Truth

- [../goal.md](../goal.md)
- [../agentic-execution.md](../agentic-execution.md)
- [../parser-fixtures.md](../parser-fixtures.md)
- [../diagnostics.md](../diagnostics.md)
- [../lowering.md](../lowering.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- regression test taxonomy for parser, binder, type checker, backend, CLI, runtime, interop, public ABI, performance, and docs
- rules for positive and negative coverage
- checklist and traceability updates

Out:
- new test harness implementation
- snapshot update command implementation
- release CI matrix design

## Acceptance Criteria

- [x] Policy defines where new regressions belong.
- [x] Policy distinguishes golden fixtures, smoke tests, generated `net48` build tests, public ABI metadata checks, and docs-only verification.
- [x] Policy gives closure rules for checklist items.
- [x] Docs index, checklist, traceability, and task index point to the policy.

## Verification

Command:

```text
rg -n "regression-testing|Regression test policy|regression test policy" docs
git diff --check
```

Expected:
- docs link to the new policy.
- whitespace check passes.

Result:
- Pass. `rg -n "regression-testing|Regression test policy|regression test policy" docs`
- Pass. `git diff --check`

## Handoff

Done:
- Added [../regression-testing.md](../regression-testing.md).
- Linked the policy from docs README, checklist, traceability, and task index.

Remaining:
- None.

Blocked:
- None.
