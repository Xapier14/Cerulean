using Cerulean;
using Cerulean.Core;
using Cerulean.Common;
using Cerulean.Components;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
// Generated with Cerulean.CLI 
namespace Cerulean.App
{
    public partial class DefaultImageSVG : Style
    {
        public DefaultImageSVG() : base()
        {
            var api = CeruleanAPI.GetAPI();
            var resources = api.EmbeddedResources;
            var styles = api.EmbeddedStyles;
            TargetType = typeof(Image);
            ApplyToChildren = true;
            AddSetter("ImageSource", "svg: <svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 93 107\"><path d=\"M74,74a42,42 0,1,0-57,0l28,29a42,41 0,0,0 0-57\" fill=\"#ff0000\" fill-rule=\"evenodd\"/></svg>");
        }
    }
}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
// Generated on: 8/18/2022 8:40:32 AM
