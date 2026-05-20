namespace TypeSharp.Compiler.TypeChecking;

internal interface ITypeSharpInferenceScope
{
    bool ResolveValue(string name, out SimpleType type);

    bool ResolveFunction(string name, out SimpleType returnType);

    bool ResolveType(string name);
}
