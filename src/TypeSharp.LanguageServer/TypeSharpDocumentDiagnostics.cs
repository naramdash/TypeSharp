using TypeSharp.Compiler.Binding;
using TypeSharp.Compiler.Diagnostics;
using TypeSharp.Compiler.Parsing;
using TypeSharp.Compiler.TypeChecking;

namespace TypeSharp.LanguageServer;

public static class TypeSharpDocumentDiagnostics
{
    public static IReadOnlyList<Diagnostic> CheckText(string text, string fileName)
    {
        var diagnostics = new List<Diagnostic>();
        var parseResult = TypeSharpParser.ParseText(text, fileName);
        diagnostics.AddRange(parseResult.Diagnostics);

        if (parseResult.HasErrors || parseResult.Root is null)
        {
            return diagnostics;
        }

        var bindingResult = TypeSharpBinder.Bind(parseResult.Root, fileName);
        diagnostics.AddRange(bindingResult.Diagnostics);
        if (!bindingResult.HasErrors)
        {
            diagnostics.AddRange(TypeSharpTypeChecker.Check(parseResult.Root, fileName).Diagnostics);
        }

        return diagnostics;
    }
}
