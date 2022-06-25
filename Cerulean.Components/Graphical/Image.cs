using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Image : Component, ISized
    {
        public Size? Size { get; set; } = null;
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
        private double _opacity { get; set; } = 1.0;
        public double Opacity
        {
            get => _opacity;
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Opacity must be between 0.0 and 1.0.");
                _opacity = value;
            }
        }

        public override void Update(object? window, Size clientArea)
        {
            ClientArea = clientArea;
        }

        public override void Draw(IGraphics graphics)
        {
            base.Draw(graphics);
            if (ClientArea is Size fullArea)
            {
                if (BackColor is Color backColor)
                {
                    if (Size is Size size)
                        graphics.DrawFilledRectangle(X, Y, size, backColor);
                    else
                        graphics.DrawFilledRectangle(0, 0, fullArea, backColor);
                }
                if (FileName != string.Empty)
                {
                    if (Size is Size size)
                        graphics.DrawImage(X, Y, size, FileName, PictureMode);
                    else
                        graphics.DrawImage(0, 0, fullArea, FileName, PictureMode);
                }
                if (BorderColor is Color borderColor)
                {
                    if (Size is Size size)
                        graphics.DrawRectangle(X, Y, size, borderColor);
                    else
                        graphics.DrawRectangle(0, 0, fullArea, borderColor);
                }
            }
        }
    }
}