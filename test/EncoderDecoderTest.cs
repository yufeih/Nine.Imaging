namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Linq;
    using Nine.Imaging.IO;

    public class EncoderDecoderTest
    {
        [Theory]
        [InlineData("TestImages/Car.bmp")]
        [InlineData("TestImages/Portrait.png")]
        [InlineData("TestImages/Backdrop.jpg")]
        [InlineData("TestImages/Windmill.gif")]
        public void decode_then_encode_image_from_stream_should_succeed(string filename)
        {
            var stream = File.OpenRead(filename);
            var image = new Image(stream);
            var encodedFilename = Path.GetFileNameWithoutExtension(filename) + "_encoded" + Path.GetExtension(filename);

            if (!image.IsAnimated)
            {
                using (var output = File.OpenWrite(encodedFilename))
                {
                    var encoder = Image.Encoders.First(e => e.IsSupportedFileExtension(Path.GetExtension(filename)));
                    encoder.Encode(image, output);
                }
            }
            else
            {
                foreach (var frame in image.Frames)
                {
                    // TODO: save the frame
                }
            }
        }
    }
}