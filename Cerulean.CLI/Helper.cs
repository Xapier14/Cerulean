﻿using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.Extensions;
using Cerulean.Common;

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
            .Any(fileInfo => fileInfo.Extension.ToLower() == ".csproj");
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

    public static IEnumerable<string> WordWrap(string text, int lineWidth)
    {
        var words = text.Split(' ');
        var buffer = new StringBuilder();
        foreach (var word in words)
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

    public static string ParseNestedComponentName(string nestedName, string root)
    {
        StringBuilder component = new();
        var nests = nestedName.Split('.');
        component.Append($"{root}GetChild(\"{nests[0]}\")");
        for (var i = 1; i < nests.Length; i++) component.Append($".GetChild(\"{nests[i]}\")");
        return component.ToString();
    }

    public static string EscapeString(string sourceString)
    {
        return sourceString
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    public static string ParseHintedString(string hintedString, string root, string? enumFamily = null,
        string? overrideType = null)
    {
        // hinted value pattern ([name]="[type]: [value]")
        var regex = Regex.Match(hintedString, @"^(\w+):\s?(.+)");
        var value = hintedString;

        // attribute value-part has datatype hint
        if (!regex.Success && overrideType is null) return value;
        var type = overrideType ?? regex.Groups[1].ToString().ToLower();
        var raw = overrideType is null ? regex.Groups[2].ToString() : hintedString;
        try
        {
            value = type switch
            {
                "bool" => $"{bool.Parse(raw)}",
                "byte" => $"{byte.Parse(raw)}",
                "char" => $"'{(raw[0] == '\'' ? "\'" : raw[0])}'",
                "short" => $"{short.Parse(raw)}",
                "ushort" => $"{ushort.Parse(raw)}",
                "int" => $"{int.Parse(raw)}",
                "uint" => $"{uint.Parse(raw)}",
                "long" => $"{long.Parse(raw)}",
                "ulong" => $"{ulong.Parse(raw)}",
                "float" => $"{float.Parse(raw)}",
                "double" => $"{double.Parse(raw)}",
                "string" => $"\"{EscapeString(raw)}\"",
                "component" => ParseNestedComponentName(raw, root),
                "color" => $"new Color(\"{raw}\")",
                "literal" => value,
                "enum" => $"{enumFamily}.{raw}",
                _ => "null"
            };
            switch (value)
            {
                case "null":
                    ColoredConsole.WriteLine(
                        $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Type '{type}' for attribute '{hintedString}' not recognized, using null instead.");
                    break;
                case "Colors.None" when type == "color":
                    ColoredConsole.WriteLine(
                        $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Color '{hintedString}' not parsed correctly, using Colors.None instead.");
                    break;
            }
        }
        catch (Exception ex)
        {
            ColoredConsole.WriteLine(
                $"[$yellow^WARN$r^][$yellow^COMPONENT$r^] Could not cast type '{type}' for attribute '{hintedString}': {raw}, using null instead. {ex.Message}");
            value = "null";
        }

        return value;
    }

    public static string? GetRecommendedDataType(string propertyName, out string? enumFamily)
    {
        var type = propertyName switch
        {
            "ForeColor" => "color",
            "BackColor" => "color",
            "BorderColor" => "color",
            "HighlightColor" => "color",
            "ActivatedColor" => "color",
            "FontName" => "string",
            "FontSize" => "int",
            "FontStyle" => "string",
            "Text" => "string",
            "FileName" => "string",
            "Source" => "string",
            "ImageSource" => "string",
            "X" => "int",
            "Y" => "int",
            "PictureMode" => "enum",
            "Orientation" => "enum",
            _ => null
        };
        enumFamily = propertyName switch
        {
            "TargetMouseButton" => "MouseButton",
            _ => propertyName
        };
        return type;
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