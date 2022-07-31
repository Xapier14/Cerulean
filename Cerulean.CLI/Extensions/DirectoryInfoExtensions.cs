namespace Cerulean.CLI.Extensions;

internal static class DirectoryInfoExtensions
{
    public static FileInfo[] GetAllFiles(this DirectoryInfo directoryInfo)
    {
        List<FileInfo> files = new();
        foreach (var subDir in directoryInfo.GetDirectories())
        {
            if (subDir.Name.ToLower() == "cerulean")
                continue;
            files.AddRange(subDir.GetAllFiles());
        }

        files.AddRange(directoryInfo.GetFiles());
        return files.ToArray();
    }

    public static int TryDelete(this DirectoryInfo dirInfo, bool recursive = false, string? appendMessage = null)
    {
        var status = 0;
        try
        {
            dirInfo.Delete(recursive);
        }
        catch (Exception ex)
        {
            if (appendMessage is { })
                appendMessage += " ";

            ColoredConsole.WriteLine($"{appendMessage}Could not delete directory '{dirInfo.FullName}'. {ex.Message}");
            status++;
        }

        return status;
    }
}