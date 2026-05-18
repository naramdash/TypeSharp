# TypeSharp.Runtime

`TypeSharp.Runtime` contains low-level helpers for compiler-generated code.

This assembly targets `net48`. User-facing standard library types belong in `TypeSharp.Core` or `TypeSharp.Collections`; generated implementation helpers belong here.

Current helpers:
- `ITypeSharpUnionCase` and `TypeSharpUnion` expose tag, case name, payload, equality, and hash helpers for generated nominal union case classes.
- `TypeSharpPattern` exposes small case and payload checks for generated pattern matching lowering.
- `TypeSharpEquality` exposes equality, sequence equality, and hash composition helpers for generated records and union cases.
