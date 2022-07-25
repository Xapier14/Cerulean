using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Image : Component, ISized
    {
        private Size? _oldClientArea = null;
        private string _oldImagePath = string.Empty;
        public Size? Size { get; set; } = null;
        public int? HintW { get; set; }
        public int? HintH { get; set; }
        private string _imagePath = string.Empty;
        public string FileName
        {
            get => _imagePath;
            set
            {
                if (!File.Exists(value))
                    throw new FileNotFoundException($"File {value} not found.");
                _imagePath = value;
            }
        }
        public Color? BackColor { get; set; }
        public Color? BorderColor { get; set; }
        public PictureMode PictureMode { get; set; } = PictureMode.None;
        private double _opacity = 1.0;
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (value is < 0.0 or > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Opacity must be between 0.0 and 1.0.");
                _opacity = value;
            }
        }

        public Image()
        {
            CanBeParent = false;
        }

        public override void Init()
        {
            base.Init();
            _oldImagePath = _imagePath;
        }

        public override void Update(object? window, Size clientArea)
        {
            if (window is not null)
                CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if ((_oldClientArea != ClientArea || _oldImagePath != _imagePath) && window is Window ceruleanWindow)
                ceruleanWindow.FlagForRedraw();

            _oldClientArea = ClientArea;
            _oldImagePath = _imagePath;

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
            {
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            }
            if (FileName != string.Empty)
            {
                graphics.DrawImage(0, 0, ClientArea.Value, FileName, PictureMode);
            }

            if (BorderColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}