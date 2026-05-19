# Task 0145: GitHub README Human Accessibility

Status: Done
Queue: Q5
Start Time: 2026-05-19 14:40:25 +09:00
End Time: 2026-05-19 14:47:32 +09:00

## Objective

Create or rewrite the repository root `README.md` so the GitHub repository page is understandable to a normal human user, not only to contributors following internal goal/task documents.

## Scope

In:
- Add a root `README.md` if it is still missing.
- Start with a plain-language explanation of what TypeSharp is, who it is for, and its current preview status.
- Provide a short "try it" path with the minimum commands a user can run from a fresh clone.
- Link to the official docs site, goal, CLI docs, VS Code/LSP docs, examples, migration guide, and diagnostics guide without making readers understand the task packet system first.
- Include a compact repository map for `src`, `tests`, `docs`, `docs-site`, `examples`, and `vscode`.
- Separate user-facing status from internal agent/goal execution notes.
- Mention that generated binaries are build outputs and are not expected to be committed.
- Keep language clear, scannable, and accessible for GitHub rendering.

Out:
- Rewriting the full docs site.
- Changing TypeSharp language behavior.
- Release packaging, badges that require external services, or publishing automation.
- Reorganizing directories beyond README links.
- Marketing copy that overstates implementation maturity.

## Acceptance Criteria

- [x] Root `README.md` exists.
- [x] The first screen answers "what is this?", ".NET Framework 4.8 why?", "current status?", and "how do I try it?".
- [x] Quickstart commands match existing CLI/build/test contracts.
- [x] Links resolve to existing repository paths or intentionally documented external locations.
- [x] The README explains where human docs live and where internal task/goal documents live.
- [x] The README avoids unexplained internal jargon such as task packet, goal mode, or traceability before the user-facing introduction.
- [x] Verification records at least README path/link checks and relevant docs/test smoke commands.

## Verification

Planned commands:

```text
Test-Path README.md
rg -n "\]\([^)]+\)" README.md
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site contract"
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- PASS root `README.md` exists.
- PASS README link extraction and local path checks.
- PASS compiler test project build.
- PASS docs site contract smoke.
- PASS docs-site build.
- PASS whitespace check.
- PASS tracked binary artifact check returned no files.

## Handoff

Done:
- Added root `README.md` with a human-readable project introduction, preview status, `net48` motivation, quickstart commands, docs links, repository map, development checks, generated binary policy, and contributor/governance pointers.
- Updated checklist, traceability, and task index.

Remaining:
- None for the root README entry point.
- Broader GitHub Pages tutorial/guide/cookbook expansion remains queued in [0146-github-pages-human-docs-expansion.md](0146-github-pages-human-docs-expansion.md).

Blocked:
- None.
