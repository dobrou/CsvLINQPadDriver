using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

using static LPRun.Context;
using static LPRun.LPRunException;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LPRun
{
    /// <summary>
    /// Provides method for executing the LINQPad script.
    /// </summary>
    public static class Runner
    {
        private static readonly string[] IgnoredErrorMessages =
        {
            "Downloading package",
            "Downloading NuGet package",
            "Restoring package"
        };

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RetryTimeout   = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// The LINQPad script execution result.
        /// </summary>
        /// <param name="Output">The LINQPad script execution captured output stream.</param>
        /// <param name="Error">The LINQPad script execution captured error stream.</param>
        /// <param name="ExitCode">The LINQPad script execution exit code.</param>
        public record Result(string Output, string Error, int ExitCode)
        {
            /// <summary>
            /// Indicates that operation completed successfully.
            /// </summary>
            public bool Success => string.IsNullOrWhiteSpace(Error) && ExitCode == 0;
        }

        /// <summary>
        /// The number of times to retry the operation on error and timeout between tries.
        /// </summary>
        /// <param name="Times">The number of times to retry the operation.</param>
        /// <param name="Timeout">The timeout between tries.</param>
        public record RetryOnError(int? Times = null, TimeSpan? Timeout = null);

        /// <summary>
        /// Executes LINQPad script using LPRun with optional timeout specified.
        /// </summary>
        /// <param name="linqFile">The LINQPad script file to execute.</param>
        /// <param name="waitForExit">The LINQPad script execution timeout. 1 minute is the default.</param>
        /// <param name="retryOnError">The number of times to retry the operation on error and timeout between tries.</param>
        /// <returns>The LINQPad script execution <see cref="Result"/>.</returns>
        /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static Result Execute(string linqFile, TimeSpan? waitForExit = null, RetryOnError? retryOnError = null)
        {
           return Retry(() => Wrap(ExecuteInternal));

           Result Retry(Func<Result> func)
           {
                var times   = retryOnError?.Times   ?? 1;
                var timeout = retryOnError?.Timeout ?? RetryTimeout;

                while (true)
                {
                    var result = func();

                    if (result.Success || --times <= 0)
                    {
                        return result;
                    }

                    Thread.Sleep(timeout);
                }
           }

            Result ExecuteInternal()
            {
                waitForExit ??= DefaultTimeout;

                var output = new StringBuilder();
                var error  = new StringBuilder();

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo(Exe, GetArguments())
                    {
                        UseShellExecute        = false,
                        CreateNoWindow         = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true
                    }
                };

                process.OutputDataReceived += OutputDataReceivedHandler;
                process.ErrorDataReceived  += ErrorDataReceivedHandler;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var completed = process.WaitForExit((int)waitForExit?.TotalMilliseconds!);

                process.CancelOutputRead();
                process.CancelErrorRead();

                process.OutputDataReceived -= OutputDataReceivedHandler;
                process.ErrorDataReceived  -= ErrorDataReceivedHandler;

                if (completed)
                {
                    return new Result(output.ToString(), error.ToString(), process.ExitCode);
                }

                process.Kill();

                throw new TimeoutException($"LPRun timed out after {waitForExit}");

                string GetArguments() =>
                    $@"-fx={FrameworkInfo.Version.Major}.{FrameworkInfo.Version.Minor} ""{GetFullPath(linqFile)}""";

                void OutputDataReceivedHandler(object _, DataReceivedEventArgs e) =>
                    output.Append(e.Data);

                void ErrorDataReceivedHandler(object _, DataReceivedEventArgs e)
                {
                    if (Array.TrueForAll(IgnoredErrorMessages, message => e.Data?.StartsWith(message) == false))
                    {
                        error.Append(e.Data);
                    }
                }
            }
        }
    }
}
