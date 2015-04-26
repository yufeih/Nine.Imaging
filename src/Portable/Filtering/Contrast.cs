// ===============================================================================
// Contrast.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// An <see cref="IImageFilter"/> to change the contrast of an <see cref="Image"/>.
    /// </summary>
    public class Contrast : ParallelImageFilter
    {
        public int Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Contrast"/> class and sets 
        /// the new contrast of the image.
        /// </summary>
        /// <param name="contrast">The new contrast of the image. Must be between -100 and 100.</param>      
        /// <exception cref="ArgumentException">
        /// 	<para><paramref name="contrast"/> is less than -100.</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="contrast"/> is greater than 100.</para>
        /// </exception>
        public Contrast(int contrast)
        {
            Value = contrast;
        }
        
        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            double pixel = 0, contrast = (100.0 + Value) / 100.0;

            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y];

                    pixel = color.R / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    pixel = pixel.RemainBetween(0, 255);

                    color.R = (byte)pixel;

                    pixel = color.G / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    pixel = pixel.RemainBetween(0, 255);

                    color.G = (byte)pixel;

                    pixel = color.B / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    pixel = pixel.RemainBetween(0, 255);

                    color.B = (byte)pixel;

                    target[x, y] = color;
                }
            }
        }
    }
}
