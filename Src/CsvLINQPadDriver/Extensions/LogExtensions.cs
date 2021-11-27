using System;

namespace CsvLINQPadDriver.Extensions
{
    internal static class LogExtensions
    {
        internal static void WriteToLog(this string additionalInfo, bool write, Exception? exception = null)
        {
            if (write)
            {
                CsvDataContextDriver.WriteToLog(additionalInfo, exception);
            }
        }
    }
}
