using System.Reflection;
using Promty.Attributes;
using Promty.Infrastructure;

namespace Promty;

public class CommandExecutor
{
    private readonly Dictionary<string, Type> _commands = new();
    private readonly ArgumentBinder _binder = new();
    private readonly HelpTextGenerator _helpGenerator = new();

    public void RegisterCommandsFromAssembly(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && IsCommandType(t));

        foreach (var type in commandTypes)
        {
            var descAttr = type.GetCustomAttribute<DescriptionAttribute>();
            var commandName = descAttr?.Name ?? type.Name.ToLowerInvariant();

            _commands[commandName] = type;
        }
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No command specified");
            Console.WriteLine();
            Console.Write(_helpGenerator.GenerateCommandList(_commands));
            return 1;
        }

        var commandName = args[0].ToLowerInvariant();

        if (!_commands.TryGetValue(commandName, out var commandType))
        {
            Console.WriteLine($"Error: Unknown command '{commandName}'");
            Console.WriteLine();
            Console.Write(_helpGenerator.GenerateCommandList(_commands));
            return 1;
        }

        try
        {
            // Create command instance
            var command = Activator.CreateInstance(commandType);
            if (command == null)
            {
                Console.WriteLine($"Error: Failed to create instance of command '{commandName}'");
                return 1;
            }

            // Get the TArgs type from Command<TArgs>
            var baseType = commandType.BaseType;
            while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(Command<>)))
            {
                baseType = baseType.BaseType;
            }

            if (baseType is not { IsGenericType: true })
            {
                Console.WriteLine($"Error: Command '{commandName}' does not inherit from Command<TArgs>");
                return 1;
            }

            var argsType = baseType.GetGenericArguments()[0];

            object boundArgs;

            // Check if this is a ProcessCommand
            if (command is ProcessCommand)
            {
                // For ProcessCommand, pass all arguments as raw strings
                boundArgs = Activator.CreateInstance(argsType)!;
                var rawArgsProperty = argsType.GetProperty("RawArguments", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rawArgsProperty != null)
                {
                    rawArgsProperty.SetValue(boundArgs, args[1..]);
                }
            }
            else
            {
                // Parse arguments (skip the command name)
                var parser = new CommandLineParser();
                parser.Parse(args[1..]);

                // Bind arguments using reflection
                var bindMethod = typeof(ArgumentBinder)
                    .GetMethod(nameof(ArgumentBinder.Bind))!
                    .MakeGenericMethod(argsType);

                boundArgs = bindMethod.Invoke(_binder, new object[] { parser })!;
            }

            // Execute command using reflection
            var executeMethod = baseType.GetMethod(nameof(Command<object>.ExecuteAsync));
            if (executeMethod == null)
            {
                Console.WriteLine($"Error: ExecuteAsync method not found on command '{commandName}'");
                return 1;
            }

            if (executeMethod.Invoke(command, [boundArgs]) is not Task<int> resultTask)
            {
                Console.WriteLine($"Error: ExecuteAsync did not return a Task<int>");
                return 1;
            }

            return await resultTask;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ArgumentException argEx)
        {
            Console.WriteLine($"Error: {argEx.Message}");
            Console.WriteLine();
            Console.Write(_helpGenerator.GenerateCommandHelp(commandType, commandName));
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing command '{commandName}': {ex.Message}");
            Console.WriteLine();
            Console.Write(_helpGenerator.GenerateCommandHelp(commandType, commandName));
            return 1;
        }
    }

    private static bool IsCommandType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Command<>))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
