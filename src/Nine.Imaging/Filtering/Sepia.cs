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

                    temp = (byte)(0.299f * color.R + 0.587f * color.G + 0.114f * color.B);

                    target[x, y] = new Color(
                        r: (byte)((temp > 206) ? 255 : temp + 49),
                        g: (byte)((temp < 14) ? 0 : temp - 14),
                        b: (byte)((temp < 56) ? 0 : temp - 56));
                }
            }
        }
    }
}
