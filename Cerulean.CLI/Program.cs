using Cerulean.CLI;
using Cerulean.CLI.Extensions;
using Cerulean.Common;

var config = Config.GetConfig();
var router = Router.GetRouter();

/* Use Default Configs */
config.UseDefaultConfiguration();

/* Register Commands */
router.RegisterCommands();

// Display help if no args
if (args.Length == 0)
{
    Splash.DisplaySplashHelp();
    Environment.Exit(0);
}

/* Parse Command */
var commandName = args[0];
var commandArgs = args.Skip(1).ToArray();

if (!router.ExecuteCommand(commandName, commandArgs))
    Splash.DisplaySplashHelp();