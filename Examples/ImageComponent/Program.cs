using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;

var ceruleanApi = CeruleanAPI.GetAPI()
                             .UseSDL2Graphics()
                             .UseConsoleLogger()
                             .Initialize();

var layout = new Layout();
layout.AddChild("Image", new Image()
{
    FileName = "Cerulean.png",
    PictureMode = PictureMode.None
});

var window = ceruleanApi.CreateWindow(layout);

ceruleanApi.WaitForAllWindowsClosed(true);