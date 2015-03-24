namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Diagnostics;
    using Nine.Imaging.Filtering;
    using Nine.Imaging.IO;

    public class ImageTransformTest
    {
        [Fact]
        public void transform_images()
        {
            if (!Directory.Exists("Transformed")) Directory.CreateDirectory("Transformed");
            
            var image = new Image(File.OpenRead("TestImages/Backdrop.jpg"));
            var watch = Stopwatch.StartNew();

            image = image.Transform(RotationType.Rotate180, FlippingType.None);

            Trace.WriteLine($"{ RotationType.Rotate180 }: { watch.ElapsedMilliseconds}ms");

            var outputFilename = $"Transformed/{ RotationType.Rotate180 }.jpg";
            using (var output = File.OpenWrite(outputFilename))
            {
                image.SaveAsJpeg(output);
            }
        }
    }
}