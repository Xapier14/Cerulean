using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Image : Component, ISized
    {
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

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics, int viewportX, int viewportY, Size viewportSize)
        {
            if (!ClientArea.HasValue) return;
            if (BackColor.HasValue)
            {
                if (Size.HasValue)
                    graphics.DrawFilledRectangle(X, Y, Size.Value, BackColor.Value);
                else
                    graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            }
            if (FileName != string.Empty)
            {
                if (Size.HasValue)
                    graphics.DrawImage(X, Y, Size.Value, FileName, PictureMode);
                else
                    graphics.DrawImage(0, 0, ClientArea.Value, FileName, PictureMode);
            }

            if (!BorderColor.HasValue) return;
            if (Size.HasValue)
                graphics.DrawRectangle(X, Y, Size.Value, BorderColor.Value);
            else
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);
        }
    }
}