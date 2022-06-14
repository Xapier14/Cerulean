using Cerulean.CLI;
using Cerulean.CLI.Commands;

Router router = Router.GetRouter();

/* Register commands */
// if using .NET 7
//router.RegisterCommands();

// if using .NET <=6
router.RegisterCommand(BuildXML.CommandName, BuildXML.DoAction);
router.RegisterCommand(NewProject.CommandName, NewProject.DoAction);

// Display help if no args
if (args.Length == 0)
{
    Help.DisplayGeneralHelp();
    Environment.Exit(0);
}

// Parse and execute command
string commandName = args[0];
string[] commandArgs = args.Skip(1).ToArray();
if (!router.ExecuteCommand(commandName, commandArgs))
{
    Help.DisplayGeneralHelp();
}