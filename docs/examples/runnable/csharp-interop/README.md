# C# Interop

TypeSharp project that consumes an explicit local `net48` C# DLL.

```powershell
dotnet build legacy-src/Legacy.Tools.csproj --nologo --verbosity quiet --ignore-failed-sources
typesharp check
typesharp build
```

The legacy build writes `lib/Legacy.Tools.dll`, and `TypeSharp.toml` references that DLL through `references.paths`.

