using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;
using Cerulean.Common;

namespace Cerulean.CLI
{
    internal static class Helper
    {
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

        public static IEnumerable<string> WordWrap(string text, int lineWidth)
        {
            var words = text.Split(' ');
            var buffer = new StringBuilder();
            foreach (var word in words)
            {
                if (buffer.Length + word.Length <= lineWidth)
                {
                    buffer.Append(word + " ");
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
    }
}
