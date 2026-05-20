namespace TypeSharp.Compiler.Testing;

public static class ParserFixtureConventions
{
    public const string Root = "test/fixtures/parser";
    public const string PositiveRoot = $"{Root}/positive";
    public const string NegativeRoot = $"{Root}/negative";
    public const string InputFileName = "input.tysh";
    public const string ExpectedDiagnosticsFileName = "expected.diagnostics.json";
    public const string ExpectedTreeFileName = "expected.tree";
}
