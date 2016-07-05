namespace Nine.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Nine.Imaging.Encoding;

    partial class Image
    {
        private static readonly Lazy<List<IImageDecoder>> defaultDecoders = new Lazy<List<IImageDecoder>>(() => new List<IImageDecoder>
        {
            new BmpDecoder(),
            new JpegDecoder(),
            new PngDecoder(),
            new GifDecoder(),
        });

        private static readonly Lazy<List<IImageEncoder>> defaultEncoders = new Lazy<List<IImageEncoder>>(() => new List<IImageEncoder>
        {
            new BmpEncoder(),
            new JpegEncoder(),
            new PngEncoder(),
        });

        public static IList<IImageDecoder> Decoders => defaultDecoders.Value;
        public static IList<IImageEncoder> Encoders => defaultEncoders.Value;

#if !PCL
        public static Image Load(string path, IList<IImageDecoder> decoders = null)
        {
            using (var stream = File.OpenRead(path))
            {
                return Load(stream, decoders);
            }
        }
#endif
        public static Image Load(Stream stream, IList<IImageDecoder> decoders = null)
        {
            if (decoders == null)
            {
                decoders = Decoders;
            }

            if (decoders.Count > 0)
            {
                var maxHeaderSize = 0;

                foreach (var decoder in decoders)
                {
                    if (decoder.HeaderSize > maxHeaderSize)
                    {
                        maxHeaderSize = decoder.HeaderSize;
                    }
                }

                if (maxHeaderSize > 0)
                {
                    byte[] header = new byte[maxHeaderSize];

                    stream.Read(header, 0, maxHeaderSize);
                    stream.Position = 0;

                    foreach (var decoder in decoders)
                    {
                        if (decoder.IsSupportedFileFormat(header))
                        {
                            return decoder.Decode(stream);
                        }
                    }
                }
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Image cannot be loaded. Available decoders:");

            foreach (IImageDecoder decoder in decoders)
            {
                stringBuilder.AppendLine("-" + decoder);
            }

            throw new NotSupportedException(stringBuilder.ToString());
        }
    }

    public static class ImageIOExtensions
    {
        public static void SaveAsPng(this Image image, Stream stream) => new PngEncoder().Encode(image, stream);
        public static void SaveAsJpeg(this Image image, Stream stream, int quality = 80) => new JpegEncoder { Quality = quality }.Encode(image, stream);
        public static void Save(this Image image, Stream stream, IImageEncoder encoder) => encoder.Encode(image, stream);
    }
}
