# Commands

Commands are the core building blocks of your CLI application. Promty provides two types of commands:

## Standard Commands

Standard commands extend `Command<TArgs>` and provide full type-safe argument binding.

### Basic Structure

```csharp
[Description("command-name", "Command description")]
public class MyCommand : Command<MyCommand.Args>
{
    public class Args
    {
        // Define your arguments here
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        // Your command logic here
        return Task.FromResult(0); // Return exit code
    }
}
```

### Example: File Copy Command

```csharp
using Promty;
using Promty.Attributes;

[Description("copy", "Copies a file from source to destination")]
public class CopyCommand : Command<CopyCommand.Args>
{
    public class Args
    {
        [Description("source", "The source file path")]
        public string Source { get; set; } = string.Empty;

        [Description("destination", "The destination file path")]
        public string Destination { get; set; } = string.Empty;

        [FlagAlias("verbose", 'v')]
        [Description("Show detailed output")]
        public bool Verbose { get; set; }

        [FlagAlias("overwrite", 'o')]
        [Description("Overwrite existing files")]
        public bool Overwrite { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        if (args.Verbose)
        {
            Console.WriteLine($"Copying {args.Source} to {args.Destination}");
        }

        try
        {
            File.Copy(args.Source, args.Destination, args.Overwrite);

            if (args.Verbose)
            {
                Console.WriteLine("Copy completed successfully");
            }

            return Task.FromResult(0); // Success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Task.FromResult(1); // Error
        }
    }
}
```

Usage:
```bash
dotnet run -- copy input.txt output.txt --verbose --overwrite
```

## Process Commands

Process commands extend `ProcessCommand` and forward all arguments to an external executable.

### Basic Structure

```csharp
[Description("command-name", "Command description")]
public class MyProcessCommand : ProcessCommand
{
    protected override string ExecutablePath => "executable-name";
}
```

### Example: Git Wrapper

```csharp
using Promty;
using Promty.Attributes;

[Description("git", "Execute git commands")]
public class GitCommand : ProcessCommand
{
    protected override string ExecutablePath => "git";
}
```

Usage:
```bash
dotnet run -- git status
dotnet run -- git commit -m "Initial commit"
dotnet run -- git --version
```

### Example: Docker Wrapper

```csharp
[Description("docker", "Execute docker commands")]
public class DockerCommand : ProcessCommand
{
    protected override string ExecutablePath => "docker";
}
```

Usage:
```bash
dotnet run -- docker ps
dotnet run -- docker build -t myapp .
```

## Command Registration

Register your commands in `Program.cs`:

### Register from Assembly

```csharp
var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
```

### Register Multiple Assemblies

```csharp
var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
executor.RegisterCommandsFromAssembly(typeof(PluginCommand).Assembly);
```

## Exit Codes

Return appropriate exit codes from your commands:

- `0` - Success
- `1` - General error
- Other codes - Application-specific errors

```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (!File.Exists(args.Source))
    {
        Console.WriteLine($"Error: Source file '{args.Source}' not found");
        return Task.FromResult(1); // Error exit code
    }

    // Success logic...
    return Task.FromResult(0); // Success exit code
}
```

## Best Practices

1. **Keep Commands Focused** - Each command should do one thing well
2. **Use Descriptive Names** - Make command names clear and intuitive
3. **Provide Good Descriptions** - Help users understand what each command does
4. **Validate Input** - Check for invalid arguments and provide helpful error messages
5. **Return Proper Exit Codes** - Follow standard conventions for exit codes
