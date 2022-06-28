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
            if (ClientArea is Size area)
            {
                if (BackColor is Color color)
                {
                    graphics.DrawFilledRectangle(X, Y, area, color);
                }
                base.Draw(graphics);
                if (BorderColor is Color borderColor)
                {
                    graphics.DrawRectangle(X, Y, area, borderColor);
                }
            }
        }
    }
}