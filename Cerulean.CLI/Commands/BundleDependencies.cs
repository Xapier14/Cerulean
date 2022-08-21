using System;
using System.Collections.Generic;
using System.IO.Compression;
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
        private static void ExtractZip(string zipFile, string extractPath)
        {
            var file = File.OpenRead(zipFile);
            var zip = new ZipArchive(file);
            Directory.CreateDirectory(extractPath);
            zip.ExtractToDirectory(extractPath, true);
        }

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

        private static IEnumerable<FileInfo> UnpackFromCacheToProject(string targetArch, string targetOS, string config, string netVersion, string projectPath)
        {
            var dependenciesPath = Path.Join(projectPath, ".dependencies");
            var cachePath = Path.Join(dependenciesPath, "cache");
            Directory.CreateDirectory(cachePath);
            var buildPath = Path.Join(projectPath, "bin", config, netVersion, $"{targetOS}-{targetArch}");
            var core = Path.Join(dependenciesPath, $"sdl2-{targetArch}.zip");
            var image = Path.Join(dependenciesPath, $"sdl2_image-{targetArch}.zip");
            var ttf = Path.Join(dependenciesPath, $"sdl2_ttf-{targetArch}.zip");
            if (!Directory.Exists(buildPath))
            {
                Console.WriteLine($"Path {buildPath} does not exist.");
                return Array.Empty<FileInfo>();
            }

            if (!File.Exists(core) ||
                !File.Exists(image) ||
                !File.Exists(ttf))
            {
                Console.WriteLine("One or more required dependencies is missing, please install/bundle them manually.");
                return Array.Empty<FileInfo>();
            }

            Console.WriteLine("Extracting SDL2 Core dependency...");
            ExtractZip(core, cachePath);

            Console.WriteLine("Extracting SDL2 Image dependency...");
            ExtractZip(image, cachePath);

            Console.WriteLine("Extracting SDL2 TTF dependency...");
            ExtractZip(ttf, cachePath);

            var cacheFolder = new DirectoryInfo(cachePath);
            return cacheFolder.GetFiles("*.*", new EnumerationOptions
            {
                RecurseSubdirectories = true
            }).Where(fileInfo => fileInfo.Extension.ToLower() == ".dll");
        }

        private static void CopyFileInfosToTargetFolder(IEnumerable<FileInfo> files, string targetPath)
        {
            Directory.CreateDirectory(targetPath);
            foreach (var file in files)
            {
                var destination = Path.Join(targetPath, file.Name);
                Console.WriteLine($"Copying {file.Name}...");
                if (File.Exists(destination))
                    File.Delete(destination);
                File.Copy(file.FullName, destination);
            }
        }

        public int DoAction(string[] args, IEnumerable<string> flags, IDictionary<string, string> options)
        {
            var projectPath = "./";
            if (args.Length > 0)
                projectPath = args[0];

            options.TryGetValue("arch", out var arch);
            arch ??= Environment.Is64BitOperatingSystem ? "x64" : "x86";

            options.TryGetValue("os", out var os);
            os ??= Helper.GetOSPlatform();

            options.TryGetValue("config", out var netConfig);
            netConfig ??= "Debug";

            options.TryGetValue("nv", out var netVersion);
            var csproj = Helper.GetProjectFileInDirectory(projectPath);
            netVersion ??= Helper.GetXMLNetVersion(csproj);

            if (os != "win")
            {
                ColoredConsole.WriteLine("$red^This command is not available for the target operating system.\n" +
                                         "$r^Please check with the documentation on how to bundle the needed " +
                                         "dependencies for the target runtime.");
                Console.WriteLine($"Target operating system: {os}");
                return 0;
            }

            var config = Config.GetConfig();
            Console.WriteLine("Fetching SDL2 data...");
            var jsonData = config.GetProperty<string>("SDL_BUNDLE_JSON") ??
                           config.GetProperty<string>("SDL_BUNDLE_JSON_FALLBACK") ??
                           string.Empty;
            var sdlLinks =
                Helper.GetJsonAsObject<SDLUrlInfo>(jsonData);
            if (sdlLinks is not null)
            {
                Console.WriteLine("Retrieving SDL2 runtime libraries from web...");
                GetSDL2FromWeb(sdlLinks, arch, projectPath);
            }

            // check if local dep cache has sdl2 packages
            // then extract to target dir
            Console.WriteLine("Extracting cached packages...");
            var dlls = UnpackFromCacheToProject(arch, os, netConfig, netVersion, projectPath);
            if (!dlls.Any())
                return -1;
            Console.WriteLine("Copying dependencies to target build folder...");
            var buildPath = Path.Join(projectPath, "bin", netConfig, netVersion, $"{os}-{arch}");
            CopyFileInfosToTargetFolder(dlls, buildPath);

            Console.WriteLine();
            ColoredConsole.WriteLine("$green^Dependencies bundled successfully!$r^");

            return 0;
        }
    }
}
