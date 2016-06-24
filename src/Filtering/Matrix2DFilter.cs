namespace Nine.Imaging.Filtering
{
    using System;

    /// <summary>
    /// Defines an abstract base filter that uses a matrix a factor and a bias value to 
    /// change the color of a matrix.
    /// </summary>
    public abstract class Matrix2DFilter : ParallelImageFilter
    {
        public abstract double[] KernelX { get; }
        public abstract double[] KernelY { get; }

        protected override void Apply(Image target, Image source, Rectangle rectangle, int startY, int endY)
        {
            double[] kernelX = KernelX;
            double[] kernelY = KernelY;
            int filterSize = (int)Math.Sqrt(kernelX.Length);

            int width = rectangle.Width;
            int height = rectangle.Height;

            int sourceWidth = source.Width;
            int halfFilterSize = filterSize >> 1;

            byte[] pixels = source.Pixels;

            for (int y = startY; y < endY; y++)
            {
                int baseY = y - halfFilterSize + height;
                int right = rectangle.Right;

                for (int x = rectangle.X; x < right; x++)
                {
                    double rX = 0, gX = 0, bX = 0;
                    double rY = 0, gY = 0, bY = 0;

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

                            double multiplierX = kernelX[filterStart + filterX];

                            rX += tr * multiplierX;
                            gX += tg * multiplierX;
                            bX += tb * multiplierX;

                            double multiplierY = kernelY[filterStart + filterX];

                            rY += tr * multiplierY;
                            gY += tg * multiplierY;
                            bY += tb * multiplierY;
                        }
                    }

                    target[x, y] = new Color(
                        r: (byte)((Math.Sqrt(rX * rX + rY * rY)).Clamp(0, 255)),
                        g: (byte)((Math.Sqrt(gX * gX + gY * gY)).Clamp(0, 255)),
                        b: (byte)((Math.Sqrt(bX * bX + bY * bY)).Clamp(0, 255)));
                }
            }
        }
    }
}
