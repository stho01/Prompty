# Flags Enums

Instead of defining multiple boolean properties, you can use a `[Flags]` enum to group related flags together. Each enum value becomes an individual command-line flag that can be combined.

## Why Use Flags Enums?

**Before (Multiple Booleans):**
```csharp
public class Args
{
    [FlagAlias("verbose", 'v')]
    public bool Verbose { get; set; }

    [FlagAlias("debug", 'd')]
    public bool Debug { get; set; }

    [FlagAlias("no-cache")]
    public bool NoCache { get; set; }

    [FlagAlias("skip-tests")]
    public bool SkipTests { get; set; }
}
```

**After (Flags Enum):**
```csharp
[Flags]
public enum BuildOptions
{
    None = 0,
    [FlagAlias("verbose", 'v')]
    Verbose = 1,
    [FlagAlias("debug", 'd')]
    Debug = 2,
    NoCache = 4,
    SkipTests = 8
}

public class Args
{
    public BuildOptions Options { get; set; }
}
```

## Basic Usage

### Define a Flags Enum

```csharp
using Promty;
using Promty.Attributes;

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

        if (args.Options.HasFlag(BuildOptions.NoCache))
            Console.WriteLine("Cache disabled");

        if (args.Options.HasFlag(BuildOptions.SkipTests))
            Console.WriteLine("Tests skipped");

        return Task.FromResult(0);
    }
}
```

### Command-Line Usage

```bash
# Combine multiple flags
dotnet run -- build MyProject --verbose --debug --skip-tests

# Use short aliases
dotnet run -- build MyProject -v -d

# Mix aliases with kebab-case names
dotnet run -- build MyProject -v --no-cache

# Use no flags
dotnet run -- build MyProject
```

## Features

### 1. Custom Aliases with FlagAlias

Define custom long and short aliases:

```csharp
[FlagAlias("verbose", 'v')]
[Description("Enable verbose output")]
Verbose = 1,
```

Command line:
```bash
--verbose  # or -v
```

### 2. Automatic Kebab-Case Conversion

Enum fields without `[FlagAlias]` automatically convert to kebab-case:

```csharp
NoCache = 4,        // Becomes: --no-cache
SkipTests = 8,      // Becomes: --skip-tests
EnableFeatureX = 16 // Becomes: --enable-feature-x
```

### 3. Descriptions on Enum Fields

Add `[Description]` to enum fields for help text:

```csharp
[Description("Enable verbose output")]
Verbose = 1,

[Description("Disable build cache")]
NoCache = 4,
```

These appear in the help text:
```
Options:
  -v, --verbose    Enable verbose output
  --no-cache       Disable build cache
```

### 4. None Value Excluded

The `None = 0` value is automatically excluded from help text and command-line parsing.

## Checking Flags in Code

Use `HasFlag()` to check if a flag is set:

```csharp
public override Task<int> ExecuteAsync(Args args)
{
    if (args.Options.HasFlag(BuildOptions.Verbose))
    {
        Console.WriteLine("Verbose mode enabled");
    }

    if (args.Options.HasFlag(BuildOptions.Debug))
    {
        Console.WriteLine("Debug mode enabled");
    }

    // Check for multiple flags
    if (args.Options.HasFlag(BuildOptions.Debug | BuildOptions.NoCache))
    {
        Console.WriteLine("Debug mode with no cache");
    }

    // Check if no flags are set
    if (args.Options == BuildOptions.None)
    {
        Console.WriteLine("No options specified");
    }

    return Task.FromResult(0);
}
```

## Complete Example

```csharp
using Promty;
using Promty.Attributes;

[Description("serve", "Start a development server")]
public class ServeCommand : Command<ServeCommand.Args>
{
    [Flags]
    public enum ServerOptions
    {
        None = 0,
        [FlagAlias("watch", 'w')]
        [Description("Watch for file changes")]
        Watch = 1,
        [FlagAlias("open", 'o')]
        [Description("Open browser automatically")]
        OpenBrowser = 2,
        [Description("Enable HTTPS")]
        Https = 4,
        [Description("Enable hot module replacement")]
        HotReload = 8,
        [Description("Show debug information")]
        Debug = 16
    }

    public class Args
    {
        [Description("port", "Port number")]
        public int Port { get; set; } = 3000;

        public ServerOptions Options { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine($"Starting server on port {args.Port}");

        if (args.Options.HasFlag(ServerOptions.Watch))
            Console.WriteLine("File watching enabled");

        if (args.Options.HasFlag(ServerOptions.OpenBrowser))
            Console.WriteLine("Opening browser...");

        if (args.Options.HasFlag(ServerOptions.Https))
            Console.WriteLine("HTTPS enabled");

        if (args.Options.HasFlag(ServerOptions.HotReload))
            Console.WriteLine("Hot reload enabled");

        if (args.Options.HasFlag(ServerOptions.Debug))
            Console.WriteLine("Debug mode enabled");

        // Start server logic...
        return Task.FromResult(0);
    }
}
```

Usage:
```bash
dotnet run -- serve 8080 --watch --open --https
dotnet run -- serve 3000 -w -o --hot-reload --debug
```

## Help Text Generation

The help text automatically displays each flag individually:

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

## Best Practices

1. **Always Start with None = 0** - Required for flags enums
2. **Use Powers of 2** - Values should be 1, 2, 4, 8, 16, 32, etc.
3. **Add Descriptions** - Help users understand each flag
4. **Use FlagAlias for Common Flags** - Provide short aliases for frequently used flags
5. **Group Related Flags** - Keep related functionality together in one enum
6. **Use Descriptive Names** - Make it clear what each flag does

## Advantages

✅ **Less Boilerplate** - One enum vs multiple boolean properties
✅ **Better Organization** - Related flags grouped together
✅ **Type Safety** - Enum values are compile-time checked
✅ **Easy to Extend** - Add new flags by adding enum values
✅ **Automatic Help Text** - Each flag appears individually
✅ **Flexible Naming** - Use FlagAlias or auto-kebab-case
