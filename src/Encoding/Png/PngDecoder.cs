using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Nine.Imaging.Png;

namespace Nine.Imaging.Encoding
{
    /// <summary>
    /// Encoder for generating a image out of a png stream.
    /// </summary>
    /// <remarks>
    /// At the moment the following features are supported:
    /// <para>
    /// <b>Filters:</b> all filters are supported.
    /// </para>
    /// <para>
    /// <b>Pixel formats:</b>
    /// <list type="bullet">
    ///     <item>RGB (Truecolor) with alpha (8 bit).</item>
    ///     <item>RGB (Truecolor) without alpha (8 bit).</item>
    ///     <item>Greyscale with alpha (8 bit).</item>
    ///     <item>Greyscale without alpha (8 bit).</item>
    ///     <item>Palette Index with alpha (8 bit).</item>
    ///     <item>Palette Index without alpha (8 bit).</item>
    /// </list>
    /// </para> 
    /// </remarks>
    public class PngDecoder : IImageDecoder
    {
        public static int MaxChunkSize = 1024 * 1024;

        public int HeaderSize => 8;
        
        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("PNG", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSupportedFileFormat(byte[] header)
        {
            return header.Length >= 8 &&
                   header[0] == 0x89 &&
                   header[1] == 0x50 && // P
                   header[2] == 0x4E && // N
                   header[3] == 0x47 && // G
                   header[4] == 0x0D && // CR
                   header[5] == 0x0A && // LF
                   header[6] == 0x1A && // EOF
                   header[7] == 0x0A;   // LF
        }

        public Image Decode(Stream stream) => new PngDecoderCore().Decode(stream);

        struct PngDecoderCore
        {
            private static readonly Dictionary<int, PngColorTypeInformation> _colorTypes = new Dictionary<int, PngColorTypeInformation>();
            private Image _image;
            private Stream _stream;
            private PngHeader _header;

            static PngDecoderCore()
            {
                _colorTypes.Add(0,
                    new PngColorTypeInformation(1, new int[] { 1, 2, 4, 8 },
                        (p, a) => new GrayscaleReader(false)));

                _colorTypes.Add(2,
                    new PngColorTypeInformation(3, new int[] { 8, 16 },
                        (p, a) => new TrueColorReader(false)));

                _colorTypes.Add(3,
                    new PngColorTypeInformation(1, new int[] { 1, 2, 4, 8 },
                        (p, a) => new PaletteIndexReader(p, a)));

                _colorTypes.Add(4,
                    new PngColorTypeInformation(2, new int[] { 8 },
                        (p, a) => new GrayscaleReader(true)));

                _colorTypes.Add(6,
                    new PngColorTypeInformation(4, new int[] { 8 },
                        (p, a) => new TrueColorReader(true)));
            }

            public Image Decode(Stream stream)
            {
                _stream = stream;
                _stream.Seek(8, SeekOrigin.Current);

                bool isEndChunckReached = false;

                PngChunk currentChunk = null;

                byte[] palette = null;
                byte[] paletteAlpha = null;

                using (MemoryStream dataStream = new MemoryStream())
                {
                    while ((currentChunk = ReadChunk()) != null)
                    {
                        if (isEndChunckReached)
                        {
                            throw new ImageFormatException("Image does not end with end chunk.");
                        }

                        if (currentChunk.Type == PngChunkTypes.Header)
                        {
                            ReadHeaderChunk(currentChunk.Data);

                            ValidateHeader();
                        }
                        else if (currentChunk.Type == PngChunkTypes.Data)
                        {
                            dataStream.Write(currentChunk.Data, 0, currentChunk.Data.Length);
                        }
                        else if (currentChunk.Type == PngChunkTypes.Palette)
                        {
                            palette = currentChunk.Data;
                        }
                        else if (currentChunk.Type == PngChunkTypes.PaletteAlpha)
                        {
                            paletteAlpha = currentChunk.Data;
                        }
                        else if (currentChunk.Type == PngChunkTypes.End)
                        {
                            isEndChunckReached = true;
                        }
                    }

                    if (_header.Width > Image.MaxWidth || _header.Height > Image.MaxHeight)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"The input png '{ _header.Width }x{ _header.Height }' is bigger then the max allowed size '{ Image.MaxWidth }x{ Image.MaxHeight }'");
                    }

                    byte[] pixels = new byte[_header.Width * _header.Height * 4];

                    PngColorTypeInformation colorTypeInformation = _colorTypes[_header.ColorType];

                    if (colorTypeInformation != null)
                    {
                        IColorReader colorReader = colorTypeInformation.CreateColorReader(palette, paletteAlpha);

                        ReadScanlines(dataStream, pixels, colorReader, colorTypeInformation);
                    }

                    Image.PremultiplyPixels(pixels);
                    return new Image(_header.Width, _header.Height, pixels);
                }
            }

            private int CalculateScanlineLength(PngColorTypeInformation colorTypeInformation)
            {
                int scanlineLength = (_header.Width * _header.BitDepth * colorTypeInformation.ChannelsPerColor);

                int amount = scanlineLength % 8;
                if (amount != 0)
                {
                    scanlineLength += 8 - amount;
                }

                return scanlineLength / 8;
            }

            private int CalculateScanlineStep(PngColorTypeInformation colorTypeInformation)
            {
                int scanlineStep = 1;

                if (_header.BitDepth >= 8)
                {
                    scanlineStep = (colorTypeInformation.ChannelsPerColor * _header.BitDepth) / 8;
                }

                return scanlineStep;
            }

            private void ReadScanlines(MemoryStream dataStream, byte[] pixels, IColorReader colorReader, PngColorTypeInformation colorTypeInformation)
            {
                dataStream.Position = 0;

                int scanlineLength = CalculateScanlineLength(colorTypeInformation);

                int scanlineStep = CalculateScanlineStep(colorTypeInformation);

                byte[] lastScanline = new byte[scanlineLength];
                byte[] currScanline = new byte[scanlineLength];

                byte a = 0;
                byte b = 0;
                byte c = 0;

                int row = 0, filter = 0, column = -1;

                using (InflaterInputStream compressedStream = new InflaterInputStream(dataStream))
                {
                    int readByte = 0;
                    while ((readByte = compressedStream.ReadByte()) >= 0)
                    {
                        if (column == -1)
                        {
                            filter = readByte;

                            column++;
                        }
                        else
                        {
                            currScanline[column] = (byte)readByte;

                            if (column >= scanlineStep)
                            {
                                a = currScanline[column - scanlineStep];
                                c = lastScanline[column - scanlineStep];
                            }
                            else
                            {
                                a = 0;
                                c = 0;
                            }

                            b = lastScanline[column];

                            if (filter == 1)
                            {
                                currScanline[column] = (byte)(currScanline[column] + a);
                            }
                            else if (filter == 2)
                            {
                                currScanline[column] = (byte)(currScanline[column] + b);
                            }
                            else if (filter == 3)
                            {
                                currScanline[column] = (byte)(currScanline[column] + (byte)((a + b) / 2));
                            }
                            else if (filter == 4)
                            {
                                currScanline[column] = (byte)(currScanline[column] + PaethPredicator(a, b, c));
                            }

                            column++;

                            if (column == scanlineLength)
                            {
                                colorReader.ReadScanline(currScanline, pixels, _header);

                                column = -1;
                                row++;

                                var tmp = currScanline;
                                currScanline = lastScanline;
                                lastScanline = tmp;
                            }
                        }
                    }
                }
            }

            private static byte PaethPredicator(byte a, byte b, byte c)
            {
                byte predicator = 0;

                int p = a + b - c;
                int pa = Math.Abs(p - a);
                int pb = Math.Abs(p - b);
                int pc = Math.Abs(p - c);

                if (pa <= pb && pa <= pc)
                {
                    predicator = a;
                }
                else if (pb <= pc)
                {
                    predicator = b;
                }
                else
                {
                    predicator = c;
                }

                return predicator;
            }

            private void ReadHeaderChunk(byte[] data)
            {
                _header = new PngHeader();

                Array.Reverse(data, 0, 4);
                Array.Reverse(data, 4, 4);

                _header.Width = BitConverter.ToInt32(data, 0);
                _header.Height = BitConverter.ToInt32(data, 4);

                _header.BitDepth = data[8];
                _header.ColorType = data[9];
                _header.FilterMethod = data[11];
                _header.InterlaceMethod = data[12];
                _header.CompressionMethod = data[10];
            }

            private void ValidateHeader()
            {
                if (!_colorTypes.ContainsKey(_header.ColorType))
                {
                    throw new ImageFormatException($"Color type '{ _header.ColorType }' is not supported or not valid.");
                }

                if (!_colorTypes[_header.ColorType].SupportedBitDepths.Contains(_header.BitDepth))
                {
                    throw new ImageFormatException($"Bit depth '{ _header.BitDepth }' is not supported or not valid for color type { _header.ColorType }.");
                }

                if (_header.FilterMethod != 0)
                {
                    throw new ImageFormatException("The png specification only defines 0 as filter method.");
                }

                if (_header.InterlaceMethod != 0)
                {
                    throw new ImageFormatException("Interlacing is not supported.");
                }
            }

            private PngChunk ReadChunk()
            {
                PngChunk chunk = new PngChunk();

                if (ReadChunkLength(chunk) == 0)
                {
                    return null;
                }

                byte[] typeBuffer = ReadChunkType(chunk);

                ReadChunkData(chunk);
                ReadChunkCrc(chunk, typeBuffer);

                return chunk;
            }

            private void ReadChunkCrc(PngChunk chunk, byte[] typeBuffer)
            {
                byte[] crcBuffer = new byte[4];

                int numBytes = _stream.Read(crcBuffer, 0, 4);
                if (numBytes >= 1 && numBytes <= 3)
                {
                    throw new ImageFormatException("Image stream is not valid!");
                }

                Array.Reverse(crcBuffer);

                chunk.Crc = BitConverter.ToUInt32(crcBuffer, 0);

                Crc32 crc = new Crc32();
                crc.Update(typeBuffer);
                crc.Update(chunk.Data);

                if (crc.Value != chunk.Crc)
                {
                    throw new ImageFormatException("CRC Error. PNG Image chunk is corrupt!");
                }
            }

            private void ReadChunkData(PngChunk chunk)
            {
                if (chunk.Length > MaxChunkSize)
                {
                    throw new ArgumentOutOfRangeException($"Png chunk size '{ chunk.Length }' excceeds the '{ MaxChunkSize }'");
                }

                chunk.Data = new byte[chunk.Length];

                _stream.Read(chunk.Data, 0, chunk.Length);
            }

            private byte[] ReadChunkType(PngChunk chunk)
            {
                byte[] typeBuffer = new byte[4];

                int numBytes = _stream.Read(typeBuffer, 0, 4);
                if (numBytes >= 1 && numBytes <= 3)
                {
                    throw new ImageFormatException("Image stream is not valid!");
                }

                char[] chars = new char[4];
                chars[0] = (char)typeBuffer[0];
                chars[1] = (char)typeBuffer[1];
                chars[2] = (char)typeBuffer[2];
                chars[3] = (char)typeBuffer[3];

                chunk.Type = new string(chars);

                return typeBuffer;
            }

            private int ReadChunkLength(PngChunk chunk)
            {
                byte[] lengthBuffer = new byte[4];

                int numBytes = _stream.Read(lengthBuffer, 0, 4);
                if (numBytes >= 1 && numBytes <= 3)
                {
                    throw new ImageFormatException("Image stream is not valid!");
                }

                Array.Reverse(lengthBuffer);

                chunk.Length = BitConverter.ToInt32(lengthBuffer, 0);

                return numBytes;
            }
        }
    }
}
