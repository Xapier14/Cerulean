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
            QueueStyle(this, styles.FetchStyle("BaseLabel"));
            AddChild("MainGrid", new Grid {
                RowCount = 2,
                ColumnCount = 3,
            });
            QueueStyle(GetChild("MainGrid"), styles.FetchStyle("DefaultImageSVG"));
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814619004", new Image {
                PictureMode = PictureMode.None,
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814628417", new Label {
                Text = "None",
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629075", new Image {
                PictureMode = PictureMode.Stretch,
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629266", new Label {
                Text = "Stretch",
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629302", new Image {
                PictureMode = PictureMode.Center,
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629332", new Label {
                Text = "Center",
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629404", new Image {
                PictureMode = PictureMode.Tile,
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629476", new Label {
                Text = "Tile",
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629505", new Image {
                PictureMode = PictureMode.Fit,
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629533", new Label {
                Text = "Fit",
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629599", new Image {
                PictureMode = PictureMode.Cover,
                GridRow = 1,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967965814629629", new Label {
                Text = "Cover",
                GridRow = 1,
                GridColumn = 2,
            });
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/22/2022 8:23:01 PM
