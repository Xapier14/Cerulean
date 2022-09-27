using Cerulean.Common;
using Cerulean.Core;
using Cerulean.Core.Input;

namespace Cerulean.Components
{
    public sealed class TextBox : Component, ISized
    {
        private const int PADDING = 4;

        #region Boilerplate Props

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

        #endregion

        #region Colors

        private Color? _backColor = new Color("#FFF");

        public Color? BackColor
        {
            get => _backColor;
            set
            {
                Modified = true;
                _backColor = value;
            }
        }

        private Color? _borderColor = new Color("#FFF");

        public Color? BorderColor
        {
            get => _borderColor;
            set
            {
                Modified = true;
                _borderColor = value;
            }
        }

        private Color? _focusedColor = new Color("#22E");

        public Color? FocusedColor
        {
            get => _focusedColor;
            set
            {
                Modified = true;
                _focusedColor = value;
            }
        }

        private Color? _foreColor = new Color("#FFF");

        public Color? ForeColor
        {
            get => _foreColor;
            set
            {
                Modified = true;
                _foreColor = value;
            }
        }

        private int _maxLength = 2048;
        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = value < 2048 ? value : 2048;
        }

        #endregion

        #region Text Area Passthrough
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
        
        #endregion

        #region State

        private bool _hasFocus = false;
        private bool _clicked = false;

        #endregion

        public TextBox()
        {
            IsHoverableComponent = true;
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = Size ?? clientArea;

            base.Update(window, clientArea);

            if (window is not Window ceruleanWindow)
                return;

            if (Mouse.CheckMouseButton(MouseButton.Left))
            {
                var withinBounds = ceruleanWindow.HoveredComponent == this;
                if (!_clicked)
                {
                    _hasFocus = withinBounds;
                }
                _clicked = withinBounds;

                if (withinBounds && CachedViewportSize.HasValue)
                {
                    ceruleanWindow.StartTextInput(this, CachedViewportX ?? 0, CachedViewportY ?? 0,
                        CachedViewportSize.Value,
                        Text,
                        Text?.Length ?? 0,
                        MaxLength);

                    ceruleanWindow.TextUpdatedDelegate = (text) =>
                    {
                        Text = text;
                    };
                }
                else
                {
                    ceruleanWindow.StopTextInput(this);
                }

                ceruleanWindow.FlagForRedraw();
            }
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;

            CacheViewportData(viewportX, viewportY, viewportSize);

            // draw back rect
            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);

            // text draw position data
            var textX = viewportX + PADDING;
            var textY = viewportY + PADDING;
            var scaledFontSize = Scaling.GetDpiScaledValue((Window)ParentWindow!, FontSize);
            var textViewport = new Size(viewportSize.W - PADDING * 2, viewportSize.H - PADDING * 2);
            
            // try to partition the text to make it short but not too short
            var textPart = Text;
            var (textWidth, _) = graphics.MeasureText(textPart, FontName, FontStyle, scaledFontSize);

            // each loop gets a partition with a length of 25% the previous text
            while (textWidth / 1.25 > textViewport.W)
            {
                textPart = textPart[^(int)Math.Floor(textPart.Length / 1.25)..];
                (textWidth, _) = graphics.MeasureText(textPart, FontName, FontStyle, scaledFontSize);
            }

            var textOffset = textWidth > textViewport.W
                ? textViewport.W - textWidth
                : 0;

            var oldArea = graphics.GetRenderArea(out var oldX, out var oldY);
            graphics.GetGlobalPosition(out var globalX, out var globalY);

            graphics.SetRenderArea(textViewport, textX, textY);
            graphics.SetGlobalPosition(textX + textOffset, textY);

            // draw text
            if (textPart.Length > 0 && ForeColor.HasValue)
            {
                graphics.DrawText(0, 0, textPart, FontName, FontStyle, scaledFontSize, ForeColor.Value, 0, 0, SeedId);
            }

            // draw cursor
            if (_hasFocus && FocusedColor.HasValue)
            {
                var xPos = textWidth >= textViewport.W
                    ? textX + textViewport.W - 1
                    : textX + textWidth;
                graphics.DrawLine(xPos, textY, xPos, textY + textViewport.H, FocusedColor.Value);
            }

            graphics.SetRenderArea(oldArea, oldX, oldY);
            graphics.SetGlobalPosition(globalX, globalY);

            // draw border rect
            if (BorderColor.HasValue && FocusedColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, _hasFocus ? FocusedColor.Value : BorderColor.Value);
        }
        
        public override Component? CheckHoveredComponent(int x, int y)
        {
            if (CachedViewportSize is not { } viewport ||
                CachedViewportX is not { } viewportX ||
                CachedViewportY is not { } viewportY)
                return null;
            if (x < viewportX || x > viewportX + viewport.W ||
                y < viewportY || y > viewportY + viewport.H)
                return null;
            return IsHoverableComponent ? this : null;
        }
    }
}
