namespace Cerulean.CLI;

public interface ICommand
{
    public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options);
}