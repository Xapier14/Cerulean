using Cerulean.Common;

namespace Cerulean.Components
{
    public class Rectangle : Component, ISized
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

        private Color? _fillColor;
        public Color? FillColor
        {
            get => _fillColor;
            set
            {
                Modified = true;
                _fillColor = value;
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

        private double _fillOpacity;
        public double FillOpacity
        {
            get => _fillOpacity;
            set
            {
                Modified = true;
                _fillOpacity = value;
            }
        }

        private double _borderOpacity;
        public double BorderOpacity
        {
            get => _borderOpacity;
            set
            {
                Modified = true;
                _borderOpacity = value;
            }
        }

        public Rectangle()
        {
            FillOpacity = 1.0;
            BorderOpacity = 1.0;
            CanBeParent = false;
        }

        public override void Update(IWindow window, Size clientArea)
        {
            CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (Modified)
            {
                Modified = false;
                window.FlagForRedraw();
            }

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
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, new Color
                {
                    R = FillColor.Value.R,
                    G = FillColor.Value.G,
                    B = FillColor.Value.B,
                    A = (byte)(255 * FillOpacity)
                });
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