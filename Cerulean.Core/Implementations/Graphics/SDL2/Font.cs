using Cerulean.Common;
using System.Runtime.InteropServices;
using static SDL2.SDL_ttf;

namespace Cerulean.Core
{
    internal class Font : IDisposable
    {
        public string Name { get; set; }
        public int PointSize { get; set; }
        public string Style { get; set; }
        public IntPtr Data { get; set; } = IntPtr.Zero;

        private Font(string name, string style, int ptSize)
        {
            Name = name;
            PointSize = ptSize;
            Style = style;
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
            //CeruleanAPI.GetAPI().Log($"Trying to find {name} in {systemPath}");
            // check if system path exists
            //CeruleanAPI.GetAPI().Log($"{systemPath} exists: {Directory.Exists(systemPath)}");
            if (Directory.Exists(systemPath))
            {
                var dirInfo = new DirectoryInfo(systemPath);
                var files = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Where(file =>
                    {
                        //if (file.Extension.ToLower() == ".ttf")
                            //CeruleanAPI.GetAPI().Log($"Checking {file.Name}");
                        return file.Name.ToLower().Contains(name.ToLower()) && (file.Extension.ToLower() == ".ttf" || file.Extension.ToLower() == ".otf");
                    });
                if (files.Any())
                {
                    path = files.First().FullName;
                    return true;
                }
            }

            return false;
        }

        public static Font LoadFont(string name, string style, int pointSize)
        {
            if (TryFindTTF(name, out string path))
            {
                var fontPtr = TTF_OpenFont(path, pointSize);
                if (fontPtr == IntPtr.Zero)
                {
                    throw new GeneralAPIException("Error loading TTF font.");
                }
                TTF_SetFontStyle(fontPtr, style.ToLower() switch
                {
                    "bold" => TTF_STYLE_BOLD,
                    "italic" => TTF_STYLE_ITALIC,
                    "bold italic" => TTF_STYLE_BOLD | TTF_STYLE_ITALIC,
                    _ => TTF_STYLE_NORMAL
                });
                return new Font(name, style, pointSize)
                {
                    Data = fontPtr
                };
            }
            throw new GeneralAPIException("Font file not found.");
        }

        public override string ToString()
        {
            return $"{Name.Trim()}#{Style.Trim()}@{PointSize}pt";
        }

        public void Dispose()
        {
            if (Data != IntPtr.Zero)
                TTF_CloseFont(Data);
            Data = IntPtr.Zero;
        }
    }
}
