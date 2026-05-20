# Codex Skills Configuration

문서 기준일: 2026-05-20

이 파일은 TypeSharp 장기 goal 작업에서 사용할 Codex skill 선택과 설치 상태를 기록한다. Skill 자체는 사용자 Codex 환경의 `C:\Users\juho_\.codex\skills`에 설치되고, 이 파일은 repo 안에서 선택 이유와 사용 규칙을 남긴다.

## Installed For This Project

| Skill | Source | Installed Path | Use For TypeSharp |
| --- | --- | --- | --- |
| `playwright` | `openai/skills` curated | `C:\Users\juho_\.codex\skills\playwright` | Real-browser CLI automation for docs UI checks, local web verification, screenshots, and the upcoming `.tysh` syntax highlighting docs task. |
| `gh-fix-ci` | `openai/skills` curated | `C:\Users\juho_\.codex\skills\gh-fix-ci` | GitHub Actions check investigation and repair planning when CI or release workflows fail. Requires authenticated `gh`. |
| `security-threat-model` | `openai/skills` curated | `C:\Users\juho_\.codex\skills\security-threat-model` | Explicit AppSec threat-model requests for compiler, CLI, VS Code extension, generated artifacts, or release surfaces. |

Codex must be restarted before newly installed skills are available in the session skill list.

## Already Available System Skills

These are already present in the system skill set and do not need project-local installation:

- `openai-docs`: use for current OpenAI product/API documentation tasks.
- `skill-installer`: use to list or install additional Codex skills.
- `skill-creator`: use only when creating a new custom Codex skill.
- `browser`: use the in-app browser for local web targets and frontend verification when the Browser plugin is the better fit.
- `documents`, `presentations`, `spreadsheets`, `imagegen`: use only for matching artifact tasks.

## Curated Inventory Checked

The `openai/skills` curated inventory was checked on 2026-05-20 through the GitHub contents API. Available curated skill names at that time were:

`aspnet-core`, `chatgpt-apps`, `cli-creator`, `cloudflare-deploy`, `figma-code-connect-components`, `figma-create-design-system-rules`, `figma-create-new-file`, `figma-generate-design`, `figma-generate-library`, `figma-implement-design`, `figma-use`, `figma`, `gh-address-comments`, `gh-fix-ci`, `hatch-pet`, `jupyter-notebook`, `linear`, `migrate-to-codex`, `netlify-deploy`, `notion-knowledge-capture`, `notion-meeting-intelligence`, `notion-research-documentation`, `notion-spec-to-implementation`, `openai-docs`, `pdf`, `playwright-interactive`, `playwright`, `render-deploy`, `screenshot`, `security-best-practices`, `security-ownership-map`, `security-threat-model`, `sentry`, `speech`, `transcribe`, `vercel-deploy`, `winui-app`, `yeet`.

## Selection Notes

- `playwright` was installed because TypeSharp has a buildable Astro docs site and multiple upcoming UI/documentation verification tasks.
- `gh-fix-ci` was installed because the backlog includes GitHub Actions release artifact work and future CI failures should be inspected with GitHub-native evidence.
- `security-threat-model` was installed because TypeSharp has meaningful trust boundaries: source parsing, generated executable artifacts, VS Code extension host process launch, GitHub release artifacts, and local C# assembly interop.
- `openai-docs` was not installed from curated because the system skill is already available.
- `aspnet-core` was not installed because TypeSharp targets .NET Framework 4.8 and classic host compatibility rather than ASP.NET Core.
- `security-best-practices` was not installed because its trigger scope is limited to Python, JavaScript/TypeScript, and Go. Use it later only for explicit JS/TS security guidance around the VS Code extension or docs tooling.
- `playwright-interactive` was not installed because the existing Browser plugin and `playwright` skill cover the current project needs.
- Deployment, Figma, Notion, audio, and unrelated app skills were not installed because they do not match the current TypeSharp backlog.

## Operating Rules

- Do not install a skill only because its name sounds related; install when a concrete task needs it.
- Prefer repo-native tests and docs contracts first. Use skills as task-specific helpers, not as substitutes for TypeSharp verification.
- When a newly installed skill is required, restart Codex before relying on it.
- If official product or platform guidance is needed, prefer official docs and record the source in the task output.
