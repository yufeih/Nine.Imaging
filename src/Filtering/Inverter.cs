// ===============================================================================
// Inverter.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Simple filter for inverting the colors of an image.
    /// </summary>
    /// <remarks>The filter inverts colored and grayscale images.</remarks> 
    public class Inverter : ParallelImageFilter
    {
        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y];

                    color.R = (byte)(255 - color.R);
                    color.G = (byte)(255 - color.G);
                    color.B = (byte)(255 - color.B);

                    target[x, y] = color;
                }
            }
        }
    }
}
