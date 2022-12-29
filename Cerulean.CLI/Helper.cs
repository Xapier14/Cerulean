using Cerulean.CLI.Attributes;
using Cerulean.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Cerulean.CLI;

internal static class Helper
{
    public static bool DoTask(string? taskName, string command, string? args, string? workingDir, bool noOutput = true)
    {
        if (taskName is not null)
            ColoredConsole.WriteLine(taskName);
        var startInfo = new ProcessStartInfo(command, args ?? "")
        {
            UseShellExecute = false,
            CreateNoWindow = noOutput,
            RedirectStandardOutput = noOutput,
            RedirectStandardError = noOutput
        };
        if (workingDir is not null)
            startInfo.WorkingDirectory = workingDir;

        var startTime = DateTime.Now;
        var process = Process.Start(startInfo);
        if (process is not { })
        {
            Console.WriteLine("Could not create '{0}' process.", command);
            return true;
        }

        process.WaitForExit();
        Console.WriteLine("Took {0:hh\\:mm\\:ss\\:fff} to finish.\n", startTime - process.ExitTime);

        if (process.ExitCode == 0)
            return false;

        ColoredConsole.WriteLine(
            $"$red^Command aborted because of an error with {command}. (Exit Code: {process.ExitCode})$r^");
        return true;
    }

    public static bool CheckProjectExists(string projectPath)
    {
        var projectPathInfo = new DirectoryInfo(projectPath);
        return projectPathInfo
            .EnumerateFiles()
            .Any(fileInfo => string.Equals(fileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase));
    }

    public static string GetProjectFileInDirectory(string directory)
    {
        var dirInfo = new DirectoryInfo(directory);

        return dirInfo.EnumerateFiles("*.csproj").First().FullName;
    }

    public static string GetXMLNetVersion(string csprojFile)
    {
        var xml = XDocument.Load(csprojFile);
        if (xml.Root == null)
            throw new InvalidDataException();
        var propGroup = xml.Root.Elements("PropertyGroup").First(x => x.Element("TargetFramework") != null);

        return propGroup.Element("TargetFramework")!.Value;
    }

    public static IEnumerable<(string, string)> GetAllCommandInfo()
    {
        var interfaceType = typeof(ICommand);
        var commands = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetLoadableTypes())
            .Where(x => x is not null
                        && x != interfaceType
                        && x.IsAssignableTo(interfaceType)
                        && x != typeof(object));

        var results = new List<(string, string)>();

        var commandInfos = commands.Select(command =>
        {
            var attributes = command?.GetCustomAttributes().ToList();
            if (attributes is null)
                throw new FatalAPIException("Missing command attributes.");

            string? commandName = null;
            var commandDescription = string.Empty;
            attributes.ForEach(attribute =>
            {
                switch (attribute)
                {
                    case CommandNameAttribute nameAttribute:
                        commandName = nameAttribute.CommandName;
                        break;
                    case CommandDescriptionAttribute descriptionAttribute:
                        commandDescription = descriptionAttribute.CommandDescription;
                        break;
                }
            });

            if (commandName is null)
                throw new FatalAPIException("Missing command name attribute.");

            return (commandName, commandDescription);
        });
        results.AddRange(commandInfos);

        return results;
    }

    public static int CountInterfaceImplementations(Type interfaceType)
    {
        var implementations = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetLoadableTypes())
            .Where(x => x is not null
                        && x != interfaceType
                        && x.IsAssignableTo(interfaceType)
                        && x != typeof(object));
        return implementations.Count();
    }

    public static IEnumerable<string> WordWrap(string text, int lineWidth)
    {
        var words = text.Split(' ');
        var buffer = new StringBuilder();
        foreach (var word in words)
        {
            if (buffer.Length + word.Length <= lineWidth)
            {
                buffer.Append(word).Append(' ');
            }
            else
            {
                var line = buffer.ToString();
                buffer.Clear();
                yield return line[..^1];
            }
        }

        if (buffer.Length <= 0)
            yield break;
        yield return buffer.ToString()[..^1];
    }

    public static string FormatString(string value, int length)
    {
        if (value.Length >= length)
            return value[..length];

        var sb = new StringBuilder();
        sb.Append(value);
        for (var i = value.Length; i < length; ++i)
            sb.Append(' ');

        return sb.ToString();
    }

    public static void GetDotNetVersion(out int major, out int minor, out int build)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        process.Start();
        process.WaitForExit();
        var versionString = process.StandardOutput.ReadToEnd();
        var match = Regex.Match(versionString, @"(\d+).(\d+).(\d+)");
        major = int.Parse(match.Groups[1].Value);
        minor = int.Parse(match.Groups[2].Value);
        build = int.Parse(match.Groups[3].Value);
    }

    public static T? GetJsonAsObject<T>(string url) where T : class
    {
        return GetJsonAsObjectAsync<T>(url).GetAwaiter().GetResult();
    }

    public static async Task<T?> GetJsonAsObjectAsync<T>(string url) where T : class
    {
        using var http = new HttpClient();
        try
        {
            var jsonData = await http.GetStringAsync(url);
            var jsonObject = JsonSerializer.Deserialize<T>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return jsonObject;
        }
        catch (Exception e)
        {
            ColoredConsole.WriteLine("$red^" + e.Message + "$r^");
            return null;
        }
    }

    public static string? GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";

        return null;
    }
}