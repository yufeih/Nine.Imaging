// ===============================================================================
// Sepia.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Simple sepia filter, which makes the image look like an old brown photo.
    /// </summary>
    /// <remarks>The filter makes an image look like an old brown photo.</remarks>
    public class Sepia : ParallelImageFilter
    {
        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            byte temp = 0;
            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y];

                    temp = (byte)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);

                    color.R = (byte)((temp > 206) ? 255 : temp + 49);
                    color.G = (byte)((temp < 14) ? 0 : temp - 14);
                    color.B = (byte)((temp < 56) ? 0 : temp - 56);

                    target[x, y] = color;
                }
            }
        }
    }
}
