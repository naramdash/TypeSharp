using System;

namespace TypeSharp.Runtime
{
    public interface ITypeSharpUnionCase
    {
        int Tag { get; }

        string CaseName { get; }

        bool HasPayload { get; }

        object Payload { get; }
    }

    public static class TypeSharpUnion
    {
        public static bool IsCase(object value, int tag)
        {
            var unionCase = value as ITypeSharpUnionCase;
            return unionCase != null && unionCase.Tag == tag;
        }

        public static int GetTag(object value)
        {
            return RequireUnionCase(value).Tag;
        }

        public static string GetCaseName(object value)
        {
            return RequireUnionCase(value).CaseName;
        }

        public static bool HasPayload(object value)
        {
            return RequireUnionCase(value).HasPayload;
        }

        public static object GetPayload(object value)
        {
            var unionCase = RequireUnionCase(value);
            if (!unionCase.HasPayload)
            {
                throw new InvalidOperationException("Union case has no payload.");
            }

            return unionCase.Payload;
        }

        public static T GetPayload<T>(object value)
        {
            return (T)GetPayload(value);
        }

        public static bool SameCase(object left, object right)
        {
            var leftCase = RequireUnionCase(left, "left");
            var rightCase = RequireUnionCase(right, "right");
            return leftCase.Tag == rightCase.Tag
                && string.Equals(leftCase.CaseName, rightCase.CaseName, StringComparison.Ordinal);
        }

        public static bool PayloadEquals(object left, object right)
        {
            var leftCase = RequireUnionCase(left, "left");
            var rightCase = RequireUnionCase(right, "right");
            if (leftCase.HasPayload != rightCase.HasPayload)
            {
                return false;
            }

            if (!leftCase.HasPayload)
            {
                return true;
            }

            return object.Equals(leftCase.Payload, rightCase.Payload);
        }

        public static int CombineHash(int tag, object payload)
        {
            unchecked
            {
                return (tag * 397) ^ (payload == null ? 0 : payload.GetHashCode());
            }
        }

        private static ITypeSharpUnionCase RequireUnionCase(object value)
        {
            return RequireUnionCase(value, "value");
        }

        private static ITypeSharpUnionCase RequireUnionCase(object value, string parameterName)
        {
            var unionCase = value as ITypeSharpUnionCase;
            if (unionCase == null)
            {
                throw new ArgumentException("Value is not a TypeSharp union case.", parameterName);
            }

            return unionCase;
        }
    }
}
