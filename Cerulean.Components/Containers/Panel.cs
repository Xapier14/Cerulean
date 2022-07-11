using System.Diagnostics;
using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class Panel : Component, ISized
    {
        public Size? Size { get; set; }
        public int? HintW { get; set; }
        public int? HintH { get; set; }
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
            var x = 0;
            var y = 0;
            if (viewportSize.W <= ClientArea.Value.W)
                x = viewportSize.W - ClientArea.Value.W;
            if (viewportSize.H <= ClientArea.Value.H)
                y = viewportSize.H - ClientArea.Value.H;
            //Debug.Assert(graphics.GetRenderArea(out var x, out var y) == viewportSize && x == viewportX && y == viewportY, "Render area is invalid");
            //if (!Children.Any())
                //Console.WriteLine("Viewport: ({0}, {1}) {2}; Panel: ({3}, {4}) {5}", viewportX, viewportY, viewportSize, X, Y, ClientArea.Value);
            if (Parent is Panel)
                Console.WriteLine("Parent Panel: ({0},{1}) {2}\nClientArea: {3}", viewportX, viewportY, viewportSize, ClientArea);
            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(x, y, ClientArea.Value, BackColor.Value);
            base.Draw(graphics, viewportX, viewportY, viewportSize);
            // Debug.Assert(graphics.GetRenderArea(out x, out y) == viewportSize && x == viewportX && y == viewportY, "Render area is invalid");
            if (BorderColor.HasValue)
                graphics.DrawRectangle(x, y, ClientArea.Value, BorderColor.Value);
        }
    }
}
