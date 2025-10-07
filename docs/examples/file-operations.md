# File Operations Example

A real-world example demonstrating file operations with proper validation and error handling.

## File Copy Command

```csharp
using Promty;
using Promty.Attributes;

[Description("copy", "Copy a file from source to destination")]
public class CopyCommand : Command<CopyCommand.Args>
{
    public class Args
    {
        [Description("source", "The source file path")]
        public string Source { get; set; } = string.Empty;

        [Description("destination", "The destination file path")]
        public string Destination { get; set; } = string.Empty;

        [FlagAlias("overwrite", 'o')]
        [Description("Overwrite destination if it exists")]
        public bool Overwrite { get; set; }

        [FlagAlias("verbose", 'v')]
        [Description("Show detailed output")]
        public bool Verbose { get; set; }

        [FlagAlias("create-dirs", 'c')]
        [Description("Create destination directories if they don't exist")]
        public bool CreateDirectories { get; set; }
    }

    public override Task<int> ExecuteAsync(Args args)
    {
        try
        {
            // Validate source file exists
            if (!File.Exists(args.Source))
            {
                Console.WriteLine($"Error: Source file '{args.Source}' does not exist");
                return Task.FromResult(1);
            }

            // Check if destination exists
            if (File.Exists(args.Destination) && !args.Overwrite)
            {
                Console.WriteLine($"Error: Destination file '{args.Destination}' already exists");
                Console.WriteLine("Use --overwrite to replace it");
                return Task.FromResult(1);
            }

            // Create destination directory if needed
            var destDir = Path.GetDirectoryName(args.Destination);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                if (args.CreateDirectories)
                {
                    if (args.Verbose)
                        Console.WriteLine($"Creating directory: {destDir}");

                    Directory.CreateDirectory(destDir);
                }
                else
                {
                    Console.WriteLine($"Error: Destination directory '{destDir}' does not exist");
                    Console.WriteLine("Use --create-dirs to create it automatically");
                    return Task.FromResult(1);
                }
            }

            // Perform the copy
            if (args.Verbose)
            {
                Console.WriteLine($"Copying: {args.Source}");
                Console.WriteLine($"To:      {args.Destination}");

                if (args.Overwrite && File.Exists(args.Destination))
                    Console.WriteLine("Overwriting existing file");
            }

            File.Copy(args.Source, args.Destination, args.Overwrite);

            // Success message
            if (args.Verbose)
            {
                var fileInfo = new FileInfo(args.Destination);
                Console.WriteLine($"Success! Copied {fileInfo.Length:N0} bytes");
            }
            else
            {
                Console.WriteLine($"Copied: {Path.GetFileName(args.Source)} -> {Path.GetFileName(args.Destination)}");
            }

            return Task.FromResult(0);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: Access denied. Check file permissions.");
            return Task.FromResult(1);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Task.FromResult(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return Task.FromResult(2);
        }
    }
}
```

## Usage Examples

### Basic copy
```bash
dotnet run -- copy input.txt output.txt
```

### Copy with overwrite
```bash
dotnet run -- copy source.txt destination.txt --overwrite
```

### Verbose output
```bash
dotnet run -- copy file.txt backup.txt -v
```
Output:
```
Copying: file.txt
To:      backup.txt
Success! Copied 1,234 bytes
```

### Create destination directories
```bash
dotnet run -- copy data.csv backups/2024/data.csv --create-dirs -v
```
Output:
```
Creating directory: backups/2024
Copying: data.csv
To:      backups/2024/data.csv
Success! Copied 5,678 bytes
```

### Combined flags
```bash
dotnet run -- copy app.log logs/$(date +%Y-%m-%d)/app.log -o -c -v
```

## Error Handling

### Source file doesn't exist
```bash
dotnet run -- copy missing.txt output.txt
```
Output:
```
Error: Source file 'missing.txt' does not exist
```
Exit code: `1`

### Destination already exists
```bash
dotnet run -- copy input.txt existing.txt
```
Output:
```
Error: Destination file 'existing.txt' already exists
Use --overwrite to replace it
```
Exit code: `1`

### Destination directory doesn't exist
```bash
dotnet run -- copy file.txt folder/file.txt
```
Output:
```
Error: Destination directory 'folder' does not exist
Use --create-dirs to create it automatically
```
Exit code: `1`

### Permission denied
```bash
dotnet run -- copy file.txt /root/protected.txt
```
Output:
```
Error: Access denied. Check file permissions.
```
Exit code: `1`

## Help Text

```bash
dotnet run -- copy --help
```

Output:
```
Usage: copy <source> <destination> [options]

Copy a file from source to destination

Arguments:
  <source>       The source file path
  <destination>  The destination file path

Options:
  -o, --overwrite     Overwrite destination if it exists
  -v, --verbose       Show detailed output
  -c, --create-dirs   Create destination directories if they don't exist
```

## Key Features Demonstrated

- ✅ Multiple positional arguments
- ✅ Input validation
- ✅ Comprehensive error handling
- ✅ User-friendly error messages
- ✅ Verbose mode for debugging
- ✅ Proper exit codes
- ✅ File system operations
- ✅ Helpful usage hints

## Extended Features

You could extend this command with:

- Progress reporting for large files
- Recursive directory copying
- Pattern matching (wildcards)
- Preserve file attributes
- Dry-run mode
- Backup existing files

Example with dry-run:

```csharp
[FlagAlias("dry-run", 'n')]
[Description("Show what would be copied without actually copying")]
public bool DryRun { get; set; }

// In ExecuteAsync:
if (args.DryRun)
{
    Console.WriteLine("DRY RUN - No files will be copied");
    Console.WriteLine($"Would copy: {args.Source} -> {args.Destination}");
    return Task.FromResult(0);
}
```
