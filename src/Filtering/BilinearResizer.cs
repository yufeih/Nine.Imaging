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
    public class BilinearResizer : ParallelImageResizer
    {
        protected override void Resize(ImageBase source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] sourcePixels = source.Pixels;

            double factorX = (double)source.PixelWidth / width;
            double factorY = (double)source.PixelHeight / height;

            double fractionX, oneMinusX, l, r;
            double fractionY, oneMinusY, t, b;

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

                for (int x = 0; x < width; x++)
                {
                    int dstOffset = (y * width + x) * 4;

                    l = (int)Math.Floor(x * factorX);

                    r = l + 1;

                    if (r >= source.PixelWidth)
                    {
                        r = l;
                    }

                    fractionX = x * factorX - l;

                    oneMinusX = 1.0 - fractionX;

                    var function = new Func<int, byte>(offset =>
                        {
                            c1 = sourcePixels[(int)((t * source.PixelWidth + l) * 4 + offset)];
                            c2 = sourcePixels[(int)((t * source.PixelWidth + r) * 4 + offset)];
                            c3 = sourcePixels[(int)((b * source.PixelWidth + l) * 4 + offset)];
                            c4 = sourcePixels[(int)((b * source.PixelWidth + r) * 4 + offset)];

                            b1 = (byte)(oneMinusX * c1 + fractionX * c2);
                            b2 = (byte)(oneMinusX * c3 + fractionX * c4);

                            return (byte)(oneMinusY * b1 + fractionY * b2);
                        });

                    pixels[dstOffset + 0] = function(0);
                    pixels[dstOffset + 1] = function(1);
                    pixels[dstOffset + 2] = function(2);
                    pixels[dstOffset + 3] = 255;
                }
            }
        }
    }
}
