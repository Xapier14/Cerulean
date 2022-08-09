<h1 align="center">Cerulean UI</h1>
<p align="center">
  <b>Useful Tools</b><br>
  <a href="#">CLI Tool</a> | 
  <a href="#">Layout Designer</a>
</p><br>
<h2>Basic Information</h2>
Write <strong>cross-platform</strong> GUI apps using simple XML and C#.<br>
Mainly testing on Windows and Ubuntu. Feel free to test on other platforms.<br>

<h2>Getting Started</h2>

> **Warning**
> This section is heavily under-construction. Use a release build instead if available.

1. Clone the repository via:
    ```bash
    git clone https://github.com/Xapier14/Cerulean.git --recurse-submodules
    ```
2. Build the Cerulean.CLI project.
    - You can build this as normal or as a single-file app.
    - Take note that some features might not be available as a single-file app.
3. Add the `crn` executable to your binaries folder or `%PATH%` environment variable.
4. Create your new app via:
    ```bash
    crn new <app_name>
    ```
5. Build the XMLs.
    > **Note**
    > This will be subject to change in a later release.
    ```bash
    cd <app_name>
    crn build-xml
    ```
    Navigate to the newly created directory and build the XMLs.
6. Build the project.
    ```bash
    dotnet build
    ```
7. [*Only For Windows Users*] Bundle the SDL2 runtime binaries.
    - Download the SDL2 runtime binary [here](https://libsdl.org/).
    - Download the SDL2_ttf runtime binaries [here](https://www.libsdl.org/projects/SDL_ttf/release/).
    - Download the SDL2_image runtime binary [here](https://www.libsdl.org/projects/SDL_image/release/).
    
    > See [Important Notes](https://github.com/Xapier14/Cerulean#important-notes) on what architecture to use.

    After downloading, copy the binaries over to the build directory.
    - Debug: `./bin/Debug/net*.*/`
    - Release: `./bin/Release/net*.*/`

    You'll need to bundle these too when publishing the app.

8. Run the app via:
    ```bash
    dotnet run
    ```

<h2>Important Notes</h2>

 - .NET 6 is **required**.
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
 - Fonts will be searched first in a folder called `Fonts` in the environment's current directory, then and the system's font directory.
    - The default font for the Label component is `Arial`.
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
     - [ ] TextBox
     - [ ] MultiTextBox
     - [x] Button
     - [x] Check Box
     - [ ] Radio Button
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
<h2>Warning</h2>
This is a WIP project, please do not use in commercial projects.<br>
