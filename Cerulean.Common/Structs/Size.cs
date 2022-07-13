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

        public Size(Size size)
        {
            W = size.W;
            H = size.H;
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

        public static bool operator ==(Size obj1, Size obj2)
        {
            return obj1.W == obj2.W && obj1.H == obj2.H;
        }

        public static bool operator !=(Size obj1, Size obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Size size)
                return size == this;
            return false;
        }

        public override int GetHashCode()
        {
            return W.GetHashCode() ^ H.GetHashCode();
        }

        public override string ToString()
        {
            return $"({W}x{H})";
        }
    }
}