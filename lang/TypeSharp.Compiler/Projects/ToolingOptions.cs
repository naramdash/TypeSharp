namespace TypeSharp.Compiler.Projects;

public sealed record ToolingOptions(
    string DiagnosticFormat,
    bool TreatWarningsAsErrors)
{
    public static ToolingOptions Default { get; } = new(
        "text",
        TreatWarningsAsErrors: false);
}
