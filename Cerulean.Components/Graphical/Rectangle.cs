using Cerulean.Common;

namespace Cerulean.Components
{
    public class Rectangle : Component, ISized
    {
        public Size? Size { get; set; } = null;
        public Color? FillColor { get; set; }
        public Color? BorderColor { get; set; }
        public double FillOpacity { get; set; }
        public double BorderOpacity { get; set; }

        public Rectangle()
        {
            FillOpacity = 1.0;
            BorderOpacity = 1.0;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics)
        {
            base.Draw(graphics);
            if (ClientArea is Size fullArea)
            {
                if (Size is Size size)
                    graphics.DrawFilledRectangle(X, Y, size);
                else
                    graphics.DrawFilledRectangle(0, 0, fullArea);
            }

        }
    }
}
