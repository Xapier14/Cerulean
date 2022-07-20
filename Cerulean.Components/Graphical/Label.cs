
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
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (window is not null)
                CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;

            // cache viewport data to be used by CheckHoveredComponent()
            CacheViewportData(viewportX, viewportY, viewportSize);

            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            if (!ForeColor.HasValue || Text == string.Empty) 
                return;

            var size = ClientArea.Value;
            size.W -= X;
            size.H -= Y;
            var textWrap = size.W;
            if (textWrap >= 0)
                graphics.DrawText(0, 0, Text, FontName, FontStyle, FontSize, ForeColor.Value, WrapText ? (uint)(textWrap) : 0);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}