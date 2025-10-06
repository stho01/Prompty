// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Promty;

var executor = new CommandExecutor();
executor.RegisterCommandsFromAssembly(Assembly.GetExecutingAssembly());
return await executor.ExecuteAsync(args);
