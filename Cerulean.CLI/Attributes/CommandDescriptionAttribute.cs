namespace Cerulean.CLI.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandDescriptionAttribute : Attribute
{
    public CommandDescriptionAttribute(string commandDescription)
    {
        CommandDescription = commandDescription;
    }

    public string CommandDescription { get; init; }
}