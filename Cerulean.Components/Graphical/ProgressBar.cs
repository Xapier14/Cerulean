using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    [SkipAutoRefGeneration]
    public sealed class ProgressBar : Component, ISized
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

        private Color? _foreColor;
        public Color? ForeColor
        {
            get => _foreColor;
            set
            {
                Modified = true;
                _foreColor = value;
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

        private int _value = 0;
        public int Value
        {
            get => _value;
            set
            {
                Modified = true;
                _value = value;
            }
        }

        private int _maximum = 100;
        public int Maximum
        {
            get => _maximum;
            set
            {
                Modified = true;
                _maximum = value;
            }
        }

        private Orientation _orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                Modified = true;
                _orientation = value;
            }
        }

        public ProgressBar()
        {
            CanBeParent = false;
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

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