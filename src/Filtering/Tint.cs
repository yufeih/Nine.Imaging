namespace Nine.Imaging.Filtering
{
    public sealed class Tint : ParallelImageFilter
    {
        public Color TintColor { get; set; } = Color.White;

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y];

                    color.R = (byte)(color.R * TintColor.R / 255);
                    color.G = (byte)(color.G * TintColor.G / 255);
                    color.B = (byte)(color.B * TintColor.B / 255);

                    target[x, y] = color;
                }
            }
        }
    }
}
