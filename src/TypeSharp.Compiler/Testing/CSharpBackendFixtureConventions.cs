namespace TypeSharp.Compiler.Testing;

public static class CSharpBackendFixtureConventions
{
    public const string Root = "tests/fixtures/backend/csharp";
    public const string PositiveRoot = $"{Root}/positive";
    public const string InputFileName = "input.tysh";
    public const string ExpectedCSharpFileName = "expected.cs";
}
