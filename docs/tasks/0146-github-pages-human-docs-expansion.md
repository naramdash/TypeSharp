# Task 0146: GitHub Pages Human Docs Expansion

Status: Planned
Queue: Q5
Start Time: 2026-05-19 14:42:55 +09:00
End Time: TBD

## Objective

Expand the Astro Starlight GitHub Pages documentation from a small source-of-truth pointer site into a human-readable documentation site that a new TypeSharp user can learn from directly.

## Source References

Use the information architecture patterns from official language documentation:

- TypeScript official docs: get-started tracks by background, handbook, reference, tutorials, declaration files, project configuration, and cheat sheets.
- C# Microsoft Learn docs: get started, fundamentals, language concepts, advanced concepts, reference, language specification, and API reference.
- F# Microsoft Learn docs: get started, language guide/reference, fundamentals, tutorials, in-practice pages, tools, and new-features pages.

## Scope

In:
- Redesign `docs-site/src/content/docs` around a human-first sidebar and page hierarchy.
- Add a "Start Here" path for .NET Framework developers, C# developers, F# developers, and TypeScript developers.
- Add Tutorials that can be followed in order:
  - hello project
  - library public API
  - C# interop
  - diagnostics workflow
  - VS Code/LSP workflow
  - ASP.NET/WCF/worker-style host compatibility overview
- Add Guides for everyday work:
  - project structure and `TypeSharp.toml`
  - `typesharp check/build/run/format`
  - generated C# and `net48` output
  - C# reference and import rules
  - null safety, Option/Result, union modeling, records, pattern matching, async Task interop
  - migration from C# or TypeScript-like code
- Add a Cookbook with short recipes:
  - call a local C# DLL
  - expose a TypeSharp API to C#
  - model nullable input safely
  - create a record and update it
  - use a nominal union with match
  - build and run an executable with arguments
  - consume generated DLLs from a host project
- Add Fundamentals pages:
  - values and functions
  - modules and imports
  - type inference
  - structural shapes versus nominal public API
  - records, classes, interfaces, delegates
  - collections, pipelines, async
  - diagnostics and strict defaults
- Add Grammar and Reference pages:
  - lexical basics
  - declarations
  - expressions
  - types
  - patterns
  - interop syntax
  - public ABI rules
  - diagnostic code index
- Add API/CLI reference pages:
  - CLI command reference
  - manifest reference
  - runtime/core library overview
  - generated assembly layout
  - VS Code extension settings and smoke commands
- Keep canonical design docs linked, but make the site pages explain concepts directly before linking to source-of-truth documents.
- Update Starlight sidebar/nav so users can browse by task: learn, use, reference, migrate, troubleshoot, contribute.
- Use current implementation status labels: implemented, preview, planned, and backlog, without overstating maturity.
- Ensure code snippets are copyable and match parser/build/test evidence where possible.

Out:
- Implementing new language features solely to make docs prettier.
- Publishing release packages or external websites beyond the existing GitHub Pages build.
- Changing generated output behavior unless a docs smoke exposes an existing bug.
- Replacing canonical `docs/` source documents.
- Adding marketing-heavy landing copy without practical learning content.

## Acceptance Criteria

- [ ] The docs site has a human-oriented information architecture inspired by the official TypeScript, C#, and F# docs patterns above.
- [ ] The first page tells users what TypeSharp is, why `net48` matters, current preview status, and the fastest safe next step.
- [ ] Tutorials are sequential and runnable or explicitly tied to existing runnable examples.
- [ ] Guides explain concepts directly instead of only pointing at internal design docs.
- [ ] Cookbook recipes solve concrete user tasks with short code and commands.
- [ ] Fundamentals pages cover the language model a new user needs before reading full grammar/reference material.
- [ ] Grammar/reference/API pages are scannable and link back to canonical grammar, CLI, diagnostics, and interop docs.
- [ ] Site navigation separates Learn, Guides, Cookbook, Reference, Migration, Examples, and Troubleshooting.
- [ ] Every new page either has verified commands, existing fixture/smoke evidence, or an explicit status note.
- [ ] Docs-site build passes and the docs contract smoke is updated if the expected page set changes.

## Verification

Planned commands:

```text
npm run build
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "docs site contract"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "runnable example"
git diff --check
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

Result:
- Not run yet. This task is queued for a future implementation pass.

## Handoff

Done:
- Task packet added to the queue.
- Source reference sites reviewed at a high level:
  - https://www.typescriptlang.org/docs/
  - https://learn.microsoft.com/en-us/dotnet/csharp/
  - https://learn.microsoft.com/en-us/dotnet/fsharp/

Remaining:
- Design the Starlight sidebar and page tree.
- Write the expanded docs content.
- Update docs-site contract smoke if the expected route set changes.
- Run verification.
- Update this packet to `Done`, set `End Time`, then commit and push the implementation.

Blocked:
- None.
