using System.Reflection;
using System.Text;
using Promty.Attributes;

namespace Promty.Infrastructure;

internal sealed class HelpTextGenerator
{
    public string GenerateCommandHelp(Type commandType, string commandName)
    {
        var sb = new StringBuilder();

        // Get command description
        var commandDesc = commandType.GetCustomAttribute<DescriptionAttribute>();

        sb.AppendLine($"Usage: {commandName} {GenerateUsageLine(commandType)}");
        sb.AppendLine();

        if (commandDesc?.Description != null)
        {
            sb.AppendLine(commandDesc.Description);
            sb.AppendLine();
        }

        // Get args type
        var baseType = commandType.BaseType;
        if (baseType == null || !baseType.IsGenericType)
        {
            return sb.ToString();
        }

        var argsType = baseType.GetGenericArguments()[0];
        var properties = argsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Separate into positional and flags
        var positionalProps = new List<PropertyInfo>();
        var flagProps = new List<PropertyInfo>();

        foreach (var prop in properties)
        {
            var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>();
            if (flagAlias != null)
            {
                flagProps.Add(prop);
            }
            else
            {
                positionalProps.Add(prop);
            }
        }

        // Show positional arguments
        if (positionalProps.Count > 0)
        {
            sb.AppendLine("Arguments:");
            foreach (var prop in positionalProps)
            {
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                var argName = descAttr?.Name ?? prop.Name.ToLower();

                sb.Append($"  <{argName}>");
                if (descAttr != null)
                {
                    sb.Append($"  {descAttr.Description}");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        // Show optional flags
        if (flagProps.Count > 0)
        {
            sb.AppendLine("Options:");
            foreach (var prop in flagProps)
            {
                var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>()!;
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();

                var aliases = new List<string>();
                if (flagAlias.ShortAlias.HasValue)
                {
                    aliases.Add($"-{flagAlias.ShortAlias.Value}");
                }
                if (!string.IsNullOrEmpty(flagAlias.LongAlias))
                {
                    aliases.Add($"--{flagAlias.LongAlias}");
                }

                var aliasText = string.Join(", ", aliases);

                // Add value placeholder for non-bool types
                var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (propertyType != typeof(bool))
                {
                    aliasText += $" <{prop.Name.ToLower()}>";
                }

                sb.Append($"  {aliasText}");
                if (descAttr != null)
                {
                    sb.Append($"  {descAttr.Description}");
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private string GenerateUsageLine(Type commandType)
    {
        var baseType = commandType.BaseType;
        if (baseType == null || !baseType.IsGenericType)
        {
            return string.Empty;
        }

        var argsType = baseType.GetGenericArguments()[0];
        var properties = argsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var parts = new List<string>();

        // Add positional arguments first
        foreach (var prop in properties)
        {
            var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>();
            if (flagAlias == null)
            {
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                var argName = descAttr?.Name ?? prop.Name.ToLower();
                parts.Add($"<{argName}>");
            }
        }

        // Add options placeholder
        var hasFlags = properties.Any(p => p.GetCustomAttribute<FlagAliasAttribute>() != null);
        if (hasFlags)
        {
            parts.Add("[options]");
        }

        return string.Join(" ", parts);
    }

    public string GenerateCommandList(Dictionary<string, Type> commands)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Available commands:");
        sb.AppendLine();

        if (commands.Count == 0)
        {
            return sb.ToString();
        }

        // Collect command info
        var commandList = commands.OrderBy(c => c.Key).Select(c =>
        {
            var descAttr = c.Value.GetCustomAttribute<DescriptionAttribute>();
            var description = descAttr?.Description ?? string.Empty;
            return (Name: c.Key, Description: description);
        }).ToList();

        // Calculate max command name length for alignment
        var maxNameLength = commandList.Max(c => c.Name.Length);
        var padding = 2; // Extra spacing between columns

        // Generate table
        foreach (var (name, description) in commandList)
        {
            var paddedName = name.PadRight(maxNameLength + padding);
            sb.AppendLine($"  {paddedName}{description}");
        }

        return sb.ToString();
    }
}
