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
        /// Gets or sets a value indicating whether this encoder
        /// will write the image uncompressed the stream.
        /// </summary>
        /// <value>
        /// <c>true</c> if the image should be written uncompressed to
        /// the stream; otherwise, <c>false</c>.
        /// </value>
        public bool IsWritingUncompressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is writing
        /// gamma information to the stream. The default value is false.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is writing gamma 
        /// information to the stream.; otherwise, <c>false</c>.
        /// </value>
        public bool IsWritingGamma { get; set; }

        /// <summary>
        /// Gets or sets the gamma value, that will be written
        /// the the stream, when the <see cref="IsWritingGamma"/> property
        /// is set to true. The default value is 2.2f.
        /// </summary>
        /// <value>The gamma value of the image.</value>
        public double Gamma { get; set; }

        /// <summary>
        /// Gets the default file extension for this encoder.
        /// </summary>
        /// <value>The default file extension for this encoder.</value>
        public string Extension
        {
            get { return "PNG"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PngEncoder"/> class.
        /// </summary>
        public PngEncoder()
        {
            Gamma = 2.2f;
        }

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
            Guard.NotNullOrEmpty(extension, "extension");

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
        public void Encode(ImageBase image, Stream stream)
        {
            Guard.NotNull(image, "image");
            Guard.NotNull(stream, "stream");

            // Write the png header.
            stream.Write(
                new byte[] 
                { 
                    0x89, 0x50, 0x4E, 0x47, 
                    0x0D, 0x0A, 0x1A, 0x0A 
                }, 0, 8);

            PngHeader header = new PngHeader();
            header.Width = image.PixelWidth;
            header.Height = image.PixelHeight;
            header.ColorType = 6;
            header.BitDepth = 8;
            header.FilterMethod = 0;
            header.CompressionMethod = 0;
            header.InterlaceMethod = 0;

            WriteHeaderChunk(stream, header);

            WritePhysicsChunk(stream, image);
            WriteGammaChunk(stream);

            if (IsWritingUncompressed)
            {
                WriteDataChunksFast(stream, image);
            }
            else
            {
                WriteDataChunks(stream, image);
            }
            WriteEndChunk(stream);

            stream.Flush();
        }

        private void WritePhysicsChunk(Stream stream, ImageBase imageBase)
        {
            var image = imageBase as Image;
            if (image != null && image.DensityX > 0 && image.DensityY > 0)
            {
                int dpmX = (int)Math.Round(image.DensityX * 39.3700787d);
                int dpmY = (int)Math.Round(image.DensityY * 39.3700787d);

                byte[] chunkData = new byte[9];

                WriteInteger(chunkData, 0, dpmX);
                WriteInteger(chunkData, 4, dpmY);

                chunkData[8] = 1;

                WriteChunk(stream, PngChunkTypes.Physical, chunkData);
            }
        }

        private void WriteGammaChunk(Stream stream)
        {
            if (IsWritingGamma)
            {
                int gammeValue = (int)(Gamma * 100000f);

                byte[] fourByteData = new byte[4];

                byte[] size = BitConverter.GetBytes(gammeValue);
                fourByteData[0] = size[3]; fourByteData[1] = size[2]; fourByteData[2] = size[1]; fourByteData[3] = size[0];

                WriteChunk(stream, PngChunkTypes.Gamma, fourByteData);
            }
        }

        private void WriteDataChunksFast(Stream stream, ImageBase image)
        {
            byte[] pixels = image.Pixels;

            // Convert the pixel array to a new array for adding
            // the filter byte.
            // --------------------------------------------------
            byte[] data = new byte[image.PixelWidth * image.PixelHeight * 4 + image.PixelHeight];

            int rowLength = image.PixelWidth * 4 + 1;

            for (int y = 0; y < image.PixelHeight; y++)
            {
                data[y * rowLength] = 0;

                Array.Copy(pixels, y * image.PixelWidth * 4, data, y * rowLength + 1, image.PixelWidth * 4);
            }
            // --------------------------------------------------

            Adler32 adler32 = new Adler32();
            adler32.Update(data);

            using (MemoryStream tempStream = new MemoryStream())
            {                
                int remainder = data.Length;

                int blockCount;
                if ((data.Length % MaxBlockSize) == 0)
                {
                    blockCount = data.Length / MaxBlockSize;
                }
                else
                {
                    blockCount = (data.Length / MaxBlockSize) + 1;
                }

                // Write headers
                tempStream.WriteByte(0x78);
                tempStream.WriteByte(0xDA);

                for (int i = 0; i < blockCount; i++)
                {
                    // Write the length
                    ushort length = (ushort)((remainder < MaxBlockSize) ? remainder : MaxBlockSize);

                    if (length == remainder)
                    {
                        tempStream.WriteByte(0x01);
                    }
                    else
                    {
                        tempStream.WriteByte(0x00);
                    }

                    tempStream.Write(BitConverter.GetBytes(length), 0, 2);

                    // Write one's compliment of length
                    tempStream.Write(BitConverter.GetBytes((ushort)~length), 0, 2);

                    // Write blocks
                    tempStream.Write(data, (int)(i * MaxBlockSize), length);

                    // Next block
                    remainder -= MaxBlockSize;
                }

                WriteInteger(tempStream, (int)adler32.Value);

                tempStream.Seek(0, SeekOrigin.Begin);

                byte[] zipData = new byte[tempStream.Length];
                tempStream.Read(zipData, 0, (int)tempStream.Length);

                WriteChunk(stream, PngChunkTypes.Data, zipData);
            }
        }

        private void WriteDataChunks(Stream stream, ImageBase image)
        {
            byte[] pixels = image.Pixels;

            byte[] data = new byte[image.PixelWidth * image.PixelHeight * 4 + image.PixelHeight];

            int rowLength = image.PixelWidth * 4 + 1;

            for (int y = 0; y < image.PixelHeight; y++)
            {
                byte compression = 0;
                if (y > 0)
                {
                    compression = 2;
                }
                data[y * rowLength] = compression;

                for (int x = 0; x < image.PixelWidth; x++)
                {
                    // Calculate the offset for the new array.
                    int dataOffset = y * rowLength + x * 4 + 1;
                    
                    // Calculate the offset for the original pixel array.
                    int pixelOffset = (y * image.PixelWidth + x) * 4;

                    data[dataOffset + 0] = pixels[pixelOffset + 2];
                    data[dataOffset + 1] = pixels[pixelOffset + 1];
                    data[dataOffset + 2] = pixels[pixelOffset + 0];
                    data[dataOffset + 3] = pixels[pixelOffset + 3];

                    if (y > 0)
                    {
                        int lastOffset = ((y - 1) * image.PixelWidth + x) * 4;

                        data[dataOffset + 0] -= pixels[lastOffset + 2];
                        data[dataOffset + 1] -= pixels[lastOffset + 1];
                        data[dataOffset + 2] -= pixels[lastOffset + 0];
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
