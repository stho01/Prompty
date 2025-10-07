# Process Wrapper Example

Examples of wrapping external CLI tools using `ProcessCommand`.

## Git Wrapper

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
dotnet run -- git add .
dotnet run -- git commit -m "Initial commit"
dotnet run -- git push origin main
```

## Docker Wrapper

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
dotnet run -- docker logs mycontainer
```

## Custom Build Script Wrapper

```csharp
[Description("build-legacy", "Run the legacy build script")]
public class BuildLegacyCommand : ProcessCommand
{
    protected override string ExecutablePath => "./scripts/build.sh";
}
```

Usage:
```bash
dotnet run -- build-legacy --target production
```

## Cross-Platform Python Wrapper

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

Usage:
```bash
dotnet run -- python script.py --arg value
dotnet run -- python -m pip install requests
```

## NPM Wrapper with Default Arguments

Override `GetArguments` to add default arguments:

```csharp
[Description("npm-install", "Install npm packages with preferred settings")]
public class NpmInstallCommand : ProcessCommand
{
    protected override string ExecutablePath => "npm";

    protected override string[] GetArguments(string[] args)
    {
        // Always use "npm install" with --save flag
        var defaultArgs = new[] { "install", "--save" };
        return defaultArgs.Concat(args).ToArray();
    }
}
```

Usage:
```bash
# Runs: npm install --save express
dotnet run -- npm-install express

# Runs: npm install --save express body-parser
dotnet run -- npm-install express body-parser
```

## Kubectl Wrapper with Namespace

```csharp
[Description("k8s", "Execute kubectl commands with default namespace")]
public class KubectlCommand : ProcessCommand
{
    protected override string ExecutablePath => "kubectl";

    protected override string[] GetArguments(string[] args)
    {
        // Add default namespace unless already specified
        if (!args.Any(a => a == "-n" || a == "--namespace"))
        {
            return new[] { "--namespace", "production" }
                .Concat(args)
                .ToArray();
        }
        return args;
    }
}
```

Usage:
```bash
# Uses production namespace by default
dotnet run -- k8s get pods

# Override with custom namespace
dotnet run -- k8s get pods -n staging
```

## Terraform Wrapper with Working Directory

```csharp
[Description("tf", "Execute Terraform commands in infrastructure directory")]
public class TerraformCommand : ProcessCommand
{
    protected override string ExecutablePath => "terraform";

    public override async Task<int> ExecuteAsync(string[] args)
    {
        // Change to infrastructure directory
        var originalDir = Directory.GetCurrentDirectory();
        var infraDir = Path.Combine(originalDir, "infrastructure");

        if (!Directory.Exists(infraDir))
        {
            Console.WriteLine($"Error: Infrastructure directory not found: {infraDir}");
            return 1;
        }

        try
        {
            Directory.SetCurrentDirectory(infraDir);
            Console.WriteLine($"Working directory: {infraDir}");
            return await base.ExecuteAsync(args);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
```

Usage:
```bash
dotnet run -- tf init
dotnet run -- tf plan
dotnet run -- tf apply -auto-approve
```

## Composite Command (Multiple Tools)

For complex workflows, use a standard command that runs multiple processes:

```csharp
[Description("deploy", "Build, test, and deploy the application")]
public class DeployCommand : Command<DeployCommand.Args>
{
    public class Args
    {
        [FlagAlias("environment", 'e')]
        [Description("Target environment (dev, staging, prod)")]
        public string Environment { get; set; } = "dev";

        [FlagAlias("skip-tests", 't')]
        [Description("Skip running tests")]
        public bool SkipTests { get; set; }
    }

    public override async Task<int> ExecuteAsync(Args args)
    {
        Console.WriteLine($"Deploying to: {args.Environment}");
        Console.WriteLine();

        // Step 1: Build
        Console.WriteLine("Step 1: Building...");
        var buildResult = await RunProcess("dotnet", "build -c Release");
        if (buildResult != 0)
        {
            Console.WriteLine("Build failed!");
            return buildResult;
        }
        Console.WriteLine("✓ Build successful");
        Console.WriteLine();

        // Step 2: Test (optional)
        if (!args.SkipTests)
        {
            Console.WriteLine("Step 2: Running tests...");
            var testResult = await RunProcess("dotnet", "test -c Release");
            if (testResult != 0)
            {
                Console.WriteLine("Tests failed!");
                return testResult;
            }
            Console.WriteLine("✓ Tests passed");
            Console.WriteLine();
        }

        // Step 3: Deploy
        Console.WriteLine($"Step 3: Deploying to {args.Environment}...");
        var deployResult = await RunProcess(
            "./deploy.sh",
            args.Environment
        );
        if (deployResult != 0)
        {
            Console.WriteLine("Deployment failed!");
            return deployResult;
        }
        Console.WriteLine("✓ Deployment successful");

        Console.WriteLine();
        Console.WriteLine($"Successfully deployed to {args.Environment}!");
        return 0;
    }

    private async Task<int> RunProcess(string executable, string arguments)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            }
        };

        process.Start();
        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
```

Usage:
```bash
# Deploy to dev
dotnet run -- deploy

# Deploy to production, skip tests
dotnet run -- deploy -e prod --skip-tests
```

Output:
```
Deploying to: prod

Step 1: Building...
✓ Build successful

Step 3: Deploying to prod...
✓ Deployment successful

Successfully deployed to prod!
```

## When to Use Process Commands

✅ **Use ProcessCommand when:**
- Wrapping existing CLI tools
- Forwarding all arguments unchanged
- Creating simple shortcuts
- No argument parsing needed

✅ **Use Standard Command when:**
- You need typed argument binding
- Custom validation required
- Running multiple processes
- Complex business logic

## Best Practices

1. **Check if executable exists** - Provide helpful error messages
2. **Handle exit codes** - Forward them appropriately
3. **Use full paths** - When executable not in PATH
4. **Cross-platform support** - Check OS when needed
5. **Add default arguments** - Override `GetArguments()` for convenience
