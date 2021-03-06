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
        public Size? Size { get; set; } = null;
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        public string? Text { get; set; }
        public string FontName { get; set; } = "Arial";
        public int FontSize { get; set; } = 12;
        public string FontStyle { get; set; } = string.Empty;
        public Color? BackColor { get; set; }
        public Color? ForeColor { get; set; }
        public Color? BorderColor { get; set; }
        public Color? HighlightColor { get; set; }
        public Color? ActivatedColor { get; set; }
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
                    OnHover?.Invoke(this, eventArgs);

                    _hovered = true;
                }

                if (Mouse.CheckMouseButton(MouseButton.Left))
                {
                    if (_clicked)
                        return;
                    OnClick?.Invoke(this, eventArgs);

                    _clicked = true;
                }
                else
                {
                    if (_clicked)
                    {
                        OnRelease?.Invoke(this, eventArgs);
                    }

                    _clicked = false;
                }
            }
            else
            {
                if (_hovered)
                {
                    OnLeave?.Invoke(this, eventArgs);
                }

                _hovered = false;
                _clicked = false;
            }
                    
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
    }
}
