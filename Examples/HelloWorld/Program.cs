using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;

// Set-up the CeruleanAPI singleton instance
var ceruleanApi = CeruleanAPI.GetAPI()
                            .UseSDL2Graphics()
                            .UseConsoleLogger()
                            .Initialize();

// Create a layout from code.
// NOTE: you should be using an XML layout file instead of this and running it through the layout compiler.
dynamic mainLayout = new Layout();
mainLayout.AddChild("Grid", new Grid()
{
    RowCount = 2,
    ColumnCount = 2
});
mainLayout.Grid.AddChild("Label", new Label()
{
    Text = "Hello World!",
    ForeColor = new Color(255, 255, 255),
    GridColumn = 1,
    GridRow = 1,
    FontName = "Arial",
    FontStyle = "",
    FontSize = 24,
    X = 0,
    Y = 0
});

// Create a window that uses the layout.
var mainWindow = ceruleanApi.CreateWindow(mainLayout);
// Set its back color.
mainWindow.BackColor = new Color(0, 0, 0);

// Wait for all windows to close and call Quit() on finish.
ceruleanApi.WaitForAllWindowsClosed(true);