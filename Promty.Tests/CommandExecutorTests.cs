using System.Reflection;
using Promty;
using Promty.Attributes;

namespace Promty.Tests;

public class CommandExecutorTests
{
    [Description("test", "A test command")]
    private class TestCommand : Command<TestCommand.Args>
    {
        public class Args
        {
            [Description("name", "The name")]
            public string Name { get; set; } = string.Empty;

            [FlagAlias("verbose", 'v')]
            public bool Verbose { get; set; }
        }

        public static int ExecuteCallCount { get; set; }
        public static Args? LastArgs { get; set; }

        public override Task<int> ExecuteAsync(Args args)
        {
            ExecuteCallCount++;
            LastArgs = args;
            return Task.FromResult(0);
        }
    }

    [Description("fail", "A command that fails")]
    private class FailingCommand : Command<FailingCommand.Args>
    {
        public class Args
        {
            [Description("value", "A value")]
            public string Value { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(Args args)
        {
            return Task.FromResult(1);
        }
    }

    [Description("throw", "A command that throws")]
    private class ThrowingCommand : Command<ThrowingCommand.Args>
    {
        public class Args
        {
            [Description("value", "A value")]
            public string Value { get; set; } = string.Empty;
        }

        public override Task<int> ExecuteAsync(Args args)
        {
            throw new InvalidOperationException("Command failed");
        }
    }

    public CommandExecutorTests()
    {
        // Reset static counters before each test
        TestCommand.ExecuteCallCount = 0;
        TestCommand.LastArgs = null;
    }

    [Fact]
    public async Task ExecuteAsync_WithNoArgs_ShouldReturnError()
    {
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);

            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

            var result = await executor.ExecuteAsync([]);

            Assert.Equal(1, result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ShouldReturnError()
    {
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);

            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

            var result = await executor.ExecuteAsync(["unknown"]);

            Assert.Equal(1, result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldExecuteCommand()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        var result = await executor.ExecuteAsync(["test", "Alice"]);

        Assert.Equal(0, result);
        Assert.Equal(1, TestCommand.ExecuteCallCount);
        Assert.NotNull(TestCommand.LastArgs);
        Assert.Equal("Alice", TestCommand.LastArgs.Name);
        Assert.False(TestCommand.LastArgs.Verbose);
    }

    [Fact]
    public async Task ExecuteAsync_WithCommandAndFlags_ShouldBindCorrectly()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        var result = await executor.ExecuteAsync(["test", "Bob", "--verbose"]);

        Assert.Equal(0, result);
        Assert.Equal(1, TestCommand.ExecuteCallCount);
        Assert.NotNull(TestCommand.LastArgs);
        Assert.Equal("Bob", TestCommand.LastArgs.Name);
        Assert.True(TestCommand.LastArgs.Verbose);
    }

    [Fact]
    public async Task ExecuteAsync_WithShortFlag_ShouldBindCorrectly()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        var result = await executor.ExecuteAsync(["test", "Charlie", "-v"]);

        Assert.Equal(0, result);
        Assert.NotNull(TestCommand.LastArgs);
        Assert.Equal("Charlie", TestCommand.LastArgs.Name);
        Assert.True(TestCommand.LastArgs.Verbose);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingCommand_ShouldReturnErrorCode()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        var result = await executor.ExecuteAsync(["fail", "test"]);

        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithThrowingCommand_ShouldCatchAndReturnError()
    {
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);

            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

            var result = await executor.ExecuteAsync(["throw", "test"]);

            Assert.Equal(1, result);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredArg_ShouldReturnError()
    {
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);

            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

            var result = await executor.ExecuteAsync(["test"]);

            Assert.Equal(1, result);
            Assert.Equal(0, TestCommand.ExecuteCallCount);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task RegisterCommandsFromAssembly_ShouldRegisterAllCommands()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        // Test command should be registered
        var result1 = await executor.ExecuteAsync(["test", "name"]);
        Assert.Equal(0, result1);

        // Fail command should be registered
        var result2 = await executor.ExecuteAsync(["fail", "value"]);
        Assert.Equal(1, result2);

        // Throw command should be registered
        var result3 = await executor.ExecuteAsync(["throw", "value"]);
        Assert.Equal(1, result3);
    }

    [Fact]
    public async Task ExecuteAsync_WithCaseInsensitiveCommand_ShouldWork()
    {
        var executor = new CommandExecutor();
        executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

        var result1 = await executor.ExecuteAsync(["TEST", "Alice"]);
        Assert.Equal(0, result1);

        var result2 = await executor.ExecuteAsync(["Test", "Bob"]);
        Assert.Equal(0, result2);

        var result3 = await executor.ExecuteAsync(["test", "Charlie"]);
        Assert.Equal(0, result3);
    }
}
