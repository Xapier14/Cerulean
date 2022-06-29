using System.Diagnostics;
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
            base.Update(window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue) return;
            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(X, Y, ClientArea.Value, BackColor.Value);
            base.Draw(graphics, viewportX, viewportY, viewportSize);
            if (BorderColor.HasValue)
                graphics.DrawRectangle(X, Y, ClientArea.Value, BorderColor.Value);
        }
    }
}
