# Task: Release Readiness Policy

Status: Done
Queue: Q5
Start Time: 2026-05-19 05:27:18 +09:00
End Time: 2026-05-19 05:29:01 +09:00

## Objective

Define the remaining release readiness policy surface so the checklist distinguishes broad release governance from the already completed runtime ABI and dependency inventory policies.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../runtime-abi.md](../runtime-abi.md)
- [../dependencies.md](../dependencies.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)

## Scope

In:
- release versioning policy
- breaking change policy
- preview feature gate policy
- package checksum/signing policy
- security policy
- release notes template
- compatibility matrix baseline
- README/checklist/traceability links

Out:
- actual package publishing pipeline
- package signing infrastructure
- NuGet restore implementation
- binary compatibility analyzer
- changing runtime ABI constants

## Acceptance Criteria

- [x] `docs/release.md` defines broad release readiness policy without duplicating runtime ABI details.
- [x] `docs/release.md` defines required release notes sections.
- [x] `docs/release.md` defines compatibility matrix baseline and checksum/signing minimum.
- [x] Checklist release readiness items are marked complete based on linked policy evidence.
- [x] Traceability and README link the new release policy.

## Verification

Command:

```text
rg -n "release.md|Release readiness policy|versioning 정책|breaking change 정책|preview feature gate|release notes template|compatibility matrix" docs
git diff --check
```

Expected:
- release policy is linked from README, checklist, traceability, and this task packet.
- documentation changes have no whitespace errors.

Result:
- Pass. `rg -n "release.md|Release readiness policy|versioning 정책|breaking change 정책|preview feature gate|release notes template|compatibility matrix" docs`.
- Pass. `git diff --check`.

## Handoff

Done:
- Added [../release.md](../release.md) as the broad release readiness policy.
- Linked release policy from README, runtime ABI, dependency inventory, checklist, traceability, and the task index.
- Marked release readiness checklist items complete for versioning, breaking change, preview feature gate, package checksum/signing, security, release notes template, and compatibility matrix.

Remaining:
- None.

Blocked:
- None.
