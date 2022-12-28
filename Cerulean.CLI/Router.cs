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

    private readonly IDictionary<string, string> _aliases = new Dictionary<string, string>();

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
            var aliases = Array.Empty<string>();
            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case CommandNameAttribute nameAttribute:
                        commandName = nameAttribute.CommandName;
                        break;
                    case CommandAliasAttribute aliasAttribute:
                        aliases = aliasAttribute.Aliases;
                        break;
                }
            }
            var method = command?.GetMethod("DoAction");
            var instance = command?.GetConstructor(Array.Empty<Type>())?.Invoke(null);
            if (commandName is not null
                && method is not null
                && instance is not null)
            {
                _commands[commandName] = (instance, method);
            }
            else
            {
                ColoredConsole.WriteLine(
                    $"[$cyan^Router.RegisterCommands()$r^] Could not load command \"{command?.Name}\".");
                continue;
            }

            foreach (var alias in aliases)
            {
                _aliases.Add(alias, commandName);
            }
        }
    }

    public bool ExecuteCommand(string commandName, params string[] argsRaw)
    {
        _aliases.TryGetValue(commandName, out var commandFromAlias);
        if (!_commands.TryGetValue(commandFromAlias ?? commandName, out var command))
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
        bool inFlag = false, isConfig = false;
        var optionKey = string.Empty;
        var configKey = string.Empty;
        foreach (var substring in argsRaw)
        {
            if (substring.StartsWith("-c:"))
            {
                if (inFlag)
                {
                    flags.Add(optionKey);
                    inFlag = false;
                    continue;
                }

                configKey = substring.Remove(0, 3);
                isConfig = true;
                continue;
            }

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

            if (isConfig)
            {
                isConfig = false;
                Config.GetConfig().SetProperty(configKey, substring);
                continue;
            }

            argsList.Add(substring);
        }

        if (inFlag)
            flags.Add(optionKey);

        args = argsList.ToArray();
    }
}