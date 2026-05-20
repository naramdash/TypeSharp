namespace TypeSharp.Compiler.Diagnostics;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ManifestNotFound = new(
        "TS0100",
        "TypeSharp manifest not found",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Could not find TypeSharp manifest.",
        "The compiler needs a TypeSharp.toml manifest to discover project settings and source files.",
        "Pass a manifest path or run the command from a directory that contains TypeSharp.toml.");

    public static readonly DiagnosticDescriptor ManifestUnreadable = new(
        "TS0101",
        "TypeSharp manifest cannot be read",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Could not read TypeSharp manifest.",
        "The manifest exists, but the compiler cannot read it because of an IO or permission failure.",
        "Check the file path and permissions, then run the command again.");

    public static readonly DiagnosticDescriptor InvalidManifestSyntax = new(
        "TS0102",
        "Invalid manifest syntax",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Invalid TypeSharp manifest syntax.",
        "The manifest parser found a section header, key/value assignment, or array value that does not match the supported TypeSharp.toml subset.",
        "Fix the manifest syntax at the reported span.");

    public static readonly DiagnosticDescriptor InvalidManifestValue = new(
        "TS0103",
        "Invalid manifest value",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Manifest value is invalid.",
        "A manifest key exists, but its value has the wrong type or is outside the supported value domain.",
        "Change the value to a supported manifest value.");

    public static readonly DiagnosticDescriptor SourceRootNotFound = new(
        "TS0110",
        "Source root not found",
        DiagnosticSeverity.Warning,
        DiagnosticCategory.Project,
        "Source root does not exist.",
        "A configured source root was not found, so no .tysh files can be discovered from that root.",
        "Create the source root or remove it from TypeSharp.toml.");

    public static readonly DiagnosticDescriptor DuplicateSourceModulePath = new(
        "TS0111",
        "Duplicate source module path",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Duplicate source module path.",
        "Source files are part of the project module graph by their source-root-relative module path. Two files with the same module path would make imports and public surface ownership ambiguous.",
        "Rename or move one source file, or adjust sourceRoots so each module path is unique.");

    public static readonly DiagnosticDescriptor UnresolvedSourceModule = new(
        "TS0112",
        "Unresolved source module",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Source module specifier could not be resolved.",
        "A relative TypeSharp source module specifier must resolve to a discovered source-root-relative module path before the project module graph is well-formed.",
        "Create the target source file, fix the relative specifier, or move the file under a configured source root.");

    public static readonly DiagnosticDescriptor UnsupportedSourceModuleImport = new(
        "TS0113",
        "Unsupported source module import",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "This source module import form is not supported by the current generated C# backend.",
        "The compiler can lower simple relative named imports through static module-container usings, relative named function import aliases through generated forwarding methods, relative named top-level value import aliases through generated properties, relative named type and module import aliases through C# using aliases, and namespace imports through module-container aliases, but other future source import forms may still need project-wide binding support.",
        "Use an unaliased relative named import, a relative named function, top-level value, type, or module import alias, a relative namespace import, or keep the declaration in the same source file until this import form is implemented.");

    public static readonly DiagnosticDescriptor MissingSourceModuleExport = new(
        "TS0114",
        "Missing source module export",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Project,
        "Source module export could not be found.",
        "A relative source named import can only import declarations that the target TypeSharp source module exports.",
        "Export the target declaration, add it to a local export list, or change the import to an exported name.");

    public static readonly DiagnosticDescriptor UnexpectedCharacter = new(
        "TS1000",
        "Unexpected character",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Parser,
        "Unexpected character.",
        "The lexer found a character that is not part of the stable TypeSharp lexical grammar.",
        "Remove the character or replace it with supported TypeSharp syntax.");

    public static readonly DiagnosticDescriptor MissingFunctionBody = new(
        "TS1001",
        "Missing function body",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Parser,
        "Expected function body after function signature.",
        "A function declaration must have either an expression body, a block body, or be marked extern.",
        "Add '= expression', add a block body, or mark the declaration as extern if it is imported.");

    public static readonly DiagnosticDescriptor UnterminatedStringLiteral = new(
        "TS1002",
        "Unterminated string literal",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Parser,
        "Unterminated string literal.",
        "The lexer reached the end of the file before finding the closing quote for a string literal.",
        "Add the closing quote or escape embedded quote characters.");

    public static readonly DiagnosticDescriptor MissingExpression = new(
        "TS1003",
        "Missing expression",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Parser,
        "Expected expression.",
        "The parser reached a token that cannot start an expression in the current context.",
        "Add an expression before the reported token.");

    public static readonly DiagnosticDescriptor UnexpectedToken = new(
        "TS1004",
        "Unexpected token",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Parser,
        "Unexpected token.",
        "The parser expected one token kind but found another while building the syntax tree.",
        "Adjust the syntax near the reported token.");

    public static readonly DiagnosticDescriptor UnresolvedName = new(
        "TS2001",
        "Unresolved name",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Binding,
        "Unresolved name.",
        "Name resolution could not find a matching value, type, import, parameter, or local binding in scope.",
        "Declare the symbol, import it explicitly, or qualify the reference.");

    public static readonly DiagnosticDescriptor DuplicateSymbol = new(
        "TS2002",
        "Duplicate symbol",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Binding,
        "Duplicate symbol.",
        "A declaration introduced a value or type name that already exists in the same scope.",
        "Rename one declaration or move it into a nested scope.");

    public static readonly DiagnosticDescriptor UnsupportedExportForwarding = new(
        "TS2003",
        "Unsupported export forwarding",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Binding,
        "This export specifier form is parsed but not implemented yet.",
        "Export specifier declarations can describe local public surface or forward public surface across files. The current compiler supports unaliased local export lists, local named function export aliases, local literal export aliases, local top-level value export aliases, explicitly annotated function-valued top-level let exports and aliases, local type export aliases, relative named function and top-level value re-exports, relative type-only re-exports, and relative star re-exports over the lowerable source module surface. Unannotated lambda-valued export let, non-relative forwarding, and unsupported non-lowerable forwarding are not lowered yet.",
        "Use direct export modifiers, unaliased local export lists, local function aliases, local literal aliases, local top-level value aliases, explicitly annotated function-valued let exports or aliases, local type aliases, relative named function or top-level value re-exports, relative type-only re-exports, or relative star re-exports; remove unsupported export specifier forms from build inputs until they are implemented.");

    public static readonly DiagnosticDescriptor DuplicateExport = new(
        "TS2004",
        "Duplicate export",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Binding,
        "Duplicate export.",
        "A local export list exported the same public name more than once.",
        "Remove the duplicate export specifier or consolidate the exported name into a single local export list entry.");

    public static readonly DiagnosticDescriptor TypeMismatch = new(
        "TS2201",
        "Type mismatch",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Type mismatch.",
        "A value expression has a known type that is not assignable to the explicit annotation or return type.",
        "Change the expression, change the annotation, or add an explicit conversion once conversions are supported.");

    public static readonly DiagnosticDescriptor NullabilityContractViolation = new(
        "TS2202",
        "Nullability contract violation",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Nullability contract violation.",
        "A null or nullable value is flowing into a TypeSharp position that is declared non-null.",
        "Add a null guard, change the target type to nullable, or provide a non-null fallback.");

    public static readonly DiagnosticDescriptor PublicBoundaryTypeLeak = new(
        "TS2204",
        "Compile-time type leaked through public boundary",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Compile-time-only type cannot appear in public API. Use a nominal union, interface, or wrapper.",
        "Type-level unions, intersections, and structural shapes are compile-time TypeSharp types and do not have stable CLR metadata representation.",
        "Replace the public type with a nominal union, nominal interface, or wrapper type.");

    public static readonly DiagnosticDescriptor UnsupportedGenericConstraint = new(
        "TS2205",
        "Unsupported generic constraint",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Unsupported generic constraint.",
        "The current C# 7.3 backend can lower class, struct, new(), and nominal/interface type constraints, but not every design-level TypeSharp constraint.",
        "Use a C# 7.3-compatible constraint or remove the unsupported constraint until the backend supports it.");

    public static readonly DiagnosticDescriptor DynamicCapabilityRequired = new(
        "TS2206",
        "Dynamic capability required",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Dynamic type annotation requires a 'dynamic' function modifier.",
        "The dynamic type weakens static checking and must be visible at the function boundary before code can cross a late-bound .NET dynamic boundary.",
        "Add the 'dynamic' modifier to the containing function or replace the annotation with a statically checked type.");

    public static readonly DiagnosticDescriptor DynamicCallRequiresCapability = new(
        "TS2207",
        "Dynamic call requires capability",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Calling a dynamic function requires a 'dynamic' function modifier.",
        "A function marked with the dynamic capability crosses a late-bound .NET dynamic boundary, so callers must also make that boundary explicit.",
        "Add the 'dynamic' modifier to the containing function or isolate the dynamic call behind a statically checked wrapper.");

    public static readonly DiagnosticDescriptor CapabilityCallRequiresMarker = new(
        "TS2208",
        "Capability call requires marker",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Calling a capability-marked function requires the same function modifier.",
        "A function marked with reflect, interop, or unsafe crosses an explicit escape boundary, so callers must also make that boundary visible.",
        "Add the matching capability modifier to the containing function or isolate the call behind a statically checked wrapper.");

    public static readonly DiagnosticDescriptor UnknownAccessRequiresNarrowing = new(
        "TS2209",
        "Unknown access requires narrowing",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Unknown value must be narrowed before member or indexer access.",
        "The unknown type is a safe gradual-typing boundary. Code must prove a shape or narrower type before accessing members or indexers.",
        "Use a match/type pattern, assign to a structural shape after validation, or change the value to a statically known type.");

    public static readonly DiagnosticDescriptor NonExhaustiveMatch = new(
        "TS2203",
        "Non-exhaustive match",
        DiagnosticSeverity.Error,
        DiagnosticCategory.TypeChecking,
        "Non-exhaustive match.",
        "A match expression over a known nominal union must handle every declared case.",
        "Add arms for the missing union cases.");

    public static readonly DiagnosticDescriptor MissingReference = new(
        "TS2401",
        "Missing referenced assembly or namespace",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "Missing referenced assembly or namespace.",
        "The project references an external assembly or path that the compiler cannot resolve.",
        "Fix the reference path or add the referenced assembly to the project environment.");

    public static readonly DiagnosticDescriptor AmbiguousCSharpOverload = new(
        "TS2402",
        "Ambiguous C# overload or constructor",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "Ambiguous C# overload or constructor.",
        "A C# interop call matches more than one method or constructor candidate and cannot be selected safely.",
        "Add an explicit type annotation or adjust the call so exactly one C# method overload or constructor is applicable.");

    public static readonly DiagnosticDescriptor InvalidByRefInterop = new(
        "TS2403",
        "Invalid byref interop use",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "Invalid byref interop use.",
        "A C# interop call uses a ref, out, or in argument modifier that does not match the referenced method metadata.",
        "Change the call-site modifier to match the imported C# method signature.");

    public static readonly DiagnosticDescriptor UnknownCSharpNullability = new(
        "TS2404",
        "Unknown C# nullability",
        DiagnosticSeverity.Warning,
        DiagnosticCategory.Interop,
        "Unknown C# nullability.",
        "A C# interop call returns a reference type from metadata that does not contain nullable annotations.",
        "Add a null guard, use nullable-annotated metadata, or loosen nullable checking for this interop boundary.");

    public static readonly DiagnosticDescriptor UnsupportedPackageReference = new(
        "TS2405",
        "Unsupported package reference",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "NuGet package references are not supported by the current compiler.",
        "The manifest reserves references.packages for future NuGet restore and package lock support, but the MVP compiler does not restore or inspect packages.",
        "Resolve the package outside TypeSharp and reference a local net48-compatible DLL path, or remove the package reference until package restore is implemented.");

    public static readonly DiagnosticDescriptor NoMatchingCSharpOverload = new(
        "TS2406",
        "No matching C# overload or constructor",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "No C# overload or constructor matches this call.",
        "The compiler found C# metadata methods or constructors with this name, but none of the candidates match the call shape, generic type argument count, argument names, arity, or byref requirements.",
        "Adjust the arguments, generic type argument list, names, or ref/out/in modifiers so exactly one imported C# overload or constructor is applicable.");

    public static readonly DiagnosticDescriptor MissingCSharpMethod = new(
        "TS2407",
        "Missing C# method",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public static method.",
        "The compiler found a C# metadata type for the call receiver, but no public static method with the requested name is available in the referenced assemblies.",
        "Check the import, method name, referenced assembly, and whether the C# method is public and static.");

    public static readonly DiagnosticDescriptor MissingCSharpType = new(
        "TS2408",
        "Missing C# type",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# namespace does not contain this public type.",
        "The compiler found C# metadata for the imported namespace, but no public type with the requested name is available in the referenced assemblies.",
        "Check the import, type name, referenced assembly, and whether the C# type is public.");

    public static readonly DiagnosticDescriptor MissingCSharpStaticMember = new(
        "TS2409",
        "Missing C# static member",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public static member.",
        "The compiler found a C# metadata type for the member-access receiver, but no public static field, property, or method with the requested name is available in the referenced assemblies.",
        "Check the member name, referenced assembly, and whether the C# member is public and static.");

    public static readonly DiagnosticDescriptor MissingCSharpInstanceMember = new(
        "TS2410",
        "Missing C# instance member",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public instance member.",
        "The compiler tracked a local value constructed from C# metadata, but no public instance field, property, or method with the requested name is available on that imported type.",
        "Check the member name, referenced assembly, and whether the C# member is public and instance-bound.");

    public static readonly DiagnosticDescriptor MissingCSharpInstanceIndexer = new(
        "TS2411",
        "Missing or mismatched C# instance indexer",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain a compatible public instance indexer.",
        "The compiler tracked a local value constructed from C# metadata, but no public instance indexer with the requested arity or known argument type shape is available on that imported type.",
        "Check the indexed value, index argument count or type, referenced assembly, and whether the C# indexer is public and instance-bound.");

    public static readonly DiagnosticDescriptor MissingCSharpInstancePropertySetter = new(
        "TS2412",
        "Missing C# instance property setter",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public instance property setter.",
        "The compiler tracked a local value constructed from C# metadata, but the assigned property does not expose a public instance setter on that imported type.",
        "Check the property name, referenced assembly, and whether the C# property has a public instance setter.");

    public static readonly DiagnosticDescriptor ReadOnlyCSharpInstanceFieldAssignment = new(
        "TS2413",
        "Read-only C# instance field assignment",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# instance field is read-only.",
        "The compiler tracked a local value constructed from C# metadata, but the assigned field is a public readonly instance field on that imported type.",
        "Assign a mutable field, call an imported mutating API, or change the C# field if mutation is intended.");

    public static readonly DiagnosticDescriptor MissingCSharpStaticPropertySetter = new(
        "TS2414",
        "Missing C# static property setter",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public static property setter.",
        "The compiler found a C# metadata type for the assignment receiver, but the assigned static property does not expose a public setter.",
        "Check the property name, referenced assembly, and whether the C# property has a public static setter.");

    public static readonly DiagnosticDescriptor ReadOnlyCSharpStaticFieldAssignment = new(
        "TS2415",
        "Read-only C# static field assignment",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# static field is read-only.",
        "The compiler found a C# metadata type for the assignment receiver, but the assigned static field is literal or readonly.",
        "Assign a mutable static field, call an imported mutating API, or change the C# field if mutation is intended.");

    public static readonly DiagnosticDescriptor MissingCSharpInstanceEvent = new(
        "TS2416",
        "Missing C# instance event",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "C# type does not contain this public instance event.",
        "The compiler tracked a local value constructed from C# metadata, but the event add/remove target does not expose a matching public instance event accessor on that imported type.",
        "Check the event name, referenced assembly, and whether the C# event is public and instance-bound.");

    public static readonly DiagnosticDescriptor UnsatisfiedCSharpGenericConstraint = new(
        "TS2417",
        "Unsatisfied C# generic constraint",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "Explicit C# generic type argument does not satisfy the imported method constraint.",
        "The compiler selected an imported C# generic method, but at least one explicit type argument violates class, struct, new(), or nominal/interface metadata constraints.",
        "Use a type argument that satisfies the C# generic constraint or call an overload with a compatible generic shape.");

    public static readonly DiagnosticDescriptor UnsupportedExecutableEntryPoint = new(
        "TS3500",
        "Unsupported executable entry point",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Backend,
        "Unsupported executable entry point.",
        "The project is configured as an executable, but the configured main function cannot be emitted as the generated .NET Framework entry point.",
        "Use a main function with no parameters or exactly one string[] parameter.");

    public static readonly DiagnosticDescriptor GeneratedProjectBuildFailed = new(
        "TS3501",
        "Generated C# project build failed",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Backend,
        "Generated C# project build failed.",
        "The C# backend emitted source and a project scaffold, but the generated project did not compile successfully.",
        "Inspect the generated C# project output and fix the TypeSharp source or backend lowering that produced invalid C#.");

    public static readonly IReadOnlyList<DiagnosticDescriptor> All =
    [
        ManifestNotFound,
        ManifestUnreadable,
        InvalidManifestSyntax,
        InvalidManifestValue,
        SourceRootNotFound,
        DuplicateSourceModulePath,
        UnresolvedSourceModule,
        UnsupportedSourceModuleImport,
        MissingSourceModuleExport,
        UnexpectedCharacter,
        MissingFunctionBody,
        UnterminatedStringLiteral,
        MissingExpression,
        UnexpectedToken,
        UnresolvedName,
        DuplicateSymbol,
        UnsupportedExportForwarding,
        DuplicateExport,
        TypeMismatch,
        NullabilityContractViolation,
        NonExhaustiveMatch,
        PublicBoundaryTypeLeak,
        UnsupportedGenericConstraint,
        DynamicCapabilityRequired,
        DynamicCallRequiresCapability,
        CapabilityCallRequiresMarker,
        UnknownAccessRequiresNarrowing,
        MissingReference,
        AmbiguousCSharpOverload,
        InvalidByRefInterop,
        UnknownCSharpNullability,
        UnsupportedPackageReference,
        NoMatchingCSharpOverload,
        MissingCSharpMethod,
        MissingCSharpType,
        MissingCSharpStaticMember,
        MissingCSharpInstanceMember,
        MissingCSharpInstanceIndexer,
        MissingCSharpInstancePropertySetter,
        ReadOnlyCSharpInstanceFieldAssignment,
        MissingCSharpStaticPropertySetter,
        ReadOnlyCSharpStaticFieldAssignment,
        MissingCSharpInstanceEvent,
        UnsatisfiedCSharpGenericConstraint,
        UnsupportedExecutableEntryPoint,
        GeneratedProjectBuildFailed
    ];

    public static bool TryGetByCode(string code, out DiagnosticDescriptor descriptor)
    {
        foreach (var candidate in All)
        {
            if (string.Equals(candidate.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                descriptor = candidate;
                return true;
            }
        }

        descriptor = default!;
        return false;
    }
}
