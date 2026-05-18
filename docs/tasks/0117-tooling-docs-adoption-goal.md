# Task 0117: Tooling Docs Adoption Goal

Status: Done
Queue: Q4-Q5
Start Time: 2026-05-19 08:17:40 +09:00
End Time: 2026-05-19 08:22:51 +09:00

## Objective

Extend the TypeSharp adoption goal around editor tooling, official documentation, and runnable examples, then complete the first concrete tooling slice by implementing `typesharp explain`.

## Scope

- Add VS Code LSP extension support as an explicit goal beyond syntax-only scaffolding.
- Add GitHub Pages official documentation powered by Astro Starlight as an explicit adoption deliverable.
- Add runnable real-world example projects as an explicit adoption deliverable.
- Implement `typesharp explain <diagnostic-code>` against the shared diagnostic descriptor registry.
- Document the implemented CLI diagnostic explanation surface and test it.

## Changes

- Added the new adoption requirements to [../goal.md](../goal.md), [../checklist.md](../checklist.md), and [../traceability.md](../traceability.md).
- Added `DiagnosticDescriptors.TryGetByCode` for stable descriptor lookup.
- Added `typesharp explain` text and JSON output in the CLI.
- Added CLI explain smoke tests for text output, JSON output, and unknown diagnostic codes.
- Updated [../cli.md](../cli.md) and [../diagnostics.md](../diagnostics.md) to mark `typesharp explain` as implemented.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "CLI explain"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
git diff --check
```

Result:
- Pass. `CLI explain` focused smoke tests pass.
- Pass. Full `tests\TypeSharp.Compiler.Tests` smoke suite passes.
- Pass. `git diff --check`.

## Handoff

Done:
- Added the requested adoption goals for VS Code LSP extension support, GitHub Pages/Astro Starlight official docs, and runnable example projects.
- Implemented and documented `typesharp explain`.

Remaining:
- Build out the next adoption slices: VS Code LSP client activation/package, Astro Starlight docs site, GitHub Pages workflow, and runnable examples catalog.

Blocked:
- None.
