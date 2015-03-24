namespace Nine.Imaging
{
    using System.IO;
    using System.ComponentModel;
    using Nine.Imaging.Encoding;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ImageIOExtensions
    {
        public static void SaveAsPng(this ImageBase image, Stream stream) => new PngEncoder().Encode(image, stream);
        public static void SaveAsJpeg(this ImageBase image, Stream stream, int quality = 80) => new JpegEncoder { Quality = quality }.Encode(image, stream);
        public static void Save(this ImageBase image, Stream stream, IImageEncoder encoder) => encoder.Encode(image, stream);
    }
}
