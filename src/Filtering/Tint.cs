namespace Nine.Imaging.Filtering
{
    public class Tint : ParallelImageFilter
    {
        public bool UseHsbSpace { get; set; } = false;
        public Color TintColor { get; set; } = Color.White;

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            if (UseHsbSpace)
            {
                HsbColor tint = HsbColor.FromRgb(TintColor);

                for (int y = startY; y < endY; y++)
                {
                    for (int x = rectangle.X; x < rectangle.Right; x++)
                    {
                        Color color = source[x, y];

                        HsbColor hsb = HsbColor.FromRgb(color);

                        hsb.H = tint.H;
                        hsb.A *= tint.A;

                        target[x, y] = hsb.ToRgb();
                    }
                }
            }
            else
            {
                for (int y = startY; y < endY; y++)
                {
                    for (int x = rectangle.X; x < rectangle.Right; x++)
                    {
                        Color color = source[x, y];

                        target[x, y] = new Color(
                            r: (byte)(color.R * TintColor.R / 255),
                            g: (byte)(color.G * TintColor.G / 255),
                            b: (byte)(color.B * TintColor.B / 255),
                            a: color.A);
                    }
                }
            }
        }
    }
}
