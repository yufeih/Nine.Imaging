// ===============================================================================
// Decoders.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.IO
{
    using System;
    using System.Collections.Generic;
    using Nine.Imaging.IO.Jpeg;
    using Nine.Imaging.IO.Bmp;
    using Nine.Imaging.IO.Png;
    
    /// <summary>
    /// Helper methods for decoders.
    /// </summary>
    public static class Decoders
    {
        private static readonly Lazy<List<IImageDecoder>> defaultDecoders = new Lazy<List<IImageDecoder>>(() => new List<IImageDecoder>
        {
            new BmpDecoder(),
            new JpegDecoder(),
            new PngDecoder(),
        });

        public static IList<IImageDecoder> Default
        {
            get { return defaultDecoders.Value; }
        }
    }
}
