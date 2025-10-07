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

        // Separate into positional, flags, and flags enums
        var positionalProps = new List<PropertyInfo>();
        var flagProps = new List<PropertyInfo>();
        var flagEnumProps = new List<PropertyInfo>();

        foreach (var prop in properties)
        {
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // Check if it's a [Flags] enum
            if (propertyType.IsEnum && propertyType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                flagEnumProps.Add(prop);
            }
            else
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

        // Collect all flag options (both regular flags and flags from enums)
        var allFlagOptions = new List<(string aliasText, string? description)>();

        // Add regular flag properties
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

            allFlagOptions.Add((aliasText, descAttr?.Description));
        }

        // Add flags from [Flags] enum properties
        foreach (var prop in flagEnumProps)
        {
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var fields = propertyType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                // Skip the "None" value (0) in flags enums
                var fieldValue = (int)field.GetValue(null)!;
                if (fieldValue == 0)
                    continue;

                var flagAlias = field.GetCustomAttribute<FlagAliasAttribute>();
                var descAttr = field.GetCustomAttribute<DescriptionAttribute>();

                var aliases = new List<string>();
                string? longAlias;
                char? shortAlias = null;

                if (flagAlias != null)
                {
                    longAlias = flagAlias.LongAlias;
                    shortAlias = flagAlias.ShortAlias;
                }
                else
                {
                    // Convert field name to kebab-case
                    longAlias = ConvertToKebabCase(field.Name);
                }

                if (shortAlias.HasValue)
                {
                    aliases.Add($"-{shortAlias.Value}");
                }
                if (!string.IsNullOrEmpty(longAlias))
                {
                    aliases.Add($"--{longAlias}");
                }

                var aliasText = string.Join(", ", aliases);
                allFlagOptions.Add((aliasText, descAttr?.Description));
            }
        }

        // Show all options
        if (allFlagOptions.Count > 0)
        {
            sb.AppendLine("Options:");
            foreach (var (aliasText, description) in allFlagOptions)
            {
                sb.Append($"  {aliasText}");
                if (description != null)
                {
                    sb.Append($"  {description}");
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
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var isFlagsEnum = propertyType.IsEnum && propertyType.GetCustomAttribute<FlagsAttribute>() != null;
            var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>();

            if (flagAlias == null && !isFlagsEnum)
            {
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                var argName = descAttr?.Name ?? prop.Name.ToLower();
                parts.Add($"<{argName}>");
            }
        }

        // Add options placeholder
        var hasFlags = properties.Any(p =>
        {
            var propertyType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            var isFlagsEnum = propertyType.IsEnum && propertyType.GetCustomAttribute<FlagsAttribute>() != null;
            return p.GetCustomAttribute<FlagAliasAttribute>() != null || isFlagsEnum;
        });

        if (hasFlags)
        {
            parts.Add("[options]");
        }

        return string.Join(" ", parts);
    }

    private static string ConvertToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = new StringBuilder();
        result.Append(char.ToLowerInvariant(value[0]));

        for (int i = 1; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                result.Append('-');
                result.Append(char.ToLowerInvariant(value[i]));
            }
            else
            {
                result.Append(value[i]);
            }
        }

        return result.ToString();
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
