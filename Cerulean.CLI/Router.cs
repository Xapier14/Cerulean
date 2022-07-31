using System.Reflection;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI;

internal class Router
{
    private static Router? _router;
    private readonly IDictionary<string, Delegate> _commands = new Dictionary<string, Delegate>();

    public static Router GetRouter()
    {
        return _router ??= new Router();
    }

    public void RegisterCommand(string? commandName, Func<string[], int> action)
    {
        if (commandName is not null)
            _commands[commandName] = action;
    }

    public void RegisterCommands()
    {
        var interfaceType = typeof(ICommand);
        var commands = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetLoadableTypes())
            .Where(x => x is not null
                        && x != interfaceType
                        && x.IsAssignableTo(interfaceType)
                        && x != typeof(object));

        foreach (var command in commands)
        {
            var attributes = command?.GetCustomAttributes();

            if (attributes is null)
                continue;

            string? commandName = null;
            foreach (var attribute in attributes)
                if (attribute is CommandNameAttribute nameAttribute)
                    commandName = nameAttribute.CommandName;
            var func = command?.GetMethod("DoAction")?.CreateDelegate(typeof(Func<string[], int>));
            if (commandName is not null && func is not null)
                _commands[commandName] = func;
            else
                ColoredConsole.WriteLine(
                    $"[$cyan^Router.RegisterCommands()$r^] Could not load command \"{command?.Name}\".");
        }
    }

    public bool ExecuteCommand(string commandName, params string[] args)
    {
        if (!_commands.TryGetValue(commandName, out var command))
            return false;
        var result = command?.DynamicInvoke(new object?[] { args });
        if (result is int exitCode && exitCode != 0)
            Environment.Exit(exitCode);
        return true;
    }
}