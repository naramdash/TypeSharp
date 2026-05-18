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

                case "textDocument/hover":
                    if (hasId)
                    {
                        WriteHoverResponse(root, output, idElement, openDocuments, workspaceRoot);
                    }
                    break;

                case "textDocument/definition":
                    if (hasId)
                    {
                        WriteDefinitionResponse(root, output, idElement, openDocuments, workspaceRoot);
                    }
                    break;

                case "textDocument/completion":
                    if (hasId)
                    {
                        WriteCompletionResponse(root, output, idElement, openDocuments, workspaceRoot);
                    }
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
            writer.WriteBoolean("hoverProvider", true);
            writer.WriteBoolean("definitionProvider", true);
            writer.WritePropertyName("completionProvider");
            writer.WriteStartObject();
            writer.WriteBoolean("resolveProvider", false);
            writer.WritePropertyName("triggerCharacters");
            writer.WriteStartArray();
            writer.WriteStringValue(".");
            writer.WriteStringValue(":");
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static void WriteHoverResponse(
        JsonElement root,
        Stream output,
        JsonElement id,
        IReadOnlyDictionary<string, string> openDocuments,
        string workspaceRoot)
    {
        if (!TryGetTextDocumentPosition(root, out var uri, out var position)
            || !openDocuments.TryGetValue(uri, out var text))
        {
            WriteNullResponse(output, id);
            return;
        }

        var fileName = ToFileName(uri, workspaceRoot);
        var hover = TypeSharpDocumentHover.GetHover(text, fileName, position);
        if (hover is null)
        {
            WriteNullResponse(output, id);
            return;
        }

        WriteHoverResult(output, id, hover);
    }

    private static void WriteDefinitionResponse(
        JsonElement root,
        Stream output,
        JsonElement id,
        IReadOnlyDictionary<string, string> openDocuments,
        string workspaceRoot)
    {
        if (!TryGetTextDocumentPosition(root, out var uri, out var position)
            || !openDocuments.TryGetValue(uri, out var text))
        {
            WriteNullResponse(output, id);
            return;
        }

        var fileName = ToFileName(uri, workspaceRoot);
        var location = TypeSharpDocumentDefinition.GetDefinition(text, fileName, uri, position);
        if (location is null)
        {
            WriteNullResponse(output, id);
            return;
        }

        WriteLocationResult(output, id, location);
    }

    private static void WriteCompletionResponse(
        JsonElement root,
        Stream output,
        JsonElement id,
        IReadOnlyDictionary<string, string> openDocuments,
        string workspaceRoot)
    {
        if (!TryGetTextDocumentPosition(root, out var uri, out var position)
            || !openDocuments.TryGetValue(uri, out var text))
        {
            WriteNullResponse(output, id);
            return;
        }

        var fileName = ToFileName(uri, workspaceRoot);
        var items = TypeSharpDocumentCompletion.GetCompletions(text, fileName, position);
        WriteCompletionResult(output, id, items);
    }

    private static bool TryGetTextDocumentPosition(JsonElement root, out string uri, out LspPosition position)
    {
        uri = string.Empty;
        position = new LspPosition(0, 0);

        if (!root.TryGetProperty("params", out var parameters)
            || !parameters.TryGetProperty("textDocument", out var textDocument)
            || !textDocument.TryGetProperty("uri", out var uriElement)
            || uriElement.GetString() is not { Length: > 0 } documentUri
            || !parameters.TryGetProperty("position", out var positionElement)
            || !positionElement.TryGetProperty("line", out var lineElement)
            || !positionElement.TryGetProperty("character", out var characterElement)
            || !lineElement.TryGetInt32(out var line)
            || !characterElement.TryGetInt32(out var character))
        {
            return false;
        }

        uri = documentUri;
        position = new LspPosition(line, character);
        return true;
    }

    private static void WriteHoverResult(Stream output, JsonElement id, LspHover hover)
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
            writer.WritePropertyName("contents");
            writer.WriteStartObject();
            writer.WriteString("kind", hover.Contents.Kind);
            writer.WriteString("value", hover.Contents.Value);
            writer.WriteEndObject();
            if (hover.Range is not null)
            {
                writer.WritePropertyName("range");
                WriteRange(writer, hover.Range);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static void WriteCompletionResult(
        Stream output,
        JsonElement id,
        IReadOnlyList<LspCompletionItem> items)
    {
        using var payload = new MemoryStream();
        using (var writer = new Utf8JsonWriter(payload))
        {
            writer.WriteStartObject();
            writer.WriteString("jsonrpc", "2.0");
            writer.WritePropertyName("id");
            id.WriteTo(writer);
            writer.WritePropertyName("result");
            writer.WriteStartArray();
            foreach (var item in items)
            {
                writer.WriteStartObject();
                writer.WriteString("label", item.Label);
                writer.WriteNumber("kind", item.Kind);
                writer.WriteString("detail", item.Detail);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        WriteFramedJson(output, payload.ToArray());
    }

    private static void WriteLocationResult(Stream output, JsonElement id, LspLocation location)
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
            writer.WriteString("uri", location.Uri);
            writer.WritePropertyName("range");
            WriteRange(writer, location.Range);
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
                WriteRange(writer, diagnostic.Range);
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

    private static void WriteRange(Utf8JsonWriter writer, LspRange range)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("start");
        writer.WriteStartObject();
        writer.WriteNumber("line", range.Start.Line);
        writer.WriteNumber("character", range.Start.Character);
        writer.WriteEndObject();
        writer.WritePropertyName("end");
        writer.WriteStartObject();
        writer.WriteNumber("line", range.End.Line);
        writer.WriteNumber("character", range.End.Character);
        writer.WriteEndObject();
        writer.WriteEndObject();
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
