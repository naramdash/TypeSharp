# Task 0137: Host Compatibility Doc Consistency

Status: Done
Queue: Q5
Start Time: 2026-05-19 13:20:47 +09:00
End Time: 2026-05-19 13:22:19 +09:00

## Objective

Make host compatibility docs distinguish the smoke coverage already implemented from the generated template, IIS packaging, WCF config generation, and Windows Service installer automation that remain Stable Backlog.

## Scope

In:
- C# interop application model compatibility scope wording.
- Feature map status for .NET Framework application model compatibility.
- Feasibility review boundaries for host smokes versus template/packaging automation.
- Traceability evidence for current runnable/internal host smoke coverage.

Out:
- New host templates.
- IIS-hosted execution tests.
- Windows Service installer scaffolding.
- WCF config generation automation.

## Acceptance Criteria

- [x] C# interop docs state ASP.NET/WCF/worker smoke coverage is implemented.
- [x] Feature map keeps host template and packaging automation in Stable Backlog.
- [x] Feasibility review no longer describes all host-specific smokes as future work.
- [x] Traceability records current host smoke evidence and remaining automation boundaries.
- [x] Task queue records the doc consistency pass.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
npm run build
git diff --check
```

Result:
- PASS compiler test project build.
- PASS full compiler test suite.
- PASS docs-site build.
- PASS whitespace check.

## Handoff

Done:
- Updated host compatibility docs to match implemented runnable/internal smoke coverage.
- Kept generated templates, IIS packaging, WCF config generation automation, and Windows Service installer scaffolding explicitly out of current MVP implementation scope.

Remaining:
- None.

Blocked:
- None.
