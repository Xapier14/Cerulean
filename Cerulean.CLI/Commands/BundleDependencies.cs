using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Cerulean.CLI.Attributes;
using Cerulean.CLI.JsonStructure;

namespace Cerulean.CLI.Commands
{
    [CommandName("bundle")]
    [CommandDescription("Bundles needed dependencies for app runtime.")]
    internal class BundleDependencies : ICommand
    {
        private static void GetArchedFile(ArchedLinkEntry? entry, string targetArch, string filePath, string fileName)
        {
            if (entry is null)
                return;
            var url = targetArch == "x64" ? entry.X64 : entry.X86;
            if (url is null)
                return;
            var path = Path.Join(filePath, fileName);
            Directory.CreateDirectory(filePath);
            var fileInfo = new FileInfo(path);
            using var http = new HttpClient();
            Console.Write("Downloading {0}... ", fileName);
            var head = http.Send(new HttpRequestMessage(HttpMethod.Head, url));
            var contentLength = head.Content.Headers.ContentLength;
            if (fileInfo.Exists && fileInfo.Length == contentLength)
            {
                Console.WriteLine("Skipped. (Already exists)");
                return;
            }
            var stream = http.GetStreamAsync(url).GetAwaiter().GetResult();
            using var file = new FileStream(path, FileMode.Create);
            stream.CopyTo(file);
            Console.WriteLine("OK.");
        }

        private static void GetSDL2FromWeb(SDLUrlInfo urls, string targetArch, string projectPath)
        {
            var dependenciesPath = Path.Join(projectPath, ".dependencies");
            Directory.CreateDirectory(dependenciesPath);
            var (preferredSDL, preferredImage, preferredTTF) = urls;
            var core = urls?.SDL?[preferredSDL];
            var image = urls?.Image?[preferredImage];
            var ttf = urls?.TTF?[preferredTTF];
            GetArchedFile(core, targetArch, dependenciesPath, $"sdl2-{targetArch}.zip");
            GetArchedFile(image, targetArch, dependenciesPath, $"sdl2_image-{targetArch}.zip");
            GetArchedFile(ttf, targetArch, dependenciesPath, $"sdl2_ttf-{targetArch}.zip");
        }

        private static void UnpackFromCacheToProject(string targetArch, string cachePath, string projectPath)
        {

        }

        public int DoAction(string[] args)
        {
            var projectPath = "./";
            if (args.Length > 0)
                projectPath = args[0];
            var os = Helper.GetOSPlatform();
            var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            if (os != "win")
            {
                ColoredConsole.WriteLine("$red^This command is not available for the target operating system.\n" +
                                         "$r^Please check with the documentation on how to bundle the needed" +
                                         "dependencies for the target runtime.");
                Console.WriteLine($"Current target operating system: {os}");
                return 0;
            }

            var config = Config.GetConfig();
            Console.WriteLine("Fetching SDL2 data...");
            var sdlLinks =
                Helper.GetJsonAsObject<SDLUrlInfo>(config.GetProperty<string>("SDL_BUNDLE_JSON") ?? string.Empty);
            if (sdlLinks is not null)
            {
                Console.WriteLine("Retrieving SDL2 runtime libraries from web...");
                GetSDL2FromWeb(sdlLinks, arch, projectPath);
            }

            // check if local dep cache has sdl2 packages
            // then extract to target dir
            Console.WriteLine("Extracting to build directory...");

            return 0;
        }
    }
}
