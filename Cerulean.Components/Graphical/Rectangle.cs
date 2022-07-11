using Cerulean.Common;

namespace Cerulean.Components
{
    public class Rectangle : Component, ISized
    {
        public Size? Size { get; set; } = null;
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        public Color? FillColor { get; set; }
        public Color? BorderColor { get; set; }
        public double FillOpacity { get; set; }
        public double BorderOpacity { get; set; }

        public Rectangle()
        {
            FillOpacity = 1.0;
            BorderOpacity = 1.0;
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue) return;
            // Draw fill
            if (FillColor.HasValue)
            {
                if (Size.HasValue)
                    graphics.DrawFilledRectangle(0, 0, Size.Value, FillColor.Value);
                else
                    graphics.DrawFilledRectangle(0, 0, ClientArea.Value, FillColor.Value);
            }
            // Draw border
            if (!BorderColor.HasValue) return;
            if (Size.HasValue)
                graphics.DrawRectangle(0, 0, Size.Value, BorderColor.Value);
            else
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);
        }
    }
}
