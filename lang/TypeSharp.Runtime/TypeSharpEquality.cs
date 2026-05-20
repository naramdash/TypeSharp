using System.Collections.Generic;

namespace TypeSharp.Runtime
{
    public static class TypeSharpEquality
    {
        public static bool AreEqual(object left, object right)
        {
            return object.Equals(left, right);
        }

        public static int GetHash(object value)
        {
            return value == null ? 0 : value.GetHashCode();
        }

        public static int CombineHash(int leftHash, int rightHash)
        {
            unchecked
            {
                return (leftHash * 397) ^ rightHash;
            }
        }

        public static int CombineHash(params object[] values)
        {
            var hash = 17;
            foreach (var value in values)
            {
                hash = CombineHash(hash, GetHash(value));
            }

            return hash;
        }

        public static bool SequenceEqual<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            var comparer = EqualityComparer<T>.Default;
            using (var leftEnumerator = left.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                while (true)
                {
                    var leftHasValue = leftEnumerator.MoveNext();
                    var rightHasValue = rightEnumerator.MoveNext();
                    if (leftHasValue != rightHasValue)
                    {
                        return false;
                    }

                    if (!leftHasValue)
                    {
                        return true;
                    }

                    if (!comparer.Equals(leftEnumerator.Current, rightEnumerator.Current))
                    {
                        return false;
                    }
                }
            }
        }
    }
}
