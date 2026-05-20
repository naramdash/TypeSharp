namespace TypeSharp.Compiler.Interop;

public sealed record CSharpOverloadCandidate(
    MetadataTypeSymbol Type,
    MetadataMethodSymbol Method);
