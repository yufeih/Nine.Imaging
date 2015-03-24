// ===============================================================================
// Grayscale.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Base class for image grayscaling.
    /// </summary>
    /// <remarks>
    /// This class is the base class for image grayscaling. Other classes should inherit from 
    /// this class and specify coefficients used for image conversion to grayscale.
    /// </remarks>
    public abstract class Grayscale : ParallelImageFilter
    {
        #region Fields

        private double _cr;
        private double _cg;
        private double _cb;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Grayscale"/> class.
        /// </summary>
        /// <param name="redCoefficient">Red coefficient.</param>
        /// <param name="greenCoefficient">Green coefficient.</param>
        /// <param name="blueCoefficient">Blue coefficient.</param>
        protected Grayscale(double redCoefficient, double greenCoefficient, double blueCoefficient)
        {
            _cr = redCoefficient;
            _cg = greenCoefficient;
            _cb = blueCoefficient;
        }

        #endregion

        #region Methods

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            byte temp = 0;

            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    Color color = source[x, y];

                    temp = (byte)(color.R * _cr + color.G * _cg + color.B * _cb);

                    color.R = temp;
                    color.G = temp;
                    color.B = temp;

                    target[x, y] = color;
                }
            }
        }

        #endregion
    }
}
