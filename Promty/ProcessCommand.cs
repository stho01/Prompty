using System.Diagnostics;

namespace Promty;

public abstract class ProcessCommand : Command<ProcessCommand.Args>
{
    public class Args
    {
        // All arguments will be captured as raw strings
        internal string[] RawArguments { get; set; } = [];

        public Args()
        {
        }
    }

    protected abstract string ExecutablePath { get; }

    public override async Task<int> ExecuteAsync(Args args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ExecutablePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };

        // Add all arguments to the process
        foreach (var arg in args.RawArguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };

        // Forward output and error streams to console
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.Error.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}
