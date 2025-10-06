using System.Text.Json;
using Promty;
using Promty.Attributes;

namespace Commander.Commands;

public sealed class FooCommand : Command<FooCommand.Args>
{
    public sealed class Args
    {
        [Description("bara", "The Bar argument")]
        public string Bar { get; set; } = "";

        [Description("baz", "The baz argument")]
        public int? Baz { get; set; }
        [FlagAlias("force", 'f')]
        [Description("Force the operation")]
        public bool Force { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine(JsonSerializer.Serialize(args));
        return Task.FromResult(0);
    }
}