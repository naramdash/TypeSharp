namespace TypeSharp.Compiler.Backend;

public sealed record TypeSharpBackendArtifact
{
    private TypeSharpBackendArtifact(
        TypeSharpBackendArtifactKind kind,
        string extension,
        string? text,
        byte[]? bytes)
    {
        Kind = kind;
        Extension = extension;
        Text = text;
        Bytes = bytes ?? [];
    }

    public TypeSharpBackendArtifactKind Kind { get; }

    public string Extension { get; }

    public string? Text { get; }

    public byte[] Bytes { get; }

    public static TypeSharpBackendArtifact SourceText(string extension, string text) =>
        new(TypeSharpBackendArtifactKind.SourceText, extension, text, null);

    public static TypeSharpBackendArtifact Assembly(string extension, byte[] bytes) =>
        new(TypeSharpBackendArtifactKind.Assembly, extension, null, bytes.ToArray());

    public string RequireText() =>
        Text ?? throw new InvalidOperationException($"Backend artifact '{Kind}' does not contain source text.");

    public byte[] RequireBytes()
    {
        if (Kind != TypeSharpBackendArtifactKind.Assembly)
        {
            throw new InvalidOperationException($"Backend artifact '{Kind}' does not contain assembly bytes.");
        }

        return Bytes.ToArray();
    }
}
