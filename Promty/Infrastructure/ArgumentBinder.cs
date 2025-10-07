using System.Reflection;
using Promty.Attributes;

namespace Promty.Infrastructure;

public class ArgumentBinder
{
    public TArgs Bind<TArgs>(CommandLineParser parser) where TArgs : new()
    {
        var args = new TArgs();
        var type = typeof(TArgs);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var positionalArgs = parser.GetPositionalArguments();
        var positionalIndex = 0;

        // Separate properties into positional (mandatory), flags (optional), and flags enums
        var positionalProperties = new List<PropertyInfo>();
        var flagProperties = new List<PropertyInfo>();
        var flagEnumProperties = new List<PropertyInfo>();

        foreach (var prop in properties)
        {
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // Check if it's a [Flags] enum
            if (propertyType.IsEnum && propertyType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                flagEnumProperties.Add(prop);
            }
            else
            {
                var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>();
                if (flagAlias != null)
                {
                    flagProperties.Add(prop);
                }
                else
                {
                    positionalProperties.Add(prop);
                }
            }
        }

        // Bind mandatory positional arguments
        foreach (var prop in positionalProperties)
        {
            if (positionalIndex >= positionalArgs.Count)
            {
                throw new ArgumentException($"Missing required argument: {prop.Name}");
            }

            var value = positionalArgs[positionalIndex++];
            SetPropertyValue(prop, args, value);
        }

        // Bind optional flag arguments
        foreach (var prop in flagProperties)
        {
            var flagAlias = prop.GetCustomAttribute<FlagAliasAttribute>()!;
            string? value = null;

            // Try to get value from long alias
            if (!string.IsNullOrEmpty(flagAlias.LongAlias))
            {
                value = parser.GetArgument(flagAlias.LongAlias);

                // Check if it's a boolean flag
                if (value == null && parser.HasFlag(flagAlias.LongAlias))
                {
                    value = "true";
                }
            }

            // Try to get value from short alias if not found
            if (value == null && flagAlias.ShortAlias.HasValue)
            {
                value = parser.GetArgument(flagAlias.ShortAlias.Value.ToString());

                // Check if it's a boolean flag
                if (value == null && parser.HasFlag(flagAlias.ShortAlias.Value.ToString()))
                {
                    value = "true";
                }
            }

            // Set property value if found
            if (value != null)
            {
                SetPropertyValue(prop, args, value);
            }
        }

        // Bind [Flags] enum properties
        foreach (var prop in flagEnumProperties)
        {
            var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var enumValue = BindFlagsEnum(propertyType, parser);
            prop.SetValue(args, enumValue);
        }

        return args;
    }

    private void SetPropertyValue(PropertyInfo prop, object obj, string value)
    {
        var propertyType = prop.PropertyType;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        try
        {
            object? convertedValue;

            if (underlyingType == typeof(bool))
            {
                convertedValue = bool.Parse(value);
            }
            else if (underlyingType == typeof(int))
            {
                convertedValue = int.Parse(value);
            }
            else if (underlyingType == typeof(long))
            {
                convertedValue = long.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (underlyingType == typeof(double))
            {
                convertedValue = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (underlyingType == typeof(string))
            {
                convertedValue = value;
            }
            else if (underlyingType.IsEnum)
            {
                convertedValue = ParseEnumWithFlagAlias(underlyingType, value);
            }
            else
            {
                convertedValue = Convert.ChangeType(value, underlyingType);
            }

            prop.SetValue(obj, convertedValue);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to convert value '{value}' for property '{prop.Name}': {ex.Message}", ex);
        }
    }

    private object ParseEnumWithFlagAlias(Type enumType, string value)
    {
        // First, try to match against FlagAlias attributes on enum fields
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var flagAlias = field.GetCustomAttribute<FlagAliasAttribute>();
            if (flagAlias != null)
            {
                // Check if the value matches the long alias
                if (!string.IsNullOrEmpty(flagAlias.LongAlias) &&
                    string.Equals(value, flagAlias.LongAlias, StringComparison.OrdinalIgnoreCase))
                {
                    return field.GetValue(null)!;
                }

                // Check if the value matches the short alias
                if (flagAlias.ShortAlias.HasValue &&
                    value.Length == 1 &&
                    char.ToLowerInvariant(value[0]) == char.ToLowerInvariant(flagAlias.ShortAlias.Value))
                {
                    return field.GetValue(null)!;
                }
            }
        }

        // Fall back to standard enum parsing (by name or numeric value)
        return Enum.Parse(enumType, value, ignoreCase: true);
    }

    private object BindFlagsEnum(Type enumType, CommandLineParser parser)
    {
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var result = 0;

        foreach (var field in fields)
        {
            var flagAlias = field.GetCustomAttribute<FlagAliasAttribute>();
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

            // Check if flag is present in parsed arguments
            bool flagPresent = false;

            if (!string.IsNullOrEmpty(longAlias) && parser.HasFlag(longAlias))
            {
                flagPresent = true;
            }

            if (!flagPresent && shortAlias.HasValue && parser.HasFlag(shortAlias.Value.ToString()))
            {
                flagPresent = true;
            }

            if (flagPresent)
            {
                var fieldValue = (int)field.GetValue(null)!;
                result |= fieldValue;
            }
        }

        return Enum.ToObject(enumType, result);
    }

    private static string ConvertToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = new System.Text.StringBuilder();
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
}
