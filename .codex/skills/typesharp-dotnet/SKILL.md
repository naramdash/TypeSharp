---
name: "typesharp-dotnet"
description: "Use for TypeSharp work that touches .NET Framework 4.8 compatibility, C# 7.3 source generation, MSBuild project files, TypeSharp.toml references, runtime/core libraries, generated assemblies, C# interop metadata, NuGet/package constraints, or dotnet-based build and test verification."
---

# TypeSharp Dotnet

Keep TypeSharp's .NET surface compatible with Windows 10, .NET Framework 4.8 generated artifacts, and the repository's installed `dotnet` and `node` toolchain assumptions.

## Workflow

1. Identify the .NET surface being changed:
   - compiler or CLI host
   - generated C# source
   - runtime/core library
   - `.csproj`, `Directory.Build.props`, `TypeSharp.toml`, or package references
   - C# metadata interop, overload resolution, nullable metadata, delegates, events, or public ABI
2. Read only the matching canonical docs:
   - `docs/src/content/docs/requirements.md`
   - `docs/src/content/docs/project-policy.md`
   - `docs/src/content/docs/dotnet-interop.md`
   - `docs/src/content/docs/lowering.md`
   - `docs/src/content/docs/api.md`
   - `docs/src/content/docs/cli.md`
3. Preserve the baseline:
   - generated user assemblies target `net48`
   - backend output remains C# 7.3-compatible unless the docs explicitly change
   - public .NET ABI must not expose anonymous structural shapes or type-level unions directly
   - compiler diagnostics remain deterministic in source discovery order
4. Prefer repository-native verification:
   - `dotnet build test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
   - `dotnet run --project test/TypeSharp.Compiler.Tests/TypeSharp.Compiler.Tests.csproj`
   - targeted `dotnet run --project ... --no-build "<filter>"` when the existing harness supports it
5. For docs changes, use the docs Node workflow:
   - `npm run build` from `docs`

## Constraints

- Do not require runtimes or global tools beyond Windows 10 defaults plus installed `dotnet` and `node`.
- Do not add dependencies unless `net48` compatibility and repository policy are clear.
- Do not adopt .NET 10/11-only runtime APIs for TypeSharp generated artifacts.
- Do not introduce preview APIs as stable behavior.
