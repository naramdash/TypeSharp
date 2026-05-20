# Host Worker

Worker-style `net48` host project that references a generated TypeSharp library. The TypeSharp code models a queued billing work item and a nominal processing decision, while the C# worker-style host calls `nextRunLabel()` from the generated assembly.

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
