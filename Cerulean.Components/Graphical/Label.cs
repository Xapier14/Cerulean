
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Label : Component, ISized
    {
        public Size? Size { get; set; } = null;
        public Color? ForeColor { get; set; }
        public Color? BackColor { get; set; }
        public string Text { get; set; } = "";
        public string FontName { get; set; } = "Arial";
        public int FontSize { get; set; } = 12;

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics)
        {
            base.Draw(graphics);
            if (ClientArea is Size fullArea)
            {
                if (BackColor is Color backColor)
                {
                    if (Size is Size size)
                        graphics.DrawFilledRectangle(X, Y, size, backColor);
                    else
                        graphics.DrawFilledRectangle(0, 0, fullArea, backColor);
                }
                if (ForeColor is Color foreColor && Text != string.Empty)
                {
                    var size = Size ?? fullArea;
                    graphics.DrawText(X, Y, Text, FontName, FontSize, foreColor, (uint)(size.W - X));
                }
            }
        }
    }
}
