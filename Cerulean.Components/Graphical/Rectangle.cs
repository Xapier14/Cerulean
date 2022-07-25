using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    public class Rectangle : Component, ISized
    {
        private Size? _oldClientArea = null;
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
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (_oldClientArea != ClientArea && window is Window ceruleanWindow)
                ceruleanWindow.FlagForRedraw();

            _oldClientArea = ClientArea;

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

            // Draw fill
            if (FillColor.HasValue)
            {
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, FillColor.Value);
            }
            // Draw border
            if (BorderColor.HasValue)
            {
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);
            }

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}