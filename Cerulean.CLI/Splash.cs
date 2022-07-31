using System.Diagnostics;

namespace Cerulean.CLI;

internal static class Splash
{
    public static void DisplaySplash()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"  ______                _                    _____ _     ___ ");
        Console.WriteLine(@" / _____|              | |                  /  ___| |   |_ _|");
        Console.WriteLine(@" | |     ___ _ __ _   _| | ___  __ _ _ ___  | |   | |    | | ");
        Console.WriteLine(@" | |    / _ \ '__| | | | |/ _ \/ _` | '_  \ | |   | |    | | ");
        Console.WriteLine(@" | |____| __/ |  | |_| | |  __/ (_| | | | | | |___| |___ | | ");
        Console.WriteLine(@" \______\___|_|  \___,_|_|\___|\__,_|_| |_| \_____|_____|___|");
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void DisplaySplashHelp()
    {
        DisplaySplash();

        var name = "crn.exe";
        try
        {
            var version = FileVersionInfo.GetVersionInfo(Path.Combine(AppContext.BaseDirectory, "crn.exe"));
            name = version.InternalName ?? "crn.exe";
            Console.WriteLine(
                $" Cerulean CLI: {version.FileMajorPart}.{version.FileMinorPart}.{version.FileBuildPart}");
        }
        catch
        {
            ColoredConsole.WriteLine(" Cerulean CLI: ?.?.? $red^(malformed executable?)$r^");
        }

        var cli = name.Remove(name.Length - 4, 4);

        Helper.GetDotNetVersion(out var netMajor, out var netMinor, out var netBuild);
        var outdated = netMajor < 6;
        ColoredConsole.WriteLine($" .NET SDK: {netMajor}.{netMinor}.{netBuild} " +
                                 (outdated ? "$red^(outdated)$r^" : "$green^(supported)$r^"));
        Console.WriteLine();

        Console.WriteLine("Usage:");
        ColoredConsole.WriteLine($"\t$cyan^{cli}$r^ $yellow^<command> [args]$r^");
        Console.WriteLine("Example:");
        ColoredConsole.WriteLine($"\t$cyan^{cli}$r^ $yellow^help build-xml$r^");
    }
}