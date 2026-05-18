# Task: Migration Guide Draft

Status: Done
Queue: Q5
Start Time: 2026-05-19 04:46:27 +09:00
End Time: 2026-05-19 04:48:00 +09:00

## Objective

Create the first migration guide draft for adopting TypeSharp in existing .NET Framework 4.8/C# environments without overstating unsupported automation.

## Source Of Truth

- [../goal.md](../goal.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../cli.md](../cli.md)
- [../lowering.md](../lowering.md)
- [../checklist.md](../checklist.md)

## Scope

In:
- `docs/migration-guide.md`
- .NET Framework `net48` adoption path
- TypeSharp project setup, references, CLI loop, public API guidance
- C# to TypeSharp porting notes for implemented features
- explicit limitations and Stable Backlog items

Out:
- automatic C# to TypeSharp conversion tooling
- NuGet restore/package migration implementation
- ASP.NET/WCF template generation
- release packaging instructions

## Acceptance Criteria

- [x] Migration guide explains when TypeSharp is appropriate for existing `net48` projects.
- [x] Guide gives a minimal adoption workflow using current CLI/project manifest concepts.
- [x] Guide maps implemented C#/.NET patterns to TypeSharp equivalents and links concrete docs.
- [x] Unsupported migration automation and backlog areas are clearly separated.
- [x] Documentation index, checklist, and traceability link the guide.

## Verification

Command:

```text
git diff --check
```

Expected:
- documentation-only migration guide update has no whitespace errors.

Result:
- Pass. `git diff --check`

## Handoff

Done:
- Added [../migration-guide.md](../migration-guide.md).
- Documented the current gradual adoption path for existing `net48` C# projects.
- Mapped implemented C#/.NET patterns to TypeSharp equivalents.
- Separated unsupported automation and Stable Backlog migration tooling.
- Linked the guide from [../README.md](../README.md), checklist, task index, and traceability.

Remaining:
- Automatic conversion tools, NuGet restore/package migration, host templates, release packaging, and public ABI snapshot tooling remain out of scope.

Blocked:
- None.
