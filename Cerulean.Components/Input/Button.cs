using Cerulean.Common;
using Cerulean.Core;
using Cerulean.Core.Input;

namespace Cerulean.Components
{
    public class ButtonEventArgs : EventArgs
    {
        public Window? CallingWindow { get; set; }
        public int MouseX { get; set; }
        public int MouseY { get; set; }
    }
    public sealed class Button : Component, ISized
    {
        private bool _hovered = false;
        private bool _clicked = false;

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

        private string? _text;
        public string? Text
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

        private Color? _highlightColor;
        public Color? HighlightColor
        {
            get => _highlightColor;
            set
            {
                Modified = true;
                _highlightColor = value;
            }
        }

        private Color? _activatedColor;
        public Color? ActivatedColor
        {
            get => _activatedColor;
            set
            {
                Modified = true;
                _activatedColor = value;
            }
        }

        public delegate void ButtonEventHandler(object sender, ButtonEventArgs e);

        public event ButtonEventHandler? OnClick;
        public event ButtonEventHandler? OnRelease;
        public event ButtonEventHandler? OnHover;
        public event ButtonEventHandler? OnLeave;

        public Button()
        {
            IsHoverableComponent = true;
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

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            base.Update(window, clientArea);

            // process events
            if (window is not Window ceruleanWindow)
                return;
            var hovering = ceruleanWindow.HoveredComponent == this;

            // prepare event args
            var coords = Mouse.GetWindowMousePosition(ceruleanWindow);
            var eventArgs = new ButtonEventArgs
            {
                CallingWindow = ceruleanWindow,
                MouseX = coords?.Item1 ?? 0,
                MouseY = coords?.Item2 ?? 0
            };

            if (hovering)
            {
                if (!_hovered)
                {
                    ceruleanWindow.FlagForRedraw();
                    OnHover?.Invoke(this, eventArgs);

                    _hovered = true;
                }

                if (Mouse.CheckMouseButton(MouseButton.Left))
                {
                    if (_clicked)
                        return;
                    RaiseHandler(OnClick, eventArgs, ceruleanWindow);

                    _clicked = true;
                }
                else
                {
                    if (_clicked)
                    {
                        RaiseHandler(OnRelease, eventArgs, ceruleanWindow);
                    }

                    _clicked = false;
                }
            }
            else
            {
                if (_hovered)
                {
                    RaiseHandler(OnLeave, eventArgs, ceruleanWindow);
                }

                _hovered = false;
                _clicked = false;
            }

            if (Modified)
            {
                Modified = false;
                ceruleanWindow.FlagForRedraw();
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

            // draw background
            var backgroundColor = BackColor;
            if (_hovered)
            {
                backgroundColor = _clicked ?
                    ActivatedColor :
                    HighlightColor;
            }
            if (backgroundColor.HasValue)
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, backgroundColor.Value);

            // draw child components
            if (Children.Any())
                base.Draw(graphics, viewportX, viewportY, viewportSize);

            // draw text
            if (Text is { } and not "" &&
                FontName is not "" &&
                FontSize > 0 &&
                ForeColor.HasValue)
            {
                var (w, h) = graphics.MeasureText(Text, FontName, FontStyle, FontSize, viewportSize.W);
                var textX = Math.Max(0, viewportSize.W - w) / 2;
                var textY = Math.Max(0, viewportSize.H - h) / 2;

                graphics.DrawText(textX, textY, Text, FontName, FontStyle, FontSize, ForeColor.Value, (uint)viewportSize.W);
            }

            // draw border
            if (BorderColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }

        private void RaiseHandler(ButtonEventHandler? handler, ButtonEventArgs eventArgs, Window ceruleanWindow)
        {
            ceruleanWindow.FlagForRedraw();
            handler?.Invoke(this, eventArgs);
        }
    }
}
