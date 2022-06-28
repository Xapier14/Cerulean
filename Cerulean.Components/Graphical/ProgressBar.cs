using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class ProgressBar : Component, ISized
    {
        public Size? Size { get; set; }

        public Color? ForeColor { get; set; }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public int Value { get; set; } = 0;
        public int Maximum { get; set; } = 100;
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public ProgressBar()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
            base.Update(window, clientArea);
        }

        public override void Draw(IGraphics graphics)
        {
            Size? area = Size ?? ClientArea;
            if (area is Size backArea && backArea.W > 4 && backArea.H > 4)
            {
                // draw border rect
                if (BorderColor.HasValue)
                    graphics.DrawFilledRectangle(X, Y, backArea, BorderColor.Value);

                // get percentage
                double value = Math.Max((double)Value / Maximum, 0.0);
                int barX = X + 2;
                int barY = Y + 2;
                Size barArea = new(backArea.W - 4, backArea.H - 4);
                Size barBackArea = new(backArea.W - 4, backArea.H - 4);
                
                // compute bar area
                switch (Orientation)
                {
                    case Orientation.Horizontal:
                        barArea.W = ((int)Math.Floor(value * barArea.W));
                        break;
                    case Orientation.Vertical:
                        int valueHeight = ((int)Math.Floor(value * barArea.H));
                        barY += backArea.H - barArea.H - 4;
                        barArea.H = valueHeight;
                        break;
                    case Orientation.HorizontalFlipped:
                        int width = ((int)Math.Floor(value * barArea.W));
                        barX += barArea.W - width;
                        barArea.W = width;
                        break;
                    case Orientation.VerticalFlipped:
                        int height = ((int)Math.Floor(value * barArea.H));
                        barY += barArea.H - height;
                        barArea.H = height;
                        break;
                }

                // draw bar back rect
                if (BackColor.HasValue)
                    graphics.DrawFilledRectangle(X + 2, Y + 2, barBackArea, BackColor.Value);

                // draw bar value rect
                if (ForeColor.HasValue)
                    graphics.DrawFilledRectangle(barX, barY, barArea, ForeColor.Value);
            }
        }
    }
}