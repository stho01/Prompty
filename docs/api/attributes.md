# Attributes API

## DescriptionAttribute

Provides descriptions for commands, arguments, and enum fields.

### Definition

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public class DescriptionAttribute : Attribute
```

### Constructors

#### For Commands

```csharp
public DescriptionAttribute(string name, string description)
```

**Parameters:**
- `name` - The command name
- `description` - The command description

**Example:**
```csharp
[Description("greet", "Greets a person by name")]
public class GreetCommand : Command<GreetCommand.Args>
```

#### For Arguments and Enum Fields

```csharp
public DescriptionAttribute(string description)
```

**Parameters:**
- `description` - The description text

**Example:**
```csharp
[Description("Enable verbose output")]
public bool Verbose { get; set; }
```

#### For Positional Arguments

```csharp
public DescriptionAttribute(string name, string description)
```

**Parameters:**
- `name` - The argument name (used in help text)
- `description` - The argument description

**Example:**
```csharp
[Description("name", "The name of the person")]
public string Name { get; set; } = string.Empty;
```

### Properties

#### Description

The description text.

```csharp
public string Description { get; }
```

#### Name

The name (optional, for commands and positional arguments).

```csharp
public string? Name { get; set; }
```

### Usage Examples

#### On Commands

```csharp
[Description("build", "Build the project")]
public class BuildCommand : Command<BuildCommand.Args>
```

#### On Positional Arguments

```csharp
[Description("source", "The source file path")]
public string Source { get; set; } = string.Empty;
```

#### On Flags

```csharp
[FlagAlias("verbose", 'v')]
[Description("Enable verbose output")]
public bool Verbose { get; set; }
```

#### On Enum Fields

```csharp
[Flags]
public enum BuildOptions
{
    [Description("Enable verbose output")]
    Verbose = 1,

    [Description("Build in debug mode")]
    Debug = 2
}
```

## FlagAliasAttribute

Defines long and short aliases for flag arguments and enum fields.

### Definition

```csharp
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FlagAliasAttribute : Attribute
```

### Constructors

#### Long and Short Alias

```csharp
public FlagAliasAttribute(string longAlias, char shortAlias)
```

**Parameters:**
- `longAlias` - The long flag name (used as `--long-alias`)
- `shortAlias` - The short flag character (used as `-s`)

**Example:**
```csharp
[FlagAlias("verbose", 'v')]
public bool Verbose { get; set; }
```

Command line: `--verbose` or `-v`

#### Long Alias Only

```csharp
public FlagAliasAttribute(string longAlias)
```

**Parameters:**
- `longAlias` - The long flag name

**Example:**
```csharp
[FlagAlias("verbose")]
public bool Verbose { get; set; }
```

Command line: `--verbose`

#### Short Alias Only

```csharp
public FlagAliasAttribute(char shortAlias)
```

**Parameters:**
- `shortAlias` - The short flag character

**Example:**
```csharp
[FlagAlias('v')]
public bool Verbose { get; set; }
```

Command line: `-v`

### Properties

#### LongAlias

The long form of the flag.

```csharp
public string LongAlias { get; }
```

#### ShortAlias

The short form of the flag (optional).

```csharp
public char? ShortAlias { get; }
```

### Usage Examples

#### On Properties

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

#### On Enum Fields

```csharp
[Flags]
public enum BuildOptions
{
    [FlagAlias("verbose", 'v')]
    [Description("Enable verbose output")]
    Verbose = 1,

    [FlagAlias("debug", 'd')]
    [Description("Build in debug mode")]
    Debug = 2,

    // No FlagAlias - uses kebab-case: --no-cache
    NoCache = 4
}
```

### Automatic Kebab-Case

For `[Flags]` enum fields without `[FlagAlias]`, the field name is automatically converted to kebab-case:

| Field Name | Command Line Flag |
|------------|-------------------|
| `Verbose` | `--verbose` |
| `NoCache` | `--no-cache` |
| `SkipTests` | `--skip-tests` |
| `EnableFeatureX` | `--enable-feature-x` |

## Best Practices

### Naming Conventions

- Use lowercase for long aliases: `verbose`, `no-cache`
- Use single lowercase letter for short aliases: `v`, `d`
- Make names descriptive and clear

### Description Guidelines

- Start with a verb for commands: "Greets a person", "Builds the project"
- Be concise but clear
- Explain what the flag does, not how to use it

### Example

```csharp
[Description("build", "Build the project with specified options")]
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
        NoCache = 4
    }

    public class Args
    {
        [Description("project", "Path to project file")]
        public string Project { get; set; } = string.Empty;

        [FlagAlias("output", 'o')]
        [Description("Output directory")]
        public string? Output { get; set; }

        public BuildOptions Options { get; set; }
    }
}
```
