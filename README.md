<div align="center">
  <img src="gpx/logo6.png" alt="Promty Logo" width="200"/>

  # Promty

  [![NuGet Version](https://img.shields.io/nuget/v/Promty?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Promty)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Promty?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Promty)
  [![Build](https://img.shields.io/github/actions/workflow/status/stho01/promty/dotnet.yml?branch=main&style=flat-square&logo=github)](https://github.com/stho01/promty/actions)
  [![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)

  A powerful and flexible command-line parser and command executor framework for .NET applications. Build beautiful CLI tools with minimal boilerplate code.

  **[📚 Documentation](https://stho01.github.io/Promty/)** · **[🚀 Getting Started](https://stho01.github.io/Promty/guide/getting-started)** · **[💡 Examples](https://stho01.github.io/Promty/examples/basic)**
</div>

## Features

- 🎯 **Type-safe argument binding** - Automatically bind command-line arguments to strongly-typed classes
- 🚩 **Long and short flags** - Support for both `--verbose` and `-v` style flags
- 📝 **Automatic help generation** - Beautiful, table-formatted help text generated from attributes
- ⚡ **Process command execution** - Easily wrap external CLI tools
- 🎨 **Attribute-based configuration** - Use simple attributes to configure commands and arguments
- ✅ **Validation** - Automatic validation for required arguments
- 🔄 **Flexible architecture** - Extend `Command<TArgs>` or `ProcessCommand` base classes

## Installation

```bash
dotnet add package Promty
```

## Quick Start

### 1. Create a Command

```csharp
using Promty;

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

### 2. Set Up the Executor

```csharp
using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

return await executor.ExecuteAsync(args);
```

### 3. Run Your CLI

```bash
# Show available commands
dotnet run

# Run with arguments
dotnet run -- greet Alice --uppercase -r 3

# Output:
# HELLO, ALICE!
# HELLO, ALICE!
# HELLO, ALICE!
```

## Command Types

### Standard Commands

Standard commands use typed argument binding with automatic parsing and validation.

```csharp
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
        // Implementation here
        File.Copy(args.Source, args.Destination, args.Overwrite);
        return Task.FromResult(0);
    }
}
```

### Process Commands

Process commands forward all arguments to an external executable. Perfect for wrapping existing CLI tools.

```csharp
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

## Attributes

### DescriptionAttribute

Use `[Description]` for both commands and arguments:

**For Commands:**
```csharp
[Description("command-name", "Command description")]
public class MyCommand : Command<MyCommand.Args>
```

**For Positional Arguments:**
```csharp
[Description("arg-name", "Argument description")]
public string MyArgument { get; set; }
```

**For Flag Arguments:**
```csharp
[FlagAlias("verbose", 'v')]
[Description("Show detailed output")]
public bool Verbose { get; set; }
```

### FlagAliasAttribute

- **`[FlagAlias(long, short)]`** - Defines flag aliases
  - `[FlagAlias("verbose", 'v')]` - Both long and short
  - `[FlagAlias("verbose")]` - Long only
  - `[FlagAlias('v')]` - Short only

## Argument Types

Promty supports automatic type conversion for:

- `string`
- `int`, `long`, `double`
- `bool`
- Nullable versions: `int?`, `bool?`, etc.
- `[Flags]` enums (see below)

### Argument Rules

1. **Positional arguments** (without `[FlagAlias]` and not `[Flags]` enums) are **required** and must come **before** flags
2. **Flag arguments** (with `[FlagAlias]`) are **optional**
3. **`[Flags]` enum properties** are automatically treated as optional flags
4. Boolean flags don't require values: `--verbose` is equivalent to `--verbose true`

### Flags Enums

Instead of defining multiple boolean properties, you can use a `[Flags]` enum to group related flags together. Each enum value becomes an individual command-line flag that can be combined.

```csharp
[Description("build", "Build a project with options")]
public class BuildCommand : Command<BuildCommand.Args>
{
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
        [Description("project", "Project name")]
        public string Project { get; set; } = string.Empty;

        // No [FlagAlias] needed on the property!
        public BuildOptions Options { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine($"Building {args.Project}");

        if (args.Options.HasFlag(BuildOptions.Verbose))
            Console.WriteLine("Verbose mode enabled");

        if (args.Options.HasFlag(BuildOptions.Debug))
            Console.WriteLine("Debug mode enabled");

        return Task.FromResult(0);
    }
}
```

Usage:
```bash
# Combine multiple flags
dotnet run -- build MyProject --verbose --debug --skip-tests

# Use short aliases
dotnet run -- build MyProject -v -d

# Mix aliases with kebab-case names
dotnet run -- build MyProject -v --no-cache
```

**Flags Enum Features:**
- Each enum field becomes an individual flag in the help text
- Use `[FlagAlias]` on enum fields for custom long/short aliases
- Enum fields without `[FlagAlias]` auto-convert to kebab-case (e.g., `NoCache` → `--no-cache`)
- Use `[Description]` on enum fields to provide help text
- The `None = 0` value is automatically excluded from help output
- Multiple flags can be combined and are stored as a bitwise combination

## Help Text

Promty automatically generates beautiful help text:

```
Usage: greet <name> [options]

Greets a person by name

Arguments:
  <name>  The name of the person to greet

Options:
  -u, --uppercase          Print the greeting in uppercase
  -r, --repeat <repeat>    Number of times to repeat the greeting
```

For commands with `[Flags]` enums, each flag is displayed individually:

```
Usage: build <project> [options]

Build a project with options

Arguments:
  <project>  Project name

Options:
  -v, --verbose     Enable verbose output
  -d, --debug       Build in debug mode
  --no-cache        Disable build cache
  --skip-tests      Skip running tests
```

Command list is formatted as a table:

```
Available commands:

  build    Build a project with options
  copy     Copies a file from source to destination
  git      Execute git commands
  greet    Greets a person by name
```

## Error Handling

Return appropriate exit codes from your commands:

```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (!File.Exists(args.Source))
    {
        Console.WriteLine($"Error: Source file '{args.Source}' not found");
        return Task.FromResult(1); // Error exit code
    }

    // Success
    return Task.FromResult(0);
}
```

## Advanced Usage

### Multiple Assemblies

Register commands from multiple assemblies:

```csharp
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
executor.RegisterCommandsFromAssembly(typeof(PluginCommand).Assembly);

return await executor.ExecuteAsync(args);
```

### Custom Validation

Implement validation in your command:

```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (args.Port < 1 || args.Port > 65535)
    {
        Console.WriteLine("Error: Port must be between 1 and 65535");
        return Task.FromResult(1);
    }

    // Continue with valid arguments
}
```

## Examples

Check out the example commands in the repository:

- **GreetCommand** - Demonstrates typed arguments and flags
- **CopyCommand** - Shows file operations with validation
- **GitCommand** - Example of wrapping an external CLI tool
- **DotNetCommand** - Another process command example

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
