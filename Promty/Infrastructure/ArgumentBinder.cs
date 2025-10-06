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

        // Separate properties into positional (mandatory) and flags (optional)
        var positionalProperties = new List<PropertyInfo>();
        var flagProperties = new List<PropertyInfo>();

        foreach (var prop in properties)
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
                convertedValue = long.Parse(value);
            }
            else if (underlyingType == typeof(double))
            {
                convertedValue = double.Parse(value);
            }
            else if (underlyingType == typeof(string))
            {
                convertedValue = value;
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
}
