using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class ProgressBar : Component, ISized
    {
        public Size? Size { get; set; }
        public int? HintW { get; set; }
        public int? HintH { get; set; }

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

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            var area = Size ?? ClientArea;
            if (area is not { W: > 4, H: > 4 }) return;
            // draw border rect
            if (BorderColor.HasValue)
                graphics.DrawFilledRectangle(X, Y, area.Value, BorderColor.Value);

            // get percentage
            var value = Math.Max((double)Value / Maximum, 0.0);
            var barX = X + 2;
            var barY = Y + 2;
            Size barArea = new(area.Value.W - 4, area.Value.H - 4);
            Size barBackArea = new(area.Value.W - 4, area.Value.H - 4);
                
            // compute bar area
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    barArea.W = ((int)Math.Floor(value * barArea.W));
                    break;
                case Orientation.Vertical:
                    var valueHeight = ((int)Math.Floor(value * barArea.H));
                    barY += area.Value.H - barArea.H - 4;
                    barArea.H = valueHeight;
                    break;
                case Orientation.HorizontalFlipped:
                    var width = ((int)Math.Floor(value * barArea.W));
                    barX += barArea.W - width;
                    barArea.W = width;
                    break;
                case Orientation.VerticalFlipped:
                    var height = ((int)Math.Floor(value * barArea.H));
                    barY += barArea.H - height;
                    barArea.H = height;
                    break;
                default:
                    throw new GeneralAPIException("Orientation is invalid.");
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