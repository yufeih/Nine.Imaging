// ===============================================================================
// SobelX.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Implements the sobel filter for detection of horizontal edges.
    /// </summary>
    public sealed class SobelX : MatrixFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SobelX"/> class.
        /// </summary>
        public SobelX()
        {
            double[] filter = new double[]
                                {
                                    -1, -2, -1,
                                    0,  0,  0,
                                    1,  2,  1
                                };

            Initialize(filter, 3);
        }
    }
}
