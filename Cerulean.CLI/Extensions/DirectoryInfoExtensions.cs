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
    }
}
