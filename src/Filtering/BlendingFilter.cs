// ===============================================================================
// BlendingFilter.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// An <see cref="IImageFilter"/> for adding an overlay image to an <see cref="Image"/>. The transperency
    /// of the overlay is respected, so an alpha value of 255 in the overlay image pixel means that the original image pixel
    /// is fully replaced. A value of 0 means that the original image pixel is not changed at all.
    /// </summary>
    public sealed class BlendingFilter : ParallelImageFilter
    {
        #region Fields

        private readonly ImageBase _blendedImage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the global alpha factor.
        /// </summary>
        /// <value>The global alpha factor.</value>
        public double? Alpha { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlendingFilter"/> class.
        /// </summary>
        /// <param name="blendedImage">The image that should be added to another image when apply is called. Is not allowed to be null.</param>
        /// <exception cref="ArgumentException"><paramref name="blendedImage"/> is null.</exception>
        public BlendingFilter(ImageBase blendedImage)
        {
            _blendedImage = blendedImage;
        }

        #endregion

        #region Methods

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            // Make sure we stop combining when the whole image that should be combined has been processed.
            if (rectangle.Right > _blendedImage.PixelWidth)
            {
                rectangle.Width = _blendedImage.PixelWidth - rectangle.Left;
            }

            if (rectangle.Bottom > _blendedImage.PixelHeight)
            {
                rectangle.Height = _blendedImage.PixelHeight - rectangle.Top;
            }

            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y], blendedColor = _blendedImage[x, y];

                    // combining colors is dependent o the alpha of the blended color
                    double alphaFactor = Alpha != null ? Alpha.Value : blendedColor.A / 255.0;

                    double invertedAlphaFactor = 1 - alphaFactor;

                    int r = (int)(color.R * invertedAlphaFactor) + (int)(blendedColor.R * alphaFactor);
                    int g = (int)(color.G * invertedAlphaFactor) + (int)(blendedColor.G * alphaFactor);
                    int b = (int)(color.B * invertedAlphaFactor) + (int)(blendedColor.B * alphaFactor);

                    r = r.RemainBetween(0, 255);
                    g = g.RemainBetween(0, 255);
                    b = b.RemainBetween(0, 255);

                    color.R = (byte)r;
                    color.G = (byte)g;
                    color.B = (byte)b;

                    target[x, y] = color;
                }
            }
        }

        #endregion
    }
}
