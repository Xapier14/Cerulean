using System.Text;
using System.Text.RegularExpressions;
using Cerulean.Core;
using Cerulean.Common;

namespace Cerulean.Components
{
    public class Image : Component, ISized, IVisible
    {
        private const string SVG_CHECK_REGEX = @"[Ss]vg:\s?(.+)";

        private Size? _size;
        public Size? Size
        {
            get => _size;
            set
            {
                Modified = _size != value;
                _size = value;
            }
        }

        private int? _hintW;
        public int? HintW
        {
            get => _hintW;
            set
            {
                Modified = _hintW != value;
                _hintW = value;
            }
        }

        private int? _hintH;
        public int? HintH
        {
            get => _hintH;
            set
            {
                Modified = _hintH != value;
                _hintH = value;
            }
        }

        private string _imageSource = string.Empty;
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                if (!File.Exists(value) && !Regex.Match(value, SVG_CHECK_REGEX).Success)
                    throw new FileNotFoundException($"File {value} not found.");
                Modified = _imageSource != value;
                _imageSource = value;
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
                Modified = _pictureMode != value;
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
                Modified = Math.Abs(_opacity - value) > 0.001;
                _opacity = value;
            }
        }

        private bool _visible = true;

        public bool Visible
        {
            get => _visible;
            set
            {
                Modified = _visible != value;
                _visible = value;
            }
        }

        public Image()
        {
            CanBeParent = false;
        }

        public override void Update(IWindow window, Size clientArea)
        {
            CallHook(this, EventHook.BeforeUpdate, window, clientArea);

            ClientArea = Size ?? clientArea;

            if (Modified)
            {
                Modified = false;
                window.FlagForRedraw();
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
            if (BackColor.HasValue)
            {
                graphics.DrawFilledRectangle(0, 0, ClientArea.Value, BackColor.Value);
            }
            if (ImageSource != string.Empty && Visible)
            {
                var match = Regex.Match(ImageSource, SVG_CHECK_REGEX);
                var useSvgString = match.Success && match.Groups[1].Value != string.Empty;
                if (useSvgString)
                {
                    var bytes = Encoding.UTF8.GetBytes(ImageSource);
                    graphics.DrawImageFromBytes(0, 0, ClientArea.Value, bytes, PictureMode);
                }
                else
                    graphics.DrawImage(0, 0, ClientArea.Value, ImageSource, PictureMode);
            }

            if (BorderColor.HasValue)
                graphics.DrawRectangle(0, 0, ClientArea.Value, BorderColor.Value);

            CallHook(this, EventHook.AfterDraw, graphics, viewportX, viewportY, viewportSize);
        }
    }
}