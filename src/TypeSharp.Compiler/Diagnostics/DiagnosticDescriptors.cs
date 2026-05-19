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
        "Manifest value has the wrong shape.",
        "A manifest key exists, but its value is not the expected type.",
        "Change the value to the expected manifest type.");

    public static readonly DiagnosticDescriptor SourceRootNotFound = new(
        "TS0110",
        "Source root not found",
        DiagnosticSeverity.Warning,
        DiagnosticCategory.Project,
        "Source root does not exist.",
        "A configured source root was not found, so no .tysh files can be discovered from that root.",
        "Create the source root or remove it from TypeSharp.toml.");

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
        "Type-level union cannot appear in public API. Use a nominal union or interface.",
        "Type-level unions and structural shapes are compile-time TypeSharp types and do not have stable CLR metadata representation.",
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
        "Ambiguous C# overload",
        DiagnosticSeverity.Error,
        DiagnosticCategory.Interop,
        "Ambiguous C# overload.",
        "A C# interop call matches more than one overload candidate and cannot be selected safely.",
        "Add an explicit type annotation or adjust the call so exactly one C# overload is applicable.");

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
        UnexpectedCharacter,
        MissingFunctionBody,
        UnterminatedStringLiteral,
        MissingExpression,
        UnexpectedToken,
        UnresolvedName,
        DuplicateSymbol,
        TypeMismatch,
        NullabilityContractViolation,
        NonExhaustiveMatch,
        PublicBoundaryTypeLeak,
        UnsupportedGenericConstraint,
        DynamicCapabilityRequired,
        DynamicCallRequiresCapability,
        MissingReference,
        AmbiguousCSharpOverload,
        InvalidByRefInterop,
        UnknownCSharpNullability,
        UnsupportedPackageReference,
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
