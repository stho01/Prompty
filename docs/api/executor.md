# Command Executor API

## CommandExecutor

The main class responsible for registering and executing commands.

### Definition

```csharp
public class CommandExecutor
```

### Constructor

```csharp
public CommandExecutor()
```

Creates a new instance of the command executor.

**Example:**
```csharp
var executor = new CommandExecutor();
```

### Methods

#### RegisterCommandsFromAssembly

Registers all commands from the specified assembly.

```csharp
public void RegisterCommandsFromAssembly(Assembly assembly)
```

**Parameters:**
- `assembly` - The assembly to scan for commands

**Example:**
```csharp
using System.Reflection;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
```

#### ExecuteAsync

Executes a command based on the provided command-line arguments.

```csharp
public Task<int> ExecuteAsync(string[] args)
```

**Parameters:**
- `args` - Command-line arguments (typically from `Main`)

**Returns:**
- `Task<int>` - Exit code from the executed command

**Example:**
```csharp
public static async Task<int> Main(string[] args)
{
    var executor = new CommandExecutor();
    executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

    return await executor.ExecuteAsync(args);
}
```

## Usage Patterns

### Basic Setup

```csharp
using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

return await executor.ExecuteAsync(args);
```

### Multiple Assemblies

Register commands from multiple assemblies:

```csharp
var executor = new CommandExecutor();

// Register commands from main assembly
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

// Register commands from plugin assembly
executor.RegisterCommandsFromAssembly(typeof(PluginCommand).Assembly);

// Register commands from external library
executor.RegisterCommandsFromAssembly(
    Assembly.LoadFrom("MyCommands.dll")
);

return await executor.ExecuteAsync(args);
```

### With Error Handling

```csharp
var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

try
{
    return await executor.ExecuteAsync(args);
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    return 1;
}
```

### In ASP.NET Core

```csharp
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Check if running as CLI
        if (args.Length > 0 && args[0] == "cli")
        {
            var executor = new CommandExecutor();
            executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
            return await executor.ExecuteAsync(args.Skip(1).ToArray());
        }

        // Otherwise start web host
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
        return 0;
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

## Behavior

### Command Discovery

The executor automatically discovers all classes that:
- Inherit from `Command<TArgs>`
- Inherit from `ProcessCommand`
- Are in the registered assemblies
- Have a `[Description]` attribute

### Command Name Resolution

Command names are determined by the `[Description]` attribute:

```csharp
[Description("my-command", "Description")]
public class MyCommand : Command<MyCommand.Args>
```

The first parameter (`"my-command"`) becomes the command name.

### No Command Provided

When no command is provided:
```bash
dotnet run
```

The executor displays a list of all available commands.

### Unknown Command

When an unknown command is provided:
```bash
dotnet run -- unknown-command
```

The executor displays an error and the command list.

### Missing Arguments

When required arguments are missing:
```bash
dotnet run -- mycommand
```

The executor displays command-specific help text.

## Return Values

The `ExecuteAsync` method returns the exit code:

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error (command not found, missing arguments, etc.) |
| Other | Command-specific exit code |

## Example Program

Complete example `Program.cs`:

```csharp
using System.Reflection;
using Promty;

// Create and configure executor
var executor = new CommandExecutor();

// Register all commands in this assembly
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

// Execute command and return exit code
return await executor.ExecuteAsync(args);
```

## Top-Level Statements

Promty works great with C# top-level statements:

```csharp
using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
return await executor.ExecuteAsync(args);
```

## Dependency Injection

For advanced scenarios with dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
var serviceProvider = services.BuildServiceProvider();

// Commands can resolve services via constructor injection
var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());

return await executor.ExecuteAsync(args);
```

::: warning
Built-in dependency injection support is not currently available. Commands must manage their own dependencies.
:::
