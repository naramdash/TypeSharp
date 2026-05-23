# TypeSharp

TypeSharp is a preview language, compiler, CLI, and VS Code tooling project for long-lived .NET Framework applications.

The goal is to generate `.NET Framework 4.8` artifacts while offering a more modern static language experience: TypeScript-inspired structural typing and narrowing, F#-style functional modeling, and C#-friendly interop with existing assemblies.

## Current Status

TypeSharp is not a released production language yet. This repository currently contains:

- a compiler pipeline for `.tysh` source files
- a C# 7.3-compatible source backend that builds `net48` projects
- a CLI with `new`, `check`, `build`, `run`, `format`, `version`, and `explain`
- runtime/core libraries targeting `net48`
- VS Code syntax, formatter, and LSP integration
- smoke-tested runnable examples for console, library, C# interop, ASP.NET/WCF-style host, worker-style host, and diagnostics workflows

Generated binaries are build outputs. They should not be committed to the repository.

## Why .NET Framework 4.8?

Many Windows desktop, server, plugin, ASP.NET, WCF, and internal business systems still run on .NET Framework. TypeSharp is designed for those environments: keep the deployment and hosting model stable, but improve the language and tooling available to new code.

The compiler and tools can run on a modern .NET SDK. The generated user-facing artifacts and TypeSharp runtime libraries target `net48`.

## Try It

Prerequisites:

- Windows with .NET Framework 4.8 targeting support
- a modern .NET SDK for building the compiler and CLI
- Git

From a fresh clone:

```powershell
git clone https://github.com/naramdash/TypeSharp.git
cd TypeSharp

dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj

dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll new console HelloTypeSharp --target net48 --output .\scratch\HelloTypeSharp
dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll check .\scratch\HelloTypeSharp\TypeSharp.toml
dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll build .\scratch\HelloTypeSharp\TypeSharp.toml
dotnet cli\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll run .\scratch\HelloTypeSharp\TypeSharp.toml
```

If a local antivirus blocks a generated short-lived `.exe`, `check` and `build` still validate the compiler path. Only add local exclusions for folders you trust.

## What TypeSharp Looks Like

```typesharp
namespace Samples.Hello

export record Customer(Name: string, Age: int)

export fun greet(customer: Customer): string =
  "Hello, " + customer.Name
```

TypeSharp lowers implemented features to C# source that can be compiled into a `net48` assembly.

## Documentation

Human-facing docs:

- GitHub Pages documentation: https://typesharp.github.io/TypeSharp/
- Docs source: [docs](docs)
- CLI contract: [docs/src/content/docs/cli.md](docs/src/content/docs/cli.md)
- VS Code and LSP contract: [docs/src/content/docs/vscode-lsp.md](docs/src/content/docs/vscode-lsp.md)
- Runnable examples: [examples/runnable](examples/runnable)
- Migration guide: [docs/src/content/docs/migration.md](docs/src/content/docs/migration.md)
- Diagnostics guide: [docs/src/content/docs/diagnostics.md](docs/src/content/docs/diagnostics.md)

Design and implementation docs:

- Project goal: [docs/src/content/docs/goal.md](docs/src/content/docs/goal.md)
- Language grammar: [docs/src/content/docs/grammar.md](docs/src/content/docs/grammar.md)
- C# interop: [docs/src/content/docs/dotnet-interop.md](docs/src/content/docs/dotnet-interop.md)
- Lowering reference: [docs/src/content/docs/lowering.md](docs/src/content/docs/lowering.md)
- Feature status: [docs/src/content/docs/feature-status.md](docs/src/content/docs/feature-status.md)

The [docs](docs) folder is the canonical Astro Starlight documentation source. Short repository-local notes live under [agent](agent).

## Repository Map

| Path | Purpose |
| --- | --- |
| [cli](cli) | TypeSharp command-line host and user-facing tool entrypoint |
| [lang](lang) | compiler, language server, runtime, and core library projects |
| [test](test) | smoke tests, parser/type-checker/backend fixtures, runnable example verification |
| [docs](docs) | canonical Astro Starlight GitHub Pages documentation site |
| [agent](agent) | short repository-local notes, ADR guidance, and the language 1.0 gap tracker |
| [examples](examples) | single-file examples and runnable adoption projects |
| [vscode](vscode) | VS Code extension workspace, syntax, formatter, and LSP client |

## Development Checks

Common local checks:

```powershell
dotnet build test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project test\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build

cd docs
npm run build
```

Before committing generated output, check that no binaries are tracked:

```powershell
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Project Governance

The project is being developed against the goal and success criteria in [docs Core Goal](docs/src/content/docs/goal.md).

If you are here to use or evaluate TypeSharp, start with the quickstart above and the GitHub Pages docs. If you are here to continue implementation work, read [agent.md](agent.md) and [agent/lang-1.0-tasks.md](agent/lang-1.0-tasks.md).
