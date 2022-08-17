using System.Reflection;
using System.Text;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;

namespace Cerulean.CLI;

internal class Router
{
    private static Router? _router;

    private readonly IDictionary<string, (object, MethodInfo)> _commands =
        new Dictionary<string, (object, MethodInfo)>();

    private Router() { }

    public static Router GetRouter()
    {
        return _router ??= new Router();
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
            var method = command?.GetMethod("DoAction");
            var instance = command?.GetConstructor(Array.Empty<Type>())?.Invoke(null);
            if (commandName is not null
                && method is not null
                && instance is not null)
            {
                _commands[commandName] = (instance, method);
            }
            else
                ColoredConsole.WriteLine(
                    $"[$cyan^Router.RegisterCommands()$r^] Could not load command \"{command?.Name}\".");
        }
    }

    public bool ExecuteCommand(string commandName, params string[] argsRaw)
    {
        if (!_commands.TryGetValue(commandName, out var command))
            return false;
        var (instance, method) = command;
        ParseArguments(argsRaw, out var args, out var flags, out var options);
        var result = method?.Invoke(instance, new object?[] { args, flags, options });
        if (result is int exitCode && exitCode != 0)
            Environment.Exit(exitCode);
        return true;
    }
    

    public static void ParseArguments(string[] argsRaw, out string[] args, out List<string> flags,
        out Dictionary<string, string> options)
    {
        var argsList = new List<string>();
        flags = new List<string>();
        options = new Dictionary<string, string>();
        var inFlag = false;
        var optionKey = string.Empty;
        foreach (var substring in argsRaw)
        {
            if (substring.StartsWith('-'))
            {
                if (inFlag)
                {
                    flags.Add(optionKey);
                }

                optionKey = substring.TrimStart('-');
                inFlag = true;
                continue;
            }

            if (inFlag)
            {
                inFlag = false;
                options.Add(optionKey, substring);
                continue;
            }

            argsList.Add(substring);
        }

        if (inFlag)
            flags.Add(optionKey);

        args = argsList.ToArray();
    }
}