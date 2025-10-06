using Promty;
using Promty.Attributes;

namespace Commander.SubCommands.Commands;

[Description("bar", "Execute the bar command")]
public class BarCommand : Command<BarCommand.Args>
{
    public class Args
    {
        public string Name { get; set; } = "";
        
        public string Value { get; set; } = "";
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine($"Bar Command: Name={args.Name}, Value={args.Value}");
        return Task.FromResult(0);
    }
}