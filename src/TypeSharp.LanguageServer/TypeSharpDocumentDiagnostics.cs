using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Semantics;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentDiagnostics
{
    public static IReadOnlyList<Diagnostic> CheckText(string text, string fileName)
    {
        return TypeSharpSemanticModel.AnalyzeText(text, fileName).Diagnostics;
    }
}
