namespace Promty.Infrastructure;

public class CommandLineParser
{
    private readonly Dictionary<string, string?> _arguments = new();
    private readonly HashSet<string> _flags = new();
    private readonly List<string> _positionalArgs = new();

    public void Parse(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            // Long flag (--flag or --key=value)
            if (arg.StartsWith("--"))
            {
                var flagName = arg[2..];

                if (flagName.Contains('='))
                {
                    var parts = flagName.Split('=', 2);
                    _arguments[parts[0]] = parts[1];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    _arguments[flagName] = args[++i];
                }
                else
                {
                    _flags.Add(flagName);
                }
            }
            // Short flag (-f or -f value)
            else if (arg.StartsWith("-") && arg.Length > 1)
            {
                var flagName = arg[1..];

                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    _arguments[flagName] = args[++i];
                }
                else
                {
                    _flags.Add(flagName);
                }
            }
            // Positional argument
            else
            {
                _positionalArgs.Add(arg);
            }
        }
    }

    public bool HasFlag(string flag) => _flags.Contains(flag);
    public string? GetArgument(string key) => _arguments.GetValueOrDefault(key);

    public string GetArgument(string key, string defaultValue) => _arguments.GetValueOrDefault(key) ?? defaultValue;
    public IReadOnlyList<string> GetPositionalArguments() => _positionalArgs.AsReadOnly();

    public IReadOnlyDictionary<string, string?> GetAllArguments() => _arguments;

    public IReadOnlySet<string> GetAllFlags() => _flags;
}
