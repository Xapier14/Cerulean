using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class Panel : Component, ISized
    {
        public Size? Size { get; set; }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public override void Update(object? window, Size clientArea)
        {
            ClientArea = Size ?? clientArea;
            base.Update(window, ClientArea.Value);
        }

        public override void Draw(IGraphics graphics)
        {
            if (Size is null)
            {

            } else 
            {

            }
            base.Draw(graphics);
        }
    }
}