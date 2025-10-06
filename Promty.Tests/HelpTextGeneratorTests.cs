using System.Reflection;
using Promty;
using Promty.Attributes;

namespace Promty.Tests;

public class HelpTextGeneratorTests
{
    [Description("simple", "A simple test command")]
    private class SimpleCommand : Command<SimpleCommand.Args>
    {
        public class Args
        {
            [Description("name", "The name argument")]
            public string Name { get; set; } = string.Empty;

            [FlagAlias("verbose", 'v')]
            [Description("Enable verbose output")]
            public bool Verbose { get; set; }
        }

        public override Task<int> ExecuteAsync(Args args) => Task.FromResult(0);
    }

    [Description("complex", "A complex command with multiple arguments")]
    private class ComplexCommand : Command<ComplexCommand.Args>
    {
        public class Args
        {
            [Description("source", "Source file path")]
            public string Source { get; set; } = string.Empty;

            [Description("destination", "Destination file path")]
            public string Destination { get; set; } = string.Empty;

            [FlagAlias("force", 'f')]
            [Description("Force overwrite")]
            public bool Force { get; set; }

            [FlagAlias("port", 'p')]
            [Description("Port number")]
            public int? Port { get; set; }

            [FlagAlias("timeout")]
            [Description("Timeout in seconds")]
            public int? Timeout { get; set; }
        }

        public override Task<int> ExecuteAsync(Args args) => Task.FromResult(0);
    }

    [Description("minimal", "Minimal command")]
    private class MinimalCommand : Command<MinimalCommand.Args>
    {
        public class Args
        {
        }

        public override Task<int> ExecuteAsync(Args args) => Task.FromResult(0);
    }

    [Fact]
    public async Task CommandExecutor_WithMissingRequiredArg_ShouldShowCommandHelp()
    {
        var output = await CaptureConsoleOutput(async () =>
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            await executor.ExecuteAsync(["simple"]);
        });

        Assert.Contains("Usage: simple <name> [options]", output);
        Assert.Contains("A simple test command", output);
        Assert.Contains("Arguments:", output);
        Assert.Contains("<name>", output);
        Assert.Contains("The name argument", output);
        Assert.Contains("Options:", output);
        Assert.Contains("-v, --verbose", output);
        Assert.Contains("Enable verbose output", output);
    }

    [Fact]
    public async Task CommandExecutor_WithComplexCommand_ShouldShowAllArguments()
    {
        var output = await CaptureConsoleOutput(async () =>
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            await executor.ExecuteAsync(["complex"]);
        });

        Assert.Contains("Usage: complex <source> <destination> [options]", output);
        Assert.Contains("A complex command with multiple arguments", output);
        Assert.Contains("<source>", output);
        Assert.Contains("Source file path", output);
        Assert.Contains("<destination>", output);
        Assert.Contains("Destination file path", output);
        Assert.Contains("-f, --force", output);
        Assert.Contains("-p, --port", output);
        Assert.Contains("--timeout", output);
    }

    [Fact]
    public async Task CommandExecutor_WithMultipleCommands_ShouldShowAllInList()
    {
        var output = await CaptureConsoleOutput(async () =>
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            await executor.ExecuteAsync([]);
        });

        // Should show all registered commands in the list
        Assert.Contains("simple", output);
        Assert.Contains("complex", output);
        Assert.Contains("minimal", output);
    }

    [Fact]
    public async Task CommandExecutor_WithUnknownCommand_ShouldShowCommandList()
    {
        var output = await CaptureConsoleOutput(async () =>
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            await executor.ExecuteAsync(["unknown"]);
        });

        Assert.Contains("Error: Unknown command 'unknown'", output);
        Assert.Contains("Available commands:", output);
    }

    [Fact]
    public async Task CommandList_ShouldBeAlphabeticallySorted()
    {
        var output = await CaptureConsoleOutput(async () =>
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            await executor.ExecuteAsync([]);
        });

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        var complexIndex = lines.FindIndex(l => l.Contains("complex"));
        var minimalIndex = lines.FindIndex(l => l.Contains("minimal"));
        var simpleIndex = lines.FindIndex(l => l.Contains("simple"));

        Assert.True(complexIndex < minimalIndex);
        Assert.True(minimalIndex < simpleIndex);
    }

    private async Task<string> CaptureConsoleOutput(Func<Task> action)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        var writer = new StringWriter();

        try
        {
            Console.SetOut(writer);
            Console.SetError(writer);

            await action();

            Console.SetOut(originalOut);
            Console.SetError(originalError);

            var result = writer.ToString();
            return result;
        }
        finally
        {
            writer.Dispose();
        }
    }
}
