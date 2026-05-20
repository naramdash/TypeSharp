# ASP.NET and WCF Host Example

This example builds a generated TypeSharp `net48` library, then compiles a C# host project that references it from ASP.NET Web Forms-style, WCF service, and WCF client/proxy-shaped code. The TypeSharp code models a greeting request and renderer that the host calls from each application shape.

## Code Walkthrough

This TypeSharp block defines the generated library surface. `GreetingRequest` is a nominal record that C# hosts can construct, and `greeting()` is the simple exported helper used by the host smoke:

```tysh
public record GreetingRequest(UserName: string, Tenant: string)

export fun renderGreeting(request: GreetingRequest): string =
  "Hello " + request.UserName + " from " + request.Tenant

export fun greeting(): string {
  let request: GreetingRequest = { UserName: "Ada", Tenant: "LegacyPortal" }
  renderGreeting(request)
}
```

This C# block is the ASP.NET-style consumer. It calls the generated TypeSharp module from a `System.Web.UI.Page`-derived type while also touching Core/Runtime dependencies expected in deployment:

```csharp
public sealed class GreetingPage : Page
{
    public string RenderGreeting()
    {
        var unit = TypeSharp.Core.Unit.Value;
        return Samples.Runnable.HostAspNetWcf.Module.greeting()
            + ":"
            + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework
            + ":"
            + unit.ToString();
    }
}
```

This C# block is the WCF-shaped consumer. It keeps the service contract and `ClientBase<T>` proxy shape in the same smoke project so the example covers both server and client reference patterns:

```csharp
[ServiceContract]
public interface IGreetingService
{
    [OperationContract]
    string GetGreeting();
}

public sealed class GreetingClient : ClientBase<IGreetingService>, IGreetingService
{
    public string GetGreeting()
    {
        return Channel.GetGreeting();
    }
}
```

This XML block is not executed by TypeSharp; it is the ASP.NET/WCF deployment-shape placeholder that records the expected `basicHttpBinding`, service endpoint, and client endpoint wiring:

```xml
<system.serviceModel>
  <bindings>
    <basicHttpBinding>
      <binding name="GreetingBinding" />
    </basicHttpBinding>
  </bindings>
</system.serviceModel>
```

## Commands

This command block builds the TypeSharp library, prepares the Core/Runtime `net48` dependencies, and compiles the ASP.NET/WCF host smoke project:

```text
typesharp build
dotnet build ../../../../lang/TypeSharp.Core/TypeSharp.Core.csproj --nologo --verbosity quiet --ignore-failed-sources
dotnet build ../../../../lang/TypeSharp.Runtime/TypeSharp.Runtime.csproj --nologo --verbosity quiet --ignore-failed-sources
mkdir lib
copy ..\..\..\..\lang\TypeSharp.Core\bin\Debug\net48\TypeSharp.Core.dll lib\
copy ..\..\..\..\lang\TypeSharp.Runtime\bin\Debug\net48\TypeSharp.Runtime.dll lib\
dotnet build host/AspNetWcfHostSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources
```

## Expected Result

- `typesharp build` produces `generated/bin/Debug/net48/HostAspNetWcf.dll`.
- The host project compiles `System.Web.UI.Page`, WCF `ServiceContract`/`OperationContract`, and `ClientBase<IGreetingService>` code that references `Samples.Runnable.HostAspNetWcf.Module.greeting()`, `TypeSharp.Core`, and `TypeSharp.Runtime`.
- `host/web.config` remains a deployment-shape placeholder with `basicHttpBinding`, service endpoint, and client endpoint entries for ASP.NET/WCF configuration smoke coverage.
