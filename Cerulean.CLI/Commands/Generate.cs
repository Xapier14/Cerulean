using Cerulean.CLI.Attributes;

namespace Cerulean.CLI;

[CommandName("generate")]
[CommandAlias("g", "gen")]
[CommandDescription("Generates and scaffolds Cerulean UI resource.")]
internal class Generate : ICommand
{
    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
    {
        throw new NotImplementedException();
    }
}