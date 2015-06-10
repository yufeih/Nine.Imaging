// ===============================================================================
// BilinearResizer.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    using System;

    /// <summary>
    /// Bilinear image resizer, which resizes the image with the bilinear interpolation.
    /// </summary>
    public class BilinearSampler : ParallelImageSampler
    {
        protected override void Sample(ImageBase source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] sourcePixels = source.Pixels;

            double factorX = (double)source.PixelWidth / width;
            double factorY = (double)source.PixelHeight / height;

            double fractionX, oneMinusX;
            double fractionY, oneMinusY;

            int b, t, l, r;

            byte c1, c2, c3, c4, b1, b2;

            for (int y = startY; y < endY; y++)
            {
                t = (int)Math.Floor(y * factorY);

                b = t + 1;

                if (b >= source.PixelHeight)
                {
                    b = t;
                }

                fractionY = y * factorY - t;

                oneMinusY = 1.0 - fractionY;

                b *= source.PixelWidth;
                t *= source.PixelWidth;

                for (int x = 0; x < width; x++)
                {
                    int dstOffset = (y * width + x) << 2;

                    l = (int)Math.Floor(x * factorX);

                    r = l + 1;

                    if (r >= source.PixelWidth)
                    {
                        r = l;
                    }

                    fractionX = x * factorX - l;

                    oneMinusX = 1.0 - fractionX;

                    for (int c = 0; c < 4; c++)
                    {
                        c1 = sourcePixels[((t + l) << 2) + c];
                        c2 = sourcePixels[((t + r) << 2) + c];
                        c3 = sourcePixels[((b + l) << 2) + c];
                        c4 = sourcePixels[((b + r) << 2) + c];

                        b1 = (byte)(oneMinusX * c1 + fractionX * c2);
                        b2 = (byte)(oneMinusX * c3 + fractionX * c4);

                        pixels[dstOffset + c] = (byte)(oneMinusY * b1 + fractionY * b2);
                    }
                }
            }
        }
    }
}
