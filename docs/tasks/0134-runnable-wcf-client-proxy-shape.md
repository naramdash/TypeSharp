# Task 0134: Runnable WCF Client Proxy Shape

Status: Done
Queue: Q5
Start Time: 2026-05-19 13:02:00 +09:00
End Time: 2026-05-19 13:08:00 +09:00

## Objective

Extend the ASP.NET/WCF runnable host example so it covers WCF client/proxy-shaped consumption of a generated TypeSharp library, not only service contract implementation.

## Scope

In:
- `ClientBase<IGreetingService>` WCF client/proxy-shaped C# code.
- `web.config` service/client endpoint placeholder.
- `basicHttpBinding` configuration placeholder.
- Runnable example smoke assertions for source/config shape.
- Checklist and traceability updates.

Out:
- Runtime WCF endpoint hosting.
- Generated proxy code from `svcutil`.
- Network calls or IIS-hosted execution.

## Acceptance Criteria

- [x] Host source contains a `ClientBase<IGreetingService>` proxy-shaped class.
- [x] Host source compiles against generated TypeSharp `net48` assembly.
- [x] `web.config` includes service and client endpoint placeholders.
- [x] Runnable example smoke verifies source/config shape and host build.
- [x] Checklist and traceability include WCF client/proxy-shaped runnable evidence.

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
- Added WCF client/proxy-shaped source to `host-aspnet-wcf`.
- Added WCF binding/service/client config placeholders.
- Added runnable example smoke assertions for client/proxy and config shape.

Remaining:
- None.

Blocked:
- None.
