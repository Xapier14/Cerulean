using System.Reflection.Metadata;
using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Image : Component, ISized
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

        private string _imagePath = string.Empty;
        public string FileName
        {
            get => _imagePath;
            set
            {
                if (!File.Exists(value))
                    throw new FileNotFoundException($"File {value} not found.");
                Modified = true;
                _imagePath = value;
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

        private PictureMode _pictureMode = PictureMode.None;
        public PictureMode PictureMode
        {
            get => _pictureMode;
            set
            {
                Modified = true;
                _pictureMode = value;
            }
        }
        private double _opacity = 1.0;
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (value is < 0.0 or > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Opacity must be between 0.0 and 1.0.");
                Modified = true;
                _opacity = value;
            }
        }

        public Image()
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