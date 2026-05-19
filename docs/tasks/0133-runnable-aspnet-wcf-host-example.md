# Task 0133: Runnable ASP.NET/WCF Host Example

Status: Done
Queue: Q5
Start Time: 2026-05-19 12:53:00 +09:00
End Time: 2026-05-19 13:02:00 +09:00

## Objective

Expand the smoke-tested runnable example catalog so .NET Framework ASP.NET/WCF adoption is represented by an actual example project, not only by internal compatibility tests.

## Scope

In:
- `docs/examples/runnable/host-aspnet-wcf` TypeSharp library project.
- C# `net48` host project that references the generated TypeSharp assembly.
- ASP.NET Web Forms-style `System.Web.UI.Page` host code.
- WCF `ServiceContract`/`OperationContract` host code.
- `web.config` deployment-shape placeholder.
- Runnable catalog smoke matrix and command smoke updates.
- Checklist, traceability, and docs-site examples updates.

Out:
- IIS execution or packaging.
- WCF endpoint hosting/runtime execution.
- ASP.NET MVC/Web API example expansion.

## Acceptance Criteria

- [x] Catalog lists an ASP.NET/WCF host example.
- [x] Example has `TypeSharp.toml`, `.tysh` source, README, host project, host source, and config placeholder.
- [x] Smoke tests copy the example to a temporary workspace.
- [x] Smoke tests run `typesharp build` and `dotnet build host/AspNetWcfHostSmoke.csproj`.
- [x] Checklist and traceability include ASP.NET/WCF runnable example evidence.

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
- Added `host-aspnet-wcf` runnable example.
- Updated runnable catalog and docs-site examples page.
- Updated runnable example smoke matrix and command smoke.

Remaining:
- None.

Blocked:
- None.
