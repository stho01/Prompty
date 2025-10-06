using Promty;
using Promty.Attributes;

namespace Promty.Tests;

public class CommandTests
{
    private class SimpleCommand : Command<SimpleCommand.Args>
    {
        public class Args
        {
            [Description("message", "The message")]
            public string Message { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(Args args)
        {
            return Task.FromResult(0);
        }
    }

    private class AsyncCommand : Command<AsyncCommand.Args>
    {
        public class Args
        {
            [Description("delay", "Delay in milliseconds")]
            public int Delay { get; set; }
        }

        public override async Task<int> ExecuteAsync(Args args)
        {
            await Task.Delay(args.Delay);
            return 0;
        }
    }

    private class CommandWithReturnCode : Command<CommandWithReturnCode.Args>
    {
        public class Args
        {
            [Description("code", "Exit code to return")]
            public int Code { get; set; }
        }

        public override Task<int> ExecuteAsync(Args args)
        {
            return Task.FromResult(args.Code);
        }
    }

    [Fact]
    public async Task Command_CanBeInstantiatedAndExecuted()
    {
        var command = new SimpleCommand();
        var args = new SimpleCommand.Args { Message = "Hello" };

        var result = await command.ExecuteAsync(args);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Command_WithAsyncOperation_ShouldComplete()
    {
        var command = new AsyncCommand();
        var args = new AsyncCommand.Args { Delay = 10 };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await command.ExecuteAsync(args);
        stopwatch.Stop();

        Assert.Equal(0, result);
        Assert.True(stopwatch.ElapsedMilliseconds >= 10);
    }

    [Fact]
    public async Task Command_CanReturnCustomExitCode()
    {
        var command = new CommandWithReturnCode();

        var args1 = new CommandWithReturnCode.Args { Code = 0 };
        Assert.Equal(0, await command.ExecuteAsync(args1));

        var args2 = new CommandWithReturnCode.Args { Code = 1 };
        Assert.Equal(1, await command.ExecuteAsync(args2));

        var args3 = new CommandWithReturnCode.Args { Code = 42 };
        Assert.Equal(42, await command.ExecuteAsync(args3));
    }

    [Fact]
    public void Command_ArgsCanHaveDefaultValues()
    {
        var args = new SimpleCommand.Args();

        Assert.Equal(string.Empty, args.Message);
    }

    [Fact]
    public void Command_ArgsCanBeModified()
    {
        var args = new SimpleCommand.Args { Message = "Initial" };
        Assert.Equal("Initial", args.Message);

        args.Message = "Modified";
        Assert.Equal("Modified", args.Message);
    }
}
