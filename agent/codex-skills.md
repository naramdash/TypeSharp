# Codex Skills Configuration

문서 기준일: 2026-05-20

이 파일은 TypeSharp 작업에 필요한 Codex skill 선택과 설치 상태를 기록한다. Skill 본체는 사용자 Codex 환경의 `C:\Users\juho_\.codex\skills`에 설치되어 있고, repo에는 선택 이유와 사용 규칙만 남긴다.

## Installed Skills

| Skill | Installed Path | Use When |
| --- | --- | --- |
| `playwright` | `C:\Users\juho_\.codex\skills\playwright` | Browser automation is needed for docs UI checks, local web verification, screenshots, or `.tysh` syntax highlighting docs work. |
| `gh-fix-ci` | `C:\Users\juho_\.codex\skills\gh-fix-ci` | GitHub Actions checks fail and the task is to inspect logs, summarize the cause, or plan a CI fix. |
| `security-threat-model` | `C:\Users\juho_\.codex\skills\security-threat-model` | The user explicitly asks for threat modeling, abuse paths, or AppSec analysis of this repo or a project path. |

## Operating Rules

- Read this file only when a task explicitly needs a skill.
- Keep this table in sync with `C:\Users\juho_\.codex\skills` when adding or removing a skill.
- Add a skill only for a concrete TypeSharp task; do not install broad inventories or adjacent possibilities.
- Restart Codex before relying on a newly installed skill in the session skill list.
- Do not commit repo-local `.codex/` skill copies; `.gitignore` excludes accidental local copies.
