using System;

namespace TypeSharp.Runtime
{
    public static class TypeSharpPattern
    {
        public static bool IsCase(object value, int tag)
        {
            return TypeSharpUnion.IsCase(value, tag);
        }

        public static bool IsPayloadCase(object value, int tag)
        {
            return TypeSharpUnion.IsCase(value, tag) && TypeSharpUnion.HasPayload(value);
        }

        public static bool IsPayloadlessCase(object value, int tag)
        {
            return TypeSharpUnion.IsCase(value, tag) && !TypeSharpUnion.HasPayload(value);
        }

        public static object RequirePayload(object value, int tag)
        {
            if (!IsPayloadCase(value, tag))
            {
                throw NoMatch(value);
            }

            return TypeSharpUnion.GetPayload(value);
        }

        public static T RequirePayload<T>(object value, int tag)
        {
            return (T)RequirePayload(value, tag);
        }

        public static InvalidOperationException NoMatch(object value)
        {
            var unionCase = value as ITypeSharpUnionCase;
            if (unionCase == null)
            {
                return new InvalidOperationException("No pattern matched value.");
            }

            return new InvalidOperationException(
                "No pattern matched union case '" + unionCase.CaseName + "' with tag " + unionCase.Tag + ".");
        }
    }
}
