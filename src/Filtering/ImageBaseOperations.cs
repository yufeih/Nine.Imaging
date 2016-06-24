// ===============================================================================
// ImageBase_Operations.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;

namespace Nine.Imaging.Filtering
{
    static class ImageBaseOperations
    {
        internal static Image Transform(Image source, RotationType rotationType, FlippingType flippingType)
        {
            Image target;

            switch (rotationType)
            {
                case RotationType.None:
                    {
                        byte[] targetPixels = source.Pixels;
                        byte[] sourcePixels = new byte[targetPixels.Length];

                        Array.Copy(targetPixels, sourcePixels, targetPixels.Length);

                        target = new Image(source.Width, source.Height, sourcePixels);
                    }
                    break;
                case RotationType.Rotate90:
                    {
                        target = Rotate90(source);
                    }
                    break;
                case RotationType.Rotate180:
                    {
                        target = Rotate180(source);
                    }
                    break;
                case RotationType.Rotate270:
                    {
                        target = Rotate270(source);
                    }
                    break;
                default:
                    {
                        target = source.Clone();
                    }
                    break;
            }

            switch (flippingType)
            {
                case FlippingType.Vertical:
                    FlipX(target);
                    break;
                case FlippingType.Horizontal:
                    FlipY(target);
                    break;
            }

            return target;
        }

        private static Image Rotate270(Image source)
        {
            int oldIndex = 0, newIndex = 0;

            byte[] sourcePixels = source.Pixels;
            byte[] targetPixels = new byte[source.Width * source.Height * 4];

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    oldIndex = (y * source.Width + x) * 4;

                    // The new index will be calculated as if the image would be flipped
                    // at the x and the y axis and rotated about 90 degrees.
                    newIndex = ((source.Width - x - 1) * source.Height + y) * 4;

                    targetPixels[newIndex + 0] = sourcePixels[oldIndex + 0];
                    targetPixels[newIndex + 1] = sourcePixels[oldIndex + 1];
                    targetPixels[newIndex + 2] = sourcePixels[oldIndex + 2];
                    targetPixels[newIndex + 3] = sourcePixels[oldIndex + 3];
                }
            }

            return new Image(source.Height, source.Width, targetPixels);
        }

        private static Image Rotate180(Image source)
        {
            int oldIndex = 0, newIndex = 0;

            byte[] sourcePixels = source.Pixels;
            byte[] targetPixels = new byte[source.Width * source.Height * 4];

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    oldIndex = (y * source.Width + x) * 4;

                    // The new index will be calculated as if the image would be flipped
                    // at the x and the y axis.
                    newIndex = ((source.Height - y - 1) * source.Width + source.Width - x - 1) * 4;

                    targetPixels[newIndex + 0] = sourcePixels[oldIndex + 0];
                    targetPixels[newIndex + 1] = sourcePixels[oldIndex + 1];
                    targetPixels[newIndex + 2] = sourcePixels[oldIndex + 2];
                    targetPixels[newIndex + 3] = sourcePixels[oldIndex + 3];
                }
            }

            return new Image(source.Width, source.Height, targetPixels);
        }

        private static Image Rotate90(Image source)
        {
            int oldIndex = 0, newIndex = 0;

            byte[] sourcePixels = source.Pixels;
            byte[] targetPixels = new byte[source.Width * source.Height * 4];

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    oldIndex = (y * source.Width + x) * 4;

                    // The new index will just be calculated by swapping
                    // the x and the y value for the pixel.
                    newIndex = ((x + 1) * source.Height - y - 1) * 4;

                    targetPixels[newIndex + 0] = sourcePixels[oldIndex + 0];
                    targetPixels[newIndex + 1] = sourcePixels[oldIndex + 1];
                    targetPixels[newIndex + 2] = sourcePixels[oldIndex + 2];
                    targetPixels[newIndex + 3] = sourcePixels[oldIndex + 3];
                }
            }

            return new Image(source.Height, source.Width, targetPixels);
        }
        
        private static void FlipX(Image image)
        {
            int oldIndex = 0, newIndex = 0;

            byte[] sourcePixels = image.Pixels;

            byte r, g, b, a;

            for (int y = 0; y < image.Height / 2; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    oldIndex = (y * image.Width + x) * 4;

                    r = sourcePixels[oldIndex + 0];
                    g = sourcePixels[oldIndex + 1];
                    b = sourcePixels[oldIndex + 2];
                    a = sourcePixels[oldIndex + 3];

                    newIndex = ((image.Height - y - 1) * image.Width + x) * 4;

                    sourcePixels[oldIndex + 0] = sourcePixels[newIndex + 0];
                    sourcePixels[oldIndex + 1] = sourcePixels[newIndex + 1];
                    sourcePixels[oldIndex + 2] = sourcePixels[newIndex + 2];
                    sourcePixels[oldIndex + 3] = sourcePixels[newIndex + 3];

                    sourcePixels[newIndex + 0] = r;
                    sourcePixels[newIndex + 1] = g;
                    sourcePixels[newIndex + 2] = b;
                    sourcePixels[newIndex + 3] = a;
                }
            }
        }
        
        private static void FlipY(Image image)
        {
            int oldIndex = 0, newIndex = 0;

            byte[] sourcePixels = image.Pixels;

            byte r, g, b, a;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width / 2; x++)
                {
                    oldIndex = (y * image.Width + x) * 4;

                    r = sourcePixels[oldIndex + 0];
                    g = sourcePixels[oldIndex + 1];
                    b = sourcePixels[oldIndex + 2];
                    a = sourcePixels[oldIndex + 3];

                    newIndex = (y * image.Width + image.Width - x - 1) * 4;

                    sourcePixels[oldIndex + 0] = sourcePixels[newIndex + 0];
                    sourcePixels[oldIndex + 1] = sourcePixels[newIndex + 1];
                    sourcePixels[oldIndex + 2] = sourcePixels[newIndex + 2];
                    sourcePixels[oldIndex + 3] = sourcePixels[newIndex + 3];

                    sourcePixels[newIndex + 0] = r;
                    sourcePixels[newIndex + 1] = g;
                    sourcePixels[newIndex + 2] = b;
                    sourcePixels[newIndex + 3] = a;
                }
            }
        }
        
        internal static Image Crop(Image source, Rectangle bounds)
        {
            if (bounds.Width < 0) throw new ArgumentOutOfRangeException(nameof(bounds));
            if (bounds.Height < 0) throw new ArgumentOutOfRangeException(nameof(bounds));
            if (bounds.Right > source.Width || bounds.Bottom > source.Height) throw new ArgumentOutOfRangeException(nameof(bounds));

            byte[] sourcePixels = source.Pixels;
            byte[] targetPixels = new byte[bounds.Width * bounds.Height * 4];

            for (int y = bounds.Top, i = 0; y < bounds.Bottom; y++, i++)
            {
                Array.Copy(sourcePixels, (y * source.Width + bounds.Left) * 4, targetPixels, i * bounds.Width * 4, bounds.Width * 4);
            }

            return new Image(bounds.Width, bounds.Height, targetPixels);
        }
    }
}
