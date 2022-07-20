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
        public Panel()
        {
            CanBeParent = true;
            CanBeChild = true;
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;
            base.Update(window, ClientArea.Value);

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
            if (Children.Any())
                base.Draw(graphics, viewportX, viewportY, viewportSize);
            if (BorderColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}