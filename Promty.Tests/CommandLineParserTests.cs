using Promty.Infrastructure;

namespace Promty.Tests;

public class CommandLineParserTests
{
    [Fact]
    public void Parse_WithLongFlag_ShouldSetFlag()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--verbose"]);

        Assert.True(parser.HasFlag("verbose"));
    }

    [Fact]
    public void Parse_WithShortFlag_ShouldSetFlag()
    {
        var parser = new CommandLineParser();
        parser.Parse(["-v"]);

        Assert.True(parser.HasFlag("v"));
    }

    [Fact]
    public void Parse_WithLongFlagAndValue_ShouldSetArgument()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--name", "Alice"]);

        Assert.Equal("Alice", parser.GetArgument("name"));
    }

    [Fact]
    public void Parse_WithShortFlagAndValue_ShouldSetArgument()
    {
        var parser = new CommandLineParser();
        parser.Parse(["-n", "Bob"]);

        Assert.Equal("Bob", parser.GetArgument("n"));
    }

    [Fact]
    public void Parse_WithLongFlagEqualsSyntax_ShouldSetArgument()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--name=Charlie"]);

        Assert.Equal("Charlie", parser.GetArgument("name"));
    }

    [Fact]
    public void Parse_WithPositionalArguments_ShouldReturnThem()
    {
        var parser = new CommandLineParser();
        parser.Parse(["file1.txt", "file2.txt"]);

        var positionalArgs = parser.GetPositionalArguments();
        Assert.Equal(2, positionalArgs.Count);
        Assert.Equal("file1.txt", positionalArgs[0]);
        Assert.Equal("file2.txt", positionalArgs[1]);
    }

    [Fact]
    public void Parse_WithMixedArguments_ShouldParseBoth()
    {
        var parser = new CommandLineParser();
        parser.Parse(["input.txt", "--verbose", "-o", "output.txt", "extra.txt"]);

        var positionalArgs = parser.GetPositionalArguments();
        Assert.Equal(2, positionalArgs.Count);
        Assert.Equal("input.txt", positionalArgs[0]);
        Assert.Equal("extra.txt", positionalArgs[1]);
        Assert.True(parser.HasFlag("verbose"));
        Assert.Equal("output.txt", parser.GetArgument("o"));
    }

    [Fact]
    public void Parse_WithMultipleFlags_ShouldSetAll()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--verbose", "--force", "-d"]);

        Assert.True(parser.HasFlag("verbose"));
        Assert.True(parser.HasFlag("force"));
        Assert.True(parser.HasFlag("d"));
    }

    [Fact]
    public void GetArgument_WithMissingKey_ShouldReturnNull()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--name", "Alice"]);

        Assert.Null(parser.GetArgument("missing"));
    }

    [Fact]
    public void GetArgument_WithDefaultValue_ShouldReturnDefault()
    {
        var parser = new CommandLineParser();
        parser.Parse([]);

        Assert.Equal("default", parser.GetArgument("missing", "default"));
    }

    [Fact]
    public void HasFlag_WithMissingFlag_ShouldReturnFalse()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--verbose"]);

        Assert.False(parser.HasFlag("missing"));
    }

    [Fact]
    public void GetAllArguments_ShouldReturnAllParsedArguments()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--name", "Alice", "--age", "30"]);

        var allArgs = parser.GetAllArguments();
        Assert.Equal(2, allArgs.Count);
        Assert.Equal("Alice", allArgs["name"]);
        Assert.Equal("30", allArgs["age"]);
    }

    [Fact]
    public void GetAllFlags_ShouldReturnAllParsedFlags()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--verbose", "--force"]);

        var allFlags = parser.GetAllFlags();
        Assert.Equal(2, allFlags.Count);
        Assert.Contains("verbose", allFlags);
        Assert.Contains("force", allFlags);
    }

    [Fact]
    public void Parse_WithFlagFollowedByAnotherFlag_ShouldTreatAsFlag()
    {
        var parser = new CommandLineParser();
        parser.Parse(["--verbose", "--force"]);

        Assert.True(parser.HasFlag("verbose"));
        Assert.True(parser.HasFlag("force"));
    }

    [Fact]
    public void Parse_WithEmptyArray_ShouldNotThrow()
    {
        var parser = new CommandLineParser();
        parser.Parse([]);

        Assert.Empty(parser.GetPositionalArguments());
        Assert.Empty(parser.GetAllArguments());
        Assert.Empty(parser.GetAllFlags());
    }
}
