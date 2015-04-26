// ===============================================================================
// SobelY.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Implements the sobel filter for detection of vertical edges.
    /// </summary>
    public class SobelY : MatrixFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SobelY"/> class.
        /// </summary>
        public SobelY()
        {
            double[] filter = new double[]
                                {
                                    -1, 0, 1,
                                    -2, 0, 2,
                                    -1, 0, 1
                                };

            Initialize(filter, 3);
        }
    }
}
