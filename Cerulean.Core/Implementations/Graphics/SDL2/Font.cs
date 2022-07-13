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

        private static bool TryGetFile(string dirPath, string name, out string filePath)
        {
            filePath = string.Empty;
            if (!Directory.Exists(dirPath)) return false;
            var dirInfo = new DirectoryInfo(dirPath);
            var files = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                .OrderBy(f => f.Name)
                .Where(file =>
                {
                    //CeruleanAPI.GetAPI().Log($"Checking {file.Name} for {name}");
                    return Path.GetFileNameWithoutExtension(file.FullName).ToLower().Replace(new[] {'-', ' '}, '\0') == name.ToLower().Replace(new[] { '-', ' ' }, '\0') // same file name
                           && (file.Extension.ToLower() == ".ttf" || file.Extension.ToLower() == ".otf") // is a ttf or otf file
                           && !file.Attributes.HasFlag(FileAttributes.ReparsePoint); // is not a symlink
                });
            if (!files.Any()) return false;
            filePath = files.First().FullName;
            CeruleanAPI.GetAPI().Log($"Found font {filePath}.");
            return true;
        }

        private static bool TryFindTTF(string name, out string path)
        {
            path = "";
            // find in local app directory
            string basePath = Path.Combine(
                Environment.CurrentDirectory,
                "Fonts");
            if (TryGetFile(basePath, name, out path))
                return true;
            // if (Directory.Exists(basePath))
            // {
            //     var dirInfo = new DirectoryInfo(basePath);
            //     var files = dirInfo.GetFiles(name + "*", SearchOption.TopDirectoryOnly)
            //         .Where(file =>
            //         {
            //             return file.Extension.ToLower() == ".ttf"
            //                 || file.Extension.Length == 0;
            //         });
            //     if (files.Any())
            //     {
            //         path = files.First().FullName;
            //         return true;
            //     }
            // }

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
            if (TryGetFile(systemPath, name, out path))
                return true;
            // if (Directory.Exists(systemPath))
            // {
            //     var dirInfo = new DirectoryInfo(systemPath);
            //     var files = dirInfo.GetFiles("*", SearchOption.AllDirectories)
            //         .Where(file =>
            //         {
            //             //if (file.Extension.ToLower() == ".ttf")
            //                 //CeruleanAPI.GetAPI().Log($"Checking {file.Name}");
            //             return file.Name.ToLower().Contains(name.ToLower()) // contains the name of the font
            //                     && (file.Extension.ToLower() == ".ttf" || file.Extension.ToLower() == ".otf"); // is a ttf or otf file
            //         });
            //     if (files.Any())
            //     {
            //         path = files.First().FullName;
            //         return true;
            //     }
            // }

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
