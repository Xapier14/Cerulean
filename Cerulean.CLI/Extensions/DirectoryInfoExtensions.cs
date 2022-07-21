namespace Cerulean.CLI.Extensions
{
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

        public static int TryDelete(this DirectoryInfo dirInfo, bool recursive = false)
        {
            var status = 0;
            try
            {
                dirInfo.Delete(recursive);
            }
            catch (Exception ex)
            {
                ColoredConsole.WriteLine($"[$red^FAIL$r^][$cyan^CLEAN$r^] Could not delete directory '{dirInfo.FullName}'. {ex.Message}");
                status++;
            }

            return status;
        }
    }
}
