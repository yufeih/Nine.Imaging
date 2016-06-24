// ===============================================================================
// JpegDecoder.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System.IO;
using BitMiracle.LibJpeg;

namespace Nine.Imaging.Encoding
{
    using System;

    /// <summary>
    /// Image decoder for generating an image out of an jpg stream.
    /// </summary>
    public class JpegDecoder : IImageDecoder
    {
        public int HeaderSize => 11;
        
        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("JPG", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("JPEG", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("JFIF", StringComparison.OrdinalIgnoreCase);
        }
        
        public bool IsSupportedFileFormat(byte[] header)
        {
            bool isSupported = false;

            if (header.Length >= 11)
            {
                bool isJpeg = IsJpeg(header);
                bool isExif = IsExif(header);

                isSupported = isJpeg || isExif;
            }

            return isSupported;
        }

        private bool IsExif(byte[] header)
        {
            bool isExif =
                header[6] == 0x45 && // E
                header[7] == 0x78 && // x
                header[8] == 0x69 && // i
                header[9] == 0x66 && // f
                header[10] == 0x00;

            return isExif;
        }

        private static bool IsJpeg(byte[] header)
        {
            bool isJpg =
                header[6] == 0x4A && // J
                header[7] == 0x46 && // F
                header[8] == 0x49 && // I
                header[9] == 0x46 && // F
                header[10] == 0x00;

            return isJpg;
        }
        
        public Image Decode(Stream stream)
        {
            JpegImage jpg = new JpegImage(stream);

            int pixelWidth = jpg.Width;
            int pixelHeight = jpg.Height;

            byte[] pixels = new byte[pixelWidth * pixelHeight * 4];

            if (!(jpg.Colorspace == Colorspace.RGB && jpg.BitsPerComponent == 8))
            {
                throw new NotSupportedException("JpegDecoder only support RGB color space.");
            }

            for (int y = 0; y < pixelHeight; y++)
            {
                SampleRow row = jpg.GetRow(y);

                for (int x = 0; x < pixelWidth; x++)
                {
                    Sample sample = row.GetAt(x);
                    
                    int offset = (y * pixelWidth + x) * 4;

                    pixels[offset + 0] = (byte)sample[2];
                    pixels[offset + 1] = (byte)sample[1];
                    pixels[offset + 2] = (byte)sample[0];
                    pixels[offset + 3] = (byte)255;
                }
            }

            return new Image(pixelWidth, pixelHeight, pixels);
        }
    }
}
