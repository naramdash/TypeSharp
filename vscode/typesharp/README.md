# TypeSharp VS Code Extension

This directory contains the initial VS Code extension scaffold for TypeSharp.

Current scope:
- registers the `typesharp` language id
- associates `.tysh` files with TypeSharp
- contributes language configuration for comments, brackets, and pairs
- contributes a TextMate grammar based on `docs/grammar/lexical.md`

Out of scope for this scaffold:
- Language Server Protocol client activation
- diagnostics
- hover
- go-to-definition
- completion
- extension packaging and publishing

Those features remain tracked by `docs/checklist.md` and will use the compiler semantic model shared with the CLI.
