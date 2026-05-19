# Task 0160: Docs Site Custom 404

Status: Done
Queue: Q5
Start Time: 2026-05-19 17:22:25 +09:00
End Time: 2026-05-19 17:25:49 +09:00

## Objective

Keep the GitHub Pages documentation site build clean by adding a Starlight `docs/404` content entry and disabling the default injected 404 route so the custom entry owns `/404.html` without route conflicts.

## Scope

In:
- Add a human-facing docs-site custom 404 page.
- Keep the 404 page concise and route users back to usable docs entry points.
- Disable Starlight's injected default 404 route after adding the custom docs entry.
- Record docs-site build verification without the previous `Entry docs -> 404 was not found` warning.
- Update task index docs.
- Commit and push when this task is completed.

Out:
- Redesigning the docs-site navigation.
- Changing the deployment workflow.
- Reworking Pagefind or Starlight dependencies.

## Acceptance Criteria

- [x] `docs-site/src/content/docs/404.md` exists with clear recovery links.
- [x] `docs-site/astro.config.mjs` disables the injected default 404 route so the custom page does not conflict with it.
- [x] `npm run build` in `docs-site` passes without `Entry docs -> 404 was not found`.
- [x] Standard repository hygiene checks pass.
- [x] Verification commands pass and are recorded before the task is marked Done.
- [x] The completed task is committed and pushed.

## Suggested Verification

```text
npm run build
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Progress

- 2026-05-19 17:22:25 +09:00: Started after repeated docs-site builds passed but printed `Entry docs -> 404 was not found`; local node_modules inspection showed Starlight probes `getEntry('docs', '404')` before using its fallback route.
- 2026-05-19 17:25:49 +09:00: Added a custom `docs/404` content entry, disabled Starlight's injected default 404 route to avoid a duplicate `/404` route, and confirmed the site generates `dist/404.html` without build warnings.

## Verification

- `npm run build` in `docs-site`: passed; no `Entry docs -> 404 was not found` warning and no duplicate `/404` route warning.
- `Select-String -Path docs-site\dist\404.html -Pattern "The page you requested|Start Here|TypeSharp"`: found the custom 404 title, recovery copy, and recovery link in the generated `/404.html`.
- `git diff --check`: passed.
- `git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"`: no tracked binaries returned.
