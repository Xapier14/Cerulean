
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Label : Component, ISized
    {
        public Size? Size { get; set; } = null;
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        public Color? ForeColor { get; set; }
        public Color? BackColor { get; set; }
        public string Text { get; set; } = "";
        public string FontName { get; set; } = "Arial";
        public int FontSize { get; set; } = 12;
        public string FontStyle { get; set; } = string.Empty;
        public bool WrapText { get; set; } = true;

        public Label()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            Console.WriteLine("viewport: ({0}, {1}) {2}", viewportX, viewportY, viewportSize);
            if (!ClientArea.HasValue) return;
            var clientArea = WrapText ? viewportSize : ClientArea.Value;
            if (BackColor.HasValue)
            {
                if (Size.HasValue)
                    graphics.DrawFilledRectangle(X, Y, Size.Value, BackColor.Value);
                else
                    graphics.DrawFilledRectangle(0, 0, clientArea, BackColor.Value);
            }

            if (!ForeColor.HasValue || Text == string.Empty) return;
            var size = Size ?? clientArea;
            size.W -= X;
            size.H -= Y;
            var textWrap = size.W - X;
            if (textWrap >= 0)
                graphics.DrawText(X, Y, Text, FontName, FontStyle, FontSize, ForeColor.Value, WrapText ? (uint)(textWrap) : 0);
        }
    }
}
