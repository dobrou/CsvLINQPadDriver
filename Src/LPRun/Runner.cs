using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

using static LPRun.Context;
using static LPRun.LPRunException;

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

        /// <summary>
        /// The LINQPad script execution result.
        /// </summary>
        /// <param name="Output">The LINQPad script execution captured output stream.</param>
        /// <param name="Error">The LINQPad script execution captured error stream.</param>
        /// <param name="ExitCode">The LINQPad script execution exit code.</param>
        public record Result(string Output, string Error, int ExitCode);

        /// <summary>
        /// Executes LINQPad script using LPRun with optional timeout specified.
        /// </summary>
        /// <param name="linqFile">The LINQPad script file to execute.</param>
        /// <param name="waitForExit">The LINQPad script execution timeout. 1 minute is the default.</param>
        /// <returns>The LINQPad script execution <see cref="Result"/>.</returns>
        /// <exception cref="LPRunException">Keeps original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static Result Execute(string linqFile, TimeSpan? waitForExit = default)
        {
            return Wrap(ExecuteInternal);

            Result ExecuteInternal()
            {
                waitForExit ??= DefaultTimeout;

                var output = new StringBuilder();
                var error = new StringBuilder();

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo(Exe, GetArguments())
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.OutputDataReceived += OutputDataReceivedHandler;
                process.ErrorDataReceived += ErrorDataReceivedHandler;

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var completed = process.WaitForExit((int)waitForExit?.TotalMilliseconds!);

                process.CancelOutputRead();
                process.CancelErrorRead();

                process.OutputDataReceived -= OutputDataReceivedHandler;
                process.ErrorDataReceived -= ErrorDataReceivedHandler;

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
                    if (IgnoredErrorMessages.All(message => e.Data?.StartsWith(message) == false))
                    {
                        error.Append(e.Data);
                    }
                }
            }
        }
    }
}
