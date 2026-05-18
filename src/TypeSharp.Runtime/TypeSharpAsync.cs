using System;
using System.Threading.Tasks;

namespace TypeSharp.Runtime
{
    public static class TypeSharpAsync
    {
        public static Task Completed()
        {
            return Task.FromResult(0);
        }

        public static Task<T> FromResult<T>(T value)
        {
            return Task.FromResult(value);
        }

        public static Task FromException(Exception exception)
        {
            var source = new TaskCompletionSource<int>();
            source.SetException(exception);
            return source.Task;
        }

        public static Task<T> FromException<T>(Exception exception)
        {
            var source = new TaskCompletionSource<T>();
            source.SetException(exception);
            return source.Task;
        }
    }
}
