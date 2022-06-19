namespace Cerulean.Common
{
    public static class Colors
    {
        public static readonly Color? None = null;
        public static readonly Color Transparent = new(0, 0, 0, 0);
    }

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
            return "#???";
        }
    }
}
