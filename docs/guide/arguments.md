# Arguments & Flags

Promty supports two types of command-line arguments: **positional arguments** and **flag arguments**.

## Positional Arguments

Positional arguments are required and must be provided in order. They don't have a `[FlagAlias]` attribute.

```csharp
public class Args
{
    [Description("name", "The name argument")]
    public string Name { get; set; } = string.Empty;

    [Description("age", "The age argument")]
    public int Age { get; set; }
}
```

Usage:
```bash
dotnet run -- mycommand John 25
```

### Rules for Positional Arguments

1. Always **required** - must be provided
2. Order **matters** - must be provided in the order they're defined
3. Must come **before** any flags
4. Cannot be nullable

## Flag Arguments

Flag arguments are optional and can be provided in any order. They have a `[FlagAlias]` attribute.

```csharp
public class Args
{
    [FlagAlias("verbose", 'v')]
    [Description("Enable verbose output")]
    public bool Verbose { get; set; }

    [FlagAlias("port", 'p')]
    [Description("Port number")]
    public int? Port { get; set; }
}
```

Usage:
```bash
# Long form
dotnet run -- mycommand --verbose --port 8080

# Short form
dotnet run -- mycommand -v -p 8080

# Mixed
dotnet run -- mycommand --verbose -p 8080
```

## Supported Types

Promty automatically converts argument strings to the following types:

### Primitive Types
- `string`
- `int`
- `long`
- `double`
- `bool`

### Nullable Types
- `int?`
- `long?`
- `double?`
- `bool?`

### Enums
- Standard enums
- `[Flags]` enums (see [Flags Enums](/guide/flags-enums))

## Boolean Flags

Boolean flags have special behavior - they don't require a value:

```csharp
[FlagAlias("force", 'f')]
public bool Force { get; set; }
```

Both of these work:
```bash
dotnet run -- mycommand --force       # Implicitly true
dotnet run -- mycommand --force true  # Explicitly true
```

## Nullable Arguments

Use nullable types for optional flags with values:

```csharp
public class Args
{
    [FlagAlias("timeout", 't')]
    [Description("Timeout in seconds")]
    public int? Timeout { get; set; }
}
```

If not provided, the value will be `null`:
```csharp
public override Task<int> ExecuteAsync(Args args)
{
    var timeout = args.Timeout ?? 30; // Use 30 if not provided
    Console.WriteLine($"Timeout: {timeout}s");
    return Task.FromResult(0);
}
```

## Description Attribute

Use `[Description]` to provide help text:

### For Commands
```csharp
[Description("greet", "Greets a person by name")]
public class GreetCommand : Command<GreetCommand.Args>
```

### For Positional Arguments
```csharp
[Description("name", "The name of the person to greet")]
public string Name { get; set; }
```

### For Flags
```csharp
[FlagAlias("verbose", 'v')]
[Description("Enable verbose output")]
public bool Verbose { get; set; }
```

## FlagAlias Attribute

Define long and/or short aliases for flags:

### Both Long and Short
```csharp
[FlagAlias("verbose", 'v')]
```

### Long Only
```csharp
[FlagAlias("verbose")]
```

### Short Only
```csharp
[FlagAlias('v')]
```

## Examples

### Simple Command
```csharp
public class Args
{
    [Description("message", "The message to display")]
    public string Message { get; set; } = string.Empty;

    [FlagAlias("uppercase", 'u')]
    [Description("Convert to uppercase")]
    public bool Uppercase { get; set; }
}
```

```bash
dotnet run -- mycommand "Hello World" --uppercase
```

### Multiple Positional Arguments
```csharp
public class Args
{
    [Description("source", "Source file")]
    public string Source { get; set; } = string.Empty;

    [Description("destination", "Destination file")]
    public string Destination { get; set; } = string.Empty;

    [FlagAlias("overwrite", 'o')]
    [Description("Overwrite if exists")]
    public bool Overwrite { get; set; }
}
```

```bash
dotnet run -- copy input.txt output.txt --overwrite
```

### Mixed Types
```csharp
public class Args
{
    [Description("name", "Server name")]
    public string Name { get; set; } = string.Empty;

    [FlagAlias("port", 'p')]
    [Description("Port number")]
    public int? Port { get; set; }

    [FlagAlias("timeout", 't')]
    [Description("Timeout in seconds")]
    public double? Timeout { get; set; }

    [FlagAlias("verbose", 'v')]
    [Description("Verbose output")]
    public bool Verbose { get; set; }
}
```

```bash
dotnet run -- server MyServer -p 8080 -t 30.5 --verbose
```

## Validation

Add custom validation in your command:

```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (args.Port < 1 || args.Port > 65535)
    {
        Console.WriteLine("Error: Port must be between 1 and 65535");
        return Task.FromResult(1);
    }

    if (!File.Exists(args.Source))
    {
        Console.WriteLine($"Error: Source file '{args.Source}' not found");
        return Task.FromResult(1);
    }

    // Continue with valid arguments...
    return Task.FromResult(0);
}
```
