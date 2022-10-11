namespace Cerulean.CLI.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandNameAttribute : Attribute
{
    public CommandNameAttribute(string commandName)
    {
        CommandName = commandName;
    }

    public string CommandName { get; }
}