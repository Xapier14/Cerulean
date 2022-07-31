using Cerulean.CLI;

var router = Router.GetRouter();

/* Register commands */
router.RegisterCommands();

// Display help if no args
if (args.Length == 0)
{
    Splash.DisplaySplashHelp();
    Environment.Exit(0);
}

// Parse and execute command
var commandName = args[0];
var commandArgs = args.Skip(1).ToArray();
if (!router.ExecuteCommand(commandName, commandArgs)) Splash.DisplaySplashHelp();