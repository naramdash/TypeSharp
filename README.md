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

dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj

dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll version
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll new console HelloTypeSharp --target net48 --output .\scratch\HelloTypeSharp
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll check .\scratch\HelloTypeSharp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll build .\scratch\HelloTypeSharp\TypeSharp.toml
dotnet src\TypeSharp.Cli\bin\Debug\net10.0\typesharp.dll run .\scratch\HelloTypeSharp\TypeSharp.toml
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
- Docs source: [docs-site](docs-site)
- CLI contract: [docs-site/src/content/docs/cli.md](docs-site/src/content/docs/cli.md)
- VS Code and LSP contract: [docs-site/src/content/docs/vscode-lsp.md](docs-site/src/content/docs/vscode-lsp.md)
- Runnable examples: [examples/runnable](examples/runnable)
- Migration guide: [docs-site/src/content/docs/migration.md](docs-site/src/content/docs/migration.md)
- Diagnostics guide: [docs-site/src/content/docs/diagnostics.md](docs-site/src/content/docs/diagnostics.md)

Design and implementation docs:

- Project goal: [docs-site/src/content/docs/goal.md](docs-site/src/content/docs/goal.md)
- Language grammar: [docs-site/src/content/docs/grammar.md](docs-site/src/content/docs/grammar.md)
- C# interop: [docs-site/src/content/docs/dotnet-interop.md](docs-site/src/content/docs/dotnet-interop.md)
- Lowering reference: [docs-site/src/content/docs/lowering.md](docs-site/src/content/docs/lowering.md)
- Feature status: [docs-site/src/content/docs/feature-status.md](docs-site/src/content/docs/feature-status.md)
- Traceability: [docs/traceability.md](docs/traceability.md)

The [docs](docs) folder is now the temporary agentic work surface for task packets, handoff, traceability, and bridge files. It is useful for contributors and agents, but it is not the best starting point for new users.

## Repository Map

| Path | Purpose |
| --- | --- |
| [src](src) | compiler, CLI, language server, runtime, and core library projects |
| [tests](tests) | smoke tests, parser/type-checker/backend fixtures, runnable example verification |
| [docs](docs) | temporary agentic work surface, task packets, traceability, and bridge files |
| [docs-site](docs-site) | canonical Astro Starlight GitHub Pages documentation site |
| [examples](examples) | single-file examples and runnable adoption projects |
| [vscode/typesharp](vscode/typesharp) | VS Code extension package, syntax, formatter, and LSP client |

## Development Checks

Common local checks:

```powershell
dotnet build tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj
dotnet run --project tests\TypeSharp.Compiler.Tests\TypeSharp.Compiler.Tests.csproj --no-build

cd docs-site
npm run build
```

Before committing generated output, check that no binaries are tracked:

```powershell
git ls-files "*.dll" "*.exe" "vscode/typesharp/server/*"
```

## Project Governance

The project is being developed against the goal and success criteria in [docs-site Core Goal](docs-site/src/content/docs/goal.md). Work is tracked in small task packets under [docs/tasks](docs/tasks), and completed behavior is connected back to requirements through [docs/traceability.md](docs/traceability.md).

If you are here to use or evaluate TypeSharp, start with the quickstart above and the GitHub Pages docs. If you are here to continue implementation work, read [agent.md](agent.md), [docs/agentic-execution.md](docs/agentic-execution.md), and the latest entries in [docs/tasks](docs/tasks).
