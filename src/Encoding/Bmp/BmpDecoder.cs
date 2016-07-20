namespace Nine.Imaging.Encoding
{
    using System;
    using System.IO;
    using Nine.Imaging.Bmp;

    /// <summary>
    /// Image decoder for generating an image out of an windows 
    /// bitmap _stream.
    /// </summary>
    /// <remarks>
    /// Does not support the following formats at the moment:
    /// <list type="bullet">
    /// 	<item>JPG</item>
    /// 	<item>PNG</item>
    /// 	<item>RLE4</item>
    /// 	<item>RLE8</item>
    /// 	<item>BitFields</item>
    /// </list>
    /// Formats will be supported in a later realease. We advice always 
    /// to use only 24 Bit windows bitmaps.
    /// </remarks>
    public class BmpDecoder : IImageDecoder
    {
        public int HeaderSize => 2;

        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("BMP", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("DIP", StringComparison.OrdinalIgnoreCase);
        }
        
        public bool IsSupportedFileFormat(byte[] header)
        {
            bool isBmp = false;
            if (header.Length >= 2)
            {
                isBmp =
                    header[0] == 0x42 && // B
                    header[1] == 0x4D;   // M
            }

            return isBmp;
        }

        public Image Decode(Stream stream) => new BmpDecoderCore().Decode(stream);

        struct BmpDecoderCore
        {
            /// <summary>
            /// The mask for the red part of the color for 16 bit rgb bitmaps.
            /// </summary>
            private const int Rgb16RMask = 0x00007C00;
            /// <summary>
            /// The mask for the green part of the color for 16 bit rgb bitmaps.
            /// </summary>
            private const int Rgb16GMask = 0x000003E0;
            /// <summary>
            /// The mask for the blue part of the color for 16 bit rgb bitmaps.
            /// </summary>
            private const int Rgb16BMask = 0x0000001F;

            private Stream _stream;
            private BmpFileHeader _fileHeader;
            private BmpInfoHeader _infoHeader;

            public Image Decode(Stream stream)
            {
                _stream = stream;

                try
                {
                    ReadFileHeader();
                    ReadInfoHeader();

                    int colorMapSize = -1;

                    if (_infoHeader.ClrUsed == 0)
                    {
                        if (_infoHeader.BitsPerPixel == 1 ||
                            _infoHeader.BitsPerPixel == 4 ||
                            _infoHeader.BitsPerPixel == 8)
                        {
                            colorMapSize = (int)Math.Pow(2, _infoHeader.BitsPerPixel) * 4;
                        }
                    }
                    else
                    {
                        colorMapSize = _infoHeader.ClrUsed * 4;
                    }

                    byte[] palette = null;

                    if (colorMapSize > 0)
                    {
                        if (colorMapSize > 255 * 4)
                        {
                            throw new ImageFormatException($"Invalid bmp colormap size '{ colorMapSize }'");
                        }

                        palette = new byte[colorMapSize];

                        _stream.Read(palette, 0, colorMapSize);
                    }

                    if (_infoHeader.Width > Image.MaxWidth || _infoHeader.Height > Image.MaxHeight)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"The input bitmap '{ _infoHeader.Width }x{ _infoHeader.Height }' is bigger then the max allowed size '{ Image.MaxWidth }x{ Image.MaxHeight }'");
                    }

                    byte[] pixels = new byte[_infoHeader.Width * _infoHeader.Height * 4];

                    switch (_infoHeader.Compression)
                    {
                        case BmpCompression.RGB:
                            if (_infoHeader.HeaderSize != 40)
                            {
                                throw new ImageFormatException(
                                    $"Header Size value '{_infoHeader.HeaderSize}' is not valid.");
                            }

                            if (_infoHeader.BitsPerPixel == 32)
                            {
                                ReadRgb32(pixels, _infoHeader.Width, _infoHeader.Height);
                            }
                            else if (_infoHeader.BitsPerPixel == 24)
                            {
                                ReadRgb24(pixels, _infoHeader.Width, _infoHeader.Height);
                            }
                            else if (_infoHeader.BitsPerPixel == 16)
                            {
                                ReadRgb16(pixels, _infoHeader.Width, _infoHeader.Height);
                            }
                            else if (_infoHeader.BitsPerPixel <= 8)
                            {
                                ReadRgbPalette(pixels, palette,
                                    _infoHeader.Width,
                                    _infoHeader.Height,
                                    _infoHeader.BitsPerPixel);
                            }
                            break;
                        default:
                            throw new NotSupportedException("Does not support this kind of bitmap files.");
                    }

                    return new Image(_infoHeader.Width, _infoHeader.Height, pixels);
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new ImageFormatException("Bitmap does not have a valid format.", e);
                }
            }

            private void ReadRgbPalette(byte[] imageData, byte[] colors, int width, int height, int bits)
            {
                // Pixels per byte (bits per pixel)
                int ppb = 8 / bits;

                int arrayWidth = (width + ppb - 1) / ppb;

                // Bit mask
                int mask = (0xFF >> (8 - bits));

                byte[] data = new byte[(arrayWidth * height)];

                _stream.Read(data, 0, data.Length);

                // Rows are aligned on 4 byte boundaries
                int alignment = arrayWidth % 4;
                if (alignment != 0)
                {
                    alignment = 4 - alignment;
                }

                int offset, row, rowOffset, colOffset, arrayOffset;

                for (int y = 0; y < height; y++)
                {
                    rowOffset = y * (arrayWidth + alignment);

                    for (int x = 0; x < arrayWidth; x++)
                    {
                        offset = rowOffset + x;

                        // Revert the y value, because bitmaps are saved from down to top
                        row = Invert(y, height);

                        colOffset = x * ppb;

                        for (int shift = 0; shift < ppb && (colOffset + shift) < width; shift++)
                        {
                            int colorIndex = ((data[offset]) >> (8 - bits - (shift * bits))) & mask;

                            arrayOffset = (row * width + (colOffset + shift)) * 4;
                            imageData[arrayOffset + 0] = colors[colorIndex * 4 + 0];
                            imageData[arrayOffset + 1] = colors[colorIndex * 4 + 1];
                            imageData[arrayOffset + 2] = colors[colorIndex * 4 + 2];

                            imageData[arrayOffset + 3] = (byte)255;

                        }
                    }
                }
            }

            private void ReadRgb16(byte[] imageData, int width, int height)
            {
                byte r, g, b;

                int scaleR = 256 / 32;
                int scaleG = 256 / 64;

                int alignment = 0;
                byte[] data = GetImageArray(width, height, 2, ref alignment);

                int offset, row, rowOffset, arrayOffset;

                for (int y = 0; y < height; y++)
                {
                    rowOffset = y * (width * 2 + alignment);

                    // Revert the y value, because bitmaps are saved from down to top
                    row = Invert(y, height);

                    for (int x = 0; x < width; x++)
                    {
                        offset = rowOffset + x * 2;

                        short temp = BitConverter.ToInt16(data, offset);

                        r = (byte)(((temp & Rgb16RMask) >> 11) * scaleR);
                        g = (byte)(((temp & Rgb16GMask) >> 5) * scaleG);
                        b = (byte)(((temp & Rgb16BMask)) * scaleR);

                        arrayOffset = (row * width + x) * 4;
                        imageData[arrayOffset + 0] = b;
                        imageData[arrayOffset + 1] = g;
                        imageData[arrayOffset + 2] = r;

                        imageData[arrayOffset + 3] = (byte)255;
                    }
                }
            }

            private void ReadRgb24(byte[] imageData, int width, int height)
            {
                int alignment = 0;
                byte[] data = GetImageArray(width, height, 3, ref alignment);

                int offset, row, rowOffset, arrayOffset;

                for (int y = 0; y < height; y++)
                {
                    rowOffset = y * (width * 3 + alignment);

                    // Revert the y value, because bitmaps are saved from down to top
                    row = Invert(y, height);

                    for (int x = 0; x < width; x++)
                    {
                        offset = rowOffset + x * 3;

                        arrayOffset = (row * width + x) * 4;
                        imageData[arrayOffset + 0] = data[offset + 0];
                        imageData[arrayOffset + 1] = data[offset + 1];
                        imageData[arrayOffset + 2] = data[offset + 2];

                        imageData[arrayOffset + 3] = (byte)255;
                    }
                }
            }

            private void ReadRgb32(byte[] imageData, int width, int height)
            {
                int alignment = 0;
                byte[] data = GetImageArray(width, height, 4, ref alignment);

                int offset, row, rowOffset, arrayOffset;

                for (int y = 0; y < height; y++)
                {
                    rowOffset = y * (width * 4 + alignment);

                    // Revert the y value, because bitmaps are saved from down to top
                    row = Invert(y, height);

                    for (int x = 0; x < width; x++)
                    {
                        offset = rowOffset + x * 4;

                        arrayOffset = (row * width + x) * 4;
                        imageData[arrayOffset + 0] = data[offset + 0];
                        imageData[arrayOffset + 1] = data[offset + 1];
                        imageData[arrayOffset + 2] = data[offset + 2];

                        imageData[arrayOffset + 3] = (byte)255;
                    }
                }
            }

            private static int Invert(int y, int height)
            {
                int row = 0;

                if (height > 0)
                {
                    row = (height - y - 1);
                }
                else
                {
                    row = y;
                }

                return row;
            }

            private byte[] GetImageArray(int width, int height, int bytes, ref int alignment)
            {
                int dataWidth = width;

                alignment = (width * bytes) % 4;

                if (alignment != 0)
                {
                    alignment = 4 - alignment;
                }

                int size = (dataWidth * bytes + alignment) * height;

                byte[] data = new byte[size];

                _stream.Read(data, 0, size);

                return data;
            }

            private void ReadInfoHeader()
            {
                byte[] data = new byte[BmpInfoHeader.Size];

                _stream.Read(data, 0, BmpInfoHeader.Size);

                _infoHeader = new BmpInfoHeader();
                _infoHeader.HeaderSize = BitConverter.ToInt32(data, 0);
                _infoHeader.Width = BitConverter.ToInt32(data, 4);
                _infoHeader.Height = BitConverter.ToInt32(data, 8);
                _infoHeader.Planes = BitConverter.ToInt16(data, 12);
                _infoHeader.BitsPerPixel = BitConverter.ToInt16(data, 14);
                _infoHeader.ImageSize = BitConverter.ToInt32(data, 20);
                _infoHeader.XPelsPerMeter = BitConverter.ToInt32(data, 24);
                _infoHeader.YPelsPerMeter = BitConverter.ToInt32(data, 28);
                _infoHeader.ClrUsed = BitConverter.ToInt32(data, 32);
                _infoHeader.ClrImportant = BitConverter.ToInt32(data, 36);
                _infoHeader.Compression = (BmpCompression)BitConverter.ToInt32(data, 16);
            }

            private void ReadFileHeader()
            {
                byte[] data = new byte[BmpFileHeader.Size];

                _stream.Read(data, 0, BmpFileHeader.Size);

                _fileHeader = new BmpFileHeader();
                _fileHeader.Type = BitConverter.ToInt16(data, 0);
                _fileHeader.FileSize = BitConverter.ToInt32(data, 2);
                _fileHeader.Reserved = BitConverter.ToInt32(data, 6);
                _fileHeader.Offset = BitConverter.ToInt32(data, 10);
            }
        }
    }
}
