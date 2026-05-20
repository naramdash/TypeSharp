namespace TypeSharp.Compiler.TypeChecking;

internal readonly record struct SimpleType(string Name, bool IsNullable, bool IsKnown, bool IsNull)
{
    public static SimpleType Unknown { get; } = new(string.Empty, IsNullable: false, IsKnown: false, IsNull: false);

    public static SimpleType Null { get; } = new("null", IsNullable: true, IsKnown: true, IsNull: true);

    public static SimpleType Named(string name) => new(name, IsNullable: false, IsKnown: true, IsNull: false);

    public SimpleType AsNullable() => this with { IsNullable = true };

    public override string ToString()
    {
        if (!IsKnown)
        {
            return "unknown";
        }

        if (IsNull)
        {
            return "null";
        }

        return IsNullable ? $"{Name}?" : Name;
    }
}
