namespace TypeSharp.Compiler.Testing;

public static class TypeCheckerFixtureConventions
{
    public const string Root = "test/fixtures/diagnostics/type-checker";
    public const string PositiveRoot = $"{Root}/positive";
    public const string NegativeRoot = $"{Root}/negative";
    public const string InputFileName = "input.tysh";
    public const string ExpectedDiagnosticsFileName = "expected.diagnostics.json";
}
