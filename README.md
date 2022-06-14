<h1 align="center">Cerulean UI</h1>
<p align="center">
  <b>Useful Tools</b><br>
  <a href="#">CLI Tool</a> | 
  <a href="#">Layout Designer</a>
</p><br>
<h2>Basic Information</h2>
Write GUI apps using simple XML and C#.<br>
<h2>Important Notes</h2>

 - ~~Mostly~~ **.NET 6** is required. ~~The CLI tool needs **.NET 7** but can be altered to only require **.NET 6**.~~ 
    - The CLI project has been reverted to **.NET 6** for now and will be upgraded once **.NET 7** releases.
    - See `Cerulean.CLI\Program.cs` for more info.
 - Do not use `Any CPU`, either use `x86` or `x64` and use the appropriate SDL2 binaries.
 - When developing apps for Cerulean UI with Visual Studio, add a pre-build event that calls `%CERULEAN_CLI_PATH%\\crn.exe build-xml`.
    - Where `%CERULEAN_CLI_PATH%` is the directory to the `crn.exe` binary
    - Eventually, a build command will be added onto `crn.exe` that automates XML layout building, resource packing and dotnet build process.
<h2>Features</h2>

### Cerulean API
 - [x] Window Events
 - [ ] Styler Element
### Cerulean Components
 - [ ] Functional Components
     - [x] Timer
     - [ ] Pointer
 - [ ] Dialog Components
     - [ ] Message Box
     - [ ] Open File Dialog
     - [ ] Save File Dialog
     - [ ] Folder Select Dialog
 - [ ] Graphical Components
     - [x] Rectangle
     - [ ] Image
     - [ ] Progress Bar
 - [ ] Input Components
     - [ ] TextBox
     - [ ] MultiTextBox
     - [ ] Button
     - [ ] Check Box
     - [ ] Radio Button
     - [ ] Drop Down List
 - [ ] Container Components
     - [x] Grid
     - [ ] Panel
     - [ ] Group Tab
### Cerulean CLI
 - [ ] Style Builder
 - [ ] C# Snippet Element
 - [ ] Project Builder/Scaffolder
<h2>Warning</h2>
This is a WIP project, please do not use in commercial projects.<br>
