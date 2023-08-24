using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="commandLineParams">The additional <a href="https://www.linqpad.net/lprun.aspx">LPRun command-line</a> parameters. <seealso href="https://www.linqpad.net/lprun.aspx">LINQPad Command-Line and Scripting</seealso></param>
        /// <returns>The LINQPad script execution <see cref="Result"/>.</returns>
        /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static Result Execute(string linqFile, TimeSpan? waitForExit = null, RetryOnError? retryOnError = null, params string[] commandLineParams) =>
            ExecuteAsyncInternal(true, linqFile, waitForExit, retryOnError, commandLineParams).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously executes LINQPad script using LPRun with optional timeout specified.
        /// </summary>
        /// <param name="linqFile">The LINQPad script file to execute.</param>
        /// <param name="waitForExit">The LINQPad script execution timeout. 1 minute is the default.</param>
        /// <param name="retryOnError">The number of times to retry the operation on error and timeout between tries.</param>
        /// <param name="commandLineParams">The additional <a href="https://www.linqpad.net/lprun.aspx">LPRun command-line</a> parameters. <seealso href="https://www.linqpad.net/lprun.aspx">LINQPad Command-Line and Scripting</seealso></param>
        /// <returns>A task that represents the asynchronous LINQPad script execution <see cref="Result"/>.</returns>
        /// <exception cref="LPRunException">Keeps the original exception as <see cref="P:System.Exception.InnerException"/>.</exception>
        public static Task<Result> ExecuteAsync(string linqFile, TimeSpan? waitForExit = null, RetryOnError? retryOnError = null, params string[] commandLineParams) =>
            ExecuteAsyncInternal(false, linqFile, waitForExit, retryOnError, commandLineParams);

        private static Task<Result> ExecuteAsyncInternal(bool asSync, string linqFile, TimeSpan? waitForExit, RetryOnError? retryOnError, params string[] commandLineParams)
        {
            return RetryAsync(() => WrapAsync(ExecuteAsyncLocal));

            async Task<Result> RetryAsync(Func<Task<Result>> func)
            {
                var times   = retryOnError?.Times   ?? 1;
                var timeout = retryOnError?.Timeout ?? RetryTimeout;

                while (true)
                {
                    var result = await func();

                    if (result.Success || --times <= 0)
                    {
                        return result;
                    }

                    await Sleep();
                }

                async Task Sleep()
                {
                    if (asSync)
                    {
                        Thread.Sleep(timeout);
                    }
                    else
                    {
                        await Task.Delay(timeout).ConfigureAwait(false);
                    }
                }
            }

            async Task<Result> ExecuteAsyncLocal()
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

                var waitForExitMilliseconds = (int)waitForExit?.TotalMilliseconds!;
                var completed = await WaitForExitAsync();

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
                    $@"-fx={FrameworkInfo.Version.Major}.{FrameworkInfo.Version.Minor} ""{GetFullPath(linqFile)}"" {string.Join(" ", commandLineParams)}";

                void OutputDataReceivedHandler(object _, DataReceivedEventArgs e) =>
                    output.Append(e.Data);

                void ErrorDataReceivedHandler(object _, DataReceivedEventArgs e)
                {
                    if (Array.TrueForAll(IgnoredErrorMessages, message => e.Data?.StartsWith(message) == false))
                    {
                        error.Append(e.Data);
                    }
                }

#if NET5_0_OR_GREATER
                async Task<bool> WaitForExitAsync()
                {
                    if (asSync)
                    {
                        return WaitForExit();
                    }

                    try
                    {
                        var waitForExitTimeSpan = TimeSpan.FromMilliseconds(waitForExitMilliseconds);
#if !NET6_0_OR_GREATER
                        using var cancellationTokenSource = new CancellationTokenSource(waitForExitTimeSpan);
#endif
                        await process.WaitForExitAsync
#if NET6_0_OR_GREATER
                            ().WaitAsync(waitForExitTimeSpan)
#else
                            (cancellationTokenSource.Token)
#endif
                            .ConfigureAwait(false);
                        return true;
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                }
#else
                Task<bool> WaitForExitAsync() =>
                    Task.FromResult(WaitForExit());
#endif

                bool WaitForExit() =>
                    process.WaitForExit(waitForExitMilliseconds);
            }
        }
    }
}
