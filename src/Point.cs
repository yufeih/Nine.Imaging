namespace Nine.Imaging
{
    using System;

    public struct Point : IEquatable<Point>
    {
        public int X, Y;

        public Point(int x, int y) { X = x; Y = y; }

        public static readonly Point Zero = new Point();
        public static readonly Point One = new Point(1, 1);

        public bool Equals(Point other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Point && Equals((Point)obj);
        public override int GetHashCode() => (X << 16) ^ Y;

        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);

        public override string ToString() => $"{ X }, { Y }";

        public static Point Parse(string text)
        {
            var parts = text.Split(',');
            return new Point(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
