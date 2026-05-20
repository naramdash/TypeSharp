# Host Worker

Worker-style `net48` host project that references a generated TypeSharp library. The TypeSharp code models a queued billing work item and a nominal processing decision, while the C# worker-style host calls `nextRunLabel()` from the generated assembly.

## Code Walkthrough

This TypeSharp block defines the generated worker-facing data model. Both records are nominal public shapes, so a C# worker host can reason about the generated metadata:

```tysh
public record WorkItem(Id: string, Queue: string, Attempts: int)

public record WorkDecision(Message: string, Delayed: bool)
```

This TypeSharp block classifies a work item and exposes a stable `nextRunLabel()` function for the worker-style C# host to call:

```tysh
export fun classify(item: WorkItem): WorkDecision =
  WorkDecision(item.Queue + ":" + item.Id + ":attempts=" + item.Attempts.ToString(), false)

export fun nextRunLabel(): string {
  let item: WorkItem = { Id: "renewal-0042", Queue: "billing-renewals", Attempts: 1 }
  let decision = classify(item)
  "processed:" + decision.Message
}
```

This C# block is the worker host proof. The `ServiceBase`-derived type calls the generated TypeSharp module and wraps the result in `TypeSharp.Core.Result`:

```csharp
public sealed class GreetingWorker : ServiceBase
{
    public string RunOnce()
    {
        var result = TypeSharp.Core.Result<string, string>.Ok(Samples.Runnable.HostWorker.Module.nextRunLabel());
        return result.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion.ToString();
    }
}
```

## Commands

This command block builds the TypeSharp library, prepares Core/Runtime `net48` dependencies, and compiles the worker host project:

```powershell
typesharp build
dotnet build ../../../../src/TypeSharp.Core/TypeSharp.Core.csproj --nologo --verbosity quiet --ignore-failed-sources
dotnet build ../../../../src/TypeSharp.Runtime/TypeSharp.Runtime.csproj --nologo --verbosity quiet --ignore-failed-sources
mkdir lib
copy ..\..\..\..\src\TypeSharp.Core\bin\Debug\net48\TypeSharp.Core.dll lib\
copy ..\..\..\..\src\TypeSharp.Runtime\bin\Debug\net48\TypeSharp.Runtime.dll lib\
dotnet build host/WorkerHostSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources
```

The TypeSharp build emits `generated/bin/Debug/net48/HostWorker.dll`, and the host project references that assembly plus `TypeSharp.Core` and `TypeSharp.Runtime` through normal MSBuild `<Reference>` items.
