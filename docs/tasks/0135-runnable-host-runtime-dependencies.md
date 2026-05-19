# Task 0135: Runnable Host Runtime Dependencies

Status: Done
Queue: Q5
Start Time: 2026-05-19 13:05:00 +09:00
End Time: 2026-05-19 13:12:00 +09:00

## Objective

Make runnable .NET Framework host examples show the same generated/Core/Runtime DLL deployment reference shape that internal ASP.NET/WCF/worker compatibility smokes already verify.

## Scope

In:
- ASP.NET/WCF host project references to `TypeSharp.Core` and `TypeSharp.Runtime`.
- Worker host project references to `TypeSharp.Core` and `TypeSharp.Runtime`.
- Host source that uses `TypeSharp.Core` and `TypeSharp.Runtime` APIs.
- Example README commands for preparing runtime dependency DLLs.
- Runnable example smoke setup that copies built Core/Runtime DLLs into each example's `lib/` folder.

Out:
- Publishing NuGet packages.
- Runtime installer or deployment packaging automation.
- IIS or Windows Service execution.

## Acceptance Criteria

- [x] Runnable ASP.NET/WCF host project references generated/Core/Runtime DLLs.
- [x] Runnable worker host project references generated/Core/Runtime DLLs.
- [x] Host source uses Core/Runtime APIs so references are compile-checked.
- [x] Runnable example smoke builds Core/Runtime DLLs, copies them into `lib/`, and compiles host projects.
- [x] Checklist and traceability include runnable host runtime dependency evidence.

## Verification

Representative commands:

```text
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build -- "runnable example"
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build
```

Result:
- PASS compiler test project build.
- PASS focused runnable example smoke tests.
- PASS full compiler test suite.

## Handoff

Done:
- Updated ASP.NET/WCF and worker runnable host projects to reference Core/Runtime dependencies.
- Updated host source and README command guidance.
- Updated runnable example smoke setup and assertions.

Remaining:
- None.

Blocked:
- None.
