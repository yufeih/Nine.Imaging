// ===============================================================================
// BarcodeResult.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System.Collections.ObjectModel;

namespace Nine.Imaging.Barcode
{
    /// <summary>
    /// Encapsulates the result of decoding a barcode within an image.
    /// </summary>
    public class BarcodeResult
    {
        /// <returns>
        /// Points related to the barcode in the image. These are typically points
        /// identifying finder patterns or the corners of the barcode. The exact meaning is
        /// specific to the type of barcode that was decoded.
        /// </returns>
        /// <value>Points related to the barcode in the image.</value>
        public Collection<Point> Points { get; } = new Collection<Point>();
        
        /// <returns>
        /// Raw bytes encoded by the barcode, if applicable, otherwise null.
        /// </returns>
        /// <value>Raw bytes of the barcode.</value>
        public Collection<int> RawBytes { get; } = new Collection<int>();

        /// <returns> 
        /// Raw text encoded by the barcode, if applicable, otherwise null.
        /// </returns>
        /// <value>Raw text of the barcode.</value>
        public string Text { get; set; }

        /// <returns>
        /// An value representing the format of the barcode that was decoded
        /// </returns>
        /// <value>The format of the barcode.</value>
        public BarcodeResultFormat Format { get; set; }
    }
}
