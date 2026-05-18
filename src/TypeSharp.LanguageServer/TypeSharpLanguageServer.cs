using System.Text;
using System.Text.Json;
using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.LanguageServer;

public static class TypeSharpLanguageServer
{
    public static void Run(Stream input, Stream output, string workspaceRoot)
    {
        var openDocuments = new Dictionary<string, string>(StringComparer.Ordinal);

        while (TryReadMessage(input, out var message))
        {
            using var document = JsonDocument.Parse(message);
            var root = document.RootElement;
            if (!root.TryGetProperty("method", out var methodElement))
            {
                continue;
            }

            var method = methodElement.GetString();
            var hasId = root.TryGetProperty("id", out var idElement);

            switch (method)
            {
                case "initialize":
                    if (hasId)
                    {
                        WriteInitializeResponse(output, idElement);
                    }
                    break;

                case "shutdown":
                    if (hasId)
                    {
                        WriteNullResponse(output, idElement);
                    }
                    break;

                case "exit":
                    return;

                case "textDocument/didOpen":
                    PublishDidOpenDiagnostics(root, output, openDocuments, workspaceRoot);
                    break;

                case "textDocument/didChange":
                    PublishDidChangeDiagnostics(root, output, openDocuments, workspaceRoot);
                    break;

                default:
                    if (hasId)
                    {
                        WriteNullResponse(output, idElement);
                    }
                    break;
            }
        }
    }

    private static void PublishDidOpenDiagnostics(
        JsonElement root,
        Stream output,
        IDictionary<string, string> openDocuments,
        string workspaceRoot)
    {
        if (!TryGetTextDocument(root, out var uri, out var text))
        {
            return;
        }

        openDocuments[uri] = text;
        PublishDiagnostics(output, uri, text, workspaceRoot);
    }

    private static void PublishDidChangeDiagnostics(
        JsonElement root,
        Stream output,
        IDictionary<string, string> openDocuments,
        string workspaceRoot)
    {
        if (!root.TryGetProperty("params", out var parameters)
            || !parameters.TryGetProperty("textDocument", out var textDocument)
            || !textDocument.TryGetProperty("uri", out var uriElement)
            || uriElement.GetString() is not { Length: > 0 } uri
            || !parameters.TryGetProperty("contentChanges", out var contentChanges)
            || contentChanges.ValueKind != JsonValueKind.Array
            || contentChanges.GetArrayLength() == 0
            || !contentChanges[0].TryGetProperty("text", out var textElement)
            || textElement.GetString() is not { } text)
        {
            return;
        }

        openDocuments[uri] = text;
        PublishDiagnostics(output, uri, text, workspaceRoot);
    }

    private static void PublishDiagnostics(Stream output, string uri, string text, string workspaceRoot)
    {
        var fileName = ToFileName(uri, workspaceRoot);
        var diagnostics = TypeSharpDocumentDiagnostics.CheckText(text, fileName);
        WritePublishDiagnostics(output, uri, diagnostics);
    }

    private static bool TryGetTextDocument(JsonElement root, out string uri, out string text)
    {
        uri = string.Empty;
        text = string.Empty;

        if (!root.TryGetProperty("params", out var parameters)
            || !parameters.TryGetProperty("textDocument", out var textDocument)
            || !textDocument.TryGetProperty("uri", out var uriElement)
            || uriElement.GetString() is not { Length: > 0 } documentUri
            || !textDocument.TryGetProperty("text", out var textElement)
            || textElement.GetString() is not { } documentText)
        {
            return false;
        }

        uri = documentUri;
        text = documentText;
        return true;
    }

    private static void WriteInitializeResponse(Stream output, JsonElement id)
    {
        using var payload = new MemoryStream();
        using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WriteString("jsonrpc", "2.0");
            writer.WritePropertyName("id");
            id.WriteTo(writer);
            writer.WritePropertyName("result");
            writer.WriteStartObject();
            writer.WritePropertyName("capabilities");
            writer.WriteStartObject();
            writer.WriteNumber("textDocumentSync", 1);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static void WriteNullResponse(Stream output, JsonElement id)
    {
        using var payload = new MemoryStream();
        using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WriteString("jsonrpc", "2.0");
            writer.WritePropertyName("id");
            id.WriteTo(writer);
            writer.WriteNull("result");
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static void WritePublishDiagnostics(Stream output, string uri, IReadOnlyList<Diagnostic> diagnostics)
    {
        using var payload = new MemoryStream();
        using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WriteString("jsonrpc", "2.0");
            writer.WriteString("method", "textDocument/publishDiagnostics");
            writer.WritePropertyName("params");
            writer.WriteStartObject();
            writer.WriteString("uri", uri);
            writer.WritePropertyName("diagnostics");
            writer.WriteStartArray();
            foreach (var diagnostic in LspDiagnosticMapper.ToLspDiagnostics(diagnostics))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("range");
                writer.WriteStartObject();
                writer.WritePropertyName("start");
                writer.WriteStartObject();
                writer.WriteNumber("line", diagnostic.Range.Start.Line);
                writer.WriteNumber("character", diagnostic.Range.Start.Character);
                writer.WriteEndObject();
                writer.WritePropertyName("end");
                writer.WriteStartObject();
                writer.WriteNumber("line", diagnostic.Range.End.Line);
                writer.WriteNumber("character", diagnostic.Range.End.Character);
                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteNumber("severity", diagnostic.Severity);
                writer.WriteString("source", diagnostic.Source);
                writer.WriteString("code", diagnostic.Code);
                writer.WriteString("message", diagnostic.Message);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static bool TryReadMessage(Stream input, out string message)
    {
        message = string.Empty;
        var header = ReadHeader(input);
        if (header.Length == 0)
        {
            return false;
        }

        var contentLength = ParseContentLength(header);
        if (contentLength <= 0)
        {
            return false;
        }

        var body = new byte[contentLength];
        var offset = 0;
        while (offset < body.Length)
        {
            var read = input.Read(body, offset, body.Length - offset);
            if (read == 0)
            {
                return false;
            }

            offset += read;
        }

        message = Encoding.UTF8.GetString(body);
        return true;
    }

    private static string ReadHeader(Stream input)
    {
        var bytes = new List<byte>();
        while (true)
        {
            var value = input.ReadByte();
            if (value < 0)
            {
                return string.Empty;
            }

            bytes.Add((byte)value);
            if (bytes.Count >= 4
                && bytes[^4] == (byte)'\r'
                && bytes[^3] == (byte)'\n'
                && bytes[^2] == (byte)'\r'
                && bytes[^1] == (byte)'\n')
            {
                return Encoding.ASCII.GetString(bytes.ToArray());
            }
        }
    }

    private static int ParseContentLength(string header)
    {
        foreach (var line in header.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = line.IndexOf(':');
            if (separator < 0)
            {
                continue;
            }

            var name = line[..separator].Trim();
            if (!string.Equals(name, "Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (int.TryParse(line[(separator + 1)..].Trim(), out var contentLength))
            {
                return contentLength;
            }
        }

        return 0;
    }

    private static void WriteFramedJson(Stream output, byte[] payload)
    {
        var header = Encoding.ASCII.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");
        output.Write(header, 0, header.Length);
        output.Write(payload, 0, payload.Length);
        output.Flush();
    }

    private static string ToFileName(string uri, string workspaceRoot)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed) && parsed.IsFile)
        {
            return Path.GetRelativePath(workspaceRoot, parsed.LocalPath).Replace('\\', '/');
        }

        return uri;
    }
}
