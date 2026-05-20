# ASP.NET and WCF Host Example

This example builds a generated TypeSharp `net48` library, then compiles a C# host project that references it from ASP.NET Web Forms-style, WCF service, and WCF client/proxy-shaped code. The TypeSharp code models a greeting request and renderer that the host calls from each application shape.

## Commands

```text
typesharp build
dotnet build ../../../../src/TypeSharp.Core/TypeSharp.Core.csproj --nologo --verbosity quiet --ignore-failed-sources
dotnet build ../../../../src/TypeSharp.Runtime/TypeSharp.Runtime.csproj --nologo --verbosity quiet --ignore-failed-sources
mkdir lib
copy ..\..\..\..\src\TypeSharp.Core\bin\Debug\net48\TypeSharp.Core.dll lib\
copy ..\..\..\..\src\TypeSharp.Runtime\bin\Debug\net48\TypeSharp.Runtime.dll lib\
dotnet build host/AspNetWcfHostSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources
```

## Expected Result

- `typesharp build` produces `generated/bin/Debug/net48/HostAspNetWcf.dll`.
- The host project compiles `System.Web.UI.Page`, WCF `ServiceContract`/`OperationContract`, and `ClientBase<IGreetingService>` code that references `Samples.Runnable.HostAspNetWcf.Module.greeting()`, `TypeSharp.Core`, and `TypeSharp.Runtime`.
- `host/web.config` remains a deployment-shape placeholder with `basicHttpBinding`, service endpoint, and client endpoint entries for ASP.NET/WCF configuration smoke coverage.
