# Progress Log Policy

문서 기준일: 2026-05-20

이 파일은 task 기록을 어디에 남길지만 정한다. 대화 요약은 최종 근거가 아니며, 장기 작업 상태는 파일에 남겨야 한다.

## Files

| File | Use |
| --- | --- |
| [tasks.md](tasks.md) | State, User Task Inbox, Agent Task Queue, completed range, rollup link |
| `docs/NNNN-short-name.md` | Active task packet for work that outlives one turn |
| [tasks-rollup.md](tasks-rollup.md) | Compressed completed task history |
| [checklist.md](checklist.md) | Current unfinished operational work |
| [traceability.md](traceability.md) | Links from active work to goals and evidence |
| [adr.md](adr.md) | Large accepted design decisions |

## Active Task Rules

- Create an active packet only when work needs handoff or spans more than one turn.
- Reuse the existing active packet for the same topic.
- Include status, queue, start/end time, objective, source of truth, scope, acceptance criteria, verification, and handoff.
- Verification must name the command and what it proved.
- If verification fails before passing, record the final result and the reason the earlier failure is no longer blocking.

## Rollup Rules

- When an active task is Done, summarize it in [tasks-rollup.md](tasks-rollup.md).
- Preserve purpose, changed surface, important verification, remaining work, and out-of-scope items.
- Remove the completed active packet file after rollup.
- Update [tasks.md](tasks.md) so `State`, `User Task Inbox`, and `Agent Task Queue` match the next state.
- Update docs-site Work Ledger when active task state or completed range changes.

## Commit And Handoff

- Commit task-related code/docs/tests together or in clearly linked commits.
- A Done task should be committed and pushed by the long-running agent when credentials/network allow it.
- Run relevant tests and `git diff --check` before commit.
- If push fails, record the local commit hash and failure reason in the task handoff or final summary.
- Final summaries stay short; detailed state belongs in the active packet or rollup.
