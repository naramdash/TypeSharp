# C# Interop Billing

TypeSharp project that consumes an explicit local `net48` C# DLL. The legacy DLL exposes a customer repository, invoice rules with optional/named arguments, an `out` parameter API, and a `params` formatter.

```powershell
dotnet build legacy-src/Legacy.Tools.csproj --nologo --verbosity quiet --ignore-failed-sources
typesharp check
typesharp build
```

The legacy build writes `lib/Legacy.Tools.dll`, and `TypeSharp.toml` references that DLL through `references.paths`. The generated TypeSharp library exposes `renewalSummary`, `creditLimit`, and `batchLine`.

