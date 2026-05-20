---
title: Writing Guide
description: TypeSharp documentation style, tysh example project rules, and review checklist.
---

This guide adapts the [Vue Docs Writing Guide](https://github.com/vuejs/docs/blob/main/.github/contributing/writing-guide.md) for TypeSharp. Reviewed on 2026-05-21.

TypeSharp documentation has one job: help .NET Framework maintainers, language implementers, and early adopters understand what is implemented today, how to verify it, and where the current preview boundary is.

## Core Principles 🧭

- Start with the user's problem before naming the feature.
- Teach one new concept at a time.
- Use specific, realistic examples instead of abstract placeholders.
- Keep guide pages readable from top to bottom; reserve dense detail for reference pages.
- Prefer plain language over jargon. Define TypeSharp-specific terms the first time they appear.
- Link to the canonical page instead of repeating the same rule across many pages.
- Label preview behavior honestly. Do not present future backlog as implemented behavior.

## Page Types 📚

| Page Type | Use It For | Writing Shape |
| --- | --- | --- |
| Overview | What TypeSharp is for and who should care. | Short problem framing, current capability, and next link. |
| Tutorial | A workflow the reader follows once. | Sequential steps with expected command output or artifact paths. |
| Guide | A concept the reader learns in order. | Problem first, one concept per section, small examples. |
| Reference | Complete facts the reader scans later. | Tables, grammar snippets, CLI options, diagnostics, and exact constraints. |
| Cookbook | A practical recipe with prerequisites. | Goal, files, commands, expected result, and failure notes. |
| Ledger | Durable project state or policy. | Current rule, evidence, and update discipline. |

## Headings And Flow ✍️

- Use Title Case headings to match the existing docs.
- Write headings as user-visible problems when possible: "Consume A Local C# DLL" is better than "Interop".
- Put assumptions near the top. If a page assumes C#, .NET Framework 4.8, or TypeScript knowledge, say so.
- When a sentence introduces the next code block, end it with a colon.
- Avoid stacked callouts. If a caveat is important, explain it in the normal flow.
- Avoid words that minimize the reader's effort, such as "just", "easy", and "obvious".

## tysh Example Project Guidelines 🧪

Every `tysh` project example should be small enough to read, but real enough that a maintainer can map it to a production codebase.

Include these files when the example describes a project:

```text
TypeSharp.toml
src/Main.tysh
generated/
```

Show the manifest before the source when configuration matters:

```toml
[project]
name = "BillingRules"
targetFramework = "net48"
outputType = "library"
rootNamespace = "Company.Billing.Rules"
sourceRoots = ["src"]
generatedOutputRoot = "generated"

[language]
version = "preview"
strict = true
nullable = "strict"
previewFeatures = []

[references]
assemblies = ["System", "System.Core"]
paths = [
  "lib/TypeSharp.Core.dll",
  "lib/TypeSharp.Runtime.dll"
]
packages = []

[tooling]
diagnosticFormat = "text"
treatWarningsAsErrors = false
```

Use `tysh` fences for TypeSharp source:

```tysh
namespace Company.Billing.Rules

import { Result, Ok, Error } from "TypeSharp.Core"

public record InvoiceDraft(customerId: string, amount: decimal)

public union BillingError {
  MissingCustomer
  InvalidAmount(amount: decimal)
}

export fun createDraft(
  customerId: string,
  amount: decimal
): Result<InvoiceDraft, BillingError> {
  if customerId.Trim().Length == 0 {
    Error(MissingCustomer)
  }
  elif amount <= 0m {
    Error(InvalidAmount(amount))
  }
  else {
    Ok(InvoiceDraft(customerId: customerId, amount: amount))
  }
}
```

Then show the commands that prove the example:

```powershell
typesharp check
typesharp build --configuration Release
```

State the expected artifact path when `build` is part of the workflow:

```text
generated/BillingRules.csproj
generated/bin/Release/net48/BillingRules.dll
```

## Example Rules ✅

- Use realistic names such as `BillingRules`, `LegacyCustomerAdapter`, or `ClaimsNormalizer`; avoid `Foo`, `Bar`, and `Thing`.
- Keep public .NET boundaries nominal: records, classes, interfaces, delegates, unions, or framework types.
- Do not expose structural shapes, type-level unions, anonymous objects, or inferred anonymous function types in public APIs.
- Show `TypeSharp.toml` when `targetFramework`, `outputType`, `rootNamespace`, references, or generated output matter.
- Include `TypeSharp.Core.dll` and `TypeSharp.Runtime.dll` under `references.paths` when the example depends on runtime or core library symbols.
- Use explicit imports or fully qualified names when a reader could otherwise miss where a symbol comes from.
- Prefer examples that can be checked by `typesharp check` and built by `typesharp build`.
- If a snippet is conceptual, label it before the code block and keep it out of runnable project instructions.
- For runnable example project READMEs, explain every command, expected output, `tysh`, C#, XML, or manifest block before the block appears.
- Keep command examples in `powershell` fences, manifest examples in `toml`, generated C# in `csharp`, and expected output in `text`.

## Emoji Policy ✨

Use emojis as scanning markers, not decoration.

| Marker | Meaning |
| --- | --- |
| 🧭 | Orientation, navigation, or page intent. |
| 🧪 | Verification, tests, or runnable examples. |
| 📦 | Build output, packages, generated projects, and deployment artifacts. |
| ⚠️ | Real warnings that can break a workflow. |
| ✅ | Checklists and review gates. |

Do not put emojis in code blocks, commands, file names, diagnostic text, or headings that are copied into generated artifacts.

## Review Checklist ✅

Before publishing a docs change:

- Confirm the page names the reader's problem before the implementation detail.
- Confirm every TypeSharp source block uses a `tysh` fence.
- Confirm runnable examples include `TypeSharp.toml`, source path, commands, and expected artifact or output.
- Confirm runnable example project READMEs explain every code block before the block appears.
- Confirm preview-only behavior is labeled as preview.
- Confirm links point to canonical docs pages such as [Project Configuration](../project-configuration/), [Runtime Artifacts](../runtime-artifacts/), [Examples](../examples/), and [Feature Status](../feature-status/).
- Run `npm run build` in `docs/`.
- Run the docs site contract when navigation, page inventory, examples, or build assumptions changed.

## Contributor Feedback 💬

Ask reviewers to check the reader journey, not only spelling. Useful review prompts are:

- Is the problem clear before the solution appears?
- Does any section teach more than one new concept at once?
- Can the `tysh` example be verified with the commands shown?
- Is any future work described as if it already exists?
