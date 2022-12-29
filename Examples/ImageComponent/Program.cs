using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;

void Callback(CeruleanAPI api)
{
    var layout = new Layout();
    layout.AddChild("Rect", new Rectangle()
    {
        FillColor = new Color("#F00")
    });
    layout.AddChild("Image", new Image()
    { 
        ImageSource = "Cerulean.png",
        PictureMode = PictureMode.None,
        BackColor = new Color("#000")
    });

    var window = api.CreateWindow(layout);
    window.AlwaysRedraw = true;
}

var ceruleanApi = CeruleanAPI.GetAPI()
                             .UseSDL2Graphics()
                             .UseConsoleLogger()
                             .Initialize(Callback);

ceruleanApi.WaitForAllWindowsClosed(true);