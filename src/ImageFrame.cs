// ===============================================================================
// ImageFrame.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging
{
    /// <summary>
    /// Represents a frame in a animation.
    /// </summary>
    public class ImageFrame : ImageBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFrame"/> class
        /// by making a copy from another image.
        /// </summary>
        /// <param name="other">The other image, where the clone should be made from.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is null
        /// (Nothing in Visual Basic).</exception>
        public ImageFrame(ImageFrame other) : base(other) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageFrame"/> class.
        /// </summary>
        public ImageFrame() { }

        #endregion
    }
}
