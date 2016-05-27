// ===============================================================================
// TruecolorReader.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;

namespace Nine.Imaging.Png
{
    /// <summary>
    /// Color reader for reading truecolors from a PNG file. Only colors
    /// with 24 or 32 bit (3 or 4 bytes) per pixel are supported at the moment.
    /// </summary>
    sealed class TrueColorReader : IColorReader
    {
        #region Fields

        private int _row;
        private bool _useAlpha;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TrueColorReader"/> class.
        /// </summary>
        /// <param name="useAlpha">if set to <c>true</c> the color reader will also read the
        /// alpha channel from the scanline.</param>
        public TrueColorReader(bool useAlpha)
        {
            _useAlpha = useAlpha;
        }

        #endregion

        #region IPngFormatHandler Members

        /// <summary>
        /// Reads the specified scanline.
        /// </summary>
        /// <param name="scanline">The scanline.</param>
        /// <param name="pixels">The pixels, where the colors should be stored in RGBA format.</param>
        /// <param name="header">The header, which contains information about the png file, like
        /// the width of the image and the height.</param>
        public void ReadScanline(byte[] scanline, byte[] pixels, PngHeader header)
        {
            int offset = 0;

            byte[] newScanline = PngColorReader.ToArrayByBitsLength(scanline, header.BitDepth);
            
            if (_useAlpha)
            {
                for (int x = 0; x < newScanline.Length; x += 4)
                {
                    offset = (_row * header.Width + (x >> 2)) * 4;

                    pixels[offset + 0] = newScanline[x + 2];
                    pixels[offset + 1] = newScanline[x + 1];
                    pixels[offset + 2] = newScanline[x + 0];
                    pixels[offset + 3] = newScanline[x + 3];
                }
            }
            else
            {
                if (header.BitDepth == 8)
                {
                    for (int x = 0; x < newScanline.Length / 3; x++)
                    {
                        offset = (_row * header.Width + x) * 4;

                        pixels[offset + 0] = newScanline[x * 3 + 2];
                        pixels[offset + 1] = newScanline[x * 3 + 1];
                        pixels[offset + 2] = newScanline[x * 3 + 0];
                        pixels[offset + 3] = (byte)255;
                    }
                }
                else
                {
                    for (int x = 0; x < newScanline.Length / 6; x++)
                    {
                        offset = (_row * header.Width + x) * 4;

                        pixels[offset + 0] = newScanline[(x * 6 + 4)];
                        pixels[offset + 1] = newScanline[(x * 6 + 2)];
                        pixels[offset + 2] = newScanline[(x * 6 + 0)];
                        pixels[offset + 3] = (byte)255;
                    }
                }
            }

            _row++;
        }

        #endregion
    }
}
