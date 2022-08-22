using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean.CLI 
namespace Cerulean.App
{
    public partial class HelloWorldLayout : Layout
    {
        public HelloWorldLayout() : base()
        {
            var api = CeruleanAPI.GetAPI();
            var resources = api.EmbeddedResources;
            var styles = api.EmbeddedStyles;
            AddChild("AnonymousComponent_637967969417929746", new Grid {
                RowCount = 2,
                ColumnCount = 2,
                BackColor = new Color("#000"),
            });
            GetChild("AnonymousComponent_637967969417929746").AddChild("AnonymousComponent_637967969418050280", new Label {
                Text = "Hello World!",
                ForeColor = new Color("#FFF"),
                GridColumn = 1,
                GridRow = 1,
                FontName = "Arial",
                FontSize = 24,
                X = 0,
                Y = 0,
            });
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/22/2022 8:29:01 PM
