# Getting Started

## Installation

Install Promty via NuGet:

```bash
dotnet add package Promty
```

## Your First Command

Create a simple command that greets a user:

```csharp
using Promty;
using Promty.Attributes;

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
```

## Set Up the Executor

In your `Program.cs`:

```csharp
using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

return await executor.ExecuteAsync(args);
```

## Run Your CLI

```bash
# Show available commands
dotnet run

# Run with arguments
dotnet run -- greet Alice

# Use flags
dotnet run -- greet Bob --uppercase -r 3
```

Output:
```
HELLO, BOB!
HELLO, BOB!
HELLO, BOB!
```

## Next Steps

- Learn about [Commands](/guide/commands)
- Explore [Arguments & Flags](/guide/arguments)
- Check out [Flags Enums](/guide/flags-enums) for grouping related flags
- See more [Examples](/examples/basic)
