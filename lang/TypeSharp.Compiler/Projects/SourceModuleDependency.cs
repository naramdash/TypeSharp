using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

public sealed record SourceModuleDependency(
    SourceModuleDependencyKind Kind,
    string FromModulePath,
    string ToModulePath,
    string Specifier,
    string File,
    SourceSpan Span);
