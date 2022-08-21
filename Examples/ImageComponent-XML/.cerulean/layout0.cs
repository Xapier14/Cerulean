using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean.CLI 
namespace Cerulean.App
{
    public partial class MainLayout : Layout
    {
        public MainLayout() : base()
        {
            var api = CeruleanAPI.GetAPI();
            var resources = api.EmbeddedResources;
            var styles = api.EmbeddedStyles;
            AddChild("MainGrid", new Grid {
                RowCount = 2,
                ColumnCount = 3,
            });
            QueueStyle(GetChild("MainGrid"), styles.FetchStyle("DefaultImageSVG"));
            GetChild("MainGrid").AddChild("Image1", new Image {
                PictureMode = PictureMode.None,
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("Label1", new Label {
                Text = "None",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("Image2", new Image {
                PictureMode = PictureMode.Stretch,
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("Label2", new Label {
                Text = "Stretch",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("Image3", new Image {
                PictureMode = PictureMode.Center,
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("Label3", new Label {
                Text = "Center",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("Image4", new Image {
                PictureMode = PictureMode.Tile,
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("Label4", new Label {
                Text = "Tile",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("Image5", new Image {
                PictureMode = PictureMode.Fit,
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("Label5", new Label {
                Text = "Fit",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("Image6", new Image {
                PictureMode = PictureMode.Cover,
                GridRow = 1,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("Label6", new Label {
                Text = "Cover",
                FontName = "Arial",
                FontSize = 28,
                ForeColor = new Color("#F00"),
                X = 8,
                Y = 8,
                GridRow = 1,
                GridColumn = 2,
            });
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/18/2022 8:40:32 AM
