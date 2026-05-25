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

            foreach (var parameter in type.GenericParameters
                .OrderBy(parameter => parameter.Index)
                .ThenBy(parameter => parameter.Name, StringComparer.Ordinal))
            {
                lines.Add($"  generic {FormatGenericParameter(parameter)}");
            }

            AppendAttributes(lines, "  ", type.CustomAttributes);

            if (type.IsEnum)
            {
                if (!string.IsNullOrWhiteSpace(type.EnumUnderlyingTypeName))
                {
                    lines.Add($"  enum underlying {type.EnumUnderlyingTypeName}");
                }

                foreach (var member in type.EnumMembers.OrderBy(member => member, StringComparer.Ordinal))
                {
                    var value = type.EnumMemberValues.TryGetValue(member, out var literalValue) && !string.IsNullOrWhiteSpace(literalValue)
                        ? $" = {literalValue}"
                        : string.Empty;
                    lines.Add($"  enum member {member}{value}");
                    var field = type.Fields.FirstOrDefault(field => string.Equals(field.Name, member, StringComparison.Ordinal));
                    AppendAttributes(lines, "    ", field?.CustomAttributes ?? []);
                }
            }

            foreach (var constructor in type.Constructors
                .OrderBy(constructor => FormatParameters(constructor.Parameters), StringComparer.Ordinal))
            {
                lines.Add($"  constructor {type.Name}({FormatParameters(constructor.Parameters)})");
                AppendAttributes(lines, "    ", constructor.CustomAttributes);
            }

            foreach (var property in type.Properties.OrderBy(property => property.Name, StringComparer.Ordinal))
            {
                lines.Add($"  property {FormatPropertyModifiers(property)}{property.Type} {property.Name}");
                AppendAttributes(lines, "    ", property.CustomAttributes);
            }

            foreach (var field in type.Fields
                .Where(_ => !type.IsEnum)
                .OrderBy(field => field.Name, StringComparer.Ordinal))
            {
                lines.Add($"  field {FormatFieldModifiers(field)}{field.Type} {field.Name}");
                AppendAttributes(lines, "    ", field.CustomAttributes);
            }

            foreach (var eventSymbol in type.Events.OrderBy(eventSymbol => eventSymbol.Name, StringComparer.Ordinal))
            {
                lines.Add($"  event {FormatEventModifiers(eventSymbol)}{eventSymbol.Type} {eventSymbol.Name}");
                AppendAttributes(lines, "    ", eventSymbol.CustomAttributes);
            }

            foreach (var method in type.Methods
                .OrderBy(method => method.Name, StringComparer.Ordinal)
                .ThenBy(method => FormatParameters(method.Parameters), StringComparer.Ordinal))
            {
                lines.Add($"  method {FormatMethodModifiers(method)}{method.ReturnType} {method.Name}({FormatParameters(method.Parameters)})");
                AppendAttributes(lines, "    ", method.CustomAttributes);
                foreach (var parameter in method.GenericParameters
                    .OrderBy(parameter => parameter.Index)
                    .ThenBy(parameter => parameter.Name, StringComparer.Ordinal))
                {
                    lines.Add($"    generic {FormatGenericParameter(parameter)}");
                }
            }
        }

        return new PublicAbiSnapshot(lines);
    }

    private static void AppendAttributes(List<string> lines, string indent, IReadOnlyList<string> attributes)
    {
        foreach (var attribute in attributes.OrderBy(attribute => attribute, StringComparer.Ordinal))
        {
            lines.Add($"{indent}attribute {attribute}");
        }
    }

    private static string FormatPropertyModifiers(MetadataPropertySymbol property)
    {
        var modifiers = new List<string>();
        if (property.IsStatic)
        {
            modifiers.Add("static");
        }

        if (property.HasPublicGetter)
        {
            modifiers.Add("get");
        }

        if (property.HasPublicSetter)
        {
            modifiers.Add("set");
        }

        return modifiers.Count == 0 ? string.Empty : $"{string.Join(" ", modifiers)} ";
    }

    private static string FormatFieldModifiers(MetadataFieldSymbol field)
    {
        var modifiers = new List<string>();
        if (field.IsStatic)
        {
            modifiers.Add("static");
        }

        if (field.IsLiteral)
        {
            modifiers.Add("literal");
        }

        if (field.IsReadOnly)
        {
            modifiers.Add("readonly");
        }

        return modifiers.Count == 0 ? string.Empty : $"{string.Join(" ", modifiers)} ";
    }

    private static string FormatEventModifiers(MetadataEventSymbol eventSymbol)
    {
        var modifiers = new List<string>();
        if (eventSymbol.IsStatic)
        {
            modifiers.Add("static");
        }

        if (eventSymbol.HasPublicAdder)
        {
            modifiers.Add("add");
        }

        if (eventSymbol.HasPublicRemover)
        {
            modifiers.Add("remove");
        }

        return modifiers.Count == 0 ? string.Empty : $"{string.Join(" ", modifiers)} ";
    }

    private static string FormatMethodModifiers(MetadataMethodSymbol method)
    {
        var modifiers = new List<string>();
        if (method.IsStatic)
        {
            modifiers.Add("static");
        }

        if (method.IsExtension)
        {
            modifiers.Add("extension");
        }

        return modifiers.Count == 0 ? string.Empty : $"{string.Join(" ", modifiers)} ";
    }

    private static string FormatParameters(IReadOnlyList<MetadataParameterSymbol> parameters) =>
        string.Join(", ", parameters.Select(FormatParameter));

    private static string FormatGenericParameter(MetadataGenericParameterSymbol parameter)
    {
        var constraints = new List<string>();
        if (parameter.HasReferenceTypeConstraint)
        {
            constraints.Add("class");
        }

        if (parameter.HasNotNullableValueTypeConstraint)
        {
            constraints.Add("struct");
        }

        constraints.AddRange(parameter.TypeConstraints.OrderBy(constraint => constraint, StringComparer.Ordinal));

        if (parameter.HasDefaultConstructorConstraint)
        {
            constraints.Add("new()");
        }

        return constraints.Count == 0
            ? parameter.Name
            : $"{parameter.Name} : {string.Join(", ", constraints)}";
    }

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
