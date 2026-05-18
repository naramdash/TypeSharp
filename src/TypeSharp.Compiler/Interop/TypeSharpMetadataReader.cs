using TypeSharp.Compiler.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace TypeSharp.Compiler.Interop;

public static class TypeSharpMetadataReader
{
    public static MetadataReadResult Read(ReferenceResolutionResult resolutionResult)
    {
        var diagnostics = new List<Diagnostic>(resolutionResult.Diagnostics);
        var assemblies = ReadAssemblies(resolutionResult.References, diagnostics);
        return new MetadataReadResult(assemblies, diagnostics);
    }

    public static MetadataReadResult Read(IEnumerable<ResolvedReference> references)
    {
        var diagnostics = new List<Diagnostic>();
        var assemblies = ReadAssemblies(references, diagnostics);
        return new MetadataReadResult(assemblies, diagnostics);
    }

    private static List<MetadataAssemblySymbol> ReadAssemblies(
        IEnumerable<ResolvedReference> references,
        List<Diagnostic> diagnostics)
    {
        var assemblies = new List<MetadataAssemblySymbol>();

        foreach (var reference in references)
        {
            if (reference.Kind == ResolvedReferenceKind.LocalAssembly && !CanReadLocalReference(reference, diagnostics))
            {
                continue;
            }

            var types = reference.Kind == ResolvedReferenceKind.LocalAssembly
                ? ReadLocalPublicTypes(reference, diagnostics)
                : [];

            assemblies.Add(new MetadataAssemblySymbol(
                reference.Identity,
                reference.Kind,
                reference.OriginalText,
                reference.Path,
                reference.RelativePath)
            {
                Types = types
            });
        }

        return assemblies;
    }

    private static IReadOnlyList<MetadataTypeSymbol> ReadLocalPublicTypes(
        ResolvedReference reference,
        List<Diagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(reference.Path))
        {
            return [];
        }

        var types = new List<MetadataTypeSymbol>();

        try
        {
            using var stream = File.Open(reference.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var peReader = new PEReader(stream);
            if (!peReader.HasMetadata)
            {
                diagnostics.Add(DiagnosticFactory.Manifest(
                    DiagnosticDescriptors.MissingReference,
                    $"Referenced assembly path '{reference.RelativePath ?? reference.OriginalText}' does not contain readable metadata.",
                    reference.Path));
                return [];
            }

            var reader = peReader.GetMetadataReader();
            var assemblyHasNullableMetadata = HasNullableMetadata(reader, reader.GetAssemblyDefinition().GetCustomAttributes());
            foreach (var handle in reader.TypeDefinitions)
            {
                var type = reader.GetTypeDefinition(handle);
                if (!IsPublicTopLevelType(type.Attributes))
                {
                    continue;
                }

                types.Add(new MetadataTypeSymbol(
                    reader.GetString(type.Namespace),
                    reader.GetString(type.Name),
                    ReadPublicMethods(reader, type, assemblyHasNullableMetadata),
                    ReadPublicProperties(reader, type)));
            }
        }
        catch (BadImageFormatException)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.MissingReference,
                $"Referenced assembly path '{reference.RelativePath ?? reference.OriginalText}' does not contain readable metadata.",
                reference.Path));
        }

        return types;
    }

    private static IReadOnlyList<MetadataMethodSymbol> ReadPublicMethods(
        MetadataReader reader,
        TypeDefinition type,
        bool assemblyHasNullableMetadata)
    {
        var methods = new List<MetadataMethodSymbol>();
        foreach (var handle in type.GetMethods())
        {
            var method = reader.GetMethodDefinition(handle);
            if (!method.Attributes.HasFlag(System.Reflection.MethodAttributes.Public) ||
                method.Attributes.HasFlag(System.Reflection.MethodAttributes.SpecialName))
            {
                continue;
            }

            methods.Add(ReadMethod(reader, type, method, assemblyHasNullableMetadata));
        }

        return methods;
    }

    private static MetadataMethodSymbol ReadMethod(
        MetadataReader reader,
        TypeDefinition declaringType,
        MethodDefinition method,
        bool assemblyHasNullableMetadata)
    {
        var provider = new SimpleSignatureTypeProvider();
        var signature = method.DecodeSignature(provider, genericContext: null);
        var parameterDefinitions = method.GetParameters()
            .Select(handle => reader.GetParameter(handle))
            .ToDictionary(parameter => parameter.SequenceNumber);

        var parameters = new List<MetadataParameterSymbol>();
        for (var index = 0; index < signature.ParameterTypes.Length; index++)
        {
            var sequence = index + 1;
            parameterDefinitions.TryGetValue(sequence, out var parameter);
            var type = signature.ParameterTypes[index];
            parameters.Add(new MetadataParameterSymbol(
                parameter.Name.IsNil ? string.Empty : reader.GetString(parameter.Name),
                type.Name,
                GetByRefKind(type, parameter.Attributes),
                HasCustomAttribute(reader, parameter.GetCustomAttributes(), "System.ParamArrayAttribute"),
                IsOptionalWithDefault(parameter)));
        }

        return new MetadataMethodSymbol(
            reader.GetString(method.Name),
            signature.ReturnType.Name,
            GetReturnNullability(reader, declaringType, method, signature.ReturnType, parameterDefinitions, assemblyHasNullableMetadata),
            parameters);
    }

    private static IReadOnlyList<MetadataPropertySymbol> ReadPublicProperties(MetadataReader reader, TypeDefinition type)
    {
        var properties = new List<MetadataPropertySymbol>();
        foreach (var handle in type.GetProperties())
        {
            var property = reader.GetPropertyDefinition(handle);
            var accessors = property.GetAccessors();
            if (IsPublicAccessor(reader, accessors.Getter) || IsPublicAccessor(reader, accessors.Setter))
            {
                properties.Add(new MetadataPropertySymbol(reader.GetString(property.Name)));
            }
        }

        return properties;
    }

    private static bool IsPublicAccessor(MetadataReader reader, MethodDefinitionHandle handle)
    {
        if (handle.IsNil)
        {
            return false;
        }

        return reader.GetMethodDefinition(handle).Attributes.HasFlag(System.Reflection.MethodAttributes.Public);
    }

    private static bool IsPublicTopLevelType(System.Reflection.TypeAttributes attributes) =>
        attributes.HasFlag(System.Reflection.TypeAttributes.Public);

    private static bool HasCustomAttribute(
        MetadataReader reader,
        CustomAttributeHandleCollection attributes,
        string fullName)
    {
        foreach (var handle in attributes)
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (string.Equals(GetAttributeTypeName(reader, attribute.Constructor), fullName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasNullableMetadata(MetadataReader reader, CustomAttributeHandleCollection attributes) =>
        HasCustomAttribute(reader, attributes, "System.Runtime.CompilerServices.NullableAttribute") ||
        HasCustomAttribute(reader, attributes, "System.Runtime.CompilerServices.NullableContextAttribute");

    private static MetadataNullabilityKind GetReturnNullability(
        MetadataReader reader,
        TypeDefinition type,
        MethodDefinition method,
        DecodedType returnType,
        IReadOnlyDictionary<int, Parameter> parameters,
        bool assemblyHasNullableMetadata)
    {
        if (!IsReferenceType(returnType.Name))
        {
            return MetadataNullabilityKind.NotApplicable;
        }

        var hasNullableMetadata = assemblyHasNullableMetadata ||
            HasNullableMetadata(reader, type.GetCustomAttributes()) ||
            HasNullableMetadata(reader, method.GetCustomAttributes()) ||
            parameters.TryGetValue(0, out var returnParameter) &&
            HasNullableMetadata(reader, returnParameter.GetCustomAttributes());

        return hasNullableMetadata
            ? MetadataNullabilityKind.Annotated
            : MetadataNullabilityKind.Unknown;
    }

    private static bool IsReferenceType(string typeName)
    {
        if (typeName.EndsWith("[]", StringComparison.Ordinal))
        {
            return true;
        }

        return typeName is "string" or "object" ||
            typeName.Contains('.', StringComparison.Ordinal) ||
            typeName.Contains('<', StringComparison.Ordinal);
    }

    private static string GetAttributeTypeName(MetadataReader reader, EntityHandle constructor)
    {
        return constructor.Kind switch
        {
            HandleKind.MethodDefinition => GetTypeDefinitionName(reader, reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType()),
            HandleKind.MemberReference => GetMemberReferenceParentName(reader, reader.GetMemberReference((MemberReferenceHandle)constructor).Parent),
            _ => string.Empty
        };
    }

    private static string GetMemberReferenceParentName(MetadataReader reader, EntityHandle parent)
    {
        return parent.Kind switch
        {
            HandleKind.TypeReference => GetTypeReferenceName(reader, (TypeReferenceHandle)parent),
            HandleKind.TypeDefinition => GetTypeDefinitionName(reader, (TypeDefinitionHandle)parent),
            _ => string.Empty
        };
    }

    private static string GetTypeDefinitionName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        return QualifiedName(reader, type.Namespace, type.Name);
    }

    private static string GetTypeReferenceName(MetadataReader reader, TypeReferenceHandle handle)
    {
        var type = reader.GetTypeReference(handle);
        return QualifiedName(reader, type.Namespace, type.Name);
    }

    private static string QualifiedName(MetadataReader reader, StringHandle namespaceHandle, StringHandle nameHandle)
    {
        var namespaceName = namespaceHandle.IsNil ? string.Empty : reader.GetString(namespaceHandle);
        var name = reader.GetString(nameHandle);
        return namespaceName.Length == 0 ? name : $"{namespaceName}.{name}";
    }

    private static MetadataByRefKind GetByRefKind(DecodedType type, System.Reflection.ParameterAttributes attributes)
    {
        if (!type.IsByRef)
        {
            return MetadataByRefKind.None;
        }

        if (attributes.HasFlag(System.Reflection.ParameterAttributes.Out))
        {
            return MetadataByRefKind.Out;
        }

        return attributes.HasFlag(System.Reflection.ParameterAttributes.In)
            ? MetadataByRefKind.In
            : MetadataByRefKind.Ref;
    }

    private static bool IsOptionalWithDefault(Parameter parameter) =>
        parameter.Attributes.HasFlag(System.Reflection.ParameterAttributes.Optional) &&
        !parameter.GetDefaultValue().IsNil;

    private static bool CanReadLocalReference(ResolvedReference reference, List<Diagnostic> diagnostics)
    {
        var displayPath = reference.RelativePath ?? reference.OriginalText;
        var diagnosticFile = reference.Path ?? displayPath;

        if (string.IsNullOrWhiteSpace(reference.Path) || !File.Exists(reference.Path))
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.MissingReference,
                $"Referenced assembly path '{displayPath}' does not exist.",
                diagnosticFile));
            return false;
        }

        try
        {
            using var stream = File.Open(reference.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(DiagnosticFactory.Manifest(
                DiagnosticDescriptors.MissingReference,
                $"Referenced assembly path '{displayPath}' cannot be read.",
                diagnosticFile));
            return false;
        }
    }

    private readonly record struct DecodedType(string Name, bool IsByRef)
    {
        public DecodedType WithByRef() => this with { IsByRef = true };
    }

    private sealed class SimpleSignatureTypeProvider : ISignatureTypeProvider<DecodedType, object?>
    {
        public DecodedType GetArrayType(DecodedType elementType, ArrayShape shape) => new($"{elementType.Name}[]", IsByRef: false);

        public DecodedType GetByReferenceType(DecodedType elementType) => elementType.WithByRef();

        public DecodedType GetFunctionPointerType(MethodSignature<DecodedType> signature) => new("function*", IsByRef: false);

        public DecodedType GetGenericInstantiation(DecodedType genericType, System.Collections.Immutable.ImmutableArray<DecodedType> typeArguments) =>
            new($"{genericType.Name}<{string.Join(", ", typeArguments.Select(argument => argument.Name))}>", IsByRef: false);

        public DecodedType GetGenericMethodParameter(object? genericContext, int index) => new($"!!{index}", IsByRef: false);

        public DecodedType GetGenericTypeParameter(object? genericContext, int index) => new($"!{index}", IsByRef: false);

        public DecodedType GetModifiedType(DecodedType modifier, DecodedType unmodifiedType, bool isRequired) => unmodifiedType;

        public DecodedType GetPinnedType(DecodedType elementType) => elementType;

        public DecodedType GetPointerType(DecodedType elementType) => new($"{elementType.Name}*", IsByRef: false);

        public DecodedType GetPrimitiveType(PrimitiveTypeCode typeCode) =>
            new(typeCode switch
            {
                PrimitiveTypeCode.Boolean => "bool",
                PrimitiveTypeCode.Byte => "byte",
                PrimitiveTypeCode.Char => "char",
                PrimitiveTypeCode.Double => "double",
                PrimitiveTypeCode.Int16 => "short",
                PrimitiveTypeCode.Int32 => "int",
                PrimitiveTypeCode.Int64 => "long",
                PrimitiveTypeCode.Object => "object",
                PrimitiveTypeCode.SByte => "sbyte",
                PrimitiveTypeCode.Single => "float",
                PrimitiveTypeCode.String => "string",
                PrimitiveTypeCode.UInt16 => "ushort",
                PrimitiveTypeCode.UInt32 => "uint",
                PrimitiveTypeCode.UInt64 => "ulong",
                PrimitiveTypeCode.Void => "void",
                _ => typeCode.ToString()
            }, IsByRef: false);

        public DecodedType GetSZArrayType(DecodedType elementType) => new($"{elementType.Name}[]", IsByRef: false);

        public DecodedType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var type = reader.GetTypeDefinition(handle);
            return new(QualifiedName(reader, type.Namespace, type.Name), IsByRef: false);
        }

        public DecodedType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var type = reader.GetTypeReference(handle);
            return new(QualifiedName(reader, type.Namespace, type.Name), IsByRef: false);
        }

        public DecodedType GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) =>
            reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

        private static string QualifiedName(MetadataReader reader, StringHandle namespaceHandle, StringHandle nameHandle)
        {
            var namespaceName = namespaceHandle.IsNil ? string.Empty : reader.GetString(namespaceHandle);
            var name = reader.GetString(nameHandle);
            return namespaceName.Length == 0 ? name : $"{namespaceName}.{name}";
        }
    }
}
