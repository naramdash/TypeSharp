# Task: Feature Specification Index

Status: Done
Queue: Q1-Q5
Start Time: 2026-05-19 05:53:15 +09:00
End Time: 2026-05-19 05:55:55 +09:00

## Objective

Create a feature-level specification index for the currently implemented or stable TypeSharp feature surface so the documentation completeness checklist can point to concrete per-feature spec locations and evidence.

## Source Of Truth

- [../goal.md](../goal.md)
- [../README.md](../README.md)
- [../feature-map.md](../feature-map.md)
- [../feature-review.md](../feature-review.md)
- [../regression-testing.md](../regression-testing.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- current implemented features
- Stable Grammar and MVP feature surfaces with fixture/smoke evidence
- primary spec links for core language, interop/ABI, tooling/build features
- implementation/evidence links
- README/checklist/traceability/task index updates

Out:
- claiming Planned, Preview Watch, Experimental, or Stable Backlog features are implemented
- writing a full standalone spec for every future feature
- compiler/runtime behavior changes

## Acceptance Criteria

- [x] `docs/feature-specs.md` exists and maps current implemented/stable features to primary spec documents.
- [x] The index distinguishes implemented/stable scope from planned/preview/backlog features.
- [x] The index links features to implementation or verification evidence categories.
- [x] README, checklist, traceability, and task index reference the feature spec index.

## Verification

Command:

```text
rg -n "feature-specs.md|Feature Specification Index|각 기능별 세부 사양|Feature specification index" docs
git diff --check
```

Expected:
- feature spec index is linked from README, checklist, traceability, and task index.
- documentation changes have no whitespace errors.

Result:
- Pass. `rg -n "feature-specs.md|Feature Specification Index|각 기능별 세부 사양|Feature specification index" docs`.
- Pass. `git diff --check`.

## Handoff

Done:
- Added [../feature-specs.md](../feature-specs.md) as the feature-level specification index for implemented/stable scope.
- Linked the index from README, checklist, traceability, and the task packet index.
- Marked the feature-level detailed specification checklist item complete for the current implemented/stable scope.

Remaining:
- None.

Blocked:
- None.
