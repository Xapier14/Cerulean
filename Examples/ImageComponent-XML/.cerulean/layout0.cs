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
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685053682", new Image {
                PictureMode = PictureMode.None,
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685066934", new Label {
                Text = "None",
                GridRow = 0,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068238", new Image {
                PictureMode = PictureMode.Stretch,
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068368", new Label {
                Text = "Stretch",
                GridRow = 0,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068530", new Image {
                PictureMode = PictureMode.Center,
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068601", new Label {
                Text = "Center",
                GridRow = 0,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068751", new Image {
                PictureMode = PictureMode.Tile,
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068825", new Label {
                Text = "Tile",
                GridRow = 1,
                GridColumn = 0,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685068893", new Image {
                PictureMode = PictureMode.Fit,
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685069022", new Label {
                Text = "Fit",
                GridRow = 1,
                GridColumn = 1,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685069092", new Image {
                PictureMode = PictureMode.Cover,
                GridRow = 1,
                GridColumn = 2,
            });
            GetChild("MainGrid").AddChild("AnonymousComponent_637967968685069163", new Label {
                Text = "Cover",
                GridRow = 1,
                GridColumn = 2,
            });
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/22/2022 8:27:48 PM
