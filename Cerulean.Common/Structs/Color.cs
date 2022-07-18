using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cerulean.Common
{
    public struct Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color(byte r,
                     byte g,
                     byte b,
                     byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(string hexColor = "#000")
        {
            R = 0;
            G = 0;
            B = 0;
            A = 255;
            SetHexColor(hexColor);
        }

        private static byte FromHex(string str)
            => (byte)int.Parse(str, NumberStyles.HexNumber);

        private static string Repeat(string str, int repetition)
        {
            StringBuilder builder = new();
            for (var i = 0; i < repetition; ++i)
                builder.Append(str);
            return builder.ToString();
        }

        public void SetHexColor(string hexColor)
        {
            // #RGB
            var pattern1 = Regex.Match(hexColor,
                @"^#([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern1.Success)
            {
                R = FromHex(Repeat(pattern1.Groups[1].Value, 2));
                G = FromHex(Repeat(pattern1.Groups[2].Value, 2));
                B = FromHex(Repeat(pattern1.Groups[3].Value, 2));
                return;
            }

            // #RGBA
            var pattern2 = Regex.Match(hexColor,
                @"^#([\da-f])([\da-f])([\da-f])([\da-f])$", RegexOptions.IgnoreCase);
            if (pattern2.Success)
            {
                R = FromHex(Repeat(pattern2.Groups[1].Value, 2));
                G = FromHex(Repeat(pattern2.Groups[2].Value, 2));
                B = FromHex(Repeat(pattern2.Groups[3].Value, 2));
                A = FromHex(Repeat(pattern2.Groups[4].Value, 2));
                return;
            }

            // #RRGGBB
            var pattern3 = Regex.Match(hexColor,
                @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern3.Success)
            {
                R = FromHex(pattern3.Groups[1].Value[..2]);
                G = FromHex(pattern3.Groups[2].Value[..2]);
                B = FromHex(pattern3.Groups[3].Value[..2]);
                return;
            }

            // #RRGGBBAA
            var pattern4 = Regex.Match(hexColor,
                @"^#([\da-f]{2})([\da-f]{2})([\da-f]{2})([\da-f]{2})$", RegexOptions.IgnoreCase);
            if (pattern4.Success)
            {
                R = FromHex(pattern4.Groups[1].Value[..2]);
                G = FromHex(pattern4.Groups[2].Value[..2]);
                B = FromHex(pattern4.Groups[3].Value[..2]);
                A = FromHex(pattern4.Groups[4].Value[..2]);
                return;
            }
        }

        public override string ToString()
        {
            var r = (R < 16 ? "0" : "") + R.ToString("X");
            var g = (G < 16 ? "0" : "") + G.ToString("X");
            var b = (B < 16 ? "0" : "") + B.ToString("X");
            var a = A < 255 ? (A < 16 ? "0" : "") + A.ToString("X") : "";
            return $"#{r}{g}{b}{a}";
        }
    }
}
