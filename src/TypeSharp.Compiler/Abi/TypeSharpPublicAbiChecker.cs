using TypeSharp.Compiler.Interop;

namespace TypeSharp.Compiler.Abi;

public static class TypeSharpPublicAbiChecker
{
    public static PublicAbiSnapshot CreateSnapshot(MetadataAssemblySymbol assembly)
    {
        var lines = new List<string>
        {
            $"assembly {assembly.Identity}"
        };

        foreach (var type in assembly.Types.OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            lines.Add($"type {type.FullName}");

            foreach (var property in type.Properties.OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                lines.Add($"  property {property.Type} {property.Name}");
            }

            foreach (var method in type.Methods
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .ThenBy(method => FormatParameters(method.Parameters), StringComparer.Ordinal))
            {
                lines.Add($"  method {method.ReturnType} {method.Name}({FormatParameters(method.Parameters)})");
            }
        }

        return new PublicAbiSnapshot(lines);
    }

    private static string FormatParameters(IReadOnlyList<MetadataParameterSymbol> parameters) =>
        string.Join(", ", parameters.Select(FormatParameter));

    private static string FormatParameter(MetadataParameterSymbol parameter)
    {
        var modifier = parameter.ByRefKind switch
        {
            MetadataByRefKind.Ref => "ref ",
            MetadataByRefKind.Out => "out ",
            MetadataByRefKind.In => "in ",
            _ => string.Empty
        };
        var paramsPrefix = parameter.IsParams ? "params " : string.Empty;
        var optionalSuffix = parameter.IsOptional ? " = optional" : string.Empty;
        return $"{paramsPrefix}{modifier}{parameter.Type} {parameter.Name}{optionalSuffix}";
    }
}
