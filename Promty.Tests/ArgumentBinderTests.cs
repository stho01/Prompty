using Promty.Attributes;
using Promty.Infrastructure;

namespace Promty.Tests;

public class ArgumentBinderTests
{
    private class SimpleArgs
    {
        [Description("name", "The name")]
        public string Name { get; set; } = string.Empty;

        [FlagAlias("verbose", 'v')]
        [Description("Verbose output")]
        public bool Verbose { get; set; }
    }

    private class NumericArgs
    {
        [Description("count", "The count")]
        public int Count { get; set; }

        [FlagAlias("port", 'p')]
        [Description("Port number")]
        public int? Port { get; set; }
    }

    private class MultiplePositionalArgs
    {
        [Description("source", "Source file")]
        public string Source { get; set; } = string.Empty;

        [Description("destination", "Destination file")]
        public string Destination { get; set; } = string.Empty;

        [FlagAlias("overwrite", 'o')]
        [Description("Overwrite existing")]
        public bool Overwrite { get; set; }
    }

    private class AllTypesArgs
    {
        [Description("text", "Text value")]
        public string Text { get; set; } = string.Empty;

        [FlagAlias("integer", 'i')]
        public int? Integer { get; set; }

        [FlagAlias("long", 'l')]
        public long? Long { get; set; }

        [FlagAlias("double", 'd')]
        public double? Double { get; set; }

        [FlagAlias("bool", 'b')]
        public bool Bool { get; set; }
    }

    [Fact]
    public void Bind_WithSimplePositionalArg_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["Alice"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<SimpleArgs>(parser);

        Assert.Equal("Alice", args.Name);
        Assert.False(args.Verbose);
    }

    [Fact]
    public void Bind_WithPositionalAndFlag_ShouldBindBoth()
    {
        var parser = new CommandLineParser();
        parser.Parse(["Bob", "--verbose"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<SimpleArgs>(parser);

        Assert.Equal("Bob", args.Name);
        Assert.True(args.Verbose);
    }

    [Fact]
    public void Bind_WithShortFlag_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["Charlie", "-v"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<SimpleArgs>(parser);

        Assert.Equal("Charlie", args.Name);
        Assert.True(args.Verbose);
    }

    [Fact]
    public void Bind_WithMissingPositionalArg_ShouldThrowException()
    {
        var parser = new CommandLineParser();
        parser.Parse([]);

        var binder = new ArgumentBinder();

        Assert.Throws<ArgumentException>(() => binder.Bind<SimpleArgs>(parser));
    }

    [Fact]
    public void Bind_WithIntegerArg_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["42"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<NumericArgs>(parser);

        Assert.Equal(42, args.Count);
        Assert.Null(args.Port);
    }

    [Fact]
    public void Bind_WithNullableIntegerFlag_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["100", "--port", "8080"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<NumericArgs>(parser);

        Assert.Equal(100, args.Count);
        Assert.Equal(8080, args.Port);
    }

    [Fact]
    public void Bind_WithMultiplePositionalArgs_ShouldBindInOrder()
    {
        var parser = new CommandLineParser();
        parser.Parse(["input.txt", "output.txt"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<MultiplePositionalArgs>(parser);

        Assert.Equal("input.txt", args.Source);
        Assert.Equal("output.txt", args.Destination);
        Assert.False(args.Overwrite);
    }

    [Fact]
    public void Bind_WithMultiplePositionalAndFlag_ShouldBindAll()
    {
        var parser = new CommandLineParser();
        parser.Parse(["src.txt", "dst.txt", "--overwrite"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<MultiplePositionalArgs>(parser);

        Assert.Equal("src.txt", args.Source);
        Assert.Equal("dst.txt", args.Destination);
        Assert.True(args.Overwrite);
    }

    [Fact]
    public void Bind_WithAllTypes_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["hello", "-i", "42", "-l", "1000000", "-b"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<AllTypesArgs>(parser);

        Assert.Equal("hello", args.Text);
        Assert.Equal(42, args.Integer);
        Assert.Equal(1000000, args.Long);
        Assert.True(args.Bool);
    }

    [Fact]
    public void Bind_WithDoubleType_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "-d", "3.14"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<AllTypesArgs>(parser);

        Assert.Equal("test", args.Text);
        Assert.Equal(3.14, args.Double);
    }

    [Fact]
    public void Bind_WithInvalidIntegerValue_ShouldThrowException()
    {
        var parser = new CommandLineParser();
        parser.Parse(["notanumber"]);

        var binder = new ArgumentBinder();

        Assert.Throws<ArgumentException>(() => binder.Bind<NumericArgs>(parser));
    }

    [Fact]
    public void Bind_WithLongFlagAlias_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["10", "--port", "3000"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<NumericArgs>(parser);

        Assert.Equal(10, args.Count);
        Assert.Equal(3000, args.Port);
    }

    [Fact]
    public void Bind_WithShortFlagAlias_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["20", "-p", "4000"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<NumericArgs>(parser);

        Assert.Equal(20, args.Count);
        Assert.Equal(4000, args.Port);
    }

    [Fact]
    public void Bind_WithNoOptionalFlags_ShouldUseDefaults()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<AllTypesArgs>(parser);

        Assert.Equal("test", args.Text);
        Assert.Null(args.Integer);
        Assert.Null(args.Long);
        Assert.Null(args.Double);
        Assert.False(args.Bool);
    }

    // Flags enum tests
    [Flags]
    private enum BuildOptions
    {
        None = 0,
        [FlagAlias("verbose", 'v')]
        Verbose = 1,
        [FlagAlias("debug", 'd')]
        Debug = 2,
        Release = 4,
        SkipTests = 8
    }

    private class FlagsEnumArgs
    {
        [Description("name", "The name")]
        public string Name { get; set; } = string.Empty;

        public BuildOptions Options { get; set; }
    }

    [Fact]
    public void Bind_WithSingleFlagAlias_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "--verbose"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.Verbose, args.Options);
    }

    [Fact]
    public void Bind_WithMultipleFlagAliases_ShouldCombineFlags()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "--verbose", "--debug"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.Verbose | BuildOptions.Debug, args.Options);
    }

    [Fact]
    public void Bind_WithShortFlagAliasForFlagsEnum_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "-v"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.Verbose, args.Options);
    }

    [Fact]
    public void Bind_WithKebabCaseFlagName_ShouldBindCorrectly()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "--skip-tests"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.SkipTests, args.Options);
    }

    [Fact]
    public void Bind_WithMixedAliasesAndKebabCase_ShouldCombineFlags()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "-v", "--release", "--skip-tests"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.Verbose | BuildOptions.Release | BuildOptions.SkipTests, args.Options);
    }

    [Fact]
    public void Bind_WithNoFlags_ShouldReturnNone()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.None, args.Options);
    }

    [Fact]
    public void Bind_WithAllFlags_ShouldCombineAll()
    {
        var parser = new CommandLineParser();
        parser.Parse(["test", "-v", "-d", "--release", "--skip-tests"]);

        var binder = new ArgumentBinder();
        var args = binder.Bind<FlagsEnumArgs>(parser);

        Assert.Equal("test", args.Name);
        Assert.Equal(BuildOptions.Verbose | BuildOptions.Debug | BuildOptions.Release | BuildOptions.SkipTests, args.Options);
    }
}
