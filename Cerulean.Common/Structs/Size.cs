namespace Cerulean.Common
{
    public struct Size
    {
        public int W { get; set; }
        public int H { get; set; }

        public Size(int width, int height)
        {
            W = width;
            H = height;
        }

        public static Size operator +(Size obj1, Size obj2)
        {
            return new Size()
            {
                W = obj1.W + obj2.W,
                H = obj2.H + obj2.H
            };
        }

        public static Size operator -(Size obj1, Size obj2)
        {
            return new Size()
            {
                W = obj1.W - obj2.W,
                H = obj2.H - obj2.H
            };
        }

        public static Size operator *(Size obj1, Size obj2)
        {
            return new Size()
            {
                W = obj1.W * obj2.W,
                H = obj2.H * obj2.H
            };
        }

        public static Size operator /(Size obj1, Size obj2)
        {
            return new Size()
            {
                W = obj1.W / obj2.W,
                H = obj2.H / obj2.H
            };
        }

        public void Set(int w, int h)
        {
            W = w;
            H = h;
        }

        public override string ToString()
        {
            return $"({W}, {H})";
        }
    }
}