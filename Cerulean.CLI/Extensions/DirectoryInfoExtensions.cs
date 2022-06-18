using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cerulean.CLI.Extensions
{
    internal static class DirectoryInfoExtensions
    {
        public static FileInfo[] GetAllFiles(this DirectoryInfo directoryInfo)
        {
            List<FileInfo> files = new();
            foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
            {
                files.AddRange(subDir.GetAllFiles());
            }
            files.AddRange(directoryInfo.GetFiles());
            return files.ToArray();
        }
    }
}
