using Cerulean.Common;
using System.Runtime.InteropServices;
using static SDL2.SDL_ttf;

namespace Cerulean.Core
{
    internal class Font : IDisposable
    {
        public string Name { get; set; }
        public int PointSize { get; set; }
        public IntPtr Data { get; set; } = IntPtr.Zero;

        private Font(string name, int ptSize)
        {
            Name = name;
            PointSize = ptSize;
        }

        private static bool TryFindTTF(string name, out string path)
        {
            path = "";
            // find in local app directory
            string basePath = Path.Combine(
                Environment.CurrentDirectory,
                "Fonts");
            if (Directory.Exists(basePath))
            {
                var dirInfo = new DirectoryInfo(basePath);
                var files = dirInfo.GetFiles(name + "*", SearchOption.TopDirectoryOnly)
                    .Where(file =>
                    {
                        return file.Extension.ToLower() == ".ttf"
                            || file.Extension.Length == 0;
                    });
                if (files.Any())
                {
                    path = files.First().FullName;
                    return true;
                }
            }

            // find in system fonts
            string systemPath = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemPath = @"C:\Windows\Fonts";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                systemPath = "/usr/share/fonts";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                systemPath = "/Library/Fonts";
            }
            if (Directory.Exists(systemPath))
            {
                var dirInfo = new DirectoryInfo(systemPath);
                var files = dirInfo.GetFiles(name + "*", SearchOption.TopDirectoryOnly)
                    .Where(file =>
                    {
                        return file.Extension.ToLower() == ".ttf"
                            || file.Extension.Length == 0;
                    });
                if (files.Any())
                {
                    path = files.First().FullName;
                    return true;
                }
            }

            return false;
        }

        public static Font LoadFont(string name, int pointSize)
        {
            if (TryFindTTF(name, out string path))
            {
                var fontPtr = TTF_OpenFont(path, pointSize);
                if (fontPtr == IntPtr.Zero)
                {
                    throw new GeneralAPIException("Error loading TTF font.");
                }
                return new Font(name, pointSize)
                {
                    Data = fontPtr
                };
            }
            throw new GeneralAPIException("Font file not found.");
        }

        public override string ToString()
        {
            return $"{Name.Trim()}@{PointSize}pt";
        }

        public void Dispose()
        {
            if (Data != IntPtr.Zero)
                TTF_CloseFont(Data);
            Data = IntPtr.Zero;
        }
    }
}
