using Cerulean.Common;

namespace Cerulean.Media
{
    public partial class Canvas : Component
    {
        private Color[,]? _bitmap;

        [ComponentType("Cerulean.Component.Button")]
        public Component? ResetButton { get; set; }

        public Size DrawArea { get; set; }

        [LateBound]
        public Color BackgroundColor { get; set; }

        public override void Init()
        {
            ClearCanvas();
            base.Init();
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue)
                return;

            // cache viewport data to be used by CheckHoveredComponent()
            CacheViewportData(viewportX, viewportY, viewportSize);

            CallHook(this, EventHook.BeforeDraw, graphics, viewportX, viewportY, viewportSize);

            // Draw Bitmap manually
            if (_bitmap is not null)
            {
                for (var y = 0; y < _bitmap.GetLength(0); ++y)
                {
                    for (var x = 0; x < _bitmap.GetLength(1); ++x)
                    {
                        //draw pixel
                        graphics.DrawPixel(x, y, _bitmap[x, y]);
                    }
                }
            }

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }

        public void ClearCanvas()
        {
            _bitmap = new Color[DrawArea.W, DrawArea.H];
        }
    }
}