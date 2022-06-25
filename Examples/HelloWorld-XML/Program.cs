using Cerulean.Core;

// Set-up the CeruleanAPI singleton instance
var ceruleanApi = CeruleanAPI.GetAPI()
                             .UseSDL2Graphics()
                             .UseConsoleLogger()
                             .Initialize();

// Create a layout from embedded layout "HelloWorldLayout".
var window = ceruleanApi.CreateWindow("HelloWorldLayout");

// Wait for all windows to close and call Quit() on finish.
ceruleanApi.WaitForAllWindowsClosed(true);