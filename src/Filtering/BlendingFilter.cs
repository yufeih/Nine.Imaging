namespace Nine.Imaging.Filtering
{
    public class BlendingFilter : ParallelImageFilter
    {
        private readonly Image _blendedImage;

        public double? Alpha { get; set; }

        public BlendingFilter(Image blendedImage)
        {
            _blendedImage = blendedImage;
        }

        protected override void Apply(Image target, Image source, Rectangle rectangle, int startY, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    if (y >= _blendedImage.Height || x >= _blendedImage.Width)
                    {
                        target[x, y] = source[x, y];
                        continue;
                    }

                    Color color = source[x, y], blendedColor = _blendedImage[x, y];

                    // combining colors is dependent o the alpha of the blended color
                    double alphaFactor = Alpha != null ? Alpha.Value : blendedColor.A / 255.0;

                    double invertedAlphaFactor = 1 - alphaFactor;

                    int r = (int)(color.R * invertedAlphaFactor) + (int)(blendedColor.R * alphaFactor);
                    int g = (int)(color.G * invertedAlphaFactor) + (int)(blendedColor.G * alphaFactor);
                    int b = (int)(color.B * invertedAlphaFactor) + (int)(blendedColor.B * alphaFactor);

                    r = r.Clamp(0, 255);
                    g = g.Clamp(0, 255);
                    b = b.Clamp(0, 255);
                    
                    target[x, y] = new Color((byte)r, (byte)g, (byte)b);
                }
            }
        }
    }
}
