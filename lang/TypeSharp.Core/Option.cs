using System;

namespace TypeSharp.Core
{
    public abstract class Option<T>
    {
        private Option()
        {
        }

        public abstract bool IsSome { get; }

        public bool IsNone
        {
            get { return !IsSome; }
        }

        public abstract T Value { get; }

        public static Option<T> Some(T value)
        {
            return new SomeCase(value);
        }

        public static Option<T> None
        {
            get { return NoneCase.Instance; }
        }

        public sealed class SomeCase : Option<T>
        {
            internal SomeCase(T value)
            {
                Value = value;
            }

            public override bool IsSome
            {
                get { return true; }
            }

            public override T Value { get; }
        }

        public sealed class NoneCase : Option<T>
        {
            internal static readonly NoneCase Instance = new NoneCase();

            private NoneCase()
            {
            }

            public override bool IsSome
            {
                get { return false; }
            }

            public override T Value
            {
                get { throw new InvalidOperationException("Option has no value."); }
            }
        }
    }
}
