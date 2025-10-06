using Promty;
using Promty.Attributes;

namespace Commander.Commands;

[Description("greet", "Greets a person by name")]
public class GreetCommand : Command<GreetCommand.Args>
{
    public class Args
    {
        [Description("name", "The name of the person to greet")]
        public string Name { get; set; } = string.Empty;

        [FlagAlias("uppercase", 'u')]
        [Description("Print the greeting in uppercase")]
        public bool Uppercase { get; set; }

        [FlagAlias("repeat", 'r')]
        [Description("Number of times to repeat the greeting")]
        public int? Repeat { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        var greeting = $"Hello, {args.Name}!";

        if (args.Uppercase)
        {
            greeting = greeting.ToUpper();
        }

        var repeat = args.Repeat ?? 1;
        for (int i = 0; i < repeat; i++)
        {
            Console.WriteLine(greeting);
        }

        return Task.FromResult(0);
    }
}
