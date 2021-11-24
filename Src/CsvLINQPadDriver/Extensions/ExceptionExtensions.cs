using System;
using System.Threading;

namespace CsvLINQPadDriver.Extensions
{
    internal static class ExceptionExtensions
    {
        public static bool CanBeHandled(this Exception exception) =>
            exception is not (
                NullReferenceException   or
                ArgumentException        or
                IndexOutOfRangeException or
                OutOfMemoryException     or
                AccessViolationException or
                ThreadAbortException     or
                StackOverflowException
            );
    }
}
