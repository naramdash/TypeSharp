using System.Text;

namespace TypeSharp.Compiler.Diagnostics;

public static class DiagnosticJsonFormatter
{
    public static string ToJson(IReadOnlyList<Diagnostic> diagnostics)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        if (diagnostics.Count == 0)
        {
            builder.AppendLine("  \"diagnostics\": []");
            builder.AppendLine("}");
            return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
        }

        builder.AppendLine("  \"diagnostics\": [");

        for (var index = 0; index < diagnostics.Count; index++)
        {
            var diagnostic = diagnostics[index];
            var comma = index == diagnostics.Count - 1 ? string.Empty : ",";

            builder.AppendLine("    {");
            builder.AppendLine($"      \"code\": \"{Escape(diagnostic.Code)}\",");
            builder.AppendLine($"      \"severity\": \"{diagnostic.Severity.ToString().ToLowerInvariant()}\",");
            builder.AppendLine($"      \"message\": \"{Escape(diagnostic.Message)}\",");
            builder.AppendLine($"      \"file\": \"{Escape(diagnostic.File)}\",");
            builder.AppendLine($"      \"start\": {{ \"line\": {diagnostic.Span.Start.Line}, \"column\": {diagnostic.Span.Start.Column} }},");
            builder.AppendLine($"      \"end\": {{ \"line\": {diagnostic.Span.End.Line}, \"column\": {diagnostic.Span.End.Column} }}");
            builder.AppendLine($"    }}{comma}");
        }

        builder.AppendLine("  ]");
        builder.AppendLine("}");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string Escape(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
}
