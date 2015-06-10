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
    public class Grayscale : ParallelImageFilter
    {
        public double[] Coefficients { get; set; } = RMY;

        public readonly static double[] BT709 = new[] { 0.2125, 0.7154, 0.0721 };
        public readonly static double[] RMY = new[] { 0.5, 0.419, 0.081 };

        #region Methods

        protected override void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY)
        {
            double _cr = Coefficients[0];
            double _cg = Coefficients[1];
            double _cb = Coefficients[2];

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
