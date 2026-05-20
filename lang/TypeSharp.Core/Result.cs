using System;

namespace TypeSharp.Core
{
    public abstract class Result<T, E>
    {
        private Result()
        {
        }

        public abstract bool IsOk { get; }

        public bool IsError
        {
            get { return !IsOk; }
        }

        public abstract T Value { get; }

        public abstract E ErrorValue { get; }

        public static Result<T, E> Ok(T value)
        {
            return new OkCase(value);
        }

        public static Result<T, E> Error(E error)
        {
            return new ErrorCase(error);
        }

        public sealed class OkCase : Result<T, E>
        {
            internal OkCase(T value)
            {
                Value = value;
            }

            public override bool IsOk
            {
                get { return true; }
            }

            public override T Value { get; }

            public override E ErrorValue
            {
                get { throw new InvalidOperationException("Result has no error."); }
            }
        }

        public sealed class ErrorCase : Result<T, E>
        {
            internal ErrorCase(E error)
            {
                ErrorValue = error;
            }

            public override bool IsOk
            {
                get { return false; }
            }

            public override T Value
            {
                get { throw new InvalidOperationException("Result has no value."); }
            }

            public override E ErrorValue { get; }
        }
    }
}
