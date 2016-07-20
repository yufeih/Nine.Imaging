// ===============================================================================
// PngEncoder.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Nine.Imaging.Png;

namespace Nine.Imaging.Encoding
{
    /// <summary>
    /// Image encoder for writing image data to a stream in png format.
    /// </summary>
    public class PngEncoder : IImageEncoder
    {
        private const int MaxBlockSize = 0xFFFF;

        /// <summary>
        /// Gets the default file extension for this encoder.
        /// </summary>
        /// <value>The default file extension for this encoder.</value>
        public string Extension => "png";

        /// <summary>
        /// Indicates if the image encoder supports the specified
        /// file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns><c>true</c>, if the encoder supports the specified
        /// extensions; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="extension"/>
        /// is null (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="extension"/> is a string
        /// of length zero or contains only blanks.</exception>
        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("PNG", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Encodes the data of the specified image and writes the result to
        /// the specified stream.
        /// </summary>
        /// <param name="image">The image, where the data should be get from.
        /// Cannot be null (Nothing in Visual Basic).</param>
        /// <param name="stream">The stream, where the image data should be written to.
        /// Cannot be null (Nothing in Visual Basic).</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="image"/> is null (Nothing in Visual Basic).</para>
        /// <para>- or -</para>
        /// <para><paramref name="stream"/> is null (Nothing in Visual Basic).</para>
        /// </exception>
        public void Encode(Image image, Stream stream)
        {
            // Write the png header.
            stream.Write(
                new byte[] 
                { 
                    0x89, 0x50, 0x4E, 0x47, 
                    0x0D, 0x0A, 0x1A, 0x0A 
                }, 0, 8);

            PngHeader header = new PngHeader();
            header.Width = image.Width;
            header.Height = image.Height;
            header.ColorType = 6;
            header.BitDepth = 8;
            header.FilterMethod = 0;
            header.CompressionMethod = 0;
            header.InterlaceMethod = 0;

            WriteHeaderChunk(stream, header);
            WriteDataChunks(stream, image);
            WriteEndChunk(stream);

            stream.Flush();
        }

        private void WriteDataChunks(Stream stream, Image image)
        {
            byte[] pixels = image.Pixels;

            byte[] data = new byte[image.Width * image.Height * 4 + image.Height];

            int rowLength = image.Width * 4 + 1;

            for (int y = 0; y < image.Height; y++)
            {
                byte compression = 0;
                if (y > 0)
                {
                    compression = 2;
                }
                data[y * rowLength] = compression;

                for (int x = 0; x < image.Width; x++)
                {
                    // Calculate the offset for the new array.
                    int dataOffset = y * rowLength + x * 4 + 1;
                    
                    // Calculate the offset for the original pixel array.
                    int pixelOffset = (y * image.Width + x) * 4;

                    var a = 255.0f / pixels[pixelOffset + 3];

                    data[dataOffset + 0] = (byte)(pixels[pixelOffset + 2] * a);
                    data[dataOffset + 1] = (byte)(pixels[pixelOffset + 1] * a);
                    data[dataOffset + 2] = (byte)(pixels[pixelOffset + 0] * a);
                    data[dataOffset + 3] = pixels[pixelOffset + 3];

                    if (y > 0)
                    {
                        int lastOffset = ((y - 1) * image.Width + x) * 4;

                        a = 255.0f / pixels[pixelOffset + 3];

                        data[dataOffset + 0] -= (byte)(pixels[lastOffset + 2] * a);
                        data[dataOffset + 1] -= (byte)(pixels[lastOffset + 1] * a);
                        data[dataOffset + 2] -= (byte)(pixels[lastOffset + 0] * a);
                        data[dataOffset + 3] -= pixels[lastOffset + 3];
                    }
                }
            }

            byte[] buffer = null;
            int bufferLength = 0;

            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();

                using (DeflaterOutputStream zStream = new DeflaterOutputStream(memoryStream))
                {
                    zStream.Write(data, 0, data.Length);
                    zStream.Flush();
                    zStream.Finish();

                    bufferLength = (int)memoryStream.Length;
                    buffer = memoryStream.ToArray();
                }
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
            }

            int numChunks = bufferLength / MaxBlockSize;

            if (bufferLength % MaxBlockSize != 0)
            {
                numChunks++;
            }

            for (int i = 0; i < numChunks; i++)
            {
                int length = bufferLength - i * MaxBlockSize;

                if (length > MaxBlockSize)
                {
                    length = MaxBlockSize;
                }

                WriteChunk(stream, PngChunkTypes.Data, buffer, i * MaxBlockSize, length);
            }
        }

        private void WriteEndChunk(Stream stream)
        {
            WriteChunk(stream, PngChunkTypes.End, null);
        }

        private void WriteHeaderChunk(Stream stream, PngHeader header)
        {
            byte[] chunkData = new byte[13];

            WriteInteger(chunkData, 0, header.Width);
            WriteInteger(chunkData, 4, header.Height);

            chunkData[8] = header.BitDepth;
            chunkData[9] = header.ColorType;
            chunkData[10] = header.CompressionMethod;
            chunkData[11] = header.FilterMethod;
            chunkData[12] = header.InterlaceMethod;

            WriteChunk(stream, PngChunkTypes.Header, chunkData);
        }

        private void WriteChunk(Stream stream, string type, byte[] data)
        {
            WriteChunk(stream, type, data, 0, data != null ? data.Length : 0);
        }

        private void WriteChunk(Stream stream, string type, byte[] data, int offset, int length)
        {
            WriteInteger(stream, length);

            byte[] typeArray = new byte[4];
            typeArray[0] = (byte)type[0];
            typeArray[1] = (byte)type[1];
            typeArray[2] = (byte)type[2];
            typeArray[3] = (byte)type[3];

            stream.Write(typeArray, 0, 4);

            if (data != null)
            {
                stream.Write(data, offset, length);
            }

            Crc32 crc32 = new Crc32();
            crc32.Update(typeArray);

            if (data != null)
            {
                crc32.Update(data, offset, length);
            }

            WriteInteger(stream, (uint)crc32.Value);
        }

        private static void WriteInteger(byte[] data, int offset, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
            Array.Copy(buffer, 0, data, offset, 4);
        }

        private static void WriteInteger(Stream stream, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);
           
            stream.Write(buffer, 0, 4);
        }

        private static void WriteInteger(Stream stream, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            Array.Reverse(buffer);

            stream.Write(buffer, 0, 4);
        }
    }
}
