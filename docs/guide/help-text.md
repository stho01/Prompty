# Help Text

Promty automatically generates beautiful help text from your command and argument attributes.

## Automatic Generation

Help text is displayed when:
- No commands are provided (shows command list)
- An unknown command is entered
- A required argument is missing
- The `--help` or `-h` flag is used (if implemented)

## Command List

When no command is provided, Promty shows all available commands:

```
Available commands:

  build    Build a project with options
  copy     Copies a file from source to destination
  git      Execute git commands
  greet    Greets a person by name
```

### Customizing Command Descriptions

Use the `[Description]` attribute on your command class:

```csharp
[Description("greet", "Greets a person by name")]
public class GreetCommand : Command<GreetCommand.Args>
{
    // ...
}
```

The first parameter is the command name, the second is the description.

## Command Help

When a required argument is missing, Promty shows command-specific help:

```
Usage: greet <name> [options]

Greets a person by name

Arguments:
  <name>  The name of the person to greet

Options:
  -u, --uppercase          Print the greeting in uppercase
  -r, --repeat <repeat>    Number of times to repeat the greeting
```

### Structure

The help text includes:

1. **Usage Line** - Shows command structure
2. **Description** - From the command's `[Description]` attribute
3. **Arguments** - Required positional arguments
4. **Options** - Optional flags

## Argument Help

### Positional Arguments

```csharp
[Description("source", "The source file path")]
public string Source { get; set; } = string.Empty;
```

Appears in help as:
```
Arguments:
  <source>  The source file path
```

### Flag Arguments

```csharp
[FlagAlias("verbose", 'v')]
[Description("Enable verbose output")]
public bool Verbose { get; set; }
```

Appears in help as:
```
Options:
  -v, --verbose  Enable verbose output
```

### Flags with Values

Non-boolean flags show a value placeholder:

```csharp
[FlagAlias("port", 'p')]
[Description("Port number")]
public int? Port { get; set; }
```

Appears in help as:
```
Options:
  -p, --port <port>  Port number
```

## Flags Enum Help

Flags enums display each flag individually:

```csharp
[Flags]
public enum BuildOptions
{
    None = 0,
    [FlagAlias("verbose", 'v')]
    [Description("Enable verbose output")]
    Verbose = 1,
    [FlagAlias("debug", 'd')]
    [Description("Build in debug mode")]
    Debug = 2,
    [Description("Disable build cache")]
    NoCache = 4,
    [Description("Skip running tests")]
    SkipTests = 8
}

public class Args
{
    public BuildOptions Options { get; set; }
}
```

Appears in help as:
```
Options:
  -v, --verbose     Enable verbose output
  -d, --debug       Build in debug mode
  --no-cache        Disable build cache
  --skip-tests      Skip running tests
```

Note: The `None = 0` value is automatically excluded.

## Examples

### Simple Command

```csharp
[Description("echo", "Print a message")]
public class EchoCommand : Command<EchoCommand.Args>
{
    public class Args
    {
        [Description("message", "The message to print")]
        public string Message { get; set; } = string.Empty;

        [FlagAlias("uppercase", 'u')]
        [Description("Convert to uppercase")]
        public bool Uppercase { get; set; }
    }
}
```

Help output:
```
Usage: echo <message> [options]

Print a message

Arguments:
  <message>  The message to print

Options:
  -u, --uppercase  Convert to uppercase
```

### Multiple Arguments

```csharp
[Description("copy", "Copy a file")]
public class CopyCommand : Command<CopyCommand.Args>
{
    public class Args
    {
        [Description("source", "Source file path")]
        public string Source { get; set; } = string.Empty;

        [Description("destination", "Destination file path")]
        public string Destination { get; set; } = string.Empty;

        [FlagAlias("overwrite", 'o')]
        [Description("Overwrite if exists")]
        public bool Overwrite { get; set; }

        [FlagAlias("verbose", 'v')]
        [Description("Show detailed output")]
        public bool Verbose { get; set; }
    }
}
```

Help output:
```
Usage: copy <source> <destination> [options]

Copy a file

Arguments:
  <source>       Source file path
  <destination>  Destination file path

Options:
  -o, --overwrite  Overwrite if exists
  -v, --verbose    Show detailed output
```

### With Flags Enum

```csharp
[Description("build", "Build a project")]
public class BuildCommand : Command<BuildCommand.Args>
{
    [Flags]
    public enum BuildOptions
    {
        None = 0,
        [FlagAlias("verbose", 'v')]
        [Description("Enable verbose output")]
        Verbose = 1,
        [Description("Skip tests")]
        SkipTests = 2
    }

    public class Args
    {
        [Description("project", "Project name")]
        public string Project { get; set; } = string.Empty;

        public BuildOptions Options { get; set; }
    }
}
```

Help output:
```
Usage: build <project> [options]

Build a project

Arguments:
  <project>  Project name

Options:
  -v, --verbose   Enable verbose output
  --skip-tests    Skip tests
```

## Best Practices

1. **Write Clear Descriptions** - Make it obvious what each command/argument does
2. **Be Consistent** - Use similar language across all commands
3. **Keep It Concise** - Help text should be scannable
4. **Use Examples** - Show typical usage patterns
5. **Group Related Flags** - Use flags enums for related options

## Custom Help Command

You can implement a custom help command if needed:

```csharp
[Description("help", "Show help information")]
public class HelpCommand : Command<HelpCommand.Args>
{
    public class Args
    {
        [Description("command", "Command to get help for")]
        public string? CommandName { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        if (string.IsNullOrEmpty(args.CommandName))
        {
            // Show all commands
            Console.WriteLine("Available commands:");
            // ... list commands
        }
        else
        {
            // Show specific command help
            Console.WriteLine($"Help for: {args.CommandName}");
            // ... show command details
        }

        return Task.FromResult(0);
    }
}
```
