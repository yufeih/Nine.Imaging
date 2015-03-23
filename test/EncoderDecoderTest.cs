namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;

    public class EncoderDecoderTest
    {
        [Theory]
        [InlineData("TestImages/Portrait.png")]
        public void decode_then_encode_image_from_stream_should_succeed(string filename)
        {
            var stream = File.OpenRead(filename);
            var image = new Image(stream);
        }
    }
}