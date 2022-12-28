using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    [SkipAutoRefGeneration]
    public class Panel : Component, ISized
    {
        private Size? _size;
        public Size? Size
        {
            get => _size;
            set
            {
                Modified = true;
                _size = value;
            }
        }

        private int? _hintW;
        public int? HintW
        {
            get => _hintW;
            set
            {
                Modified = true;
                _hintW = value;
            }
        }

        private int? _hintH;
        public int? HintH
        {
            get => _hintH;
            set
            {
                Modified = true;
                _hintH = value;
            }
        }

        private Color? _backColor;
        public Color? BackColor
        {
            get => _backColor;
            set
            {
                Modified = true;
                _backColor = value;
            }
        }

        private Color? _borderColor;
        public Color? BorderColor
        {
            get => _borderColor;
            set
            {
                Modified = true;
                _borderColor = value;
            }
        }
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

            if (Modified && window is Window ceruleanWindow)
            {
                Modified = false;
                ceruleanWindow.FlagForRedraw();
            }

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