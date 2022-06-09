using Cerulean.CLI;
using Cerulean.CLI.Commands;

Router router = Router.GetRouter();

// Register commands
router.RegisterCommand("build-xml", BuildXML.Action);
router.RegisterCommand("new", NewProject.Action);

//router.RegisterCommand("build", Build.Action
//router.RegisterCommand("run", Run.Action);

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