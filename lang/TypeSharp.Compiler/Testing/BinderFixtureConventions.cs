namespace TypeSharp.Compiler.Testing;

public static class BinderFixtureConventions
{
    public const string Root = "test/fixtures/diagnostics/binder";
    public const string PositiveRoot = $"{Root}/positive";
    public const string NegativeRoot = $"{Root}/negative";
    public const string InputFileName = "input.tysh";
    public const string ExpectedDiagnosticsFileName = "expected.diagnostics.json";
}
