using Promty;
using Promty.Attributes;

namespace Commander.Commands;

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
        if (args.Verbose)
        {
            Console.WriteLine($"Copying from '{args.Source}' to '{args.Destination}'...");
        }

        if (File.Exists(args.Destination) && !args.Overwrite)
        {
            Console.WriteLine($"Error: Destination file '{args.Destination}' already exists. Use --overwrite to overwrite.");
            return Task.FromResult(1);
        }

        try
        {
            File.Copy(args.Source, args.Destination, args.Overwrite);

            if (args.Verbose)
            {
                Console.WriteLine("Copy completed successfully!");
            }

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return Task.FromResult(1);
        }
    }
}
