<h1 align="center">Cerulean UI</h1>
<p align="center">
  <b>Useful Tools</b><br>
  <a href="#">CLI Tool</a> | 
  <a href="#">Layout Designer</a>
</p><br>
<h2>Basic Information</h2>
Write <strong>cross-platform</strong> GUI apps using simple XML and C#.<br>
Mainly testing on Windows and Ubuntu. Feel free to test on other platforms.<br>

<h2>Important Notes</h2>

 - ~~Mostly~~ **.NET 6** is required. ~~The CLI tool needs **.NET 7** but can be altered to only require **.NET 6**.~~ 
    - The CLI project has been reverted to **.NET 6** for now and will be upgraded once **.NET 7** releases.
    - See `Cerulean.CLI\Program.cs` for more info.
 - Use the appropriate SDL2 binaries for your app/system.
    - On Any CPU, use the binaries with the for your system's architecture.
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
 - When developing apps for Cerulean UI with Visual Studio, add a pre-build event that calls `%CERULEAN_CLI_PATH%\\crn.exe build-xml`.
    - Where `%CERULEAN_CLI_PATH%` is the directory to the `crn.exe` binary
    - Eventually, a build command will be added onto `crn.exe` that automates XML layout building, resource packing and dotnet build process.
    - If you are using an editor such as VS Code and want to do this manually, the command syntax is:
        ```bash
        crn build-xml <project-folder> <project-folder>/.cerulean
        ```
        **IMPORTANT:** The second path parameter is the output folder and will be **WIPED** by the tool. Double-check to make sure the path is correct.
    - After this, edit your C# project file (`.csproj`) to include the `.cerulean` sub-directory.
    - You can manually do this by adding the following to your project file.
        ```XML
          <ItemGroup>
            <Compile Include=".cerulean\*.cs" />
          </ItemGroup>
        ```
 - Fonts will be searched first in a folder called `Fonts` in the environment's current directory, then and the system's font directory.
    - The default font for the Label component is `Arial`.
    - The library will find the first font with a filename of `Arial.ttf` or `Arial.otf`, this is case-insensitive.
    - If not found, it will simply throw a `GeneralAPIException: "Font not found."`.
<h2>Features</h2>

### Cerulean API
 - [x] Window Events
 - [ ] Styler Element
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
     - [ ] TextBox
     - [ ] MultiTextBox
     - [ ] Button
     - [ ] Check Box
     - [ ] Radio Button
     - [ ] Drop Down List
 - [ ] Container Components
     - [x] Grid
     - [x] Panel
     - [ ] Stack Panel
     - [ ] Group Tab
### Cerulean CLI
 - [ ] Style Builder
 - [ ] C# Snippet Element
 - [ ] Project Builder/Scaffolder
<h2>Warning</h2>
This is a WIP project, please do not use in commercial projects.<br>
