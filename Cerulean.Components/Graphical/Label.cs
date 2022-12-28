
using Cerulean.Common;
using Cerulean.Core;

namespace Cerulean.Components
{
    [SkipAutoRefGeneration]
    public class Label : Component, ISized
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

        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                Modified = true;
                _text = value;
            }
        }

        private string _fontName = "Arial";
        public string FontName
        {
            get => _fontName;
            set
            {
                Modified = true;
                _fontName = value;
            }
        }

        private int _fontSize = 12;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                Modified = true;
                _fontSize = value;
            }
        }

        private string _fontStyle = string.Empty;
        public string FontStyle
        {
            get => _fontStyle;
            set
            {
                Modified = true;
                _fontStyle = value;
            }
        }

        private bool _wrapText = true;
        public bool WrapText
        {
            get => _wrapText;
            set
            {
                Modified = true;
                _wrapText = value;
            }
        }

        public Label()
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
            if (!ClientArea.HasValue)
                return;

            // cache viewport data to be used by CheckHoveredComponent()
            CacheViewportData(viewportX, viewportY, viewportSize);

            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            if (!ForeColor.HasValue || Text == string.Empty)
                return;

            var window = ParentWindow as Window;

            var size = ClientArea.Value;
            size.W -= X;
            size.H -= Y;
            var textWrap = size.W;

            // only draw a part of the text!

            if (textWrap >= 0)
                graphics.DrawText(0, 0, Text, FontName, FontStyle, Scaling.GetDpiScaledValue(window, FontSize), ForeColor.Value, WrapText ? (uint)(textWrap) : 0, 0, SeedId);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}