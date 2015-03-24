// ===============================================================================
// MatrixFilter.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    using System;

    /// <summary>
    /// Defines an abstract base filter that uses a matrix a factor and a bias value to 
    /// change the color of a matrix.
    /// </summary>
    public abstract class MatrixFilter : IImageFilter
    {
        #region Fields

        private int _filterSize;
        private double[] _filter;

        #endregion

        #region Properties

        /// <summary>
        /// Initializes this filter with the filter matrix.
        /// </summary>
        protected void Initialize(double[] filter, int filterSize)
        {
            if (filterSize % 2 == 0)
            {
                throw new ArgumentException("The number of filter size cannot be an even number.", "filter");
            }

            _filter = filter;
            _filterSize = filterSize;
        }

        /// <summary>
        /// This method is called before the filter is applied to prepare the filter 
        /// matrix. Only calculate a new matrix, when the properties has been changed.
        /// </summary>
        protected virtual void PrepareFilter() { }

        /// <summary>
        /// Apply filter to an image at the area of the specified rectangle.
        /// </summary>
        /// <param name="target">Target image to apply filter to.</param>
        /// <param name="source">The source image. Cannot be null.</param>
        /// <param name="rectangle">The rectangle, which defines the area of the
        /// image where the filter should be applied to.</param>
        /// <remarks>The method keeps the source image unchanged and returns the
        /// the result of image processing filter as new image.</remarks>
        /// <exception cref="System.ArgumentNullException">
        /// 	<para><paramref name="target"/> is null.</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="target"/> is null.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException"><paramref name="rectangle"/> doesnt fits
        /// to the image.</exception>
        public void Apply(ImageBase target, ImageBase source, Rectangle rectangle)
        {
            PrepareFilter();
            
                int width = rectangle.Width;
                int height = rectangle.Height;

                int sourceWidth = source.PixelWidth;
                int halfFilterSize = _filterSize >> 1;

                byte[] pixels = source.Pixels;

                for (int y = rectangle.Y; y < rectangle.Bottom; y++)
                {
                    for (int x = rectangle.X; x < rectangle.Right; x++)
                    {
                        double r = 0, g = 0, b = 0;

                        for (int filterY = 0; filterY < _filterSize; filterY++)
                        {
                            for (int filterX = 0; filterX < _filterSize; filterX++)
                            {
                                int imageX = (x - halfFilterSize + filterX + width) % width;
                                int imageY = (y - halfFilterSize + filterY + height) % height;

                                int sourceStart = (imageX + imageY * sourceWidth) << 2;

                                byte tr = pixels[sourceStart];
                                byte tg = pixels[sourceStart + 1];
                                byte tb = pixels[sourceStart + 2];

                                double multiplier = _filter[filterX + filterY * _filterSize];

                                r += tr * multiplier;
                                g += tg * multiplier;
                                b += tb * multiplier;
                            }
                        }

                        target[x, y] = new Color(
                            (byte)r.RemainBetween(0, 255),
                            (byte)g.RemainBetween(0, 255),
                            (byte)b.RemainBetween(0, 255));
                    }
                }
        }

        #endregion
    }
}
