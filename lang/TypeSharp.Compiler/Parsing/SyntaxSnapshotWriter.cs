using System.Text;

namespace TypeSharp.Compiler.Parsing;

public static class SyntaxSnapshotWriter
{
    public static string Write(SyntaxNode root)
    {
        var builder = new StringBuilder();
        WriteNode(builder, root, indent: 0);
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void WriteNode(StringBuilder builder, SyntaxNode node, int indent)
    {
        builder.Append(' ', indent * 2);

        if (node.IsMissing)
        {
            builder.Append("Missing ");
            builder.Append(node.Kind);
            builder.Append(" TextSpan=");
            builder.Append(node.Span);
            builder.AppendLine();
            return;
        }

        if (node.IsToken)
        {
            builder.Append("Token ");
            builder.Append(node.Kind);
            builder.Append(" ");
            builder.Append('"');
            builder.Append(Escape(node.Text ?? string.Empty));
            builder.Append('"');
            builder.Append(" TextSpan=");
            builder.Append(node.Span);
            builder.AppendLine();
            return;
        }

        builder.Append(node.Kind);
        builder.Append(" TextSpan=");
        builder.Append(node.Span);
        builder.AppendLine();

        foreach (var child in node.Children)
        {
            WriteNode(builder, child, indent + 1);
        }
    }

    private static string Escape(string text) =>
        text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
}
