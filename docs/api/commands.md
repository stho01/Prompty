# Commands API

## Command\<TArgs\>

Base class for standard commands with typed argument binding.

### Definition

```csharp
public abstract class Command<TArgs> where TArgs : new()
```

### Methods

#### ExecuteAsync

Execute the command with bound arguments.

```csharp
public abstract Task<int> ExecuteAsync(TArgs args)
```

**Parameters:**
- `args` - The bound arguments instance

**Returns:**
- `Task<int>` - Exit code (0 for success, non-zero for error)

**Example:**
```csharp
public override Task<int> ExecuteAsync(Args args)
{
    Console.WriteLine($"Hello, {args.Name}!");
    return Task.FromResult(0);
}
```

### Usage

Create a command by extending `Command<TArgs>` (using generic type parameter):

```csharp
[Description("mycommand", "My command description")]
public class MyCommand : Command<MyCommand.Args>
{
    public class Args
    {
        // Define your arguments here
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        // Your command logic here
        return Task.FromResult(0);
    }
}
```

## ProcessCommand

Base class for commands that forward arguments to external executables.

### Definition

```csharp
public abstract class ProcessCommand
```

### Properties

#### ExecutablePath

The path to the executable to run.

```csharp
protected abstract string ExecutablePath { get; }
```

**Example:**
```csharp
protected override string ExecutablePath => "git";
```

### Usage

Create a process command by extending `ProcessCommand`:

```csharp
[Description("git", "Execute git commands")]
public class GitCommand : ProcessCommand
{
    protected override string ExecutablePath => "git";
}
```

All arguments are automatically forwarded to the executable.

## Exit Codes

Commands should return appropriate exit codes:

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | General error |
| `2+` | Application-specific errors |

**Example:**
```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (!File.Exists(args.FilePath))
    {
        Console.WriteLine("Error: File not found");
        return Task.FromResult(1); // Error
    }

    // Process file...
    return Task.FromResult(0); // Success
}
```

## Async Operations

Commands support asynchronous operations:

```csharp
public override async Task<int> ExecuteAsync(Args args)
{
    await SomeAsyncOperation();

    using var client = new HttpClient();
    var response = await client.GetAsync(args.Url);

    if (!response.IsSuccessStatusCode)
    {
        return 1; // Error
    }

    return 0; // Success
}
```

## Error Handling

Handle errors gracefully and return appropriate exit codes:

```csharp
public override async Task<int> ExecuteAsync(Args args)
{
    try
    {
        await ProcessAsync(args);
        return 0;
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine($"Error: File not found - {ex.Message}");
        return 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 2;
    }
}
```

## Type Parameters

### TArgs

The arguments class for your command. Must:
- Have a parameterless constructor
- Be a nested class within your command
- Have public properties for arguments

**Example:**
```csharp
public class Args
{
    [Description("name", "The name")]
    public string Name { get; set; } = string.Empty;

    [FlagAlias("verbose", 'v')]
    public bool Verbose { get; set; }
}
```
