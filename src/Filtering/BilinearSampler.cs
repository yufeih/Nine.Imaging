namespace Nine.Imaging.Filtering
{
    using System;

    /// <summary>
    /// Bilinear image resizer, which resizes the image with the bilinear interpolation.
    /// </summary>
    public class BilinearSampler : ParallelImageSampler
    {
        protected override void Sample(Image source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] sourcePixels = source.Pixels;

            double factorX = (double)source.Width / width;
            double factorY = (double)source.Height / height;

            double fractionX, oneMinusX;
            double fractionY, oneMinusY;

            int b, t, l, r;

            double c1, c2, c3, c4, b1, b2;
            double a1, a2, a3, a4, a;

            for (int y = startY; y < endY; y++)
            {
                t = (int)Math.Floor(y * factorY);

                b = t + 1;

                if (b >= source.Height)
                {
                    b = t;
                }

                fractionY = y * factorY - t;

                oneMinusY = 1.0 - fractionY;

                b *= source.Width;
                t *= source.Width;

                for (int x = 0; x < width; x++)
                {
                    int dstOffset = (y * width + x) << 2;

                    l = (int)Math.Floor(x * factorX);

                    r = l + 1;

                    if (r >= source.Width)
                    {
                        r = l;
                    }

                    fractionX = x * factorX - l;

                    oneMinusX = 1.0 - fractionX;

                    a1 = sourcePixels[((t + l) << 2) + 3];
                    a2 = sourcePixels[((t + r) << 2) + 3];
                    a3 = sourcePixels[((b + l) << 2) + 3];
                    a4 = sourcePixels[((b + r) << 2) + 3];
                    
                    b1 = oneMinusX * a1 + fractionX * a2;
                    b2 = oneMinusX * a3 + fractionX * a4;

                    a = (oneMinusY * b1 + fractionY * b2);

                    pixels[dstOffset + 3] = (byte)a;

                    for (int c = 0; c < 3; c++)
                    {
                        c1 = sourcePixels[((t + l) << 2) + c] * a1;
                        c2 = sourcePixels[((t + r) << 2) + c] * a2;
                        c3 = sourcePixels[((b + l) << 2) + c] * a3;
                        c4 = sourcePixels[((b + r) << 2) + c] * a4;

                        b1 = oneMinusX * c1 + fractionX * c2;
                        b2 = oneMinusX * c3 + fractionX * c4;

                        pixels[dstOffset + c] = (byte)((oneMinusY * b1 + fractionY * b2) / a);
                    }
                }
            }
        }
    }
}
