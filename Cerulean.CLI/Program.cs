using Cerulean.CLI;
using Cerulean.CLI.Commands;

var router = Router.GetRouter();

/* Register commands */
// if using .NET 7
router.RegisterCommands();

// if using .NET <=6
//router.RegisterCommand(BuildXml.CommandName, BuildXml.DoAction);
//router.RegisterCommand(NewProject.CommandName, NewProject.DoAction);

// Display help if no args
if (args.Length == 0)
{
    Splash.DisplaySplashHelp();
    Environment.Exit(0);
}

// Parse and execute command
var commandName = args[0];
var commandArgs = args.Skip(1).ToArray();
if (!router.ExecuteCommand(commandName, commandArgs))
{
    Splash.DisplaySplashHelp();
}