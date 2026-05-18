namespace TypeSharp.Compiler.Diagnostics;

public readonly record struct SourceSpan(SourcePosition Start, SourcePosition End)
{
    public override string ToString() => $"{Start}-{End}";
}
