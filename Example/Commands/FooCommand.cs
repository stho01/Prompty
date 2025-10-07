using System.Text.Json;
using Promty;
using Promty.Attributes;

namespace Example.Commands;

public sealed class FooCommand : Command<FooCommand.Args>
{
    [Flags]
    public enum BuildOptions
    {
        None = 0,
        [FlagAlias("verbose", 'v')]
        Verbose = 1,
        [FlagAlias("debug", 'd')]
        Debug = 2,
        NoCache = 4,
        SkipTests = 8
    }

    public sealed class Args
    {
        [Description("bara", "The Bar argument")]
        public string Bar { get; set; } = "";

        [Description("baz", "The baz argument")]
        public int? Baz { get; set; }

        public BuildOptions Options { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine(JsonSerializer.Serialize(args));
        Console.WriteLine($"\nBuild options:");
        if (args.Options.HasFlag(BuildOptions.Verbose))
            Console.WriteLine("  - Verbose enabled");
        if (args.Options.HasFlag(BuildOptions.Debug))
            Console.WriteLine("  - Debug mode");
        if (args.Options.HasFlag(BuildOptions.NoCache))
            Console.WriteLine("  - Cache disabled");
        if (args.Options.HasFlag(BuildOptions.SkipTests))
            Console.WriteLine("  - Tests skipped");
        if (args.Options == BuildOptions.None)
            Console.WriteLine("  - No options set");

        return Task.FromResult(0);
    }
}