namespace Nine.Imaging
{
    using System;

    public struct Point : IEquatable<Point>
    {
        public int X, Y;

        public Point(int x, int y) { X = x; Y = y; }

        public static readonly Point Zero = new Point();
        public static readonly Point One = new Point(1, 1);

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            bool result = false;
            if (obj is Point)
            {
                result = Equals((Point)obj);
            }

            return result;
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }
        
        public override int GetHashCode()
        {
            return (X << 16) ^ Y;
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !left.Equals(right);
        }
    }
}
