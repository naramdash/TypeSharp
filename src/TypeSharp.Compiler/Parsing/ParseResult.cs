using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Parsing;

public sealed record ParseResult(IReadOnlyList<Diagnostic> Diagnostics)
{
    public SyntaxNode? Root { get; init; }

    public bool HasErrors => Diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
