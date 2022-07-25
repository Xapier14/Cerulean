using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    public sealed class ProgressBar : Component, ISized
    {
        private int _oldValue;
        private Size? _oldClientArea = null;
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

        public override void Init()
        {
            base.Init();
            _oldValue = Value;
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if ((_oldClientArea != ClientArea || _oldValue != Value) && window is Window ceruleanWindow)
                ceruleanWindow.FlagForRedraw();

            _oldValue = Value;
            _oldClientArea = ClientArea;

            if (window is not null)
                CallHook(this, EventHook.AfterUpdate, window, clientArea);
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (ClientArea is not { W: > 4, H: > 4 })
                return;

            // cache viewport data to be used by CheckHoveredComponent()
            CacheViewportData(viewportX, viewportY, viewportSize);
            
            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            // draw border rect
            if (BorderColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BorderColor.Value);

            // get percentage
            var value = Math.Max((double)Value / Maximum, 0.0);
            var barX = 2;
            var barY = 2;
            Size barArea = new(ClientArea.Value.W - 4, ClientArea.Value.H - 4);
            Size barBackArea = new(ClientArea.Value.W - 4, ClientArea.Value.H - 4);
                
            // compute bar area
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    barArea.W = ((int)Math.Floor(value * barArea.W));
                    break;
                case Orientation.Vertical:
                    var valueHeight = ((int)Math.Floor(value * barArea.H));
                    barY += ClientArea.Value.H - barArea.H - 4;
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
                graphics.DrawFilledRectangle(2, 2, barBackArea, BackColor.Value);

            // draw bar value rect
            if (ForeColor.HasValue)
                graphics.DrawFilledRectangle(barX, barY, barArea, ForeColor.Value);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}