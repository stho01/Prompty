# Basic Command Example

A simple command that demonstrates the core features of Promty.

## Echo Command

Create a command that echoes text with optional transformations:

```csharp
using Promty;
using Promty.Attributes;

[Description("echo", "Print a message to the console")]
public class EchoCommand : Command<EchoCommand.Args>
{
    public class Args
    {
        [Description("message", "The message to print")]
        public string Message { get; set; } = string.Empty;

        [FlagAlias("uppercase", 'u')]
        [Description("Convert message to uppercase")]
        public bool Uppercase { get; set; }

        [FlagAlias("repeat", 'r')]
        [Description("Number of times to repeat the message")]
        public int? Repeat { get; set; }

        [FlagAlias("prefix", 'p')]
        [Description("Prefix to add before the message")]
        public string? Prefix { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        var message = args.Message;

        // Apply transformations
        if (args.Uppercase)
        {
            message = message.ToUpper();
        }

        if (!string.IsNullOrEmpty(args.Prefix))
        {
            message = $"{args.Prefix}: {message}";
        }

        // Repeat the message
        var repeat = args.Repeat ?? 1;
        for (int i = 0; i < repeat; i++)
        {
            Console.WriteLine(message);
        }

        return Task.FromResult(0);
    }
}
```

## Usage Examples

### Basic usage
```bash
dotnet run -- echo "Hello, World!"
```
Output:
```
Hello, World!
```

### With uppercase flag
```bash
dotnet run -- echo "Hello, World!" --uppercase
```
Output:
```
HELLO, WORLD!
```

### With repeat
```bash
dotnet run -- echo "Hello!" -r 3
```
Output:
```
Hello!
Hello!
Hello!
```

### With prefix
```bash
dotnet run -- echo "Something went wrong" -p "ERROR"
```
Output:
```
ERROR: Something went wrong
```

### Combined flags
```bash
dotnet run -- echo "warning" -u -p "ALERT" -r 2
```
Output:
```
ALERT: WARNING
ALERT: WARNING
```

## Help Text

```bash
dotnet run -- echo --help
```

Output:
```
Usage: echo <message> [options]

Print a message to the console

Arguments:
  <message>  The message to print

Options:
  -u, --uppercase        Convert message to uppercase
  -r, --repeat <repeat>  Number of times to repeat the message
  -p, --prefix <prefix>  Prefix to add before the message
```

## Key Features Demonstrated

- ✅ Positional argument (`message`)
- ✅ Boolean flag (`uppercase`)
- ✅ Nullable int flag (`repeat`)
- ✅ String flag with value (`prefix`)
- ✅ Long and short aliases
- ✅ Default values handling
- ✅ Simple logic implementation

## Complete Program

```csharp
using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
return await executor.ExecuteAsync(args);

[Description("echo", "Print a message to the console")]
public class EchoCommand : Command<EchoCommand.Args>
{
    public class Args
    {
        [Description("message", "The message to print")]
        public string Message { get; set; } = string.Empty;

        [FlagAlias("uppercase", 'u')]
        [Description("Convert message to uppercase")]
        public bool Uppercase { get; set; }

        [FlagAlias("repeat", 'r')]
        [Description("Number of times to repeat the message")]
        public int? Repeat { get; set; }

        [FlagAlias("prefix", 'p')]
        [Description("Prefix to add before the message")]
        public string? Prefix { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        var message = args.Message;

        if (args.Uppercase)
            message = message.ToUpper();

        if (!string.IsNullOrEmpty(args.Prefix))
            message = $"{args.Prefix}: {message}";

        var repeat = args.Repeat ?? 1;
        for (int i = 0; i < repeat; i++)
            Console.WriteLine(message);

        return Task.FromResult(0);
    }
}
```
