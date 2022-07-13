using Cerulean.Common;

namespace Cerulean.Components
{
    public class Panel : Component, ISized
    {
        public Size? Size { get; set; }
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public Panel()
        {
            CanBeParent = true;
            CanBeChild = true;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = Size ?? clientArea;
            base.Update(window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;
            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            if (Children.Any())
                base.Draw(graphics, viewportX, viewportY, viewportSize);
            if (BorderColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);
        }
    }
}