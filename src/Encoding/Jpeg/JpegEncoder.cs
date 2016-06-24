// ===============================================================================
// JpegEncoder.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System.IO;

namespace Nine.Imaging.Encoding
{
    using System;
    using BitMiracle.LibJpeg;

    /// <summary>
    /// Encoder for writing the data image to a stream in jpg format.
    /// </summary>
    public class JpegEncoder : IImageEncoder
    {
        public int Quality { get; set; } = 100;
        
        public string Extension => "jpg";

        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("JPG", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("JPEG", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("JFIF", StringComparison.OrdinalIgnoreCase);
        }

        public void Encode(Image image, Stream stream)
        {
            int pixelWidth  = image.Width;
            int pixelHeight = image.Height;

            byte[] sourcePixels = image.Pixels;

            SampleRow[] rows = new SampleRow[pixelHeight];

            for (int y = 0; y < pixelHeight; y++)
            {
                byte[] samples = new byte[pixelWidth * 3];

                for (int x = 0; x < pixelWidth; x++)
                {
                    int start = x * 3;
                    int source = (y * pixelWidth + x) * 4;

                    samples[start] = sourcePixels[source + 2];
                    samples[start + 1] = sourcePixels[source + 1];
                    samples[start + 2] = sourcePixels[source];
                }

                rows[y] = new SampleRow(samples, pixelWidth, 8, 3);
            }

            JpegImage jpg = new JpegImage(rows, Colorspace.RGB);
            jpg.WriteJpeg(stream, new CompressionParameters { Quality = Quality });
        }
    }
}
