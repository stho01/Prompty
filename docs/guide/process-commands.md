# Process Commands

Process commands allow you to easily wrap external CLI tools and forward all arguments to them. This is perfect for creating shortcuts, wrappers, or composite tools.

## Basic Usage

Extend `ProcessCommand` and specify the executable path:

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
dotnet run -- git push origin main
```

All arguments after `git` are forwarded directly to the git executable.

## Examples

### Docker Wrapper

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
dotnet run -- docker run -p 8080:80 myapp
```

### .NET CLI Wrapper

```csharp
[Description("dotnet", "Execute dotnet CLI commands")]
public class DotNetCommand : ProcessCommand
{
    protected override string ExecutablePath => "dotnet";
}
```

Usage:
```bash
dotnet run -- dotnet build
dotnet run -- dotnet test
dotnet run -- dotnet pack
```

### NPM Wrapper

```csharp
[Description("npm", "Execute npm commands")]
public class NpmCommand : ProcessCommand
{
    protected override string ExecutablePath => "npm";
}
```

Usage:
```bash
dotnet run -- npm install
dotnet run -- npm run build
dotnet run -- npm test
```

## With Full Paths

If the executable is not in the system PATH, specify the full path:

```csharp
[Description("custom", "Execute custom tool")]
public class CustomCommand : ProcessCommand
{
    protected override string ExecutablePath =>
        "/usr/local/bin/custom-tool"; // Unix
        // or
        // @"C:\Tools\custom-tool.exe"; // Windows
}
```

## Cross-Platform Paths

For cross-platform support:

```csharp
[Description("python", "Execute Python scripts")]
public class PythonCommand : ProcessCommand
{
    protected override string ExecutablePath =>
        Environment.OSVersion.Platform == PlatformID.Win32NT
            ? "python.exe"
            : "python3";
}
```

## Use Cases

### 1. Create Shortcuts

Simplify complex commands:

```csharp
[Description("build-all", "Build all projects")]
public class BuildAllCommand : ProcessCommand
{
    protected override string ExecutablePath => "dotnet";

    // Override to add default arguments
    protected override string[] GetArguments(string[] args)
    {
        return new[] { "build", "-c", "Release" }
            .Concat(args)
            .ToArray();
    }
}
```

### 2. Wrap External Tools

Make external tools part of your CLI:

```csharp
[Description("format", "Format code with Prettier")]
public class FormatCommand : ProcessCommand
{
    protected override string ExecutablePath => "npx";

    protected override string[] GetArguments(string[] args)
    {
        return new[] { "prettier", "--write", "." }
            .Concat(args)
            .ToArray();
    }
}
```

### 3. Create Composite Commands

Chain multiple operations:

```csharp
[Description("deploy", "Build and deploy the application")]
public class DeployCommand : Command<DeployCommand.Args>
{
    public class Args { }

    public override async Task<int> ExecuteAsync(Args args)
    {
        // Build
        Console.WriteLine("Building...");
        var buildResult = await RunProcessAsync("dotnet", "build -c Release");
        if (buildResult != 0) return buildResult;

        // Test
        Console.WriteLine("Testing...");
        var testResult = await RunProcessAsync("dotnet", "test");
        if (testResult != 0) return testResult;

        // Deploy
        Console.WriteLine("Deploying...");
        var deployResult = await RunProcessAsync("./deploy.sh");
        return deployResult;
    }

    private async Task<int> RunProcessAsync(string executable, string args = "")
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = executable,
                Arguments = args,
                UseShellExecute = false
            }
        };
        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
```

## Exit Codes

Process commands automatically return the exit code from the external process:

- `0` - Process succeeded
- Non-zero - Process failed (specific meaning depends on the tool)

## Standard Output/Error

By default, process commands:
- Forward stdout to your console
- Forward stderr to your console
- Wait for the process to complete
- Return the exit code

## Advantages

✅ **No Argument Parsing** - All arguments forwarded as-is
✅ **Simple Setup** - Just specify the executable path
✅ **Exit Code Propagation** - Exit codes automatically forwarded
✅ **Console Integration** - Output appears directly in your console
✅ **Quick Wrappers** - Perfect for creating shortcuts and aliases

## When to Use

**Use Process Commands when:**
- Wrapping existing CLI tools
- Creating shortcuts for complex commands
- Building composite workflows
- Forwarding all arguments to external tools

**Use Standard Commands when:**
- You need typed argument binding
- You want validation and help text
- Building custom business logic
- Processing arguments in code
