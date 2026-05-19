# Task 0120: Astro Starlight Docs Site

Status: Done
Queue: Q5
Start Time: 2026-05-19 10:22:30 +09:00
End Time: 2026-05-19 10:26:47 +09:00

## Objective

Complete the official documentation site adoption slice by adding an Astro Starlight site that links the core goal, grammar, CLI, diagnostics, VS Code/LSP, migration, and examples docs, then add a GitHub Pages deployment workflow and prove the site builds.

## Scope

In:
- `docs-site` Astro Starlight project.
- Starlight config, content collection config, TypeScript config, package manifest, and lockfile.
- Documentation pages for overview, goal, grammar, CLI, diagnostics, VS Code/LSP, migration, and examples.
- GitHub Pages workflow that builds on PR/push and deploys non-PR builds.
- Dependency inventory, checklist, traceability, and task index updates.

Out:
- Custom theme/components.
- Generated API reference import.
- Publishing a live Pages environment from this local session.

## Acceptance Criteria

- [x] `docs-site/package.json` defines `dev`, `build`, and `preview` scripts.
- [x] `docs-site/astro.config.mjs` configures Starlight title, description, and sidebar.
- [x] `docs-site/src/content.config.ts` configures Starlight `docsLoader` and `docsSchema`.
- [x] Starlight pages connect core goal, grammar, CLI, diagnostics, VS Code/LSP, migration, and examples.
- [x] `.github/workflows/docs.yml` builds docs and deploys `docs-site/dist` through GitHub Pages actions.
- [x] Static smoke tests cover docs site package/config/pages and GitHub Pages workflow contract.
- [x] `npm run build` succeeds.

## Verification

Representative commands:

```text
npm view astro version
npm view @astrojs/starlight version
npm view astro license
npm view @astrojs/starlight license
npm view typescript license
npm install
npm run build
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "GitHub Pages workflow"
```

Result:
- PASS verified current npm versions: `astro` 6.3.5 and `@astrojs/starlight` 0.39.2.
- PASS verified licenses: `astro` MIT, `@astrojs/starlight` MIT, `typescript` Apache-2.0.
- PASS `npm install`, creating `docs-site/package-lock.json`.
- PASS `npm run build`, generating 9 static pages plus Pagefind search index.
- PASS `docs site contract is stable`.
- PASS `GitHub Pages workflow contract is stable`.

## Handoff

Done:
- Added the official Starlight docs site and GitHub Pages workflow.
- Marked docs site, Pages workflow, and docs build smoke checklist items complete.

Remaining:
- Live deployment must run in GitHub Actions after these files are pushed to the default branch.
- Future docs can expand beyond the initial navigation set without changing the build contract.

Blocked:
- None.
