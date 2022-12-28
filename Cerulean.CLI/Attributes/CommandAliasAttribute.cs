namespace Cerulean.CLI.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAliasAttribute : Attribute
{
    public CommandAliasAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }

    public string[] Aliases { get; }
}