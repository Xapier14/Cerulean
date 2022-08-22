using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean.CLI 
namespace Cerulean.App
{
    public partial class BaseLabel : Style
    {
        public BaseLabel() : base()
        {
            var api = CeruleanAPI.GetAPI();
            var resources = api.EmbeddedResources;
            var styles = api.EmbeddedStyles;
            TargetType = typeof(Label);
            ApplyToChildren = true;
            AddSetter("FontName", "Arial");
            AddSetter("FontSize", 28);
            AddSetter("ForeColor", new Color("#FFF"));
            AddSetter("X", 8);
            AddSetter("Y", 8);
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/22/2022 8:27:48 PM
