namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Linq;
    using Nine.Imaging.IO;
    using System.Diagnostics;

    public class EncoderDecoderTest
    {
        [Theory]
        [InlineData("TestImages/Car.bmp")]
        [InlineData("TestImages/Portrait.png")]
        [InlineData("TestImages/Backdrop.jpg")]
        [InlineData("TestImages/Windmill.gif")]
        public void decode_then_encode_image_from_stream_should_succeed(string filename)
        {
            if (!Directory.Exists("Encoded")) Directory.CreateDirectory("Encoded");

            var stream = File.OpenRead(filename);
            var watch = Stopwatch.StartNew();
            var image = new Image(stream);
            
            var encodedFilename = "Encoded/" + Path.GetFileName(filename);

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

            Trace.WriteLine($"{ filename }: { watch.ElapsedMilliseconds}ms");
        }
    }
}