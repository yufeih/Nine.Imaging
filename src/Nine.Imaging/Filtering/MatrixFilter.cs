namespace Nine.Imaging.Filtering
{
    using System;

    /// <summary>
    /// Defines an abstract base filter that uses a matrix a factor and a bias value to 
    /// change the color of a matrix.
    /// </summary>
    public abstract class MatrixFilter : ParallelImageFilter
    {
        public abstract double[] Kernel { get; }

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            double[] filter = Kernel;
            int filterSize = (int)Math.Sqrt(filter.Length);

            int width = rectangle.Width;
            int height = rectangle.Height;

            int sourceWidth = source.PixelWidth;
            int halfFilterSize = filterSize >> 1;

            byte[] pixels = source.Pixels;

            for (int y = startY; y < endY; y++)
            {
                int baseY = y - halfFilterSize + height;
                int right = rectangle.Right;

                for (int x = rectangle.X; x < right; x++)
                {
                    double r = 0, g = 0, b = 0;

                    int baseX = x - halfFilterSize + width;

                    for (int filterY = 0; filterY < filterSize; filterY++)
                    {
                        int filterStart = filterY * filterSize;
                        int imageY = ((baseY + filterY) % height) * sourceWidth;

                        for (int filterX = 0; filterX < filterSize; filterX++)
                        {
                            int sourceStart = ((baseX + filterX) % width + imageY) << 2;

                            byte tb = pixels[sourceStart];
                            byte tg = pixels[sourceStart + 1];
                            byte tr = pixels[sourceStart + 2];

                            double multiplier = filter[filterStart + filterX];

                            r += tr * multiplier;
                            g += tg * multiplier;
                            b += tb * multiplier;
                        }
                    }

                    target[x, y] = new Color(
                        (byte)r.Clamp(0, 255),
                        (byte)g.Clamp(0, 255),
                        (byte)b.Clamp(0, 255));
                }
            }
        }
    }
}
