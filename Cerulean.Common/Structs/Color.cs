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
            throw new NotImplementedException();
        }

        public void SetHexColor(string hexColor) => throw new NotImplementedException();

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
