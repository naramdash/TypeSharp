---
title: Grammar
description: Stable TypeSharp syntax documents and language coverage tracking.
---

The grammar source of truth lives under `docs/grammar/`.

Key entry points:

- `docs/grammar/README.md`: grammar structure, stability levels, and implementation order.
- `docs/grammar/coverage.md`: TypeScript, F#, and C# features classified as Direct, Equivalent, Replacement, Planned, Experimental, or Rejected.
- `docs/grammar/declarations.md`, `types.md`, `expressions.md`, and `patterns.md`: current syntax sketches and examples.
- `tests/fixtures/parser/`: parser snapshots for stable grammar coverage.

Stable grammar features must have both syntax examples and parser-readable fixtures before they are treated as implemented.

