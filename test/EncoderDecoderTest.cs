namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Linq;
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
                using (var output = File.OpenWrite($"Encoded/{ Path.GetFileNameWithoutExtension(filename) }.jpg"))
                {
                    image.SaveAsJpeg(output, 40);
                }

                for (int i = 0; i < image.Frames.Count; i++)
                {
                    using (var output = File.OpenWrite($"Encoded/{ i }_{ Path.GetFileNameWithoutExtension(filename) }.png"))
                    {
                        image.Frames[i].SaveAsPng(output);
                    }
                }
            }

            Trace.WriteLine($"{ filename }: { watch.ElapsedMilliseconds}ms");
        }

        [Theory]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(40)]
        [InlineData(80)]
        [InlineData(100)]
        public void jpeg_quality(int quality)
        {
            if (!Directory.Exists("Jpeg")) Directory.CreateDirectory("Jpeg");
            
            var image = new Image(File.OpenRead("TestImages/Backdrop.jpg"));

            using (var output = File.OpenWrite($"Jpeg/{ quality }.jpg"))
            {
                image.SaveAsJpeg(output, quality);
                output.Flush();

                Trace.WriteLine($"{ quality }: { output.Length }");
            }
        }
    }
}