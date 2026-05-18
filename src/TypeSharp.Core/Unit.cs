using System;

namespace TypeSharp.Core
{
    public struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Value = new Unit();

        public bool Equals(Unit other)
        {
            return true;
        }

#pragma warning disable CS8765
        public override bool Equals(object obj)
        {
            return obj is Unit;
        }
#pragma warning restore CS8765

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}
