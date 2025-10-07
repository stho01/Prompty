# Flags Enums Example

A complete example showing how to use `[Flags]` enums to group related command-line flags.

## Build Command with Flags Enum

```csharp
using Promty;
using Promty.Attributes;

[Description("build", "Build a project with various options")]
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
        SkipTests = 8,

        [Description("Generate documentation")]
        GenerateDocs = 16,

        [FlagAlias("watch", 'w')]
        [Description("Watch for file changes and rebuild")]
        Watch = 32
    }

    public class Args
    {
        [Description("project", "Project file or directory to build")]
        public string Project { get; set; } = string.Empty;

        [FlagAlias("output", 'o')]
        [Description("Output directory")]
        public string? Output { get; set; }

        public BuildOptions Options { get; set; }
    }

    public override async Task<int> ExecuteAsync(Args args)
    {
        // Display what we're building
        Console.WriteLine($"Building: {args.Project}");

        if (!string.IsNullOrEmpty(args.Output))
            Console.WriteLine($"Output directory: {args.Output}");

        Console.WriteLine();

        // Show selected options
        if (args.Options != BuildOptions.None)
        {
            Console.WriteLine("Build options:");

            if (args.Options.HasFlag(BuildOptions.Verbose))
                Console.WriteLine("  ✓ Verbose output enabled");

            if (args.Options.HasFlag(BuildOptions.Debug))
                Console.WriteLine("  ✓ Debug mode");

            if (args.Options.HasFlag(BuildOptions.NoCache))
                Console.WriteLine("  ✓ Cache disabled");

            if (args.Options.HasFlag(BuildOptions.SkipTests))
                Console.WriteLine("  ✓ Tests will be skipped");

            if (args.Options.HasFlag(BuildOptions.GenerateDocs))
                Console.WriteLine("  ✓ Documentation will be generated");

            if (args.Options.HasFlag(BuildOptions.Watch))
                Console.WriteLine("  ✓ Watch mode enabled");

            Console.WriteLine();
        }

        // Simulate build process
        Console.WriteLine("Starting build...");

        if (args.Options.HasFlag(BuildOptions.Verbose))
        {
            Console.WriteLine("  Restoring packages...");
            await Task.Delay(500);
            Console.WriteLine("  Compiling source files...");
            await Task.Delay(500);
            Console.WriteLine("  Linking assemblies...");
            await Task.Delay(500);
        }
        else
        {
            await Task.Delay(1500);
        }

        Console.WriteLine("Build completed successfully!");

        // Run tests unless skipped
        if (!args.Options.HasFlag(BuildOptions.SkipTests))
        {
            Console.WriteLine();
            Console.WriteLine("Running tests...");
            await Task.Delay(500);
            Console.WriteLine("All tests passed!");
        }

        // Generate docs if requested
        if (args.Options.HasFlag(BuildOptions.GenerateDocs))
        {
            Console.WriteLine();
            Console.WriteLine("Generating documentation...");
            await Task.Delay(500);
            Console.WriteLine("Documentation generated!");
        }

        // Watch mode
        if (args.Options.HasFlag(BuildOptions.Watch))
        {
            Console.WriteLine();
            Console.WriteLine("Watching for changes... (Press Ctrl+C to stop)");
            // In real implementation, set up file watcher here
        }

        return 0;
    }
}
```

## Usage Examples

### Basic build
```bash
dotnet run -- build MyProject.csproj
```
Output:
```
Building: MyProject.csproj

Starting build...
Build completed successfully!

Running tests...
All tests passed!
```

### With verbose flag
```bash
dotnet run -- build MyProject.csproj --verbose
```
Output:
```
Building: MyProject.csproj

Build options:
  ✓ Verbose output enabled

Starting build...
  Restoring packages...
  Compiling source files...
  Linking assemblies...
Build completed successfully!

Running tests...
All tests passed!
```

### Multiple flags using aliases
```bash
dotnet run -- build MyProject.csproj -v -d --skip-tests
```
Output:
```
Building: MyProject.csproj

Build options:
  ✓ Verbose output enabled
  ✓ Debug mode
  ✓ Tests will be skipped

Starting build...
  Restoring packages...
  Compiling source files...
  Linking assemblies...
Build completed successfully!
```

### Kebab-case flags
```bash
dotnet run -- build MyProject.csproj --no-cache --generate-docs
```
Output:
```
Building: MyProject.csproj

Build options:
  ✓ Cache disabled
  ✓ Documentation will be generated

Starting build...
Build completed successfully!

Running tests...
All tests passed!

Generating documentation...
Documentation generated!
```

### All options
```bash
dotnet run -- build MyProject.csproj -v -d --no-cache --skip-tests --generate-docs --watch -o ./dist
```
Output:
```
Building: MyProject.csproj
Output directory: ./dist

Build options:
  ✓ Verbose output enabled
  ✓ Debug mode
  ✓ Cache disabled
  ✓ Tests will be skipped
  ✓ Documentation will be generated
  ✓ Watch mode enabled

Starting build...
  Restoring packages...
  Compiling source files...
  Linking assemblies...
Build completed successfully!

Generating documentation...
Documentation generated!

Watching for changes... (Press Ctrl+C to stop)
```

## Help Text

```bash
dotnet run -- build --help
```

Output:
```
Usage: build <project> [options]

Build a project with various options

Arguments:
  <project>  Project file or directory to build

Options:
  -o, --output <output>  Output directory
  -v, --verbose          Enable verbose output
  -d, --debug            Build in debug mode
  --no-cache             Disable build cache
  --skip-tests           Skip running tests
  --generate-docs        Generate documentation
  -w, --watch            Watch for file changes and rebuild
```

## Checking Multiple Flags

You can check for multiple flags at once:

```csharp
// Check if either verbose or debug is set
if (args.Options.HasFlag(BuildOptions.Verbose) ||
    args.Options.HasFlag(BuildOptions.Debug))
{
    Console.WriteLine("Detailed logging enabled");
}

// Check if both verbose and debug are set
if (args.Options.HasFlag(BuildOptions.Verbose | BuildOptions.Debug))
{
    Console.WriteLine("Maximum verbosity");
}

// Get raw value
var optionsValue = (int)args.Options;
Console.WriteLine($"Raw options value: {optionsValue}");
```

## Key Features Demonstrated

- ✅ `[Flags]` enum with multiple values
- ✅ Mix of `[FlagAlias]` and auto-kebab-case
- ✅ Descriptions on enum fields
- ✅ Checking individual flags with `HasFlag()`
- ✅ Combining short and long aliases
- ✅ None value handling
- ✅ Async command execution
- ✅ Conditional logic based on flags

## Real-World Use Cases

This pattern is perfect for:

1. **Build Tools** - Compilation options, optimization levels
2. **Server Commands** - Feature flags, protocol options
3. **Deploy Tools** - Deployment strategies, rollout options
4. **Test Runners** - Test categories, reporting options
5. **File Processors** - Processing modes, output formats

## Advantages Over Multiple Booleans

**Before (8 boolean properties):**
```csharp
public bool Verbose { get; set; }
public bool Debug { get; set; }
public bool NoCache { get; set; }
public bool SkipTests { get; set; }
public bool GenerateDocs { get; set; }
public bool Watch { get; set; }
// ... more flags
```

**After (1 enum property):**
```csharp
public BuildOptions Options { get; set; }
```

Benefits:
- Less code to write
- Related options grouped together
- Easy to add new options
- Automatic help text
- Type-safe combinations
