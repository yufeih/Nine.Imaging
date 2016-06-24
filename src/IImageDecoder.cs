namespace Nine.Imaging.Encoding
{
    using System.IO;

    public interface IImageDecoder
    {
        int HeaderSize { get; }

        bool IsSupportedFileExtension(string extension);

        bool IsSupportedFileFormat(byte[] header);

        Image Decode(Stream stream);
    }
}
