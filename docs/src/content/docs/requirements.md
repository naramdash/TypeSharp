---
title: Project Requirements
description: Required TypeSharp platform, language, compiler, interop, tooling, quality, and security constraints.
---

This page is the docs canonical requirements ledger established by task `0251-docs-canonical-language-ledger`. The former repository bridge file `docs/requirements.md` was removed after its durable content was folded here.

## Requirement Levels

| Level | Meaning |
| --- | --- |
| Required | TypeSharp's stated goal breaks if the item is not implemented or preserved. |
| Recommended | Early versions can ship without the item, but the design should not block it. |
| Optional | Experimental, future, or profile-specific work. |

## Platform Requirements

Required:

- Generated artifacts target `.NET Framework 4.8` by default.
- Generated assemblies must be referenceable from `net48` projects.
- TypeSharp runtime/core libraries must load on `.NET Framework 4.8`.
- Lowering must not depend on APIs that exist only on .NET 5 or later.
- Generated assemblies and runtime dependencies must behave like ordinary `net48` libraries inside ASP.NET Web Forms, ASP.NET MVC/Web API, WCF service/client, Windows Service, scheduled job, queue, and worker-style hosts.
- ASP.NET/WCF/worker compatibility must not require migration to ASP.NET Core or modern .NET worker templates.
- External dependencies require documented `net48` compatibility, license, and deployment impact.
- Compiler, CLI, and language-server hosts may run on a modern .NET LTS runtime as long as generated outputs and runtime libraries pass `net48` compatibility checks.

Recommended:

- Keep compiler core, runtime library, CLI entrypoint, and language server separable.
- Preserve deterministic build behavior.
- Decide debug symbol support before stable release.
- Keep Windows `net48` smoke coverage in CI when the environment is available.
- Track `net48` and `net481` as separate compatibility profiles for vendor-qualified environments.

## Language Requirements

Required:

- Name resolution must be specified for files, namespaces, modules, types, members, and local bindings.
- Stable grammar must cover lexical, module, declaration, type, expression, pattern, and interop syntax.
- Practical C#, F#, and TypeScript features must be classified as Direct, Equivalent, Replacement, Planned, Experimental, or Rejected.
- Primitive types must map clearly to .NET Framework primitive types.
- Class, interface, enum, delegate, record, and nominal closed union models must be defined.
- TypeScript-style type-level unions must support local inference, literal unions, `unknown` narrowing, and structural shape checks.
- Type-level unions, structural shapes, anonymous object shapes, and marker-free `dynamic` must not leak through public .NET ABI.
- Generic types and generic functions must be supported.
- Functions, methods, properties, constructors, events, and calls must have specified declaration and call rules.
- Immutable binding is the default; mutable state must be explicit.
- Compile-time constants use `literal` declarations.
- Nullability is part of the type system.
- Pattern matching must support a practical initial subset and report exhaustiveness diagnostics where possible.
- Public API lowering and generated metadata shape must be documented.

Recommended:

- Keep syntax expression-oriented without making .NET object interop awkward.
- Use local-first inference and prefer explicit public boundary types.
- Keep pipeline, member access, and functional composition readable for C#, F#, and TypeScript users.
- Treat structural types as compile-time proof tools first, not public ABI shapes.

## Feature Classification Requirements

Every modern language feature must be placed in one of these implementation buckets:

| Bucket | Meaning |
| --- | --- |
| MVP | Intended for the first stable implementation path. |
| Stable Backlog | Accepted direction after the first stable slice. |
| Preview Watch | External language or .NET feature is still preview or unsettled. |
| Experimental | TypeSharp needs feature gating and separate validation. |
| Rejected | Incompatible with the goal, runtime, security, or tooling model. |

External preview features, including C# preview and TypeScript beta behavior, cannot become default TypeSharp behavior without an explicit TypeSharp classification and feature gate.

## Compiler Requirements

Required:

- The pipeline has parse, bind, type check, lower, and emit stages.
- Parser recovery should produce multiple diagnostics from one file where possible.
- The symbol table must represent both TypeSharp source symbols and referenced .NET metadata symbols.
- Type checking covers nullability, generics, overload/member resolution, and interop validation.
- Lowering must produce `.NET Framework 4.8`-compatible output.
- The MVP emitter generates C# 7.3-compatible source before building the generated `net48` project.
- Direct IL emission stays behind a backend abstraction and remains backlog until justified.
- Diagnostics include stable codes, spans, explanations, and fix guidance where useful.
- Compiler crashes must be separated from user-code diagnostics.

Recommended:

- Syntax tree and semantic model should be reusable by the language server and analyzer tooling.
- Grammar docs should remain usable by parser, formatter, TextMate grammar, and LSP semantic-token work.
- Parser, diagnostics, lowering, and interop fixtures should stay separate.

## .NET Interop Requirements

Required:

- TypeSharp can reference existing .NET Framework assemblies.
- Manifests distinguish framework references and local DLL references.
- TypeSharp can consume C# classes, interfaces, delegates, enums, attributes, and generic types.
- TypeSharp can call supported constructors, static members, instance members, properties, fields, indexers, events, and delegates.
- C# `.NET Framework` projects can consume TypeSharp public APIs.
- C# overload resolution must be predictable for supported calls.
- Named arguments, optional parameters, `params`, `ref`, `out`, and `in` parameters are metadata-backed.
- Attribute use follows .NET metadata rules.
- WCF contract/data/message shapes must be expressible through TypeSharp public API and C# metadata interop rules.
- Exceptions remain compatible with the .NET exception model.
- Async interop uses `Task` and `Task<T>`.
- Missing C# nullable metadata is treated as unknown nullability in strict contexts.
- Public ABI rejects structural shapes, type-level unions, anonymous objects, and marker-free `dynamic`.

Recommended:

- NuGet support should include `net48` asset selection, transitive dependency, lock, license, and checksum policy before becoming stable.
- Extension method instance-call syntax can grow from the supported metadata receiver subset.
- F# option, tuple, and record interop should remain a future compatibility layer.
- CLS compliance and dynamic/reflection/COM profiles should be explicit opt-in checks.

## Runtime And Standard Library Requirements

Required:

- `Option<T>` or an equivalent absence type is provided.
- `Result<T, E>` or an equivalent error modeling type is provided.
- Core public types live under `TypeSharp.Core` unless a future versioned policy changes that.
- Nominal closed union lowering has stable runtime representation support.
- Collection helpers interoperate with .NET Framework collections.
- Async helpers are `Task`-based.
- Runtime helper ABI is stable enough for generated assemblies and C# consumers.

Recommended:

- Immutable collections need an explicit dependency or implementation decision.
- Equality, ordering, and hashing semantics should be documented per public type family.
- Span-like APIs require `System.Memory` and .NET Framework performance review before adoption.

## Tooling Requirements

Required:

- CLI supports `typesharp version`, `typesharp check`, and `typesharp build`; `typesharp run` is part of the target command surface.
- CLI command, option, exit-code, manifest, source-discovery, and diagnostics contracts are documented.
- VS Code support targets syntax highlighting, diagnostics, hover, go-to-definition, and completion.
- VS Code uses Language Server Protocol and shares compiler semantic information.
- Project manifests are reproducible and mean the same thing to CLI and compiler.
- Formatting conventions exist even before every formatting path is complete.
- Syntax highlighting follows stable grammar.

Recommended:

- `typesharp new` and `typesharp format` remain first-class adoption tools.
- Formatter, linter, analyzer, and LSP features should build on the compiler semantic model.

## Quality, Security, And Release Requirements

Required:

- MVP features need positive tests and negative diagnostics where applicable.
- Public features need documentation, examples, or tests before they are treated as usable.
- Windows `.NET Framework 4.8` smoke tests remain the compatibility proof when available.
- Breaking change policy must be documented before stable release.
- Preview features are separated from stable promises.
- The compiler must not execute arbitrary user code during normal builds.
- Extension points such as source generators, plugins, or macros stay disabled or sandboxed until a policy exists.
- Package and release artifacts need checksum or signing strategy before release.
- Build artifact dependencies must be tracked.

Recommended:

- Performance baselines, diagnostic snapshot review, fuzzing, release reproducibility, and vulnerable dependency response policy should be added as the implementation matures.

## Related Pages

- [Core Goal](../goal/)
- [Feature Status](../feature-status/)
- [Project Configuration](../project-configuration/)
- [.NET Interop](../dotnet-interop/)
- [Advanced Topics](../advanced/)
- [Document Ownership](../document-ownership/)
