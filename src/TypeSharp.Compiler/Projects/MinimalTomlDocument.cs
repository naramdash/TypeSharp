using TypeSharp.Compiler.Diagnostics;

namespace TypeSharp.Compiler.Projects;

internal sealed class MinimalTomlDocument
{
    private readonly string _path;
    private readonly Dictionary<string, Dictionary<string, TomlEntry>> _sections;

    private MinimalTomlDocument(string path, Dictionary<string, Dictionary<string, TomlEntry>> sections)
    {
        _path = path;
        _sections = sections;
    }

    public static MinimalTomlDocument Parse(string path, string[] lines, List<Diagnostic> diagnostics)
    {
        var sections = new Dictionary<string, Dictionary<string, TomlEntry>>(StringComparer.Ordinal);
        var currentSection = string.Empty;
        sections[currentSection] = new Dictionary<string, TomlEntry>(StringComparer.Ordinal);

        for (var index = 0; index < lines.Length; index++)
        {
            var lineNumber = index + 1;
            var line = StripComment(lines[index]).Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (line.StartsWith('['))
            {
                if (!line.EndsWith(']') || line.Count(character => character == '[') != 1 || line.Count(character => character == ']') != 1)
                {
                    diagnostics.Add(DiagnosticFactory.Manifest(
                        DiagnosticDescriptors.InvalidManifestSyntax,
                        "Invalid manifest section header.",
                        path,
                        lineNumber,
                        1));
                    continue;
                }

                currentSection = line[1..^1].Trim();
                if (currentSection.Length == 0)
                {
                    diagnostics.Add(DiagnosticFactory.Manifest(
                        DiagnosticDescriptors.InvalidManifestSyntax,
                        "Manifest section name cannot be empty.",
                        path,
                        lineNumber,
                        1));
                    continue;
                }

                if (!sections.ContainsKey(currentSection))
                {
                    sections[currentSection] = new Dictionary<string, TomlEntry>(StringComparer.Ordinal);
                }

                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0)
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestSyntax,
                    "Expected manifest key/value assignment.",
                    path,
                    lineNumber,
                    1));
                continue;
            }

            var key = line[..equalsIndex].Trim();
            var value = line[(equalsIndex + 1)..].Trim();
            if (key.Length == 0)
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.InvalidManifestSyntax,
                    "Manifest key cannot be empty.",
                    path,
                    lineNumber,
                    1));
                continue;
            }

            if (value.StartsWith('[') && !ValueClosesArray(value))
            {
                var arrayLines = new List<string> { value };
                while (++index < lines.Length)
                {
                    var continued = StripComment(lines[index]).Trim();
                    arrayLines.Add(continued);
                    if (ValueClosesArray(continued))
                    {
                        break;
                    }
                }

                value = string.Join(" ", arrayLines);
                if (!ValueClosesArray(value))
                {
                    diagnostics.Add(DiagnosticFactory.Manifest(
                        DiagnosticDescriptors.InvalidManifestSyntax,
                        $"Array value for '{key}' is not closed.",
                        path,
                        lineNumber,
                        equalsIndex + 2));
                    continue;
                }
            }

            sections[currentSection][key] = new TomlEntry(value, lineNumber, equalsIndex + 2);
        }

        return new MinimalTomlDocument(path, sections);
    }

    public string GetString(string section, string key, string defaultValue, List<Diagnostic> diagnostics)
    {
        if (!TryGetEntry(section, key, out var entry))
        {
            return defaultValue;
        }

        if (TryParseString(entry.Value, out var value))
        {
            return value;
        }

        diagnostics.Add(InvalidValue(section, key, "string", entry));
        return defaultValue;
    }

    public string? GetOptionalString(string section, string key, List<Diagnostic> diagnostics)
    {
        if (!TryGetEntry(section, key, out var entry))
        {
            return null;
        }

        if (TryParseString(entry.Value, out var value))
        {
            return value;
        }

        diagnostics.Add(InvalidValue(section, key, "string", entry));
        return null;
    }

    public bool GetBool(string section, string key, bool defaultValue, List<Diagnostic> diagnostics)
    {
        if (!TryGetEntry(section, key, out var entry))
        {
            return defaultValue;
        }

        if (bool.TryParse(entry.Value, out var value))
        {
            return value;
        }

        diagnostics.Add(InvalidValue(section, key, "bool", entry));
        return defaultValue;
    }

    public IReadOnlyList<string> GetStringArray(
        string section,
        string key,
        IReadOnlyList<string> defaultValue,
        List<Diagnostic> diagnostics)
    {
        if (!TryGetEntry(section, key, out var entry))
        {
            return defaultValue;
        }

        if (TryParseStringArray(entry.Value, out var values))
        {
            return values;
        }

        diagnostics.Add(InvalidValue(section, key, "string array", entry));
        return defaultValue;
    }

    private static bool ValueClosesArray(string value)
    {
        var inString = false;
        var escaped = false;

        foreach (var character in value)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\' && inString)
            {
                escaped = true;
                continue;
            }

            if (character == '"')
            {
                inString = !inString;
                continue;
            }

            if (character == ']' && !inString)
            {
                return true;
            }
        }

        return false;
    }

    private static string StripComment(string line)
    {
        var inString = false;
        var escaped = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (character == '\\' && inString)
            {
                escaped = true;
                continue;
            }

            if (character == '"')
            {
                inString = !inString;
                continue;
            }

            if (character == '#' && !inString)
            {
                return line[..index];
            }
        }

        return line;
    }

    private bool TryGetEntry(string section, string key, out TomlEntry entry)
    {
        entry = default;
        return _sections.TryGetValue(section, out var values) && values.TryGetValue(key, out entry);
    }

    private Diagnostic InvalidValue(string section, string key, string expected, TomlEntry entry) =>
        DiagnosticFactory.Manifest(
            DiagnosticDescriptors.InvalidManifestValue,
            $"Manifest value '{section}.{key}' must be a {expected}.",
            _path,
            entry.Line,
            entry.Column);

    private static bool TryParseString(string value, out string result)
    {
        result = string.Empty;
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '"' || trimmed[^1] != '"')
        {
            return false;
        }

        result = UnescapeString(trimmed[1..^1]);
        return true;
    }

    private static bool TryParseStringArray(string value, out IReadOnlyList<string> result)
    {
        result = [];
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '[' || trimmed[^1] != ']')
        {
            return false;
        }

        var values = new List<string>();
        var index = 1;
        while (index < trimmed.Length - 1)
        {
            SkipWhitespaceAndComma(trimmed, ref index);
            if (index >= trimmed.Length - 1)
            {
                break;
            }

            if (trimmed[index] != '"')
            {
                return false;
            }

            index++;
            var start = index;
            var escaped = false;
            var closed = false;
            while (index < trimmed.Length - 1)
            {
                var character = trimmed[index];
                if (escaped)
                {
                    escaped = false;
                    index++;
                    continue;
                }

                if (character == '\\')
                {
                    escaped = true;
                    index++;
                    continue;
                }

                if (character == '"')
                {
                    values.Add(UnescapeString(trimmed[start..index]));
                    index++;
                    closed = true;
                    break;
                }

                index++;
            }

            if (!closed)
            {
                return false;
            }

            SkipWhitespace(trimmed, ref index);
            if (index < trimmed.Length - 1 && trimmed[index] != ',')
            {
                return false;
            }
        }

        result = values;
        return true;
    }

    private static void SkipWhitespaceAndComma(string value, ref int index)
    {
        while (index < value.Length && (char.IsWhiteSpace(value[index]) || value[index] == ','))
        {
            index++;
        }
    }

    private static void SkipWhitespace(string value, ref int index)
    {
        while (index < value.Length && char.IsWhiteSpace(value[index]))
        {
            index++;
        }
    }

    private static string UnescapeString(string text) =>
        text
            .Replace("\\\"", "\"", StringComparison.Ordinal)
            .Replace("\\\\", "\\", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal)
            .Replace("\\r", "\r", StringComparison.Ordinal)
            .Replace("\\t", "\t", StringComparison.Ordinal);

    private readonly record struct TomlEntry(string Value, int Line, int Column);
}
