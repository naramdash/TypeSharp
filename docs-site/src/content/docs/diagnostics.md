---
title: Diagnostics
description: Diagnostic codes, metadata, JSON shape, and explanation surface.
---

Diagnostic policy is defined in `docs/diagnostics.md`.

Diagnostics are shared by:

- compiler and checker internals,
- `typesharp check` and `typesharp build`,
- `typesharp explain`,
- Language Server Protocol diagnostics,
- golden fixture snapshots.

Example explanation command:

```text
typesharp explain TS2204
```

The descriptor registry keeps diagnostic code, severity, category, explanation, and suggested action stable for tools. Current project diagnostics include source discovery and module graph errors like duplicate source module paths with `TS0111`, unresolved source modules with `TS0112`, and unsupported source module imports with `TS0113`; current binding diagnostics include unresolved names like `TS2001`, duplicate symbols like `TS2002`, including same-file import alias conflicts, and unsupported export forwarding like `TS2003`; current type-checking diagnostics include public-boundary leaks like `TS2204`, C# 7.3 backend constraint limits like `TS2205`, dynamic annotation boundary errors like `TS2206`, dynamic call propagation errors like `TS2207`, reflect/interop/unsafe capability call marker errors like `TS2208`, and unknown member/indexer access errors like `TS2209`.
