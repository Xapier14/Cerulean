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
        public IntPtr Data { get; private set; } = IntPtr.Zero;

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
                .Where(file =>
                {
                    //CeruleanAPI.GetAPI().Log($"Checking {file.Name} for {name}");
                    return Path.GetFileNameWithoutExtension(file.FullName).ToLower().Replace(new[] { '-', ' ' }, '\0') == name.ToLower().Replace(new[] { '-', ' ' }, '\0') // same file name
                           && (string.Equals(file.Extension, ".ttf", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(file.Extension, ".otf", StringComparison.OrdinalIgnoreCase)) // is a ttf or otf file
                           && !file.Attributes.HasFlag(FileAttributes.ReparsePoint); // is not a symlink
                })
                .OrderBy(f => f.Name)
                .ToArray();
            if (!files.Any()) return false;
            filePath = files[0].FullName;
            CeruleanAPI.GetAPI().Log($"Found font {filePath}.");
            return true;
        }

        private static bool TryFindTTF(string name, out string path)
        {
            // find in local app directory
            var basePath = Path.Combine(
                Environment.CurrentDirectory,
                "Fonts");
            if (TryGetFile(basePath, name, out path))
                return true;

            // find in system fonts
            var systemPath = "";
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

            return TryGetFile(systemPath, name, out path);
        }

        public static Font LoadFont(string name, string style, int pointSize)
        {
            var path = string.Empty;
            foreach (var fontName in name.Split(';',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (TryFindTTF(fontName, out path))
                    break;
            }
            if (path == string.Empty)
                throw new GeneralAPIException("Font file not found.");

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
