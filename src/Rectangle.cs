namespace Nine.Imaging
{
    using System;

    public struct Rectangle : IEquatable<Rectangle>
    {
        public static readonly Rectangle Zero = new Rectangle(0, 0, 0, 0);

        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int Left => X;
        public int Right => X + Width;
        public int Top => Y;
        public int Bottom => Y + Height;

        public Rectangle(int width, int height)
        {
            X = Y = 0;
            Width = width;
            Height = height;
        }

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public Rectangle(Rectangle other)
        {
            X = other.X;
            Y = other.Y;
            Width = other.Width;
            Height = other.Height;
        }

        public bool Equals(Rectangle other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        public override bool Equals(object obj) => obj is Rectangle && Equals((Rectangle)obj);
        public override int GetHashCode() => X ^ Y ^ Width ^ Height;
        public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);
        public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);

        public override string ToString() => $"{ X }, { Y }, { Width }, { Height }";

        public static Rectangle Parse(string text)
        {
            var parts = text.Split(',');
            return parts.Length == 2
                ? new Rectangle(0, 0, int.Parse(parts[0]), int.Parse(parts[1]))
                : new Rectangle(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
        }
    }
}
