using Cerulean.Common;

namespace Cerulean.Components
{
    public sealed class StackPanel : Component, ISized
    {
        public Size? Size { get; set; }
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = Size ?? clientArea;
            if (!ClientArea.HasValue)
            {
                X = 0;
                Y = 0;
            }

            foreach (var child in Children)
            {
                //var centerX = ClientArea.Value.W / 2;
                //var centerY = ClientArea.Value.H / 2;
                
                //switch (Orientation)
                //{
                //    // update child x and y and center it on stack panel width if vertical and height if horizontal
                //    case Orientation.Horizontal or Orientation.HorizontalFlipped:
                //        child.Y = centerY - ((child is ISized sized ? sized.HintH : child.ClientArea) ?? clientArea.H);
                //        break;
                //    case Orientation.Vertical or Orientation.VerticalFlipped:
                //        break;
                //    default:
                //        throw new GeneralAPIException("Orientation is invalid.");
                //}
            }
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;
            if (BackColor.HasValue)
                graphics.DrawFilledRectangle(X, Y, ClientArea.Value, BackColor.Value);
            base.Draw(graphics, viewportX, viewportY, viewportSize);
            if (BorderColor.HasValue)
                graphics.DrawRectangle(X, Y, ClientArea.Value, BorderColor.Value);
        }
    }
}