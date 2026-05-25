using System.Globalization;
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

            var metadataReference = reference.Kind == ResolvedReferenceKind.FrameworkAssembly
                ? ResolveFrameworkMetadataReference(reference)
                : reference;

            var types = !string.IsNullOrWhiteSpace(metadataReference.Path)
                ? ReadPublicTypes(metadataReference, diagnostics)
                : [];

            assemblies.Add(new MetadataAssemblySymbol(
                reference.Identity,
                reference.Kind,
                reference.OriginalText,
                metadataReference.Path,
                reference.RelativePath)
            {
                Types = types
            });
        }

        return assemblies;
    }

    private static IReadOnlyList<MetadataTypeSymbol> ReadPublicTypes(
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

                var baseTypeName = GetMetadataTypeName(reader, type.BaseType);
                var fields = ReadPublicFields(reader, type);
                var isEnum = string.Equals(baseTypeName, "System.Enum", StringComparison.Ordinal);
                types.Add(new MetadataTypeSymbol(
                    reader.GetString(type.Namespace),
                    reader.GetString(type.Name),
                    ReadPublicMethods(reader, type, assemblyHasNullableMetadata),
                    ReadPublicProperties(reader, type),
                    fields,
                    ReadPublicEvents(reader, type))
                {
                    BaseTypeName = baseTypeName,
                    Constructors = ReadPublicConstructors(reader, type, assemblyHasNullableMetadata),
                    Operators = ReadPublicOperators(reader, type, assemblyHasNullableMetadata),
                    GenericParameters = ReadGenericParameters(reader, type.GetGenericParameters()),
                    CustomAttributes = ReadPublicAbiCustomAttributeNames(reader, type.GetCustomAttributes()),
                    HasPublicParameterlessConstructor = HasPublicParameterlessConstructor(reader, type),
                    InterfaceNames = ReadInterfaceNames(reader, type),
                    IsInterface = type.Attributes.HasFlag(System.Reflection.TypeAttributes.Interface),
                    IsEnum = isEnum,
                    EnumMembers = isEnum ? GetEnumMemberNames(fields) : [],
                    EnumUnderlyingTypeName = isEnum ? GetEnumUnderlyingTypeName(fields) : null,
                    EnumMemberValues = isEnum ? GetEnumMemberValues(fields) : new Dictionary<string, string>(StringComparer.Ordinal),
                    IsValueType = string.Equals(baseTypeName, "System.ValueType", StringComparison.Ordinal) ||
                        string.Equals(baseTypeName, "System.Enum", StringComparison.Ordinal)
                });
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

    private static IReadOnlyList<MetadataMethodSymbol> ReadPublicConstructors(
        MetadataReader reader,
        TypeDefinition type,
        bool assemblyHasNullableMetadata)
    {
        var constructors = new List<MetadataMethodSymbol>();
        foreach (var handle in type.GetMethods())
        {
            var method = reader.GetMethodDefinition(handle);
            if (!method.Attributes.HasFlag(System.Reflection.MethodAttributes.Public) ||
                method.Attributes.HasFlag(System.Reflection.MethodAttributes.Static) ||
                !method.Attributes.HasFlag(System.Reflection.MethodAttributes.SpecialName) ||
                !string.Equals(reader.GetString(method.Name), ".ctor", StringComparison.Ordinal))
            {
                continue;
            }

            constructors.Add(ReadMethod(reader, type, method, assemblyHasNullableMetadata));
        }

        return constructors;
    }

    private static ResolvedReference ResolveFrameworkMetadataReference(ResolvedReference reference)
    {
        var assemblyFileName = reference.Identity.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? reference.Identity
            : $"{reference.Identity}.dll";

        foreach (var candidateRoot in GetFrameworkReferenceAssemblyRoots())
        {
            var candidatePath = Path.Combine(candidateRoot, assemblyFileName);
            if (File.Exists(candidatePath))
            {
                return reference with { Path = candidatePath };
            }
        }

        return reference;
    }

    private static IEnumerable<string> GetFrameworkReferenceAssemblyRoots()
    {
        foreach (var programFiles in new[]
        {
            Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetEnvironmentVariable("ProgramFiles"),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
        })
        {
            if (string.IsNullOrWhiteSpace(programFiles))
            {
                continue;
            }

            yield return Path.Combine(
                programFiles,
                "Reference Assemblies",
                "Microsoft",
                "Framework",
                ".NETFramework",
                "v4.8");
        }
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

    private static IReadOnlyList<MetadataMethodSymbol> ReadPublicOperators(
        MetadataReader reader,
        TypeDefinition type,
        bool assemblyHasNullableMetadata)
    {
        var operators = new List<MetadataMethodSymbol>();
        foreach (var handle in type.GetMethods())
        {
            var method = reader.GetMethodDefinition(handle);
            if (!method.Attributes.HasFlag(System.Reflection.MethodAttributes.Public) ||
                !method.Attributes.HasFlag(System.Reflection.MethodAttributes.Static) ||
                !method.Attributes.HasFlag(System.Reflection.MethodAttributes.SpecialName))
            {
                continue;
            }

            var methodName = reader.GetString(method.Name);
            if (!IsSupportedMultiplicativeOperatorMethodName(methodName))
            {
                continue;
            }

            var symbol = ReadMethod(reader, type, method, assemblyHasNullableMetadata);
            if (symbol.Parameters.Count == 2 &&
                symbol.GenericParameterCount == 0 &&
                !string.Equals(symbol.ReturnType, "void", StringComparison.Ordinal))
            {
                operators.Add(symbol);
            }
        }

        return operators;
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

        var genericParameters = ReadGenericParameters(reader, method.GetGenericParameters());
        return new MetadataMethodSymbol(
            reader.GetString(method.Name),
            signature.ReturnType.Name,
            GetReturnNullability(reader, declaringType, method, signature.ReturnType, parameterDefinitions, assemblyHasNullableMetadata),
            parameters,
            method.Attributes.HasFlag(System.Reflection.MethodAttributes.Static),
            genericParameters.Count,
            HasCustomAttribute(reader, method.GetCustomAttributes(), "System.Runtime.CompilerServices.ExtensionAttribute"))
        {
            GenericParameters = genericParameters,
            CustomAttributes = ReadPublicAbiCustomAttributeNames(reader, method.GetCustomAttributes())
        };
    }

    private static IReadOnlyList<MetadataGenericParameterSymbol> ReadGenericParameters(
        MetadataReader reader,
        GenericParameterHandleCollection handles)
    {
        var parameters = new List<MetadataGenericParameterSymbol>();
        foreach (var handle in handles)
        {
            var parameter = reader.GetGenericParameter(handle);
            var attributes = parameter.Attributes;
            var constraints = parameter.GetConstraints()
                .Select(constraintHandle => reader.GetGenericParameterConstraint(constraintHandle))
                .Select(constraint => GetMetadataTypeName(reader, constraint.Type))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToArray();

            parameters.Add(new MetadataGenericParameterSymbol(
                parameter.Index,
                reader.GetString(parameter.Name),
                attributes.HasFlag(System.Reflection.GenericParameterAttributes.ReferenceTypeConstraint),
                attributes.HasFlag(System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint),
                attributes.HasFlag(System.Reflection.GenericParameterAttributes.DefaultConstructorConstraint),
                constraints));
        }

        return parameters;
    }

    private static IReadOnlyList<MetadataPropertySymbol> ReadPublicProperties(MetadataReader reader, TypeDefinition type)
    {
        var properties = new List<MetadataPropertySymbol>();
        foreach (var handle in type.GetProperties())
        {
            var property = reader.GetPropertyDefinition(handle);
            var accessors = property.GetAccessors();
            var hasPublicGetter = IsPublicAccessor(reader, accessors.Getter);
            var hasPublicSetter = IsPublicAccessor(reader, accessors.Setter);
            if (hasPublicGetter || hasPublicSetter)
            {
                var provider = new SimpleSignatureTypeProvider();
                var signature = property.DecodeSignature(provider, genericContext: null);
                properties.Add(new MetadataPropertySymbol(
                    reader.GetString(property.Name),
                    signature.ReturnType.Name,
                    IsStaticAccessor(reader, accessors.Getter) || IsStaticAccessor(reader, accessors.Setter),
                    hasPublicGetter,
                    hasPublicSetter,
                    signature.ParameterTypes.Length > 0,
                    signature.ParameterTypes.Length)
                {
                    ParameterTypes = signature.ParameterTypes.Select(type => type.Name).ToArray(),
                    CustomAttributes = ReadPublicAbiCustomAttributeNames(reader, property.GetCustomAttributes())
                });
            }
        }

        return properties;
    }

    private static IReadOnlyList<MetadataFieldSymbol> ReadPublicFields(MetadataReader reader, TypeDefinition type)
    {
        var fields = new List<MetadataFieldSymbol>();
        foreach (var handle in type.GetFields())
        {
            var field = reader.GetFieldDefinition(handle);
            if (!field.Attributes.HasFlag(System.Reflection.FieldAttributes.Public))
            {
                continue;
            }

            var provider = new SimpleSignatureTypeProvider();
            var signature = field.DecodeSignature(provider, genericContext: null);
            fields.Add(new MetadataFieldSymbol(
                reader.GetString(field.Name),
                signature.Name,
                field.Attributes.HasFlag(System.Reflection.FieldAttributes.Static),
                field.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal),
                field.Attributes.HasFlag(System.Reflection.FieldAttributes.InitOnly))
            {
                LiteralValue = ReadLiteralValue(reader, field),
                CustomAttributes = ReadPublicAbiCustomAttributeNames(reader, field.GetCustomAttributes())
            });
        }

        return fields;
    }

    private static IReadOnlyList<string> GetEnumMemberNames(IReadOnlyList<MetadataFieldSymbol> fields) =>
        fields
            .Where(field =>
                field.IsStatic &&
                field.IsLiteral &&
                !string.Equals(field.Name, "value__", StringComparison.Ordinal))
            .Select(field => field.Name)
            .ToArray();

    private static string? GetEnumUnderlyingTypeName(IReadOnlyList<MetadataFieldSymbol> fields) =>
        fields
            .FirstOrDefault(field => string.Equals(field.Name, "value__", StringComparison.Ordinal))
            ?.Type;

    private static IReadOnlyDictionary<string, string> GetEnumMemberValues(IReadOnlyList<MetadataFieldSymbol> fields) =>
        fields
            .Where(field =>
                field.IsStatic &&
                field.IsLiteral &&
                field.LiteralValue is { Length: > 0 } &&
                !string.Equals(field.Name, "value__", StringComparison.Ordinal))
            .ToDictionary(field => field.Name, field => field.LiteralValue!, StringComparer.Ordinal);

    private static string? ReadLiteralValue(MetadataReader reader, FieldDefinition field)
    {
        var constantHandle = field.GetDefaultValue();
        if (constantHandle.IsNil)
        {
            return null;
        }

        var constant = reader.GetConstant(constantHandle);
        var value = reader.GetBlobReader(constant.Value);
        return constant.TypeCode switch
        {
            ConstantTypeCode.Boolean => value.ReadBoolean().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.Char => ((int)value.ReadUInt16()).ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.SByte => value.ReadSByte().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.Byte => value.ReadByte().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.Int16 => value.ReadInt16().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.UInt16 => value.ReadUInt16().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.Int32 => value.ReadInt32().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.UInt32 => value.ReadUInt32().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.Int64 => value.ReadInt64().ToString(CultureInfo.InvariantCulture),
            ConstantTypeCode.UInt64 => value.ReadUInt64().ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    }

    private static IReadOnlyList<MetadataEventSymbol> ReadPublicEvents(MetadataReader reader, TypeDefinition type)
    {
        var events = new List<MetadataEventSymbol>();
        foreach (var handle in type.GetEvents())
        {
            var eventDefinition = reader.GetEventDefinition(handle);
            var accessors = eventDefinition.GetAccessors();
            var hasPublicAdder = IsPublicAccessor(reader, accessors.Adder);
            var hasPublicRemover = IsPublicAccessor(reader, accessors.Remover);
            if (hasPublicAdder || hasPublicRemover)
            {
                events.Add(new MetadataEventSymbol(
                    reader.GetString(eventDefinition.Name),
                    GetEventTypeName(reader, eventDefinition.Type),
                    IsStaticAccessor(reader, accessors.Adder) || IsStaticAccessor(reader, accessors.Remover),
                    hasPublicAdder,
                    hasPublicRemover)
                {
                    CustomAttributes = ReadPublicAbiCustomAttributeNames(reader, eventDefinition.GetCustomAttributes())
                });
            }
        }

        return events;
    }

    private static bool IsPublicAccessor(MetadataReader reader, MethodDefinitionHandle handle)
    {
        if (handle.IsNil)
        {
            return false;
        }

        return reader.GetMethodDefinition(handle).Attributes.HasFlag(System.Reflection.MethodAttributes.Public);
    }

    private static bool IsStaticAccessor(MetadataReader reader, MethodDefinitionHandle handle)
    {
        if (handle.IsNil)
        {
            return false;
        }

        return reader.GetMethodDefinition(handle).Attributes.HasFlag(System.Reflection.MethodAttributes.Static);
    }

    private static bool IsPublicTopLevelType(System.Reflection.TypeAttributes attributes) =>
        attributes.HasFlag(System.Reflection.TypeAttributes.Public);

    private static bool IsSupportedMultiplicativeOperatorMethodName(string methodName) =>
        methodName is "op_Multiply" or "op_Division" or "op_Modulus";

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

    private static IReadOnlyList<string> ReadPublicAbiCustomAttributeNames(
        MetadataReader reader,
        CustomAttributeHandleCollection attributes)
    {
        var names = new List<string>();
        foreach (var handle in attributes)
        {
            var attribute = reader.GetCustomAttribute(handle);
            var name = GetAttributeTypeName(reader, attribute.Constructor);
            if (IsPublicAbiCustomAttributeName(name))
            {
                names.Add(name);
            }
        }

        return names
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsPublicAbiCustomAttributeName(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        !name.StartsWith("System.Runtime.CompilerServices.", StringComparison.Ordinal) &&
        !string.Equals(name, "System.ParamArrayAttribute", StringComparison.Ordinal);

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

    private static string GetEventTypeName(MetadataReader reader, EntityHandle handle)
    {
        var provider = new SimpleSignatureTypeProvider();
        return handle.Kind switch
        {
            HandleKind.TypeDefinition => GetTypeDefinitionName(reader, (TypeDefinitionHandle)handle),
            HandleKind.TypeReference => GetTypeReferenceName(reader, (TypeReferenceHandle)handle),
            HandleKind.TypeSpecification => reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(provider, genericContext: null).Name,
            _ => string.Empty
        };
    }

    private static string? GetMetadataTypeName(MetadataReader reader, EntityHandle handle)
    {
        if (handle.IsNil)
        {
            return null;
        }

        var provider = new SimpleSignatureTypeProvider();
        return handle.Kind switch
        {
            HandleKind.TypeDefinition => GetTypeDefinitionName(reader, (TypeDefinitionHandle)handle),
            HandleKind.TypeReference => GetTypeReferenceName(reader, (TypeReferenceHandle)handle),
            HandleKind.TypeSpecification => reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(provider, genericContext: null).Name,
            _ => null
        };
    }

    private static IReadOnlyList<string> ReadInterfaceNames(MetadataReader reader, TypeDefinition type)
    {
        var names = new List<string>();
        foreach (var handle in type.GetInterfaceImplementations())
        {
            var implementation = reader.GetInterfaceImplementation(handle);
            var name = GetMetadataTypeName(reader, implementation.Interface);
            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

    private static bool HasPublicParameterlessConstructor(MetadataReader reader, TypeDefinition type)
    {
        foreach (var handle in type.GetMethods())
        {
            var method = reader.GetMethodDefinition(handle);
            if (!method.Attributes.HasFlag(System.Reflection.MethodAttributes.Public) ||
                !method.Attributes.HasFlag(System.Reflection.MethodAttributes.SpecialName) ||
                !string.Equals(reader.GetString(method.Name), ".ctor", StringComparison.Ordinal))
            {
                continue;
            }

            var provider = new SimpleSignatureTypeProvider();
            var signature = method.DecodeSignature(provider, genericContext: null);
            if (signature.ParameterTypes.Length == 0)
            {
                return true;
            }
        }

        return false;
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
