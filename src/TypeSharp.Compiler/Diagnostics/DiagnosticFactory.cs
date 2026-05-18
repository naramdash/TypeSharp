namespace TypeSharp.Compiler.Diagnostics;

internal static class DiagnosticFactory
{
    public static Diagnostic Manifest(
        DiagnosticDescriptor descriptor,
        string message,
        string file,
        int line = 1,
        int column = 1)
    {
        var position = new SourcePosition(line, column);
        return new Diagnostic(descriptor.Code, descriptor.DefaultSeverity, message, file, new SourceSpan(position, position));
    }
}
