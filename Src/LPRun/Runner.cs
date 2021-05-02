using System;
using System.Diagnostics;
using System.Text;

namespace LPRun
{
    public static class Runner
    {
        public record Result(string Output, string Error, int ExitCode);

        public static Result Execute(string linqFile, TimeSpan waitForExit)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo(Context.Exe, Context.GetFullPath(linqFile))
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

            var completed = process.WaitForExit((int) waitForExit.TotalMilliseconds);

            process.CancelOutputRead();
            process.CancelErrorRead();

            process.OutputDataReceived -= OutputDataReceivedHandler;
            process.ErrorDataReceived -= ErrorDataReceivedHandler;

            if (completed)
            {
                return new Result(output.ToString(), error.ToString(), process.ExitCode);
            }

            process.Kill();

            throw new TimeoutException($"LPRun timed out for {waitForExit}");

            void OutputDataReceivedHandler(object _, DataReceivedEventArgs e) =>
                output.Append(e.Data);

            void ErrorDataReceivedHandler(object _, DataReceivedEventArgs e)
            {
                if (e.Data?.StartsWith("Downloading NuGet package") == false &&
                    e.Data?.StartsWith("Restoring package") == false)
                {
                    error.Append(e.Data);
                }
            }
        }
    }
}
