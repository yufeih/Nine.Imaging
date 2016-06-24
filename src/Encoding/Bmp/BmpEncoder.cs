// ===============================================================================
// BmpEncoder.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

namespace Nine.Imaging.Encoding
{
    using System;
    using System.IO;
    using Nine.Imaging.Bmp;

    /// <summary>
    /// Image encoder for writing an image to a stream
    /// as windows bitmap.
    /// </summary>
    /// <remarks>The encoder can only write 24-bit rpg images
    /// to streams. All other formats does not make much sense today.</remarks>
    public class BmpEncoder : IImageEncoder
    {
        public string Extension => "bmp";

        public bool IsSupportedFileExtension(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Substring(1);
            return extension.Equals("BMP", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals("DIP", StringComparison.OrdinalIgnoreCase);
        }

        public void Encode(Image image, Stream stream)
        {
            int rowWidth = image.Width;

            int amount = (image.Width * 3) % 4; 
            if (amount != 0)
            {
                rowWidth += 4 - amount;
            }

            BinaryWriter writer = new BinaryWriter(stream);

            BmpFileHeader fileHeader = new BmpFileHeader();
            fileHeader.Type = 19778;
            fileHeader.Offset = 54;
            fileHeader.FileSize = 54 + image.Height * rowWidth * 3;
            Write(writer, fileHeader);

            BmpInfoHeader infoHeader = new BmpInfoHeader();
            infoHeader.HeaderSize = 40;
            infoHeader.Height = image.Height;
            infoHeader.Width = image.Width;
            infoHeader.BitsPerPixel = 24;
            infoHeader.Planes = 1;
            infoHeader.Compression = BmpCompression.RGB;
            infoHeader.ImageSize = image.Height * rowWidth * 3;
            infoHeader.ClrUsed = 0;
            infoHeader.ClrImportant = 0;
            Write(writer, infoHeader);

            WriteImage(writer, image);

            writer.Flush();
        }

        private static void WriteImage(BinaryWriter writer, Image image)
        {
            int amount = (image.Width * 3) % 4, offset = 0;
            if (amount != 0)
            {
                amount = 4 - amount;
            }

            byte[] data = image.Pixels;

            for (int y = image.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    offset = (y * image.Width + x) * 4;

                    writer.Write(data[offset + 0]);
                    writer.Write(data[offset + 1]);
                    writer.Write(data[offset + 2]);
                }

                for (int i = 0; i < amount; i++)
                {
                    writer.Write((byte)0);
                }
            }
        }

        private static void Write(BinaryWriter writer, BmpFileHeader fileHeader)
        {
            writer.Write(fileHeader.Type);
            writer.Write(fileHeader.FileSize);
            writer.Write(fileHeader.Reserved);
            writer.Write(fileHeader.Offset);
        }

        private static void Write(BinaryWriter writer, BmpInfoHeader infoHeader)
        {
            writer.Write(infoHeader.HeaderSize);
            writer.Write(infoHeader.Width);
            writer.Write(infoHeader.Height);
            writer.Write(infoHeader.Planes);
            writer.Write(infoHeader.BitsPerPixel);
            writer.Write((int)infoHeader.Compression);
            writer.Write(infoHeader.ImageSize);
            writer.Write(infoHeader.XPelsPerMeter);
            writer.Write(infoHeader.YPelsPerMeter);
            writer.Write(infoHeader.ClrUsed);
            writer.Write(infoHeader.ClrImportant);
        }
    }
}
