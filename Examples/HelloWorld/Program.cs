using Cerulean.Core;
using Cerulean.Components;

// Set-up the CeruleanAPI singleton instance
var ceruleanApi = CeruleanAPI.GetAPI()
                            .UseSDL2Graphics()
                            .UseConsoleLogger()
                            .Initialize();

// Create a layout from code.
// NOTE: you should be using an XML layout file instead of this and running it through the layout compiler.
Layout mainLayout = new();
mainLayout.AddChild("Label", new Label()
{
    Text = "Hello World!",
    ForeColor = new(255, 255, 255),
    FontName = "Arial",
    FontStyle = "",
    FontSize = 24,
    X = 120,
    Y = 120
});

// Create a window that uses the layout.
var mainWindow = ceruleanApi.CreateWindow(mainLayout);
// Set its back color.
mainWindow.BackColor = new(0, 0, 0);

// Wait for all windows to close and call Quit() on finish.
ceruleanApi.WaitForAllWindowsClosed(true);