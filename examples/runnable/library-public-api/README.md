# Library Public API

Generated `net48` library with a C#-friendly billing account, quote, decision, and calculator API. The TypeSharp build emits the library, then the C# host smoke references the generated assembly plus TypeSharp Core/Runtime dependencies.

```powershell
typesharp check
typesharp build
dotnet build host/LibraryConsumerSmoke.csproj --nologo --verbosity quiet --ignore-failed-sources
```

The build emits `generated/bin/Debug/net48/LibraryPublicApi.dll`.
