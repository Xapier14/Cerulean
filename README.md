<h1 align="center">Cerulean UI</h1>
<p align="center">
  <b>Useful Tools</b><br>
  <a href="#">CLI Tool</a> | 
  <a href="#">Layout Designer</a>
</p>
<h4>Available on Nuget</h4>

![Nuget](https://img.shields.io/nuget/v/Cerulean.Core?label=Cerulean.Core) ![Nuget](https://img.shields.io/nuget/v/Cerulean.Common?label=Cerulean.Common) ![Nuget](https://img.shields.io/nuget/v/Cerulean.Components?label=Cerulean.Components) ![Nuget](https://img.shields.io/nuget/v/Cerulean.CLI?label=Cerulean.CLI)

<h2>Basic Information</h2>
Write <strong>cross-platform</strong> GUI apps using simple XML and C#.<br>
Mainly testing on Windows and Ubuntu. Feel free to test on other platforms.<br>
<br>
Check out this <a href="https://github.com/Xapier14/Calculator">sample calculator app on CeruleanUI</a>.<br>
<br>
Should also work on OSX, but you need to install SDL2 and the other libraries to your frameworks folder.<br>
Bundle your needed fonts in a folder called "Fonts" in the build directory as the font loader is jank with OSX.

<h2>Getting Started</h2>

> **Warning**
> This section is heavily under-construction. Use a release build instead if available.

1. Download a release build [here](https://github.com/Xapier14/Cerulean/releases) and extract the archive.
2. Add the `crn` executable to your binaries folder or add the containing folder to the `%PATH%` environment variable.
3. Create your new app via:
    ```bash
    crn new <app_name>
    ```
4. Build the project.
    ```bash
    crn build
    ```
    This should automatically build the XMLs, the dotnet project, and bundle the needed dependencies.
    
    Currently, the bundler only works on a Windows build target for now. Support for other operating systems will come in a later update.
    
    See "Important Notes" below on how to bundle and install dependencies for other OSs.
5. Run the app via:
    ```bash
    crn run
    ```
    If you run into DllNotFound errors, try running `crn bundle`.

<h2>Important Notes</h2>

 - .NET 6 is **required** for the CLI tool.
 - .NET 6 or newer projects are compatible with CeruleanUI.
 - By default, the CLI should use the latest .NET SDK and creates a project with the same version.
 - Use the appropriate SDL2 binaries for your app/system.
    - On Any CPU, use the binaries appropriate for your system's architecture.
    - On x86, use x86.
    - On x64, use x64.
 - On Windows, use the runtime binaries available at <a href="https://libsdl.org/">libsdl.org</a>.
 - On Ubuntu, simply do:
    ```bash
    sudo apt-get install libsdl2-dev
    sudo apt-get install libsdl2-ttf-dev
    sudo apt-get install libsdl2-image-dev
    ```
    - **NOTE:** the snap version of `dotnet` will NOT find the SDL2 library. Please install dotnet with APT.
        - See <a href="https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu">installing dotnet</a> on MSDN.
        - Relevant issue: <a href="https://github.com/exelix11/SysDVR/issues/118">exelix11/SysDVR (#118)</a>.
 - Fonts will be searched first in a folder called `Fonts` in the environment's current directory, and then the system's font directory.
    - The default font for the Label component is `Arial` (*Subject to change*).
    - The library will find the first font with a filename of `Arial.ttf` or `Arial.otf`, this is case-insensitive.
    - If not found, it will simply throw a `GeneralAPIException: "Font not found."`.
<h2>Features</h2>

### Cerulean API
 - [x] Window Events
 - [x] Styler Element
### Cerulean Components
 - [x] Functional Components
     - [x] Timer
     - [x] Pointer
 - [ ] Dialog Components
     - [ ] Message Box
     - [ ] Open File Dialog
     - [ ] Save File Dialog
     - [ ] Folder Select Dialog
 - [x] Graphical Components
     - [x] Rectangle
     - [x] Image
     - [x] Progress Bar
     - [x] Label
 - [ ] Input Components
     - [x] TextBox
     - [ ] MultiTextBox
     - [x] Button
     - [x] Check Box
     - [x] Radio Button
     - [ ] Drop Down List
 - [ ] Container Components
     - [x] Grid
     - [x] Panel
     - [ ] Stack Panel
     - [ ] Group Tab
### Cerulean CLI
 - [x] Style Builder
 - [ ] C# Snippet Element
 - [x] Project Builder/Scaffolder
 
 
## Warning
This is a WIP project, please do not use in commercial projects.

## License

This project uses the MIT License.
SDL2 and SDL2-CS, dependencies of this project, are released under the zlib license.


