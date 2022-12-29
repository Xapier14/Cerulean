using Cerulean.Common;

namespace Cerulean.Core
{
    public static class DrawHooks
    {
        public static void DrawViewportBorder(Component component, object[] args)
        {
            if (args.Length != 4)
                return;

            var graphics = (IGraphics)args[0];
            var viewportSize = (Size)args[3];

            graphics.DrawRectangle(0, 0, viewportSize, new Color("#0F0"));
        }
    }
}
