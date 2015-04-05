// ===============================================================================
// NearestNeighborResizer.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Default image resizer, which resizes the image with the fast known method,
    /// without optimizing the quality of the image. Uses the nearest neighbor interpolation.
    /// </summary>
    public sealed class NearestNeighborResizer : ParallelImageResizer
    {
        protected override void Resize(ImageBase source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] newPixels = new byte[width * height * 4];

            double xFactor = (double)source.PixelWidth / width;
            double yFactor = (double)source.PixelHeight / height;

            int dstOffsetLine = 0;
            int dstOffset = 0;

            int srcOffsetLine = 0;
            int srcOffset = 0;

            byte[] sourcePixels = source.Pixels;

            for (int y = startY; y < endY; y++)
            {
                dstOffsetLine = 4 * width * y;

                // Calculate the line offset at the source image, where the pixels should be get from.
                srcOffsetLine = 4 * source.PixelWidth * (int)(y * yFactor);

                for (int x = 0; x < width; x++)
                {
                    dstOffset = dstOffsetLine + 4 * x;
                    srcOffset = srcOffsetLine + 4 * (int)(x * xFactor);

                    pixels[dstOffset + 0] = sourcePixels[srcOffset + 0];
                    pixels[dstOffset + 1] = sourcePixels[srcOffset + 1];
                    pixels[dstOffset + 2] = sourcePixels[srcOffset + 2];
                    pixels[dstOffset + 3] = sourcePixels[srcOffset + 3];
                }
            }
        }
    }
}
