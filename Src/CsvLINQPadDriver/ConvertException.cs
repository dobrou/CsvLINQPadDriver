using System;
using System.Runtime.Serialization;

namespace CsvLINQPadDriver
{
    [Serializable]
    public class ConvertException : Exception
    {
        private ConvertException(string type, string? value, Exception? innerException)
            : base($@"Failed to convert ""{value}"" to {type}.", innerException)
        {
        }

        protected ConvertException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal static ConvertException Create(string type, string? value, Exception? innerException) =>
            new(type, value, innerException);

        internal static ConvertException Create(string type, ReadOnlySpan<char> value, Exception? innerException) =>
            new(type, value.ToString(), innerException);
    }
}
